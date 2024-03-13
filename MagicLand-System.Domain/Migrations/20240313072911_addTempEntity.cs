using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addTempEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MultipleChoiceAnswer_ExamQuestion_ExamQuestionId",
                table: "MultipleChoiceAnswer");

            migrationBuilder.DropIndex(
                name: "IX_MultipleChoiceAnswer_ExamQuestionId",
                table: "MultipleChoiceAnswer");

            migrationBuilder.AddColumn<int>(
                name: "NoAttempt",
                table: "TestResult",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "ScoreEarned",
                table: "TestResult",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<Guid>(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TempQuiz",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGraded = table.Column<bool>(type: "bit", nullable: false),
                    ExamType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMark = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempQuiz", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TempQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TempQuizId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempQuestion_TempQuiz_TempQuizId",
                        column: x => x.TempQuizId,
                        principalTable: "TempQuiz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TempMCAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    TempQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempMCAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempMCAnswer_TempQuestion_TempQuestionId",
                        column: x => x.TempQuestionId,
                        principalTable: "TempQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TempMCAnswer_TempQuestionId",
                table: "TempMCAnswer",
                column: "TempQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TempQuestion_TempQuizId",
                table: "TempQuestion",
                column: "TempQuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamQuestion_MultipleChoiceAnswer_MultipleChoiceAnswerId",
                table: "ExamQuestion",
                column: "MultipleChoiceAnswerId",
                principalTable: "MultipleChoiceAnswer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamQuestion_MultipleChoiceAnswer_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.DropTable(
                name: "TempMCAnswer");

            migrationBuilder.DropTable(
                name: "TempQuestion");

            migrationBuilder.DropTable(
                name: "TempQuiz");

            migrationBuilder.DropIndex(
                name: "IX_ExamQuestion_MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.DropColumn(
                name: "NoAttempt",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "ScoreEarned",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "MultipleChoiceAnswerId",
                table: "ExamQuestion");

            migrationBuilder.CreateIndex(
                name: "IX_MultipleChoiceAnswer_ExamQuestionId",
                table: "MultipleChoiceAnswer",
                column: "ExamQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_MultipleChoiceAnswer_ExamQuestion_ExamQuestionId",
                table: "MultipleChoiceAnswer",
                column: "ExamQuestionId",
                principalTable: "ExamQuestion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
