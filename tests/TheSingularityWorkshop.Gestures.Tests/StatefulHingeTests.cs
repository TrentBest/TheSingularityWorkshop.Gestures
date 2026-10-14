using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulHingeTests
{
    [Fact]
    public void AdvancesTowardTargetWithinSpeedBudget()
    {
        var hinge = new StatefulHinge(0f, -1f, 1f);

        var result = hinge.Advance(0.8f, 0.25f, 2f);

        Assert.Equal(0f, result.PreviousAngleRadians);
        Assert.Equal(0.5f, result.AngleRadians, 5);
        Assert.Equal(0.5f, hinge.AngleRadians, 5);
        Assert.Equal(JointMotionState.MovingPositive, result.MotionState);
        Assert.False(result.TargetReached);
        Assert.False(result.JointLimitConstrained);
    }

    [Fact]
    public void ClassifiesPositiveNegativeAndStationaryMotion()
    {
        var hinge = new StatefulHinge(0f, -1f, 1f);

        var positive = hinge.Advance(0.5f, 1f, 1f);
        var negative = hinge.Advance(-0.5f, 1f, 1f);
        var stationary = hinge.Advance(-0.5f, 1f, 1f);

        Assert.Equal(JointMotionState.MovingPositive, positive.MotionState);
        Assert.Equal(JointMotionState.MovingNegative, negative.MotionState);
        Assert.Equal(JointMotionState.Stationary, stationary.MotionState);
        Assert.True(stationary.TargetReached);
    }

    [Fact]
    public void OutOfRangeRequestStopsAtLimitAndReportsUnmetTarget()
    {
        var hinge = new StatefulHinge(0f, -0.5f, 0.5f);

        var result = hinge.Advance(1f, 10f, 1f);

        Assert.Equal(0.5f, result.AngleRadians);
        Assert.False(result.TargetReached);
        Assert.True(result.JointLimitConstrained);
        Assert.Equal(JointMotionState.MovingPositive, result.MotionState);
    }

    [Fact]
    public void RequestOutsideLimitRemainsUnmetEvenInsidePositionTolerance()
    {
        var hinge = new StatefulHinge(0.5f, -1f, 0.5f, stationaryToleranceRadians: 0.01f);

        var result = hinge.Advance(0.505f, 1f, 1f);

        Assert.Equal(0.5f, result.AngleRadians);
        Assert.False(result.TargetReached);
        Assert.True(result.JointLimitConstrained);
    }

    [Fact]
    public void ReachesAnInRangeTargetWhenSpeedBudgetIsSufficient()
    {
        var hinge = new StatefulHinge(0f, -1f, 1f);

        var result = hinge.Advance(0.25f, 1f, 1f);

        Assert.Equal(0.25f, result.AngleRadians);
        Assert.True(result.TargetReached);
        Assert.Equal(JointMotionState.MovingPositive, result.MotionState);
    }

    [Fact]
    public void ZeroDeltaTimeDoesNotMoveTheHinge()
    {
        var hinge = new StatefulHinge(0.25f, -1f, 1f);

        var result = hinge.Advance(-0.25f, 0f, 100f);

        Assert.Equal(0.25f, result.AngleRadians);
        Assert.Equal(JointMotionState.Stationary, result.MotionState);
        Assert.False(result.TargetReached);
    }

    [Fact]
    public void RejectsInvalidConstructionAndStepInputs()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulHinge(float.NaN, -1f, 1f));
        Assert.Throws<ArgumentException>(() => new StatefulHinge(0f, 1f, -1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulHinge(2f, -1f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulHinge(0f, -1f, 1f, -1f));

        var hinge = new StatefulHinge(0f, -1f, 1f);
        Assert.Throws<ArgumentOutOfRangeException>(() => hinge.Advance(float.PositiveInfinity, 1f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => hinge.Advance(0f, -1f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => hinge.Advance(0f, 1f, -1f));
    }
}
