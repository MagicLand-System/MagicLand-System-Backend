using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class removePrequisiteEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SyllabusPrerequisite");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Course");

            migrationBuilder.AddColumn<DateTime>(
                name: "DoingDate",
                table: "TestResult",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "PrequisiteSyllabusId",
                table: "Syllabus",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoingDate",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "PrequisiteSyllabusId",
                table: "Syllabus");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Course",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SyllabusPrerequisite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentSyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrerequisiteSyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SyllabusPrerequisite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SyllabusPrerequisite_Syllabus_CurrentSyllabusId",
                        column: x => x.CurrentSyllabusId,
                        principalTable: "Syllabus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SyllabusPrerequisite_CurrentSyllabusId",
                table: "SyllabusPrerequisite",
                column: "CurrentSyllabusId");
        }
    }
}
