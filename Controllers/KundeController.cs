using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vista.Core.Data;
using Vista.Core.DTOs.Common;
using Vista.Core.DTOs.Kunde;
using Vista.Core.Models;

namespace Vista.Core.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class KundeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<KundeController> _logger;

    public KundeController(AppDbContext db, ILogger<KundeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] string? search = null)
    {
        var query = _db.Kunden.Where(k => !k.IstGeloescht).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(k => k.Unternehmen.Contains(search) || k.Vorname.Contains(search) || k.Nachname.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(k => k.ErstelltAm)
            .Skip((page - 1) * size)
            .Take(size)
            .Select(k => new KundeResponseDto
            {
                Id = k.Id,
                MandantId = k.MandantId,
                Unternehmen = k.Unternehmen,
                Vorname = k.Vorname,
                Nachname = k.Nachname,
                Email = k.Email,
                TelefonMobil = k.TelefonMobil,
                TelefonHaus = k.TelefonHaus,
                Adresse = k.Adresse,
                Website = k.Website,
                Logo = k.Logo,
                Hinweise = k.Hinweise
            })
            .ToListAsync();

        return Ok(new PagedResult<KundeResponseDto>
        {
            Items = items,
            PageNumber = page,
            PageSize = size,
            TotalCount = total
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var k = await _db.Kunden.FirstOrDefaultAsync(x => x.Id == id && !x.IstGeloescht);
        if (k is null) return NotFound(new { Nachricht = "Kunde nicht gefunden." });

        return Ok(new KundeResponseDto
        {
            Id = k.Id,
            MandantId = k.MandantId,
            Unternehmen = k.Unternehmen,
            Vorname = k.Vorname,
            Nachname = k.Nachname,
            Email = k.Email,
            TelefonMobil = k.TelefonMobil,
            TelefonHaus = k.TelefonHaus,
            Adresse = k.Adresse,
            Website = k.Website,
            Logo = k.Logo,
            Hinweise = k.Hinweise
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KundeRequestDto dto)
    {
        var mandantId = GetMandantId();
        if (mandantId is null) return BadRequest(new { Nachricht = "MandantId fehlt." });

        var kunde = new Kunde
        {
            MandantId = mandantId.Value,
            Unternehmen = dto.Unternehmen,
            Vorname = dto.Vorname,
            Nachname = dto.Nachname,
            Email = dto.Email,
            TelefonMobil = dto.TelefonMobil,
            TelefonHaus = dto.TelefonHaus,
            Adresse = dto.Adresse,
            Website = dto.Website,
            Logo = dto.Logo,
            Hinweise = dto.Hinweise
        };

        _db.Kunden.Add(kunde);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Kunde erstellt | Id: {Id}", kunde.Id);
        return CreatedAtAction(nameof(GetById), new { id = kunde.Id }, new { kunde.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] KundeRequestDto dto)
    {
        var kunde = await _db.Kunden.FirstOrDefaultAsync(k => k.Id == id && !k.IstGeloescht);
        if (kunde is null) return NotFound(new { Nachricht = "Kunde nicht gefunden." });

        kunde.Unternehmen = dto.Unternehmen;
        kunde.Vorname = dto.Vorname;
        kunde.Nachname = dto.Nachname;
        kunde.Email = dto.Email;
        kunde.TelefonMobil = dto.TelefonMobil;
        kunde.TelefonHaus = dto.TelefonHaus;
        kunde.Adresse = dto.Adresse;
        kunde.Website = dto.Website;
        kunde.Logo = dto.Logo;
        kunde.Hinweise = dto.Hinweise;
        kunde.AktualisiertAm = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Kunde aktualisiert | Id: {Id}", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var kunde = await _db.Kunden.FirstOrDefaultAsync(k => k.Id == id && !k.IstGeloescht);
        if (kunde is null) return NotFound(new { Nachricht = "Kunde nicht gefunden." });

        kunde.IstGeloescht = true;
        kunde.AktualisiertAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Kunde soft-deleted | Id: {Id}", id);
        return NoContent();
    }

    private Guid? GetMandantId()
    {
        var header = Request.Headers["X-Mandant-Id"].FirstOrDefault();
        return Guid.TryParse(header, out var id) ? id : null;
    }
}
