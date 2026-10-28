using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace LocalEats.Infrastructure.Persistence;

public sealed class LocalEatsDbContextFactory : IDesignTimeDbContextFactory<LocalEatsDbContext>
{
    public LocalEatsDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=127.0.0.1;Port=15432;Database=localeats;Username=localeats;Password=localeats_dev_password";

        var options = new DbContextOptionsBuilder<LocalEatsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LocalEatsDbContext(options);
    }
}
