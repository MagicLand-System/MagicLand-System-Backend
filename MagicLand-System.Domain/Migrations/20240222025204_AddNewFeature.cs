using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Session");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "SessionDescription",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "SessionDescription");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
