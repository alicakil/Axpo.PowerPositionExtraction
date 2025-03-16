using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Jobs;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Axpo.PowerPositionExtraction.ConsoleApp.Services
{
    public class JobManager : IJobManager
    {
        private readonly ILogger<JobManager> _logger;
        private readonly AppSettings _settings;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly PowerPositionJob _powerPositionJob;

        public JobManager(
            ILogger<JobManager> logger,
            IConfiguration configuration,
            IRecurringJobManager recurringJobManager,
            PowerPositionJob powerPositionJob
            )
        {
            _logger = logger;
            _settings = configuration.GetSection("AppSettings").Get<AppSettings>();
            _recurringJobManager = recurringJobManager;
            _powerPositionJob = powerPositionJob;
        }

        public async Task InitializeAsync()
        {
            _logger.LogInformation("Initializing job manager");

            // Run job immediately on startup
            await _powerPositionJob.ExecuteAsync();

            // Schedule recurring job
            await ScheduleJobsAsync();
        }

        public Task ScheduleJobsAsync()
        {
            _logger.LogInformation("Scheduling jobs with interval: {IntervalMinutes} minutes", _settings.ExtractIntervalMinutes);

            // Schedule recurring job
            _recurringJobManager.AddOrUpdate<PowerPositionJob>(
                "generate-power-position-report",
                job => job.ExecuteAsync(),
                $"*/{_settings.ExtractIntervalMinutes} * * * *"); // I may need to improve this line. might not work for some cases.

            return Task.CompletedTask;
        }
    }
}