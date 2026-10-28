namespace LocalEats.Application.Checkout;

public sealed record CheckoutLine(string Name, int Quantity, long UnitPriceMinor)
{
    public long TotalMinor => checked(Quantity * UnitPriceMinor);
}
