using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Role> Roles { get; set; }
        public DbSet<User>? Users { get;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureRole(modelBuilder);
            ConfigureUser(modelBuilder);
        }

        private static void ConfigureUser(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(r => r.Id);

                entity.Property(r => r.Id).ValueGeneratedOnAdd();
                entity.Property(r => r.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(r => r.Description)
                    .HasMaxLength(255);
                entity.Property(r => r.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                
                entity.HasIndex(r => r.Name).IsUnique();

                entity.HasMany(r => r.Users)
                    .WithOne(u => u.Role)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Id).ValueGeneratedOnAdd();
                entity.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);
                entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);
                entity.Property(u => u.CreatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(u => u.UpdatedAt)
                    .HasColumnType("datetime2")
                    .HasDefaultValueSql("SYSUTCDATETIME()");
                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);

                entity.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}