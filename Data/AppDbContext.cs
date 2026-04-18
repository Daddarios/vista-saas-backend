using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Vista.Core.Models;
using Vista.Core.Models.Base;

namespace Vista.Core.Data;

public class AppDbContext : IdentityDbContext<Benutzer>
{
    private readonly Guid? _currentMandantId;

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        var mandantHeader = httpContextAccessor.HttpContext?.Request.Headers["X-Mandant-Id"].FirstOrDefault();
        if (Guid.TryParse(mandantHeader, out var mandantId))
        {
            _currentMandantId = mandantId;
        }
    }

    public DbSet<Mandant> Mandanten => Set<Mandant>();
    public DbSet<Kunde> Kunden => Set<Kunde>();
    public DbSet<Filiale> Filialen => Set<Filiale>();
    public DbSet<Ansprechpartner> Ansprechpartner => Set<Ansprechpartner>();
    public DbSet<Projekt> Projekte => Set<Projekt>();
    public DbSet<Bericht> Berichte => Set<Bericht>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketNachricht> TicketNachrichten => Set<TicketNachricht>();
    public DbSet<ChatRaum> ChatRaeume => Set<ChatRaum>();
    public DbSet<ChatNachricht> ChatNachrichten => Set<ChatNachricht>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Taslak — Ödeme & Abonelik
    public DbSet<Abonnement> Abonnements => Set<Abonnement>();
    public DbSet<Zahlung> Zahlungen => Set<Zahlung>();
    public DbSet<Rechnung> Rechnungen => Set<Rechnung>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        if (_currentMandantId is null)
        {
            return;
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(MandantEntity).IsAssignableFrom(t.ClrType)))
        {
            var method = typeof(AppDbContext)
                .GetMethod(nameof(SetMandantFilter), BindingFlags.NonPublic | BindingFlags.Static)?
                .MakeGenericMethod(entityType.ClrType);

            method?.Invoke(null, new object[] { modelBuilder, _currentMandantId.Value });
        }
    }

    private static void SetMandantFilter<TEntity>(ModelBuilder modelBuilder, Guid mandantId)
        where TEntity : MandantEntity
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(e => e.MandantId == mandantId);
    }
}
