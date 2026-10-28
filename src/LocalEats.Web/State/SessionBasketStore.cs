using LocalEats.Application.Basket;
using Microsoft.Extensions.Caching.Memory;

namespace LocalEats.Web.State;

public sealed class SessionBasketStore(IHttpContextAccessor accessor, IMemoryCache cache) : IBasketStateStore
{
    private static readonly object CreationGate = new();
    public BasketState Current
    {
        get
        {
            var context = accessor.HttpContext ?? throw new InvalidOperationException("A basket requires an HTTP request.");
            var key = $"LocalEatsBasket:{context.Session.Id}";
            lock (CreationGate)
            {
                if (cache.TryGetValue<BasketState>(key, out var state) && state is not null) return state;
                state = new BasketState();
                cache.Set(key, state, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromMinutes(30) });
                return state;
            }
        }
    }
}
