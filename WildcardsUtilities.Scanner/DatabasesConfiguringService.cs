using System.Text.Json;
using System.Text.Json.Nodes;

namespace WildcardsUtilities.Scanner;

public class DatabasesConfiguringService
(
    IDatabasesOptions options,
    IPlainTextTableGenerator plainTextTableGenerator,
    IHostApplicationLifetime appLifetime,
    IHostEnvironment environment
)
: BackgroundService
{
    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const string DatabasesPropertyName = "Databases";

        var appSettingsFile = environment
            .ContentRootFileProvider
            .GetFileInfo("appsettings.json")
            .PhysicalPath;

        if (appSettingsFile is null)
            return;

        var root = JsonNode
            .Parse(await File.ReadAllTextAsync(appSettingsFile, stoppingToken))?
            .AsObject();

        if (root is null)
            return;

        var databases = root.ContainsKey(DatabasesPropertyName) ?
            root[DatabasesPropertyName]!.Deserialize<List<DatabaseConnectionInfo>>() :
            [];

        if (databases is null)
            return;

        if (options.NewDatabase is not null)
        {
            if (databases.Any(d => d.Identifier == options.NewDatabase.Identifier))
                Console.WriteLine("WARNING: unable to add the new database: identifier already in use.");
            else
                databases.Add(options.NewDatabase);
        }

        if (options.DatabaseToRemove is not null && databases.RemoveAll(d => d.Identifier == options.DatabaseToRemove) <= 0)
            Console.WriteLine("WARNING: unable to remove the specified database: database not found.");

        root[DatabasesPropertyName] = JsonSerializer.SerializeToNode(databases);

        await File.WriteAllTextAsync
        (
            appSettingsFile,
            root.ToJsonString(new() { WriteIndented = true }),
            stoppingToken
        );

        if (options.ListDatabases)
        {
            var table = plainTextTableGenerator.ToPlainText
            (
                from db in databases
                select new Dictionary<string, object>
                {
                    ["Identifier"] = db.Identifier,
                    ["Provider"] = db.Provider,
                    ["Connection string"] = db.ConnectionString
                }
            );

            Console.WriteLine(table);
        }

        appLifetime.StopApplication();
    }
}
