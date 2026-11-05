using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class MarchingCohesionProgressionTests
{
    private static MarchingCohesionProgression CreateProgression() => new(
        new GestureVariation { MaximumSpeedDeviation = 0.2d, MaximumLateralOffset = 0.4f },
        new GestureVariation { MaximumSpeedDeviation = 0.025d, MaximumLateralOffset = 0.05f },
        0.5d);

    [Fact]
    public void VariationDoesNotChangeUntilEverySecondMarchStepHasCompleted()
    {
        var initial = CreateProgression().Start();

        var afterFirst = initial.CompleteMarchStep();

        Assert.Equal(1, afterFirst.CompletedMarchSteps);
        Assert.Equal(initial.CurrentVariation, afterFirst.CurrentVariation);

        var afterSecond = afterFirst.CompleteMarchStep();

        Assert.Equal(2, afterSecond.CompletedMarchSteps);
        Assert.Equal(0.1d, afterSecond.CurrentVariation.MaximumSpeedDeviation, 12);
        Assert.Equal(0.2f, afterSecond.CurrentVariation.MaximumLateralOffset, 6);
    }

    [Fact]
    public void TighteningIsDeterministicAndNeverRemovesIndividualVariation()
    {
        var progression = CreateProgression();
        var first = progression.Start();
        var second = progression.Start();

        for (var step = 0; step < 20; step++)
        {
            first = first.CompleteMarchStep();
            second = second.CompleteMarchStep();
            Assert.Equal(first, second);
            Assert.True(first.CurrentVariation.MaximumSpeedDeviation >=
                progression.MinimumVariation.MaximumSpeedDeviation);
            Assert.True(first.CurrentVariation.MaximumLateralOffset >=
                progression.MinimumVariation.MaximumLateralOffset);
        }

        Assert.Equal(progression.MinimumVariation, first.CurrentVariation);
        Assert.Equal(20, first.CompletedMarchSteps);
    }

    [Fact]
    public void PreviousStateRemainsUnchangedWhenNextStepCompletes()
    {
        var before = CreateProgression().Start();
        var afterOne = before.CompleteMarchStep();
        var afterTwo = afterOne.CompleteMarchStep();

        Assert.Equal(0, before.CompletedMarchSteps);
        Assert.Equal(1, afterOne.CompletedMarchSteps);
        Assert.Equal(2, afterTwo.CompletedMarchSteps);
        Assert.Equal(0.2d, before.CurrentVariation.MaximumSpeedDeviation);
        Assert.Equal(0.2d, afterOne.CurrentVariation.MaximumSpeedDeviation);
        Assert.Equal(0.1d, afterTwo.CurrentVariation.MaximumSpeedDeviation);
    }

    [Fact]
    public void RejectsZeroOrInvertedVariationFloor()
    {
        Assert.Throws<ArgumentException>(() => new MarchingCohesionProgression(
            new GestureVariation { MaximumSpeedDeviation = 0.2d, MaximumLateralOffset = 0.4f },
            new GestureVariation { MaximumSpeedDeviation = 0d, MaximumLateralOffset = 0.05f }));
        Assert.Throws<ArgumentException>(() => new MarchingCohesionProgression(
            new GestureVariation { MaximumSpeedDeviation = 0.2d, MaximumLateralOffset = 0.4f },
            new GestureVariation { MaximumSpeedDeviation = 0.1d, MaximumLateralOffset = 0.5f }));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void RejectsInvalidTighteningFactor(double factor)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MarchingCohesionProgression(
            new GestureVariation { MaximumSpeedDeviation = 0.2d, MaximumLateralOffset = 0.4f },
            new GestureVariation { MaximumSpeedDeviation = 0.025d, MaximumLateralOffset = 0.05f },
            factor));
    }
}
