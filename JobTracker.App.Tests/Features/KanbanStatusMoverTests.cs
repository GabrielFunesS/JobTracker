using Dapper;
using JobTracker.App.Domain;
using JobTracker.App.Features.JobApplications;
using JobTracker.App.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.App.Tests.Features;

/// <summary>
/// Tests del cambio de estado del tablero (BRIEF-001). Cubren la persistencia y
/// las dos reglas que el PLAN fija: misma columna no escribe y un destino fuera
/// del enum se rechaza. SQLite real en archivo temporal, como en la app.
/// </summary>
public class KanbanStatusMoverTests : IDisposable
{
    private static readonly DateTime FollowUp = new(2026, 10, 5);

    private readonly string _dbPath;
    private readonly DbContextOptions<AppDbContext> _options;

    public KanbanStatusMoverTests()
    {
        SqlMapper.AddTypeHandler(new SqliteGuidTypeHandler());
        _dbPath = Path.Combine(Path.GetTempPath(), $"jobtracker-mover-{Guid.NewGuid():N}.db");
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbPath}")
            .Options;

        using var ctx = new AppDbContext(_options);
        ctx.Database.EnsureCreated();
    }

    private JobApplication Seed(ApplicationStatus status = ApplicationStatus.Enviado)
    {
        var app = new JobApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = "Acme",
            JobTitle = "Dev",
            OriginId = 1, // FK a JobOrigins: el seed 1 del AppDbContext
            ApplicationDate = new DateTime(2026, 10, 1),
            LastFollowUpDate = FollowUp,
            Status = status,
        };

        using var ctx = new AppDbContext(_options);
        new SqliteJobTrackerService(ctx).AddJobApplicationAsync(app).GetAwaiter().GetResult();
        return app;
    }

    private JobApplication? Reload(Guid id)
    {
        using var ctx = new AppDbContext(_options);
        return new SqliteJobTrackerService(ctx).GetJobApplicationByIdAsync(id).GetAwaiter().GetResult();
    }

    private KanbanStatusMover Mover()
    {
        var ctx = new AppDbContext(_options);
        return new KanbanStatusMover(new SqliteJobTrackerService(ctx));
    }

    [Fact]
    public async Task MoveAsync_CambiaEstado_Persiste()
    {
        var app = Seed(ApplicationStatus.Enviado);

        var moved = await Mover().MoveAsync(app, ApplicationStatus.Entrevistas);

        Assert.True(moved);
        var reloaded = Reload(app.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(ApplicationStatus.Entrevistas, reloaded!.Status);
        Assert.Equal(FollowUp, reloaded.LastFollowUpDate);
    }

    [Fact]
    public async Task MoveAsync_MismaColumna_NoEscribe()
    {
        var app = Seed(ApplicationStatus.Enviado);

        var moved = await Mover().MoveAsync(app, ApplicationStatus.Enviado);

        Assert.False(moved);
        var reloaded = Reload(app.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(ApplicationStatus.Enviado, reloaded!.Status);
        Assert.Equal(FollowUp, reloaded.LastFollowUpDate);

        using var ctx = new AppDbContext(_options);
        Assert.Equal(1, ctx.JobApplications.Count());
    }

    [Fact]
    public async Task MoveAsync_DestinoNoDefinido_NoEscribe()
    {
        var app = Seed(ApplicationStatus.Enviado);

        await Assert.ThrowsAsync<ArgumentException>(
            () => Mover().MoveAsync(app, (ApplicationStatus)999));

        var reloaded = Reload(app.Id);
        Assert.NotNull(reloaded);
        Assert.Equal(ApplicationStatus.Enviado, reloaded!.Status);
    }

    public void Dispose()
    {
        try
        {
            if (File.Exists(_dbPath)) File.Delete(_dbPath);
        }
        catch (IOException)
        {
            // el archivo quedará en temp; no invalida el test
        }
    }
}
