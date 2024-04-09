

namespace WildcardsUtilities.Scanning;

public interface IScanningDao
{
    ValueTask<SnapshotDbItem> AddNewSnapshotAsync
        (Func<ValueTask<IEnumerable<ChecksummedFileInfo>>> scanningAsyncFunc);
    ValueTask<uint> GetFileCount(SnapshotId snapshotId);
}
