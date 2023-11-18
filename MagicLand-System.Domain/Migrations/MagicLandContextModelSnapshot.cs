﻿// <auto-generated />
using System;
using MagicLand_System.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    [DbContext(typeof(MagicLandContext))]
    partial class MagicLandContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("MagicLand_System.Domain.Models.Address", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("District")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Address", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Cart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Cart", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CartItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CartId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.ToTable("CartItem", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CourseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("EndTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<Guid>("LecturerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("LimitNumberStudent")
                        .HasColumnType("int");

                    b.Property<string>("Method")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<DateTime>("StartTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("CourseId");

                    b.HasIndex("LecturerId");

                    b.ToTable("Class", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassFeeTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double?>("ActualPrice")
                        .HasColumnType("float");

                    b.Property<DateTime>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<Guid>("ParentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("ClassFeeTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassInstance", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SessionId");

                    b.HasIndex("StudentId");

                    b.ToTable("ClassInstance", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassFeeTransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassFeeTransactionId");

                    b.HasIndex("ClassId");

                    b.ToTable("ClassTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Course", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("MaxYearOldsStudent")
                        .HasColumnType("int");

                    b.Property<int>("MinYearOldsStudent")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfSession")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Course", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CoursePrerequisite", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CurrentCourseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("PrerequisiteCourseId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CurrentCourseId");

                    b.ToTable("CoursePrerequisite", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PersonalWallet", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Balance")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("PersonalWallet", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Promotion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DiscountValue")
                        .HasColumnType("int");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Image")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UnitDiscount")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Promotion", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PromotionTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassFeeTransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserPromotionId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("UserPromotionId");

                    b.ToTable("PromotionTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Role", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Role", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Floor")
                        .HasColumnType("int");

                    b.Property<string>("LinkURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Room", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("int");

                    b.Property<Guid>("RoomId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("SlotId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("RoomId");

                    b.HasIndex("SlotId");

                    b.ToTable("Session", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Slot", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EndTime")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StartTime")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Slot", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Student", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ParentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("Student", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassTransactionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassTransactionId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("AddressId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CartId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("DateOfBirth")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PersonalWalletId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("AddressId")
                        .IsUnique()
                        .HasFilter("[AddressId] IS NOT NULL");

                    b.HasIndex("CartId")
                        .IsUnique()
                        .HasFilter("[CartId] IS NOT NULL");

                    b.HasIndex("PersonalWalletId")
                        .IsUnique()
                        .HasFilter("[PersonalWalletId] IS NOT NULL");

                    b.HasIndex("RoleId");

                    b.HasIndex(new[] { "Phone" }, "UX_User_Phone");

                    b.ToTable("User", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.UserPromotion", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int?>("AccumulateQuantity")
                        .HasColumnType("int");

                    b.Property<Guid>("PromotionId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("PromotionId");

                    b.ToTable("UserPromotion", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.WalletTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Money")
                        .HasColumnType("float");

                    b.Property<Guid>("PersonalWalletId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("PersonalWalletId");

                    b.ToTable("WalletTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CartItem", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Cart", "Cart")
                        .WithMany("Carts")
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cart");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Address", "Address")
                        .WithMany("Classes")
                        .HasForeignKey("AddressId");

                    b.HasOne("MagicLand_System.Domain.Models.Course", "Course")
                        .WithMany("Classes")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.User", "User")
                        .WithMany("Classes")
                        .HasForeignKey("LecturerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("Course");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassFeeTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.User", "User")
                        .WithMany("ClassFeeTransactions")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassInstance", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Session", "Session")
                        .WithMany("ClassInstances")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Student", "Student")
                        .WithMany("ClassInstances")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Session");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.ClassFeeTransaction", "ClassFeeTransaction")
                        .WithMany("ClassTransactions")
                        .HasForeignKey("ClassFeeTransactionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Class", "Class")
                        .WithMany("ClasssTransactions")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("ClassFeeTransaction");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CoursePrerequisite", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Course", "Course")
                        .WithMany("CoursePrerequisites")
                        .HasForeignKey("CurrentCourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PromotionTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.ClassFeeTransaction", "ClassFeeTransaction")
                        .WithMany("PromotionTransactions")
                        .HasForeignKey("UserPromotionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.UserPromotion", "UserPromotion")
                        .WithMany("PromotionTransactions")
                        .HasForeignKey("UserPromotionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClassFeeTransaction");

                    b.Navigation("UserPromotion");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Session", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Class", "Class")
                        .WithMany("Sessions")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Room", "Room")
                        .WithMany("Sessions")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Slot", "Slot")
                        .WithMany("Sessions")
                        .HasForeignKey("SlotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Room");

                    b.Navigation("Slot");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Student", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.User", "User")
                        .WithMany("Students")
                        .HasForeignKey("ParentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.ClassTransaction", "ClassTransaction")
                        .WithMany("StudentTransactions")
                        .HasForeignKey("ClassTransactionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Student", "Student")
                        .WithMany("StudentTransactions")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ClassTransaction");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Address", "Address")
                        .WithOne("User")
                        .HasForeignKey("MagicLand_System.Domain.Models.User", "AddressId");

                    b.HasOne("MagicLand_System.Domain.Models.Cart", "Cart")
                        .WithOne("User")
                        .HasForeignKey("MagicLand_System.Domain.Models.User", "CartId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("MagicLand_System.Domain.Models.PersonalWallet", "PersonalWallet")
                        .WithOne("User")
                        .HasForeignKey("MagicLand_System.Domain.Models.User", "PersonalWalletId");

                    b.HasOne("MagicLand_System.Domain.Models.Role", "Role")
                        .WithMany("Accounts")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_USER_ROLE");

                    b.Navigation("Address");

                    b.Navigation("Cart");

                    b.Navigation("PersonalWallet");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.UserPromotion", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Promotion", "Promotion")
                        .WithMany("UserPromotions")
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.User", "User")
                        .WithMany("UserPromotions")
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Promotion");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.WalletTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.PersonalWallet", "PersonalWallet")
                        .WithMany("WalletTransactions")
                        .HasForeignKey("PersonalWalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PersonalWallet");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Address", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("User")
                        .IsRequired();
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Cart", b =>
                {
                    b.Navigation("Carts");

                    b.Navigation("User")
                        .IsRequired();
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.Navigation("ClasssTransactions");

                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassFeeTransaction", b =>
                {
                    b.Navigation("ClassTransactions");

                    b.Navigation("PromotionTransactions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.ClassTransaction", b =>
                {
                    b.Navigation("StudentTransactions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Course", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("CoursePrerequisites");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PersonalWallet", b =>
                {
                    b.Navigation("User")
                        .IsRequired();

                    b.Navigation("WalletTransactions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Promotion", b =>
                {
                    b.Navigation("UserPromotions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Role", b =>
                {
                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Room", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Session", b =>
                {
                    b.Navigation("ClassInstances");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Slot", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Student", b =>
                {
                    b.Navigation("ClassInstances");

                    b.Navigation("StudentTransactions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
                    b.Navigation("ClassFeeTransactions");

                    b.Navigation("Classes");

                    b.Navigation("Students");

                    b.Navigation("UserPromotions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.UserPromotion", b =>
                {
                    b.Navigation("PromotionTransactions");
                });
#pragma warning restore 612, 618
        }
    }
}
