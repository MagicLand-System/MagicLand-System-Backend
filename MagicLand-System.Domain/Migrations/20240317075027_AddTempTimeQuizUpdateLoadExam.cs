using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddTempTimeQuizUpdateLoadExam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptsAllowed",
                table: "QuestionPackage");

            migrationBuilder.CreateTable(
                name: "TempQuizTime",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ExamEndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    AttemptAllowed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempQuizTime", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TempQuizTime");

            migrationBuilder.AddColumn<int>(
                name: "AttemptsAllowed",
                table: "QuestionPackage",
                type: "int",
                nullable: true);
        }
    }
}
