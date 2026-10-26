namespace LocalEats.Domain.Entities;

public sealed class AuditLog
{
    public Guid Id { get; set; }

    public Guid? ActorId { get; set; }

    public Guid? RestaurantId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public Guid? ResourceId { get; set; }

    public string? MetadataJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
