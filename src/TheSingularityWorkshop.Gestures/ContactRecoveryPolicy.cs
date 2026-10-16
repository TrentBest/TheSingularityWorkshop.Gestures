namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A kinematic/domain-level response to an externally reported contact outcome.
/// These are recommendations for the caller's next intent, not physical actions.
/// </summary>
public enum ContactRecoveryAction
{
    /// <summary>Keep the current target while the external model reports adequate support.</summary>
    ContinueCurrentTarget,
    /// <summary>Choose a target intended to establish contact.</summary>
    SeekContact,
    /// <summary>Choose a new target after a previously present contact was released or lost.</summary>
    ReacquireContact,
    /// <summary>Choose an alternate contact target after the current contact is reported to be slipping.</summary>
    SeekAlternativeContact,
    /// <summary>Choose a target or pose intended to reduce the load on an unsupported contact.</summary>
    RepositionToReduceLoad,
    /// <summary>Wait for a useful observation before choosing a contact-dependent response.</summary>
    AwaitObservation
}

/// <summary>
/// Maps externally assessed contact observations to deterministic next-intent recommendations.
/// </summary>
/// <remarks>
/// This policy does not calculate contact, friction, force, balance, or target coordinates. The external
/// physics/domain owner remains authoritative, and the caller decides whether and how to execute a recommendation.
/// </remarks>
public static class ContactRecoveryPolicy
{
    /// <summary>
    /// Recommends the next intent from the latest external contact observation.
    /// Missing, unknown, or unevaluated observations safely defer a contact-dependent decision.
    /// </summary>
    public static ContactRecoveryAction Recommend(ContactObservation? observation)
    {
        if (observation is null)
            return ContactRecoveryAction.AwaitObservation;

        return observation.Presence switch
        {
            ContactPresence.Unknown => ContactRecoveryAction.AwaitObservation,
            ContactPresence.NoContact => ContactRecoveryAction.SeekContact,
            ContactPresence.Released => ContactRecoveryAction.ReacquireContact,
            ContactPresence.Contact => observation.LoadOutcome switch
            {
                ContactLoadOutcome.Supported => ContactRecoveryAction.ContinueCurrentTarget,
                ContactLoadOutcome.Slipping => ContactRecoveryAction.SeekAlternativeContact,
                ContactLoadOutcome.Unsupported => ContactRecoveryAction.RepositionToReduceLoad,
                ContactLoadOutcome.Unknown or ContactLoadOutcome.NotEvaluated => ContactRecoveryAction.AwaitObservation,
                _ => throw new ArgumentOutOfRangeException(nameof(observation), "Unknown contact load outcome.")
            },
            _ => throw new ArgumentOutOfRangeException(nameof(observation), "Unknown contact presence.")
        };
    }
}
