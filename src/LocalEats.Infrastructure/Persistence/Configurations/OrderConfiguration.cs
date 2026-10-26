using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(order => order.Status).IsRequired();
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.HasIndex(order => order.OrderNumber).IsUnique();
        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt });
        builder.HasIndex(order => new { order.RestaurantId, order.Status, order.CreatedAt });
        builder.ToTable(table => table.HasCheckConstraint("CK_Orders_TotalMinor_NonNegative", "\"TotalMinor\" >= 0"));
        builder.HasMany(order => order.Items)
            .WithOne(item => item.Order)
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
