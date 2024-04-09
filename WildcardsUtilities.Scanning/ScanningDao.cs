using System.Management;
using System.Security.Cryptography;
using System.Text;
using Z.BulkOperations;

namespace WildcardsUtilities.Scanning;

public class ScanningDao(IDbContextFactory<ScanningDbContext> contextFactory) : IScanningDao
{
    private static Guid HashStringToGuid(string s) =>
        new(MD5.HashData(Encoding.Default.GetBytes(s)));

    private static string GetDirectoryNameRelativeToRoot(string path) =>
        Path.GetRelativePath
        (
            Path.GetPathRoot(path)!,
            Path.GetDirectoryName(path)!
        );

    private static IEnumerable<ManagementObject> GetManagementObjects(string className) =>
        new ManagementObjectSearcher($"SELECT * FROM {className}").Get().Cast<ManagementObject>();

    public async ValueTask<SnapshotDbItem> AddNewSnapshotAsync
        (Func<ValueTask<IEnumerable<ChecksummedFileInfo>>> scanningAsyncFunc)
    {
        const int ChunkSize = 1000;

        var snapshot = new SnapshotDbItem
        {
            SnapshotId = SnapshotId.New()
        };

        var scanningTask = scanningAsyncFunc();
        await using var context = await contextFactory.CreateDbContextAsync();
        var snapshotInsertionTask = context.Snapshots.SingleInsertAsync(snapshot);

        var drivesVolumes =
            from volumePartition in GetManagementObjects("Win32_LogicalDiskToPartition")
            let volume = new ManagementObject((string)volumePartition["Dependent"])
            let partition = new ManagementObject((string)volumePartition["Antecedent"])
            join disk in GetManagementObjects("Win32_DiskDrive") on
                (uint)partition["DiskIndex"] equals (uint)disk["Index"]
            let volumeId = new VolumeId((string)volume["VolumeSerialNumber"])
            let driveDbItem = new DriveDbItem
            {
                DriveId = new((string)disk["SerialNumber"]),
                Model = (string)disk["Model"]
            }
            let volumeDbItem = new VolumeDbItem
            {
                VolumeId = volumeId,
                DriveId = driveDbItem.DriveId,
                Label = (string)volume["VolumeName"]
            }
            select new
            {
                Drive = new
                {
                    DbItem = driveDbItem
                },
                Volume = new
                {
                    Letter = (string)volume["DeviceID"],
                    DbItem = volumeDbItem
                }
            };

        var files = await scanningTask;
        await snapshotInsertionTask;

        var drivesVolumesWithFileAndPathDbItemsChunks =
        (
            from file in files
            join driveVolume in drivesVolumes on
                file.Path[..(file.Path.IndexOf(Path.VolumeSeparatorChar) + 1)] equals
                driveVolume.Volume.Letter
            let pathName = GetDirectoryNameRelativeToRoot(file.Path)
            let pathDbItem = new PathDbItem
            {
                Name = pathName,
                PathId = new(HashStringToGuid(pathName))
            }
            let fileDbItem = new FileDbItem
            {
                FileId = FileId.New(),
                Attributes = file.Attributes,
                Checksum = file.Checksum?.ToArray(),
                Name = Path.GetFileName(file.Path),
                SnapshotId = snapshot.SnapshotId,
                VolumeId = driveVolume.Volume.DbItem.VolumeId,
                PathId = pathDbItem.PathId
            }
            select new
            {
                DriveVolume = driveVolume,
                PathDbItem = pathDbItem,
                FileDbItem = fileDbItem
            }
        )
        .Chunk(ChunkSize);

        foreach (var chunk in drivesVolumesWithFileAndPathDbItemsChunks)
        {
            drivesVolumes =
                from association in chunk
                select association.DriveVolume;


            var driveDbItems =
            (
                from driveVolume in drivesVolumes
                select driveVolume.Drive.DbItem
            )
            .DistinctBy(d => d.DriveId);

            await context.Drives.BulkInsertAsync
            (
                driveDbItems,
                new BulkOperationOptions<DriveDbItem> { InsertIfNotExists = true }
            );


            var volumeDbItems =
            (
                from driveVolume in drivesVolumes
                select driveVolume.Volume.DbItem
            )
            .DistinctBy(d => d.VolumeId);

            await context.Volumes.BulkMergeAsync(volumeDbItems);


            var pathDbItems =
            (
                from association in chunk
                select association.PathDbItem
            )
            .DistinctBy(p => p.PathId);

            await context.Paths.BulkInsertAsync
            (
                pathDbItems,
                new BulkOperationOptions<PathDbItem> { InsertIfNotExists = true }
            );


            var fileDbItems =
                from association in chunk
                select association.FileDbItem;

            await context.Files.BulkInsertAsync(fileDbItems);
        }

        return snapshot;
    }

    public async ValueTask<uint> GetFileCount(SnapshotId snapshotId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        return (uint)context
            .Files
            .Where(file => file.SnapshotId == snapshotId)
            .Count();
    }
}
