﻿using MagicLand_System.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace MagicLand_System.Domain
{
    public class MagicLandContext : DbContext
    {
        public MagicLandContext() { }
        public MagicLandContext(DbContextOptions<MagicLandContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<PersonalWallet> PersonalWallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<StudentInCart> StudentIncarts { get; set; }

        public DbSet<UserPromotion> UserPromotions { get; set; } // On Fixing
        public DbSet<Promotion> Promotions { get; set; } // On Fixing
        public DbSet<PromotionTransaction> PromotionTransactions { get; set; } // On Fixing
        public DbSet<Student> Students { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Schedule> Sessions { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<CourseCategory> CourseCategories { get; set; }
        public DbSet<CourseSyllabus> CourseSyllabuses { get; set; }
        public DbSet<SubDescriptionTitle> SubDescriptionTitles { get; set; }
        public DbSet<SubDescriptionContent> SubDescriptionContents { get; set; }
        public DbSet<Attendance> Attendances { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(GetConnectionString());
            }
        }
        private string GetConnectionString()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            var strConn = config["ConnectionStrings:DefaultDB"]!;
            return strConn;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Phone, "UX_User_Phone");
                entity.Property(entity => entity.FullName).HasMaxLength(255);
                entity.Property(e => e.DateOfBirth).HasDefaultValueSql("getutcdate()");
                entity.HasOne(e => e.Role).WithMany(r => r.Accounts).HasForeignKey(e => e.RoleId).HasConstraintName("FK_USER_ROLE");
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(entity => entity.Id);
                entity.Property(entity => entity.Name).HasMaxLength(20);
            });

            modelBuilder.Entity<Cart>(entity =>
            {
                entity.ToTable("Cart");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithOne(e => e.Cart).HasForeignKey<User>(e => e.CartId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.ToTable("CartItem");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Cart).WithMany(e => e.CartItems).HasForeignKey(e => e.CartId);
            });
            modelBuilder.Entity<StudentInCart>(entity =>
            {
                entity.ToTable("StudentInCart");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.CartItem).WithMany(e => e.StudentInCarts).HasForeignKey(e => e.CartItemId);
            });
            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Class");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Lecture).WithMany(e => e.Classes).HasForeignKey(entity => entity.LecturerId);
                entity.HasOne(e => e.Course).WithMany(r => r.Classes).HasForeignKey(e => e.CourseId);
                entity.Property(e => e.StartDate).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.EndDate).HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<StudentClass>(entity =>
            {
                entity.ToTable("StudentClass");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Class).WithMany(e => e.StudentClasses).HasForeignKey(e => e.ClassId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Student).WithMany(e => e.StudentClasses).HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.CourseCategory).WithMany(e => e.Courses).HasForeignKey(e => e.CourseCategoryId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.CourseSyllabus).WithOne(e => e.Course).HasForeignKey<CourseSyllabus>(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CoursePrerequisite>(entity =>
            {
                entity.ToTable("CoursePrerequisite");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Course).WithMany(e => e.CoursePrerequisites).HasForeignKey(e => e.CurrentCourseId);
            });
            modelBuilder.Entity<PersonalWallet>(entity =>
            {
                entity.ToTable("PersonalWallet");
                entity.HasKey(entity => entity.Id);
                entity.Property(entity => entity.Balance).HasDefaultValue(0);

                entity.HasOne(e => e.User).WithOne(e => e.PersonalWallet).HasForeignKey<User>(e => e.PersonalWalletId);
            });
            modelBuilder.Entity<WalletTransaction>(entity =>
            {
                entity.ToTable("WalletTransaction");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.PersonalWallet).WithMany(e => e.WalletTransactions).HasForeignKey(e => e.PersonalWalletId);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedule");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Class).WithMany(e => e.Schedules).HasForeignKey(e => e.ClassId);
                entity.HasOne(e => e.Room).WithMany(e => e.Schedules).HasForeignKey(e => e.RoomId);
                entity.HasOne(e => e.Slot).WithMany(e => e.Schedules).HasForeignKey(e => e.SlotId);

            });
            modelBuilder.Entity<Slot>(entity =>
            {
                entity.ToTable("Slot");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Student");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.User).WithMany(e => e.Students).HasForeignKey(e => e.ParentId);

            });
            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.ToTable("CourseCategory");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<SubDescriptionTitle>(entity =>
            {
                entity.ToTable("SubDescriptionTitle");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Course).WithMany(e => e.SubDescriptionTitles).HasForeignKey(e => e.CourseId);
            });
            modelBuilder.Entity<SubDescriptionContent>(entity =>
            {
                entity.ToTable("SubDescriptionContent");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.SubDescriptionTitle).WithMany(e => e.SubDescriptionContents).HasForeignKey(e => e.SubDescriptionTitleId);
            });
            modelBuilder.Entity<CourseSyllabus>(entity =>
            {
                entity.ToTable("CourseSyllabus");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<Topic>(entity =>
            {
                entity.ToTable("Topic");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.CourseSyllabus).WithMany(e => e.Topics).HasForeignKey(e => e.CourseSyllabusId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Session");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Topic).WithMany(e => e.Sessions).HasForeignKey(e => e.TopicId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable("Attendance");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Schedule).WithMany(e => e.Attendances).HasForeignKey(e => e.ScheduleId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Student).WithMany(e => e.Attendances).HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
