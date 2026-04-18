using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vista.Core.Data;

namespace Vista.Core.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;

    public ChatController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("raeume")]
    public async Task<IActionResult> GetRaeume()
    {
        var list = await _db.ChatRaeume
            .Where(r => !r.IstGeloescht)
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.Name, r.ProjektId, r.TicketId })
            .ToListAsync();

        return Ok(list);
    }

    [HttpGet("raum/{raumId}/nachrichten")]
    public async Task<IActionResult> GetNachrichten(Guid raumId, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        var nachrichten = await _db.ChatNachrichten
            .Where(n => n.RaumId == raumId)
            .OrderByDescending(n => n.GeschicktAm)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(n => new
            {
                n.Id,
                n.Inhalt,
                n.GeschicktAm,
                Absender = n.Absender == null ? null : new { n.Absender.Vorname, n.Absender.Nachname }
            })
            .ToListAsync();

        return Ok(nachrichten);
    }
}
