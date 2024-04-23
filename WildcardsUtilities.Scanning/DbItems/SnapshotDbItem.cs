namespace WildcardsUtilities.Scanning.DbItems;

public class SnapshotDbItem
{
    public SnapshotId SnapshotId { get; set; }
    public DateTime StartDateTime { get; set; } = DateTime.Now;
    public DateTime? EndDateTime { get; set; } = null;
}
