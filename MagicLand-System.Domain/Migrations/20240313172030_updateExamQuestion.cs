using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateExamQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "QuestionId",
                table: "ExamQuestion",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuestionId",
                table: "ExamQuestion");
        }
    }
}
