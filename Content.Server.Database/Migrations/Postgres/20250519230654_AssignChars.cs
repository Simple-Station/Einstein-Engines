using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AssignChars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "assigned_char_slot",
                table: "job",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<int>(
                name: "preference_id",
                table: "job",
                type: "integer",
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

            migrationBuilder.AlterColumn<int>(
                name: "assigned_char_slot",
                table: "job",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
