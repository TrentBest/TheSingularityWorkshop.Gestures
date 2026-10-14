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
    public void RejectsNonFinitePositionRotationAndScaleComponents()
    {
        Assert.Throws<ArgumentException>(() => new GestureKeyframe(0,
            new Dictionary<string, PoseTransform>
            {
                ["position"] = new PoseTransform(new Vector3(float.NaN, 0f, 0f), Quaternion.Identity, Vector3.One)
            }));
        Assert.Throws<ArgumentException>(() => new GestureKeyframe(0,
            new Dictionary<string, PoseTransform>
            {
                ["rotation"] = new PoseTransform(Vector3.Zero, new Quaternion(0f, float.PositiveInfinity, 0f, 1f), Vector3.One)
            }));
        Assert.Throws<ArgumentException>(() => new GestureKeyframe(0,
            new Dictionary<string, PoseTransform>
            {
                ["scale"] = new PoseTransform(Vector3.Zero, Quaternion.Identity, new Vector3(1f, float.NegativeInfinity, 1f))
            }));
    }

    [Fact]
    public void RejectsNonFiniteAnchorTransformComponents()
    {
        var anchor = new GestureAnchor("grip", "hand",
            new PoseTransform(new Vector3(0f, 0f, float.NaN), Quaternion.Identity, Vector3.One));
        Assert.Throws<ArgumentException>(() => anchor.Validate());
    }

    [Fact]
    public void DefensiveCopyProtectsKeyframeTransforms()
    {
        var transforms = new Dictionary<string, PoseTransform> { ["root"] = PoseTransform.Identity };
        var frame = new GestureKeyframe(0, transforms);
        transforms["root"] = new PoseTransform(Vector3.One, Quaternion.Identity, Vector3.One);
        Assert.Equal(Vector3.Zero, frame.LocalTransforms["root"].Position);
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
