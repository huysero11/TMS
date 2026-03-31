using backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend.Configurations
{
    public class TicketCategoryConfiguration : IEntityTypeConfiguration<TicketCategory>
    {
        public void Configure(EntityTypeBuilder<TicketCategory> builder)
        {
            builder.ToTable("TicketCategories");

            builder.HasKey(tc => tc.Id);

            builder.Property(tc => tc.Name)
                .IsRequired()
                .HasMaxLength(100);
            
             builder.Property(tc => tc.Description)
            .HasMaxLength(255);

            builder.Property(tc => tc.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(tc => tc.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.Property(tc => tc.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime2")
                .HasDefaultValueSql("SYSUTCDATETIME()");

            builder.HasIndex(tc => tc.Name)
                .IsUnique();

            builder.HasMany(tc => tc.Tickets)
                .WithOne(tc => tc.Category)
                .HasForeignKey(tc => tc.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}