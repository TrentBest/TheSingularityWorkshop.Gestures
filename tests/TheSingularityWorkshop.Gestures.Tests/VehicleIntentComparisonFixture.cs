using System.Numerics;
using TheSingularityWorkshop.Gestures;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Test-owned consumer adapter: compares an evaluated Gesture reference with a separate
/// achieved vehicle sample. It is deliberately not part of the Gestures production API.
/// </summary>
internal static class VehicleIntentComparisonFixture
{
    internal enum ComparisonStatus
    {
        Comparable,
        Unresolved
    }

    internal readonly record struct VehicleSample(
        double TimestampSeconds,
        string TimebaseId,
        string CoordinateFrameId,
        string LengthUnit,
        Vector3 RootPosition);

    internal sealed record Comparison(
        ComparisonStatus Status,
        string? Reason,
        Vector3? PositionDelta);

    internal static Comparison Compare(
        EvaluatedPose intended,
        double intendedTimestampSeconds,
        string intendedTimebaseId,
        string intendedCoordinateFrameId,
        string intendedLengthUnit,
        VehicleSample achieved)
    {
        ArgumentNullException.ThrowIfNull(intended);
        if (!double.IsFinite(intendedTimestampSeconds))
            throw new ArgumentOutOfRangeException(nameof(intendedTimestampSeconds));
        if (string.IsNullOrWhiteSpace(intendedTimebaseId))
            throw new ArgumentException("Timebase id is required.", nameof(intendedTimebaseId));
        if (string.IsNullOrWhiteSpace(intendedCoordinateFrameId))
            throw new ArgumentException("Coordinate frame id is required.", nameof(intendedCoordinateFrameId));
        if (string.IsNullOrWhiteSpace(intendedLengthUnit))
            throw new ArgumentException("Length unit is required.", nameof(intendedLengthUnit));
        if (!double.IsFinite(achieved.TimestampSeconds))
            throw new ArgumentOutOfRangeException(nameof(achieved));
        if (string.IsNullOrWhiteSpace(achieved.TimebaseId) ||
            string.IsNullOrWhiteSpace(achieved.CoordinateFrameId) ||
            string.IsNullOrWhiteSpace(achieved.LengthUnit))
            return Unresolved("Achieved sample is missing timebase, coordinate frame, or length unit.");
        if (intendedTimestampSeconds != achieved.TimestampSeconds ||
            !StringComparer.Ordinal.Equals(intendedTimebaseId, achieved.TimebaseId))
            return Unresolved("Samples are not aligned to the same instant and timebase.");
        if (!StringComparer.Ordinal.Equals(intendedCoordinateFrameId, achieved.CoordinateFrameId))
            return Unresolved("Coordinate frames differ; a consumer-owned transform is required.");
        if (!StringComparer.Ordinal.Equals(intendedLengthUnit, achieved.LengthUnit))
            return Unresolved("Length units differ; a consumer-owned conversion is required.");
        if (!intended.LocalTransforms.TryGetValue("root", out var intendedRoot))
            return Unresolved("Gesture reference has no 'root' transform.");
        if (!IsFinite(achieved.RootPosition))
            return Unresolved("Achieved root position is not finite.");

        return new Comparison(
            ComparisonStatus.Comparable,
            null,
            achieved.RootPosition - intendedRoot.Position);
    }

    private static Comparison Unresolved(string reason) =>
        new(ComparisonStatus.Unresolved, reason, null);

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}
