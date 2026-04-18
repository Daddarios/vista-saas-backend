using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Vista.Core.Models;

namespace Vista.Core.Data;

public static class DataSeeder
{
    /// <summary>
    /// Identity rollerini ve demo admin kullanıcısını seed eder (ADIM 3.9)
    /// </summary>
    public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<Benutzer>>();

        string[] rollen = ["SuperAdmin", "Admin", "Manager", "Standard", "NurLesen"];

        foreach (var rolle in rollen)
        {
            if (!await roleManager.RoleExistsAsync(rolle))
                await roleManager.CreateAsync(new IdentityRole(rolle));
        }

        // Demo SuperAdmin kullanıcı
        const string adminEmail = "admin@vista.local";
        const string adminPassword = "Test123!";
        var existingAdmin = await userManager.FindByEmailAsync(adminEmail);

        if (existingAdmin is null)
        {
            var admin = new Benutzer
            {
                UserName = adminEmail,
                Email = adminEmail,
                Vorname = "System",
                Nachname = "Admin",
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
        }
        else
        {
            // Şifre/lockout düzelt (development reset)
            await userManager.SetLockoutEnabledAsync(existingAdmin, true);
            await userManager.SetLockoutEndDateAsync(existingAdmin, null);
            await userManager.ResetAccessFailedCountAsync(existingAdmin);
            var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
            await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);
        }
    }

    public static async Task SeedAsync(AppDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Mandanten.AnyAsync(cancellationToken))
        {
            return;
        }

        var mandant = new Mandant
        {
            Name = "Demo Mandant",
            Domain = "demo.vista.local",
            IstAktiv = true
        };

        var kunde = new Kunde
        {
            MandantId = mandant.Id,
            Unternehmen = "X Bank",
            Vorname = "Ali",
            Nachname = "Yilmaz",
            Email = "info@xbank.local",
            TelefonMobil = "+49 170 000 0000",
            TelefonHaus = "+49 30 000 000",
            Adresse = "Berlin",
            Website = "https://xbank.local",
            Hinweise = "Demo müşteri kaydı"
        };

        var filiale = new Filiale
        {
            MandantId = mandant.Id,
            Kunde = kunde,
            Name = "X Bank Berlin Şubesi",
            Adresse = "Berlin Mitte",
            Telefon = "+49 30 111 111"
        };

        var ansprechpartner = new Ansprechpartner
        {
            MandantId = mandant.Id,
            Kunde = kunde,
            Filiale = filiale,
            Name = "Ayşe Kara",
            Telefon = "+49 170 111 1111",
            Email = "ayse.kara@xbank.local",
            Abteilung = "Destek"
        };

        var projekt = new Projekt
        {
            MandantId = mandant.Id,
            Kunde = kunde,
            Name = "ATM Arıza Takip",
            Beschreibung = "Banka ATM arızalarının izlenmesi",
            Startdatum = DateTime.UtcNow.Date,
            Status = "InBearbeitung",
            Prioritaet = "Hoch",
            AbschlussInProzent = 15,
            IstAbgeschlossen = false
        };

        var ticket = new Ticket
        {
            MandantId = mandant.Id,
            Kunde = kunde,
            Projekt = projekt,
            Titel = "ATM ekranı siyah kaldı",
            Beschreibung = "Müşteri ATM ekranının çalışmadığını bildirdi.",
            Status = "Offen",
            Prioritaet = "Hoch",
            Kategorie = "ATM",
            Faelligkeitsdatum = DateTime.UtcNow.AddDays(2)
        };

        var bericht = new Bericht
        {
            MandantId = mandant.Id,
            Titel = "Haftalık Servis Raporu",
            DateiPfad = "/Storage/berichte/demo.pdf",
            DateiTyp = "application/pdf",
            Version = "v1"
        };

        var chatRaum = new ChatRaum
        {
            MandantId = mandant.Id,
            Name = "Ticket-ATM-Chat",
            Projekt = projekt,
            Ticket = ticket
        };

        dbContext.Mandanten.Add(mandant);
        dbContext.Kunden.Add(kunde);
        dbContext.Filialen.Add(filiale);
        dbContext.Ansprechpartner.Add(ansprechpartner);
        dbContext.Projekte.Add(projekt);
        dbContext.Tickets.Add(ticket);
        dbContext.Berichte.Add(bericht);
        dbContext.ChatRaeume.Add(chatRaum);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
