using Axpo.PowerPositionExtraction.ConsoleApp.Models;

namespace Axpo.PowerPositionExtraction.ConsoleApp.Services;

public interface ICsvGenerator
{
    Task GenerateCsvAsync(IEnumerable<HourlyPosition> positions, string filePath);
}