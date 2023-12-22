using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_Course_CourseId",
                table: "Session");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Session",
                newName: "TopicId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_CourseId",
                table: "Session",
                newName: "IX_Session_TopicId");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseSyllabusId",
                table: "Course",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseSyllabus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSyllabus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Topic",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    CourseSyllabusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topic", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                        column: x => x.CourseSyllabusId,
                        principalTable: "CourseSyllabus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_CourseId",
                table: "CourseSyllabus",
                column: "CourseId",
                unique: true,
                filter: "[CourseId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Topic_CourseSyllabusId",
                table: "Topic",
                column: "CourseSyllabusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session");

            migrationBuilder.DropTable(
                name: "Topic");

            migrationBuilder.DropTable(
                name: "CourseSyllabus");

            migrationBuilder.DropColumn(
                name: "CourseSyllabusId",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "TopicId",
                table: "Session",
                newName: "CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_TopicId",
                table: "Session",
                newName: "IX_Session_CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Course_CourseId",
                table: "Session",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
