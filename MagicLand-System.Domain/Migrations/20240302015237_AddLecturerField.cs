using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddLecturerField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LecturerFieldId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LecturerField",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerField", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_LecturerFieldId",
                table: "User",
                column: "LecturerFieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_LecturerField_LecturerFieldId",
                table: "User",
                column: "LecturerFieldId",
                principalTable: "LecturerField",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_LecturerField_LecturerFieldId",
                table: "User");

            migrationBuilder.DropTable(
                name: "LecturerField");

            migrationBuilder.DropIndex(
                name: "IX_User_LecturerFieldId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "LecturerFieldId",
                table: "User");
        }
    }
}
