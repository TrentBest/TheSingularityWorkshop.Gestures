using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Result of one contact-informed kinematic update.
/// A missing kinematic step means the controller deliberately did not advance because no safe next target was selected.
/// </summary>
public readonly record struct StatefulPlanarIkRecoveryStep(
    ContactRecoveryAction RecoveryAction,
    bool TargetSelected,
    Vector2 CurrentTarget,
    StatefulPlanarIkStep? KinematicStep);

/// <summary>
/// Connects externally assessed contact outcomes, domain-owned target selection, and the stateful planar IK controller.
/// </summary>
/// <remarks>
/// This coordinator does not simulate contact or decide which target is physically safe. It asks the caller's
/// <see cref="IContactRecoveryTargetSelector"/> for a target and advances IK only when target selection succeeds.
/// A selected target and a kinematic endpoint are not evidence of contact or load-bearing success; the external
/// physics/domain owner must provide the next <see cref="ContactObservation"/>.
/// </remarks>
public sealed class StatefulPlanarIkRecoveryController
{
    private readonly StatefulPlanarIkController _kinematics;
    private readonly IContactRecoveryTargetSelector _targetSelector;
    private Vector2 _currentTarget;

    /// <summary>Creates a recovery coordinator with an initial world-space target and domain-owned target selector.</summary>
    public StatefulPlanarIkRecoveryController(
        StatefulPlanarIkController kinematics,
        IContactRecoveryTargetSelector targetSelector,
        Vector2 initialTarget)
    {
        ArgumentNullException.ThrowIfNull(kinematics);
        ArgumentNullException.ThrowIfNull(targetSelector);
        ValidateFinite(initialTarget, nameof(initialTarget));

        _kinematics = kinematics;
        _targetSelector = targetSelector;
        _currentTarget = initialTarget;
    }

    /// <summary>The last target accepted by this coordinator.</summary>
    public Vector2 CurrentTarget => _currentTarget;

    /// <summary>
    /// Processes an external contact observation and advances kinematics only if the recovery policy
    /// can retain the current target or the domain selector supplies a finite replacement.
    /// </summary>
    public StatefulPlanarIkRecoveryStep Advance(
        ContactObservation? observation,
        Vector2 rootAnchor,
        float deltaTimeSeconds,
        float maximumShoulderSpeedRadiansPerSecond,
        float maximumElbowSpeedRadiansPerSecond)
    {
        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            observation,
            _currentTarget,
            _targetSelector,
            out var action,
            out var nextTarget);

        if (!selected)
        {
            return new StatefulPlanarIkRecoveryStep(
                action,
                false,
                _currentTarget,
                null);
        }

        var kinematicStep = _kinematics.Advance(
            rootAnchor,
            nextTarget,
            deltaTimeSeconds,
            maximumShoulderSpeedRadiansPerSecond,
            maximumElbowSpeedRadiansPerSecond);

        _currentTarget = nextTarget;
        return new StatefulPlanarIkRecoveryStep(
            action,
            true,
            _currentTarget,
            kinematicStep);
    }

    private static void ValidateFinite(Vector2 target, string parameterName)
    {
        if (!float.IsFinite(target.X) || !float.IsFinite(target.Y))
            throw new ArgumentOutOfRangeException(parameterName, "Target coordinates must be finite.");
    }
}
