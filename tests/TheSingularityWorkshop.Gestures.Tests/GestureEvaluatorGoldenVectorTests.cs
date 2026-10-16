using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Small golden vectors for the evaluator's unvaried linear-position path.
/// Values are contract fixtures, not measurements of physical motion.
/// </summary>
public sealed class GestureEvaluatorGoldenVectorTests
{
    [Theory]
    [InlineData(0.0, 0.0)]
    [InlineData(0.25, 2.5)]
    [InlineData(0.5, 5.0)]
    [InlineData(0.75, 7.5)]
    [InlineData(1.0, 10.0)]
    public void LinearRootTranslationMatchesGoldenVector(double elapsedSeconds, double expectedX)
    {
        var pose = GestureEvaluator.Evaluate(CreateTranslationGesture(), elapsedSeconds, 1.0);

        Assert.Equal(elapsedSeconds, pose.NormalizedProgress, 8);
        Assert.Equal((float)expectedX, pose.LocalTransforms["root"].Position.X, 5);
        Assert.Equal(0f, pose.LocalTransforms["root"].Position.Y, 5);
        Assert.Equal(0f, pose.LocalTransforms["root"].Position.Z, 5);
        Assert.Equal(elapsedSeconds >= 1.0, pose.IsComplete);
        Assert.Equal(Vector3.Zero, pose.RootOffset);
    }

    [Fact]
    public void NegativeElapsedTimeMatchesStartVector()
    {
        var pose = GestureEvaluator.Evaluate(CreateTranslationGesture(), -10.0, 1.0);

        Assert.Equal(0.0, pose.NormalizedProgress);
        Assert.False(pose.IsComplete);
        Assert.Equal(Vector3.Zero, pose.LocalTransforms["root"].Position);
    }

    private static GestureDefinition CreateTranslationGesture()
    {
        var start = new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(Vector3.Zero, Quaternion.Identity, Vector3.One)
        };
        var end = new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(10f, 0f, 0f), Quaternion.Identity, Vector3.One)
        };
        return new GestureDefinition("golden.linear-translation", new[]
        {
            new GestureKeyframe(0, start),
            new GestureKeyframe(1, end)
        });
    }
}
