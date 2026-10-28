namespace LocalEats.Application.Orders;

public sealed record OrderPlacementResult(OrderReceipt? Receipt, string? Error)
{
    public bool Succeeded => Receipt is not null;
}
