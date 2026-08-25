using JobTracker.App.Domain;

namespace JobTracker.App.Features.Settings
{
    public interface IJobOriginService
    {
        Task<List<JobOrigin>> GetJobOriginsAsync();
        Task<JobOrigin?> GetJobOriginByIdAsync(int id);
        Task AddJobOriginAsync(JobOrigin jobOrigin);
        Task UpdateJobOriginAsync(JobOrigin jobOrigin);
        Task SoftDeleteJobOriginAsync(int id);
    }
}
