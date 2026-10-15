using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class MotionResponseComparisonTests
{
    [Fact]
    public void IdenticalInputsProduceIdenticalSamplesAndMetrics()
    {
        var parameters = new SpringDamperParameters(1d, 40d, 12d);

        var first = MotionResponseComparison.Run(1d, 4d, 1d / 120d, parameters);
        var second = MotionResponseComparison.Run(1d, 4d, 1d / 120d, parameters);

        Assert.Equal(first.Samples, second.Samples);
        Assert.Equal(first.MeanAbsoluteTrackingError, second.MeanAbsoluteTrackingError);
        Assert.Equal(first.MaximumAbsoluteTrackingError, second.MaximumAbsoluteTrackingError);
        Assert.Equal(first.PeakOvershoot, second.PeakOvershoot);
        Assert.Equal(first.SettlingTimeSeconds, second.SettlingTimeSeconds);
    }

    [Fact]
    public void DirectReferenceTracksTheSharedTargetExactly()
    {
        var result = MotionResponseComparison.Run(
            rampDurationSeconds: 1d,
            holdDurationSeconds: 0.5d,
            stepSeconds: 0.01d,
            new SpringDamperParameters(1d, 40d, 12d));

        Assert.All(result.Samples, sample =>
        {
            Assert.Equal(sample.TargetPosition, sample.DirectPosition);
            Assert.InRange(sample.TargetPosition, 0d, 1d);
        });
        Assert.True(result.MeanAbsoluteTrackingError > 0d);
        Assert.True(result.MaximumAbsoluteTrackingError > 0d);
    }

    [Fact]
    public void SufficientObservationWindowFindsSettlingForDampedParameters()
    {
        var result = MotionResponseComparison.Run(
            rampDurationSeconds: 0.5d,
            holdDurationSeconds: 8d,
            stepSeconds: 1d / 120d,
            new SpringDamperParameters(1d, 40d, 12d));

        Assert.NotNull(result.SettlingTimeSeconds);
        Assert.InRange(result.SettlingTimeSeconds!.Value, 0d, 8d);
        Assert.InRange(result.Samples[^1].SpringPosition, 0.999d, 1.001d);
        Assert.InRange(result.Samples[^1].SpringVelocity, -0.001d, 0.001d);
    }

    [Fact]
    public void ShortObservationWindowReportsUnsettledInsteadOfInventingTime()
    {
        var result = MotionResponseComparison.Run(
            rampDurationSeconds: 1d,
            holdDurationSeconds: 0d,
            stepSeconds: 0.01d,
            new SpringDamperParameters(1d, 1d, 0.1d));

        Assert.Null(result.SettlingTimeSeconds);
    }

    [Theory]
    [InlineData(0d, 1d, 0.01d, 0.01d)]
    [InlineData(double.NaN, 1d, 0.01d, 0.01d)]
    [InlineData(1d, -1d, 0.01d, 0.01d)]
    [InlineData(1d, 1d, 0d, 0.01d)]
    [InlineData(1d, 1d, 0.01d, 0d)]
    [InlineData(1d, double.PositiveInfinity, 0.01d, 0.01d)]
    public void InvalidInputsAreRejected(
        double rampDuration,
        double holdDuration,
        double step,
        double tolerance)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MotionResponseComparison.Run(
            rampDuration, holdDuration, step, new SpringDamperParameters(1d, 40d, 12d), tolerance));
    }
}
