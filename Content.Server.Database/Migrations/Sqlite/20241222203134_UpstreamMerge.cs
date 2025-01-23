using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class UpstreamMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS ProileLoadouts;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileLoadout",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.ForeignKey(
                        name: "FK_loadout_profile_profile_id",
                        column: x => x.ProfileId,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
