using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>Outcome of one stateful, speed-limited planar IK control step.</summary>
public enum StatefulPlanarIkStatus
{
    /// <summary>The measured endpoint is within the planar solver's target tolerance.</summary>
    Reached,
    /// <summary>The target is geometrically feasible, but the retained hinges have not reached the requested angles yet.</summary>
    Moving,
    /// <summary>The constrained geometric reference could not reach the requested target.</summary>
    Approximate
}

/// <summary>Measured result of one stateful planar IK step.</summary>
/// <param name="RequestedSolution">The geometric reference solution for this target.</param>
/// <param name="ShoulderStep">The retained shoulder hinge's bounded update.</param>
/// <param name="ElbowStep">The retained elbow hinge's bounded update.</param>
/// <param name="EndEffector">The endpoint measured after updating and evaluating the hierarchy.</param>
/// <param name="ErrorDistance">Distance from the measured endpoint to the requested world-space target.</param>
/// <param name="Status">Kinematic status after this step; this is not a physical-contact result.</param>
public readonly record struct StatefulPlanarIkStep(
    PlanarIkResult RequestedSolution,
    StatefulHingeStep ShoulderStep,
    StatefulHingeStep ElbowStep,
    Vector2 EndEffector,
    float ErrorDistance,
    StatefulPlanarIkStatus Status);

/// <summary>
/// Connects the two-link planar geometry reference to retained, speed-limited hinges and a measured hierarchy.
/// This is a kinematic control loop, not a dynamics or contact solver.
/// </summary>
public sealed class StatefulPlanarIkController
{
    private readonly StatefulJointNode _shoulder;
    private readonly StatefulJointNode _elbow;
    private readonly JointAngleLimits _shoulderLimits;
    private readonly JointAngleLimits _elbowLimits;

    /// <summary>
    /// Creates a controller for a root link and its first child link.
    /// The two nodes must use link lengths and hinge limits consistent with the supplied geometry.
    /// </summary>
    public StatefulPlanarIkController(
        StatefulJointNode shoulder,
        StatefulJointNode elbow,
        JointAngleLimits shoulderLimits,
        JointAngleLimits elbowLimits)
    {
        ArgumentNullException.ThrowIfNull(shoulder);
        ArgumentNullException.ThrowIfNull(elbow);
        if (shoulder.Parent is not null)
            throw new ArgumentException("Shoulder node must be the hierarchy root.", nameof(shoulder));
        if (!ReferenceEquals(elbow.Parent, shoulder))
            throw new ArgumentException("Elbow node must be a direct child of the shoulder node.", nameof(elbow));

        ValidateLimits(shoulderLimits, nameof(shoulderLimits));
        ValidateLimits(elbowLimits, nameof(elbowLimits));
        ValidateMatchingLimits(shoulder.Hinge, shoulderLimits, nameof(shoulderLimits));
        ValidateMatchingLimits(elbow.Hinge, elbowLimits, nameof(elbowLimits));

        _shoulder = shoulder;
        _elbow = elbow;
        _shoulderLimits = shoulderLimits;
        _elbowLimits = elbowLimits;
    }

    /// <summary>
    /// Solves the requested world-space target geometrically, advances each retained hinge within its limits,
    /// evaluates the hierarchy, and reports the endpoint actually produced by that step.
    /// </summary>
    public StatefulPlanarIkStep Advance(
        Vector2 rootAnchor,
        Vector2 target,
        float deltaTimeSeconds,
        float maximumShoulderSpeedRadiansPerSecond,
        float maximumElbowSpeedRadiansPerSecond)
    {
        ValidateFinite(rootAnchor.X, nameof(rootAnchor));
        ValidateFinite(rootAnchor.Y, nameof(rootAnchor));
        ValidateFinite(target.X, nameof(target));
        ValidateFinite(target.Y, nameof(target));

        // Validate update inputs before mutating either hinge.
        ValidateNonNegativeFinite(deltaTimeSeconds, nameof(deltaTimeSeconds));
        ValidateNonNegativeFinite(maximumShoulderSpeedRadiansPerSecond, nameof(maximumShoulderSpeedRadiansPerSecond));
        ValidateNonNegativeFinite(maximumElbowSpeedRadiansPerSecond, nameof(maximumElbowSpeedRadiansPerSecond));

        var localTarget = target - rootAnchor;
        if (!float.IsFinite(localTarget.X) || !float.IsFinite(localTarget.Y))
            throw new ArgumentOutOfRangeException(nameof(target), "Target offset must be finite.");

        var solution = PlanarKinematics.SolveTwoLink(
            _shoulder.LinkLength,
            _elbow.LinkLength,
            localTarget,
            _shoulderLimits,
            _elbowLimits);

        var shoulderStep = _shoulder.Hinge.Advance(
            solution.ShoulderRadians, deltaTimeSeconds, maximumShoulderSpeedRadiansPerSecond);
        var elbowStep = _elbow.Hinge.Advance(
            solution.ElbowRadians, deltaTimeSeconds, maximumElbowSpeedRadiansPerSecond);

        _shoulder.EvaluateIncremental(rootAnchor);
        var endEffector = _elbow.WorldEnd;
        var errorDistance = Vector2.Distance(endEffector, target);
        if (!float.IsFinite(errorDistance))
            throw new InvalidOperationException("Measured endpoint error is not finite.");

        var status = errorDistance <= 1e-4f
            ? StatefulPlanarIkStatus.Reached
            : solution.TargetReached
                ? StatefulPlanarIkStatus.Moving
                : StatefulPlanarIkStatus.Approximate;

        return new StatefulPlanarIkStep(
            solution,
            shoulderStep,
            elbowStep,
            endEffector,
            errorDistance,
            status);
    }

    private static void ValidateMatchingLimits(StatefulHinge hinge, JointAngleLimits limits, string parameterName)
    {
        if (hinge.MinimumRadians != limits.MinimumRadians || hinge.MaximumRadians != limits.MaximumRadians)
        {
            throw new ArgumentException(
                "Solver limits must exactly match the corresponding retained hinge's legal range.",
                parameterName);
        }
    }

    private static void ValidateLimits(JointAngleLimits limits, string parameterName)
    {
        ValidateFinite(limits.MinimumRadians, parameterName);
        ValidateFinite(limits.MaximumRadians, parameterName);
        if (limits.MinimumRadians > limits.MaximumRadians)
            throw new ArgumentException("Minimum angle must not exceed maximum angle.", parameterName);
    }

    private static void ValidateNonNegativeFinite(float value, string parameterName)
    {
        ValidateFinite(value, parameterName);
        if (value < 0f)
            throw new ArgumentOutOfRangeException(parameterName, "Value must not be negative.");
    }

    private static void ValidateFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
    }
}
