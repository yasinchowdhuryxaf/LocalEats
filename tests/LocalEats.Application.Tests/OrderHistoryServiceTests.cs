using System.Security.Claims;
using LocalEats.Application.Orders;

namespace LocalEats.Application.Tests;

public sealed class OrderHistoryServiceTests
{
    private sealed class Reader : IOrderHistoryReader
    {
        public Guid? CustomerId { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public Task<OrderHistoryPage> GetAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            CustomerId = customerId; Page = page; PageSize = pageSize;
            return Task.FromResult(new OrderHistoryPage([], 0, 1, 1));
        }
    }

    [Theory]
    [InlineData("anonymous")]
    [InlineData("Manager")]
    [InlineData("Admin")]
    [InlineData("invalid-id")]
    public async Task History_InvalidCustomer_CannotRead(string role)
    {
        var reader = new Reader();
        var user = role == "anonymous" ? new ClaimsPrincipal() : new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, role == "invalid-id" ? "Customer" : role),
            new Claim(ClaimTypes.NameIdentifier, role == "invalid-id" ? "invalid" : Guid.NewGuid().ToString())], "test"));
        Assert.Null(await new OrderHistoryService(reader).GetAsync(user));
        Assert.Null(reader.CustomerId);
    }

    [Fact]
    public async Task History_ValidCustomer_UsesSignedIdentityAndBoundedPage()
    {
        var reader = new Reader();
        var id = Guid.NewGuid();
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Customer"), new Claim(ClaimTypes.NameIdentifier, id.ToString())], "test"));
        Assert.NotNull(await new OrderHistoryService(reader).GetAsync(user, -1));
        Assert.Equal(id, reader.CustomerId);
        Assert.Equal(1, reader.Page);
        Assert.Equal(10, reader.PageSize);
    }
}
