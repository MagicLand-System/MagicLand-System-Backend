using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateFieldNameSyllabus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Syllabus_CourseCategory_CourseCategoryId",
                table: "Syllabus");

            migrationBuilder.RenameColumn(
                name: "CourseCategoryId",
                table: "Syllabus",
                newName: "SyllabusCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Syllabus_CourseCategoryId",
                table: "Syllabus",
                newName: "IX_Syllabus_SyllabusCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabus_CourseCategory_SyllabusCategoryId",
                table: "Syllabus",
                column: "SyllabusCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Syllabus_CourseCategory_SyllabusCategoryId",
                table: "Syllabus");

            migrationBuilder.RenameColumn(
                name: "SyllabusCategoryId",
                table: "Syllabus",
                newName: "CourseCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Syllabus_SyllabusCategoryId",
                table: "Syllabus",
                newName: "IX_Syllabus_CourseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabus_CourseCategory_CourseCategoryId",
                table: "Syllabus",
                column: "CourseCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
