using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;
using static TheSingularityWorkshop.Gestures.Tests.VehicleIntentComparisonFixture;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class VehicleIntentComparisonFixtureTests
{
    private const string Timebase = "flight-sim-clock";
    private const string Frame = "vehicle-local";
    private const string Unit = "m";

    [Fact]
    public void ReportsAchievedMinusIntendedPositionWithoutMutatingEitherInput()
    {
        var gesture = CreateGesture(includeRoot: true);
        var intended = GestureEvaluator.Evaluate(gesture, 0.5d, 1d);
        var achievedPosition = new Vector3(7f, 1f, -2f);
        var achieved = Sample(achievedPosition);

        var result = VehicleIntentComparisonFixture.Compare(
            intended, 0.5d, Timebase, Frame, Unit, achieved);

        Assert.Equal(ComparisonStatus.Comparable, result.Status);
        Assert.Null(result.Reason);
        Assert.Equal(new Vector3(2f, 1f, -2f), result.PositionDelta);
        Assert.Equal(new Vector3(5f, 0f, 0f), intended.LocalTransforms["root"].Position);
        Assert.Equal(achievedPosition, achieved.RootPosition);
    }

    [Fact]
    public void IdenticalInputsProduceIdenticalComparison()
    {
        var intended = GestureEvaluator.Evaluate(CreateGesture(includeRoot: true), 0.25d, 1d);
        var achieved = Sample(new Vector3(3f, 0f, 0f));

        var first = VehicleIntentComparisonFixture.Compare(intended, 0.25d, Timebase, Frame, Unit, achieved);
        var second = VehicleIntentComparisonFixture.Compare(intended, 0.25d, Timebase, Frame, Unit, achieved);

        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData("other-clock", "vehicle-local", "m", 0.5d, "timebase")]
    [InlineData("flight-sim-clock", "world", "m", 0.5d, "frame")]
    [InlineData("flight-sim-clock", "vehicle-local", "ft", 0.5d, "unit")]
    [InlineData("flight-sim-clock", "vehicle-local", "m", 0.75d, "time")]
    public void IncompatibleComparisonMetadataReturnsUnresolved(
        string timebase,
        string frame,
        string unit,
        double timestamp,
        string expectedReason)
    {
        var intended = GestureEvaluator.Evaluate(CreateGesture(includeRoot: true), 0.5d, 1d);
        var achieved = new VehicleSample(timestamp, timebase, frame, unit, Vector3.Zero);

        var result = VehicleIntentComparisonFixture.Compare(intended, 0.5d, Timebase, Frame, Unit, achieved);

        Assert.Equal(ComparisonStatus.Unresolved, result.Status);
        Assert.Null(result.PositionDelta);
        Assert.Contains(expectedReason, result.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void MissingReferenceRootReturnsUnresolved()
    {
        var intended = GestureEvaluator.Evaluate(CreateGesture(includeRoot: false), 0.5d, 1d);

        var result = VehicleIntentComparisonFixture.Compare(
            intended, 0.5d, Timebase, Frame, Unit, Sample(Vector3.Zero));

        Assert.Equal(ComparisonStatus.Unresolved, result.Status);
        Assert.Null(result.PositionDelta);
        Assert.Contains("root", result.Reason!);
    }

    [Fact]
    public void NonFiniteAchievedPositionReturnsUnresolved()
    {
        var intended = GestureEvaluator.Evaluate(CreateGesture(includeRoot: true), 0.5d, 1d);

        var result = VehicleIntentComparisonFixture.Compare(
            intended, 0.5d, Timebase, Frame, Unit, Sample(new Vector3(float.NaN, 0f, 0f)));

        Assert.Equal(ComparisonStatus.Unresolved, result.Status);
        Assert.Null(result.PositionDelta);
    }

    [Fact]
    public void OverflowingPositionDeltaReturnsUnresolved()
    {
        var extreme = float.MaxValue;
        var start = new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(-extreme, 0f, 0f), Quaternion.Identity, Vector3.One)
        };
        var gesture = new GestureDefinition("extreme-position-reference",
        [
            new GestureKeyframe(0d, start),
            new GestureKeyframe(1d, start)
        ]);
        var intended = GestureEvaluator.Evaluate(gesture, 0.5d, 1d);

        var result = VehicleIntentComparisonFixture.Compare(
            intended, 0.5d, Timebase, Frame, Unit, Sample(new Vector3(extreme, 0f, 0f)));

        Assert.Equal(ComparisonStatus.Unresolved, result.Status);
        Assert.Null(result.PositionDelta);
        Assert.Contains("delta", result.Reason!, StringComparison.OrdinalIgnoreCase);
    }

    private static VehicleSample Sample(Vector3 position) =>
        new(0.5d, Timebase, Frame, Unit, position);

    private static GestureDefinition CreateGesture(bool includeRoot)
    {
        var start = new Dictionary<string, PoseTransform>();
        var end = new Dictionary<string, PoseTransform>();
        if (includeRoot)
        {
            start["root"] = new PoseTransform(Vector3.Zero, Quaternion.Identity, Vector3.One);
            end["root"] = new PoseTransform(new Vector3(10f, 0f, 0f), Quaternion.Identity, Vector3.One);
        }

        return new GestureDefinition("vehicle-approach-reference",
        [
            new GestureKeyframe(0d, start),
            new GestureKeyframe(1d, end)
        ]);
    }
}
