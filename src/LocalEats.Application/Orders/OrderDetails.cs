namespace LocalEats.Application.Orders;

public sealed record OrderDetails(Guid Id, string Number, string RestaurantName, string Status, string Currency,
    long TotalMinor, DateTimeOffset CreatedAt, string? ContactName, string? ContactPhone, string? Address,
    string? Notes, string? PaymentMethod, IReadOnlyList<OrderDetailLine> Items, int Page, int TotalPages);
