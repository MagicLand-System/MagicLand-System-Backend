using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addRatingScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Priority",
                table: "Notification",
                newName: "Type");

            migrationBuilder.AddColumn<double>(
                name: "TotalRate",
                table: "Course",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRate",
                table: "Course");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Notification",
                newName: "Priority");
        }
    }
}
