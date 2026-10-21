using JobTracker.App.Domain;

namespace JobTracker.App.Tests.Domain;

/// <summary>
/// Tests de la regla de detección de follow-up (US-001, AC-2/3/4 y R1-R5).
/// Fecha fija: miércoles 2026-10-07 como "hoy" (la regla no lee el reloj, D-2).
/// </summary>
public class FollowUpAlertRuleTests
{
    private static readonly DateTime Today = new(2026, 10, 7); // miércoles

    private static JobApplication App(
        ApplicationStatus status = ApplicationStatus.Enviado,
        DateTime? applicationDate = null,
        DateTime? lastFollowUpDate = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            CompanyName = "Acme",
            JobTitle = "Dev",
            Status = status,
            ApplicationDate = applicationDate ?? new DateTime(2026, 9, 1),
            LastFollowUpDate = lastFollowUpDate,
        };

    // --- AC-2: umbral ---

    [Fact]
    public void FiveBusinessDays_DoesNotAlert()
    {
        // mié 2026-09-30 → hoy: Jue(1), Vie(2), Lun(3), Mar(4), Mié(5) = 5 → aún no
        var app = App(applicationDate: new DateTime(2026, 9, 30));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    [Fact]
    public void SixBusinessDays_Alerts()
    {
        // mar 2026-09-29 → hoy: Mié(1), Jue(2), Vie(3), Lun(4), Mar(5), Mié(6) = 6
        var app = App(applicationDate: new DateTime(2026, 9, 29));

        Assert.True(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    // --- R3: el conteo excluye sábados y domingos ---

    [Fact]
    public void WeekendDays_AreNotCounted()
    {
        // vie 2026-10-02 → jue 2026-10-08: solo Lun(1), Mar(2), Mié(3), Jue(4).
        // Por calendario serían 6 días (alertaría); por hábiles son 4 (no alerta).
        var app = App(applicationDate: new DateTime(2026, 10, 2));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, new DateTime(2026, 10, 8)));
    }

    // --- R4: fecha de referencia ---

    [Fact]
    public void LastFollowUpDate_TakesPrecedenceOverApplicationDate()
    {
        // Postulación antigua (01-08 alertaría) pero seguimiento ayer → 1 día hábil
        var app = App(
            applicationDate: new DateTime(2026, 8, 1),
            lastFollowUpDate: new DateTime(2026, 10, 6));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    [Fact]
    public void WithoutLastFollowUpDate_FallsBackToApplicationDate()
    {
        // Sin seguimiento: manda ApplicationDate; mar 09-29 → 6 hábiles → alerta
        var app = App(
            applicationDate: new DateTime(2026, 9, 29),
            lastFollowUpDate: null);

        Assert.True(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    // --- AC-3: reinicio ---

    [Fact]
    public void NewFollowUpDate_ResetsAlert()
    {
        // Estaba en alerta (antigua) y se registra seguimiento hoy → 0 días → no alerta
        var app = App(
            applicationDate: new DateTime(2026, 8, 1),
            lastFollowUpDate: new DateTime(2026, 10, 7));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    [Fact]
    public void ReferenceDateIsToday_DoesNotAlert()
    {
        // Fecha de referencia = hoy → 0 días hábiles transcurridos
        var app = App(applicationDate: new DateTime(2026, 10, 7));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, Today));
    }

    // --- AC-4: solo «Enviado» ---

    [Theory]
    [InlineData(ApplicationStatus.Pendiente)]
    [InlineData(ApplicationStatus.Entrevistas)]
    [InlineData(ApplicationStatus.Oferta)]
    [InlineData(ApplicationStatus.Rechazado)]
    [InlineData(ApplicationStatus.Caducado)]
    public void NonSentStatuses_NeverAlert(ApplicationStatus status)
    {
        // Antigüedad muy grande: sin importar la fecha, fuera de «Enviado» no alerta
        var app = App(status: status, applicationDate: new DateTime(2026, 1, 1));

        Assert.False(FollowUpAlertRule.ShouldAlert(app, Today));
    }
}
