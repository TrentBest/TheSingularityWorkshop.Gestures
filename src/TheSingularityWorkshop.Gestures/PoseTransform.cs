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

    private static Quaternion NormalizeOrIdentity(Quaternion value)
    {
        // Scale first so LengthSquared cannot overflow for very large, finite components.
        var maxComponent = MathF.Max(
            MathF.Max(MathF.Abs(value.X), MathF.Abs(value.Y)),
            MathF.Max(MathF.Abs(value.Z), MathF.Abs(value.W)));

        if (maxComponent == 0f)
            return Quaternion.Identity;

        var scaled = new Quaternion(
            value.X / maxComponent,
            value.Y / maxComponent,
            value.Z / maxComponent,
            value.W / maxComponent);
        var scaledLengthSquared = scaled.LengthSquared();

        // Preserve the existing near-zero fallback without squaring a large component.
        if (maxComponent < 1e-6f &&
            scaledLengthSquared * maxComponent * maxComponent < 1e-12f)
            return Quaternion.Identity;

        return Quaternion.Normalize(scaled);
    }
}
