using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.HasKey(restaurant => restaurant.Id);
        builder.Property(restaurant => restaurant.Name).HasMaxLength(160).IsRequired();
        builder.Property(restaurant => restaurant.Slug).HasMaxLength(160).IsRequired();
        builder.Property(restaurant => restaurant.Description).HasMaxLength(2000);
        builder.Property(restaurant => restaurant.Area).HasMaxLength(120);
        builder.Property(restaurant => restaurant.Cuisine).HasMaxLength(120);
        builder.HasIndex(restaurant => restaurant.Slug).IsUnique();
        builder.HasIndex(restaurant => new { restaurant.IsApproved, restaurant.IsActive });
        builder.HasMany(restaurant => restaurant.Categories)
            .WithOne(category => category.Restaurant)
            .HasForeignKey(category => category.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(restaurant => restaurant.Orders)
            .WithOne(order => order.Restaurant)
            .HasForeignKey(order => order.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
