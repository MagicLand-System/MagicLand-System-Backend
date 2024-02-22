using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesSyllabusV9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_QuestionPackageId",
                table: "Question");

            migrationBuilder.DropColumn(
                name: "QuestionPackageId",
                table: "Question");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionPacketId",
                table: "Question",
                column: "QuestionPacketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPacketId",
                table: "Question",
                column: "QuestionPacketId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPacketId",
                table: "Question");

            migrationBuilder.DropIndex(
                name: "IX_Question_QuestionPacketId",
                table: "Question");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionPackageId",
                table: "Question",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionPackageId",
                table: "Question",
                column: "QuestionPackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Question_QuestionPackage_QuestionPackageId",
                table: "Question",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
