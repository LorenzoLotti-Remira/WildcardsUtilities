namespace WildcardsUtilities.Common;

public sealed record FilesFilteringOptions
{
    public ImmutableArray<string> Filters { get; init; } =
        [FilesFiltering.PathWildcard + FilesFiltering.PathSeparator + FilesFiltering.StringWildcard];

    public string Root { get; init; } = FilesFiltering.PathSeparator;
    public bool ComputeChecksum { get; init; } = false;
}
