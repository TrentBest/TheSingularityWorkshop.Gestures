using System.Globalization;
using System.Text;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Versioned bounds for deterministic crowd-response intent. This schedules intent only; it
/// does not evaluate a gesture, simulate bodies, synthesize audio, or claim physical completion.
/// </summary>
public sealed record CrowdResponsePolicy
{
    public CrowdResponsePolicy(string id, int version, double minimumDelaySeconds,
        double maximumDelaySeconds, int minimumVariant, int maximumVariant,
        double minimumIntensity, double maximumIntensity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Policy id must not be empty.", nameof(id));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version), "Policy version must be positive.");
        if (!double.IsFinite(minimumDelaySeconds) || minimumDelaySeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(minimumDelaySeconds));
        if (!double.IsFinite(maximumDelaySeconds) || maximumDelaySeconds < minimumDelaySeconds)
            throw new ArgumentOutOfRangeException(nameof(maximumDelaySeconds));
        if (maximumVariant < minimumVariant)
            throw new ArgumentOutOfRangeException(nameof(maximumVariant));
        if (!double.IsFinite(minimumIntensity) || minimumIntensity < 0d)
            throw new ArgumentOutOfRangeException(nameof(minimumIntensity));
        if (!double.IsFinite(maximumIntensity) || maximumIntensity < minimumIntensity)
            throw new ArgumentOutOfRangeException(nameof(maximumIntensity));

        Id = id;
        Version = version;
        MinimumDelaySeconds = minimumDelaySeconds;
        MaximumDelaySeconds = maximumDelaySeconds;
        MinimumVariant = minimumVariant;
        MaximumVariant = maximumVariant;
        MinimumIntensity = minimumIntensity;
        MaximumIntensity = maximumIntensity;
    }

    public string Id { get; }
    public int Version { get; }
    public double MinimumDelaySeconds { get; }
    public double MaximumDelaySeconds { get; }
    public int MinimumVariant { get; }
    public int MaximumVariant { get; }
    public double MinimumIntensity { get; }
    public double MaximumIntensity { get; }
}

/// <summary>One stable participant's inputs for a crowd-response event.</summary>
public sealed record CrowdResponseRequest
{
    public CrowdResponseRequest(string eventId, double eventTimeSeconds, string participantId,
        ulong stableSeed)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            throw new ArgumentException("Event id must not be empty.", nameof(eventId));
        if (!double.IsFinite(eventTimeSeconds))
            throw new ArgumentOutOfRangeException(nameof(eventTimeSeconds));
        if (string.IsNullOrWhiteSpace(participantId))
            throw new ArgumentException("Participant id must not be empty.", nameof(participantId));
        EventId = eventId;
        EventTimeSeconds = eventTimeSeconds;
        ParticipantId = participantId;
        StableSeed = stableSeed;
    }

    public string EventId { get; }
    public double EventTimeSeconds { get; }
    public string ParticipantId { get; }
    public ulong StableSeed { get; }
}

/// <summary>
/// Immutable deterministic intent schedule. Acoustic arrival is deliberately absent: it belongs
/// to a separate physical/acoustic result and must not be inferred from this intent timestamp.
/// </summary>
public sealed record CrowdResponseSchedule(
    string EventId,
    string PolicyId,
    int PolicyVersion,
    string ParticipantId,
    double IntentTimeSeconds,
    double DelaySeconds,
    int Variant,
    double Intensity);

/// <summary>
/// Pure CPU reference scheduler. Stable FNV-1a hashing over explicit UTF-8 inputs avoids runtime
/// string-hash randomization and enumeration-order dependence. Hash details are part of v1.
/// </summary>
public static class CrowdResponseScheduler
{
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    /// <summary>Schedules one participant without reading wall-clock time or mutable global state.</summary>
    public static CrowdResponseSchedule Schedule(CrowdResponseRequest request, CrowdResponsePolicy policy)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(policy);

        var hash = OffsetBasis;
        Append(ref hash, policy.Id);
        Append(ref hash, policy.Version.ToString(CultureInfo.InvariantCulture));
        Append(ref hash, request.EventId);
        Append(ref hash, request.ParticipantId);
        Append(ref hash, request.StableSeed.ToString(CultureInfo.InvariantCulture));

        var delayUnit = UnitInterval(Mix(hash, 0xD1B54A32D192ED03UL));
        var variantUnit = UnitInterval(Mix(hash, 0x94D049BB133111EBUL));
        var intensityUnit = UnitInterval(Mix(hash, 0xBF58476D1CE4E5B9UL));

        var delay = Lerp(policy.MinimumDelaySeconds, policy.MaximumDelaySeconds, delayUnit);
        var intentTime = request.EventTimeSeconds + delay;
        if (!double.IsFinite(intentTime))
            throw new ArgumentOutOfRangeException(nameof(request), "Scheduled intent time must remain finite.");

        var variantRange = (long)policy.MaximumVariant - policy.MinimumVariant + 1L;
        var variant = (int)(policy.MinimumVariant + (long)(variantUnit * variantRange));
        var intensity = Lerp(policy.MinimumIntensity, policy.MaximumIntensity, intensityUnit);

        return new CrowdResponseSchedule(request.EventId, policy.Id, policy.Version,
            request.ParticipantId, intentTime, delay, variant, intensity);
    }

    private static double Lerp(double minimum, double maximum, double unit) =>
        minimum + ((maximum - minimum) * unit);

    private static double UnitInterval(ulong value) =>
        (value >> 11) * (1d / 9007199254740992d);

    private static ulong Mix(ulong value, ulong salt)
    {
        value ^= salt;
        value ^= value >> 30;
        value *= 0xBF58476D1CE4E5B9UL;
        value ^= value >> 27;
        value *= 0x94D049BB133111EBUL;
        value ^= value >> 31;
        return value;
    }

    private static void Append(ref ulong hash, string value)
    {
        foreach (var octet in Encoding.UTF8.GetBytes(value))
        {
            hash ^= octet;
            hash *= Prime;
        }

        // A delimiter prevents simple concatenation ambiguity between adjacent fields.
        hash ^= 0xFF;
        hash *= Prime;
    }
}
