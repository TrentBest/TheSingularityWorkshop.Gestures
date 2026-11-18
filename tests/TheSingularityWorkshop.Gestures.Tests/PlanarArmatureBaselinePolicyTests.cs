using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class PlanarArmatureBaselinePolicyTests
{
    [Theory]
    [InlineData(1f, 1f, 2f, 0f, 0f, 0f)]
    [InlineData(1f, 1f, 1f, 1f, 0.25f, -0.5f)]
    [InlineData(1.5f, 0.75f, 1.25f, 0.5f, -0.3f, 0.4f)]
    [InlineData(0.8f, 1.2f, 2.5f, 0f, 0.7f, -0.2f)]
    public void ProducesDeterministicJointTargetsAcrossHeldOutTargetsAndConfigurations(
        float shoulderLength,
        float elbowLength,
        float targetX,
        float targetY,
        float initialShoulder,
        float initialElbow)
    {
        var limits = new JointAngleLimits(-MathF.PI, MathF.PI);
        var definition = new PlanarArmatureDefinition(
            "held-out",
            "shoulder",
            "upper",
            "elbow",
            "lower",
            "end-effector",
            new Vector2(4f, -2f),
            shoulderLength,
            elbowLength,
            limits,
            limits,
            initialShoulder,
            initialElbow,
            2f,
            2f);
        var armature = definition.CreateArmature();
        var observation = armature.Reset("evaluation-episode", 17);
        var target = definition.RootAnchor + new Vector2(targetX, targetY);

        var first = PlanarArmatureBaselinePolicy.CreateAction(definition, observation, target);
        var second = PlanarArmatureBaselinePolicy.CreateAction(definition, observation, target);

        Assert.Equal(first, second);
        Assert.Equal(PlanarArmatureActionMode.JointAngleTargets, first.Mode);
        Assert.Equal("evaluation-episode", first.EpisodeId);
        Assert.Equal(0, first.ExpectedStepIndex);
        Assert.True(float.IsFinite(first.ShoulderTargetRadians));
        Assert.True(float.IsFinite(first.ElbowTargetRadians));

        var result = armature.Advance(first, 10f);
        Assert.True(float.IsFinite(result.Observation.AchievedEndEffectorPosition.X));
        Assert.True(float.IsFinite(result.Observation.AchievedEndEffectorPosition.Y));
        Assert.InRange(result.Observation.ShoulderAngleRadians, limits.MinimumRadians, limits.MaximumRadians);
        Assert.InRange(result.Observation.ElbowAngleRadians, limits.MinimumRadians, limits.MaximumRadians);
    }

    [Fact]
    public void RejectsObservationFromDifferentScenarioOrTopology()
    {
        var limits = new JointAngleLimits(-MathF.PI, MathF.PI);
        var definition = Definition("scenario-a", "shoulder-a");
        var otherDefinition = Definition("scenario-b", "shoulder-b");
        var observation = definition.CreateArmature().Reset("episode", 1);

        Assert.Throws<ArgumentException>(() =>
            PlanarArmatureBaselinePolicy.CreateAction(otherDefinition, observation, Vector2.One));
    }

    [Fact]
    public void RejectsNonFiniteTargets()
    {
        var definition = Definition("scenario", "shoulder");
        var observation = definition.CreateArmature().Reset("episode", 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarArmatureBaselinePolicy.CreateAction(
                definition, observation, new Vector2(float.NaN, 0f)));
    }

    private static PlanarArmatureDefinition Definition(string scenario, string shoulderId)
    {
        var limits = new JointAngleLimits(-MathF.PI, MathF.PI);
        return new PlanarArmatureDefinition(
            scenario, shoulderId, "upper", "elbow", "lower", "tip",
            new Vector2(2f, 3f), 1f, 1f, limits, limits, 0f, 0f, 1f, 1f);
    }
}
