using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Vista.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddDirektChatFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Benutzer1Id",
                table: "ChatRaeume",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Benutzer2Id",
                table: "ChatRaeume",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IstDirektChat",
                table: "ChatRaeume",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_Benutzer1Id",
                table: "ChatRaeume",
                column: "Benutzer1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatRaeume_Benutzer2Id",
                table: "ChatRaeume",
                column: "Benutzer2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRaeume_AspNetUsers_Benutzer1Id",
                table: "ChatRaeume",
                column: "Benutzer1Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRaeume_AspNetUsers_Benutzer2Id",
                table: "ChatRaeume",
                column: "Benutzer2Id",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRaeume_AspNetUsers_Benutzer1Id",
                table: "ChatRaeume");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatRaeume_AspNetUsers_Benutzer2Id",
                table: "ChatRaeume");

            migrationBuilder.DropIndex(
                name: "IX_ChatRaeume_Benutzer1Id",
                table: "ChatRaeume");

            migrationBuilder.DropIndex(
                name: "IX_ChatRaeume_Benutzer2Id",
                table: "ChatRaeume");

            migrationBuilder.DropColumn(
                name: "Benutzer1Id",
                table: "ChatRaeume");

            migrationBuilder.DropColumn(
                name: "Benutzer2Id",
                table: "ChatRaeume");

            migrationBuilder.DropColumn(
                name: "IstDirektChat",
                table: "ChatRaeume");
        }
    }
}
