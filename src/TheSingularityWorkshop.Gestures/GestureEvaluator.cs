using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>Deterministic CPU reference evaluator for optimized and future GPU implementations to match.</summary>
public static class GestureEvaluator
{
    /// <summary>Evaluates a Gesture at elapsed time using optional stable per-entity variation.</summary>
    public static EvaluatedPose Evaluate(
        GestureDefinition definition,
        double elapsedSeconds,
        double durationSeconds,
        GestureVariation? variation = null,
        ulong identitySeed = 0)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!double.IsFinite(elapsedSeconds))
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Elapsed time must be finite.");
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");

        variation ??= GestureVariation.None;
        variation.Validate();
        var speedUnit = ToUnitInterval(Mix(identitySeed));
        var speed = 1d + ((speedUnit * 2d - 1d) * variation.MaximumSpeedDeviation);
        var progress = Math.Clamp(Math.Max(0d, elapsedSeconds) * speed / durationSeconds, 0d, 1d);

        var rightIndex = 1;
        while (rightIndex < definition.Keyframes.Count - 1 && definition.Keyframes[rightIndex].Time < progress)
            rightIndex++;
        var left = definition.Keyframes[rightIndex - 1];
        var right = definition.Keyframes[rightIndex];
        var segmentLength = right.Time - left.Time;
        var amount = segmentLength <= 0d ? 1f : (float)((progress - left.Time) / segmentLength);

        var names = new HashSet<string>(left.LocalTransforms.Keys, StringComparer.Ordinal);
        names.UnionWith(right.LocalTransforms.Keys);
        var transforms = new Dictionary<string, PoseTransform>(names.Count, StringComparer.Ordinal);
        foreach (var name in names)
        {
            var from = left.LocalTransforms.TryGetValue(name, out var fromPose) ? fromPose : right.LocalTransforms[name];
            var to = right.LocalTransforms.TryGetValue(name, out var toPose) ? toPose : from;
            transforms.Add(name, PoseTransform.Interpolate(from, to, amount));
        }

        var lateralUnit = ToUnitInterval(Mix(identitySeed ^ 0x9E3779B97F4A7C15UL));
        var lateral = (float)((lateralUnit * 2d - 1d) * variation.MaximumLateralOffset);
        return new EvaluatedPose(progress, progress >= 1d, transforms, new Vector3(lateral, 0f, 0f));
    }

    private static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    private static double ToUnitInterval(ulong value) => (value >> 11) * (1.0 / (1UL << 53));
}
