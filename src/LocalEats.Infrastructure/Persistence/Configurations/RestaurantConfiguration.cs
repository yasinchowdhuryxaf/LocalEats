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
        builder.Property(restaurant => restaurant.Address).HasMaxLength(500);
        builder.Property(restaurant => restaurant.IsOpen).HasDefaultValue(false);
        builder.Ignore(restaurant => restaurant.IsVisible);
        builder.Ignore(restaurant => restaurant.CanAcceptOrders);
        builder.Property(restaurant => restaurant.Area).HasMaxLength(120);
        builder.Property(restaurant => restaurant.Cuisine).HasMaxLength(120);
        builder.HasIndex(restaurant => restaurant.Slug).IsUnique();
        builder.HasIndex(restaurant => new { restaurant.IsApproved, restaurant.IsActive });
        builder.HasIndex(restaurant => restaurant.Area);
        builder.HasIndex(restaurant => restaurant.Cuisine);
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_Restaurants_Name_NotBlank", "length(btrim(\"Name\")) > 0");
            table.HasCheckConstraint("CK_Restaurants_Slug_NotBlank", "length(btrim(\"Slug\")) > 0");
        });
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
