namespace WildcardsUtilities.Scanning.DbItems;

public class SnapshotDbItem
{
    public Guid SnapshotId { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
}
