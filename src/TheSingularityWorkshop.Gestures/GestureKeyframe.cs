using System.Collections.ObjectModel;

namespace TheSingularityWorkshop.Gestures;

/// <summary>A normalized point in a Gesture's ideal pose progression.</summary>
public sealed class GestureKeyframe
{
    /// <summary>Creates a keyframe and defensively copies its transforms.</summary>
    public GestureKeyframe(double time, IReadOnlyDictionary<string, PoseTransform> localTransforms)
    {
        if (!double.IsFinite(time) || time < 0d || time > 1d)
            throw new ArgumentOutOfRangeException(nameof(time), "Keyframe time must be finite and within [0, 1].");
        ArgumentNullException.ThrowIfNull(localTransforms);
        var copy = new Dictionary<string, PoseTransform>(StringComparer.Ordinal);
        foreach (var (name, transform) in localTransforms)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Transform names must not be empty.", nameof(localTransforms));
            transform.ValidateFinite(nameof(localTransforms));
            copy.Add(name, transform);
        }
        Time = time;
        LocalTransforms = new ReadOnlyDictionary<string, PoseTransform>(copy);
    }

    /// <summary>Normalized progression time in [0, 1].</summary>
    public double Time { get; }
    /// <summary>Joint/transform local poses at this point in the procedure.</summary>
    public IReadOnlyDictionary<string, PoseTransform> LocalTransforms { get; }
}
