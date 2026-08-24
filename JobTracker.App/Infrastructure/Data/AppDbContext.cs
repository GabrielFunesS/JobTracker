using JobTracker.App.Domain;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.App.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<JobOrigin> JobOrigins { get; set; }

        // El constructor recibe las opciones (como la ruta del archivo SQLite)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Datos semilla: Insertamos orígenes comunes para no arrancar con la tabla vacía
            modelBuilder.Entity<JobOrigin>().HasData(
                new JobOrigin { Id = 1, Name = "LinkedIn", IsActive = true },
                new JobOrigin { Id = 2, Name = "Computrabajo", IsActive = true },
                new JobOrigin { Id = 3, Name = "Email Directo", IsActive = true },
                new JobOrigin { Id = 4, Name = "Consultora", IsActive = true }
            );
        }
    }
}
