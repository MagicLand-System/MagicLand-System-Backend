using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateAnwerTestResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "ExamQuestion");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ExamQuestion");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "MultipleChoiceAnswer",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "MultipleChoiceAnswer",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "FlashCardAnswer",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "FlashCardAnswer",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Score",
                table: "MultipleChoiceAnswer");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "MultipleChoiceAnswer");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "FlashCardAnswer");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "FlashCardAnswer");

            migrationBuilder.AddColumn<double>(
                name: "Score",
                table: "ExamQuestion",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ExamQuestion",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
