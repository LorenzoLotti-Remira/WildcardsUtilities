namespace WildcardsUtilities;
                                    
public static class FilesFiltering
{
    internal const string
        ExclusionPrefix = "!",
        PathSeparator = "/",
        CharWildcard = "?",
        StringWildcard = "*",
        PathWildcard = "**";

    /// <summary>
    /// Yields every file that matches a specified set of wildcard filters.
    /// </summary>
    /// <param name="filters">A set of filters that may contain wildcards.</param>
    /// <param name="root">The base directory from which every file is taken.</param>
    /// <returns>Every matching file.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    public static IEnumerable<FileInfo> GetFiles(string[] filters, string root)
    {
        ArgumentNullException.ThrowIfNull(filters);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        if (!Directory.Exists(root))
            throw new DirectoryNotFoundException($"The specified '{nameof(root)}' is not a targetDir.");

        if (filters.Length == 0)
            return [];

        // Splits the given filters into names.
        var splittedFilters = filters.SelectMany(filter =>
        {
            var excludes = filter.StartsWith(ExclusionPrefix);
            filter = excludes ? filter[1..] : filter;

            var splittedFilter =
            (
                Names: filter.Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries),
                Excludes: excludes
            );

            IList<(string[] Names, bool Excludes)> results = [splittedFilter];

            if (splittedFilter.Names[0] == PathWildcard)
                results.Add(splittedFilter with { Names = splittedFilter.Names[1..] });

            return results;
        });

        // Retrieves the file name filters.
        var fileNameFilters =
        (
            from s in splittedFilters
            where s.Names.Length == 1
            let negation = s.Excludes ? ExclusionPrefix : string.Empty
            select negation + s.Names[0]
        )
        .Distinct();

        // Retrieves the inclusive file name filters.
        var inclusiveFileNameFilters = fileNameFilters.Where(f => !f.StartsWith(ExclusionPrefix));

        // Retrieves the exclusive file name filters converted to regexes.
        var exclusiveFileNameRegexes =
            from f in fileNameFilters
            where f.StartsWith(ExclusionPrefix)
            select ToRegex(f);

        // Retrieves splitted filters which require further recursive operations.
        var splittedDirFilters = splittedFilters.Where(s => s.Names.Length > 1);

        // Retrievs the new filters that will be applied to matching folders in the next recursive iteration.
        var folderNameRegexesWithNewFilters =
            from s in splittedDirFilters
            let negation = s.Excludes ? ExclusionPrefix : string.Empty
            let joinStartIndex = s.Names[0] == PathWildcard ? 0 : 1
            let newFilter = PathSeparator + string.Join(PathSeparator, s.Names, joinStartIndex, s.Names.Length - joinStartIndex)
            select
            (
                Regex: ToRegex(s.Names[0]),
                NewFilter: negation + newFilter
            );

        // Retrieves the inclusive folder name filters.
        var inclusiveFolderNameFilters =
        (
            from s in splittedDirFilters
            where !s.Excludes
            select s.Names[0]
        )
        .Distinct();
        
        // Retrieves filtered files inside every matching directories inside the root by using recursion.
        var filesInFolders = inclusiveFolderNameFilters
            .SelectMany(filter => GetFilesByFolderFilter(root, filter, folderNameRegexesWithNewFilters));

        // Retrieves filtered files inside the root directory.
        var files = inclusiveFileNameFilters
            .SelectMany(filter => GetFilesByFileFilter(root, filter, exclusiveFileNameRegexes));

        // Removes duplicates.
        return files.Concat(filesInFolders).DistinctBy(f => f.FullName);
    }

    // Retrieves the filters to be applied inside a specified folder. 
    internal static string[] GetNewFilters
    (
        string folderName,
        IEnumerable<(Regex Regex, string NewFilter)> folderNameRegexesWithNewFilters
    )
    =>
    (
        from f in folderNameRegexesWithNewFilters
        where f.Regex.IsMatch(folderName)
        select f.NewFilter
    )
    .ToArray();

    // Yields every file inside every directory that matches with the given folder name filter
    // and applying the new filters.
    internal static IEnumerable<FileInfo> GetFilesByFolderFilter
    (
        string rootDirectory,
        string folderNameFilter,
        IEnumerable<(Regex Regex, string NewFilter)> folderNameRegexesWithNewFilters
    )
    {
        if (HasWildcards(folderNameFilter))
        {
            var regex = ToRegex(folderNameFilter);

            var dirs =
                from path in Directory.EnumerateDirectories(rootDirectory)
                let dir = new DirectoryInfo(path)
                where regex.IsMatch(dir.Name)
                select dir;

            // For each directory retrieves matching files.
            return dirs.SelectMany
            (
                dir =>
                    GetFiles(GetNewFilters(dir.Name, folderNameRegexesWithNewFilters), dir.FullName)
            );
        }


        // Directory targeting optimization.

        var targetDir = new DirectoryInfo(rootDirectory + PathSeparator + folderNameFilter);

        return targetDir.Exists ?
            GetFiles(GetNewFilters(targetDir.Name, folderNameRegexesWithNewFilters), targetDir.FullName) :
            [];
    }

    // Yields every file in the given directory that matches with the given file name filter
    // and that complies with the given exclusions.
    internal static IEnumerable<FileInfo> GetFilesByFileFilter
    (
        string directory,
        string fileNameFilter,
        IEnumerable<Regex> exclusiveRegexes
    )
    {
        if (HasWildcards(fileNameFilter))
        {
            var regex = ToRegex(fileNameFilter);

            // Retrieves matching files.
            return
                from path in Directory.EnumerateFiles(directory)
                let file = new FileInfo(path)
                where regex.IsMatch(file.Name) && !exclusiveRegexes.AnyMatch(file.Name)
                select file;
        }


        // File targeting optimization.

        var targetFile = new FileInfo(directory + PathSeparator + fileNameFilter);

        return targetFile.Exists && !exclusiveRegexes.AnyMatch(targetFile.Name) ?
            [targetFile] : [];
    }

    // Returns true if the specified filter contains at least one wildcard symbol, otherwise false.
    internal static bool HasWildcards(string filter) =>
        filter.Contains(CharWildcard) || filter.Contains(StringWildcard) || filter.Contains(PathWildcard);

    // Returns true if at least one of the provided regexes matches with the specified input,
    // otherwise false.
    internal static bool AnyMatch(this IEnumerable<Regex> regexes, string input) =>
        regexes.Any(regex => regex.IsMatch(input));

    // Converts a file/folder name filter to a regex.
    internal static Regex ToRegex(string nameFilter)
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

        // ^ at the start and $ at the end indicate that the filter should be matched for the whole input.
        // The regex will also match strings with or without the path separator at the start.
        return new($"^[{PathSeparator}]?{regexFilter}$");
    }
}