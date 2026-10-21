using JobTracker.App.Domain;

namespace JobTracker.App.Tests.Domain;

/// <summary>
/// Tests de caracterización de <see cref="JobOrigin"/> (fase E, PLAN rev. 2):
/// valores por defecto e ida y vuelta de todas las propiedades.
/// </summary>
public class JobOriginTests
{
    [Fact]
    public void Defaults_MatchCurrentBehavior()
    {
        var origin = new JobOrigin();

        Assert.Equal(0, origin.Id);
        Assert.Equal(string.Empty, origin.Name);
        Assert.True(origin.IsActive);
    }

    [Fact]
    public void AllProperties_RoundTrip_PreserveValues()
    {
        var origin = new JobOrigin
        {
            Id = 13,
            Name = "LinkedIn",
            IsActive = false,
        };

        Assert.Equal(13, origin.Id);
        Assert.Equal("LinkedIn", origin.Name);
        Assert.False(origin.IsActive);
    }
}
