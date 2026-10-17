namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A host-reported event involving one or more Gesture participants, suitable for independent
/// observers to interpret. This record describes an occurrence; it does not prescribe a reaction
/// or assert that the participants achieved a physical goal.
/// </summary>
/// <remarks>
/// Event kind values are consumer-defined semantic keys, not a built-in emotion or ontology
/// vocabulary. Spatial bounds, perception, interpretation, and physical outcomes belong to their
/// respective host, observer, and physics contracts.
/// </remarks>
public sealed record GestureObservationEvent
{
    /// <summary>Creates a validated, immutable record of an observable Gesture-related occurrence.</summary>
    /// <param name="eventId">Stable identity for this occurrence.</param>
    /// <param name="eventKind">Consumer-defined semantic key describing what was observed.</param>
    /// <param name="occurredAtSeconds">Occurrence time on the host-supplied timeline, in seconds.</param>
    /// <param name="participantIds">Stable IDs of the participants represented by this occurrence.</param>
    /// <param name="causedByEventId">Optional causal parent event, when this occurrence is a reaction or consequence.</param>
    public GestureObservationEvent(
        string eventId,
        string eventKind,
        double occurredAtSeconds,
        IEnumerable<string> participantIds,
        string? causedByEventId = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("Event id must not be empty.", nameof(eventId));
        if (string.IsNullOrWhiteSpace(eventKind))
            throw new ArgumentException("Event kind must not be empty.", nameof(eventKind));
        if (!double.IsFinite(occurredAtSeconds))
            throw new ArgumentOutOfRangeException(nameof(occurredAtSeconds));
        ArgumentNullException.ThrowIfNull(participantIds);
        if (causedByEventId is not null && string.IsNullOrWhiteSpace(causedByEventId))
            throw new ArgumentException("Causal parent event id must be null or non-empty.", nameof(causedByEventId));
        if (string.Equals(eventId, causedByEventId, StringComparison.Ordinal))
            throw new ArgumentException("An event cannot be its own causal parent.", nameof(causedByEventId));

        var participants = participantIds.ToArray();
        if (participants.Length == 0)
            throw new ArgumentException("An observable event requires at least one participant.", nameof(participantIds));

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var participantId in participants)
        {
            if (string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException("Participant ids must not be empty.", nameof(participantIds));
            if (!seen.Add(participantId))
                throw new ArgumentException($"Duplicate participant id '{participantId}'.", nameof(participantIds));
        }

        EventId = eventId;
        EventKind = eventKind;
        OccurredAtSeconds = occurredAtSeconds;
        ParticipantIds = Array.AsReadOnly(participants);
        CausedByEventId = causedByEventId;
    }

    /// <summary>Stable identity for this occurrence.</summary>
    public string EventId { get; }

    /// <summary>Consumer-defined semantic key for the observed occurrence.</summary>
    public string EventKind { get; }

    /// <summary>Occurrence time on the host-supplied timeline, in seconds.</summary>
    public double OccurredAtSeconds { get; }

    /// <summary>Stable identities of the participants represented by this occurrence.</summary>
    public IReadOnlyList<string> ParticipantIds { get; }

    /// <summary>Optional causal parent event ID, without embedding or owning the parent event.</summary>
    public string? CausedByEventId { get; }
}
