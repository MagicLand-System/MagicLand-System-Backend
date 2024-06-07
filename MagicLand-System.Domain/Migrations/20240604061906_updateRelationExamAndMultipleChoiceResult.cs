using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateRelationExamAndMultipleChoiceResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_MultipleChoiceAnswer_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.DropIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.DropColumn(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.CreateIndex(
                name: "IX_MultipleChoiceAnswer_ExamQuestionId",
                table: "MultipleChoiceAnswer",
                column: "ExamQuestionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MultipleChoiceAnswer_ExamQuestion_ExamQuestionId",
                table: "MultipleChoiceAnswer",
                column: "ExamQuestionId",
                principalTable: "ExamQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultipleChoiceAnswer_ExamQuestion_ExamQuestionId",
                table: "MultipleChoiceAnswer");

            migrationBuilder.DropIndex(
                name: "IX_MultipleChoiceAnswer_ExamQuestionId",
                table: "MultipleChoiceAnswer");

            migrationBuilder.AddColumn<Guid>(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                unique: true,
                filter: "[MultipleChoiceAnswerId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_MultipleChoiceAnswer_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                principalTable: "MultipleChoiceAnswer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
