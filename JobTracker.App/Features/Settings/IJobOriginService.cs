using JobTracker.App.Domain;

namespace JobTracker.App.Features.Settings
{
    public interface IJobOriginService
    {
        Task<List<JobOrigin>> GetJobOriginsAsync();
        Task<JobOrigin?> GetJobOriginByIdAsync(Guid id);
        Task AddJobOriginAsync(JobOrigin jobOrigin);
        Task UpdateJobOriginAsync(JobOrigin jobOrigin);
        Task DeleteJobOriginsAsync(Guid id);
    }
}
