namespace WildcardsUtilities.Common;

public sealed record FilesFilteringOptions
{
    public ImmutableArray<string> Filters { get; init; } =
        [FilesFilteringCore.PathWildcard + FilesFilteringCore.PathSeparator + FilesFilteringCore.StringWildcard];

    public string Root { get; init; } = FilesFilteringCore.PathSeparator;
    public bool ComputeChecksum { get; init; } = false;
}
