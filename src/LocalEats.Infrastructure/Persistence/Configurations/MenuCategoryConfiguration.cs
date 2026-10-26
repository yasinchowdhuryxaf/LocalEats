using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class MenuCategoryConfiguration : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).HasMaxLength(120).IsRequired();
        builder.HasIndex(category => new { category.RestaurantId, category.SortOrder });
        builder.HasMany(category => category.Items)
            .WithOne(item => item.MenuCategory)
            .HasForeignKey(item => item.MenuCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
