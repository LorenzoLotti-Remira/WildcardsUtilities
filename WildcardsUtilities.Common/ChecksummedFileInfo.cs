namespace WildcardsUtilities.Common;

public record ChecksummedFileInfo
(
    string Path,
    FileAttributes Attributes,
    ImmutableArray<byte> Checksum
);
