namespace WildcardsUtilities.Scanning;

public interface IScanningDao
{
    ValueTask<SnapshotDbItem?> AddNewSnapshotAsync
    (
        Func<CancellationToken, ValueTask<IEnumerable<ChecksummedFileInfo>>> scanningAsyncFunc,
        CancellationToken cancellationToken = default
    );

    ValueTask<bool> DisbandSnapshotGroupAsync
    (
        SnapshotGroupId id,
        CancellationToken cancellationToken = default
    );

    ValueTask<uint> GetFileCountAsync
    (
        SnapshotId snapshotId,
        CancellationToken cancellationToken = default
    );

    ValueTask<SnapshotId[]> GetLinkedSnapshotsAsync
    (
        SnapshotGroupId groupId,
        CancellationToken cancellationToken = default
    );

    ValueTask<SnapshotGroupDbItem?> GetSnapshotGroupAsync
    (
        string name,
        CancellationToken cancellationToken = default
    );

    ValueTask<IReadOnlyDictionary<SnapshotGroupDbItem, SnapshotId[]>> GetSnapshotGroupDetailsAsync
        (CancellationToken cancellationToken = default);

    ValueTask<string[]> GetSnapshotGroupNamesAsync(CancellationToken cancellationToken = default);

    Task LinkSnapshotsAsync
    (
        SnapshotGroupId groupId,
        IEnumerable<SnapshotId> snapshotsIds,
        CancellationToken cancellationToken = default
    );

    Task UnlinkSnapshotsAsync
    (
        SnapshotGroupId groupId,
        IEnumerable<SnapshotId> snapshotsIds,
        CancellationToken cancellationToken = default
    );

    ValueTask<bool> UpsertSnapshotGroupAsync
    (
        SnapshotGroupDbItem snapshotGroup,
        CancellationToken cancellationToken = default
    );
}
