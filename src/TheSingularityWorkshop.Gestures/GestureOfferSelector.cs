namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A provider's offer to perform one Gesture under explicit actor-capability and anchor requirements.
/// The offer describes eligibility only; it does not choose an actor action or prove physical success.
/// </summary>
public sealed class GestureOffer
{
    public GestureOffer(
        GestureDefinition definition,
        IEnumerable<string>? requiredActorCapabilities = null,
        IEnumerable<string>? requiredAnchorNames = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
        RequiredActorCapabilities = SnapshotNames(requiredActorCapabilities, nameof(requiredActorCapabilities));
        RequiredAnchorNames = SnapshotNames(requiredAnchorNames, nameof(requiredAnchorNames));

        var definedAnchors = new HashSet<string>(
            definition.Anchors.Select(anchor => anchor.Name), StringComparer.Ordinal);
        var unknownAnchors = RequiredAnchorNames.Where(name => !definedAnchors.Contains(name)).ToArray();
        if (unknownAnchors.Length > 0)
            throw new ArgumentException(
                $"Required anchors are not defined by Gesture '{definition.Id}': {string.Join(", ", unknownAnchors)}.",
                nameof(requiredAnchorNames));
    }

    /// <summary>The reusable procedure offered by the provider.</summary>
    public GestureDefinition Definition { get; }

    /// <summary>Actor capabilities that must all be present for this offer to be eligible.</summary>
    public IReadOnlyList<string> RequiredActorCapabilities { get; }

    /// <summary>Semantic anchors that the consumer must report as available for this offer.</summary>
    public IReadOnlyList<string> RequiredAnchorNames { get; }

    private static IReadOnlyList<string> SnapshotNames(IEnumerable<string>? names, string parameterName)
    {
        var snapshot = (names ?? []).ToArray();
        if (snapshot.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Requirement names must not be empty.", parameterName);
        if (snapshot.Distinct(StringComparer.Ordinal).Count() != snapshot.Length)
            throw new ArgumentException("Requirement names must be unique.", parameterName);
        Array.Sort(snapshot, StringComparer.Ordinal);
        return Array.AsReadOnly(snapshot);
    }
}

/// <summary>Why a provider offer was not eligible for a particular actor and situation.</summary>
public sealed class GestureOfferRejection
{
    internal GestureOfferRejection(GestureOffer offer, IReadOnlyList<string> missingCapabilities,
        IReadOnlyList<string> missingAnchors)
    {
        Offer = offer;
        MissingActorCapabilities = missingCapabilities;
        MissingAnchorNames = missingAnchors;
    }

    /// <summary>The rejected provider offer.</summary>
    public GestureOffer Offer { get; }

    /// <summary>Required actor capabilities absent from the supplied actor context.</summary>
    public IReadOnlyList<string> MissingActorCapabilities { get; }

    /// <summary>Required semantic anchors absent from the supplied environment context.</summary>
    public IReadOnlyList<string> MissingAnchorNames { get; }
}

/// <summary>Deterministic eligibility filtering for provider-offered Gestures.</summary>
public sealed class GestureOfferSelection
{
    internal GestureOfferSelection(IReadOnlyList<GestureOffer> eligible,
        IReadOnlyList<GestureOfferRejection> rejected)
    {
        Eligible = eligible;
        Rejected = rejected;
    }

    /// <summary>Eligible offers, ordered by stable Gesture ID using ordinal comparison.</summary>
    public IReadOnlyList<GestureOffer> Eligible { get; }

    /// <summary>Ineligible offers, ordered by stable Gesture ID with explicit missing requirements.</summary>
    public IReadOnlyList<GestureOfferRejection> Rejected { get; }
}

/// <summary>
/// Filters provider offers from an actor's perspective without owning actor policy, world state,
/// anchor resolution, scheduling, or physical outcomes.
/// </summary>
public static class GestureOfferSelector
{
    /// <summary>
    /// Returns eligible and rejected offers. The same inputs yield the same ordering regardless of input enumeration order.
    /// Duplicate Gesture IDs are rejected because they make provider selection ambiguous.
    /// </summary>
    public static GestureOfferSelection Evaluate(
        IEnumerable<GestureOffer> offers,
        IEnumerable<string> actorCapabilities,
        IEnumerable<string> availableAnchorNames)
    {
        ArgumentNullException.ThrowIfNull(offers);
        ArgumentNullException.ThrowIfNull(actorCapabilities);
        ArgumentNullException.ThrowIfNull(availableAnchorNames);

        var offerSnapshot = offers.ToArray();
        if (offerSnapshot.Any(offer => offer is null))
            throw new ArgumentException("Offers must not contain null.", nameof(offers));
        var duplicateId = offerSnapshot.GroupBy(offer => offer.Definition.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateId is not null)
            throw new ArgumentException($"Duplicate offered Gesture ID '{duplicateId.Key}'.", nameof(offers));

        var actorSet = ReadContextNames(actorCapabilities, nameof(actorCapabilities));
        var anchorSet = ReadContextNames(availableAnchorNames, nameof(availableAnchorNames));
        var eligible = new List<GestureOffer>();
        var rejected = new List<GestureOfferRejection>();

        foreach (var offer in offerSnapshot.OrderBy(item => item.Definition.Id, StringComparer.Ordinal))
        {
            var missingCapabilities = offer.RequiredActorCapabilities
                .Where(name => !actorSet.Contains(name)).ToArray();
            var missingAnchors = offer.RequiredAnchorNames
                .Where(name => !anchorSet.Contains(name)).ToArray();

            if (missingCapabilities.Length == 0 && missingAnchors.Length == 0)
                eligible.Add(offer);
            else
                rejected.Add(new GestureOfferRejection(offer,
                    Array.AsReadOnly(missingCapabilities), Array.AsReadOnly(missingAnchors)));
        }

        return new GestureOfferSelection(
            Array.AsReadOnly(eligible.ToArray()), Array.AsReadOnly(rejected.ToArray()));
    }

    private static HashSet<string> ReadContextNames(IEnumerable<string> names, string parameterName)
    {
        var snapshot = names.ToArray();
        if (snapshot.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Context names must not be empty.", parameterName);
        return new HashSet<string>(snapshot, StringComparer.Ordinal);
    }
}
