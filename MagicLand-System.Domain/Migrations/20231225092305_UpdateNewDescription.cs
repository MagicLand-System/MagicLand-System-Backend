using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "CourseDescription");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Topic",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            //migrationBuilder.AddColumn<string>(
            //    name: "MainDescription",
            //    table: "Course",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "SubDescriptionTitle",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubDescriptionTitle", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_SubDescriptionTitle_Course_CourseId",
            //            column: x => x.CourseId,
            //            principalTable: "Course",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "SubDescriptionContent",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        SubDescriptionTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_SubDescriptionContent", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_SubDescriptionContent_SubDescriptionTitle_SubDescriptionTitleId",
            //            column: x => x.SubDescriptionTitleId,
            //            principalTable: "SubDescriptionTitle",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_SubDescriptionContent_SubDescriptionTitleId",
            //    table: "SubDescriptionContent",
            //    column: "SubDescriptionTitleId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_SubDescriptionTitle_CourseId",
            //    table: "SubDescriptionTitle",
            //    column: "CourseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubDescriptionContent");

            migrationBuilder.DropTable(
                name: "SubDescriptionTitle");

            migrationBuilder.DropColumn(
                name: "MainDescription",
                table: "Course");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Topic",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "CourseSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CourseDescription",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDescription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseDescription_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseDescription_CourseId",
                table: "CourseDescription",
                column: "CourseId");
        }
    }
}
