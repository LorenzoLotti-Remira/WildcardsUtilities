namespace WildcardsUtilities.Scanner;

public class EnsureCreatedService<TDbContext>(TDbContext dbContext) : BackgroundService
    where TDbContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
        await EnsureCreatedAsync(dbContext, stoppingToken).ConfigureAwait(false);

    private static async ValueTask<bool> EnsureCreatedAsync
    (
        DbContext dbContext,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var result = await dbContext
                .Database
                .EnsureCreatedAsync(cancellationToken)
                .ConfigureAwait(false);

            var views =
                from prop in typeof(TDbContext).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                where
                (
                    prop.PropertyType.IsGenericType &&
                    prop.PropertyType.GetGenericTypeDefinition() == typeof(IQueryable<>)
                )
                let query = (prop.GetValue(dbContext) as IQueryable)!.ToQueryString()
                select $"create view {prop.Name} as {query}";

            foreach (var view in views)
            {
                await dbContext
                    .Database
                    .ExecuteSqlRawAsync(view, cancellationToken)
                    .ConfigureAwait(false);
            }

            return result;
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
        (this IServiceCollection services) where TDbContext : DbContext =>
            services.AddHostedService<EnsureCreatedService<TDbContext>>();
}
