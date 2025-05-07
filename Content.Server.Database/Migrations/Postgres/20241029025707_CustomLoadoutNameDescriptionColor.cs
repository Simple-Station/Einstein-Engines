using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class CustomLoadoutNameDescriptionColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "custom_color_tint",
                table: "loadout",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_description",
                table: "loadout",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_name",
                table: "loadout",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "custom_color_tint",
                table: "loadout");

            migrationBuilder.DropColumn(
                name: "custom_description",
                table: "loadout");

            migrationBuilder.DropColumn(
                name: "custom_name",
                table: "loadout");
        }
    }
}
