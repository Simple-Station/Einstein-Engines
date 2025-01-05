using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class UpstreamMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS ProileLoadouts;");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_read_rules",
                table: "player",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ban_template",
                columns: table => new
                {
                    ban_template_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    length = table.Column<TimeSpan>(type: "interval", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: false),
                    exempt_flags = table.Column<int>(type: "integer", nullable: false),
                    severity = table.Column<int>(type: "integer", nullable: false),
                    auto_delete = table.Column<bool>(type: "boolean", nullable: false),
                    hidden = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ban_template", x => x.ban_template_id);
                });


            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "server_role_ban",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "server_ban",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "last_seen_hwid_type",
                table: "player",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "connection_log",
                type: "integer",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout",
                column: "profile_id",
                principalTable: "profile",
                principalColumn: "profile_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddColumn<float>(
                name: "trust",
                table: "connection_log",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout");

            migrationBuilder.DropColumn(
                name: "last_read_rules",
                table: "player");

            migrationBuilder.DropTable(
                name: "ban_template");

            migrationBuilder.DropColumn(
                name: "hwid_type",
                table: "server_role_ban");

            migrationBuilder.DropColumn(
                name: "hwid_type",
                table: "server_ban");

            migrationBuilder.DropColumn(
                name: "last_seen_hwid_type",
                table: "player");

            migrationBuilder.DropColumn(
                name: "hwid_type",
                table: "connection_log");

            migrationBuilder.CreateTable(
                name: "ProfileLoadout",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "integer", nullable: true)
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

            migrationBuilder.DropColumn(
                name: "trust",
                table: "connection_log");
        }
    }
}
