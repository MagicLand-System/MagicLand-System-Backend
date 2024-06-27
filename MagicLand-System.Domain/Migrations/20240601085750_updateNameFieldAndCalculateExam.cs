using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateNameFieldAndCalculateExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CorrectRightCardAnswerImage",
                table: "FlashCardAnswer",
                newName: "StudentCardAnswerImage");

            migrationBuilder.RenameColumn(
                name: "CorrectRightCardAnswerId",
                table: "FlashCardAnswer",
                newName: "StudentCardAnswerId");

            migrationBuilder.RenameColumn(
                name: "CorrectRightCardAnswer",
                table: "FlashCardAnswer",
                newName: "StudentCardAnswer");

            migrationBuilder.AddColumn<bool>(
                name: "IsGraded",
                table: "ExamResult",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGraded",
                table: "ExamResult");

            migrationBuilder.RenameColumn(
                name: "StudentCardAnswerImage",
                table: "FlashCardAnswer",
                newName: "CorrectRightCardAnswerImage");

            migrationBuilder.RenameColumn(
                name: "StudentCardAnswerId",
                table: "FlashCardAnswer",
                newName: "CorrectRightCardAnswerId");

            migrationBuilder.RenameColumn(
                name: "StudentCardAnswer",
                table: "FlashCardAnswer",
                newName: "CorrectRightCardAnswer");
        }
    }
}
