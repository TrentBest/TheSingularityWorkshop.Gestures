using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Supplies a domain-owned target for a contact recovery intent.
/// Implementations may consult scene geometry, available holds, body state, or application rules.
/// They must not assume that a selected target is reachable or that contact will succeed.
/// </summary>
public interface IContactRecoveryTargetSelector
{
    /// <summary>
    /// Attempts to select a world-space target for the requested recovery intent.
    /// Return false when the domain cannot currently provide a suitable target.
    /// </summary>
    bool TrySelectTarget(
        ContactRecoveryAction action,
        ContactObservation observation,
        Vector2 currentTarget,
        out Vector2 target);
}

/// <summary>
/// Connects contact recovery recommendations to caller-owned target selection without inventing target geometry.
/// </summary>
/// <remarks>
/// This helper only routes intent. The selector owns target choice, the kinematic controller owns pose evaluation,
/// and the external physics/domain model remains authoritative for subsequent contact outcomes.
/// </remarks>
public static class ContactRecoveryTargetSelection
{
    /// <summary>
    /// Recommends a response and, when needed, asks the supplied domain selector for a new target.
    /// Returns false when an observation is needed, no selector is supplied for a recovery intent,
    /// or the selector cannot provide a finite target.
    /// </summary>
    public static bool TrySelectNextTarget(
        ContactObservation? observation,
        Vector2 currentTarget,
        IContactRecoveryTargetSelector? selector,
        out ContactRecoveryAction action,
        out Vector2 nextTarget)
    {
        ValidateFinite(currentTarget, nameof(currentTarget));

        action = ContactRecoveryPolicy.Recommend(observation);
        nextTarget = currentTarget;

        if (action == ContactRecoveryAction.AwaitObservation)
            return false;

        if (action == ContactRecoveryAction.ContinueCurrentTarget)
            return true;

        if (observation is null || selector is null)
            return false;

        if (!selector.TrySelectTarget(action, observation, currentTarget, out var candidate))
            return false;

        if (!float.IsFinite(candidate.X) || !float.IsFinite(candidate.Y))
            return false;

        nextTarget = candidate;
        return true;
    }

    private static void ValidateFinite(Vector2 target, string parameterName)
    {
        if (!float.IsFinite(target.X) || !float.IsFinite(target.Y))
            throw new ArgumentOutOfRangeException(parameterName, "Target coordinates must be finite.");
    }
}
