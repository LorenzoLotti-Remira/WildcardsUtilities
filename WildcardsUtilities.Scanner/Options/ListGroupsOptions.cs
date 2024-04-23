namespace WildcardsUtilities.Scanner.Options;

[Verb("groups", HelpText = "Show snapshot groups informations.")]
internal sealed class ListGroupsOptions : DatabaseDependentOperationOptions, IListGroupsOptions
{
    [Option('d', "detailed", HelpText = "Show detailed informations about snapshot groups.")]
    public bool Detailed { get; set; }
}
