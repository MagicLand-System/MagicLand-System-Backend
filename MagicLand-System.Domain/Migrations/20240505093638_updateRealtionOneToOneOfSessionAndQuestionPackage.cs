using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateRealtionOneToOneOfSessionAndQuestionPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "QuestionPackageId",
                table: "Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "QuestionPackage",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionPackage_SessionId",
                table: "QuestionPackage",
                column: "SessionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionPackage_Session_SessionId",
                table: "QuestionPackage",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionPackage_Session_SessionId",
                table: "QuestionPackage");

            migrationBuilder.DropIndex(
                name: "IX_QuestionPackage_SessionId",
                table: "QuestionPackage");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SessionId",
                table: "QuestionPackage",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                unique: true,
                filter: "[QuestionPackageId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
