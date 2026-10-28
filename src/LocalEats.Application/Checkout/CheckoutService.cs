using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LocalEats.Application.Basket;
using LocalEats.Application.Security;

namespace LocalEats.Application.Checkout;

public sealed class CheckoutService(ICheckoutCatalogReader catalog)
{
    public async Task<CheckoutSummary> SummaryAsync(ClaimsPrincipal user, BasketSnapshot basket, CancellationToken cancellationToken = default)
    {
        CheckoutSummary Failure(string message) => new(null, [], [message]);
        if (user.Identity?.IsAuthenticated != true || !user.IsInRole(RoleNames.Customer)
            || !Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var userId) || userId == Guid.Empty)
            return Failure("Sign in with a customer account to continue.");
        if (basket.Lines.Count == 0) return Failure("Your basket is empty. Add an available item first.");
        if (basket.Lines.Count > BasketState.MaxLines || basket.Lines.Any(line => line.Quantity < 1 || line.Quantity > BasketState.MaxQuantity)
            || basket.Lines.Select(line => line.ItemId).Distinct().Count() != basket.Lines.Count)
            return Failure("Your basket contains invalid quantities or items. Review your basket.");
        var products = (await catalog.GetManyAsync(basket.Lines.Select(line => line.ItemId).ToArray(), cancellationToken)).ToDictionary(product => product.Id);
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
            lines.Add(new(product.Name, line.Quantity, product.PriceMinor));
        }
        return new(restaurantName, lines, []);
    }

    public async Task<CheckoutReview> ReviewAsync(ClaimsPrincipal user, BasketSnapshot basket, CheckoutInput input, CancellationToken cancellationToken = default)
    {
        var normalized = new CheckoutInput
        {
            Name = input.Name?.Trim() ?? string.Empty,
            Phone = input.Phone?.Trim() ?? string.Empty,
            Address = input.Address?.Trim(),
            Notes = input.Notes?.Trim(),
            PaymentMethod = input.PaymentMethod
        };
        var validation = new List<ValidationResult>();
        Validator.TryValidateObject(normalized, new ValidationContext(normalized), validation, true);
        var summary = await SummaryAsync(user, basket, cancellationToken);
        return new CheckoutReview(summary, validation.Select(result => result.ErrorMessage ?? "Check your details.").Concat(summary.Errors).ToArray());
    }
}
