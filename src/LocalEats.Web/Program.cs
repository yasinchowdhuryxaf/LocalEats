using LocalEats.Application.Basket;
using LocalEats.Application.Restaurants;
using LocalEats.Application.Users;
using LocalEats.Application.Users.Profile;
using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Persistence.Seed;
using LocalEats.Web.Components;
using LocalEats.Web.State;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<RestaurantDirectoryService>();
builder.Services.AddScoped<RestaurantMenuService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "LocalEats.BasketSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});
builder.Services.AddScoped<IBasketStateStore, SessionBasketStore>();
builder.Services.AddScoped<BasketService>();
if (!builder.Environment.IsDevelopment())
{
    builder.Services.ConfigureApplicationCookie(options =>
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always);
}

var app = builder.Build();

await app.Services.SeedDevelopmentDataAsync(app.Environment.IsDevelopment());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();
app.Use(async (context, next) =>
{
    if (!Path.HasExtension(context.Request.Path.Value) && !context.Request.Path.StartsWithSegments("/_framework")
        && !context.Request.Path.StartsWithSegments("/health"))
    {
        context.Response.Headers.CacheControl = "no-store";
        if (context.Session.GetString("BasketReady") is null) context.Session.SetString("BasketReady", "1");
    }
    await next(context);
});
app.UseAntiforgery();

app.MapHealthChecks("/health");

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
