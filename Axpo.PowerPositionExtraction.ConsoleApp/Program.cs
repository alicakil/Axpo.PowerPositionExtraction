using Axpo;
using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Jobs;
using Axpo.PowerPositionExtraction.ConsoleApp.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Initialize SQLite dependencies
SQLitePCL.Batteries_V2.Init();

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Configuration.Sources.Clear();
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddCommandLine(args);

// Configure services
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Hangfire SQLite Configuration
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSQLiteStorage(builder.Configuration.GetConnectionString("HangfireConnection")));
builder.Services.AddHangfireServer();

// Register services
builder.Services.AddSingleton<IPowerService, PowerService>();
builder.Services.AddSingleton<ICsvGenerator, CsvGenerator>();
builder.Services.AddSingleton<IJobManager, JobManager>();
builder.Services.AddTransient<PowerPositionJob>();

// Build and run the host
using var host = builder.Build();

// Initialize jobs
try
{
    using var scope = host.Services.CreateScope();
    var jobManager = scope.ServiceProvider.GetRequiredService<IJobManager>();
    await jobManager.InitializeAsync();

    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Application started successfully");
}
catch (Exception ex)
{
    var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger<Program>();
    logger.LogError(ex, "An error occurred during application startup");
}

await host.RunAsync();