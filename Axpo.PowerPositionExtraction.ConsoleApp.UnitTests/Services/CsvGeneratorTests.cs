using Axpo.PowerPositionExtraction.ConsoleApp.Models;
using Axpo.PowerPositionExtraction.ConsoleApp.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Axpo.PowerPositionExtraction.ConsoleApp.UnitTests.Services
{
    public class CsvGeneratorTests
    {
        [Fact]
        public async Task GenerateCsvAsync_ShouldCreateFileWithCorrectContent()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CsvGenerator>>();
            var csvGenerator = new CsvGenerator(loggerMock.Object);

            var positions = new List<HourlyPosition>
            {
                new() { LocalTime = 0, Volume = 100 },
                new() { LocalTime = 1, Volume = 150.5 },
                new() { LocalTime = 23, Volume = 200 }
            };

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var filePath = Path.Combine(tempDir, "test.csv");

            // Act
            await csvGenerator.GenerateCsvAsync(positions, filePath);

            // Assert
            File.Exists(filePath).Should().BeTrue("because the CSV file should be created.");
            var lines = await File.ReadAllLinesAsync(filePath);
            lines.Should().NotBeNullOrEmpty("because the file should contain data.");
            lines[0].Should().Be("Local Time,Volume");

            // Check each generated line
            lines[1].Should().Be("00:00,100");
            lines[2].Should().Be("01:00,150.5");
            lines[3].Should().Be("23:00,200");

            // Cleanup
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public async Task GenerateCsvAsync_ShouldCreateDirectoryIfNotExists()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CsvGenerator>>();
            var csvGenerator = new CsvGenerator(loggerMock.Object);

            var positions = new List<HourlyPosition>
            {
                new HourlyPosition { LocalTime = 5, Volume = 123.45 }
            };

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            // Ensure directory does not exist
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            var filePath = Path.Combine(tempDir, "test.csv");

            // Act
            await csvGenerator.GenerateCsvAsync(positions, filePath);

            // Assert
            Directory.Exists(tempDir).Should().BeTrue("because the directory should be created if it does not exist.");
            File.Exists(filePath).Should().BeTrue("because the CSV file should be created.");

            var lines = await File.ReadAllLinesAsync(filePath);
            lines[0].Should().Be("Local Time,Volume");
            lines[1].Should().Be("05:00,123.45");

            // Cleanup
            Directory.Delete(tempDir, true);
        }
    }
}
