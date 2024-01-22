using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateWalletTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "WalletTransaction");

            migrationBuilder.RenameColumn(
                name: "SystemDescription",
                table: "WalletTransaction",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "CreatedTime",
                table: "WalletTransaction",
                newName: "CreateTime");

            migrationBuilder.AddColumn<string>(
                name: "CreateBy",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Discount",
                table: "WalletTransaction",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Signature",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateBy",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "Signature",
                table: "WalletTransaction");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "WalletTransaction",
                newName: "SystemDescription");

            migrationBuilder.RenameColumn(
                name: "CreateTime",
                table: "WalletTransaction",
                newName: "CreatedTime");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "WalletTransaction",
                type: "bit",
                nullable: true);
        }
    }
}
