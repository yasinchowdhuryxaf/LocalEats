using System.ComponentModel.DataAnnotations;

namespace LocalEats.Application.Checkout;

public sealed class CheckoutInput : IValidatableObject
{
    [Required, StringLength(100)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(32), Phone] public string Phone { get; set; } = string.Empty;
    [StringLength(500)] public string? Address { get; set; }
    [StringLength(500)] public string? Notes { get; set; }
    [Required, RegularExpression("^(CashOnDelivery|PayAtPickup)$", ErrorMessage = "Choose Cash on Delivery or Pay at Pickup.")]
    public string PaymentMethod { get; set; } = "CashOnDelivery";
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PaymentMethod == "CashOnDelivery" && string.IsNullOrWhiteSpace(Address))
            yield return new ValidationResult("A delivery address is required for Cash on Delivery.", [nameof(Address)]);
    }
}
