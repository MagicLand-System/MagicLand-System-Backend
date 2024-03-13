using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addNullMultipleChoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.AlterColumn<Guid>(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                unique: true,
                filter: "[MultipleChoiceAnswerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.AlterColumn<Guid>(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                unique: true);
        }
    }
}
