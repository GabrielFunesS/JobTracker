using JobTracker.App.Domain;
using JobTracker.App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Dapper;

namespace JobTracker.App.Features.JobApplications
{
    public class SqliteJobTrackerService : IJobTrackerService       
    {
        readonly AppDbContext _context;

        public SqliteJobTrackerService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<JobApplication?> GetJobApplicationByIdAsync(Guid id)
        {
            var connection = _context.Database.GetDbConnection();
            var sql = "SELECT * FROM JobApplications WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<JobApplication>(sql, new { Id = id });
        }

        public async Task<List<JobApplication>> GetJobApplicationsAsync()
        {
            var connection = _context.Database.GetDbConnection();
            var sql = "SELECT * FROM JobApplications";
            var result = await connection.QueryAsync<JobApplication>(sql);
            return result.ToList();
        }

        public async Task AddJobApplicationAsync(JobApplication jobApplication)
        {
            _context.JobApplications.Add(jobApplication);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobApplicationAsync(JobApplication application)
        {
            // 1. Limpiamos la memoria de EF Core para que no haya conflictos de ID
            _context.ChangeTracker.Clear();

            // 2. Ahora sí, actualizamos sin que el guardia se queje
            _context.JobApplications.Update(application);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteJobApplicationAsync(Guid id)
        {
            await _context.JobApplications
                .Where(j => j.Id == id)
                .ExecuteDeleteAsync();
        }
    }
}
