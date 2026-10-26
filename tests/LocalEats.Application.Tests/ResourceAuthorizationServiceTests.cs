using System.Security.Claims;
using LocalEats.Application.Security;

namespace LocalEats.Application.Tests;

public sealed class ResourceAuthorizationServiceTests
{
    [Theory]
    [InlineData(RoleNames.Customer, true, true)]
    [InlineData(RoleNames.Customer, false, false)]
    [InlineData(RoleNames.Manager, true, false)]
    [InlineData(RoleNames.Manager, false, false)]
    [InlineData(RoleNames.Admin, true, true)]
    [InlineData(RoleNames.Admin, false, true)]
    [InlineData("Unknown", true, false)]
    public async Task CanReadOrderAsync_RoleAndOwnership_EnforcesAccess(string role, bool isOwner, bool expected)
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var reader = new RecordingReader(new OrderOwnership(orderId, isOwner ? userId : Guid.NewGuid()));
        var user = Principal(userId.ToString(), role);

        Assert.Equal(expected, await new ResourceAuthorizationService(reader).CanReadOrderAsync(user, orderId));
    }

    [Theory]
    [InlineData(false, "valid", RoleNames.Admin)]
    [InlineData(true, "not-a-guid", RoleNames.Admin)]
    [InlineData(true, "00000000-0000-0000-0000-000000000000", RoleNames.Customer)]
    [InlineData(true, "valid", "Unknown")]
    public async Task CanReadOrderAsync_UntrustedIdentity_DeniesWithoutDatabaseLookup(bool authenticated, string id, string role)
    {
        var reader = new RecordingReader(null);
        var user = Principal(id == "valid" ? Guid.NewGuid().ToString() : id, role, authenticated);

        Assert.False(await new ResourceAuthorizationService(reader).CanReadOrderAsync(user, Guid.NewGuid()));
        Assert.Equal(0, reader.Calls);
    }

    [Theory]
    [InlineData(RoleNames.Customer)]
    [InlineData(RoleNames.Admin)]
    public async Task CanReadOrderAsync_MissingOrder_DeniesEvenForAdmin(string role)
    {
        var service = new ResourceAuthorizationService(new RecordingReader(null));
        Assert.False(await service.CanReadOrderAsync(Principal(Guid.NewGuid().ToString(), role), Guid.NewGuid()));
    }

    private static ClaimsPrincipal Principal(string id, string role, bool authenticated = true) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Role, role)],
        authenticated ? "test" : null));

    private sealed class RecordingReader(OrderOwnership? order) : IOrderOwnershipReader
    {
        public int Calls { get; private set; }
        public Task<OrderOwnership?> FindAsync(Guid orderId, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(order);
        }
    }
}
