using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LocalEats.Application.Checkout;

public static class CheckoutQuote
{
    public static string Fingerprint(CheckoutSummary summary)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            summary.RestaurantName,
            Lines = summary.Lines.OrderBy(line => line.ItemId).ToArray()
        }))));
}
