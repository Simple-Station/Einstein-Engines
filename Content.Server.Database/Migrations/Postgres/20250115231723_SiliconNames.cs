using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class SiliconNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cyborg_name",
                table: "profile",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "station_ai_name",
                table: "profile",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cyborg_name",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "station_ai_name",
                table: "profile");
        }
    }
}
