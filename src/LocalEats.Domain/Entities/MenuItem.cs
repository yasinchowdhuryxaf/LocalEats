namespace LocalEats.Domain.Entities;

public sealed class MenuItem
{
    public Guid Id { get; set; }

    public Guid MenuCategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public long PriceMinor { get; set; }

    public bool IsAvailable { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public MenuCategory MenuCategory { get; set; } = null!;
}
