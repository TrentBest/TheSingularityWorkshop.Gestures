namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A provider's versioned declaration that it can supply Gestures for a target kind.
/// This is discovery metadata, not a rig schema or proof that a concrete target is compatible.
/// </summary>
public sealed class GestureTargetProfile
{
    public GestureTargetProfile(
        string providerId,
        string offerId,
        string version,
        string targetKind,
        IEnumerable<string>? capabilities = null,
        IEnumerable<string>? gestureIds = null)
    {
        ProviderId = Required(providerId, nameof(providerId));
        OfferId = Required(offerId, nameof(offerId));
        Version = Required(version, nameof(version));
        TargetKind = Required(targetKind, nameof(targetKind));
        Capabilities = SnapshotNames(capabilities, nameof(capabilities));
        GestureIds = SnapshotNames(gestureIds, nameof(gestureIds));
    }

    /// <summary>Stable identifier of the provider.</summary>
    public string ProviderId { get; }
    /// <summary>Stable identifier of this provider's offer.</summary>
    public string OfferId { get; }
    /// <summary>Provider-controlled offer version used to invalidate stale references.</summary>
    public string Version { get; }
    /// <summary>Provider-defined target kind, such as dog, bird, butterfly, or industrial-arm.</summary>
    public string TargetKind { get; }
    /// <summary>Capabilities declared by this profile.</summary>
    public IReadOnlyList<string> Capabilities { get; }
    /// <summary>Stable Gesture IDs this profile advertises; these are not embedded definitions.</summary>
    public IReadOnlyList<string> GestureIds { get; }

    private static string Required(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value must not be empty.", parameterName)
            : value;

    private static IReadOnlyList<string> SnapshotNames(IEnumerable<string>? names, string parameterName)
    {
        var snapshot = (names ?? []).ToArray();
        if (snapshot.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Names must not be empty.", parameterName);
        if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
            throw new ArgumentException("Names must be unique.", parameterName);
        Array.Sort(snapshot, StringComparer.Ordinal);
        return Array.AsReadOnly(snapshot);
    }
}

/// <summary>A target capability query. Matching uses exact ordinal target-kind and capability names.</summary>
public sealed class GestureTargetQuery
{
    public GestureTargetQuery(string targetKind, IEnumerable<string>? requiredCapabilities = null)
    {
        if (string.IsNullOrWhiteSpace(targetKind))
            throw new ArgumentException("Target kind must not be empty.", nameof(targetKind));
        TargetKind = targetKind;
        var snapshot = (requiredCapabilities ?? []).ToArray();
        if (snapshot.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Capability names must not be empty.", nameof(requiredCapabilities));
        if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
            throw new ArgumentException("Capability names must be unique.", nameof(requiredCapabilities));
        Array.Sort(snapshot, StringComparer.Ordinal);
        RequiredCapabilities = Array.AsReadOnly(snapshot);
    }

    public string TargetKind { get; }
    public IReadOnlyList<string> RequiredCapabilities { get; }
}

/// <summary>A matching profile and its index in this specific query result snapshot.</summary>
public sealed class GestureTargetMatch
{
    internal GestureTargetMatch(int index, GestureTargetProfile profile)
    {
        Index = index;
        Profile = profile;
    }

    /// <summary>Query-local result index; never persist it as an identity.</summary>
    public int Index { get; }
    public GestureTargetProfile Profile { get; }
}

/// <summary>Deterministic discovery of provider-declared target profiles.</summary>
public static class GestureTargetDiscovery
{
    /// <summary>
    /// Finds profiles matching the target kind and all requested capabilities.
    /// Results are stably ordered by provider ID, offer ID, and version.
    /// Duplicate provider/offer/version identities are rejected as ambiguous.
    /// </summary>
    public static IReadOnlyList<GestureTargetMatch> Find(
        IEnumerable<GestureTargetProfile> profiles,
        GestureTargetQuery query)
    {
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(query);
        var snapshot = profiles.ToArray();
        if (snapshot.Any(profile => profile is null))
            throw new ArgumentException("Profiles must not contain null.", nameof(profiles));

        var duplicate = snapshot.GroupBy(
                profile => (profile.ProviderId, profile.OfferId, profile.Version))
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
            throw new ArgumentException(
                $"Duplicate profile identity '{duplicate.Key.ProviderId}/{duplicate.Key.OfferId}@{duplicate.Key.Version}'.",
                nameof(profiles));

        var required = new HashSet<string>(query.RequiredCapabilities, StringComparer.Ordinal);
        var matches = snapshot
            .Where(profile => string.Equals(profile.TargetKind, query.TargetKind, StringComparison.Ordinal))
            .Where(profile => required.IsSubsetOf(new HashSet<string>(profile.Capabilities, StringComparer.Ordinal)))
            .OrderBy(profile => profile.ProviderId, StringComparer.Ordinal)
            .ThenBy(profile => profile.OfferId, StringComparer.Ordinal)
            .ThenBy(profile => profile.Version, StringComparer.Ordinal)
            .ToArray();

        return Array.AsReadOnly(matches.Select((profile, index) => new GestureTargetMatch(index, profile)).ToArray());
    }
}
