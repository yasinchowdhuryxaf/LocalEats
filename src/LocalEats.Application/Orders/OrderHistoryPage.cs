namespace LocalEats.Application.Orders;

public sealed record OrderHistoryPage(IReadOnlyList<OrderHistoryEntry> Items, int Total, int Page, int TotalPages);
