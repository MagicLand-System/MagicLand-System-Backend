using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addEntitiesForSaveQuiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "MutipleChoiceAnswer");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "SessionDescription",
                type: "int",
                nullable: false,
                defaultValue: 0);

            //migrationBuilder.CreateTable(
            //    name: "MultipleChoice",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Score = table.Column<double>(type: "float", nullable: false),
            //        QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_MultipleChoice", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_MultipleChoice_Question_QuestionId",
            //            column: x => x.QuestionId,
            //            principalTable: "Question",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateTable(
                name: "TestResult",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamCategory = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMark = table.Column<int>(type: "int", nullable: false),
                    CorrectMark = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<double>(type: "float", nullable: false),
                    ExamStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResult", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResult_StudentClass_StudentClassId",
                        column: x => x.StudentClassId,
                        principalTable: "StudentClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamQuestion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false),
                    TestResultId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamQuestion_TestResult_TestResultId",
                        column: x => x.TestResultId,
                        principalTable: "TestResult",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FlashCardAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeftCardAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeftCardAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeftCardAnswerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightCardAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RightCardAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RightCardAnswerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectRightCardAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectRightCardAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectRightCardAnswerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashCardAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashCardAnswer_ExamQuestion_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MultipleChoiceAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AnswerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectAnswerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CorrectAnswerImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExamQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MultipleChoiceAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MultipleChoiceAnswer_ExamQuestion_ExamQuestionId",
                        column: x => x.ExamQuestionId,
                        principalTable: "ExamQuestion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamQuestion_TestResultId",
                table: "ExamQuestion",
                column: "TestResultId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashCardAnswer_ExamQuestionId",
                table: "FlashCardAnswer",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MultipleChoice_QuestionId",
                table: "MultipleChoice",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MultipleChoiceAnswer_ExamQuestionId",
                table: "MultipleChoiceAnswer",
                column: "ExamQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResult_StudentClassId",
                table: "TestResult",
                column: "StudentClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlashCardAnswer");

            migrationBuilder.DropTable(
                name: "MultipleChoice");

            migrationBuilder.DropTable(
                name: "MultipleChoiceAnswer");

            migrationBuilder.DropTable(
                name: "ExamQuestion");

            migrationBuilder.DropTable(
                name: "TestResult");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "SessionDescription");

            migrationBuilder.CreateTable(
                name: "MutipleChoiceAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutipleChoiceAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MutipleChoiceAnswer_QuestionId",
                table: "MutipleChoiceAnswer",
                column: "QuestionId");
        }
    }
}
