using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Vista.Core.Data;
using Vista.Core.Models;

namespace Vista.Core.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly AppDbContext _db;
    private static readonly Dictionary<string, HashSet<string>> _onlineUsers = new();

    public ChatHub(AppDbContext db)
    {
        _db = db;
    }

    public async Task JoinRoom(string raumId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, raumId);

        if (!_onlineUsers.ContainsKey(raumId))
            _onlineUsers[raumId] = new HashSet<string>();

        var userId = GetUserId();
        _onlineUsers[raumId].Add(userId);

        await Clients.Group(raumId).SendAsync("UserJoined", userId);
        await Clients.Caller.SendAsync("OnlineUsers", _onlineUsers[raumId]);
    }

    public async Task LeaveRoom(string raumId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, raumId);

        var userId = GetUserId();
        if (_onlineUsers.ContainsKey(raumId))
            _onlineUsers[raumId].Remove(userId);

        await Clients.Group(raumId).SendAsync("UserLeft", userId);
    }

    public async Task SendMessage(string raumId, string inhalt)
    {
        var userId = GetUserId();
        var mandantId = GetMandantId();

        var nachricht = new ChatNachricht
        {
            MandantId = mandantId,
            RaumId = Guid.Parse(raumId),
            AbsenderId = userId,
            Inhalt = inhalt
        };

        _db.ChatNachrichten.Add(nachricht);
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(userId);

        await Clients.Group(raumId).SendAsync("ReceiveMessage", new
        {
            nachricht.Id,
            nachricht.Inhalt,
            nachricht.GeschicktAm,
            Absender = new { user?.Vorname, user?.Nachname }
        });
    }

    public async Task Typing(string raumId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(raumId).SendAsync("UserTyping", userId);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        foreach (var room in _onlineUsers)
        {
            if (room.Value.Remove(userId))
                await Clients.Group(room.Key).SendAsync("UserLeft", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private string GetUserId() =>
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private Guid GetMandantId()
    {
        var header = Context.GetHttpContext()?.Request.Headers["X-Mandant-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.Empty;
    }
}
