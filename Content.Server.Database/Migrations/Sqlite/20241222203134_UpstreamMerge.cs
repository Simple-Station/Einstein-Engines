using System;
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
            migrationBuilder.Sql("ALTER TABLE \"player\" ADD COLUMN IF NOT EXISTS \"last_read_rules\" TEXT;");

            migrationBuilder.CreateTable(
                name: "ban_template",
                columns: table => new
                {
                    ban_template_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    title = table.Column<string>(type: "TEXT", nullable: false),
                    length = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    reason = table.Column<string>(type: "TEXT", nullable: false),
                    exempt_flags = table.Column<int>(type: "INTEGER", nullable: false),
                    severity = table.Column<int>(type: "INTEGER", nullable: false),
                    auto_delete = table.Column<bool>(type: "INTEGER", nullable: false),
                    hidden = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ban_template", x => x.ban_template_id);
                });

            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "server_role_ban",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "server_ban",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "last_seen_hwid_type",
                table: "player",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "hwid_type",
                table: "connection_log",
                type: "INTEGER",
                nullable: true,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "trust",
                table: "connection_log",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.DropColumn(
                name: "trust",
                table: "connection_log");
        }
    }
}
