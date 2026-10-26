namespace LocalEats.Domain.Entities;

public sealed class MenuCategory
{
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Restaurant Restaurant { get; set; } = null!;

    public ICollection<MenuItem> Items { get; set; } = new List<MenuItem>();
}
