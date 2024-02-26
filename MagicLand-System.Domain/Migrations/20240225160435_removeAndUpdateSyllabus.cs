using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class removeAndUpdateSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExamOrder",
                table: "ExamSyllabus");

            migrationBuilder.RenameColumn(
                name: "PackageOrder",
                table: "QuestionPackage",
                newName: "AttemptsAllowed");

            migrationBuilder.RenameColumn(
                name: "PrerequisiteCourseId",
                table: "CoursePrerequisite",
                newName: "PrerequisiteSyllabusId");

            migrationBuilder.AddColumn<string>(
                name: "ContentName",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ContentName",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentName",
                table: "QuestionPackage");

            migrationBuilder.RenameColumn(
                name: "AttemptsAllowed",
                table: "QuestionPackage",
                newName: "PackageOrder");

            migrationBuilder.RenameColumn(
                name: "PrerequisiteSyllabusId",
                table: "CoursePrerequisite",
                newName: "PrerequisiteCourseId");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentName",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExamOrder",
                table: "ExamSyllabus",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
