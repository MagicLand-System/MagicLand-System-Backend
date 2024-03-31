using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updtetempEntityAndQuestionPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeadlineTime",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "ExamSyllabus");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "TempQuizTime",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "QuestionPackage",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OrderPackage",
                table: "QuestionPackage",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "NoSession",
                table: "QuestionPackage",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContentName",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageType",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "QuizType",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "TempQuizTime");

            migrationBuilder.DropColumn(
                name: "PackageType",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "QuizType",
                table: "QuestionPackage");

            migrationBuilder.AlterColumn<int>(
                name: "Score",
                table: "QuestionPackage",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "OrderPackage",
                table: "QuestionPackage",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "NoSession",
                table: "QuestionPackage",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ContentName",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "DeadlineTime",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
