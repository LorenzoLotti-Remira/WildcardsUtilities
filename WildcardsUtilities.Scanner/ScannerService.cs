
using System.Data;

namespace WildcardsUtilities.Scanner;

public sealed class ScannerService
(
    IScanningDao dao,
    IHostApplicationLifetime appLifetime,
    IScannerOptions? options
)
: BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (options is null)
        {
            appLifetime.StopApplication();
            return;
        }

        try
        {
            Console.WriteLine("Scanning files...");

            var snapshot = await dao.AddNewSnapshotAsync(ScanFilesAsync);

            Console.WriteLine("Completed.");
            Console.WriteLine();

            var snapshotCreationTime = (DateTime.Now - snapshot.Date).TotalSeconds;
            var fileCount = await dao.GetFileCount(snapshot.SnapshotId);

            var table = new DataTable();
            table.Columns.Add("Snapshot");
            table.Columns.Add("Execution time");
            table.Columns.Add("Matching files");
            table.Rows.Add(snapshot.SnapshotId, $"{snapshotCreationTime} s", fileCount);

            var consoleTable = ConsoleTable.From(table);
            consoleTable.Options.EnableCount = false;
            consoleTable.Write();
        }
        catch (Exception e)
        {
            Console.WriteLine("ERROR: an error occurred during the snapshot creation.");
            Console.WriteLine(e);
            throw;
        }

        appLifetime.StopApplication();
    }

    private async ValueTask<IEnumerable<ChecksummedFileInfo>> ScanFilesAsync()
    {
        var drives = DriveInfo
            .GetDrives()
            .Where(d => d.IsReady)
            .ExceptBy(options!.ExcludedDrives.Select(char.ToUpper), d => d.Name[0]);

        if (options.Drives.Any())
            drives = drives.IntersectBy(options.Drives.Select(char.ToUpper), d => d.Name[0]);

        IEnumerable<ChecksummedFileInfo> files = [];

        foreach (var drive in drives)
        {
            files = files.Concat
            (
                await FilesFiltering.GetFilesAsync
                (
                    new()
                    {
                        ComputeChecksum = !options.SkipChecksumComputations,
                        Filters = [.. options.Filters],
                        Root = drive.Name
                    }
                )
            );
        }

        return files;
    }
}
