using LocalEats.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalEats.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ItemName).HasMaxLength(160).IsRequired();
        builder.ToTable(table =>
        {
            table.HasCheckConstraint("CK_OrderItems_Quantity_Positive", "\"Quantity\" > 0");
            table.HasCheckConstraint("CK_OrderItems_UnitPriceMinor_NonNegative", "\"UnitPriceMinor\" >= 0");
            table.HasCheckConstraint("CK_OrderItems_LineTotalMinor_NonNegative", "\"LineTotalMinor\" >= 0");
        });
        builder.HasOne(item => item.MenuItem)
            .WithMany()
            .HasForeignKey(item => item.MenuItemId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
