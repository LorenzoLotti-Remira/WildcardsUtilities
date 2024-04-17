IScannerOptions? scannerOptions = new Parser(options => options.IgnoreUnknownArguments = true)
    .ParseArguments<ScannerOptions>(args)
    .Value;

if (scannerOptions is null)
    return;

var defaultDbInfo = new DatabaseConnectionInfo
(
    "SQLite",
    $"Data Source={AppDomain.CurrentDomain.BaseDirectory}/default.db"
);

var dbInfo = scannerOptions.DatabaseInfo ?? defaultDbInfo;

var builder = Host.CreateApplicationBuilder();
builder.Logging.SetMinimumLevel(LogLevel.None);

var providersAssemblyNames = builder
    .Configuration
    .GetSection("ProvidersAssemblies")
    .Get<string[]>() ?? [];

foreach (var assemblyName in providersAssemblyNames)
    if (AppDomain.CurrentDomain.GetAssemblies().All(a => a.FullName != assemblyName))
        Assembly.Load(assemblyName);

builder.Services
    .AddDbContextFactory<ScanningDbContext>
    (
        options =>
        {
            if (ApplyDbInfo(options, dbInfo))
                return;

            Console.WriteLine("ERROR: unable to use the provided database. The default one will be used instead.");
            ApplyDbInfo(options, defaultDbInfo);
        }
    )
    .AddSingleton(scannerOptions)
    .AddSingleton<IScanningDao, ScanningDao>()
    .AddHostedService<ScannerService>();

using var host = builder.Build();
await host.RunAsync();

static bool ApplyDbInfo(DbContextOptionsBuilder options, DatabaseConnectionInfo dbInfo)
{
    try
    {
        options.Use
        (
            dbInfo.Provider,
            dbInfo.ConnectionString,
            b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name)
        );

        return true;
    }
    catch
    {
        return false;
    }
}