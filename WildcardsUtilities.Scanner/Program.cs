using Microsoft.Extensions.Logging;
using System.Reflection;
using WildcardsUtilities.Scanner;

var builder = Host.CreateApplicationBuilder();

builder.Environment.ContentRootPath = AppDomain.CurrentDomain.BaseDirectory;
builder.Logging.SetMinimumLevel(LogLevel.None);

builder.Services
    .AddDbContextFactory<ScanningDbContext>
    (
        options => options.UseSqlite
        (
            $"Data Source={builder.Environment.ContentRootPath}/db/scanning.db",
            b => b.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name)
        )
    )
    .AddSingleton<IScannerOptions>(_ => Parser.Default.ParseArguments<ScannerOptions>(args).Value)
    .AddSingleton<IScanningDao, ScanningDao>()
    .AddHostedService<ScannerService>();

using var host = builder.Build();
await host.RunAsync();