using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateNameEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_TestResult_ExamResultResultId",
                table: "ExamQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_TestResult_StudentClass_StudentClassId",
                table: "TestResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TestResult",
                table: "TestResult");

            migrationBuilder.RenameTable(
                name: "TestResult",
                newName: "ExamResult");

            migrationBuilder.RenameIndex(
                name: "IX_TestResult_StudentClassId",
                table: "ExamResult",
                newName: "IX_ExamResult_StudentClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExamResult",
                table: "ExamResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_ExamResult_ExamResultResultId",
                table: "ExamQuestion",
                column: "ExamResultResultId",
                principalTable: "ExamResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamResult_StudentClass_StudentClassId",
                table: "ExamResult",
                column: "StudentClassId",
                principalTable: "StudentClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_ExamResult_ExamResultResultId",
                table: "ExamQuestion");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamResult_StudentClass_StudentClassId",
                table: "ExamResult");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExamResult",
                table: "ExamResult");

            migrationBuilder.RenameTable(
                name: "ExamResult",
                newName: "TestResult");

            migrationBuilder.RenameIndex(
                name: "IX_ExamResult_StudentClassId",
                table: "TestResult",
                newName: "IX_TestResult_StudentClassId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TestResult",
                table: "TestResult",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_TestResult_ExamResultResultId",
                table: "ExamQuestion",
                column: "ExamResultResultId",
                principalTable: "TestResult",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TestResult_StudentClass_StudentClassId",
                table: "TestResult",
                column: "StudentClassId",
                principalTable: "StudentClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
