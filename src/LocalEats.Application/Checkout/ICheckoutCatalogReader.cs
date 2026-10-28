using LocalEats.Application.Basket;

namespace LocalEats.Application.Checkout;

public interface ICheckoutCatalogReader
{
    Task<IReadOnlyList<BasketProduct>> GetManyAsync(IReadOnlyList<Guid> ids, CancellationToken cancellationToken = default);
}
