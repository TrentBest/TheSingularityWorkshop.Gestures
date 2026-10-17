namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// An immutable proposal to replace an experience definition using optimistic concurrency.
/// This is not a live-session mutation and does not apply itself.
/// </summary>
public sealed class ExperienceDefinitionPatch
{
    public ExperienceDefinitionPatch(string experienceId, int expectedVersion, ExperienceDefinition replacement)
    {
        if (string.IsNullOrWhiteSpace(experienceId))
            throw new ArgumentException("Experience ID must not be empty.", nameof(experienceId));
        if (expectedVersion < 1)
            throw new ArgumentOutOfRangeException(nameof(expectedVersion), "Expected version must be positive.");
        ArgumentNullException.ThrowIfNull(replacement);
        if (!string.Equals(experienceId, replacement.Id, StringComparison.Ordinal))
            throw new ArgumentException("Replacement must target the same experience ID.", nameof(replacement));
        if (replacement.Version <= expectedVersion)
            throw new ArgumentException("Replacement version must be greater than the expected version.", nameof(replacement));

        ExperienceId = experienceId;
        ExpectedVersion = expectedVersion;
        Replacement = replacement;
    }

    /// <summary>Stable ID of the definition being revised.</summary>
    public string ExperienceId { get; }

    /// <summary>Definition version the author expected to replace.</summary>
    public int ExpectedVersion { get; }

    /// <summary>Complete validated replacement definition, not a mutable operation list.</summary>
    public ExperienceDefinition Replacement { get; }

    /// <summary>
    /// Checks whether the proposal was authored against the currently observed definition.
    /// A consumer must still coordinate application at a safe boundary.
    /// </summary>
    public bool CanApplyTo(ExperienceDefinition current) =>
        current is not null
        && string.Equals(current.Id, ExperienceId, StringComparison.Ordinal)
        && current.Version == ExpectedVersion;
}
