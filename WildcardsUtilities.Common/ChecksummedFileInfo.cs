namespace WildcardsUtilities.Common;

public record ChecksummedFileInfo
(
    string Path,
    FileAttributes Attributes = default,
    long Size = 0,
    ImmutableArray<byte>? Checksum = null
);
