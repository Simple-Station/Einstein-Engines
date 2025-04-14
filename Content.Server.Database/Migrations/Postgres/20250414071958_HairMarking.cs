using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class HairMarking : Migration
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /*
                Nope, theres no reverting this!
                Because the markings json doesn't specify slot in its format (nor does it need to), we cannot roll this change back.
                At the moment, we don't drop the original hair table in case we do want to rollback
            */
        }
    }
}
