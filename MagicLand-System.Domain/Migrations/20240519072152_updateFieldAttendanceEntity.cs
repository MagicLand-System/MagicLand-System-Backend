using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class updateFieldAttendanceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DoingDate",
                table: "TestResult");

            migrationBuilder.DropColumn(
                name: "IsValid",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "IsValid",
                table: "Evaluate",
                newName: "IsPublic");

            migrationBuilder.AddColumn<Guid>(
                name: "MakeUpFromScheduleId",
                table: "Evaluate",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MakeUpFromScheduleId",
                table: "Attendance",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MakeUpFromScheduleId",
                table: "Evaluate");

            migrationBuilder.DropColumn(
                name: "MakeUpFromScheduleId",
                table: "Attendance");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "Evaluate",
                newName: "IsValid");

            migrationBuilder.AddColumn<DateTime>(
                name: "DoingDate",
                table: "TestResult",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsValid",
                table: "Attendance",
                type: "bit",
                nullable: true);
        }
    }
}
