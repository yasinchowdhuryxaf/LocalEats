using LocalEats.Domain.Enums;

namespace LocalEats.Domain.Entities;

public sealed class Order
{
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public Guid? CustomerId { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public long TotalMinor { get; set; }

    public string Currency { get; set; } = "BDT";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
