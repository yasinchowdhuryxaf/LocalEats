namespace LocalEats.Application.Basket;

public interface IBasketProductReader
{
    Task<BasketProduct?> GetAsync(Guid itemId, CancellationToken cancellationToken = default);
}
