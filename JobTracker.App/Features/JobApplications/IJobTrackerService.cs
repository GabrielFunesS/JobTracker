using JobTracker.App.Domain;

namespace JobTracker.App.Features.JobApplications
{
    public interface IJobTrackerService
    {
        Task<List<JobApplication>> GetJobApplicationsAsync();
        Task<JobApplication?> GetJobApplicationByIdAsync(Guid id);
        Task AddJobApplicationAsync(JobApplication jobApplication);
        Task UpdateJobApplicationAsync(JobApplication jobApplication);
        Task DeleteJobApplicationAsync(Guid id);
    }
}
