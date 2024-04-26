var pressedCancelKey = false;

Console.CancelKeyPress += delegate
{
    if (!pressedCancelKey)
        Console.WriteLine("Cancelling execution...");

    pressedCancelKey = true;
};

var scannerOptions = ParseCommandLineArguments(args);

if (scannerOptions is null)
    return;

HostApplicationBuilder builder;

try
{
    builder = Host.CreateApplicationBuilder
    (
        new HostApplicationBuilderSettings
        {
            ContentRootPath = AppDomain.CurrentDomain.BaseDirectory
        }
    );
}
catch
{
    Console.WriteLine
        ("ERROR: unable to start the application. The application files may be corrupted.");

    return;
}

builder.Logging.SetMinimumLevel(LogLevel.None);
builder.Services.AddSingleton<IPlainTextTableGenerator, TababularPlainTextTableGenerator>();

switch (scannerOptions)
{
    case IDatabaseDependentOperationOptions dbDependentOperationOptions:
        var defaultDbInfo = new DatabaseConnectionInfo
        (
            "Default",
            "SQLite",
            $"Data Source={builder.Environment.ContentRootPath}/default.db"
        );

        var dbInfo = defaultDbInfo;

        var specifiedDbInfo = builder
            .Configuration
            .GetSection("Databases")
            .Get<DatabaseConnectionInfo[]>()?
            .FirstOrDefault(d => d.Identifier == dbDependentOperationOptions.Database);

        if (specifiedDbInfo is not null)
            dbInfo = specifiedDbInfo;
        else if (dbDependentOperationOptions.Database is not null)
            Console.WriteLine("WARNING: the provided database does not exist. The default one will be used instead.");

        builder.Services
            .AddDbContextFactory<ScanningDbContext>
            (
                options =>
                {
                    if (ApplyDbInfo(options, dbInfo))
                        return;

                    Console.WriteLine("WARNING: unable to use the provided database. The default one will be used instead.");
                    ApplyDbInfo(options, defaultDbInfo);
                }
            )
            .AddEnsureCreatedService<ScanningDbContext>()
            .AddSingleton(dbDependentOperationOptions)
            .AddSingleton<IScanningDao, ScanningDao>()
            .AddHostedService<ScannerService>();

        break;

    case IDatabasesOptions dbConfiguringOptions:
        builder.Services
            .AddSingleton(dbConfiguringOptions)
            .AddHostedService<DatabasesConfiguringService>();

        break;
}

using var host = builder.Build();
await host.RunAsync().ConfigureAwait(false);

static bool ApplyDbInfo(DbContextOptionsBuilder options, DatabaseConnectionInfo dbInfo)
{
    try
    {
        options.Use(dbInfo.Provider, dbInfo.ConnectionString);
        return true;
    }
    catch
    {
        return false;
    }
}

static IScannerOptions? ParseCommandLineArguments(string[] args) =>
    new Parser
    (
        settings =>
        {
            settings.IgnoreUnknownArguments = true;
            settings.HelpWriter = Parser.Default.Settings.HelpWriter;
            settings.AutoHelp = Parser.Default.Settings.AutoHelp;
            settings.AutoVersion = Parser.Default.Settings.AutoVersion;
        }
    )
    .ParseArguments
    (
        args,
        AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where
            (
                type =>
                    type.HasAttribute<VerbAttribute>() &&
                    type.GetConstructor(Type.EmptyTypes) is not null &&
                    type.IsSealed &&
                    type.IsAssignableTo(typeof(IScannerOptions))
            )
            .ToArray()
    )
    .Value as IScannerOptions;