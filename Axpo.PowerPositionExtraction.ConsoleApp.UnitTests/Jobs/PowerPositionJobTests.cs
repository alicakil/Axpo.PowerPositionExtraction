using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Axpo;
using Axpo.PowerPositionExtraction.ConsoleApp.Config;
using Axpo.PowerPositionExtraction.ConsoleApp.Jobs;
using Axpo.PowerPositionExtraction.ConsoleApp.Models;
using Axpo.PowerPositionExtraction.ConsoleApp.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Axpo.PowerPositionExtraction.Tests
{
    public class PowerPositionJobTests
    {
        private IConfiguration CreateTestConfiguration()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"AppSettings:OutputPath", "C:\\TestOutput"},
                {"AppSettings:ExtractIntervalMinutes", "5"}
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task ExecuteAsync_ShouldCallGenerateCsv_WhenTradesExist()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PowerPositionJob>>();
            var powerServiceMock = new Mock<IPowerService>();
            var csvGeneratorMock = new Mock<ICsvGenerator>();
            var configuration = CreateTestConfiguration();

            // Create a fake trade using Axpo.PowerTrade.Create for a given date and two periods.
            var tradingDate = DateTime.Today;
            var fakeTrade = PowerTrade.Create(tradingDate, 2);
            // Manually set volumes on the periods (assuming PowerPeriod.Volume is settable)
            fakeTrade.Periods[0].SetVolume(100);
            fakeTrade.Periods[1].SetVolume(200);

            // Setup IPowerService to return a list with one trade
            powerServiceMock.Setup(ps => ps.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(Task.FromResult((IEnumerable<PowerTrade>)new List<PowerTrade> { fakeTrade }));

            var job = new PowerPositionJob(loggerMock.Object, powerServiceMock.Object, csvGeneratorMock.Object, configuration);

            // Act
            await job.ExecuteAsync();

            // Assert
            powerServiceMock.Verify(ps => ps.GetTradesAsync(It.IsAny<DateTime>()), Times.Once);
            csvGeneratorMock.Verify(cg => cg.GenerateCsvAsync(
                It.Is<IEnumerable<HourlyPosition>>(hp => hp.Any() && hp.Count() == 2),
                It.Is<string>(s => s.Contains("PowerPosition_"))
            ), Times.Once);
        }

        [Fact]
        public async Task ExecuteAsync_ShouldNotCallGenerateCsv_WhenNoTradesFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<PowerPositionJob>>();
            var powerServiceMock = new Mock<IPowerService>();
            var csvGeneratorMock = new Mock<ICsvGenerator>();
            var configuration = CreateTestConfiguration();

            // Setup IPowerService to return an empty list
            powerServiceMock.Setup(ps => ps.GetTradesAsync(It.IsAny<DateTime>()))
                .Returns(Task.FromResult((IEnumerable<PowerTrade>)new List<PowerTrade>()));

            var job = new PowerPositionJob(loggerMock.Object, powerServiceMock.Object, csvGeneratorMock.Object, configuration);

            // Act
            await job.ExecuteAsync();

            // Assert
            powerServiceMock.Verify(ps => ps.GetTradesAsync(It.IsAny<DateTime>()), Times.Once);
            csvGeneratorMock.Verify(cg => cg.GenerateCsvAsync(It.IsAny<IEnumerable<HourlyPosition>>(), It.IsAny<string>()), Times.Never);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No trades found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
