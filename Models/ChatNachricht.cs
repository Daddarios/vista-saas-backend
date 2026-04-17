using Vista.Core.Models.Base;

namespace Vista.Core.Models;

public class ChatNachricht : MandantEntity
{
    public Guid RaumId { get; set; }
    public ChatRaum? Raum { get; set; }

    public string AbsenderId { get; set; } = string.Empty;
    public Benutzer? Absender { get; set; }

    public string Inhalt { get; set; } = string.Empty;
    public DateTime GeschicktAm { get; set; } = DateTime.UtcNow;
}
