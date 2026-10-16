using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulBreakableJointTests
{
    [Fact]
    public void ThresholdEqualityDoesNotBreakJoint()
    {
        var joint = new StatefulBreakableJoint(500f, 120f);

        var result = joint.ObserveLoad(500f, 120f);

        Assert.Equal(JointIntegrityState.Intact, result.Integrity);
        Assert.False(result.BrokeThisStep);
    }

    [Theory]
    [InlineData(500.01f, 0f)]
    [InlineData(0f, 120.01f)]
    public void ExceedingEitherThresholdBreaksJoint(float force, float torque)
    {
        var joint = new StatefulBreakableJoint(500f, 120f);

        var result = joint.ObserveLoad(force, torque);

        Assert.Equal(JointIntegrityState.Broken, result.Integrity);
        Assert.True(result.BrokeThisStep);
        Assert.Equal(force, result.ForceMagnitudeNewtons);
        Assert.Equal(torque, result.TorqueMagnitudeNewtonMeters);
    }

    [Fact]
    public void BrokenJointRemainsBrokenWhenLaterLoadsAreSmall()
    {
        var joint = new StatefulBreakableJoint(500f, 120f);
        joint.ObserveLoad(501f, 0f);

        var result = joint.ObserveLoad(0f, 0f);

        Assert.Equal(JointIntegrityState.Broken, result.Integrity);
        Assert.False(result.BrokeThisStep);
    }

    [Fact]
    public void RejectsInvalidThresholdsAndLoadObservations()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulBreakableJoint(0f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulBreakableJoint(1f, float.NaN));
        var joint = new StatefulBreakableJoint(1f, 1f);

        Assert.Throws<ArgumentOutOfRangeException>(() => joint.ObserveLoad(-1f, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() => joint.ObserveLoad(0f, float.PositiveInfinity));
    }
}
