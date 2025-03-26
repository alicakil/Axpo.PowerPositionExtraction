using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Jobs;
using Axpo.PowerPositionExtraction.ConsoleApp.Services;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Axpo.PowerPositionExtraction.ConsoleApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Init SQLite dependecies
            SQLitePCL.Batteries_V2.Init();

            var host = CreateHostBuilder(args).Build();

            // Initialize Hangfire
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var jobManager = services.GetRequiredService<IJobManager>();
                    await jobManager.InitializeAsync();

                    logger.LogInformation("Application started successfully");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred during application startup");
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<AppSettings>(hostContext.Configuration.GetSection("AppSettings"));

                    // Hangfire SQLite Configuration
                    services.AddHangfire(configuration => configuration
                        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                        .UseSimpleAssemblyNameTypeSerializer()
                        .UseRecommendedSerializerSettings()
                        .UseSQLiteStorage(hostContext.Configuration.GetConnectionString("HangfireConnection")));

                    services.AddHangfireServer();

                    // Register services
                    services.AddSingleton<IPowerService, PowerService>();                    
                    services.AddSingleton<ICsvGenerator, CsvGenerator>();
                    services.AddSingleton<IJobManager, JobManager>();
                    services.AddTransient<PowerPositionJob>();
                });
    }
}