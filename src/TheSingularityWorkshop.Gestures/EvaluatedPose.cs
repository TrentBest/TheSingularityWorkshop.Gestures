using System.Collections.ObjectModel;
using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>Pose output for one evaluation instant; it is presentation data, not authoritative world state.</summary>
public sealed class EvaluatedPose
{
    internal EvaluatedPose(double normalizedProgress, bool isComplete,
        IReadOnlyDictionary<string, PoseTransform> localTransforms, Vector3 rootOffset)
    {
        NormalizedProgress = normalizedProgress;
        IsComplete = isComplete;
        LocalTransforms = new ReadOnlyDictionary<string, PoseTransform>(
            new Dictionary<string, PoseTransform>(localTransforms, StringComparer.Ordinal));
        RootOffset = rootOffset;
    }

    /// <summary>Clamped normalized progress in [0, 1].</summary>
    public double NormalizedProgress { get; }
    /// <summary>True when the requested evaluation time reaches or exceeds the Gesture duration.</summary>
    public bool IsComplete { get; }
    /// <summary>Evaluated local transform map.</summary>
    public IReadOnlyDictionary<string, PoseTransform> LocalTransforms { get; }
    /// <summary>Small deterministic offset to apply at the presentation/root-projection layer.</summary>
    public Vector3 RootOffset { get; }
}
