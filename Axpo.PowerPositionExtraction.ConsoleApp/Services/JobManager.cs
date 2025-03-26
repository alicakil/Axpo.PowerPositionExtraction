using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Jobs;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Axpo.PowerPositionExtraction.ConsoleApp.Services;

public class JobManager(
    ILogger<JobManager> logger,
    IConfiguration configuration,
    IRecurringJobManager recurringJobManager,
    PowerPositionJob powerPositionJob
        ) : IJobManager
{
    private readonly AppSettings _settings = configuration.GetSection("AppSettings").Get<AppSettings>();

    public async Task InitializeAsync()
    {
        logger.LogInformation("Initializing job manager");

        // Run job immediately on startup
        await powerPositionJob.ExecuteAsync();

        // Schedule recurring job
        await ScheduleJobsAsync();
    }

    public Task ScheduleJobsAsync()
    {
        logger.LogInformation("Scheduling jobs with interval: {IntervalMinutes} minutes", _settings.ExtractIntervalMinutes);

        // Schedule recurring job
        recurringJobManager.AddOrUpdate<PowerPositionJob>(
            "generate-power-position-report",
            job => job.ExecuteAsync(),
            $"*/{_settings.ExtractIntervalMinutes} * * * *"); // I may need to improve this line. might not work for some cases.

        return Task.CompletedTask;
    }
}