using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>Engine-neutral local transform used by Gesture keyframes and evaluated poses.</summary>
public readonly record struct PoseTransform(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    /// <summary>The identity transform.</summary>
    public static PoseTransform Identity { get; } =
        new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    /// <summary>Interpolates position and scale linearly and rotation with spherical interpolation.</summary>
    public static PoseTransform Interpolate(PoseTransform from, PoseTransform to, float amount)
    {
        var t = Math.Clamp(amount, 0f, 1f);
        var rotation = Quaternion.Slerp(NormalizeOrIdentity(from.Rotation), NormalizeOrIdentity(to.Rotation), t);
        return new PoseTransform(Vector3.Lerp(from.Position, to.Position, t),
            Quaternion.Normalize(rotation), Vector3.Lerp(from.Scale, to.Scale, t));
    }

    internal void ValidateFinite(string parameterName)
    {
        if (!IsFinite(Position) || !IsFinite(Rotation) || !IsFinite(Scale))
            throw new ArgumentException("Transform position, rotation, and scale components must be finite.", parameterName);
    }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private static bool IsFinite(Quaternion value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) &&
        float.IsFinite(value.Z) && float.IsFinite(value.W);

    private static Quaternion NormalizeOrIdentity(Quaternion value) =>
        value.LengthSquared() < 1e-12f ? Quaternion.Identity : Quaternion.Normalize(value);
}
