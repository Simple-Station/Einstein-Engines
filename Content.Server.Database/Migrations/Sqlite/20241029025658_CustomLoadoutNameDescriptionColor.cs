using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
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
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_description",
                table: "loadout",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "custom_name",
                table: "loadout",
                type: "TEXT",
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
