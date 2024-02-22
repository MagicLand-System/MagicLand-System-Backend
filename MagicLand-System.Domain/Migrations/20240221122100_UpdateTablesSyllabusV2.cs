using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesSyllabusV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Target",
                table: "CourseSyllabus",
                newName: "StudentTasks");

            migrationBuilder.RenameColumn(
                name: "PointGrade",
                table: "CourseSyllabus",
                newName: "ScoringScale");

            migrationBuilder.RenameColumn(
                name: "NumberMinuteOfSession",
                table: "CourseSyllabus",
                newName: "TimePerSession");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimePerSession",
                table: "CourseSyllabus",
                newName: "NumberMinuteOfSession");

            migrationBuilder.RenameColumn(
                name: "StudentTasks",
                table: "CourseSyllabus",
                newName: "Target");

            migrationBuilder.RenameColumn(
                name: "ScoringScale",
                table: "CourseSyllabus",
                newName: "PointGrade");
        }
    }
}
