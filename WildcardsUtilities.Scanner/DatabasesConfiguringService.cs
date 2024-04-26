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
        var appSettingsFile = environment.ContentRootFileProvider.GetFileInfo("appsettings.json");

        if (appSettingsFile.PhysicalPath is null)
        {
            Console.WriteLine("ERROR: unable to access the configuration file.");
            return;
        }

        JsonObject? root = null;

        if (appSettingsFile.Exists)
        {
            var appSettingsContent = await File
                .ReadAllTextAsync(appSettingsFile.PhysicalPath, stoppingToken)
                .ConfigureAwait(false);

            root = JsonNode.Parse(appSettingsContent)?.AsObject();
        }

        root ??= [];
        List<DatabaseConnectionInfo>? databases = null;

        try
        {
            databases = root[DatabasesPropertyName]!.Deserialize<List<DatabaseConnectionInfo>>();
        }
        catch { }

        databases ??= [];

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
            appSettingsFile.PhysicalPath,
            root.ToJsonString(new() { WriteIndented = true }),
            stoppingToken
        )
        .ConfigureAwait(false);

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
