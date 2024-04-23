namespace WildcardsUtilities.Scanner.Options;

public interface IScannerOptions;

public interface IDatabaseDependentOperationOptions : IScannerOptions
{
    string? Database { get; }
}

public interface IDatabasesOptions : IScannerOptions
{
    DatabaseConnectionInfo? NewDatabase { get; }
    string? DatabaseToRemove { get; }
    bool ListDatabases { get; }
}

public interface IScanOptions : IDatabaseDependentOperationOptions
{
    IEnumerable<char> Drives { get; }
    IEnumerable<char> ExcludedDrives { get; }
    IEnumerable<string> Filters { get; }
    bool SkipChecksumComputations { get; }
}

public interface IGroupOptions : IDatabaseDependentOperationOptions
{
    string? Description { get; }
    bool Disband { get; }
    string Name { get; }
    string? NewName { get; }
    IEnumerable<SnapshotId> SnapshotsToLink { get; }
    IEnumerable<SnapshotId> SnapshotsToUnlink { get; }
    bool DisplayInformations { get; }
}

public interface IListGroupsOptions : IDatabaseDependentOperationOptions
{
    bool Detailed { get; }
}
