using Dapper;
using JobTracker.App.Domain;
using JobTracker.App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.App.Features.Settings
{
    internal class SqliteJobOriginService : IJobOriginService   
    {
        readonly AppDbContext _context;

        public SqliteJobOriginService(AppDbContext dbContext)
        {
            _context = dbContext;
        }

        public async Task<JobOrigin?> GetJobOriginByIdAsync(int id)
        {
            var connection = _context.Database.GetDbConnection();
            var sql = "SELECT * FROM JobOrigins WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<JobOrigin>(sql, new { Id = id });
        }

        public async Task<List<JobOrigin>> GetJobOriginsAsync()
        {
            var connection = _context.Database.GetDbConnection();
            var sql = "SELECT * FROM JobOrigins WHERE IsActive = 1";
            var result = await connection.QueryAsync<JobOrigin>(sql);
            return result.ToList();
        }

        public async Task AddJobOriginAsync(JobOrigin jobOrigin)
        {
            _context.JobOrigins.Add(jobOrigin);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateJobOriginAsync(JobOrigin jobOrigin)
        {
            _context.JobOrigins.Update(jobOrigin);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteJobOriginAsync(int id)
        {
            await _context.JobOrigins
                .Where(o => o.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(origin => origin.IsActive, false));
        }
    }
}
