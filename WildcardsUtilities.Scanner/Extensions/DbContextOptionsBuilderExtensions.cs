namespace WildcardsUtilities.Scanner.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder Use
    (
        this DbContextOptionsBuilder builder,
        string provider,
        string? connectionString,
        Action<IRelationalDbContextOptionsBuilderLike>? providerOptionsAction = null
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);

        void InvokeWrapped(dynamic options) =>
            providerOptionsAction?.Invoke
                (new RelationalDbContextOptionsBuilderDynamicWrapper(options));

        try
        {
            return provider.ToLower() switch
            {
                "sqlite" => builder.UseSqlite
                (
                    connectionString,
                    options => InvokeWrapped(options)
                ),

                "sqlserver" => builder.UseSqlServer
                (
                    connectionString,
                    options => InvokeWrapped(options)
                ),

                "mysql" or "mariadb" => builder.UseMySql
                (
                    connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    options => InvokeWrapped(options)
                ),

                _ => throw new NotImplementedException()
            };
        }
        catch
        {
            throw new ArgumentException
            (
                $"'{provider}' database provider is not supported",
                nameof(provider)
            );
        }
    }
}
