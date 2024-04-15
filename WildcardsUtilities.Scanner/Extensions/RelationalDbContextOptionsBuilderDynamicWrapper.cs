namespace WildcardsUtilities.Scanner.Extensions;

internal sealed class RelationalDbContextOptionsBuilderDynamicWrapper(dynamic wrappedObject) :
    IRelationalDbContextOptionsBuilderLike
{
    private IRelationalDbContextOptionsBuilderLike With(Action<dynamic> optionsAction)
    {
        try
        {
            optionsAction(wrappedObject);
            return this;
        }
        catch
        {
            throw new NotSupportedException();
        }
    }

    public IRelationalDbContextOptionsBuilderLike CommandTimeout(int? commandTimeout) =>
        With(options => options.CommandTimeout(commandTimeout));

    public IRelationalDbContextOptionsBuilderLike ExecutionStrategy
        (Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy) =>
            With(options => options.ExecutionStrategy(getExecutionStrategy));

    public IRelationalDbContextOptionsBuilderLike MaxBatchSize(int maxBatchSize) =>
        With(options => options.MaxBatchSize(maxBatchSize));

    public IRelationalDbContextOptionsBuilderLike MigrationsAssembly(string? assemblyName) =>
        With(options => options.MigrationsAssembly(assemblyName));

    public IRelationalDbContextOptionsBuilderLike MigrationsHistoryTable
        (string tableName, string? schema = null) =>
            With(options => options.MigrationsHistoryTable(tableName, schema));

    public IRelationalDbContextOptionsBuilderLike MinBatchSize(int minBatchSize) =>
        With(options => options.MinBatchSize(minBatchSize));


    public IRelationalDbContextOptionsBuilderLike UseQuerySplittingBehavior
        (QuerySplittingBehavior querySplittingBehavior) =>
            With(options => options.UseQuerySplittingBehavior(querySplittingBehavior));

    public IRelationalDbContextOptionsBuilderLike UseRelationalNulls
        (bool useRelationalNulls = true) =>
            With(options => options.UseRelationalNulls(useRelationalNulls));
}