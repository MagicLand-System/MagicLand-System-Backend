using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class FixingEntityNotFinish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassFeeTransaction_User_ParentId",
                table: "ClassFeeTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTransaction_ClassFeeTransaction_ClassFeeTransactionId",
                table: "ClassTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTransaction_Class_ClassId",
                table: "ClassTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTransaction_ClassFeeTransaction_UserPromotionId",
                table: "PromotionTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTransaction_UserPromotion_UserPromotionId",
                table: "PromotionTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Class_ClassId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Room_RoomId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Slot_SlotId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransaction_ClassTransaction_ClassTransactionId",
                table: "StudentTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransaction_Student_StudentId",
                table: "StudentTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Address_AddressId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotion_Promotion_PromotionId",
                table: "UserPromotion");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotion_User_UserId",
                table: "UserPromotion");

            migrationBuilder.DropTable(
                name: "CartItemRelation");

            migrationBuilder.DropTable(
                name: "ClassInstance");

            migrationBuilder.DropIndex(
                name: "IX_User_AddressId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Session_ClassId",
                table: "Session");

            migrationBuilder.DropIndex(
                name: "IX_Session_RoomId",
                table: "Session");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPromotion",
                table: "UserPromotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentTransaction",
                table: "StudentTransaction");

            migrationBuilder.DropIndex(
                name: "IX_StudentTransaction_StudentId",
                table: "StudentTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionTransaction",
                table: "PromotionTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassTransaction",
                table: "ClassTransaction");

            migrationBuilder.DropIndex(
                name: "IX_ClassTransaction_ClassId",
                table: "ClassTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassFeeTransaction",
                table: "ClassFeeTransaction");

            migrationBuilder.DropIndex(
                name: "IX_ClassFeeTransaction_ParentId",
                table: "ClassFeeTransaction");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "StudentId",
                table: "StudentTransaction");

            migrationBuilder.DropColumn(
                name: "ClassId",
                table: "ClassTransaction");

            migrationBuilder.DropColumn(
                name: "ParentId",
                table: "ClassFeeTransaction");

            migrationBuilder.RenameTable(
                name: "UserPromotion",
                newName: "UserPromotions");

            migrationBuilder.RenameTable(
                name: "StudentTransaction",
                newName: "StudentTransactions");

            migrationBuilder.RenameTable(
                name: "PromotionTransaction",
                newName: "PromotionTransactions");

            migrationBuilder.RenameTable(
                name: "Promotion",
                newName: "Promotions");

            migrationBuilder.RenameTable(
                name: "ClassTransaction",
                newName: "ClassTransactions");

            migrationBuilder.RenameTable(
                name: "ClassFeeTransaction",
                newName: "ClassFeeTransactions");

            migrationBuilder.RenameColumn(
                name: "SlotId",
                table: "Session",
                newName: "CourseId");

            migrationBuilder.RenameColumn(
                name: "DayOfWeek",
                table: "Session",
                newName: "NoSession");

            migrationBuilder.RenameIndex(
                name: "IX_Session_SlotId",
                table: "Session",
                newName: "IX_Session_CourseId");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Class",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Class",
                newName: "EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_UserPromotion_UserId",
                table: "UserPromotions",
                newName: "IX_UserPromotions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPromotion_PromotionId",
                table: "UserPromotions",
                newName: "IX_UserPromotions_PromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentTransaction_ClassTransactionId",
                table: "StudentTransactions",
                newName: "IX_StudentTransactions_ClassTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionTransaction_UserPromotionId",
                table: "PromotionTransactions",
                newName: "IX_PromotionTransactions_UserPromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassTransaction_ClassFeeTransactionId",
                table: "ClassTransactions",
                newName: "IX_ClassTransactions_ClassFeeTransactionId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "User",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Student",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Session",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "Room",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Role",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Course",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Course",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Class",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Class",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Class",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Class",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClassCode",
                table: "Class",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeastNumberStudent",
                table: "Class",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "CartItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Address",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "ClassFeeTransactions",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getutcdate()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPromotions",
                table: "UserPromotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentTransactions",
                table: "StudentTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionTransactions",
                table: "PromotionTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassTransactions",
                table: "ClassTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassFeeTransactions",
                table: "ClassFeeTransactions",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Schedule",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schedule_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedule_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Schedule_Slot_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentClass",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentClass", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentClass_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentClass_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentInCart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentInCart", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentInCart_CartItem_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_UserId",
                table: "Address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions",
                column: "ClassFeeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_ClassId",
                table: "Schedule",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_RoomId",
                table: "Schedule",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedule_SlotId",
                table: "Schedule",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClass_ClassId",
                table: "StudentClass",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentClass_StudentId",
                table: "StudentClass",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentInCart_CartItemId",
                table: "StudentInCart",
                column: "CartItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Address_User_UserId",
                table: "Address",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "ClassTransactions",
                column: "ClassFeeTransactionId",
                principalTable: "ClassFeeTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions",
                column: "ClassFeeTransactionId",
                principalTable: "ClassFeeTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTransactions_UserPromotions_UserPromotionId",
                table: "PromotionTransactions",
                column: "UserPromotionId",
                principalTable: "UserPromotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Course_CourseId",
                table: "Session",
                column: "CourseId",
                principalTable: "Course",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransactions_ClassTransactions_ClassTransactionId",
                table: "StudentTransactions",
                column: "ClassTransactionId",
                principalTable: "ClassTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotions_Promotions_PromotionId",
                table: "UserPromotions",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotions_User_UserId",
                table: "UserPromotions",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Address_User_UserId",
                table: "Address");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "ClassTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTransactions_UserPromotions_UserPromotionId",
                table: "PromotionTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Session_Course_CourseId",
                table: "Session");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentTransactions_ClassTransactions_ClassTransactionId",
                table: "StudentTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotions_Promotions_PromotionId",
                table: "UserPromotions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPromotions_User_UserId",
                table: "UserPromotions");

            migrationBuilder.DropTable(
                name: "Schedule");

            migrationBuilder.DropTable(
                name: "StudentClass");

            migrationBuilder.DropTable(
                name: "StudentInCart");

            migrationBuilder.DropIndex(
                name: "IX_Address_UserId",
                table: "Address");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserPromotions",
                table: "UserPromotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudentTransactions",
                table: "StudentTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionTransactions",
                table: "PromotionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PromotionTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassTransactions",
                table: "ClassTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ClassFeeTransactions",
                table: "ClassFeeTransactions");

            migrationBuilder.DropColumn(
                name: "City",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "User");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Student");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Session");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "Room");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Role");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "ClassCode",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "LeastNumberStudent",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "CartItem");

            migrationBuilder.RenameTable(
                name: "UserPromotions",
                newName: "UserPromotion");

            migrationBuilder.RenameTable(
                name: "StudentTransactions",
                newName: "StudentTransaction");

            migrationBuilder.RenameTable(
                name: "PromotionTransactions",
                newName: "PromotionTransaction");

            migrationBuilder.RenameTable(
                name: "Promotions",
                newName: "Promotion");

            migrationBuilder.RenameTable(
                name: "ClassTransactions",
                newName: "ClassTransaction");

            migrationBuilder.RenameTable(
                name: "ClassFeeTransactions",
                newName: "ClassFeeTransaction");

            migrationBuilder.RenameColumn(
                name: "NoSession",
                table: "Session",
                newName: "DayOfWeek");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "Session",
                newName: "SlotId");

            migrationBuilder.RenameIndex(
                name: "IX_Session_CourseId",
                table: "Session",
                newName: "IX_Session_SlotId");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "Class",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "Class",
                newName: "EndTime");

            migrationBuilder.RenameIndex(
                name: "IX_UserPromotions_UserId",
                table: "UserPromotion",
                newName: "IX_UserPromotion_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_UserPromotions_PromotionId",
                table: "UserPromotion",
                newName: "IX_UserPromotion_PromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_StudentTransactions_ClassTransactionId",
                table: "StudentTransaction",
                newName: "IX_StudentTransaction_ClassTransactionId");

            migrationBuilder.RenameIndex(
                name: "IX_PromotionTransactions_UserPromotionId",
                table: "PromotionTransaction",
                newName: "IX_PromotionTransaction_UserPromotionId");

            migrationBuilder.RenameIndex(
                name: "IX_ClassTransactions_ClassFeeTransactionId",
                table: "ClassTransaction",
                newName: "IX_ClassTransaction_ClassFeeTransactionId");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "WalletTransaction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "User",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "User",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Student",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Session",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Session",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Course",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Class",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Class",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Method",
                table: "Class",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Address",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "StudentId",
                table: "StudentTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "ClassId",
                table: "ClassTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCreated",
                table: "ClassFeeTransaction",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                table: "ClassFeeTransaction",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserPromotion",
                table: "UserPromotion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudentTransaction",
                table: "StudentTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionTransaction",
                table: "PromotionTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotion",
                table: "Promotion",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassTransaction",
                table: "ClassTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ClassFeeTransaction",
                table: "ClassFeeTransaction",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CartItemRelation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItemRelation_CartItem_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassInstance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassInstance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassInstance_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassInstance_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_AddressId",
                table: "User",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ClassId",
                table: "Session",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_RoomId",
                table: "Session",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransaction_StudentId",
                table: "StudentTransaction",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransaction_ClassId",
                table: "ClassTransaction",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeeTransaction_ParentId",
                table: "ClassFeeTransaction",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItemRelation_CartItemId",
                table: "CartItemRelation",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInstance_SessionId",
                table: "ClassInstance",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassInstance_StudentId",
                table: "ClassInstance",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassFeeTransaction_User_ParentId",
                table: "ClassFeeTransaction",
                column: "ParentId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTransaction_ClassFeeTransaction_ClassFeeTransactionId",
                table: "ClassTransaction",
                column: "ClassFeeTransactionId",
                principalTable: "ClassFeeTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassTransaction_Class_ClassId",
                table: "ClassTransaction",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTransaction_ClassFeeTransaction_UserPromotionId",
                table: "PromotionTransaction",
                column: "UserPromotionId",
                principalTable: "ClassFeeTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTransaction_UserPromotion_UserPromotionId",
                table: "PromotionTransaction",
                column: "UserPromotionId",
                principalTable: "UserPromotion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Class_ClassId",
                table: "Session",
                column: "ClassId",
                principalTable: "Class",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Room_RoomId",
                table: "Session",
                column: "RoomId",
                principalTable: "Room",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Session_Slot_SlotId",
                table: "Session",
                column: "SlotId",
                principalTable: "Slot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransaction_ClassTransaction_ClassTransactionId",
                table: "StudentTransaction",
                column: "ClassTransactionId",
                principalTable: "ClassTransaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentTransaction_Student_StudentId",
                table: "StudentTransaction",
                column: "StudentId",
                principalTable: "Student",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Address_AddressId",
                table: "User",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotion_Promotion_PromotionId",
                table: "UserPromotion",
                column: "PromotionId",
                principalTable: "Promotion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPromotion_User_UserId",
                table: "UserPromotion",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
