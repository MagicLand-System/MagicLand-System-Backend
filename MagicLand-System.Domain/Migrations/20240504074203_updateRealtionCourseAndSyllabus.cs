using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateRealtionCourseAndSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Syllabus_Course_CourseId",
                table: "Syllabus");

            migrationBuilder.DropIndex(
                name: "IX_Syllabus_CourseId",
                table: "Syllabus");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Syllabus");

            migrationBuilder.AlterColumn<Guid>(
                name: "SyllabusId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Course_SyllabusId",
                table: "Course",
                column: "SyllabusId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Course_Syllabus_SyllabusId",
                table: "Course",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_Syllabus_SyllabusId",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Course_SyllabusId",
                table: "Course");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseId",
                table: "Syllabus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SyllabusId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_CourseId",
                table: "Syllabus",
                column: "CourseId",
                unique: true,
                filter: "[CourseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabus_Course_CourseId",
                table: "Syllabus",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
