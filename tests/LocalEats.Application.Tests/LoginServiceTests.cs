using LocalEats.Application.Users;

namespace LocalEats.Application.Tests;

public sealed class LoginServiceTests
{
    [Theory]
    [InlineData(null, "/session")]
    [InlineData("https://example.com", "/session")]
    [InlineData("//example.com", "/session")]
    [InlineData("/\\example.com", "/session")]
    [InlineData("/path\r\nLocation: bad", "/session")]
    [InlineData("/orders?page=2", "/orders?page=2")]
    [InlineData("/", "/")]
    public void ReturnUrlPolicy_UntrustedInput_OnlyAllowsLocalPaths(string? input, string expected)
    {
        Assert.Equal(expected, ReturnUrlPolicy.Normalize(input));
    }

    [Fact]
    public async Task SignInAsync_InvalidInput_DoesNotCallIdentityStore()
    {
        var store = new RecordingSessions();
        Assert.False(await new LoginService(store).SignInAsync(new LoginRequest { Email = "bad", Password = "test" }));
        Assert.Equal(0, store.SignInCalls);
    }

    [Fact]
    public async Task SignInAsync_ValidInput_PassesPersistencePreference()
    {
        var store = new RecordingSessions();
        Assert.True(await new LoginService(store).SignInAsync(new LoginRequest
        {
            Email = "user@example.test",
            Password = "Valid-Password123!",
            RememberMe = true
        }));
        Assert.True(store.RememberMe);
        Assert.Equal(1, store.SignInCalls);
    }

    private sealed class RecordingSessions : ISessionStore
    {
        public int SignInCalls { get; private set; }
        public bool RememberMe { get; private set; }
        public Task<bool> SignInAsync(string email, string password, bool rememberMe)
        {
            SignInCalls++;
            RememberMe = rememberMe;
            return Task.FromResult(true);
        }

        public Task SignOutAsync() => Task.CompletedTask;
    }
}
