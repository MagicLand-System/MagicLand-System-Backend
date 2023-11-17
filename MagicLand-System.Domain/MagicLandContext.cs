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
        public DbSet<Schedule> Schedules { get; set; }
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
                entity.ToTable("ClasInstance");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Schedule).WithMany(e => e.ClassInstances).HasForeignKey(e => e.ScheduleId);
            });
            modelBuilder.Entity<ClassTransaction>(entity =>
            {
                entity.ToTable("ClassTransaction");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Class).WithMany(entity => entity.ClasssTransactions);
                entity.HasOne(e => e.ClassFeeTransaction).WithMany(e => e.ClassTransactions).HasForeignKey(e => e.ClassFeeTransactionId);
            });
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");
                entity.HasKey(e => e.Id);

            });
        }
    }
}
