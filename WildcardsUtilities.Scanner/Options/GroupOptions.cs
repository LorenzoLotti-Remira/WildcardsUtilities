
namespace WildcardsUtilities.Scanner.Options;

[Verb("group", HelpText = "Manage a specific snapshot group.")]
internal sealed class GroupOptions : DatabaseDependentOperationOptions, IGroupOptions
{
    [Value(0, MetaName = "Name", Required = true, HelpText = "The name of the group.")]
    public string Name { get; set; } = string.Empty;

    [Option('d', "description", HelpText = "Set the description of the group.")]
    public string? Description { get; set; }

    [Option('r', "rename", HelpText = "Input the new name for the group.")]
    public string? NewName { get; set; }

    [Option('l', "link", Min = 1, HelpText = "Input snapshots to be linked to the group.")]
    public IEnumerable<string> SnapshotsToLink { get; set; } = [];

    [Option('u', "unlink", Min = 1, HelpText = "Input snapshot to be unlinked from the group.")]
    public IEnumerable<string> SnapshotsToUnlink { get; set; } = [];

    [Option("disband", HelpText = "Disband the group.")]
    public bool Disband { get; set; }

    [Option('p', "print", HelpText = "Print informations about the group.")]
    public bool DisplayInformations { get; set; }

    IEnumerable<SnapshotId> IGroupOptions.SnapshotsToLink =>
        AsSnapshotIdsWhereParsable(SnapshotsToLink);

    IEnumerable<SnapshotId> IGroupOptions.SnapshotsToUnlink =>
        AsSnapshotIdsWhereParsable(SnapshotsToUnlink);

    private static IEnumerable<SnapshotId>
        AsSnapshotIdsWhereParsable(IEnumerable<string> strings) => strings
            .Select(s => SnapshotId.TryParse(s, out var id) ? id : default(SnapshotId?))
            .Where(id => id is not null)
            .Select(id => id!.Value);
}
