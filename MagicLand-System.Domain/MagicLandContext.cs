using MagicLand_System.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public DbSet<Address> Address { get; set; }
        public DbSet<UserPromotion> UserPromotions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<PromotionTransaction> PromotionTransactions { get; set; }
        public DbSet<Student> Students { get; set; }    
        public DbSet<ClassFeeTransaction> ClassFeeTransactions { get; set; }
        public DbSet<ClassTransaction> ClassTransactions { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<StudentTransaction> StudentTransactions { get; set; }
        public DbSet<ClassInstance> ClassInstances { get; set; }
        public DbSet<Course> Courses { get; set; }  
        public DbSet<CoursePrerequisite> CoursePrerequisites { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Room> Rooms { get; set; }  
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
                entity.HasOne(e => e.Address).WithOne(r => r.User).HasForeignKey<Address>(e => e.UserId);
            });
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Role");
                entity.HasKey(entity => entity.Id);
                entity.Property(entity => entity.Name).HasMaxLength(20);
            });
            modelBuilder.Entity<Address>(entity =>
            {
                entity.ToTable("Address");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithOne(e => e.Address).HasForeignKey<User>(e => e.AddressId);

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
                entity.HasOne(e => e.Cart).WithMany(e => e.Carts).HasForeignKey(e => e.CartId);
            });
            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("Class");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithMany(e => e.Classes).HasForeignKey(entity => entity.LecturerId);
                entity.HasOne(e => e.Course).WithMany(r => r.Classes).HasForeignKey(e => e.CourseId);
                entity.HasOne(e => e.Address).WithMany(r => r.Classes).HasForeignKey(e => e.AddressId);
                entity.Property(e => e.StartTime).HasDefaultValueSql("getutcdate()");
                entity.Property(e => e.EndTime).HasDefaultValueSql("getutcdate()");
            });
            modelBuilder.Entity<ClassFeeTransaction>(entity =>
            {
                entity.ToTable("ClassFeeTransaction");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.User).WithMany(e => e.ClassFeeTransactions).HasForeignKey(e => e.ParentId);
                entity.Property(e => e.DateCreated).HasDefaultValueSql("getutcdate()");
            });
            modelBuilder.Entity<ClassInstance>(entity =>
            {
                entity.ToTable("ClassInstance");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Session).WithMany(e => e.ClassInstances).HasForeignKey(e => e.SessionId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Student).WithMany(e => e.ClassInstances).HasForeignKey(e => e.StudentId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<ClassTransaction>(entity =>
            {
                entity.ToTable("ClassTransaction");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Class).WithMany(entity => entity.ClasssTransactions);
                entity.HasOne(e => e.ClassFeeTransaction).WithMany(e => e.ClassTransactions).HasForeignKey(e => e.ClassFeeTransactionId).OnDelete(DeleteBehavior.Restrict);

            });
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");
                entity.HasKey(e => e.Id);
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
            modelBuilder.Entity<Promotion>(entity =>
            {
                entity.ToTable("Promotion");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<UserPromotion>(entity =>
            {
                entity.ToTable("UserPromotion");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Promotion).WithMany(e => e.UserPromotions).HasForeignKey(e => e.PromotionId);
                entity.HasOne(e => e.User).WithMany(e => e.UserPromotions).HasForeignKey(e => e.UserId);
            });
            modelBuilder.Entity<PromotionTransaction>(entity =>
            {
                entity.ToTable("PromotionTransaction");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.UserPromotion).WithMany(e => e.PromotionTransactions).HasForeignKey(e => e.UserPromotionId);
                entity.HasOne(e => e.ClassFeeTransaction).WithMany(e => e.PromotionTransactions).HasForeignKey(e => e.UserPromotionId).OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Room>(entity =>
            {
                entity.ToTable("Room");
                entity.HasKey(entity => entity.Id);
            });
            modelBuilder.Entity<Session>(entity =>
            {
                entity.ToTable("Session");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Class).WithMany(e => e.Sessions).HasForeignKey(e => e.ClassId);
                entity.HasOne(e => e.Room).WithMany(e => e.Sessions).HasForeignKey(e => e.RoomId);
                entity.HasOne(e => e.Slot).WithMany(e => e.Sessions).HasForeignKey(e => e.SlotId);

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
            modelBuilder.Entity<StudentTransaction>(entity =>
            {
                entity.ToTable("StudentTransaction");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Student).WithMany(e => e.StudentTransactions).HasForeignKey(e => e.StudentId);
                entity.HasOne(e => e.ClassTransaction).WithMany(e => e.StudentTransactions).HasForeignKey(e => e.ClassTransactionId);

            });
            });
            modelBuilder.Entity<StudentTransaction>(entity =>
            {
                entity.ToTable("StudentTransaction");
                entity.HasKey(entity => entity.Id);
                entity.HasOne(e => e.Student).WithMany(e => e.StudentTransactions).HasForeignKey(e => e.StudentId);
                entity.HasOne(e => e.ClassTransaction).WithMany(e => e.StudentTransactions).HasForeignKey(e => e.ClassTransactionId).OnDelete(DeleteBehavior.Restrict);

            });
        }
    }
}
