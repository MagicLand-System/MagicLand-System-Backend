using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    /// <inheritdoc />
    public partial class deletedNotUsingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Address_AddressId",
                table: "Class");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "StudentTransactions");

            migrationBuilder.DropTable(
                name: "ClassTransactions");

            migrationBuilder.DropTable(
                name: "ClassFeeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_PromotionTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Class_AddressId",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "Class");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AddressId",
                table: "Class",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Street = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Address_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualPrice = table.Column<double>(type: "float", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeeTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassFeeTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                        column: x => x.ClassFeeTransactionId,
                        principalTable: "ClassFeeTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClassTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTransactions_ClassTransactions_ClassTransactionId",
                        column: x => x.ClassTransactionId,
                        principalTable: "ClassTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PromotionTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions",
                column: "ClassFeeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Class_AddressId",
                table: "Class",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Address_UserId",
                table: "Address",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTransactions_ClassFeeTransactionId",
                table: "ClassTransactions",
                column: "ClassFeeTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTransactions_ClassTransactionId",
                table: "StudentTransactions",
                column: "ClassTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Address_AddressId",
                table: "Class",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTransactions_ClassFeeTransactions_ClassFeeTransactionId",
                table: "PromotionTransactions",
                column: "ClassFeeTransactionId",
                principalTable: "ClassFeeTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
