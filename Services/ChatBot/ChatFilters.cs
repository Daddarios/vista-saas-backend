using Microsoft.Extensions.Caching.Distributed;

namespace Vista.Core.Services.ChatBot;

public class ChatInputFilter
{
    private static readonly string[] VerboteneWoerter = [
        "passwort", "password", "şifre", "iban", "kreditkarte", "credit card"
    ];

    public (bool IstErlaubt, string Grund) Validieren(string eingabe)
    {
        if (string.IsNullOrWhiteSpace(eingabe))
            return (false, "Leere Nachricht.");

        if (eingabe.Length > 1000)
            return (false, "Nachricht ist zu lang (max 1000 Zeichen).");

        var lower = eingabe.ToLowerInvariant();
        foreach (var wort in VerboteneWoerter)
        {
            if (lower.Contains(wort))
                return (false, "Anfragen zu sensiblen Daten sind nicht erlaubt.");
        }

        return (true, string.Empty);
    }
}

public class ChatRateLimiter
{
    private readonly IDistributedCache _cache;
    private const int MaxAnfragenProTag = 100;

    public ChatRateLimiter(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<(bool IstErlaubt, int Verbleibend)> PruefeLimit(Guid mandantId)
    {
        var schluessel = $"vika:limit:{mandantId}:{DateTime.UtcNow:yyyyMMdd}";
        var wert = await _cache.GetStringAsync(schluessel);
        var aktuelleAnzahl = string.IsNullOrEmpty(wert) ? 0 : int.Parse(wert);

        if (aktuelleAnzahl >= MaxAnfragenProTag)
            return (false, 0);

        aktuelleAnzahl++;
        await _cache.SetStringAsync(schluessel, aktuelleAnzahl.ToString(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });

        return (true, MaxAnfragenProTag - aktuelleAnzahl);
    }
}

public class ChatOutputFilter
{
    private static readonly string[] SensibleMuster = [
        @"\b[A-Z]{2}\d{2}\s?\d{4}\s?\d{4}\s?\d{4}\s?\d{4}\s?\d{0,2}\b", // IBAN
        @"\b\d{16}\b", // Kreditkarte
        @"passwort\s*[:=]\s*\S+", // Passwort
    ];

    public string Filtern(string antwort)
    {
        foreach (var muster in SensibleMuster)
        {
            antwort = System.Text.RegularExpressions.Regex.Replace(
                antwort, muster, "[GEFILTERT]",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }
        return antwort;
    }
}
