namespace WildcardsUtilities;
                                    
public static class FilesFiltering
{
    /// <summary>
    /// Yields every file that matches a specified set of wildcard filters.
    /// </summary>
    /// <param name="filters">A set of filters that may contain wildcards.</param>
    /// <param name="root">The base directory from which every file is taken.</param>
    /// <returns>Every matching file.</returns>
    /// <exception cref="ArgumentNullException"/>
    /// <exception cref="ArgumentException"/>
    /// <exception cref="DirectoryNotFoundException"/>
    public static IEnumerable<Common.ChecksummedFileInfo> GetFiles(Common.FilesFilteringOptions options) =>
        Common.FilesFilteringCore.GetFiles
        (
            options,
            Enumerable.SelectMany,
            Enumerable.SelectMany,
            GetFiles,
            fileInfo =>
            {
                try
                {
                    using var file = File.OpenRead(fileInfo.Path);
                    return fileInfo with { Checksum = [.. SHA1.HashData(file)] };
                }
                catch
                {
                    return fileInfo;
                }
            },
            fileInfo => options.ComputeChecksum
        );
}