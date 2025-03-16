using Axpo;
using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Models;
using Axpo.PowerPositionExtraction.ConsoleApp.Services;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.TimeZones;

namespace Axpo.PowerPositionExtraction.ConsoleApp.Jobs
{
    public class PowerPositionJob
    {
        private readonly ILogger<PowerPositionJob> _logger;
        private readonly IPowerService _powerService;
        private readonly ICsvGenerator _csvGenerator;
        private readonly AppSettings _settings;

        public PowerPositionJob(
            ILogger<PowerPositionJob> logger,
            IPowerService powerService,
            ICsvGenerator csvGenerator,
            IConfiguration configuration)
        {
            _logger = logger;
            _powerService = powerService;
            _csvGenerator = csvGenerator;
            _settings = configuration.GetSection("AppSettings").Get<AppSettings>();
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Starting power position extract");

                // Get current date in London timezone
                var londonTimeZone = DateTimeZoneProviders.Tzdb["Europe/London"];
                var nowInLondon = SystemClock.Instance.GetCurrentInstant().InZone(londonTimeZone);

                var tradingDate = (nowInLondon.Hour >= 23)
                    ? nowInLondon.Date.PlusDays(-1).ToDateTimeUnspecified()
                    : nowInLondon.Date.ToDateTimeUnspecified();

                _logger.LogInformation("Fetching trades for date: {TradingDate}", tradingDate);

                // Get trades for the date
                var trades = await _powerService.GetTradesAsync(tradingDate);

                if (trades?.Any() != true)
                {
                    _logger.LogWarning("No trades found for date: {TradingDate}", tradingDate);
                    return;
                }

                // Aggregate positions
                var hourlyPositions = trades
                    .SelectMany(p => p.Periods)
                    .GroupBy(k => k.Period)
                    .Select(g => new HourlyPosition
                    {
                        LocalTime = g.Key,
                        Volume = g.Sum(p => p.Volume)
                    });

                // Generate CSV
                var extractTime = nowInLondon.ToDateTimeOffset();
                var outputPath = string.IsNullOrEmpty(_settings.OutputPath)
                    ? Directory.GetCurrentDirectory()
                    : _settings.OutputPath;

                var fileName = $"PowerPosition_{extractTime:yyyyMMdd}_{extractTime:HHmm}.csv";
                var filePath = Path.Combine(outputPath, fileName);

                _logger.LogInformation("Generating CSV file: {FilePath}", filePath);

                await _csvGenerator.GenerateCsvAsync(hourlyPositions, filePath);

                _logger.LogInformation("Power position extract completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during power position extract");
                throw;
            }
        }
    }
}