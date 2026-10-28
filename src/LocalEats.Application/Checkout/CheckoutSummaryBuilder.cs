using LocalEats.Application.Basket;

namespace LocalEats.Application.Checkout;

public static class CheckoutSummaryBuilder
{
    public static CheckoutSummary Build(BasketSnapshot basket, IReadOnlyList<BasketProduct> catalog)
    {
        CheckoutSummary Failure(string message) => new(null, [], [message]);
        var products = catalog.ToDictionary(product => product.Id);
        var lines = new List<CheckoutLine>();
        Guid? restaurantId = null;
        string? restaurantName = null;
        foreach (var line in basket.Lines)
        {
            if (!products.TryGetValue(line.ItemId, out var product) || !product.CanOrder || product.RestaurantId == Guid.Empty
                || product.PriceMinor < 0 || product.PriceMinor > long.MaxValue / (BasketState.MaxLines * BasketState.MaxQuantity))
                return Failure("An item is unavailable or its restaurant is closed. Review your basket.");
            if (restaurantId is not null && restaurantId != product.RestaurantId)
                return Failure("Your basket must contain items from one restaurant.");
            restaurantId = product.RestaurantId;
            restaurantName = product.RestaurantName;
            lines.Add(new(line.ItemId, product.Name, line.Quantity, product.PriceMinor));
        }
        return new(restaurantName, lines, []);
    }

}
