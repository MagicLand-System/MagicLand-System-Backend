using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldFromCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisite_Course_CourseId",
                table: "CoursePrerequisite");

            migrationBuilder.DropIndex(
                name: "IX_CoursePrerequisite_CourseId",
                table: "CoursePrerequisite");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "CoursePrerequisite");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
