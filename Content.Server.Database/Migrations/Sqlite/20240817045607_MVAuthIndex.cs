using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class MVAuthIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_player_last_seen_address",
                table: "player",
                column: "last_seen_address");

            migrationBuilder.CreateIndex(
                name: "IX_player_last_seen_hwid",
                table: "player",
                column: "last_seen_hwid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_player_last_seen_address",
                table: "player");

            migrationBuilder.DropIndex(
                name: "IX_player_last_seen_hwid",
                table: "player");
        }
    }
}
