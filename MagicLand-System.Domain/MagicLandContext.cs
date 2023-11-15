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
        }
    }
}
