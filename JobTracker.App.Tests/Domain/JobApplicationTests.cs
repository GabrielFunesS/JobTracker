using JobTracker.App.Domain;

namespace JobTracker.App.Tests.Domain;

/// <summary>
/// Tests de caracterización de <see cref="JobApplication"/> (fase E, PLAN rev. 2):
/// valores por defecto e ida y vuelta de todas las propiedades.
/// </summary>
public class JobApplicationTests
{
    [Fact]
    public void Defaults_MatchCurrentBehavior()
    {
        var app = new JobApplication();

        Assert.Equal(Guid.Empty, app.Id);
        Assert.Equal(string.Empty, app.CompanyName);
        Assert.Equal(string.Empty, app.JobTitle);
        Assert.Null(app.JobUrl);
        Assert.Equal(default, app.ApplicationDate);
        Assert.Equal(0, app.OriginId);
        Assert.Null(app.Origin);
        Assert.Equal(ApplicationStatus.Pendiente, app.Status);
        Assert.Null(app.CurrentStage);
        Assert.Null(app.RejectionReason);
        Assert.Null(app.ExpectedSalary);
        Assert.Null(app.Notes);
        Assert.Null(app.LastFollowUpDate);
    }

    [Fact]
    public void AllProperties_RoundTrip_PreserveValues()
    {
        var origin = new JobOrigin { Id = 7, Name = "LinkedIn", IsActive = true };
        var app = new JobApplication
        {
            Id = Guid.NewGuid(),
            CompanyName = "Acme Corp",
            JobTitle = "Dev Sénior",
            JobUrl = "https://example.com/job/1",
            ApplicationDate = new DateTime(2026, 1, 15),
            OriginId = 42,
            Origin = origin,
            Status = ApplicationStatus.Entrevistas,
            CurrentStage = "Prueba técnica",
            RejectionReason = RejectionReason.Ghosting,
            ExpectedSalary = 55000.75m,
            Notes = "Contacto por LinkedIn",
            LastFollowUpDate = new DateTime(2026, 2, 1),
        };

        Assert.Equal(app.Id, new Guid(app.Id.ToString()));
        Assert.Equal("Acme Corp", app.CompanyName);
        Assert.Equal("Dev Sénior", app.JobTitle);
        Assert.Equal("https://example.com/job/1", app.JobUrl);
        Assert.Equal(new DateTime(2026, 1, 15), app.ApplicationDate);
        Assert.Equal(42, app.OriginId);
        Assert.Same(origin, app.Origin);
        Assert.Equal(ApplicationStatus.Entrevistas, app.Status);
        Assert.Equal("Prueba técnica", app.CurrentStage);
        Assert.Equal(RejectionReason.Ghosting, app.RejectionReason);
        Assert.Equal(55000.75m, app.ExpectedSalary);
        Assert.Equal("Contacto por LinkedIn", app.Notes);
        Assert.Equal(new DateTime(2026, 2, 1), app.LastFollowUpDate);
    }
}
