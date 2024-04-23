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

    private async ValueTask<ScanningDbContext> GetDbContextAsync()
    {
        var context = await contextFactory.CreateDbContextAsync();

        if (await context.Database.CanConnectAsync())
            return context;

        throw new InvalidOperationException("Unable to connect to the database.");
    }

    public async ValueTask<SnapshotDbItem?> AddNewSnapshotAsync
        (Func<ValueTask<IEnumerable<ChecksummedFileInfo>>> scanningAsyncFunc)
    {
        const int ChunkSize = 10_000;
        const uint GCCollectInterval = 5;

        var snapshot = new SnapshotDbItem
        {
            SnapshotId = SnapshotId.New()
        };

        var scanningTask = scanningAsyncFunc();
        await using var context = await GetDbContextAsync();

        try
        {
            await context.Snapshots.SingleInsertAsync(snapshot);

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
                    Size = file.Size,
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

            uint currentChunkNumber = 1;

            foreach (var chunk in drivesVolumesWithFileAndPathDbItemsChunks)
            {
                if (currentChunkNumber % GCCollectInterval == 0)
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive);

                currentChunkNumber++;


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

            snapshot.EndDateTime = DateTime.Now;
            await context.Snapshots.SingleUpdateAsync(snapshot);
            return snapshot;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask<SnapshotGroupDbItem?> GetSnapshotGroupAsync(string name)
    {
        await using var context = await GetDbContextAsync();

        return await
        (
            from snapshotGroup in context.SnapshotGroups
            where snapshotGroup.Name == name
            select snapshotGroup
        )
        .SingleOrDefaultAsync();
    }

    public async ValueTask<bool> UpsertSnapshotGroupAsync(SnapshotGroupDbItem snapshotGroup)
    {
        await using var context = await GetDbContextAsync();

        try
        {
            await context.SnapshotGroups.SingleMergeAsync(snapshotGroup);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask<bool> DisbandSnapshotGroupAsync(SnapshotGroupId id)
    {
        await using var context = await GetDbContextAsync();

        return await context
            .SnapshotGroups
            .Where(g => g.SnapshotGroupId == id)
            .DeleteFromQueryAsync() > 0;
    }

    private static IEnumerable<GroupSnapshotRelationDbItem> AsGroupSnapshotRelations
        (SnapshotGroupId groupId, IEnumerable<SnapshotId> snapshotsIds) =>
            from snapshotId in snapshotsIds
            select new GroupSnapshotRelationDbItem
            {
                SnapshotGroupId = groupId,
                SnapshotId = snapshotId
            };

    public async Task LinkSnapshotsAsync(SnapshotGroupId groupId, IEnumerable<SnapshotId> snapshotsIds)
    {
        await using var context = await GetDbContextAsync();

        if (!snapshotsIds.Any())
            return;

        await context.GroupSnapshotRelations.BulkInsertAsync
        (
            AsGroupSnapshotRelations(groupId, snapshotsIds),
            new BulkOperationOptions<GroupSnapshotRelationDbItem>
            {
                InsertIfNotExists = true,
                ErrorMode = ErrorModeType.IgnoreAndContinue
            }
        );
    }

    public async Task UnlinkSnapshotsAsync(SnapshotGroupId groupId, IEnumerable<SnapshotId> snapshotsIds)
    {
        await using var context = await GetDbContextAsync();

        if (!snapshotsIds.Any())
            return;

        await context.GroupSnapshotRelations.BulkDeleteAsync
        (
            AsGroupSnapshotRelations(groupId, snapshotsIds),
            new BulkOperationOptions<GroupSnapshotRelationDbItem>
            {
                ErrorMode = ErrorModeType.IgnoreAndContinue
            }
        );
    }

    public async ValueTask<SnapshotId[]> GetLinkedSnapshotsAsync(SnapshotGroupId groupId)
    {
        await using var context = await GetDbContextAsync();

        return await
        (
            from relation in context.GroupSnapshotRelations
            where relation.SnapshotGroupId == groupId
            select relation.SnapshotId
        )
        .ToArrayAsync();
    }

    public async ValueTask<string[]> GetSnapshotGroupNamesAsync()
    {
        await using var context = await GetDbContextAsync();

        return await context
            .SnapshotGroups
            .Select(group => group.Name)
            .ToArrayAsync();
    }

    public async ValueTask<IReadOnlyDictionary<SnapshotGroupDbItem, SnapshotId[]>> GetSnapshotGroupDetailsAsync()
    {
        await using var context = await GetDbContextAsync();

        return await
        (
            from snapshotGroup in context.SnapshotGroups
            join relation in context.GroupSnapshotRelations
                on snapshotGroup.SnapshotGroupId equals relation.SnapshotGroupId
                into relations
            from relation in relations.DefaultIfEmpty()
            let snapshotId = ReferenceEquals(relation, null) ?
                default(SnapshotId?) : relation.SnapshotId
            group snapshotId by snapshotGroup
        )
        .ToDictionaryAsync
        (
            grouping => grouping.Key,
            grouping =>
            (
                from snapshot in grouping
                where snapshot is not null
                select snapshot.Value
            )
            .ToArray()
        );
    }

    public async ValueTask<uint> GetFileCountAsync(SnapshotId snapshotId)
    {
        await using var context = await GetDbContextAsync();

        return (uint)await context
            .Files
            .Where(file => file.SnapshotId == snapshotId)
            .CountAsync();
    }
}
