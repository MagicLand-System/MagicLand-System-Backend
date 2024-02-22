using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class aaa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
