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

    private Kernel KernelMitPlugins()
    {
        var clone = _kernel.Clone();
        clone.Plugins.AddFromObject(_kundePlugin, "KundePlugin");
        clone.Plugins.AddFromObject(_ticketPlugin, "TicketPlugin");
        clone.Plugins.AddFromObject(_projektPlugin, "ProjektPlugin");
        return clone;
    }

    public async Task<ChatBotResponseDto> FrageStellen(string nachricht, Guid mandantId)
    {
        var validierung = await VorPruefung(nachricht, mandantId);
        if (!validierung.IstErlaubt)
        {
            return new ChatBotResponseDto { Antwort = validierung.Fehlermeldung, RelevanzScore = 0 };
        }

        var rag = await HoleRagKontext(nachricht, mandantId);
        if (IstDatenFrage(nachricht) && !rag.HatKontext)
        {
            return new ChatBotResponseDto
            {
                Antwort = "Ich habe dazu keine belastbaren Daten gefunden.",
                Quelle = "RAG",
                RelevanzScore = 0
            };
        }

        var history = ErstelleHistory(nachricht, rag.KontextText);
        var kernelMitPlugins = KernelMitPlugins();
        var settings = ErstelleAusfuehrungseinstellungen();

        try
        {
            var result = await _chat.GetChatMessageContentAsync(history, settings, kernelMitPlugins);
            var antwort = _outputFilter.Filtern(result.Content ?? "Keine Antwort erhalten.");

            _logger.LogInformation(
                "VIKA | Antwort gesendet | MandantId: {MandantId} | RAG: {HatKontext} | Relevanz: {Relevanz:F2}",
                mandantId, rag.HatKontext, rag.BesteRelevanz);

            return new ChatBotResponseDto
            {
                Antwort = antwort,
                Quelle = rag.Quelle,
                RelevanzScore = rag.BesteRelevanz
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VIKA | LLM Fehler | MandantId: {MandantId}", mandantId);
            return new ChatBotResponseDto
            {
                Antwort = "Ein Fehler ist aufgetreten. Bitte versuchen Sie es später erneut.",
                RelevanzScore = 0
            };
        }
    }

    public async IAsyncEnumerable<string> FrageStreamen(string nachricht, Guid mandantId)
    {
        var validierung = await VorPruefung(nachricht, mandantId);
        if (!validierung.IstErlaubt)
        {
            yield return validierung.Fehlermeldung;
            yield break;
        }

        var rag = await HoleRagKontext(nachricht, mandantId);
        if (IstDatenFrage(nachricht) && !rag.HatKontext)
        {
            yield return "Ich habe dazu keine belastbaren Daten gefunden.";
            yield break;
        }

        var history = ErstelleHistory(nachricht, rag.KontextText);
        var kernelMitPlugins = KernelMitPlugins();
        var settings = ErstelleAusfuehrungseinstellungen();

        string complete = string.Empty;
        await foreach (var part in _chat.GetStreamingChatMessageContentsAsync(history, settings, kernelMitPlugins))
        {
            if (string.IsNullOrEmpty(part.Content))
            {
                continue;
            }

            complete += part.Content;
            yield return part.Content;
        }

        _logger.LogInformation(
            "VIKA | Stream abgeschlossen | MandantId: {MandantId} | RAG: {HatKontext} | Relevanz: {Relevanz:F2} | Laenge: {Laenge}",
            mandantId, rag.HatKontext, rag.BesteRelevanz, complete.Length);
    }

    private async Task<(bool IstErlaubt, string Fehlermeldung)> VorPruefung(string nachricht, Guid mandantId)
    {
        var (istErlaubt, grund) = _inputFilter.Validieren(nachricht);
        if (!istErlaubt)
        {
            _logger.LogWarning("VIKA | Input abgelehnt | MandantId: {MandantId} | Grund: {Grund}", mandantId, grund);
            return (false, grund);
        }

        var (limitOk, verbleibend) = await _rateLimiter.PruefeLimit(mandantId);
        if (!limitOk)
        {
            _logger.LogWarning("VIKA | Rate limit erreicht | MandantId: {MandantId}", mandantId);
            return (false, "Tageslimit erreicht. Bitte versuchen Sie es morgen erneut.");
        }

        _logger.LogDebug("VIKA | Rate limit ok | MandantId: {MandantId} | Verbleibend: {Verbleibend}", mandantId, verbleibend);
        return (true, string.Empty);
    }

    private ChatHistory ErstelleHistory(string nachricht, string? ragKontext)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);

        if (!string.IsNullOrWhiteSpace(ragKontext))
        {
            history.AddSystemMessage(
                "Nutze den folgenden Kontext als primäre Faktengrundlage. " +
                "Wenn die Antwort nicht im Kontext oder via Plugins belegbar ist, sage klar, dass du es nicht weißt.\n\n" +
                ragKontext);
        }

        history.AddUserMessage(nachricht);
        return history;
    }

    private OllamaPromptExecutionSettings ErstelleAusfuehrungseinstellungen()
    {
#pragma warning disable SKEXP0070
        return new OllamaPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            Temperature = _configuration.GetValue<float?>("Vika:Generation:Temperature") ?? 0.0f,
            NumPredict = _configuration.GetValue<int?>("Vika:Generation:MaxTokens") ?? 500,
            ExtensionData = new Dictionary<string, object>
            {
                ["repeat_penalty"] = _configuration.GetValue<double?>("Vika:Generation:RepeatPenalty") ?? 1.3
            }
        };
#pragma warning restore SKEXP0070
    }

    private async Task<(bool HatKontext, string? KontextText, string Quelle, double BesteRelevanz)> HoleRagKontext(string nachricht, Guid mandantId)
    {
        try
        {
            var minRelevanz = _configuration.GetValue<double?>("Vika:Rag:MinRelevance") ?? 0.72;
            var maxPartitionen = _configuration.GetValue<int?>("Vika:Rag:MaxPartitions") ?? 8;
            var maxTextLaenge = _configuration.GetValue<int?>("Vika:Rag:MaxContextChars") ?? 3000;

            var mandantFilter = new MemoryFilter().ByTag("mandantId", mandantId.ToString());

            var suchErgebnis = await _memory.SearchAsync(
                query: nachricht,
                index: string.Empty,
                filter: mandantFilter,
                minRelevance: minRelevanz,
                limit: maxPartitionen);

            if (suchErgebnis.NoResult || suchErgebnis.Results.Count == 0)
            {
                return (false, null, "RAG", 0);
            }

            var partitionen = suchErgebnis.Results
                .SelectMany(r => r.Partitions)
                .Where(p => !string.IsNullOrWhiteSpace(p.Text))
                .OrderByDescending(p => p.Relevance)
                .Take(maxPartitionen)
                .ToList();

            if (partitionen.Count == 0)
            {
                return (false, null, "RAG", 0);
            }

            var besteRelevanz = partitionen.Max(p => p.Relevance);
            if (besteRelevanz < minRelevanz)
            {
                return (false, null, "RAG", besteRelevanz);
            }

            var builder = new System.Text.StringBuilder();
            foreach (var p in partitionen)
            {
                if (builder.Length >= maxTextLaenge) break;

                var rest = maxTextLaenge - builder.Length;
                var text = p.Text!;
                if (text.Length > rest)
                {
                    text = text[..rest];
                }

                builder.Append("- ").AppendLine(text);
            }

            var quellen = string.Join(", ", suchErgebnis.Results.Select(r => r.DocumentId).Distinct());
            return (true, builder.ToString(), $"RAG:{quellen}", besteRelevanz);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VIKA | RAG Fehler");
            return (false, null, "RAG", 0);
        }
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
}
