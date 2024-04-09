namespace WildcardsUtilities.Scanning.DbItems;

public class VolumeDbItem
{
    public VolumeId VolumeId { get; set; }
    public DriveId DriveId { get; set; }
    public string Label { get; set; } = string.Empty;
}
