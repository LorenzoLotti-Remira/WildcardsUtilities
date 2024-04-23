

namespace WildcardsUtilities.Scanning;

public interface IScanningDao
{
    ValueTask<SnapshotDbItem?> AddNewSnapshotAsync
        (Func<ValueTask<IEnumerable<ChecksummedFileInfo>>> scanningAsyncFunc);
    ValueTask<bool> DisbandSnapshotGroupAsync(SnapshotGroupId id);
    ValueTask<uint> GetFileCountAsync(SnapshotId snapshotId);
    ValueTask<SnapshotId[]> GetLinkedSnapshotsAsync(SnapshotGroupId groupId);
    ValueTask<SnapshotGroupDbItem?> GetSnapshotGroupAsync(string name);
    ValueTask<IReadOnlyDictionary<SnapshotGroupDbItem, SnapshotId[]>> GetSnapshotGroupDetailsAsync();
    ValueTask<string[]> GetSnapshotGroupNamesAsync();
    Task LinkSnapshotsAsync(SnapshotGroupId groupId, IEnumerable<SnapshotId> snapshotsIds);
    Task UnlinkSnapshotsAsync(SnapshotGroupId groupId, IEnumerable<SnapshotId> snapshotsIds);
    ValueTask<bool> UpsertSnapshotGroupAsync(SnapshotGroupDbItem snapshotGroup);
}
