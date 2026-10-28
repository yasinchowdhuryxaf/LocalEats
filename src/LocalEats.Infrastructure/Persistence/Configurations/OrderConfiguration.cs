using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(order => order.Id);
        builder.Property(order => order.ContactName).HasMaxLength(100);
        builder.Property(order => order.ContactPhone).HasMaxLength(32);
        builder.Property(order => order.DeliveryAddress).HasMaxLength(500);
        builder.Property(order => order.Notes).HasMaxLength(500);
        builder.Property(order => order.PaymentMethod).HasMaxLength(20);
        builder.HasIndex(order => new { order.CustomerId, order.RequestId }).IsUnique().HasFilter("\"RequestId\" IS NOT NULL");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.SetNull);
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
