using Vista.Core.Models.Base;

namespace Vista.Core.Models;

public class ChatRaum : MandantEntity
{
    public string Name { get; set; } = string.Empty;

    public Guid? ProjektId { get; set; }
    public Projekt? Projekt { get; set; }

    public Guid? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public ICollection<ChatNachricht> Nachrichten { get; set; } = new List<ChatNachricht>();
}
