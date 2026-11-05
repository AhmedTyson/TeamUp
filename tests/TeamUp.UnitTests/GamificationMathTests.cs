using TeamUp.Application.Services;

namespace TeamUp.UnitTests;

public class GamificationMathTests
{
    private readonly GamificationService _service = new(null!);

    [Fact]
    public void CalculateLevelMetrics_SpecExample2540Points_ReturnsLevel6AndCorrectProgress()
    {
        // Spec Example: TotalPoints = 2540
        var result = _service.CalculateLevelMetrics(1, 2540);

        Assert.Equal(6, result.Level);
        Assert.Equal(2500, result.CurrentLevelStart);
        Assert.Equal(3600, result.NextLevelThreshold);
        Assert.Equal(1060, result.PointsToNext);
        Assert.Equal(3.64, result.Progress);
    }

    [Fact]
    public void CalculateLevelMetrics_ZeroPoints_ReturnsLevel1AndZeroProgress()
    {
        var result = _service.CalculateLevelMetrics(1, 0);

        Assert.Equal(1, result.Level);
        Assert.Equal(0, result.CurrentLevelStart);
        Assert.Equal(100, result.NextLevelThreshold);
        Assert.Equal(100, result.PointsToNext);
        Assert.Equal(0.0, result.Progress);
    }

    [Fact]
    public void CalculateLevelMetrics_ExactBoundary100Points_ReturnsLevel2()
    {
        var result = _service.CalculateLevelMetrics(1, 100);

        Assert.Equal(2, result.Level);
        Assert.Equal(100, result.CurrentLevelStart);
        Assert.Equal(400, result.NextLevelThreshold);
        Assert.Equal(300, result.PointsToNext);
        Assert.Equal(0.0, result.Progress);
    }

    [Fact]
    public void CalculateLevelMetrics_NegativePoints_SafeHandlingReturnsLevel1()
    {
        var result = _service.CalculateLevelMetrics(1, -200);

        Assert.Equal(1, result.Level);
        Assert.Equal(0.0, result.Progress);
    }
}
