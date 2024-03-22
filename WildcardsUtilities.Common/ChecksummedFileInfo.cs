namespace WildcardsUtilities.Common;

public record ChecksummedFileInfo
(
    string Path,
    FileAttributes Attributes = default,
    ImmutableArray<byte>? Checksum = null
);
