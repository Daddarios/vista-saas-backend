using System.Collections.Concurrent;
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

    // raumId -> userId -> Set<connectionId>
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, HashSet<string>>> _rooms = new();
    // connectionId -> Set<raumId> (für Cleanup bei Disconnect)
    private static readonly ConcurrentDictionary<string, HashSet<string>> _connectionRooms = new();
    private static readonly object _lock = new();

    public ChatHub(AppDbContext db)
    {
        _db = db;
    }

    public async Task JoinRoom(string raumId)
    {
        if (!Guid.TryParse(raumId, out var raumGuid))
            throw new HubException("Ungültige raumId");

        var mandantId = GetMandantId();
        var userId = GetUserId();

        // Mandant-Guard: nur Räume des eigenen Mandanten
        var raumExists = await _db.ChatRaeume
            .AsNoTracking()
            .AnyAsync(r => r.Id == raumGuid && r.MandantId == mandantId && !r.IstGeloescht);
        if (!raumExists)
            throw new HubException("Raum nicht gefunden oder kein Zugriff");

        await Groups.AddToGroupAsync(Context.ConnectionId, raumId);

        bool firstConnectionForUser;
        string[] onlineUserIds;

        lock (_lock)
        {
            var users = _rooms.GetOrAdd(raumId, _ => new ConcurrentDictionary<string, HashSet<string>>());
            var conns = users.GetOrAdd(userId, _ => new HashSet<string>());
            firstConnectionForUser = conns.Count == 0;
            conns.Add(Context.ConnectionId);

            var rooms = _connectionRooms.GetOrAdd(Context.ConnectionId, _ => new HashSet<string>());
            rooms.Add(raumId);

            onlineUserIds = users.Keys.ToArray();
        }

        // Snapshot an Caller
        await Clients.Caller.SendAsync("OnlineUsers", onlineUserIds);

        // Andere benachrichtigen nur wenn neue Anwesenheit
        if (firstConnectionForUser)
            await Clients.OthersInGroup(raumId).SendAsync("UserJoined", userId);
    }

    public async Task LeaveRoom(string raumId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, raumId);
        var userId = GetUserId();

        bool wentOffline = RemoveConnectionFromRoom(raumId, userId, Context.ConnectionId);

        if (wentOffline)
            await Clients.Group(raumId).SendAsync("UserLeft", userId);
    }

    public async Task SendMessage(string raumId, string inhalt)
    {
        if (!Guid.TryParse(raumId, out var raumGuid))
            throw new HubException("Ungültige raumId");
        if (string.IsNullOrWhiteSpace(inhalt))
            throw new HubException("Nachricht darf nicht leer sein");
        if (inhalt.Length > 4000)
            inhalt = inhalt.Substring(0, 4000);

        var userId = GetUserId();
        var mandantId = GetMandantId();

        // Raum-Guard
        var raum = await _db.ChatRaeume
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == raumGuid && r.MandantId == mandantId && !r.IstGeloescht);
        if (raum == null)
            throw new HubException("Raum nicht gefunden oder kein Zugriff");

        var nachricht = new ChatNachricht
        {
            MandantId = mandantId,
            RaumId = raumGuid,
            AbsenderId = userId,
            Inhalt = inhalt
        };

        _db.ChatNachrichten.Add(nachricht);
        await _db.SaveChangesAsync();

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

        var payload = new
        {
            id = nachricht.Id,
            raumId = raumGuid,
            inhalt = nachricht.Inhalt,
            geschicktAm = nachricht.GeschicktAm,
            absenderId = userId,
            absender = user == null ? null : new
            {
                vorname = user.Vorname,
                nachname = user.Nachname,
                bild = user.Bild
            }
        };

        await Clients.Group(raumId).SendAsync("ReceiveMessage", payload);
    }

    public async Task Typing(string raumId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(raumId).SendAsync("UserTyping", new { userId, raumId });
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connId = Context.ConnectionId;

        string[] roomsToCheck;
        lock (_lock)
        {
            roomsToCheck = _connectionRooms.TryRemove(connId, out var set)
                ? set.ToArray()
                : Array.Empty<string>();
        }

        foreach (var raumId in roomsToCheck)
        {
            bool wentOffline = RemoveConnectionFromRoom(raumId, userId, connId);
            if (wentOffline)
                await Clients.Group(raumId).SendAsync("UserLeft", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static bool RemoveConnectionFromRoom(string raumId, string userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_rooms.TryGetValue(raumId, out var users)) return false;
            if (!users.TryGetValue(userId, out var conns)) return false;

            conns.Remove(connectionId);
            if (conns.Count == 0)
            {
                users.TryRemove(userId, out _);
                if (users.IsEmpty) _rooms.TryRemove(raumId, out _);
                return true;
            }
            return false;
        }
    }

    private string GetUserId() =>
        Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

    private Guid GetMandantId()
    {
        var header = Context.GetHttpContext()?.Request.Headers["X-Mandant-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : Guid.Empty;
    }
}
