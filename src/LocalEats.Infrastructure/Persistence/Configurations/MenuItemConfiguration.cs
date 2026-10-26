using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Name).HasMaxLength(160).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(2000);
        builder.Property(item => item.PriceMinor).IsRequired();
        builder.ToTable(table => table.HasCheckConstraint("CK_MenuItems_PriceMinor_NonNegative", "\"PriceMinor\" >= 0"));
        builder.HasIndex(item => new { item.MenuCategoryId, item.IsAvailable });
    }
}
