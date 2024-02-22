using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class bbb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SideFlashCard_FlashCard_FlashCardId",
                table: "SideFlashCard");

            migrationBuilder.AddForeignKey(
                name: "FK_SideFlashCard_FlashCard_FlashCardId",
                table: "SideFlashCard",
                column: "FlashCardId",
                principalTable: "FlashCard",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SideFlashCard_FlashCard_FlashCardId",
                table: "SideFlashCard");

            migrationBuilder.AddForeignKey(
                name: "FK_SideFlashCard_FlashCard_FlashCardId",
                table: "SideFlashCard",
                column: "FlashCardId",
                principalTable: "FlashCard",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
