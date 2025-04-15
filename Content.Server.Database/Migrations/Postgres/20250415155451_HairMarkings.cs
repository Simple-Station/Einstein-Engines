using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class HairMarkings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE profile
                SET markings = jsonb_set(
                    markings,
                    '{0}',
                    COALESCE(markings->'0', '[]'::jsonb) || to_jsonb(hair_name || '@' || hair_color)
                )
                WHERE hair_name IS NOT NULL AND hair_color IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                UPDATE profile
                SET markings = jsonb_set(
                    markings,
                    '{0}',
                    COALESCE(markings->'0', '[]'::jsonb) || to_jsonb(facial_hair_name || '@' || facial_hair_color)
                )
                WHERE facial_hair_name IS NOT NULL AND facial_hair_color IS NOT NULL;
            ");

            migrationBuilder.DropColumn(
                name: "facial_hair_color",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "facial_hair_name",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "hair_color",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "hair_name",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "eye_color",
                table: "profile");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
                You have been warned!
                This rollback incurs dataloss because markings neither store in a deterministic order nor do they key which slot they go in
                It is impossible to determine which marking is a hair
            */
            migrationBuilder.AddColumn<string>(
                name: "facial_hair_color",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "facial_hair_name",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "hair_color",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "hair_name",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "eye_color",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
