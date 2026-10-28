using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using LocalEats.Application.Basket;
using LocalEats.Application.Checkout;
using LocalEats.Application.Security;

namespace LocalEats.Application.Orders;

public sealed class OrderPlacementService(IOrderPlacementStore store)
{
    public static bool TryCustomer(ClaimsPrincipal user, out Guid id)
    {
        id = Guid.Empty;
        return user.Identity?.IsAuthenticated == true && user.IsInRole(RoleNames.Customer)
            && Guid.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out id) && id != Guid.Empty;
    }
    public Task<OrderReceipt?> FindAsync(ClaimsPrincipal user, Guid requestId, CancellationToken cancellationToken = default)
        => TryCustomer(user, out var id) ? store.FindAsync(id, requestId, cancellationToken) : Task.FromResult<OrderReceipt?>(null);

    public async Task<OrderPlacementResult> PlaceAsync(ClaimsPrincipal user, Guid requestId, BasketSnapshot basket, CheckoutInput input, string? quote, CancellationToken cancellationToken = default)
    {
        if (!TryCustomer(user, out var id) || requestId == Guid.Empty) return new(null, "Sign in with a customer account and reload checkout.");
        var existing = await store.FindAsync(id, requestId, cancellationToken);
        if (existing is not null) return new(existing, null);
        var contact = new CheckoutInput
        {
            Name = input.Name?.Trim() ?? string.Empty,
            Phone = input.Phone?.Trim() ?? string.Empty,
            Address = input.Address?.Trim(),
            Notes = input.Notes?.Trim(),
            PaymentMethod = input.PaymentMethod
        };
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(contact, new ValidationContext(contact), errors, true))
            return new(null, errors.FirstOrDefault()?.ErrorMessage ?? "Check your contact details.");
        if (basket.Lines.Count == 0 || basket.Lines.Count > BasketState.MaxLines
            || basket.Lines.Any(line => line.Quantity < 1 || line.Quantity > BasketState.MaxQuantity)
            || basket.Lines.Select(line => line.ItemId).Distinct().Count() != basket.Lines.Count)
            return new(null, "Your basket is empty or invalid. Review your basket.");
        if (string.IsNullOrWhiteSpace(quote)) return new(null, "Review checkout before placing your order.");
        return await store.PlaceAsync(id, requestId, basket, contact, quote, cancellationToken);
    }
}
