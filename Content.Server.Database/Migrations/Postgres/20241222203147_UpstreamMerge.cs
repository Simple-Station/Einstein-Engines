using System;
using Microsoft.EntityFrameworkCore.Migrations;

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

            migrationBuilder.Sql("ALTER TABLE \"player\" ADD COLUMN IF NOT EXISTS \"last_read_rules\" TIMESTAMP WITH TIME ZONE NULL;");

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
