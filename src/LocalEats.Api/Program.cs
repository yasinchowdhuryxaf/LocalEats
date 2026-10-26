using LocalEats.Infrastructure;
using LocalEats.Infrastructure.Persistence.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

await app.Services.SeedDevelopmentDataAsync(app.Environment.IsDevelopment());

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.Run();
