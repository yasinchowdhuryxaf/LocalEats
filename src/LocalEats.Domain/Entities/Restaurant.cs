namespace LocalEats.Domain.Entities;

public sealed class Restaurant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Area { get; set; }

    public string? Cuisine { get; set; }

    public bool IsApproved { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public ICollection<MenuCategory> Categories { get; set; } = new List<MenuCategory>();

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
