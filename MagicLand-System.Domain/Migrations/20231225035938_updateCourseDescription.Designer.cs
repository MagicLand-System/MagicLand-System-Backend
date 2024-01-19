﻿// <auto-generated />
using System;
using MagicLand_System.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace MagicLand_System.Domain.Migrations
{
    [DbContext(typeof(MagicLandContext))]
    [Migration("20231225035938_updateCourseDescription")]
    partial class updateCourseDescription
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.13")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

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

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CartId");

                    b.ToTable("CartItem", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClassCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("CourseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("District")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("LeastNumberStudent")
                        .HasColumnType("int");

                    b.Property<Guid>("LecturerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("LimitNumberStudent")
                        .HasColumnType("int");

                    b.Property<string>("Method")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("StartDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Street")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Video")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.HasIndex("LecturerId");

                    b.ToTable("Class", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Course", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CourseCategoryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CourseSyllabusId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Image")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MainDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("MaxYearOldsStudent")
                        .HasColumnType("int");

                    b.Property<int?>("MinYearOldsStudent")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NumberOfSession")
                        .HasColumnType("int");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CourseCategoryId");

                    b.ToTable("Course", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CourseCategory", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("CourseCategory", (string)null);
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

            modelBuilder.Entity("MagicLand_System.Domain.Models.CourseSyllabus", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("CourseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("UpdateTime")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("CourseId")
                        .IsUnique()
                        .HasFilter("[CourseId] IS NOT NULL");

                    b.ToTable("CourseSyllabus", (string)null);
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

                    b.ToTable("Promotions");
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

                    b.ToTable("PromotionTransactions");
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

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Role", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Room", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Capacity")
                        .HasColumnType("int");

                    b.Property<int?>("Floor")
                        .HasColumnType("int");

                    b.Property<string>("LinkURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Room", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Schedule", b =>
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

                    b.ToTable("Schedule", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("NoSession")
                        .HasColumnType("int");

                    b.Property<Guid>("TopicId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("TopicId");

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

                    b.Property<string>("AvatarImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ParentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ParentId");

                    b.ToTable("Student", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentClass", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("ClassId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ClassId");

                    b.HasIndex("StudentId");

                    b.ToTable("StudentClass", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentInCart", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CartItemId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("StudentId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CartItemId");

                    b.ToTable("StudentInCart", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.SubDescriptionContent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SubDescriptionTitleId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("SubDescriptionTitleId");

                    b.ToTable("SubDescriptionContent", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.SubDescriptionTitle", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CourseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CourseId");

                    b.ToTable("SubDescriptionTitle", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Topic", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CourseSyllabusId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OrderNumber")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CourseSyllabusId");

                    b.ToTable("Topic", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("AvatarImage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("CartId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("City")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("DateOfBirth")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<string>("District")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FullName")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Gender")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("PersonalWalletId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Phone")
                        .HasColumnType("nvarchar(450)");

                    b.Property<Guid>("RoleId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Street")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

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

                    b.HasIndex("UserId");

                    b.ToTable("UserPromotions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.WalletTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Money")
                        .HasColumnType("float");

                    b.Property<Guid>("PersonalWalletId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("PersonalWalletId");

                    b.ToTable("WalletTransaction", (string)null);
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CartItem", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Cart", "Cart")
                        .WithMany("CartItems")
                        .HasForeignKey("CartId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cart");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Course", "Course")
                        .WithMany("Classes")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.User", "Lecture")
                        .WithMany("Classes")
                        .HasForeignKey("LecturerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");

                    b.Navigation("Lecture");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Course", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.CourseCategory", "CourseCategory")
                        .WithMany("Courses")
                        .HasForeignKey("CourseCategoryId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CourseCategory");
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

            modelBuilder.Entity("MagicLand_System.Domain.Models.CourseSyllabus", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Course", "Course")
                        .WithOne("CourseSyllabus")
                        .HasForeignKey("MagicLand_System.Domain.Models.CourseSyllabus", "CourseId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Course");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PromotionTransaction", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.UserPromotion", "UserPromotion")
                        .WithMany("PromotionTransactions")
                        .HasForeignKey("UserPromotionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserPromotion");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Schedule", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Class", "Class")
                        .WithMany("Schedules")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Room", "Room")
                        .WithMany("Schedules")
                        .HasForeignKey("RoomId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Slot", "Slot")
                        .WithMany("Schedules")
                        .HasForeignKey("SlotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Room");

                    b.Navigation("Slot");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Session", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Topic", "Topic")
                        .WithMany("Sessions")
                        .HasForeignKey("TopicId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Topic");
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

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentClass", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Class", "Class")
                        .WithMany("StudentClasses")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("MagicLand_System.Domain.Models.Student", "Student")
                        .WithMany("StudentClasses")
                        .HasForeignKey("StudentId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Class");

                    b.Navigation("Student");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.StudentInCart", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.CartItem", "CartItem")
                        .WithMany("StudentInCarts")
                        .HasForeignKey("CartItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CartItem");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.SubDescriptionContent", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.SubDescriptionTitle", "SubDescriptionTitle")
                        .WithMany("SubDescriptionContents")
                        .HasForeignKey("SubDescriptionTitleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SubDescriptionTitle");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.SubDescriptionTitle", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.Course", "Course")
                        .WithMany("SubDescriptionTitles")
                        .HasForeignKey("CourseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Course");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Topic", b =>
                {
                    b.HasOne("MagicLand_System.Domain.Models.CourseSyllabus", "CourseSyllabus")
                        .WithMany("Topics")
                        .HasForeignKey("CourseSyllabusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("CourseSyllabus");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
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
                        .WithMany()
                        .HasForeignKey("UserId")
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

            modelBuilder.Entity("MagicLand_System.Domain.Models.Cart", b =>
                {
                    b.Navigation("CartItems");

                    b.Navigation("User");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CartItem", b =>
                {
                    b.Navigation("StudentInCarts");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Class", b =>
                {
                    b.Navigation("Schedules");

                    b.Navigation("StudentClasses");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Course", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("CoursePrerequisites");

                    b.Navigation("CourseSyllabus");

                    b.Navigation("SubDescriptionTitles");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CourseCategory", b =>
                {
                    b.Navigation("Courses");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.CourseSyllabus", b =>
                {
                    b.Navigation("Topics");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.PersonalWallet", b =>
                {
                    b.Navigation("User");

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
                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Slot", b =>
                {
                    b.Navigation("Schedules");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Student", b =>
                {
                    b.Navigation("StudentClasses");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.SubDescriptionTitle", b =>
                {
                    b.Navigation("SubDescriptionContents");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.Topic", b =>
                {
                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.User", b =>
                {
                    b.Navigation("Classes");

                    b.Navigation("Students");
                });

            modelBuilder.Entity("MagicLand_System.Domain.Models.UserPromotion", b =>
                {
                    b.Navigation("PromotionTransactions");
                });
#pragma warning restore 612, 618
        }
    }
}
