namespace WildcardsUtilities.Scanner.Options;

[Verb("databases", HelpText = "Manage databases connections.")]
internal sealed class DatabasesOptions : IDatabasesOptions
{
    [Option('a', "add", Min = 3, Max = 3, HelpText = "Input the identifier, provider and connection string of a new database.")]
    public IEnumerable<string> NewDatabase { get; set; } = [];

    [Option('r', "remove", HelpText = "Input the identifier of the database to remove.")]
    public string? DatabaseToRemove { get; set; }

    DatabaseConnectionInfo? IDatabasesOptions.NewDatabase => NewDatabase.Any() ?
        new(NewDatabase.ElementAt(0), NewDatabase.ElementAt(1), NewDatabase.ElementAt(2)) :
        null;

    bool IDatabasesOptions.ListDatabases =>
        this is IDatabasesOptions { NewDatabase: null, DatabaseToRemove: null };
}
