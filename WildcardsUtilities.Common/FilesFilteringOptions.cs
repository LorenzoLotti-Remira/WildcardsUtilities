namespace WildcardsUtilities.Common;

public sealed record FilesFilteringOptions
{
    public ImmutableArray<string> Filters { get; init; } =
        [Wildcards.PathWildcard + Wildcards.PathSeparator + Wildcards.StringWildcard];

    public string Root { get; init; } = Wildcards.PathSeparator;
    public bool ComputeChecksum { get; init; } = false;
}
