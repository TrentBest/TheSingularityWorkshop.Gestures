using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureEvaluatorTests
{
    [Fact]
    public void EvaluatesLinearPositionAtHalfProgress()
    {
        var pose = GestureEvaluator.Evaluate(CreateGesture(), 0.5, 1.0);
        Assert.Equal(0.5, pose.NormalizedProgress, 6);
        Assert.False(pose.IsComplete);
        Assert.Equal(new Vector3(5f, 0f, 0f), pose.LocalTransforms["root"].Position);
    }

    [Fact]
    public void ClampsProgressAtCompletion()
    {
        var pose = GestureEvaluator.Evaluate(CreateGesture(), 3.0, 1.0);
        Assert.Equal(1.0, pose.NormalizedProgress);
        Assert.True(pose.IsComplete);
        Assert.Equal(new Vector3(10f, 0f, 0f), pose.LocalTransforms["root"].Position);
    }

    [Fact]
    public void SameIdentityAndInputsProduceSameVariation()
    {
        var variation = new GestureVariation { MaximumSpeedDeviation = 0.15, MaximumLateralOffset = 0.2f };
        var first = GestureEvaluator.Evaluate(CreateGesture(), 0.4, 1.0, variation, 1234);
        var second = GestureEvaluator.Evaluate(CreateGesture(), 0.4, 1.0, variation, 1234);
        Assert.Equal(first.NormalizedProgress, second.NormalizedProgress);
        Assert.Equal(first.RootOffset, second.RootOffset);
        Assert.Equal(first.LocalTransforms["root"], second.LocalTransforms["root"]);
    }

    [Fact]
    public void VariationStaysWithinConfiguredBounds()
    {
        var variation = new GestureVariation { MaximumSpeedDeviation = 0.1, MaximumLateralOffset = 0.25f };
        for (ulong seed = 0; seed < 256; seed++)
        {
            var pose = GestureEvaluator.Evaluate(CreateGesture(), 0.5, 1.0, variation, seed);
            Assert.InRange(pose.NormalizedProgress, 0.45, 0.55);
            Assert.InRange(pose.RootOffset.X, -0.25f, 0.25f);
        }
    }

    [Fact]
    public void RejectsNonIncreasingKeyframeTimes()
    {
        var frames = new[]
        {
            new GestureKeyframe(0, new Dictionary<string, PoseTransform>()),
            new GestureKeyframe(0, new Dictionary<string, PoseTransform>()),
            new GestureKeyframe(1, new Dictionary<string, PoseTransform>())
        };
        Assert.Throws<ArgumentException>(() => new GestureDefinition("invalid", frames));
    }

    [Fact]
    public void RejectsMissingNormalizedEndpoints()
    {
        var frames = new[]
        {
            new GestureKeyframe(0.1, new Dictionary<string, PoseTransform>()),
            new GestureKeyframe(1, new Dictionary<string, PoseTransform>())
        };
        Assert.Throws<ArgumentException>(() => new GestureDefinition("invalid", frames));
    }

    [Fact]
    public void DefensiveCopyProtectsKeyframeTransforms()
    {
        var transforms = new Dictionary<string, PoseTransform> { ["root"] = PoseTransform.Identity };
        var frame = new GestureKeyframe(0, transforms);
        transforms["root"] = new PoseTransform(Vector3.One, Quaternion.Identity, Vector3.One);
        Assert.Equal(Vector3.Zero, frame.LocalTransforms["root"].Position);
    }

    [Theory]
    [InlineData("humanoid", "hips", "left_hand")]
    [InlineData("quadruped", "spine", "front_left_paw")]
    [InlineData("mechanism", "base_hinge", "tool_tip")]
    public void EvaluatesNamedTransformsWithoutAssumingHumanoidBoneNames(
        string rigKind,
        string rootTransformName,
        string endpointTransformName)
    {
        var start = new Dictionary<string, PoseTransform>
        {
            [rootTransformName] = PoseTransform.Identity,
            [endpointTransformName] = PoseTransform.Identity
        };
        var end = new Dictionary<string, PoseTransform>
        {
            [rootTransformName] = new PoseTransform(
                new Vector3(2f, 0f, 0f), Quaternion.Identity, Vector3.One),
            [endpointTransformName] = new PoseTransform(
                new Vector3(0f, 4f, 0f), Quaternion.Identity, Vector3.One)
        };
        var definition = new GestureDefinition($"example.{rigKind}", new[]
        {
            new GestureKeyframe(0d, start),
            new GestureKeyframe(1d, end)
        });

        var pose = GestureEvaluator.Evaluate(definition, 0.5d, 1d);

        Assert.Equal(new[] { rootTransformName, endpointTransformName }.OrderBy(x => x),
            pose.LocalTransforms.Keys.OrderBy(x => x));
        Assert.Equal(new Vector3(1f, 0f, 0f), pose.LocalTransforms[rootTransformName].Position);
        Assert.Equal(new Vector3(0f, 2f, 0f), pose.LocalTransforms[endpointTransformName].Position);
    }

    private static GestureDefinition CreateGesture()
    {
        var start = new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(Vector3.Zero, Quaternion.Identity, Vector3.One)
        };
        var end = new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(10f, 0f, 0f), Quaternion.Identity, Vector3.One)
        };
        return new GestureDefinition("test.translate", new[]
        {
            new GestureKeyframe(0, start),
            new GestureKeyframe(1, end)
        });
    }
}
