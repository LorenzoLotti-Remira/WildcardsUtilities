namespace WildcardsUtilities.Scanner;

public sealed class ScannerService
(
    IScanningDao dao,
    IHostApplicationLifetime appLifetime,
    IDatabaseDependentOperationOptions options,
    IPlainTextTableGenerator plainTextTableGenerator
)
: BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var task = options switch
        {
            IScanOptions o => ExecuteScanAsync(o, stoppingToken),
            IGroupOptions o => ExecuteGroupAsync(o, stoppingToken),
            IListGroupsOptions o => ExecuteListGroupsAsync(o, stoppingToken),
            _ => Task.CompletedTask
        };

        try
        {
            await task.ConfigureAwait(false);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine
                ($"ERROR: {e.Message.ToLower()} The provided connection string may be wrong.");
        }

        appLifetime.StopApplication();
    }

    private async Task ExecuteScanAsync
    (
        IScanOptions options,
        CancellationToken stoppingToken = default
    )
    {
        Console.WriteLine("Scanning files...");

        var snapshot = await dao.AddNewSnapshotAsync
        (
            cancellationToken => ScanFilesAsync(options, cancellationToken),
            stoppingToken
        )
        .ConfigureAwait(false);

        stoppingToken.ThrowIfCancellationRequested();

        if (snapshot is null)
        {
            Console.WriteLine("ERROR: an error occurred during the snapshot creation.");
            return;
        }

        Console.WriteLine("Completed.");
        Console.WriteLine();

        var snapshotCreationTime = (snapshot.EndDateTime - snapshot.StartDateTime)!
            .Value
            .TotalSeconds;

        var fileCount = await dao
            .GetFileCountAsync(snapshot.SnapshotId, stoppingToken)
            .ConfigureAwait(false);

        var table = plainTextTableGenerator.ToPlainText
        (
            [
                new Dictionary<string, object>
                {
                    ["Snapshot"] = snapshot.SnapshotId,
                    ["Execution time"] = $"{snapshotCreationTime} s",
                    ["Matching files"] = fileCount
                }
            ]
        );

        Console.WriteLine(table);
    }

    private async Task ExecuteGroupAsync
    (
        IGroupOptions options,
        CancellationToken stoppingToken = default
    )
    {
        var snapshotGroup = await dao
            .GetSnapshotGroupAsync(options.Name, stoppingToken)
            .ConfigureAwait(false) ?? new()
            {
                SnapshotGroupId = SnapshotGroupId.New(),
                Name = options.Name
            };

        if (options.Disband)
        {
            Console.WriteLine
            (
                await dao
                    .DisbandSnapshotGroupAsync(snapshotGroup.SnapshotGroupId, stoppingToken)
                    .ConfigureAwait(false) ?
                        "Snapshot group successfully disbanded." :
                        "ERROR: unable to disband a non existing snapshot group."
            );

            return;
        }

        if (options.Description is not null)
            snapshotGroup.Description = options.Description;

        if (options.NewName is not null)
            snapshotGroup.Name = options.NewName;

        var upserted = await dao
            .UpsertSnapshotGroupAsync(snapshotGroup, stoppingToken)
            .ConfigureAwait(false);

        if (!upserted)
        {
            Console.WriteLine("ERROR: unable to update the snapshot group.");
            return;
        }

        await dao.LinkSnapshotsAsync
        (
            snapshotGroup.SnapshotGroupId,
            options.SnapshotsToLink,
            stoppingToken
        )
        .ConfigureAwait(false);

        await dao.UnlinkSnapshotsAsync
        (
            snapshotGroup.SnapshotGroupId,
            options.SnapshotsToUnlink,
            stoppingToken
        )
        .ConfigureAwait(false);

        if (options.DisplayInformations)
        {
            var table = GenerateSnapshotGroupsPlainTextTable
            (
                new Dictionary<SnapshotGroupDbItem, SnapshotId[]>
                {
                    [snapshotGroup] = await dao.GetLinkedSnapshotsAsync
                    (
                        snapshotGroup.SnapshotGroupId,
                        stoppingToken
                    )
                    .ConfigureAwait(false)
                }
            );

            Console.WriteLine(table);
        }
    }

    private async Task ExecuteListGroupsAsync
    (
        IListGroupsOptions options,
        CancellationToken stoppingToken = default
    )
    {
        if (options.Detailed)
        {
            var details = await dao
                .GetSnapshotGroupDetailsAsync(stoppingToken)
                .ConfigureAwait(false);

            Console.WriteLine(GenerateSnapshotGroupsPlainTextTable(details));
            return;
        }

        var names = await dao.GetSnapshotGroupNamesAsync(stoppingToken).ConfigureAwait(false);

        foreach (var name in names)
            Console.WriteLine(name);
    }

    private string GenerateSnapshotGroupsPlainTextTable
        (IReadOnlyDictionary<SnapshotGroupDbItem, SnapshotId[]> table) =>
            plainTextTableGenerator.ToPlainText
            (
                from pair in table
                select new Dictionary<string, object>
                {
                    ["Name"] = pair.Key.Name,
                    ["Description"] = pair.Key.Description,
                    ["Linked snapshots"] = pair.Value
                }
            );

    private static async ValueTask<IEnumerable<ChecksummedFileInfo>> ScanFilesAsync
    (
        IScanOptions options,
        CancellationToken cancellationToken = default
    )
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
                    },
                    cancellationToken
                )
                .ConfigureAwait(false)
            );
        }

        return files;
    }
}
