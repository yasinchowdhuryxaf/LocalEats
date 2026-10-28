namespace LocalEats.Application.Basket;

public sealed record BasketProduct(Guid Id, Guid RestaurantId, string RestaurantName, string Name, long PriceMinor, bool CanOrder);
