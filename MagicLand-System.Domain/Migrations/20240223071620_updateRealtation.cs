using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateRealtation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_Material_CourseSyllabus_CourseSyllabusId",
                table: "Material");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                table: "Topic");

            migrationBuilder.DropTable(
                name: "CourseSyllabus");

            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Material_CourseSyllabusId",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "CourseSyllabusId",
                table: "Material");

            migrationBuilder.RenameColumn(
                name: "CourseSyllabusId",
                table: "Topic",
                newName: "SyllabusId");

            migrationBuilder.RenameIndex(
                name: "IX_Topic_CourseSyllabusId",
                table: "Topic",
                newName: "IX_Topic_SyllabusId");

            migrationBuilder.RenameColumn(
                name: "CourseSyllabusId",
                table: "ExamSyllabus",
                newName: "SyllabusId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSyllabus_CourseSyllabusId",
                table: "ExamSyllabus",
                newName: "IX_ExamSyllabus_SyllabusId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SideFlashCard",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "Detail",
                table: "SessionDescription",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SessionDescription",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SessionDescription",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QuestionPackage",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Question",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Question",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MutipleChoiceAnswer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MutipleChoiceAnswer",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Material",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Material",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<Guid>(
                name: "SyllabusId",
                table: "Material",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "FlashCard",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ExamSyllabus",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldDefaultValueSql: "NEWID()");

            migrationBuilder.AddColumn<string>(
                name: "SubjectName",
                table: "Course",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Syllabus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StudentTasks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoringScale = table.Column<double>(type: "float", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimePerSession = table.Column<int>(type: "int", nullable: false),
                    MinAvgMarkToPass = table.Column<double>(type: "float", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SyllabusLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CourseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Syllabus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Syllabus_CourseCategory_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Syllabus_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Material_SyllabusId",
                table: "Material",
                column: "SyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_CourseCategoryId",
                table: "Syllabus",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Syllabus_CourseId",
                table: "Syllabus",
                column: "CourseId",
                unique: true,
                filter: "[CourseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSyllabus_Syllabus_SyllabusId",
                table: "ExamSyllabus",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Material_Syllabus_SyllabusId",
                table: "Material",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_Syllabus_SyllabusId",
                table: "Topic",
                column: "SyllabusId",
                principalTable: "Syllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExamSyllabus_Syllabus_SyllabusId",
                table: "ExamSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_Material_Syllabus_SyllabusId",
                table: "Material");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_Syllabus_SyllabusId",
                table: "Topic");

            migrationBuilder.DropTable(
                name: "Syllabus");

            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Material_SyllabusId",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "SyllabusId",
                table: "Material");

            migrationBuilder.DropColumn(
                name: "SubjectName",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "SyllabusId",
                table: "Topic",
                newName: "CourseSyllabusId");

            migrationBuilder.RenameIndex(
                name: "IX_Topic_SyllabusId",
                table: "Topic",
                newName: "IX_Topic_CourseSyllabusId");

            migrationBuilder.RenameColumn(
                name: "SyllabusId",
                table: "ExamSyllabus",
                newName: "CourseSyllabusId");

            migrationBuilder.RenameIndex(
                name: "IX_ExamSyllabus_SyllabusId",
                table: "ExamSyllabus",
                newName: "IX_ExamSyllabus_CourseSyllabusId");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SideFlashCard",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Detail",
                table: "SessionDescription",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "SessionDescription",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SessionDescription",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "QuestionPackage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QuestionPackage",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Question",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Question",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "MutipleChoiceAnswer",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "MutipleChoiceAnswer",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "URL",
                table: "Material",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Material",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "CourseSyllabusId",
                table: "Material",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "FlashCard",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionType",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Duration",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "ExamSyllabus",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "ExamSyllabus",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()",
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateTable(
                name: "CourseSyllabus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MinAvgMarkToPass = table.Column<double>(type: "float", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScoringScale = table.Column<double>(type: "float", nullable: false),
                    StudentTasks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SyllabusLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TimePerSession = table.Column<int>(type: "int", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseSyllabus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_CourseCategory_CourseCategoryId",
                        column: x => x.CourseCategoryId,
                        principalTable: "CourseCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseSyllabus_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                unique: true,
                filter: "[QuestionPackageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Material_CourseSyllabusId",
                table: "Material",
                column: "CourseSyllabusId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_CourseCategoryId",
                table: "CourseSyllabus",
                column: "CourseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSyllabus_CourseId",
                table: "CourseSyllabus",
                column: "CourseId",
                unique: true,
                filter: "[CourseId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Material_CourseSyllabus_CourseSyllabusId",
                table: "Material",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                table: "Topic",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
