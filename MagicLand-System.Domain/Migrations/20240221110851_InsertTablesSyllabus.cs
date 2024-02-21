using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InsertTablesSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GradingGuide",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "KnowledgeAndSkill",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "NoQuestion",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "Part",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ExamSyllabus");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Weight",
                table: "ExamSyllabus",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "QuestionPackage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionPackage", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionPacketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Question_QuestionPackage_QuestionPackageId",
                        column: x => x.QuestionPackageId,
                        principalTable: "QuestionPackage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlashCard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Score = table.Column<double>(type: "float", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlashCard_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MutipleChoiceAnswer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Img = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MutipleChoiceAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SideFlashCard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Side = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlashCardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SideFlashCard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SideFlashCard_FlashCard_FlashCardId",
                        column: x => x.FlashCardId,
                        principalTable: "FlashCard",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                unique: true,
                filter: "[QuestionPackageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FlashCard_QuestionId",
                table: "FlashCard",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_MutipleChoiceAnswer_QuestionId",
                table: "MutipleChoiceAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_QuestionPackageId",
                table: "Question",
                column: "QuestionPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_SideFlashCard_FlashCardId",
                table: "SideFlashCard",
                column: "FlashCardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropTable(
                name: "MutipleChoiceAnswer");

            migrationBuilder.DropTable(
                name: "SideFlashCard");

            migrationBuilder.DropTable(
                name: "FlashCard");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "QuestionPackage");

            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "QuestionPackageId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "ExamSyllabus");

            migrationBuilder.AddColumn<string>(
                name: "GradingGuide",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "KnowledgeAndSkill",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NoQuestion",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Part",
                table: "ExamSyllabus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
