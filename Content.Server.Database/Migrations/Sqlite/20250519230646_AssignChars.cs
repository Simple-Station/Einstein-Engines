using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AssignChars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "preference_id",
                table: "job",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_preference_id",
                table: "job",
                column: "preference_id");

            migrationBuilder.AddForeignKey(
                name: "FK_job_preference_preference_id",
                table: "job",
                column: "preference_id",
                principalTable: "preference",
                principalColumn: "preference_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_job_preference_preference_id",
                table: "job");

            migrationBuilder.DropIndex(
                name: "IX_job_preference_id",
                table: "job");

            migrationBuilder.DropColumn(
                name: "preference_id",
                table: "job");
        }
    }
}
