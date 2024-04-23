using Microsoft.EntityFrameworkCore.Infrastructure;

namespace WildcardsUtilities.Scanner;

public class EnsureCreatedService<TDbContext>(TDbContext dbContext) : BackgroundService
    where TDbContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await SafeEnsureCreatedAsync(dbContext.Database, stoppingToken);

    private static async ValueTask<bool> SafeEnsureCreatedAsync
    (
        DatabaseFacade db,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await db.EnsureCreatedAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }
}

public static class ServiceCollectionEnsureCreatedServiceExtensions
{
    public static IServiceCollection AddEnsureCreatedService<TDbContext>
        (this IServiceCollection serviceCollection) where TDbContext : DbContext =>
            serviceCollection.AddHostedService<EnsureCreatedService<TDbContext>>();
}
