using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFullCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_Course_CourseId",
                table: "CourseSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashCard_Question_QuestionId",
                table: "FlashCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Material_CourseSyllabus_CourseSyllabusId",
                table: "Material");

            migrationBuilder.DropForeignKey(
                name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                table: "MutipleChoiceAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionDescription_Session_SessionId",
                table: "SessionDescription");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                table: "Topic");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course",
                column: "CourseCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_Course_CourseId",
                table: "CourseSyllabus",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashCard_Question_QuestionId",
                table: "FlashCard",
                column: "QuestionId",
                principalTable: "Question",
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
                name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                table: "MutipleChoiceAnswer",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionDescription_Session_SessionId",
                table: "SessionDescription",
                column: "SessionId",
                principalTable: "Session",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseSyllabus_Course_CourseId",
                table: "CourseSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus");

            migrationBuilder.DropForeignKey(
                name: "FK_FlashCard_Question_QuestionId",
                table: "FlashCard");

            migrationBuilder.DropForeignKey(
                name: "FK_Material_CourseSyllabus_CourseSyllabusId",
                table: "Material");

            migrationBuilder.DropForeignKey(
                name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                table: "MutipleChoiceAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_SessionDescription_Session_SessionId",
                table: "SessionDescription");

            migrationBuilder.DropForeignKey(
                name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                table: "Topic");

            migrationBuilder.AddForeignKey(
                name: "FK_Course_CourseCategory_CourseCategoryId",
                table: "Course",
                column: "CourseCategoryId",
                principalTable: "CourseCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseSyllabus_Course_CourseId",
                table: "CourseSyllabus",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExamSyllabus_CourseSyllabus_CourseSyllabusId",
                table: "ExamSyllabus",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FlashCard_Question_QuestionId",
                table: "FlashCard",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Material_CourseSyllabus_CourseSyllabusId",
                table: "Material",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MutipleChoiceAnswer_Question_QuestionId",
                table: "MutipleChoiceAnswer",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Topic_TopicId",
                table: "Session",
                column: "TopicId",
                principalTable: "Topic",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SessionDescription_Session_SessionId",
                table: "SessionDescription",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Topic_CourseSyllabus_CourseSyllabusId",
                table: "Topic",
                column: "CourseSyllabusId",
                principalTable: "CourseSyllabus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
