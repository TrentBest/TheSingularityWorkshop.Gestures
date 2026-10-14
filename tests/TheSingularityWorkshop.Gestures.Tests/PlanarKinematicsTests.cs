using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class PlanarKinematicsTests
{
    [Fact]
    public void ForwardKinematicsKeepsAnchorAtOrigin()
    {
        var end = PlanarKinematics.ForwardKinematics(1f, 1f, 0f, 0f);

        Assert.Equal(new Vector2(2f, 0f), end);
    }

    [Fact]
    public void SolvesReachableTargetAndReportsComputedEndEffector()
    {
        var result = PlanarKinematics.SolveTwoLink(
            1f,
            1f,
            new Vector2(1f, 1f),
            new JointAngleLimits(-MathF.PI, MathF.PI),
            new JointAngleLimits(-MathF.PI, MathF.PI));

        Assert.True(result.TargetReached);
        Assert.InRange(result.ErrorDistance, 0f, 1e-4f);
        Assert.InRange(Vector2.Distance(result.EndEffector, result.Target), 0f, 1e-4f);
    }

    [Fact]
    public void UnreachableTargetReturnsBestConstrainedPoseWithoutClaimingSuccess()
    {
        var result = PlanarKinematics.SolveTwoLink(
            1f,
            1f,
            new Vector2(3f, 0f),
            new JointAngleLimits(-MathF.PI, MathF.PI),
            new JointAngleLimits(-MathF.PI, MathF.PI));

        Assert.False(result.TargetReached);
        Assert.Equal(new Vector2(2f, 0f), result.EndEffector);
        Assert.Equal(1f, result.ErrorDistance, 4);
    }

    [Fact]
    public void JointLimitsAreRespectedEvenWhenTargetCannotBeReached()
    {
        var result = PlanarKinematics.SolveTwoLink(
            1f,
            1f,
            new Vector2(0f, 2f),
            new JointAngleLimits(0f, 0f),
            new JointAngleLimits(0f, 0f));

        Assert.Equal(0f, result.ShoulderRadians);
        Assert.Equal(0f, result.ElbowRadians);
        Assert.False(result.TargetReached);
        Assert.Equal(new Vector2(2f, 0f), result.EndEffector);
    }

    [Fact]
    public void RejectsNonPositiveLinkLengths()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarKinematics.ForwardKinematics(0f, 1f, 0f, 0f));
    }

    [Fact]
    public void RejectsInvertedJointLimits()
    {
        Assert.Throws<ArgumentException>(() => PlanarKinematics.SolveTwoLink(
            1f,
            1f,
            Vector2.One,
            new JointAngleLimits(1f, -1f),
            new JointAngleLimits(-MathF.PI, MathF.PI)));
    }
}
