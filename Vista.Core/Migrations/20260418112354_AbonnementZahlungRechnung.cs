using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vista.Core.Migrations
{
    /// <inheritdoc />
    public partial class AbonnementZahlungRechnung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Abonnements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MandantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Plan = table.Column<int>(type: "int", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Preis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDatum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstAktiv = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "bit", nullable: false)
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
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    BenutzerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AblaufDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IstWiderrufen = table.Column<bool>(type: "bit", nullable: false),
                    ErsetztDurch = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Rechnungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MandantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AbonnementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nummer = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Betrag = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RechnungsDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FaelligkeitsDatum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PdfPfad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "bit", nullable: false)
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
                name: "Zahlungen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MandantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RechnungId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Betrag = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ZahlungsDatum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TransaktionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IBAN = table.Column<string>(type: "nvarchar(34)", maxLength: 34, nullable: true),
                    Hinweise = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktualisiertAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IstGeloescht = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_Abonnements_MandantId",
                table: "Abonnements",
                column: "MandantId");

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
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Zahlungen");

            migrationBuilder.DropTable(
                name: "Rechnungen");

            migrationBuilder.DropTable(
                name: "Abonnements");
        }
    }
}
