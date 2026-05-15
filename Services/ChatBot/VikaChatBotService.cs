using System.Text;
using System.Text.RegularExpressions;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
#pragma warning disable SKEXP0070
using Microsoft.SemanticKernel.Connectors.Ollama;
#pragma warning restore SKEXP0070
using Vista.Core.DTOs.ChatBot;
using Vista.Core.Plugins;

namespace Vista.Core.Services.ChatBot;

public class VikaChatBotService
{
    private readonly Kernel _kernel;
    private readonly IKernelMemory _memory;
    private readonly IChatCompletionService _chat;
    private readonly ChatInputFilter _inputFilter;
    private readonly ChatOutputFilter _outputFilter;
    private readonly ChatRateLimiter _rateLimiter;
    private readonly KundePlugin _kundePlugin;
    private readonly TicketPlugin _ticketPlugin;
    private readonly ProjektPlugin _projektPlugin;
    private readonly ILogger<VikaChatBotService> _logger;
    private readonly IConfiguration _configuration;

    private string SystemPrompt => File.Exists(".VikaRules.md")
        ? File.ReadAllText(".VikaRules.md")
        : "You are VIKA. Reply concisely and do not fabricate facts.";

    public VikaChatBotService(
        Kernel kernel,
        IKernelMemory memory,
        IChatCompletionService chat,
        ChatInputFilter inputFilter,
        ChatOutputFilter outputFilter,
        ChatRateLimiter rateLimiter,
        KundePlugin kundePlugin,
        TicketPlugin ticketPlugin,
        ProjektPlugin projektPlugin,
        IConfiguration configuration,
        ILogger<VikaChatBotService> logger)
    {
        _kernel = kernel;
        _memory = memory;
        _chat = chat;
        _inputFilter = inputFilter;
        _outputFilter = outputFilter;
        _rateLimiter = rateLimiter;
        _kundePlugin = kundePlugin;
        _ticketPlugin = ticketPlugin;
        _projektPlugin = projektPlugin;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ChatBotResponseDto> FrageStellen(string nachricht, Guid mandantId)
    {
        var validierung = await VorPruefung(nachricht, mandantId);
        if (!validierung.IstErlaubt)
            return new ChatBotResponseDto { Antwort = validierung.Fehlermeldung, RelevanzScore = 0 };

        var route = BestimmeRoute(nachricht);
        var rag = route == AnfrageRoute.Data
            ? await HoleRagKontext(nachricht, mandantId)
            : new RagContext(false, null, "LLM", 0d);

        if (route == AnfrageRoute.Data && BrauchtRagBeleg(nachricht) && !rag.HatKontext)
        {
            return new ChatBotResponseDto
            {
                Antwort = "Ich habe dazu keine belastbaren Daten gefunden.",
                Quelle = "RAG",
                RelevanzScore = rag.BesteRelevanz
            };
        }

        var history = ErstelleHistory(nachricht, rag.KontextText);
        var kernel = route == AnfrageRoute.Data ? KernelMitPlugins() : _kernel;
        var settings = ErstelleAusfuehrungseinstellungen(route == AnfrageRoute.Data);

        var result = await _chat.GetChatMessageContentAsync(history, settings, kernel);
        var antwort = FinalSanitize(result.Content ?? "Keine Antwort erhalten.");

        _logger.LogInformation("VIKA | Sync Antwort | Route: {Route} | MandantId: {MandantId} | RAG: {Rag} | Relevanz: {Rel:F2}",
            route, mandantId, rag.HatKontext, rag.BesteRelevanz);

        return new ChatBotResponseDto
        {
            Antwort = antwort,
            Quelle = route == AnfrageRoute.Data ? rag.Quelle : "LLM",
            RelevanzScore = rag.BesteRelevanz
        };
    }

    public async IAsyncEnumerable<string> FrageStreamen(string nachricht, Guid mandantId)
    {
        var validierung = await VorPruefung(nachricht, mandantId);
        if (!validierung.IstErlaubt)
        {
            yield return validierung.Fehlermeldung;
            yield break;
        }

        var route = BestimmeRoute(nachricht);
        var rag = route == AnfrageRoute.Data
            ? await HoleRagKontext(nachricht, mandantId)
            : new RagContext(false, null, "LLM", 0d);

        if (route == AnfrageRoute.Data && BrauchtRagBeleg(nachricht) && !rag.HatKontext)
        {
            yield return "Ich habe dazu keine belastbaren Daten gefunden.";
            yield break;
        }

        var history = ErstelleHistory(nachricht, rag.KontextText);
        var kernel = route == AnfrageRoute.Data ? KernelMitPlugins() : _kernel;
        var settings = ErstelleAusfuehrungseinstellungen(route == AnfrageRoute.Data);

        var complete = new StringBuilder();
        await foreach (var part in _chat.GetStreamingChatMessageContentsAsync(history, settings, kernel))
        {
            if (!string.IsNullOrEmpty(part.Content))
                complete.Append(part.Content);
        }

        var final = FinalSanitize(complete.ToString());
        const int chunkSize = 120;
        for (var i = 0; i < final.Length; i += chunkSize)
        {
            var len = Math.Min(chunkSize, final.Length - i);
            yield return final.Substring(i, len);
        }

        _logger.LogInformation("VIKA | Stream Antwort | Route: {Route} | MandantId: {MandantId} | RAG: {Rag} | Relevanz: {Rel:F2} | Laenge: {Len}",
            route, mandantId, rag.HatKontext, rag.BesteRelevanz, final.Length);
    }

    private Kernel KernelMitPlugins()
    {
        var clone = _kernel.Clone();
        clone.Plugins.AddFromObject(_kundePlugin, "KundePlugin");
        clone.Plugins.AddFromObject(_ticketPlugin, "TicketPlugin");
        clone.Plugins.AddFromObject(_projektPlugin, "ProjektPlugin");
        return clone;
    }

    private async Task<(bool IstErlaubt, string Fehlermeldung)> VorPruefung(string nachricht, Guid mandantId)
    {
        var (istErlaubt, grund) = _inputFilter.Validieren(nachricht);
        if (!istErlaubt) return (false, grund);

        var (limitOk, _) = await _rateLimiter.PruefeLimit(mandantId);
        if (!limitOk) return (false, "Tageslimit erreicht. Bitte versuchen Sie es morgen erneut.");

        return (true, string.Empty);
    }

    private ChatHistory ErstelleHistory(string nachricht, string? ragKontext)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);

        if (!string.IsNullOrWhiteSpace(ragKontext))
        {
            history.AddSystemMessage(
                "NUTZE DEN KONTEXT ALS FAKTENQUELLE. " +
                "Wenn im Kontext keine belastbare Information steht, sage klar, dass du es nicht weißt.\n\n" + ragKontext);
        }

        history.AddUserMessage(nachricht);
        return history;
    }

    private OllamaPromptExecutionSettings ErstelleAusfuehrungseinstellungen(bool allowTools)
    {
#pragma warning disable SKEXP0070
        return new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = allowTools ? FunctionChoiceBehavior.Auto() : FunctionChoiceBehavior.None(),
            Temperature = _configuration.GetValue<float?>("Vika:Generation:Temperature") ?? 0.0f,
            NumPredict = _configuration.GetValue<int?>("Vika:Generation:MaxTokens") ?? 500,
            ExtensionData = new Dictionary<string, object>
            {
                ["repeat_penalty"] = _configuration.GetValue<double?>("Vika:Generation:RepeatPenalty") ?? 1.3
            }
        };
#pragma warning restore SKEXP0070
    }

    private async Task<RagContext> HoleRagKontext(string nachricht, Guid mandantId)
    {
        try
        {
            var minRelevanz = _configuration.GetValue<double?>("Vika:Rag:MinRelevance") ?? 0.72;
            var maxPartitionen = _configuration.GetValue<int?>("Vika:Rag:MaxPartitions") ?? 8;
            var maxTextLaenge = _configuration.GetValue<int?>("Vika:Rag:MaxContextChars") ?? 3000;

            var mandantFilter = new MemoryFilter().ByTag("mandantId", mandantId.ToString());
            var suchErgebnis = await _memory.SearchAsync(nachricht, string.Empty, mandantFilter, null, minRelevanz, maxPartitionen);

            if (suchErgebnis.NoResult || suchErgebnis.Results.Count == 0)
                return new RagContext(false, null, "RAG", 0);

            var partitionen = suchErgebnis.Results
                .SelectMany(r => r.Partitions)
                .Where(p => !string.IsNullOrWhiteSpace(p.Text))
                .OrderByDescending(p => p.Relevance)
                .Take(maxPartitionen)
                .ToList();

            if (partitionen.Count == 0)
                return new RagContext(false, null, "RAG", 0);

            var besteRelevanz = partitionen.Max(p => p.Relevance);

            var builder = new StringBuilder();
            foreach (var p in partitionen)
            {
                if (builder.Length >= maxTextLaenge) break;
                var rest = maxTextLaenge - builder.Length;
                var text = p.Text!.Length > rest ? p.Text[..rest] : p.Text!;
                builder.Append("- ").AppendLine(text);
            }

            var quellen = string.Join(", ", suchErgebnis.Results.Select(r => r.DocumentId).Distinct());
            return new RagContext(true, builder.ToString(), $"RAG:{quellen}", besteRelevanz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VIKA | RAG Fehler");
            return new RagContext(false, null, "RAG", 0);
        }
    }

    private string FinalSanitize(string text)
    {
        var cleaned = _outputFilter.Filtern(text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(cleaned))
            return "Ich kann dazu aktuell keine klare Antwort geben.";

        cleaned = Regex.Replace(cleaned, @"(?i)would you like.*$", "", RegexOptions.Singleline).Trim();
        cleaned = Regex.Replace(cleaned, @"(?i)i support english, german, french, and italian\.?", "", RegexOptions.Singleline).Trim();

        var paragraphs = cleaned
            .Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        return paragraphs.Count > 0 ? paragraphs[0] : cleaned;
    }

    private static AnfrageRoute BestimmeRoute(string nachricht)
    {
        return IstDatenFrage(nachricht) ? AnfrageRoute.Data : AnfrageRoute.Conversation;
    }

    private static bool BrauchtRagBeleg(string nachricht)
    {
        var n = nachricht.ToLowerInvariant();
        string[] aggregate =
        [
            "wie viel", "wieviele", "wieviel", "anzahl", "count",
            "statistik", "overview", "übersicht", "uebersicht"
        ];

        return !aggregate.Any(n.Contains);
    }

    private static bool IstDatenFrage(string nachricht)
    {
        var n = nachricht.ToLowerInvariant();
        string[] dataKeywords =
        [
            "kunde", "kunden", "ticket", "tickets", "projekt", "projekte",
            "anzahl", "count", "liste", "status", "priorität", "prioritaet",
            "offen", "open", "report", "statistik", "details"
        ];

        return dataKeywords.Any(n.Contains);
    }

    private enum AnfrageRoute
    {
        Conversation,
        Data
    }

    private readonly record struct RagContext(bool HatKontext, string? KontextText, string Quelle, double BesteRelevanz);
}
