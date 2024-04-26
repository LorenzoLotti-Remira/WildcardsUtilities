namespace WildcardsUtilities.Common;

public static class Wildcards
{
    public const string
        ExclusionPrefix = "!",
        PathSeparator = "/",
        CharWildcard = "?",
        StringWildcard = "*",
        PathWildcard = "**";

    // Returns true if the specified filter contains at least one wildcard symbol, otherwise false.
    public static bool HasWildcards(string filter) =>
        filter.Contains(CharWildcard) ||
        filter.Contains(StringWildcard) ||
        filter.Contains(PathWildcard);

    // Converts a file/folder name filter to a regex.
    public static Regex ToRegex(string nameFilter)
    {
        const string CharRegex = $"[^{PathSeparator}]?";
        const string StringRegex = $"[^{PathSeparator}]*";

        // Removes the unnecessary initial exclusion prefix if present.
        if (nameFilter.StartsWith(ExclusionPrefix))
            nameFilter = nameFilter[1..];

        // Removes the unnecessary initial path separator if present.
        if (nameFilter.StartsWith(PathSeparator))
            nameFilter = nameFilter[1..];

        var regexFilter = Regex
            .Escape(nameFilter)  // Escapes the filter to exclude special regex characters for future matching operations.
            .Replace(Regex.Escape(PathWildcard), StringRegex)  // Replaces the eascaped path wildcard with a regex that matches any folder name.
            .Replace(Regex.Escape(StringWildcard), StringRegex)  // Replaces the escaped string wildcard with its regex version.
            .Replace(Regex.Escape(CharWildcard), CharRegex);  // Replaces the escaped char wildcard with its regex version.

        // ^ at the start and $ at the end indicate that the filter should be matched
        // for the whole input.
        // The regex will also match strings with or without the path separator at the start.
        return new($"^[{PathSeparator}]?{regexFilter}$");
    }
}
