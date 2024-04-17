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

        var expectedMethodName = nameof(Use) + provider;

        var optionsActionParamName =
            nameof(providerOptionsAction).Replace(nameof(provider), provider);

        var x = typeof(DbContextOptionsBuilder).EnumerateExtensionMethods();

        var targetMethod =
        (
            from method in typeof(DbContextOptionsBuilder).EnumerateExtensionMethods()
            let parameters = method.GetParameters()
            where
            (
                method.Name.Equals(expectedMethodName, StringComparison.OrdinalIgnoreCase) &&
                parameters.Length >= 3 &&
                parameters.Skip(3).All(p => p.IsOptional) &&
                parameters[1].Name == nameof(connectionString) &&
                parameters[1].ParameterType == typeof(string) &&
                parameters[2].Name!.Equals(optionsActionParamName, StringComparison.OrdinalIgnoreCase) &&
                parameters[2].ParameterType.GetGenericTypeDefinition() == typeof(Action<>)
            )
            orderby parameters.Length
            select method
        )
        .FirstOrDefault() ?? throw new ArgumentException
        (
            $"'{provider}' database provider is not supported",
            nameof(provider)
        );

        targetMethod.Invoke
        (
            null,
            [
                builder,
                connectionString,

                (dynamic options) => providerOptionsAction?.Invoke
                    (new RelationalDbContextOptionsBuilderDynamicWrapper(options))
            ]
        );

        return builder;
    }
}
