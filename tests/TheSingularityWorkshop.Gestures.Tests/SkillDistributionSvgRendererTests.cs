using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class SkillDistributionSvgRendererTests
{
    [Fact]
    public void RenderComparisonProducesRepeatableStandaloneSvgWithBothPanelsAndMetrics()
    {
        var first = SkillDistributionSvgRenderer.RenderComparison(1729, 64, 0.1, 0.7, 0.08);
        var second = SkillDistributionSvgRenderer.RenderComparison(1729, 64, 0.1, 0.7, 0.08);

        Assert.Equal(first, second);
        Assert.Contains("<svg", first);
        Assert.Contains("Narrow movement envelope", first);
        Assert.Contains("Broad movement envelope", first);
        Assert.Contains("Mean pose error:", first);
        Assert.Contains("Movement variance:", first);
        Assert.Contains("Outcome spread:", first);
        Assert.Contains("Not a physical or ballistic model", first);
    }

    [Fact]
    public void MaximumSupportedSpreadKeepsRenderedOutcomePointsInsideBothPanels()
    {
        var svg = SkillDistributionSvgRenderer.RenderComparison(1729, 240, 1.0, 1.5, 0.8);
        var points = System.Text.RegularExpressions.Regex.Matches(
            svg,
            "<circle cx=\\"(?<x>[^\\"]+)\\" cy=\\"(?<y>[^\\"]+)\\"");

        Assert.NotEmpty(points);
        foreach (System.Text.RegularExpressions.Match point in points)
        {
            var x = double.Parse(point.Groups["x"].Value, System.Globalization.CultureInfo.InvariantCulture);
            var y = double.Parse(point.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture);
            Assert.InRange(x, 30, 1170);
            Assert.InRange(y, 90, 610);
        }
    }

    [Theory]
    [InlineData(0, 0.1, 0.1)]
    [InlineData(8, 0.1, double.NaN)]
    [InlineData(8, 0.1, double.PositiveInfinity)]
    [InlineData(8, 0.1, -0.1)]
    public void InvalidSampleCountOrOutcomeNoiseIsRejected(
        int sampleCount,
        double narrow,
        double outcomeNoise)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SkillDistributionSvgRenderer.RenderComparison(1, sampleCount, narrow, 0.8, outcomeNoise));
    }

    [Theory]
    [InlineData(-0.1, 0.8)]
    [InlineData(0.8, 0.1)]
    [InlineData(0.1, double.NaN)]
    public void InvalidMovementSpreadComparisonIsRejected(double narrow, double broad)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SkillDistributionSvgRenderer.RenderComparison(1, 8, narrow, broad, 0.1));
    }
}