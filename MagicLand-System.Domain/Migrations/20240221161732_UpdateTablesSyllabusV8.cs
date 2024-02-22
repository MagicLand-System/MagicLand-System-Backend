using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesSyllabusV8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
