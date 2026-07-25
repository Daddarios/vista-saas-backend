using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Vista.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Vorname = table.Column<string>(type: "text", nullable: false),
                    Nachname = table.Column<string>(type: "text", nullable: false),
                    RufNummer = table.Column<string>(type: "text", nullable: false),
                    Abteilung = table.Column<string>(type: "text", nullable: false),
                    Rolle = table.Column<string>(type: "text", nullable: false),
                    Bild = table.Column<string>(type: "text", nullable: false),
                    Hinweise = table.Column<string>(type: "text", nullable: false),
                    MandantId = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Berichte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titel = table.Column<string>(type: "text", nullable: false),
                    DateiPfad = table.Column<string>(type: "text", nullable: false),
                    DateiTyp = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: false),
                    HochgeladenAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BearbeitetAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Berichte", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Kunden",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Unternehmen = table.Column<string>(type: "text", nullable: false),
                    Vorname = table.Column<string>(type: "text", nullable: false),
                    Nachname = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    TelefonMobil = table.Column<string>(type: "text", nullable: false),
                    TelefonHaus = table.Column<string>(type: "text", nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false),
                    Website = table.Column<string>(type: "text", nullable: false),
                    Logo = table.Column<string>(type: "text", nullable: false),
                    Hinweise = table.Column<string>(type: "text", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kunden", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mandanten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Domain = table.Column<string>(type: "text", nullable: false),
                    IstAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mandanten", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    BenutzerId = table.Column<string>(type: "text", nullable: false),
                    AblaufDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IstWiderrufen = table.Column<bool>(type: "boolean", nullable: false),
                    ErsetztDurch = table.Column<string>(type: "text", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_AspNetUsers_BenutzerId",
                        column: x => x.BenutzerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Filialen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Adresse = table.Column<string>(type: "text", nullable: false),
                    Telefon = table.Column<string>(type: "text", nullable: false),
                    KundeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Filialen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Filialen_Kunden_KundeId",
                        column: x => x.KundeId,
                        principalTable: "Kunden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Projekte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Beschreibung = table.Column<string>(type: "text", nullable: false),
                    Startdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Enddatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Prioritaet = table.Column<string>(type: "text", nullable: false),
                    AbschlussInProzent = table.Column<int>(type: "integer", nullable: false),
                    IstAbgeschlossen = table.Column<bool>(type: "boolean", nullable: false),
                    KundeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projekte", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projekte_Kunden_KundeId",
                        column: x => x.KundeId,
                        principalTable: "Kunden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Abonnements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    PlanName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Preis = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    StartDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstAktiv = table.Column<bool>(type: "boolean", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Abonnements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Abonnements_Mandanten_MandantId",
                        column: x => x.MandantId,
                        principalTable: "Mandanten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ansprechpartner",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Telefon = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Abteilung = table.Column<string>(type: "text", nullable: false),
                    KundeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilialeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ansprechpartner", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ansprechpartner_Filialen_FilialeId",
                        column: x => x.FilialeId,
                        principalTable: "Filialen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ansprechpartner_Kunden_KundeId",
                        column: x => x.KundeId,
                        principalTable: "Kunden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BenutzerProjekte",
                columns: table => new
                {
                    ProjektId = table.Column<Guid>(type: "uuid", nullable: false),
                    BenutzerId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BenutzerProjekte", x => new { x.ProjektId, x.BenutzerId });
                    table.ForeignKey(
                        name: "FK_BenutzerProjekte_AspNetUsers_BenutzerId",
                        column: x => x.BenutzerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BenutzerProjekte_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titel = table.Column<string>(type: "text", nullable: false),
                    Beschreibung = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Prioritaet = table.Column<string>(type: "text", nullable: false),
                    Kategorie = table.Column<string>(type: "text", nullable: false),
                    Faelligkeitsdatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ZugewiesenAnId = table.Column<string>(type: "text", nullable: true),
                    KundeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjektId = table.Column<Guid>(type: "uuid", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tickets_AspNetUsers_ZugewiesenAnId",
                        column: x => x.ZugewiesenAnId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Tickets_Kunden_KundeId",
                        column: x => x.KundeId,
                        principalTable: "Kunden",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Tickets_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Rechnungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbonnementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nummer = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Betrag = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    RechnungsDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FaelligkeitsDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PdfPfad = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rechnungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rechnungen_Abonnements_AbonnementId",
                        column: x => x.AbonnementId,
                        principalTable: "Abonnements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Rechnungen_Mandanten_MandantId",
                        column: x => x.MandantId,
                        principalTable: "Mandanten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatRaeume",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ProjektId = table.Column<Guid>(type: "uuid", nullable: true),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: true),
                    IstDirektChat = table.Column<bool>(type: "boolean", nullable: false),
                    Benutzer1Id = table.Column<string>(type: "text", nullable: true),
                    Benutzer2Id = table.Column<string>(type: "text", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatRaeume", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatRaeume_AspNetUsers_Benutzer1Id",
                        column: x => x.Benutzer1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRaeume_AspNetUsers_Benutzer2Id",
                        column: x => x.Benutzer2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatRaeume_Projekte_ProjektId",
                        column: x => x.ProjektId,
                        principalTable: "Projekte",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChatRaeume_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketNachrichten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbsenderId = table.Column<string>(type: "text", nullable: false),
                    Inhalt = table.Column<string>(type: "text", nullable: false),
                    IstInternNotiz = table.Column<bool>(type: "boolean", nullable: false),
                    GeschicktAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketNachrichten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketNachrichten_AspNetUsers_AbsenderId",
                        column: x => x.AbsenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketNachrichten_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Zahlungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false),
                    RechnungId = table.Column<Guid>(type: "uuid", nullable: true),
                    Betrag = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ZahlungsDatum = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransaktionId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: true),
                    Hinweise = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zahlungen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Zahlungen_Mandanten_MandantId",
                        column: x => x.MandantId,
                        principalTable: "Mandanten",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Zahlungen_Rechnungen_RechnungId",
                        column: x => x.RechnungId,
                        principalTable: "Rechnungen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ChatNachrichten",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RaumId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbsenderId = table.Column<string>(type: "text", nullable: false),
                    Inhalt = table.Column<string>(type: "text", nullable: false),
                    GeschicktAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IstDatei = table.Column<bool>(type: "boolean", nullable: false),
                    DateiPfad = table.Column<string>(type: "text", nullable: true),
                    DateiName = table.Column<string>(type: "text", nullable: true),
                    DateiTyp = table.Column<string>(type: "text", nullable: true),
                    DateiGroesse = table.Column<long>(type: "bigint", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "boolean", nullable: false),
                    MandantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatNachrichten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatNachrichten_AspNetUsers_AbsenderId",
                        column: x => x.AbsenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatNachrichten_ChatRaeume_RaumId",
                        column: x => x.RaumId,
                        principalTable: "ChatRaeume",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Abonnements_MandantId",
                table: "Abonnements",
                column: "MandantId");

            migrationBuilder.CreateIndex(
                name: "IX_Ansprechpartner_FilialeId",
                table: "Ansprechpartner",
                column: "FilialeId");

            migrationBuilder.CreateIndex(
                name: "IX_Ansprechpartner_KundeId",
                table: "Ansprechpartner",
                column: "KundeId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BenutzerProjekte_BenutzerId",
                table: "BenutzerProjekte",
                column: "BenutzerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNachrichten_AbsenderId",
                table: "ChatNachrichten",
                column: "AbsenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatNachrichten_RaumId",
                table: "ChatNachrichten",
                column: "RaumId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_Benutzer1Id",
                table: "ChatRaeume",
                column: "Benutzer1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_Benutzer2Id",
                table: "ChatRaeume",
                column: "Benutzer2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_ProjektId",
                table: "ChatRaeume",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_TicketId",
                table: "ChatRaeume",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Filialen_KundeId",
                table: "Filialen",
                column: "KundeId");

            migrationBuilder.CreateIndex(
                name: "IX_Projekte_KundeId",
                table: "Projekte",
                column: "KundeId");

            migrationBuilder.CreateIndex(
                name: "IX_Rechnungen_AbonnementId",
                table: "Rechnungen",
                column: "AbonnementId");

            migrationBuilder.CreateIndex(
                name: "IX_Rechnungen_MandantId",
                table: "Rechnungen",
                column: "MandantId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_BenutzerId",
                table: "RefreshTokens",
                column: "BenutzerId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketNachrichten_AbsenderId",
                table: "TicketNachrichten",
                column: "AbsenderId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNachrichten_TicketId",
                table: "TicketNachrichten",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_KundeId",
                table: "Tickets",
                column: "KundeId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ProjektId",
                table: "Tickets",
                column: "ProjektId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_ZugewiesenAnId",
                table: "Tickets",
                column: "ZugewiesenAnId");

            migrationBuilder.CreateIndex(
                name: "IX_Zahlungen_MandantId",
                table: "Zahlungen",
                column: "MandantId");

            migrationBuilder.CreateIndex(
                name: "IX_Zahlungen_RechnungId",
                table: "Zahlungen",
                column: "RechnungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ansprechpartner");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BenutzerProjekte");

            migrationBuilder.DropTable(
                name: "Berichte");

            migrationBuilder.DropTable(
                name: "ChatNachrichten");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "TicketNachrichten");

            migrationBuilder.DropTable(
                name: "Zahlungen");

            migrationBuilder.DropTable(
                name: "Filialen");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ChatRaeume");

            migrationBuilder.DropTable(
                name: "Rechnungen");

            migrationBuilder.DropTable(
                name: "Tickets");

            migrationBuilder.DropTable(
                name: "Abonnements");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Projekte");

            migrationBuilder.DropTable(
                name: "Mandanten");

            migrationBuilder.DropTable(
                name: "Kunden");
        }
    }
}
