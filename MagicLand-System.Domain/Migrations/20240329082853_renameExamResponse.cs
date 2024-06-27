using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class renameExamResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExamType",
                table: "TestResult",
                newName: "QuizType");

            migrationBuilder.RenameColumn(
                name: "ExamCategory",
                table: "TestResult",
                newName: "QuizName");

            migrationBuilder.AddColumn<string>(
                name: "QuizCategory",
                table: "TestResult",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuizCategory",
                table: "TestResult");

            migrationBuilder.RenameColumn(
                name: "QuizType",
                table: "TestResult",
                newName: "ExamType");

            migrationBuilder.RenameColumn(
                name: "QuizName",
                table: "TestResult",
                newName: "ExamCategory");
        }
    }
}
