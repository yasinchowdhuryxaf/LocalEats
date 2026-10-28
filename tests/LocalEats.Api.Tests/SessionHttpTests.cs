using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using LocalEats.Application.Restaurants;
using LocalEats.Application.Security;
using LocalEats.Domain.Entities;
using LocalEats.Infrastructure.Identity;
using LocalEats.Infrastructure.Persistence;
using LocalEats.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LocalEats.Api.Tests;

[Collection("PostgreSQL")]
public sealed class SessionHttpTests : IAsyncLifetime
{
    private WebApplicationFactory<WebEntryPoint>? factory;
    private readonly string email = $"session-{Guid.NewGuid():N}@example.test";
    private const string Password = "Session-Test-Password123!";
    private Guid? testRestaurantId;
    private Guid? extraUserId;
    private readonly List<Guid> directoryIds = [];

    private async Task<Guid> MenuItemId(string slug, int number = 1)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().MenuItems
            .Where(item => item.MenuCategory.Restaurant.Slug == slug && item.Name == $"Menu item {number:D2}")
            .Select(item => item.Id).SingleAsync();
    }

    private static async Task<Dictionary<string, string>> BasketFields(HttpClient client, string path, string handler)
    {
        var html = await client.GetStringAsync(path);
        var form = Regex.Matches(html, "<form\\b.*?</form>", RegexOptions.Singleline).Cast<Match>()
            .Select(match => match.Value).Single(value => value.Contains($"value=\"{handler}\"", StringComparison.Ordinal));
        var fields = new Dictionary<string, string>();
        foreach (Match match in Regex.Matches(form, "<input[^>]*type=\"hidden\"[^>]*>"))
        {
            var name = Regex.Match(match.Value, "name=\"([^\"]*)\"").Groups[1].Value;
            var value = Regex.Match(match.Value, "value=\"([^\"]*)\"").Groups[1].Value;
            fields[WebUtility.HtmlDecode(name)] = WebUtility.HtmlDecode(value);
        }
        Assert.Contains("__RequestVerificationToken", fields.Keys);
        return fields;
    }

    private static async Task AddBasketItem(HttpClient client, string slug, Guid itemId)
    {
        var path = $"/restaurants/{slug}";
        var fields = await BasketFields(client, path, "basket-add");
        fields["AddInput.ItemId"] = itemId.ToString();
        fields["AddInput.PriceMinor"] = "1";
        fields["AddInput.RestaurantId"] = Guid.NewGuid().ToString();
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync(path, new FormUrlEncodedContent(fields))).StatusCode);
    }

    [Fact]
    public async Task Basket_Anonymous_AddChangeRemoveAndIsolation_UsesServerPrice()
    {
        var slug = await CreateMenuFixtureAsync("open", 2);
        var id = await MenuItemId(slug);
        using var client = Client();
        using var other = Client();
        await AddBasketItem(client, slug, id);
        var page = await client.GetAsync($"/restaurants/{slug}");
        Assert.True(page.Headers.CacheControl?.NoStore);
        var html = await page.Content.ReadAsStringAsync();
        Assert.Contains("Basket (1)", html);
        Assert.Contains("Display subtotal: BDT 123.46", html);
        Assert.Contains("Your basket is empty", await other.GetStringAsync($"/restaurants/{slug}"));
        foreach (var operation in new[] { $"increase:{id}", $"decrease:{id}", $"remove:{id}" })
        {
            var fields = await BasketFields(client, $"/restaurants/{slug}", "basket-change");
            fields["ChangeInput.Operation"] = operation;
            Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync($"/restaurants/{slug}", new FormUrlEncodedContent(fields))).StatusCode);
            html = await client.GetStringAsync($"/restaurants/{slug}");
            Assert.Contains(operation.StartsWith("increase", StringComparison.Ordinal) ? "Basket (2)" : operation.StartsWith("decrease", StringComparison.Ordinal) ? "Basket (1)" : "Your basket is empty", html);
        }
    }

    [Theory]
    [InlineData("open", 0)]
    [InlineData("closed", 1)]
    [InlineData("inactive", 1)]
    public async Task Basket_UnavailableProduct_CannotBeAddedFromAnotherPublicMenu(string state, int number)
    {
        var publicSlug = await CreateMenuFixtureAsync("open", 2);
        var rejectedSlug = await CreateMenuFixtureAsync(state, 2);
        using var client = Client();
        await AddBasketItem(client, publicSlug, await MenuItemId(rejectedSlug, number));
        var html = await client.GetStringAsync($"/restaurants/{publicSlug}");
        Assert.Contains("Your basket is empty", html);
        Assert.Contains("This item is unavailable or the restaurant is closed", html);
    }

    [Fact]
    public async Task Basket_SecondRestaurant_RejectsAndLogoutClears()
    {
        var first = await CreateMenuFixtureAsync("open", 2);
        var second = await CreateMenuFixtureAsync("open", 2);
        using var client = Client();
        await Login(client, email, Password);
        await AddBasketItem(client, first, await MenuItemId(first));
        await AddBasketItem(client, second, await MenuItemId(second));
        var html = await client.GetStringAsync($"/restaurants/{second}");
        Assert.Contains("Your basket belongs to another restaurant", html);
        Assert.Contains("Basket (1)", html);
        var fields = await FormFields(client, "/logout");
        await client.PostAsync("/logout", new FormUrlEncodedContent(fields));
        Assert.Contains("Your basket is empty", await client.GetStringAsync($"/restaurants/{first}"));
    }

    [Fact]
    public async Task Basket_PostWithoutAntiforgery_IsRejected()
    {
        var slug = await CreateMenuFixtureAsync("open", 2);
        using var client = Client();
        var response = await client.PostAsync($"/restaurants/{slug}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "basket-add",
            ["AddInput.ItemId"] = (await MenuItemId(slug)).ToString()
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<string> CreateMenuFixtureAsync(string state, int itemCount)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var row = new Restaurant
        {
            Id = Guid.NewGuid(),
            Slug = $"menu-{Guid.NewGuid():N}",
            Name = "Private menu fixture",
            Address = "Sample road",
            Area = "North",
            Cuisine = "Rice",
            IsApproved = state != "unapproved",
            IsActive = state != "inactive",
            IsOpen = state != "closed",
            DeletedAt = state == "deleted" ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var category = new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Meals",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        for (var i = 0; i < itemCount; i++)
            category.Items.Add(new MenuItem
            {
                Id = Guid.NewGuid(),
                Name = $"Menu item {i:D2}",
                PriceMinor = 12345 + i,
                IsAvailable = i != 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
        row.Categories.Add(category);
        row.Categories.Add(new MenuCategory
        {
            Id = Guid.NewGuid(),
            Name = "Hidden category",
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Items = [new MenuItem { Id = Guid.NewGuid(), Name = "Hidden item", PriceMinor = 100, IsAvailable = true,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }]
        });
        directoryIds.Add(row.Id);
        db.Restaurants.Add(row);
        await db.SaveChangesAsync();
        return row.Slug;
    }

    [Fact]
    public async Task Menu_Public_PaginatesAndDisablesUnavailablePlaceholders()
    {
        var slug = await CreateMenuFixtureAsync("open", 21);
        using var client = Client();
        var html = await client.GetStringAsync($"/restaurants/{slug}");
        Assert.Contains("Sample road", html);
        Assert.Contains("Meals", html);
        Assert.Contains("Menu item 00", html);
        Assert.Contains("BDT 123.45", html);
        Assert.Contains("Currently unavailable", html);
        Assert.Matches("<button[^>]*disabled[^>]*>Unavailable</button>", html);
        Assert.Contains(">Add to basket</button>", html);
        Assert.DoesNotContain("Hidden category", html);
        Assert.DoesNotContain("Hidden item", html);
        Assert.DoesNotContain("Menu item 20", html);
        var last = await client.GetStringAsync($"/restaurants/{slug}?page=2147483647");
        Assert.Contains("Page 2 of 2", last);
        Assert.Contains("Menu item 20", last);
        Assert.DoesNotContain("Menu item 00", last);
    }

    [Theory]
    [InlineData("unapproved")]
    [InlineData("inactive")]
    [InlineData("deleted")]
    [InlineData("missing")]
    public async Task Menu_NonPublicOrMissing_ReturnsIndistinguishableNotFound(string state)
    {
        var slug = state == "missing" ? $"missing-{Guid.NewGuid():N}" : await CreateMenuFixtureAsync(state, 1);
        using var client = Client();
        var response = await client.GetAsync($"/restaurants/{slug}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Restaurant not found", html);
        Assert.DoesNotContain("Private menu fixture", html);
        Assert.DoesNotContain("Menu item", html);
    }

    [Fact]
    public async Task Menu_ClosedAndEmpty_ShowsBrowseNoticeAndEmptyState()
    {
        var slug = await CreateMenuFixtureAsync("closed", 0);
        using var client = Client();
        var html = await client.GetStringAsync($"/restaurants/{slug}?page=invalid");
        Assert.Contains("This restaurant is currently closed", html);
        Assert.Contains("Menu coming soon", html);
        Assert.DoesNotContain("Hidden item", html);
    }

    [Fact]
    public async Task Menu_InvalidSlug_ReturnsSafeNotFound()
    {
        using var client = Client();
        var response = await client.GetAsync("/restaurants/bad_slug");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("Restaurant not found", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Menu_ReadFailure_ShowsSafeRetry()
    {
        await using var failing = Factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IRestaurantMenuReader>(_ => new FailingMenuReader())));
        using var client = failing.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/restaurants/example");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("We couldn’t load the menu. Please try again.", html);
        Assert.DoesNotContain("sensitive-menu-detail", html);
    }

    private sealed class FailingMenuReader : IRestaurantMenuReader
    {
        public Task<RestaurantMenuPage?> GetAsync(string slug, int page, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromException<RestaurantMenuPage?>(new InvalidOperationException("sensitive-menu-detail"));
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("-1")]
    [InlineData("2147483648")]
    public async Task Directory_InvalidPage_FallsBackSafely(string page)
    {
        using var client = Client();
        var html = await client.GetStringAsync($"/restaurants?q={Guid.NewGuid():N}&page={page}");
        Assert.Contains("Page 1 of 1", html);
        Assert.Contains("No restaurants match your search", html);
    }

    [Fact]
    public async Task Directory_ReadFailure_ShowsSafeRetryWithoutProviderDetails()
    {
        await using var failing = Factory.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddScoped<IRestaurantDirectoryReader>(_ => new FailingDirectoryReader())));
        using var client = failing.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var html = WebUtility.HtmlDecode(await client.GetStringAsync("/restaurants"));
        Assert.Contains("We couldn’t load restaurants. Please try again.", html);
        Assert.Contains("Try again", html);
        Assert.DoesNotContain("sensitive-provider-detail", html);
    }

    private sealed class FailingDirectoryReader : IRestaurantDirectoryReader
    {
        public Task<RestaurantDirectoryResult> SearchAsync(RestaurantDirectoryQuery query, int pageSize, CancellationToken cancellationToken = default)
            => Task.FromException<RestaurantDirectoryResult>(new InvalidOperationException("sensitive-provider-detail"));
    }

    [Fact]
    public async Task Directory_Anonymous_FiltersAndPaginatesOnlyPublicRecords()
    {
        var token = Guid.NewGuid().ToString("N");
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
            for (var i = 0; i < 10; i++)
            {
                var row = new Restaurant
                {
                    Id = Guid.NewGuid(),
                    Name = $"Directory {token} {i:D2}",
                    Slug = $"directory-{token}-{i}",
                    Area = i == 0 ? "North" : "South",
                    Cuisine = "Rice",
                    IsApproved = i != 7,
                    IsActive = i != 8,
                    DeletedAt = i == 9 ? DateTimeOffset.UtcNow : null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                directoryIds.Add(row.Id);
                db.Restaurants.Add(row);
            }
            await db.SaveChangesAsync();
        }
        using var client = Client();
        var first = await client.GetStringAsync($"/restaurants?q={token}");
        Assert.Contains("7 restaurants found", first);
        Assert.Contains($"Directory {token} 00", first);
        foreach (var number in new[] { 6, 7, 8, 9 }) Assert.DoesNotContain($"Directory {token} {number:D2}", first);
        var second = await client.GetStringAsync($"/restaurants?q={token}&page=2");
        Assert.Contains($"Directory {token} 06", second);
        Assert.DoesNotContain($"Directory {token} 00", second);
        Assert.Contains("Page 2 of 2", second);
        var filtered = await client.GetStringAsync($"/restaurants?q={token}&area=nORTH&cuisine=RICE&page=2147483647");
        Assert.Contains("1 restaurant found", filtered);
        Assert.Contains($"Directory {token} 00", filtered);
        Assert.Contains("Closed", filtered);
        Assert.DoesNotContain($"Directory {token} 01", filtered);
        Assert.Contains("No restaurants match your search", await client.GetStringAsync($"/restaurants?q={token}&area=Missing"));
    }

    [Fact]
    public async Task Directory_OversizedFilter_ShowsSafeValidation()
    {
        using var client = Client();
        var html = await client.GetStringAsync("/restaurants?q=" + new string('a', 161));
        Assert.Contains("Use up to 160 characters", html);
        Assert.DoesNotContain("Npgsql", html);
    }

    public async Task InitializeAsync()
    {
        var connection = Environment.GetEnvironmentVariable("LOCALEATS_TEST_CONNECTION")
            ?? "Host=127.0.0.1;Port=15432;Database=localeats_registration_tests;Username=localeats;Password=localeats_dev_password";
        var parsed = new Npgsql.NpgsqlConnectionStringBuilder(connection);
        if (parsed.Database?.EndsWith("_tests", StringComparison.Ordinal) != true)
        {
            throw new InvalidOperationException("HTTP session tests require a dedicated _tests database.");
        }

        factory = new WebApplicationFactory<WebEntryPoint>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("ConnectionStrings:DefaultConnection", connection);
        });
        await using var scope = Factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Database.MigrateAsync();
        await DevelopmentRoleSeeder.SeedAsync(scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>());
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { Email = email, UserName = email };
        Assert.True((await users.CreateAsync(user, Password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, ApplicationRoles.Customer)).Succeeded);
    }

    [Fact]
    public async Task Shell_Anonymous_ShowsAccessiblePublicNavigation()
    {
        using var client = Client();
        var html = await client.GetStringAsync("/");
        Assert.Contains("Skip to content", html);
        Assert.Contains("id=\"main-content\"", html);
        Assert.Contains("aria-label=\"Mobile navigation\"", html);
        Assert.Contains("href=\"/login\"", html);
        Assert.DoesNotContain("href=\"/manager\"", html);
        Assert.DoesNotContain("href=\"/admin\"", html);
        Assert.Contains("Basket (0)", html);
    }

    [Fact]
    public async Task Shell_Customer_ShowsOwnAreaAndAccountLinks()
    {
        using var client = Client();
        await Login(client, email, Password, "/");
        var html = await client.GetStringAsync("/");
        Assert.Contains("href=\"/dashboard\"", html);
        Assert.Contains("href=\"/account\"", html);
        Assert.Contains("href=\"/logout\"", html);
        Assert.DoesNotContain("href=\"/manager\"", html);
        Assert.DoesNotContain("href=\"/admin\"", html);
        Assert.DoesNotContain("href=\"/login\"", html);
    }

    [Fact]
    public async Task Dashboard_Customer_ShowsEncodedWelcomeAndHonestPlaceholders()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.DisplayName = "<script>alert(1)</script>";
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }
        using var client = Client();
        await Login(client, email, Password);
        var html = await client.GetStringAsync("/dashboard");
        Assert.Contains("Welcome, &lt;script&gt;alert(1)&lt;/script&gt;!", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
        Assert.Contains("Order history is coming soon", html);
        Assert.Contains("href=\"/restaurants\"", html);
        Assert.Contains("href=\"/account\"", html);
        Assert.Contains("href=\"/logout\"", html);
        Assert.Contains("Find your neighborhood favorites.", await client.GetStringAsync("/restaurants"));
    }

    [Theory]
    [InlineData("", "/dashboard")]
    [InlineData("/account", "/account")]
    [InlineData("/session", "/session")]
    public async Task Login_Customer_UsesDashboardDefaultAndPreservesLocalReturn(string returnUrl, string expected)
    {
        using var client = Client();
        var response = await Login(client, email, Password, returnUrl);
        Assert.Equal(expected, response.Headers.Location?.ToString());
    }

    [Theory]
    [InlineData("userId")]
    [InlineData("customerId")]
    public async Task Dashboard_TwoCustomers_QueryCannotSelectAnotherProfile(string queryKey)
    {
        var otherEmail = $"dashboard-{Guid.NewGuid():N}@example.test";
        Guid ownId;
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var own = await users.FindByEmailAsync(email);
            Assert.NotNull(own);
            ownId = own.Id;
            own.DisplayName = "Customer Alpha Private";
            Assert.True((await users.UpdateAsync(own)).Succeeded);
            var other = new ApplicationUser
            {
                Email = otherEmail,
                UserName = otherEmail,
                DisplayName = "Customer Beta Private"
            };
            Assert.True((await users.CreateAsync(other, Password)).Succeeded);
            extraUserId = other.Id;
            Assert.True((await users.AddToRoleAsync(other, ApplicationRoles.Customer)).Succeeded);
        }

        using var first = Client();
        using var second = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(first, email, Password)).StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await Login(second, otherEmail, Password)).StatusCode);
        var firstHtml = await first.GetStringAsync($"/dashboard?{queryKey}={extraUserId}");
        var secondHtml = await second.GetStringAsync($"/dashboard?{queryKey}={ownId}");
        Assert.Contains("Welcome, Customer Alpha Private!", firstHtml);
        Assert.DoesNotContain("Customer Beta Private", firstHtml);
        Assert.DoesNotContain(otherEmail, firstHtml);
        Assert.Contains("Welcome, Customer Beta Private!", secondHtml);
        Assert.DoesNotContain("Customer Alpha Private", secondHtml);
        Assert.DoesNotContain(email, secondHtml);
    }

    [Fact]
    public async Task Dashboard_BlankProfile_ShowsFallbackWelcomeAndFutureOrderState()
    {
        using var client = Client();
        await Login(client, email, Password);
        var html = await client.GetStringAsync("/dashboard");
        Assert.Contains("Welcome back!", html);
        Assert.Contains("Order history is coming soon", html);
        Assert.Contains("When ordering is available", html);
        Assert.DoesNotContain("We couldn", html);
    }

    [Fact]
    public async Task Dashboard_ProfileDeletedAfterLogin_ShowsSafeRetryInsteadOfPrivateData()
    {
        using var client = Client();
        await Login(client, email, Password);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                .Where(user => user.Email == email).ExecuteDeleteAsync();
        }
        var response = await client.GetAsync("/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Assert.Contains("We couldn’t load your account details. Please try again.", html);
        Assert.Contains("Reload dashboard", html);
        Assert.DoesNotContain(email, html);
        Assert.DoesNotContain("Npgsql", html);
        Assert.DoesNotContain("Exception", html);
    }

    [Theory]
    [InlineData("/dashboard")]
    [InlineData("/manager")]
    [InlineData("/admin")]
    public async Task Dashboard_AfterLogout_ChallengesBeforeRoleAuthorization(string path)
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/logout");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/logout", new FormUrlEncodedContent(fields))).StatusCode);
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task ProtectedPage_Anonymous_RedirectsToLogin()
    {
        using var client = Client();
        var response = await client.GetAsync("/session");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Restaurant_Foundation_RoundTripsAddressAndDefaultsClosed()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Foundation test",
            Slug = $"foundation-{Guid.NewGuid():N}",
            Address = "Test street",
            Area = "Test area",
            Cuisine = "Test cuisine",
            IsApproved = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        testRestaurantId = restaurant.Id;
        db.Restaurants.Add(restaurant);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var saved = await db.Restaurants.SingleAsync(row => row.Id == testRestaurantId);
        Assert.Equal("Test street", saved.Address);
        Assert.True(saved.IsVisible);
        Assert.False(saved.IsOpen);
        Assert.False(saved.CanAcceptOrders);
        saved.IsOpen = true;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        Assert.True((await db.Restaurants.SingleAsync(row => row.Id == testRestaurantId)).CanAcceptOrders);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("slug")]
    [InlineData("address")]
    public async Task Restaurant_InvalidFoundationFields_PostgresRejectsWrite(string field)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Constraint test",
            Slug = $"constraint-{Guid.NewGuid():N}",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        testRestaurantId = restaurant.Id;
        if (field == "name") restaurant.Name = "   ";
        if (field == "slug") restaurant.Slug = "   ";
        if (field == "address") restaurant.Address = new string('a', 501);
        db.Restaurants.Add(restaurant);
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        var provider = Assert.IsType<Npgsql.PostgresException>(exception.InnerException);
        Assert.Equal(field == "address" ? "22001" : "23514", provider.SqlState);
    }

    [Fact]
    public async Task Login_ValidCredentials_SetsSecureCookieAndAllowsProtectedPage()
    {
        using var client = Client();
        var response = await Login(client, email, Password, "//example.com");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/dashboard", response.Headers.Location?.ToString());
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_InvalidAndUnknownCredentials_ReturnSameSafeMessage()
    {
        using var client = Client();
        foreach (var address in new[] { email, "unknown@example.test" })
        {
            var response = await Login(client, address, "Wrong-Password123!");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Unable to sign in. Check your details or try again later.", await response.Content.ReadAsStringAsync());
            Assert.DoesNotContain(response.Headers.Where(header => header.Key == "Set-Cookie")
                .SelectMany(header => header.Value), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task Logout_ValidPost_ClearsCookieAndBlocksProtectedPage()
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/logout")).StatusCode);
        // GET renders confirmation; it must not sign the user out.
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
        var fields = await FormFields(client, "/logout");
        var response = await client.PostAsync("/logout", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/login", response.Headers.Location?.ToString());
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = Client();
        var response = await client.PostAsync("/login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "login",
            ["Input.Email"] = email,
            ["Input.Password"] = Password
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Logout_PostWithoutAntiforgeryToken_PreservesSession()
    {
        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        var response = await client.PostAsync("/logout", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "logout"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
    }

    [Fact]
    public async Task Login_RememberMe_CreatesPersistentCookie()
    {
        using var client = Client();
        var response = await Login(client, email, Password, rememberMe: true);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), value => value.StartsWith("LocalEats.Auth=", StringComparison.Ordinal));
        Assert.Contains("expires=", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_FiveFailures_LocksAccountAndKeepsResponseGeneric()
    {
        using var client = Client();
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Login(client, email, "Wrong-Password123!");
        }

        var response = await Login(client, email, Password);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Unable to sign in. Check your details or try again later.", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.True(await users.IsLockedOutAsync(user));
    }

    [Theory]
    [InlineData(ApplicationRoles.Customer, "/session/customer", true)]
    [InlineData(ApplicationRoles.Customer, "/dashboard", true)]
    [InlineData(ApplicationRoles.Manager, "/dashboard", false)]
    [InlineData(ApplicationRoles.Admin, "/dashboard", false)]
    [InlineData(ApplicationRoles.Customer, "/manager", false)]
    [InlineData(ApplicationRoles.Customer, "/manager/restaurant", false)]
    [InlineData(ApplicationRoles.Customer, "/manager/menu", false)]
    [InlineData(ApplicationRoles.Customer, "/manager/orders", false)]
    [InlineData(ApplicationRoles.Manager, "/manager/restaurant", true)]
    [InlineData(ApplicationRoles.Manager, "/manager/menu", true)]
    [InlineData(ApplicationRoles.Manager, "/manager/orders", true)]
    [InlineData(ApplicationRoles.Admin, "/manager/restaurant", false)]
    [InlineData(ApplicationRoles.Admin, "/manager/menu", false)]
    [InlineData(ApplicationRoles.Admin, "/manager/orders", false)]
    [InlineData(ApplicationRoles.Customer, "/admin", false)]
    [InlineData(ApplicationRoles.Customer, "/admin/restaurants", false)]
    [InlineData(ApplicationRoles.Customer, "/admin/users", false)]
    [InlineData(ApplicationRoles.Manager, "/admin/restaurants", false)]
    [InlineData(ApplicationRoles.Manager, "/admin/users", false)]
    [InlineData(ApplicationRoles.Admin, "/admin/restaurants", true)]
    [InlineData(ApplicationRoles.Admin, "/admin/users", true)]
    [InlineData(ApplicationRoles.Manager, "/session/customer", false)]
    [InlineData(ApplicationRoles.Manager, "/manager", true)]
    [InlineData(ApplicationRoles.Manager, "/admin", false)]
    [InlineData(ApplicationRoles.Admin, "/session/customer", false)]
    [InlineData(ApplicationRoles.Admin, "/manager", false)]
    [InlineData(ApplicationRoles.Admin, "/admin", true)]
    public async Task RolePages_SignedInRole_EnforcesPolicyOnServer(string role, string path, bool allowed)
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            Assert.True((await users.RemoveFromRoleAsync(user, ApplicationRoles.Customer)).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        }

        using var client = Client();
        Assert.Equal(HttpStatusCode.Redirect, (await Login(client, email, Password)).StatusCode);
        var response = await client.GetAsync(path);
        if (allowed)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var html = await response.Content.ReadAsStringAsync();
            if (path == "/admin")
            {
                Assert.Contains("Admin dashboard", html);
                Assert.Contains("Count not available yet", html);
                Assert.Contains("href=\"/admin/restaurants\"", html);
                Assert.Contains("href=\"/admin/users\"", html);
                Assert.Contains("href=\"/account\"", html);
                Assert.Contains("href=\"/logout\"", html);
            }
            else if (path.StartsWith("/admin/", StringComparison.Ordinal))
            {
                Assert.Contains("These tools are not available yet.", html);
                Assert.Contains("href=\"/admin\"", html);
                var heading = path == "/admin/users" ? "User management is coming soon" : "Restaurant management is coming soon";
                Assert.Contains($"<h1>{heading}</h1>", html);
            }
            else if (path == "/manager")
            {
                Assert.Contains("Manager dashboard", html);
                Assert.Contains("Assignment integration coming soon", html);
                Assert.Contains("Count not available yet", html);
                foreach (var route in new[] { "/manager/restaurant", "/manager/menu", "/manager/orders" })
                {
                    Assert.Contains($"href=\"{route}\"", html);
                }
            }
            else if (path.StartsWith("/manager/", StringComparison.Ordinal))
            {
                Assert.Contains("These tools are not available yet.", html);
                Assert.Contains("href=\"/manager\"", html);
                var heading = path switch
                {
                    "/manager/menu" => "Menu management is coming soon",
                    "/manager/orders" => "Order management is coming soon",
                    _ => "Restaurant setup is coming soon"
                };
                Assert.Contains($"<h1>{heading}</h1>", html);
            }
        }
        else
        {
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/access-denied", response.Headers.Location?.ToString());
        }
    }

    [Theory]
    [InlineData("/session/customer")]
    [InlineData("/dashboard")]
    [InlineData("/manager")]
    [InlineData("/manager/restaurant")]
    [InlineData("/manager/menu")]
    [InlineData("/manager/orders")]
    [InlineData("/admin")]
    [InlineData("/admin/restaurants")]
    [InlineData("/admin/users")]
    public async Task RolePages_Anonymous_RedirectsToLogin(string path)
    {
        using var client = Client();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task OrderOwnership_LoadedFromPostgres_DeniesChangedIdAndAllowsExplicitAdmin()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
        var now = DateTimeOffset.UtcNow;
        var restaurant = new Restaurant
        {
            Id = Guid.NewGuid(),
            Name = "Authorization test",
            Slug = $"authorization-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        testRestaurantId = restaurant.Id;
        var ownOrder = new Order
        {
            Id = Guid.NewGuid(),
            Restaurant = restaurant,
            CustomerId = user.Id,
            OrderNumber = $"OWN-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        var otherOrder = new Order
        {
            Id = Guid.NewGuid(),
            Restaurant = restaurant,
            CustomerId = Guid.NewGuid(),
            OrderNumber = $"OTHER-{Guid.NewGuid():N}",
            CreatedAt = now,
            UpdatedAt = now
        };
        database.Orders.AddRange(ownOrder, otherOrder);
        await database.SaveChangesAsync();
        var authorization = scope.ServiceProvider.GetRequiredService<IResourceAuthorizationService>();
        var customer = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user);

        Assert.True(await authorization.CanReadOrderAsync(customer, ownOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(customer, otherOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(customer, Guid.NewGuid()));
        Assert.True((await users.AddToRoleAsync(user, ApplicationRoles.Admin)).Succeeded);
        var admin = await scope.ServiceProvider.GetRequiredService<IUserClaimsPrincipalFactory<ApplicationUser>>().CreateAsync(user);
        Assert.True(await authorization.CanReadOrderAsync(admin, otherOrder.Id));
        Assert.False(await authorization.CanReadOrderAsync(new ClaimsPrincipal(), ownOrder.Id));
    }

    [Fact]
    public async Task Account_Anonymous_RedirectsToLogin()
    {
        using var client = Client();
        var response = await client.GetAsync("/account");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/login?ReturnUrl=", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Account_SaveProfile_PersistsAndKeepsSessionWithoutVerifyingPhone()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Test Account";
        fields["Input.PhoneNumber"] = "+8801712345678";
        fields["Input.PhoneNumberConfirmed"] = "true";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var page = await client.GetStringAsync(response.Headers.Location);
        Assert.Contains("Your profile was saved.", page);
        Assert.Contains("Test Account", page);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/session")).StatusCode);
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal("Test Account", user.DisplayName);
        Assert.Equal("+8801712345678", user.PhoneNumber);
        Assert.False(user.PhoneNumberConfirmed);
        Assert.Equal(email, user.UserName);
    }

    [Fact]
    public async Task Account_InvalidPhone_DoesNotPersistAnyProfileChanges()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Should not save";
        fields["Input.PhoneNumber"] = "not-a-phone";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Enter a valid phone number", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(string.Empty, user.DisplayName);
        Assert.Null(user.PhoneNumber);
    }

    [Fact]
    public async Task Account_ChangedUserIdAndEmail_UpdateOnlySignedInProfile()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var otherEmail = $"other-{Guid.NewGuid():N}@example.test";
            var other = new ApplicationUser { UserName = otherEmail, Email = otherEmail, DisplayName = "Other user" };
            Assert.True((await users.CreateAsync(other, Password)).Succeeded);
            extraUserId = other.Id;
        }

        using var client = Client();
        await Login(client, email, Password);
        var path = $"/account?userId={extraUserId}";
        var fields = await FormFields(client, path);
        fields["Input.UserId"] = extraUserId.ToString() ?? string.Empty;
        fields["Input.Email"] = "changed@example.test";
        fields["Input.DisplayName"] = "My name";
        fields["Input.PhoneNumber"] = string.Empty;
        var response = await client.PostAsync(path, new FormUrlEncodedContent(fields));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        await using var verify = Factory.Services.CreateAsyncScope();
        var manager = verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var current = await manager.FindByEmailAsync(email);
        Assert.NotNull(current);
        Assert.Equal("My name", current.DisplayName);
        Assert.Equal(email, current.Email);
        var otherUser = await manager.FindByIdAsync(extraUserId.ToString() ?? string.Empty);
        Assert.NotNull(otherUser);
        Assert.Equal("Other user", otherUser.DisplayName);
    }

    [Fact]
    public async Task Account_StaleForm_DoesNotOverwriteNewerProfile()
    {
        using var client = Client();
        await Login(client, email, Password);
        var first = await FormFields(client, "/account");
        var stale = await FormFields(client, "/account");
        first["Input.DisplayName"] = "First edit";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(first))).StatusCode);
        stale["Input.DisplayName"] = "Stale edit";
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(stale));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("changed in another session", await response.Content.ReadAsStringAsync());
        await using var scope = Factory.Services.CreateAsyncScope();
        var user = await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal("First edit", user.DisplayName);
    }

    [Fact]
    public async Task Account_PostWithoutAntiforgeryToken_IsRejected()
    {
        using var client = Client();
        await Login(client, email, Password);
        var response = await client.PostAsync("/account", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["_handler"] = "profile",
            ["Input.DisplayName"] = "Unprotected change"
        }));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Account_DeletedUser_ShowsSafeLoadError()
    {
        using var client = Client();
        await Login(client, email, Password);
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                .Where(user => user.Email == email).ExecuteDeleteAsync();
        }

        var response = await client.GetAsync("/account");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("We couldn't load your profile", WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()));
    }

    [Fact]
    public async Task Account_ChangedPreviouslyVerifiedPhone_RequiresVerificationAgain()
    {
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            user.PhoneNumber = "+8801712345678";
            user.PhoneNumberConfirmed = true;
            Assert.True((await users.UpdateAsync(user)).Succeeded);
        }

        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "Changed phone";
        fields["Input.PhoneNumber"] = "+8801812345678";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(fields))).StatusCode);
        await using var verify = Factory.Services.CreateAsyncScope();
        var current = await verify.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email);
        Assert.NotNull(current);
        Assert.False(current.PhoneNumberConfirmed);
        Assert.Equal("+8801812345678", current.PhoneNumber);
    }

    [Fact]
    public async Task Account_HtmlInName_RendersAsText()
    {
        using var client = Client();
        await Login(client, email, Password);
        var fields = await FormFields(client, "/account");
        fields["Input.DisplayName"] = "<script>alert(1)</script>";
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/account", new FormUrlEncodedContent(fields))).StatusCode);
        var html = await client.GetStringAsync("/account");
        Assert.Contains("&lt;script&gt;", html);
        Assert.DoesNotContain("<script>alert(1)</script>", html);
    }

    private WebApplicationFactory<WebEntryPoint> Factory => factory ?? throw new InvalidOperationException("Not initialized.");

    private HttpClient Client() => Factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("https://localhost")
    });

    private static async Task<HttpResponseMessage> Login(HttpClient client, string address, string password, string returnUrl = "/session", bool rememberMe = false)
    {
        var path = "/login?ReturnUrl=" + Uri.EscapeDataString(returnUrl);
        var fields = await FormFields(client, path);
        fields["Input.Email"] = address;
        fields["Input.Password"] = password;
        fields["Input.RememberMe"] = rememberMe.ToString();
        return await client.PostAsync(path, new FormUrlEncodedContent(fields));
    }

    private static async Task<Dictionary<string, string>> FormFields(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();
        var fields = new Dictionary<string, string>();
        foreach (Match match in Regex.Matches(html, "<input[^>]*type=\"hidden\"[^>]*>"))
        {
            var name = Regex.Match(match.Value, "name=\"([^\"]*)\"").Groups[1].Value;
            var value = Regex.Match(match.Value, "value=\"([^\"]*)\"").Groups[1].Value;
            fields[WebUtility.HtmlDecode(name)] = WebUtility.HtmlDecode(value);
        }

        Assert.Contains("__RequestVerificationToken", fields.Keys);
        Assert.Contains("_handler", fields.Keys);
        return fields;
    }

    public async Task DisposeAsync()
    {
        if (factory is null)
        {
            return;
        }

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Restaurants
                .Where(row => directoryIds.Contains(row.Id)).ExecuteDeleteAsync();
            await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                .Where(user => user.Email == email).ExecuteDeleteAsync();
            if (extraUserId is Guid otherUserId)
            {
                await scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>().Users
                    .Where(user => user.Id == otherUserId).ExecuteDeleteAsync();
            }
            if (testRestaurantId is Guid restaurantId)
            {
                var database = scope.ServiceProvider.GetRequiredService<LocalEatsDbContext>();
                await database.Orders.Where(order => order.RestaurantId == restaurantId).ExecuteDeleteAsync();
                await database.Restaurants.Where(restaurant => restaurant.Id == restaurantId).ExecuteDeleteAsync();
            }
        }

        await factory.DisposeAsync();
    }
}
