namespace Axpo.PowerPositionExtraction.ConsoleApp.Services
{
    public interface IJobManager
    {
        Task InitializeAsync();
        Task ScheduleJobsAsync();
    }
}