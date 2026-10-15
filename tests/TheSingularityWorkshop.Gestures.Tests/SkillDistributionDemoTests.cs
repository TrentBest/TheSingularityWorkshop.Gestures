using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class SkillDistributionDemoTests
{
    [Fact]
    public void SameSeedAndInputsProduceIdenticalSamplesAndMetrics()
    {
        var first = SkillDistributionDemo.Generate(1729, 128, 0.2, 0.05);
        var second = SkillDistributionDemo.Generate(1729, 128, 0.2, 0.05);

        Assert.Equal(first, second);
        Assert.Equal(first.Samples, second.Samples);
    }

    [Fact]
    public void ZeroMovementAndOutcomeSpreadProduceIdealSamples()
    {
        var result = SkillDistributionDemo.Generate(7, 16, 0, 0);

        Assert.Equal(0, result.MeanPoseError);
        Assert.Equal(0, result.MovementVariance);
        Assert.Equal(0, result.OutcomeSpread);
        Assert.All(result.Samples, sample =>
        {
            Assert.Equal(0, sample.MovementX);
            Assert.Equal(0, sample.MovementY);
            Assert.Equal(0, sample.OutcomeX);
            Assert.Equal(0, sample.OutcomeY);
        });
    }

    [Fact]
    public void BroaderMovementEnvelopeIncreasesMovementVarianceForSameSeed()
    {
        var narrow = SkillDistributionDemo.Generate(41, 512, 0.1, 0.02);
        var broad = SkillDistributionDemo.Generate(41, 512, 0.8, 0.02);

        Assert.True(broad.MovementVariance > narrow.MovementVariance);
        Assert.True(broad.MeanPoseError > narrow.MeanPoseError);
    }

    [Fact]
    public void IndependentOutcomeNoiseCanIncreaseOutcomeSpreadWithoutChangingMovementSamples()
    {
        var quiet = SkillDistributionDemo.Generate(99, 256, 0.3, 0);
        var noisy = SkillDistributionDemo.Generate(99, 256, 0.3, 0.4);

        Assert.Equal(quiet.Samples.Select(s => (s.MovementX, s.MovementY)),
            noisy.Samples.Select(s => (s.MovementX, s.MovementY)));
        Assert.Equal(quiet.MovementVariance, noisy.MovementVariance);
        Assert.True(noisy.OutcomeSpread > quiet.OutcomeSpread);
    }

    [Theory]
    [InlineData(0, 1, 0, 0)]
    [InlineData(1, 0, 0, 0)]
    [InlineData(1, 1, -0.1, 0)]
    [InlineData(1, 1, 0, -0.1)]
    [InlineData(1, 1, double.NaN, 0)]
    [InlineData(1, 1, 0, double.PositiveInfinity)]
    public void InvalidInputsAreRejected(int seed, int sampleCount, double movementSpread, double outcomeNoise)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            SkillDistributionDemo.Generate(seed, sampleCount, movementSpread, outcomeNoise));
    }
}
