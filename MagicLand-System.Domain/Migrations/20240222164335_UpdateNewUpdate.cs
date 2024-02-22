using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course");

            migrationBuilder.DropIndex(
                name: "IX_Course_CourseCategoryId",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "CourseCategoryId",
                table: "Course");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseCategoryId",
                table: "CourseSyllabus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_CourseCategoryId",
                table: "CourseSyllabus",
                column: "CourseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_CourseCategory_CourseCategoryId",
                table: "CourseSyllabus",
                column: "CourseCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_CourseCategory_CourseCategoryId",
                table: "CourseSyllabus");

            migrationBuilder.DropIndex(
                name: "IX_CourseSyllabus_CourseCategoryId",
                table: "CourseSyllabus");

            migrationBuilder.DropColumn(
                name: "CourseCategoryId",
                table: "CourseSyllabus");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseCategoryId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Course_CourseCategoryId",
                table: "Course",
                column: "CourseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course",
                column: "CourseCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
