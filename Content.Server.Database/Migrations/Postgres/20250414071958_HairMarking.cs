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
                UPDATE your_table_name
                SET markings = jsonb_set(
                    COALESCE(markings, '{}'::jsonb),
                    '{hair_attributes}',
                    COALESCE(markings->'hair_attributes', '[]'::jsonb) ||
                    to_jsonb(hair_name || '@' || hair_colour)
                )
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
