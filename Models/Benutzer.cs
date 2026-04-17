using Microsoft.AspNetCore.Identity;

namespace Vista.Core.Models;

public class Benutzer : IdentityUser
{
    public string Vorname { get; set; } = string.Empty;
    public string Nachname { get; set; } = string.Empty;
    public string RufNummer { get; set; } = string.Empty;
    public string Abteilung { get; set; } = string.Empty;
    public string Rolle { get; set; } = string.Empty;
    public string Bild { get; set; } = string.Empty;
    public string Hinweise { get; set; } = string.Empty;

    public ICollection<Projekt> Projekte { get; set; } = new List<Projekt>();
}
