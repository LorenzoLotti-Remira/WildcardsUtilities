namespace WildcardsUtilities.Tests;

public class FilesFilteringTests
{
    public static readonly IEnumerable<object[]> GetFiles_should_work_DATA =
    [
        [
            new string[]
            {
                "foo/*.*",
                "!foo/I*.*"
            },

            new string[]
            {
                "foo/file.cs",
                "foo/file.fs",
                "foo/test.txt",
                "foo/test_0.txt",
            }
        ],
        [
            new string[]
            {
                "foo/*.*"
            },

            new string[]
            {
                "foo/file.cs",
                "foo/file.fs",
                "foo/test.txt",
                "foo/test_0.txt",
                "foo/Index.cshtml.css"
            }
        ],
        [
            new string[]
            {
                "foo/*.*",
                "!foo/I*.*",
                "**/test_?.txt",
            },

            new string[]
            {
                "foo/file.cs",
                "foo/file.fs",
                "foo/test.txt",
                "foo/test_0.txt",
                "Hello/World/test_2.txt",
                "Hxhxhx/test_2.txt",
                "test_0.txt",
                "test_1.txt",
                "test_2.txt"
            }
        ],
        [
            new string[]
            {
                "foo/*.*",
                "!foo/I*.*",
                "**/test_?.txt",
            },

            new string[]
            {
                "foo/file.cs",
                "foo/file.fs",
                "foo/test.txt",
                "foo/test_0.txt",
                "Hello/World/test_2.txt",
                "Hxhxhx/test_2.txt",
                "test_0.txt",
                "test_1.txt",
                "test_2.txt"
            }
        ],
        [
            new string[]
            {
                "Hello/**/archive?*",
                "!H*/**/*fhwe*",
                "**/*hx*/t*_*.txt"
            },

            new string[]
            {
                "Hello/World/archive.zip",
                "Hello/World/archive_yfewq78fgue.zip",
                "Hxhxhx/test_2.txt"
            }
        ],
        [

            new string[]
            {
                "Hello/**/World/*.html"
            },

            new string[]
            {
                "Hello/Brother/Hi/World/World.html",
                "Hello/World/index.html"
            }
        ],
        [

            new string[]
            {
                "Hello/**/World/*.html",
                "!Hello/**/*/World/*.html"
            },

            new string[]
            {
                "Hello/World/index.html"
            }
        ],
        [

            new string[]
            {
                "**/prefix*",
                "!**/prefix?",
                "!H*/prefix*H*"
            },

            new string[]
            {
                "Hello/prefixUEWGIUFG"
            }
        ],
        [

            new string[]
            {
                "**/prefix*",
                "!**/prefix?",
                "H*/prefix*H*"
            },

            new string[]
            {
                "Hello/prefixUEWGIUFG",
                "Hello/prefixHOUFHERO"
            }
        ]
    ];
    /*
    [Theory]
    [MemberData(nameof(GetFiles_should_work_DATA))]
    public void GetFiles_should_work(string[] filters, string[] expected)
    {
        var root = $"{Environment.CurrentDirectory}/TestRootFolder";

        FilesFiltering
            .GetFiles(filters, root)
            .Select(file => Path.GetRelativePath(root, file.FullName).Replace('\\', '/'))
            .Should()
            .BeEquivalentTo(expected);
    }
    */
    [Theory]
    [MemberData(nameof(GetFiles_should_work_DATA))]
    public async Task GetFilesAsync_should_work(string[] filters, string[] expected)
    {
        var root = $"{Environment.CurrentDirectory}/TestRootFolder";

        var x = (await Async.FilesFiltering.GetFilesAsync(new() { Filters = [.. filters], Root = root }));
            x
            .Select(file => Path.GetRelativePath(root, file.Path).Replace('\\', '/'))
            .Should()
            .BeEquivalentTo(expected);
    }

    //[Fact(Skip = "Too slow")]
    //public async Task GetFilesAsync_should_be_faster_than_GetFiles()
    //{
    //    string
    //        Filter = "/**/*",
    //        Root = "/";

    //    var getFilesAsyncStopwatch = Stopwatch.StartNew();
    //    _ = (await Async.FilesFiltering.GetFilesAsync([Filter], Root)).ToArray();
    //    getFilesAsyncStopwatch.Stop();

    //    var getFilesStopwatch = Stopwatch.StartNew();
    //    _ = FilesFiltering.GetFiles([Filter], Root).AsParallel().ToArray();
    //    getFilesStopwatch.Stop();

    //    getFilesAsyncStopwatch.Elapsed.Should().BeLessThan(getFilesStopwatch.Elapsed);
    //}
}