using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addNullNableAndUpdateSyllabusName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisite_Syllabus_CurrentSyllabusId",
                table: "CoursePrerequisite");

            migrationBuilder.DropForeignKey(
                name: "FK_Syllabus_CourseCategory_SyllabusCategoryId",
                table: "Syllabus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CoursePrerequisite",
                table: "CoursePrerequisite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseCategory",
                table: "CourseCategory");

            migrationBuilder.RenameTable(
                name: "CoursePrerequisite",
                newName: "SyllabusPrerequisite");

            migrationBuilder.RenameTable(
                name: "CourseCategory",
                newName: "SyllabusCategory");

            migrationBuilder.RenameIndex(
                name: "IX_CoursePrerequisite_CurrentSyllabusId",
                table: "SyllabusPrerequisite",
                newName: "IX_SyllabusPrerequisite_CurrentSyllabusId");

            migrationBuilder.AlterColumn<int>(
                name: "AttemptsAllowed",
                table: "QuestionPackage",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentSyllabusId",
                table: "SyllabusPrerequisite",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyllabusPrerequisite",
                table: "SyllabusPrerequisite",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SyllabusCategory",
                table: "SyllabusCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabus_SyllabusCategory_SyllabusCategoryId",
                table: "Syllabus",
                column: "SyllabusCategoryId",
                principalTable: "SyllabusCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SyllabusPrerequisite_Syllabus_CurrentSyllabusId",
                table: "SyllabusPrerequisite",
                column: "CurrentSyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Syllabus_SyllabusCategory_SyllabusCategoryId",
                table: "Syllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_SyllabusPrerequisite_Syllabus_CurrentSyllabusId",
                table: "SyllabusPrerequisite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SyllabusPrerequisite",
                table: "SyllabusPrerequisite");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SyllabusCategory",
                table: "SyllabusCategory");

            migrationBuilder.RenameTable(
                name: "SyllabusPrerequisite",
                newName: "CoursePrerequisite");

            migrationBuilder.RenameTable(
                name: "SyllabusCategory",
                newName: "CourseCategory");

            migrationBuilder.RenameIndex(
                name: "IX_SyllabusPrerequisite_CurrentSyllabusId",
                table: "CoursePrerequisite",
                newName: "IX_CoursePrerequisite_CurrentSyllabusId");

            migrationBuilder.AlterColumn<int>(
                name: "AttemptsAllowed",
                table: "QuestionPackage",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CurrentSyllabusId",
                table: "CoursePrerequisite",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CoursePrerequisite",
                table: "CoursePrerequisite",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseCategory",
                table: "CourseCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePrerequisite_Syllabus_CurrentSyllabusId",
                table: "CoursePrerequisite",
                column: "CurrentSyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Syllabus_CourseCategory_SyllabusCategoryId",
                table: "Syllabus",
                column: "SyllabusCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
