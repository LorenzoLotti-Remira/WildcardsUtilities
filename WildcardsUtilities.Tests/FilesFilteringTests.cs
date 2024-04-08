using System.Collections.Immutable;

namespace WildcardsUtilities.Tests;

public class FilesFilteringTests
{
    private static readonly string _testRoot = $"{Environment.CurrentDirectory}/TestRootFolder";
    private static readonly ImmutableArray<string> _matchAll = ["/**/*"];

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

    [Theory]
    [MemberData(nameof(GetFiles_should_work_DATA))]
    public void GetFiles_should_work(string[] filters, string[] expected) =>
        FilesFiltering
            .GetFiles(new() { Filters = [.. filters], Root = _testRoot })
            .Select(file => Path.GetRelativePath(_testRoot, file.Path).Replace('\\', '/'))
            .Should()
            .BeEquivalentTo(expected);

    [Theory]
    [MemberData(nameof(GetFiles_should_work_DATA))]
    public async Task GetFilesAsync_should_work(string[] filters, string[] expected) =>
        (await Async.FilesFiltering.GetFilesAsync(new() { Filters = [.. filters], Root = _testRoot }))
            .Select(file => Path.GetRelativePath(_testRoot, file.Path).Replace('\\', '/'))
            .Should()
            .BeEquivalentTo(expected);

    [Fact]
    public async Task GetFilesAsync_should_be_faster_than_GetFiles()
    {
        var options = new Common.FilesFilteringOptions
        {
            Filters = _matchAll,
            ComputeChecksum = true
        };

        var getFilesAsyncStopwatch = Stopwatch.StartNew();
        _ = (await Async.FilesFiltering.GetFilesAsync(options)).ToArray();
        getFilesAsyncStopwatch.Stop();

        var getFilesStopwatch = Stopwatch.StartNew();
        _ = FilesFiltering.GetFiles(options).ToArray();
        getFilesStopwatch.Stop();

        getFilesAsyncStopwatch.Elapsed.Should().BeLessThan(getFilesStopwatch.Elapsed);
    }

    [Fact]
    public async Task GetFilesAsync_should_be_faster_than_GetFiles_AsParallel()
    {
        var options = new Common.FilesFilteringOptions
        {
            Filters = _matchAll,
            ComputeChecksum = true
        };

        var getFilesAsyncStopwatch = Stopwatch.StartNew();
        _ = (await Async.FilesFiltering.GetFilesAsync(options)).ToArray();
        getFilesAsyncStopwatch.Stop();

        var getFilesStopwatch = Stopwatch.StartNew();
        _ = FilesFiltering.GetFiles(options).AsParallel().ToArray();
        getFilesStopwatch.Stop();

        getFilesAsyncStopwatch.Elapsed.Should().BeLessThan(getFilesStopwatch.Elapsed);
    }

    [Fact]
    public async Task GetFilesAsync_result_should_be_equivalent_to_GetFiles_result()
    {
        var options = new Common.FilesFilteringOptions
        {
            Filters = _matchAll,
            Root = _testRoot,
            ComputeChecksum = true
        };

        var filesTask = Async.FilesFiltering.GetFilesAsync(options);
        var files = FilesFiltering.GetFiles(options);
        files.Should().BeEquivalentTo(await filesTask);
    }

    [Fact]
    public async Task SHA1_Checksum_should_be_correct()
    {
        const string Hash = "1788a49c60407774cbd66d2c1b06927af0e11067";

        var options = new Common.FilesFilteringOptions
        {
            Filters = ["/test.txt"],
            Root = _testRoot,
            ComputeChecksum = true
        };

        var file = FilesFiltering.GetFiles(options).Single();
        var asyncFile = (await Async.FilesFiltering.GetFilesAsync(options)).Single();

        Convert.ToHexString([.. file.Checksum!]).ToLower().Should().BeEquivalentTo(Hash);
        Convert.ToHexString([.. asyncFile.Checksum!]).ToLower().Should().BeEquivalentTo(Hash);
    }


    [Fact(Skip = "Requires specific folders.")]
    public async Task GetFilesAsync_result_should_be_equivalent_to_GetFiles_result_INTENSIVE()
    {
        var options = new Common.FilesFilteringOptions
        {
            Filters =
            [
                "/AMD/**/*",
                "/DBeaver/**/*"
            ],
            Root = "/Program Files",
            ComputeChecksum = true
        };

        var filesTask = Async.FilesFiltering.GetFilesAsync(options);
        var files = FilesFiltering.GetFiles(options);
        files.Should().BeEquivalentTo(await filesTask);
    }
}