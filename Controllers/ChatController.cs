using System.Security.Claims;
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

    private Guid RequireMandantId()
    {
        var mandantId = GetMandantId();
        if (mandantId == null)
            throw new Exception("X-Mandant-Id Header fehlt");
        return mandantId.Value;
    }

    [HttpGet("raeume")]
    public async Task<IActionResult> GetRaeume()
    {
        try
        {
            var mandantId = RequireMandantId();
            var raeume = await _db.ChatRaeume
                .AsNoTracking()
                .Where(r => !r.IstGeloescht && r.MandantId == mandantId)
                .Include(r => r.Projekt!).ThenInclude(p => p.Benutzer)
                .Include(r => r.Ticket!).ThenInclude(t => t.ZugewiesenAn)
                .OrderBy(r => r.Name)
                .ToListAsync();

            var result = raeume.Select(r =>
            {
                var teilnehmer = new List<object>();
                if (r.Projekt != null)
                {
                    teilnehmer.AddRange(r.Projekt.Benutzer.Select(b => (object)new
                    {
                        id = b.Id,
                        vorname = b.Vorname,
                        nachname = b.Nachname,
                        bild = b.Bild
                    }));
                }
                else if (r.Ticket?.ZugewiesenAn != null)
                {
                    teilnehmer.Add(new
                    {
                        id = r.Ticket.ZugewiesenAn.Id,
                        vorname = r.Ticket.ZugewiesenAn.Vorname,
                        nachname = r.Ticket.ZugewiesenAn.Nachname,
                        bild = r.Ticket.ZugewiesenAn.Bild
                    });
                }

                return new
                {
                    id = r.Id,
                    name = r.Name,
                    projektId = r.ProjektId,
                    ticketId = r.TicketId,
                    teilnehmer
                };
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("raum/{raumId}/nachrichten")]
    public async Task<IActionResult> GetNachrichten(Guid raumId, [FromQuery] int page = 1, [FromQuery] int size = 50)
    {
        try
        {
            var mandantId = RequireMandantId();
            var raum = await _db.ChatRaeume
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == raumId && r.MandantId == mandantId && !r.IstGeloescht);
            if (raum == null) return NotFound();

            if (size < 1) size = 50;
            if (size > 200) size = 200;
            if (page < 1) page = 1;

            var query = _db.ChatNachrichten
                .AsNoTracking()
                .Where(n => n.RaumId == raumId);

            var total = await query.CountAsync();

            var slice = await query
                .OrderByDescending(n => n.GeschicktAm)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(n => new
                {
                    id = n.Id,
                    inhalt = n.Inhalt,
                    geschicktAm = n.GeschicktAm,
                    absenderId = n.AbsenderId,
                    absender = n.Absender == null ? null : new
                    {
                        vorname = n.Absender.Vorname,
                        nachname = n.Absender.Nachname,
                        bild = n.Absender.Bild
                    }
                })
                .ToListAsync();

            var items = slice.OrderBy(n => n.geschicktAm).ToList();

            return Ok(new { total, page, size, items });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid? GetMandantId()
    {
        var header = Request.Headers["X-Mandant-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : null;
    }
}
