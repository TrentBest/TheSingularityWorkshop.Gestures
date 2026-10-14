namespace TheSingularityWorkshop.Gestures;

/// <summary>Classifies a joint's observed angular movement relative to its local coordinate axis.</summary>
/// <remarks>This is one motion classification, not the complete physical state of a joint.</remarks>
public enum JointMotionState
{
    /// <summary>Angle is increasing beyond the stationary tolerance.</summary>
    MovingPositive,
    /// <summary>Angle is decreasing beyond the stationary tolerance.</summary>
    MovingNegative,
    /// <summary>Angle change is within the stationary tolerance.</summary>
    Stationary
}

/// <summary>Result of one deterministic, speed-limited hinge update.</summary>
/// <param name="PreviousAngleRadians">Angle before this update.</param>
/// <param name="AngleRadians">Angle after this update.</param>
/// <param name="RequestedAngleRadians">Original requested angle, before limit handling.</param>
/// <param name="MotionState">Observed movement classification for this update.</param>
/// <param name="TargetReached">Whether the requested angle itself was reached within tolerance.</param>
/// <param name="JointLimitConstrained">Whether the requested angle lies outside the legal interval.</param>
public readonly record struct StatefulHingeStep(
    float PreviousAngleRadians,
    float AngleRadians,
    float RequestedAngleRadians,
    JointMotionState MotionState,
    bool TargetReached,
    bool JointLimitConstrained);

/// <summary>
/// Minimal stateful hinge controller used to research retained joint state and bounded transitions.
/// It advances a single angle toward a request; it does not simulate torque, inertia, collision,
/// contact, or physical forces.
/// </summary>
public sealed class StatefulHinge
{
    private readonly float _minimumRadians;
    private readonly float _maximumRadians;
    private readonly float _stationaryToleranceRadians;

    /// <summary>Creates a hinge with an initial angle, legal range, and movement classification tolerance.</summary>
    public StatefulHinge(
        float initialAngleRadians,
        float minimumRadians,
        float maximumRadians,
        float stationaryToleranceRadians = 1e-5f)
    {
        ValidateFinite(initialAngleRadians, nameof(initialAngleRadians));
        ValidateFinite(minimumRadians, nameof(minimumRadians));
        ValidateFinite(maximumRadians, nameof(maximumRadians));
        ValidateFinite(stationaryToleranceRadians, nameof(stationaryToleranceRadians));

        if (minimumRadians > maximumRadians)
            throw new ArgumentException("Minimum angle must not exceed maximum angle.");
        if (initialAngleRadians < minimumRadians || initialAngleRadians > maximumRadians)
            throw new ArgumentOutOfRangeException(nameof(initialAngleRadians), "Initial angle must be within the legal range.");
        if (stationaryToleranceRadians < 0f)
            throw new ArgumentOutOfRangeException(nameof(stationaryToleranceRadians), "Tolerance must not be negative.");

        _minimumRadians = minimumRadians;
        _maximumRadians = maximumRadians;
        _stationaryToleranceRadians = stationaryToleranceRadians;
        AngleRadians = initialAngleRadians;
    }

    /// <summary>The currently retained hinge angle in radians.</summary>
    public float AngleRadians { get; private set; }

    /// <summary>The inclusive minimum legal hinge angle in radians.</summary>
    public float MinimumRadians => _minimumRadians;

    /// <summary>The inclusive maximum legal hinge angle in radians.</summary>
    public float MaximumRadians => _maximumRadians;

    // Internal invalidation signal: hierarchy nodes subscribe so an angle change dirties its ancestor path.
    internal event Action? AngleChanged;

    /// <summary>
    /// Advances toward a requested angle by at most maximumAngularSpeedRadiansPerSecond * deltaTimeSeconds.
    /// Out-of-range requests are constrained to the nearest legal limit and explicitly reported as unmet.
    /// </summary>
    public StatefulHingeStep Advance(
        float requestedAngleRadians,
        float deltaTimeSeconds,
        float maximumAngularSpeedRadiansPerSecond)
    {
        ValidateFinite(requestedAngleRadians, nameof(requestedAngleRadians));
        ValidateFinite(deltaTimeSeconds, nameof(deltaTimeSeconds));
        ValidateFinite(maximumAngularSpeedRadiansPerSecond, nameof(maximumAngularSpeedRadiansPerSecond));

        if (deltaTimeSeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds), "Delta time must not be negative.");
        if (maximumAngularSpeedRadiansPerSecond < 0f)
            throw new ArgumentOutOfRangeException(nameof(maximumAngularSpeedRadiansPerSecond), "Maximum angular speed must not be negative.");

        var previous = AngleRadians;
        var constrainedTarget = Math.Clamp(requestedAngleRadians, _minimumRadians, _maximumRadians);
        var remaining = constrainedTarget - previous;
        var maximumStep = maximumAngularSpeedRadiansPerSecond * deltaTimeSeconds;
        var step = Math.Clamp(remaining, -maximumStep, maximumStep);
        AngleRadians = Math.Clamp(previous + step, _minimumRadians, _maximumRadians);
        if (AngleRadians != previous)
            AngleChanged?.Invoke();

        var change = AngleRadians - previous;
        var motionState = change > _stationaryToleranceRadians
            ? JointMotionState.MovingPositive
            : change < -_stationaryToleranceRadians
                ? JointMotionState.MovingNegative
                : JointMotionState.Stationary;
        var jointLimitConstrained = requestedAngleRadians < _minimumRadians
            || requestedAngleRadians > _maximumRadians;
        var targetReached = !jointLimitConstrained
            && MathF.Abs(AngleRadians - requestedAngleRadians) <= _stationaryToleranceRadians;

        return new StatefulHingeStep(
            previous,
            AngleRadians,
            requestedAngleRadians,
            motionState,
            targetReached,
            jointLimitConstrained);
    }

    private static void ValidateFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
    }
}
