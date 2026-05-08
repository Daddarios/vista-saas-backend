using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vista.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddChatFileSharing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DateiGroesse",
                table: "ChatNachrichten",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateiName",
                table: "ChatNachrichten",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateiPfad",
                table: "ChatNachrichten",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DateiTyp",
                table: "ChatNachrichten",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IstDatei",
                table: "ChatNachrichten",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateiGroesse",
                table: "ChatNachrichten");

            migrationBuilder.DropColumn(
                name: "DateiName",
                table: "ChatNachrichten");

            migrationBuilder.DropColumn(
                name: "DateiPfad",
                table: "ChatNachrichten");

            migrationBuilder.DropColumn(
                name: "DateiTyp",
                table: "ChatNachrichten");

            migrationBuilder.DropColumn(
                name: "IstDatei",
                table: "ChatNachrichten");
        }
    }
}
