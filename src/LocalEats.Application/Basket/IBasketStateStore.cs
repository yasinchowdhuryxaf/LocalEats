namespace LocalEats.Application.Basket;

public interface IBasketStateStore
{
    BasketState Current { get; }
}
