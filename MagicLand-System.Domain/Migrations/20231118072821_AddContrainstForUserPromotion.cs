using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddContrainstForUserPromotion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotion_User_PromotionId",
                table: "UserPromotion");

            migrationBuilder.CreateIndex(
                name: "IX_UserPromotion_UserId",
                table: "UserPromotion",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotion_User_UserId",
                table: "UserPromotion",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotion_User_UserId",
                table: "UserPromotion");

            migrationBuilder.DropIndex(
                name: "IX_UserPromotion_UserId",
                table: "UserPromotion");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotion_User_PromotionId",
                table: "UserPromotion",
                column: "PromotionId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
