namespace WildcardsUtilities.Scanner.Extensions;

public interface IRelationalDbContextOptionsBuilderLike
{
    IRelationalDbContextOptionsBuilderLike CommandTimeout(int? commandTimeout);

    IRelationalDbContextOptionsBuilderLike ExecutionStrategy
        (Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy);

    IRelationalDbContextOptionsBuilderLike MaxBatchSize(int maxBatchSize);
    IRelationalDbContextOptionsBuilderLike MigrationsAssembly(string? assemblyName);

    IRelationalDbContextOptionsBuilderLike MigrationsHistoryTable
        (string tableName, string? schema = null);

    IRelationalDbContextOptionsBuilderLike MinBatchSize(int minBatchSize);

    IRelationalDbContextOptionsBuilderLike UseQuerySplittingBehavior
        (QuerySplittingBehavior querySplittingBehavior);

    IRelationalDbContextOptionsBuilderLike UseRelationalNulls(bool useRelationalNulls = true);
}
