[assembly: StronglyDefaults(converters: StronglyConverter.EfValueConverter)]

namespace WildcardsUtilities.Scanning.Identifiers;

[Strongly]
public partial struct SnapshotId;

[Strongly]
public partial struct FileId;

[Strongly]
public partial struct PathId;

[Strongly(StronglyType.String)]
public partial struct VolumeId;

[Strongly(StronglyType.String)]
public partial struct DriveId;

[Strongly]
public partial struct SnapshotGroupId;
