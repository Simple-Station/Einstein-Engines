using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class LoadoutProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout");

            migrationBuilder.DropIndex(
                name: "IX_loadout_profile_id_loadout_name",
                table: "loadout");

            migrationBuilder.RenameColumn(
                name: "loadout_name",
                table: "loadout",
                newName: "player_user_id");

            migrationBuilder.AddColumn<int>(
                name: "current_loadout",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "profile_id",
                table: "loadout",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "custom_name",
                table: "loadout",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "items",
                table: "loadout",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "loadout_job_preference",
                columns: table => new
                {
                    loadout_job_preference_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    loadout_idx = table.Column<int>(type: "INTEGER", nullable: false),
                    job_name = table.Column<string>(type: "TEXT", nullable: false),
                    profile_id = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_loadout_job_preference", x => x.loadout_job_preference_id);
                    table.ForeignKey(
                        name: "FK_loadout_job_preference_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_loadout_profile_id",
                table: "loadout",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_loadout_job_preference_profile_id",
                table: "loadout_job_preference",
                column: "profile_id");

            migrationBuilder.CreateIndex(
                name: "IX_one_loadout_per_job",
                table: "loadout_job_preference",
                columns: new[] { "loadout_idx", "job_name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout",
                column: "profile_id",
                principalTable: "profile",
                principalColumn: "profile_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout");

            migrationBuilder.DropTable(
                name: "loadout_job_preference");

            migrationBuilder.DropIndex(
                name: "IX_loadout_profile_id",
                table: "loadout");

            migrationBuilder.DropColumn(
                name: "current_loadout",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "custom_name",
                table: "loadout");

            migrationBuilder.DropColumn(
                name: "items",
                table: "loadout");

            migrationBuilder.RenameColumn(
                name: "player_user_id",
                table: "loadout",
                newName: "loadout_name");

            migrationBuilder.AlterColumn<int>(
                name: "profile_id",
                table: "loadout",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_loadout_profile_id_loadout_name",
                table: "loadout",
                columns: new[] { "profile_id", "loadout_name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_loadout_profile_profile_id",
                table: "loadout",
                column: "profile_id",
                principalTable: "profile",
                principalColumn: "profile_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
