namespace WildcardsUtilities.Scanning.DbItems;

public class SnapshotDbItem
{
    public SnapshotId SnapshotId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}
