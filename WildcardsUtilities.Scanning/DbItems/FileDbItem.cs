namespace WildcardsUtilities.Scanning.DbItems;

public class FileDbItem
{
    public FileId FileId { get; set; }
    public SnapshotId SnapshotId { get; set; }
    public PathId PathId { get; set; }
    public VolumeId VolumeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte[]? Checksum { get; set; } = null;
    public long Size { get; set; }
    public FileAttributes Attributes { get; set; }
}
