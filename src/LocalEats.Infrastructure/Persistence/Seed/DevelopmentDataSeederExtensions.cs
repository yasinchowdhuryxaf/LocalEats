namespace LocalEats.Infrastructure.Persistence.Seed;

public static class DevelopmentDataSeederExtensions
{
    public static Task SeedDevelopmentDataAsync(
        this IServiceProvider services,
        bool isDevelopment,
        CancellationToken cancellationToken = default)
    {
        return isDevelopment
            ? DevelopmentDataSeeder.SeedAsync(services, cancellationToken)
            : Task.CompletedTask;
    }
}
