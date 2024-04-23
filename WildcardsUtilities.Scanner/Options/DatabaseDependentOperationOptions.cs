namespace WildcardsUtilities.Scanner.Options;

internal abstract class DatabaseDependentOperationOptions : IDatabaseDependentOperationOptions
{
    [Option('@', "database", HelpText = "Input the identifier of the database to use.")]
    public string? Database { get; set; }
}
