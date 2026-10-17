namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// An immutable proposal to replace an activity definition using optimistic concurrency.
/// This is not a live-session mutation and does not apply itself.
/// </summary>
public sealed class ActivityDefinitionPatch
{
    public ActivityDefinitionPatch(string activityId, int expectedVersion, ActivityDefinition replacement)
    {
        if (string.IsNullOrWhiteSpace(activityId))
            throw new ArgumentException("Activity ID must not be empty.", nameof(activityId));
        if (expectedVersion < 1)
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version must be positive.");
        ArgumentNullException.ThrowIfNull(replacement);
        if (!string.Equals(activityId, replacement.Id, StringComparison.Ordinal))
            throw new ArgumentException("Replacement must target the same activity ID.", nameof(replacement));
        if (replacement.Version <= expectedVersion)
            throw new ArgumentException("Replacement version must be greater than the expected version.", nameof(replacement));

        ActivityId = activityId;
        ExpectedVersion = expectedVersion;
        Replacement = replacement;
    }

    /// <summary>Stable ID of the definition being revised.</summary>
    public string ActivityId { get; }

    /// <summary>Definition version the author expected to replace.</summary>
    public int ExpectedVersion { get; }

    /// <summary>Complete validated replacement definition, not a mutable operation list.</summary>
    public ActivityDefinition Replacement { get; }

    /// <summary>
    /// Checks whether the proposal was authored against the currently observed definition.
    /// A consumer must still coordinate application at a safe boundary.
    /// </summary>
    public bool CanApplyTo(ActivityDefinition current) =>
        current is not null
        && string.Equals(current.Id, ActivityId, StringComparison.Ordinal)
        && current.Version == ExpectedVersion;
}
