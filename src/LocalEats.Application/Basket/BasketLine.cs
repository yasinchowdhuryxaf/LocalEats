namespace LocalEats.Application.Basket;

public sealed record BasketLine(Guid ItemId, Guid RestaurantId, string RestaurantName, string Name, long PriceMinor, int Quantity)
{
    public long TotalMinor => checked(PriceMinor * Quantity);
}
