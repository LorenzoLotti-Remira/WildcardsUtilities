namespace WildcardsUtilities.Scanning.DbItems;

public class FileDbItem
{
    public Guid FileId { get; set; }
    public Guid SnapshotId { get; set; }
    public Guid PathId { get; set; }
    public string VolumeId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public byte[]? Checksum { get; set; } = null;
    public FileAttributes Attributes { get; set; }
}
