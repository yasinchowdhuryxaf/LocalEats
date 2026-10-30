namespace LocalEats.Domain.Entities;

public sealed class ManagerRestaurant
{
    public Guid ManagerId { get; set; }
    public Guid RestaurantId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
}
