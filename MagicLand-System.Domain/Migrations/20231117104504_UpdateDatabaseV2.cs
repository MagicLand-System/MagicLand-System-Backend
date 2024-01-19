using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDatabaseV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClasInstance_Session_SessionId",
                table: "ClasInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_ClasInstance_Student_StudentId",
                table: "ClasInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_ClasInstance_User_UserId",
                table: "ClasInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransaction_User_UserId",
                table: "StudentTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CartId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PersonalWalletId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransaction_UserId",
                table: "StudentTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClasInstance",
                table: "ClasInstance");

            migrationBuilder.DropIndex(
                name: "IX_ClasInstance_UserId",
                table: "ClasInstance");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StudentTransaction");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ClasInstance");

            migrationBuilder.RenameTable(
                name: "ClasInstance",
                newName: "ClassInstance");

            migrationBuilder.RenameColumn(
                name: "Banlance",
                table: "PersonalWallet",
                newName: "Balance");

            migrationBuilder.RenameIndex(
                name: "IX_ClasInstance_StudentId",
                table: "ClassInstance",
                newName: "IX_ClassInstance_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClasInstance_SessionId",
                table: "ClassInstance",
                newName: "IX_ClassInstance_SessionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonalWalletId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "CartId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Address",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassInstance",
                table: "ClassInstance",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_CartId",
                table: "User",
                column: "CartId",
                unique: true,
                filter: "[CartId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_PersonalWalletId",
                table: "User",
                column: "PersonalWalletId",
                unique: true,
                filter: "[PersonalWalletId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInstance_Session_SessionId",
                table: "ClassInstance",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassInstance_Student_StudentId",
                table: "ClassInstance",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User",
                column: "PersonalWalletId",
                principalTable: "PersonalWallet",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassInstance_Session_SessionId",
                table: "ClassInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassInstance_Student_StudentId",
                table: "ClassInstance");

            migrationBuilder.DropForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CartId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PersonalWalletId",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassInstance",
                table: "ClassInstance");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Address");

            migrationBuilder.RenameTable(
                name: "ClassInstance",
                newName: "ClasInstance");

            migrationBuilder.RenameColumn(
                name: "Balance",
                table: "PersonalWallet",
                newName: "Banlance");

            migrationBuilder.RenameIndex(
                name: "IX_ClassInstance_StudentId",
                table: "ClasInstance",
                newName: "IX_ClasInstance_StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassInstance_SessionId",
                table: "ClasInstance",
                newName: "IX_ClasInstance_SessionId");

            migrationBuilder.AlterColumn<Guid>(
                name: "PersonalWalletId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CartId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "StudentTransaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ClasInstance",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClasInstance",
                table: "ClasInstance",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_User_CartId",
                table: "User",
                column: "CartId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PersonalWalletId",
                table: "User",
                column: "PersonalWalletId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransaction_UserId",
                table: "StudentTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasInstance_UserId",
                table: "ClasInstance",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClasInstance_Session_SessionId",
                table: "ClasInstance",
                column: "SessionId",
                principalTable: "Session",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClasInstance_Student_StudentId",
                table: "ClasInstance",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClasInstance_User_UserId",
                table: "ClasInstance",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransaction_User_UserId",
                table: "StudentTransaction",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User",
                column: "PersonalWalletId",
                principalTable: "PersonalWallet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
