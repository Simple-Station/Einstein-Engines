using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class MVAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "public_key",
                table: "player",
                type: "bytea",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "public_key",
                table: "connection_log",
                type: "bytea",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_public_key",
                table: "player",
                column: "public_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_player_public_key",
                table: "player");

            migrationBuilder.DropColumn(
                name: "public_key",
                table: "player");

            migrationBuilder.DropColumn(
                name: "public_key",
                table: "connection_log");
        }
    }
}
