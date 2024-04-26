using static WildcardsUtilities.Common.Wildcards;
namespace WildcardsUtilities.Common;

public static class FilesFilteringCore
{
    private static readonly EnumerationOptions _enumerationOptions = new()
    {
        IgnoreInaccessible = true,
        AttributesToSkip = FileAttributes.ReparsePoint
    };

    public static IEnumerable<FileMetadata> GetFiles
    (
        FilesFilteringOptions options,
        FlatMap<string, FileMetadata> inclusiveFolderNameFilterToFileFlatMap,
        FlatMap<DirectoryInfo, FileMetadata> directoryToFileFlatMap,
        Func<FilesFilteringOptions, IEnumerable<FileMetadata>> getNextFilesFunc,
        Func<FileMetadata, FileMetadata>? filesMappingFunc = null,
        Predicate<FileMetadata>? filesMappingPredicate = null
    )
    {
        ArgumentNullException.ThrowIfNull(options.Filters);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Root);

        if (!Directory.Exists(options.Root))
        {
            throw new DirectoryNotFoundException
                ($"The specified '{nameof(options.Root)}' is not a targetDir.");
        }

        if (options.Filters.Length == 0)
            return [];

        filesMappingFunc ??= file => file;
        filesMappingPredicate ??= _ => true;

        // Splits the given filters into names.
        var splittedFilters = options.Filters.SelectMany(filter =>
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

        // Retrievs the new filters that will be applied to matching folders
        // in the next recursive iteration.
        var folderNameRegexesWithNewFilters =
            from s in splittedDirFilters
            let negation = s.Excludes ? ExclusionPrefix : string.Empty
            let joinStartIndex = s.Names[0] == PathWildcard ? 0 : 1
            let newFilter = PathSeparator + string.Join
            (
                PathSeparator,
                s.Names,
                joinStartIndex,
                s.Names.Length - joinStartIndex
            )
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

        var filesInFolders = inclusiveFolderNameFilterToFileFlatMap
        (
            inclusiveFolderNameFilters,
            filter => GetFilesByFolderFilter
            (
                options.Root,
                filter,
                folderNameRegexesWithNewFilters,
                options,
                directoryToFileFlatMap,
                getNextFilesFunc
            )
        );

        // Retrieves filtered files inside the root directory.
        var files = inclusiveFileNameFilters.SelectMany
        (
            filter => GetFilesByFileFilter
            (
                options.Root,
                filter,
                exclusiveFileNameRegexes,
                filesMappingFunc,
                filesMappingPredicate
            )
        );

        // Removes duplicates.
        return files.Concat(filesInFolders).DistinctBy(f => f.Path);
    }

    // Retrieves the filters to be applied inside a specified folder. 
    internal static ImmutableArray<string> GetNewFilters
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
    .ToImmutableArray();

    internal static IEnumerable<FileMetadata> GetFilesByFolderFilter
    (
        string rootDirectory,
        string folderNameFilter,
        IEnumerable<(Regex Regex, string NewFilter)> folderNameRegexesWithNewFilters,
        FilesFilteringOptions originalOptions,
        FlatMap<DirectoryInfo, FileMetadata> directoryToFileFlatMap,
        Func<FilesFilteringOptions, IEnumerable<FileMetadata>> getNextFilesFunc
    )
    {
        var regex = ToRegex(folderNameFilter);

        IEnumerable<string> paths;

        if (HasWildcards(folderNameFilter))
        {
            if (Directory.Exists(rootDirectory))
                paths = Directory.EnumerateDirectories(rootDirectory, "*", _enumerationOptions);
            else
                paths = [];
        }
        else
            paths = [rootDirectory + PathSeparator + folderNameFilter];

        var dirs =
            from path in paths
            let dir = new DirectoryInfo(path)
            where dir.Exists && regex.IsMatch(dir.Name)
            select dir;

        return directoryToFileFlatMap
        (
            dirs,
            dir => getNextFilesFunc
            (
                originalOptions with
                {
                    Filters = GetNewFilters(dir.Name, folderNameRegexesWithNewFilters),
                    Root = dir.FullName
                }
            )
        );
    }

    // Yields every file in the given directory that matches with the given file name filter
    // and that complies with the given exclusions.
    internal static IEnumerable<FileMetadata> GetFilesByFileFilter
    (
        string directory,
        string fileNameFilter,
        IEnumerable<Regex> exclusiveRegexes,
        Func<FileMetadata, FileMetadata> filesMappingFunc,
        Predicate<FileMetadata> filesMappingPredicate
    )
    {
        var regex = ToRegex(fileNameFilter);

        IEnumerable<string> paths;

        if (HasWildcards(fileNameFilter))
        {
            if (Directory.Exists(directory))
                paths = Directory.EnumerateFiles(directory, "*", _enumerationOptions);
            else
                paths = [];
        }
        else
            paths = [directory + PathSeparator + fileNameFilter];

        return
            from path in paths
            let fileInfo = new FileInfo(path)
            where
                fileInfo.Exists &&
                regex.IsMatch(fileInfo.Name) &&
                !exclusiveRegexes.AnyMatch(fileInfo.Name)
            let metadata = new FileMetadata(path, fileInfo.Attributes, fileInfo.Length)
            select filesMappingPredicate(metadata) ? filesMappingFunc(metadata) : metadata;
    }
}
