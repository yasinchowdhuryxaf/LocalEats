namespace LocalEats.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }

    public Guid? MenuItemId { get; set; }

    public string ItemName { get; set; } = string.Empty;

    public long UnitPriceMinor { get; set; }

    public int Quantity { get; set; }

    public long LineTotalMinor { get; set; }

    public Order Order { get; set; } = null!;

    public MenuItem? MenuItem { get; set; }
}
