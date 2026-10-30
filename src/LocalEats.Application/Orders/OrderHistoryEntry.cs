namespace LocalEats.Application.Orders;

public sealed record OrderHistoryEntry(Guid Id, string Number, string RestaurantName, DateTimeOffset CreatedAt,
    long TotalMinor, string Currency, string Status);
