using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Vista.Core.DTOs.ChatBot;
using Vista.Core.Plugins;

namespace Vista.Core.Services.ChatBot;

public class VikaChatBotService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly ChatInputFilter _inputFilter;
    private readonly ChatOutputFilter _outputFilter;
    private readonly ChatRateLimiter _rateLimiter;
    private readonly KundePlugin _kundePlugin;
    private readonly TicketPlugin _ticketPlugin;
    private readonly ProjektPlugin _projektPlugin;
    private readonly ILogger<VikaChatBotService> _logger;

    private string SystemPrompt => File.Exists(".VikaRules.md") 
        ? File.ReadAllText(".VikaRules.md") 
        : "Sen VIKA'sın. (Dosya bulunamadı)";

    public VikaChatBotService(
        Kernel kernel,
        IChatCompletionService chat,
        ChatInputFilter inputFilter,
        ChatOutputFilter outputFilter,
        ChatRateLimiter rateLimiter,
        KundePlugin kundePlugin,
        TicketPlugin ticketPlugin,
        ProjektPlugin projektPlugin,
        ILogger<VikaChatBotService> logger)
    {
        _kernel = kernel;
        _chat = chat;
        _inputFilter = inputFilter;
        _outputFilter = outputFilter;
        _rateLimiter = rateLimiter;
        _kundePlugin = kundePlugin;
        _ticketPlugin = ticketPlugin;
        _projektPlugin = projektPlugin;
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
        // 1. Input filtre
        var (istErlaubt, grund) = _inputFilter.Validieren(nachricht);
        if (!istErlaubt)
        {
            _logger.LogWarning("VIKA | Input abgelehnt | MandantId: {MandantId} | Grund: {Grund}", mandantId, grund);
            return new ChatBotResponseDto { Antwort = grund, RelevanzScore = 0 };
        }

        // 2. Rate limit
        var (limitOk, verbleibend) = await _rateLimiter.PruefeLimit(mandantId);
        if (!limitOk)
        {
            _logger.LogWarning("VIKA | Rate limit erreicht | MandantId: {MandantId}", mandantId);
            return new ChatBotResponseDto
            {
                Antwort = "Tageslimit erreicht. Bitte versuchen Sie es morgen erneut.",
                RelevanzScore = 0
            };
        }

        // 3. Kernel mit Plugins
        var kernelMitPlugins = KernelMitPlugins();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(SystemPrompt);
        chatHistory.AddUserMessage(nachricht);

        // 4. LLM çağrısı — auto function calling
        try
        {
            var settings = new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                ExtensionData = new Dictionary<string, object>
                {
                    ["max_tokens"] = 500,
                    ["temperature"] = 0.0,
                    ["repeat_penalty"] = 1.3,
                    ["frequency_penalty"] = 0.5
                }
            };

            var result = await _chat.GetChatMessageContentAsync(
                chatHistory, settings, kernelMitPlugins);

            var antwort = result.Content ?? "Keine Antwort erhalten.";

            // 5. Output filtre
            antwort = _outputFilter.Filtern(antwort);

            _logger.LogInformation("VIKA | Antwort gesendet | MandantId: {MandantId} | Verbleibend: {V}", mandantId, verbleibend);

            return new ChatBotResponseDto
            {
                Antwort = antwort,
                Quelle = "VIKA Plugin Pipeline",
                RelevanzScore = 0.85
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
        var (istErlaubt, grund) = _inputFilter.Validieren(nachricht);
        if (!istErlaubt) { yield return grund; yield break; }

        var (limitOk, _) = await _rateLimiter.PruefeLimit(mandantId);
        if (!limitOk) { yield return "Tageslimit erreicht."; yield break; }

        var kernelMitPlugins = KernelMitPlugins();

        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(SystemPrompt);
        chatHistory.AddUserMessage(nachricht);

        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
            ExtensionData = new Dictionary<string, object>
            {
                ["max_tokens"] = 500,
                ["temperature"] = 0.0,
                ["repeat_penalty"] = 1.3,
                ["frequency_penalty"] = 0.5
            }
        };

        var enumerator = _chat.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernelMitPlugins).GetAsyncEnumerator();
        bool hasError = false;
        try
        {
            while (true)
            {
                string? content = null;
                try
                {
                    if (!await enumerator.MoveNextAsync()) break;
                    content = enumerator.Current.Content;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "VIKA | Stream Fehler");
                    hasError = true;
                    break;
                }
                if (!string.IsNullOrEmpty(content)) yield return content!;
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }

        if (hasError)
        {
            yield return "\n[Bağlantı veya model hatası oluştu, lütfen tekrar deneyin.]";
        }
    }
}
