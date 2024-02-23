using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateAllowNullSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session");

            migrationBuilder.AlterColumn<Guid>(
                name: "QuestionPackageId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Session_QuestionPackageId",
                table: "Session",
                column: "QuestionPackageId",
                unique: true);
        }
    }
}
