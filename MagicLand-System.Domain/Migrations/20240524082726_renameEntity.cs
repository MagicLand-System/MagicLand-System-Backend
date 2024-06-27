using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class renameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_TestResult_TestResultId",
                table: "ExamQuestion");

            migrationBuilder.RenameColumn(
                name: "TestResultId",
                table: "ExamQuestion",
                newName: "ExamResultResultId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestion_TestResultId",
                table: "ExamQuestion",
                newName: "IX_ExamQuestion_ExamResultResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_TestResult_ExamResultResultId",
                table: "ExamQuestion",
                column: "ExamResultResultId",
                principalTable: "TestResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_TestResult_ExamResultResultId",
                table: "ExamQuestion");

            migrationBuilder.RenameColumn(
                name: "ExamResultResultId",
                table: "ExamQuestion",
                newName: "TestResultId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamQuestion_ExamResultResultId",
                table: "ExamQuestion",
                newName: "IX_ExamQuestion_TestResultId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_TestResult_TestResultId",
                table: "ExamQuestion",
                column: "TestResultId",
                principalTable: "TestResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
