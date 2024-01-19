using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "User");

            migrationBuilder.DropColumn(
                name: "District",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "Street",
                table: "User",
                newName: "Address");

            migrationBuilder.AddColumn<string>(
                name: "Method",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemDescription",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "UpdateTime",
            //    table: "WalletTransaction",
            //    type: "datetime2",
            //    nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Student",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Method",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "SystemDescription",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "UpdateTime",
                table: "WalletTransaction");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Student");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "User",
                newName: "Street");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
