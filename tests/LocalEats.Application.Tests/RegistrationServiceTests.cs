using LocalEats.Application.Users;

namespace LocalEats.Application.Tests;

public sealed class RegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_ValidInput_TrimsEmailAndCallsStore()
    {
        var store = new RecordingStore();
        var result = await new RegistrationService(store).RegisterAsync(Request(" user@example.com "));

        Assert.True(result.Succeeded);
        Assert.Equal("user@example.com", store.Email);
        Assert.Equal(1, store.Calls);
    }

    [Theory]
    [InlineData("invalid-email", "Valid-Password123!", "Valid-Password123!", "Email")]
    [InlineData("user@example.com", "short", "short", "Password")]
    [InlineData("user@example.com", "Valid-Password123!", "Different123!", "ConfirmPassword")]
    [InlineData("", "Valid-Password123!", "Valid-Password123!", "Email")]
    public async Task RegisterAsync_InvalidInput_DoesNotCallStore(string email, string password, string confirmation, string field)
    {
        var store = new RecordingStore();
        var request = new RegistrationRequest { Email = email, Password = password, ConfirmPassword = confirmation };

        var result = await new RegistrationService(store).RegisterAsync(request);

        Assert.False(result.Succeeded);
        Assert.Contains(field, result.Errors.Keys);
        Assert.Equal(0, store.Calls);
    }

    [Fact]
    public async Task RegisterAsync_StoreFailure_PreservesSafeError()
    {
        var expected = RegistrationResult.Failure(string.Empty, "Unable to create an account.");
        var result = await new RegistrationService(new RecordingStore { Result = expected }).RegisterAsync(Request("user@example.com"));

        Assert.Same(expected, result);
    }

    private static RegistrationRequest Request(string email) => new()
    {
        Email = email,
        Password = "Valid-Password123!",
        ConfirmPassword = "Valid-Password123!"
    };

    private sealed class RecordingStore : IRegistrationStore
    {
        public int Calls { get; private set; }
        public string? Email { get; private set; }
        public RegistrationResult Result { get; init; } = RegistrationResult.Success();

        public Task<RegistrationResult> CreateCustomerAsync(string email, string password, CancellationToken cancellationToken)
        {
            Calls++;
            Email = email;
            return Task.FromResult(Result);
        }
    }
}
