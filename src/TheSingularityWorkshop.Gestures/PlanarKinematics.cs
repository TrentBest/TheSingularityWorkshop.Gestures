using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Joint-angle limits for a planar revolute joint. Angles are expressed in radians.
/// </summary>
/// <param name="MinimumRadians">Inclusive lower angle limit.</param>
/// <param name="MaximumRadians">Inclusive upper angle limit.</param>
public readonly record struct JointAngleLimits(float MinimumRadians, float MaximumRadians);

/// <summary>
/// The result of solving a two-link planar inverse-kinematics target.
/// </summary>
/// <param name="ShoulderRadians">Solved shoulder angle relative to the positive X axis.</param>
/// <param name="ElbowRadians">Solved elbow angle relative to the first link.</param>
/// <param name="EndEffector">Computed end-effector position from the solved angles.</param>
/// <param name="Target">Requested target position.</param>
/// <param name="ErrorDistance">Euclidean distance between the computed end effector and requested target.</param>
/// <param name="TargetReached">Whether the requested target was reached within the solver tolerance.</param>
public readonly record struct PlanarIkResult(
    float ShoulderRadians,
    float ElbowRadians,
    Vector2 EndEffector,
    Vector2 Target,
    float ErrorDistance,
    bool TargetReached);

/// <summary>
/// Deterministic, engine-neutral reference mathematics for a fixed-anchor, two-link planar arm.
/// This is a CPU reference implementation, not a GPU solver or a general-purpose rig system.
/// </summary>
public static class PlanarKinematics
{
    private const float TargetTolerance = 1e-4f;
    private const float TwoPi = 2f * MathF.PI;

    /// <summary>
    /// Computes the end-effector position for a fixed anchor at the origin and two planar links.
    /// The shoulder angle is relative to +X; the elbow angle is relative to the first link.
    /// </summary>
    public static Vector2 ForwardKinematics(
        float firstLinkLength,
        float secondLinkLength,
        float shoulderRadians,
        float elbowRadians)
    {
        ValidateLengths(firstLinkLength, secondLinkLength);
        ValidateFinite(shoulderRadians, nameof(shoulderRadians));
        ValidateFinite(elbowRadians, nameof(elbowRadians));

        var firstX = firstLinkLength * MathF.Cos(shoulderRadians);
        var firstY = firstLinkLength * MathF.Sin(shoulderRadians);
        var combinedAngle = shoulderRadians + elbowRadians;

        return new Vector2(
            firstX + secondLinkLength * MathF.Cos(combinedAngle),
            firstY + secondLinkLength * MathF.Sin(combinedAngle));
    }

    /// <summary>
    /// Solves a two-link planar target using both elbow configurations, then chooses the
    /// joint-limit-clamped candidate with the smallest end-effector error. Unreachable targets
    /// return the best constrained pose rather than pretending the target was reached.
    /// </summary>
    public static PlanarIkResult SolveTwoLink(
        float firstLinkLength,
        float secondLinkLength,
        Vector2 target,
        JointAngleLimits shoulderLimits,
        JointAngleLimits elbowLimits)
    {
        ValidateLengths(firstLinkLength, secondLinkLength);
        ValidateFinite(target.X, nameof(target));
        ValidateFinite(target.Y, nameof(target));
        ValidateLimits(shoulderLimits, nameof(shoulderLimits));
        ValidateLimits(elbowLimits, nameof(elbowLimits));

        var distanceSquared = target.LengthSquared();
        var cosineElbow = (distanceSquared - firstLinkLength * firstLinkLength
            - secondLinkLength * secondLinkLength)
            / (2f * firstLinkLength * secondLinkLength);
        cosineElbow = Math.Clamp(cosineElbow, -1f, 1f);

        var elbowMagnitude = MathF.Acos(cosineElbow);
        var targetAngle = MathF.Atan2(target.Y, target.X);

        var positive = EvaluateCandidate(
            firstLinkLength, secondLinkLength, target, targetAngle, elbowMagnitude,
            shoulderLimits, elbowLimits);
        var negative = EvaluateCandidate(
            firstLinkLength, secondLinkLength, target, targetAngle, -elbowMagnitude,
            shoulderLimits, elbowLimits);

        var best = positive.ErrorDistance <= negative.ErrorDistance ? positive : negative;
        return best with { TargetReached = best.ErrorDistance <= TargetTolerance };
    }

    private static PlanarIkResult EvaluateCandidate(
        float firstLinkLength,
        float secondLinkLength,
        Vector2 target,
        float targetAngle,
        float elbowAngle,
        JointAngleLimits shoulderLimits,
        JointAngleLimits elbowLimits)
    {
        var shoulderAngle = targetAngle - MathF.Atan2(
            secondLinkLength * MathF.Sin(elbowAngle),
            firstLinkLength + secondLinkLength * MathF.Cos(elbowAngle));

        shoulderAngle = ClampEquivalentAngle(shoulderAngle, shoulderLimits);
        elbowAngle = ClampEquivalentAngle(elbowAngle, elbowLimits);

        var endEffector = ForwardKinematics(
            firstLinkLength, secondLinkLength, shoulderAngle, elbowAngle);
        var error = Vector2.Distance(endEffector, target);

        return new PlanarIkResult(
            shoulderAngle, elbowAngle, endEffector, target, error, error <= TargetTolerance);
    }

    private static float ClampEquivalentAngle(float angle, JointAngleLimits limits)
    {
        // Select the equivalent angle (angle + 2πk) nearest the legal interval before clamping.
        var center = (limits.MinimumRadians + limits.MaximumRadians) * 0.5f;
        var turns = MathF.Round((center - angle) / TwoPi);
        var equivalent = angle + turns * TwoPi;
        return Math.Clamp(equivalent, limits.MinimumRadians, limits.MaximumRadians);
    }

    private static void ValidateLengths(float first, float second)
    {
        ValidateFinite(first, nameof(first));
        ValidateFinite(second, nameof(second));

        if (first <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(first), "Link length must be positive.");
        }

        if (second <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(second), "Link length must be positive.");
        }
    }

    private static void ValidateLimits(JointAngleLimits limits, string parameterName)
    {
        ValidateFinite(limits.MinimumRadians, parameterName);
        ValidateFinite(limits.MaximumRadians, parameterName);

        if (limits.MinimumRadians > limits.MaximumRadians)
        {
            throw new ArgumentException("Minimum angle must not exceed maximum angle.", parameterName);
        }
    }

    private static void ValidateFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}
