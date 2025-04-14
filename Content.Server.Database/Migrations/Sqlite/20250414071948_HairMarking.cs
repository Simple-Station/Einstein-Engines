using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class HairMarking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE profile
                SET markings =
                    json_insert(
                        IFNULL(markings, '[]'),
                        '$[#]',
                        hair_name || '@' || hair_color)
                WHERE markings IS NOT NULL OR markings != ''
            ");

            migrationBuilder.Sql(@"
                UPDATE profile
                SET markings =
                    json_insert(
                        IFNULL(markings, '[]'),
                        '$[#]',
                        facial_hair_name || '@' || facial_hair_color)
                WHERE markings IS NOT NULL OR markings != ''
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
