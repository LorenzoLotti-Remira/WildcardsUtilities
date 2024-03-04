namespace WildcardsUtilities.Async.Messages;

internal record SearchResponseMessage
(
    IEnumerable<Common.ChecksummedFileInfo> Files
);
