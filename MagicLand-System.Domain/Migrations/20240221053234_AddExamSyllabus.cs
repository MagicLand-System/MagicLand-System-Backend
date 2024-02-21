using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddExamSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PointGrade",
                table: "CourseSyllabus",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Target",
                table: "CourseSyllabus",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExamSyllabus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Part = table.Column<int>(type: "int", nullable: false),
                    CompleteionCriteria = table.Column<double>(type: "float", nullable: false),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NoQuestion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KnowledgeAndSkill = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GradingGuide = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CourseSyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamSyllabus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                        column: x => x.CourseSyllabusId,
                        principalTable: "CourseSyllabus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamSyllabus_CourseSyllabusId",
                table: "ExamSyllabus",
                column: "CourseSyllabusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "PointGrade",
                table: "CourseSyllabus");

            migrationBuilder.DropColumn(
                name: "Target",
                table: "CourseSyllabus");
        }
    }
}
