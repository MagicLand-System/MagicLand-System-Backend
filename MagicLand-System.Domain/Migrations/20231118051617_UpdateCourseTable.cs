using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCourseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinYearStudent",
                table: "Course",
                newName: "MinYearOldsStudent");

            migrationBuilder.RenameColumn(
                name: "MaxYearStudent",
                table: "Course",
                newName: "MaxYearOldsStudent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinYearOldsStudent",
                table: "Course",
                newName: "MinYearStudent");

            migrationBuilder.RenameColumn(
                name: "MaxYearOldsStudent",
                table: "Course",
                newName: "MaxYearStudent");
        }
    }
}
