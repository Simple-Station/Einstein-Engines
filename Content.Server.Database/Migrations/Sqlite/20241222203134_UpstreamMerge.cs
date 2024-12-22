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

            migrationBuilder.RenameColumn(
                name: "hwid_type",
                table: "server_role_ban",
                newName: "hwid__type");

            migrationBuilder.RenameColumn(
                name: "hwid_type",
                table: "server_ban",
                newName: "hwid__type");

            migrationBuilder.RenameColumn(
                name: "last_seen_hwid_type",
                table: "player",
                newName: "last_seen_hwid__type");

            migrationBuilder.RenameColumn(
                name: "hwid_type",
                table: "connection_log",
                newName: "hwid__type");

            migrationBuilder.AddForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout",
                column: "profile_id",
                principalTable: "profile",
                principalColumn: "profile_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout");

            migrationBuilder.RenameColumn(
                name: "hwid__type",
                table: "server_role_ban",
                newName: "hwid_type");

            migrationBuilder.RenameColumn(
                name: "hwid__type",
                table: "server_ban",
                newName: "hwid_type");

            migrationBuilder.RenameColumn(
                name: "last_seen_hwid__type",
                table: "player",
                newName: "last_seen_hwid_type");

            migrationBuilder.RenameColumn(
                name: "hwid__type",
                table: "connection_log",
                newName: "hwid_type");

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
