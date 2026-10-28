namespace LocalEats.Application.Checkout;

public sealed record CheckoutLine(Guid ItemId, string Name, int Quantity, long UnitPriceMinor)
{
    public long TotalMinor => checked(Quantity * UnitPriceMinor);
}
