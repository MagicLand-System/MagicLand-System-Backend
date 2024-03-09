using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addNewUserField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentIdAccount",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoSession",
                table: "QuestionPackage",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                principalTable: "QuestionPackage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Session_QuestionPackage_QuestionPackageId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "StudentIdAccount",
                table: "User");

            migrationBuilder.DropColumn(
                name: "NoSession",
                table: "QuestionPackage");

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
