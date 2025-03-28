﻿using Axpo.PowerPositionExtraction.ConsoleApp.Models;
using Microsoft.Extensions.Logging;

namespace Axpo.PowerPositionExtraction.ConsoleApp.Services;

public class CsvGenerator(ILogger<CsvGenerator> logger) : ICsvGenerator
{
    public async Task GenerateCsvAsync(IEnumerable<HourlyPosition> positions, string filePath)
    {
        logger.LogInformation("Generating CSV file: {FilePath}", filePath);

        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var writer = new StreamWriter(filePath))
            {
                // Write header
                await writer.WriteLineAsync("Local Time,Volume");

                // Write data rows
                foreach (var position in positions)
                {
                    var timeString = position.LocalTime.ToString("00") + ":00";
                    await writer.WriteLineAsync($"{timeString},{position.Volume}");
                }
            }

            logger.LogInformation("CSV file generated successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating CSV file");
            throw;
        }
    }
}