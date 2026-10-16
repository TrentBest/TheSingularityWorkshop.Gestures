namespace TheSingularityWorkshop.Gestures;

/// <summary>Whether the external physics/domain owner reports a contact at the interaction point.</summary>
public enum ContactPresence
{
    /// <summary>The contact state is not currently known.</summary>
    Unknown,
    /// <summary>No contact is present.</summary>
    NoContact,
    /// <summary>A contact is currently present.</summary>
    Contact,
    /// <summary>A previously present contact has been lost or released.</summary>
    Released
}

/// <summary>External assessment of whether a contact can support the current modeled load.</summary>
public enum ContactLoadOutcome
{
    /// <summary>The load outcome is not known.</summary>
    Unknown,
    /// <summary>Contact exists, but support has not been assessed.</summary>
    NotEvaluated,
    /// <summary>The physics/domain model reports that the current load is supported.</summary>
    Supported,
    /// <summary>The contact is moving under load according to the physics/domain model.</summary>
    Slipping,
    /// <summary>The contact exists but cannot support the current modeled load.</summary>
    Unsupported
}

/// <summary>
/// A data-only observation reported by an external physics or domain model.
/// </summary>
/// <remarks>
/// Gestures does not infer contact, friction, grip, adhesion, or load support. Optional force values are
/// measurements or assessments supplied by the owner of those mechanics. Available tangential force may
/// include an explicitly modeled adhesion mechanism; this type does not calculate it.
/// </remarks>
public sealed record ContactObservation
{
    /// <summary>Creates a validated observation of contact and load outcome.</summary>
    public ContactObservation(
        ContactPresence presence,
        ContactLoadOutcome loadOutcome,
        string? contactId = null,
        float? normalForceNewtons = null,
        float? tangentialLoadNewtons = null,
        float? availableTangentialForceNewtons = null)
    {
        if (!Enum.IsDefined(presence))
            throw new ArgumentOutOfRangeException(nameof(presence));
        if (!Enum.IsDefined(loadOutcome))
            throw new ArgumentOutOfRangeException(nameof(loadOutcome));
        if (contactId is not null && string.IsNullOrWhiteSpace(contactId))
            throw new ArgumentException("Contact identifier must be null or non-empty.", nameof(contactId));

        ValidateOptionalForce(normalForceNewtons, nameof(normalForceNewtons));
        ValidateOptionalForce(tangentialLoadNewtons, nameof(tangentialLoadNewtons));
        ValidateOptionalForce(availableTangentialForceNewtons, nameof(availableTangentialForceNewtons));

        if (presence != ContactPresence.Contact
            && loadOutcome is ContactLoadOutcome.Supported
                or ContactLoadOutcome.Slipping
                or ContactLoadOutcome.Unsupported)
        {
            throw new ArgumentException(
                "A load-bearing outcome requires an active contact observation.",
                nameof(loadOutcome));
        }

        Presence = presence;
        LoadOutcome = loadOutcome;
        ContactId = contactId;
        NormalForceNewtons = normalForceNewtons;
        TangentialLoadNewtons = tangentialLoadNewtons;
        AvailableTangentialForceNewtons = availableTangentialForceNewtons;
    }

    /// <summary>Reported contact presence.</summary>
    public ContactPresence Presence { get; }

    /// <summary>Reported load outcome, evaluated by the physics/domain owner.</summary>
    public ContactLoadOutcome LoadOutcome { get; }

    /// <summary>Optional identifier for the contacted surface or hold.</summary>
    public string? ContactId { get; }

    /// <summary>Optional reported normal force magnitude, in newtons.</summary>
    public float? NormalForceNewtons { get; }

    /// <summary>Optional reported tangential load magnitude, in newtons.</summary>
    public float? TangentialLoadNewtons { get; }

    /// <summary>Optional available tangential support force, in newtons, as reported by the external model.</summary>
    public float? AvailableTangentialForceNewtons { get; }

    private static void ValidateOptionalForce(float? value, string parameterName)
    {
        if (value is not float force)
            return;
        if (!float.IsFinite(force) || force < 0f)
            throw new ArgumentOutOfRangeException(parameterName, "Force magnitude must be finite and non-negative.");
    }
}
