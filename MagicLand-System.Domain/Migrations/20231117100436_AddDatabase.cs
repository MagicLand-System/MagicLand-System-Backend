using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "User");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "User",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "getutcdate()");

            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "User",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CartId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PersonalWalletId",
                table: "User",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeeTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    ActualPrice = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeeTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassFeeTransaction_User_ParentId",
                        column: x => x.ParentId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NumberOfSession = table.Column<int>(type: "int", nullable: false),
                    MinYearStudent = table.Column<int>(type: "int", nullable: false),
                    MaxYearStudent = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Course", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PersonalWallet",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Banlance = table.Column<double>(type: "float", nullable: false, defaultValue: 0.0),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalWallet", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promotion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitDiscount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountValue = table.Column<int>(type: "int", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promotion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkURL = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Room", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Slot",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EndTime = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slot", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Student",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Student", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Student_User_ParentId",
                        column: x => x.ParentId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItem_Cart_CartId",
                        column: x => x.CartId,
                        principalTable: "Cart",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Class",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LecturerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LimitNumberStudent = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Class", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Class_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Class_Course_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Class_User_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoursePrerequisite",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrerequisiteCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePrerequisite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoursePrerequisite_Course_CurrentCourseId",
                        column: x => x.CurrentCourseId,
                        principalTable: "Course",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WalletTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Money = table.Column<double>(type: "float", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonalWalletId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WalletTransaction_PersonalWallet_PersonalWalletId",
                        column: x => x.PersonalWalletId,
                        principalTable: "PersonalWallet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPromotion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccumulateQuantity = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPromotion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPromotion_Promotion_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPromotion_User_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassFeeTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTransaction_ClassFeeTransaction_ClassFeeTransactionId",
                        column: x => x.ClassFeeTransactionId,
                        principalTable: "ClassFeeTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassTransaction_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Session",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlotId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Session", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Session_Class_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Class",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Session_Room_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Session_Slot_SlotId",
                        column: x => x.SlotId,
                        principalTable: "Slot",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PromotionTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassFeeTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserPromotionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromotionTransaction_ClassFeeTransaction_UserPromotionId",
                        column: x => x.UserPromotionId,
                        principalTable: "ClassFeeTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PromotionTransaction_UserPromotion_UserPromotionId",
                        column: x => x.UserPromotionId,
                        principalTable: "UserPromotion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTransaction_ClassTransaction_ClassTransactionId",
                        column: x => x.ClassTransactionId,
                        principalTable: "ClassTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTransaction_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentTransaction_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClasInstance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StudentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasInstance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClasInstance_Session_SessionId",
                        column: x => x.SessionId,
                        principalTable: "Session",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClasInstance_Student_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Student",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClasInstance_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_AddressId",
                table: "User",
                column: "AddressId",
                unique: true,
                filter: "[AddressId] IS NOT NULL");

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
                name: "IX_CartItem_CartId",
                table: "CartItem",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasInstance_SessionId",
                table: "ClasInstance",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasInstance_StudentId",
                table: "ClasInstance",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasInstance_UserId",
                table: "ClasInstance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_AddressId",
                table: "Class",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_CourseId",
                table: "Class",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_LecturerId",
                table: "Class",
                column: "LecturerId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeeTransaction_ParentId",
                table: "ClassFeeTransaction",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransaction_ClassFeeTransactionId",
                table: "ClassTransaction",
                column: "ClassFeeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransaction_ClassId",
                table: "ClassTransaction",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisite_CurrentCourseId",
                table: "CoursePrerequisite",
                column: "CurrentCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTransaction_UserPromotionId",
                table: "PromotionTransaction",
                column: "UserPromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_ClassId",
                table: "Session",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_RoomId",
                table: "Session",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Session_SlotId",
                table: "Session",
                column: "SlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Student_ParentId",
                table: "Student",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransaction_ClassTransactionId",
                table: "StudentTransaction",
                column: "ClassTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransaction_StudentId",
                table: "StudentTransaction",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransaction_UserId",
                table: "StudentTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPromotion_PromotionId",
                table: "UserPromotion",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransaction_PersonalWalletId",
                table: "WalletTransaction",
                column: "PersonalWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Address_AddressId",
                table: "User",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Cart_CartId",
                table: "User",
                column: "CartId",
                principalTable: "Cart",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User",
                column: "PersonalWalletId",
                principalTable: "PersonalWallet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Address_AddressId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Cart_CartId",
                table: "User");

            migrationBuilder.DropForeignKey(
                name: "FK_User_PersonalWallet_PersonalWalletId",
                table: "User");

            migrationBuilder.DropTable(
                name: "CartItem");

            migrationBuilder.DropTable(
                name: "ClasInstance");

            migrationBuilder.DropTable(
                name: "CoursePrerequisite");

            migrationBuilder.DropTable(
                name: "PromotionTransaction");

            migrationBuilder.DropTable(
                name: "StudentTransaction");

            migrationBuilder.DropTable(
                name: "WalletTransaction");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Session");

            migrationBuilder.DropTable(
                name: "UserPromotion");

            migrationBuilder.DropTable(
                name: "ClassTransaction");

            migrationBuilder.DropTable(
                name: "Student");

            migrationBuilder.DropTable(
                name: "PersonalWallet");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "Slot");

            migrationBuilder.DropTable(
                name: "Promotion");

            migrationBuilder.DropTable(
                name: "ClassFeeTransaction");

            migrationBuilder.DropTable(
                name: "Class");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropIndex(
                name: "IX_User_AddressId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_CartId",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_PersonalWalletId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "User");

            migrationBuilder.DropColumn(
                name: "PersonalWalletId",
                table: "User");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateOfBirth",
                table: "User",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "getutcdate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "getutcdate()");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
