using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureExperienceProfileTests
{
    [Fact]
    public void SameSeedAndProfileProduceSameVariation()
    {
        var profile = new GestureExperienceProfile(0.02, 0.2, 0.01f, 0.4f);
        Assert.Equal(profile.Sample(42), profile.Sample(42));
    }

    [Fact]
    public void DifferentSeedsProduceIndividualSamples()
    {
        var profile = new GestureExperienceProfile(0.02, 0.2, 0.01f, 0.4f);
        Assert.NotEqual(profile.Sample(42), profile.Sample(43));
    }

    [Fact]
    public void EverySampleStaysWithinConfiguredBounds()
    {
        var profile = new GestureExperienceProfile(0.05, 0.25, 0.1f, 0.5f);
        for (ulong seed = 0; seed < 10_000; seed++)
        {
            var sample = profile.Sample(seed);
            Assert.InRange(sample.MaximumSpeedDeviation, 0.05, 0.25);
            Assert.InRange(sample.MaximumLateralOffset, 0.1f, 0.5f);
            sample.Validate();
        }
    }

    [Fact]
    public void FixedRangesProduceFixedVariationRegardlessOfSeed()
    {
        var profile = new GestureExperienceProfile(0.1, 0.1, 0.25f, 0.25f);
        var expected = new GestureVariation { MaximumSpeedDeviation = 0.1, MaximumLateralOffset = 0.25f };
        Assert.Equal(expected, profile.Sample(0));
        Assert.Equal(profile.Sample(0), profile.Sample(ulong.MaxValue));
    }

    [Fact]
    public void AllowsZeroWidthRangesAtZero()
    {
        var profile = new GestureExperienceProfile(0, 0, 0, 0);
        Assert.Equal(GestureVariation.None, profile.Sample(123));
    }

    [Theory]
    [InlineData(double.NaN, 0.2)]
    [InlineData(-0.1, 0.2)]
    [InlineData(0.2, 0.1)]
    [InlineData(0, 1)]
    [InlineData(double.PositiveInfinity, double.PositiveInfinity)]
    public void RejectsInvalidSpeedRanges(double minimum, double maximum)
    {
        Assert.ThrowsAny<ArgumentException>(() => new GestureExperienceProfile(minimum, maximum, 0, 0));
    }

    [Fact]
    public void RejectsInvalidLateralRanges()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GestureExperienceProfile(0, 0.1, -0.1f, 0.2f));
        Assert.Throws<ArgumentException>(() => new GestureExperienceProfile(0, 0.1, 0.3f, 0.2f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GestureExperienceProfile(0, 0.1, 0, float.PositiveInfinity));
    }
}
