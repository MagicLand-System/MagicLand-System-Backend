using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class FixFieldAndRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisite_Course_CurrentCourseId",
                table: "CoursePrerequisite");

            migrationBuilder.RenameColumn(
                name: "CurrentCourseId",
                table: "CoursePrerequisite",
                newName: "CurrentSyllabusId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePrerequisite_CurrentCourseId",
                table: "CoursePrerequisite",
                newName: "IX_CoursePrerequisite_CurrentSyllabusId");

            migrationBuilder.AddColumn<int>(
                name: "DeadlineTime",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Score",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContentName",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "CoursePrerequisite",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisite_CourseId",
                table: "CoursePrerequisite",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePrerequisite_Course_CourseId",
                table: "CoursePrerequisite",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePrerequisite_Syllabus_CurrentSyllabusId",
                table: "CoursePrerequisite",
                column: "CurrentSyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisite_Course_CourseId",
                table: "CoursePrerequisite");

            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisite_Syllabus_CurrentSyllabusId",
                table: "CoursePrerequisite");

            migrationBuilder.DropIndex(
                name: "IX_CoursePrerequisite_CourseId",
                table: "CoursePrerequisite");

            migrationBuilder.DropColumn(
                name: "DeadlineTime",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "Score",
                table: "QuestionPackage");

            migrationBuilder.DropColumn(
                name: "ContentName",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "ExamSyllabus");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CoursePrerequisite");

            migrationBuilder.RenameColumn(
                name: "CurrentSyllabusId",
                table: "CoursePrerequisite",
                newName: "CurrentCourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePrerequisite_CurrentSyllabusId",
                table: "CoursePrerequisite",
                newName: "IX_CoursePrerequisite_CurrentCourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePrerequisite_Course_CurrentCourseId",
                table: "CoursePrerequisite",
                column: "CurrentCourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
