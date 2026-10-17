using System.Globalization;
using System.Numerics;
using System.Text;

namespace TheSingularityWorkshop.Gestures;

/// <summary>A stable, axis-aligned region used as the initial CPU bake domain.</summary>
public sealed record ExtremusRegion
{
    public ExtremusRegion(string id, Vector3 minimum, Vector3 maximum)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Region id must not be empty.", nameof(id));
        ValidateFinite(minimum, nameof(minimum));
        ValidateFinite(maximum, nameof(maximum));
        if (minimum.X >= maximum.X || minimum.Y >= maximum.Y || minimum.Z >= maximum.Z)
            throw new ArgumentException("Every region minimum component must be less than its maximum.");
        Id = id;
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Id { get; }
    public Vector3 Minimum { get; }
    public Vector3 Maximum { get; }

    private static void ValidateFinite(Vector3 value, string name)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
            throw new ArgumentOutOfRangeException(name, "Region coordinates must be finite.");
    }
}

/// <summary>An axis-aligned no-placement envelope supplied by the host/domain.</summary>
public sealed record ExtremusExclusion
{
    public ExtremusExclusion(string id, Vector3 minimum, Vector3 maximum)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Exclusion id must not be empty.", nameof(id));
        ValidateFinite(minimum, nameof(minimum));
        ValidateFinite(maximum, nameof(maximum));
        if (minimum.X > maximum.X || minimum.Y > maximum.Y || minimum.Z > maximum.Z)
            throw new ArgumentException("Exclusion minimum components must not exceed maximum components.");
        Id = id;
        Minimum = minimum;
        Maximum = maximum;
    }

    public string Id { get; }
    public Vector3 Minimum { get; }
    public Vector3 Maximum { get; }

    internal bool Contains(Vector3 point) =>
        point.X >= Minimum.X && point.X <= Maximum.X
        && point.Y >= Minimum.Y && point.Y <= Maximum.Y
        && point.Z >= Minimum.Z && point.Z <= Maximum.Z;

    private static void ValidateFinite(Vector3 value, string name)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y) || !float.IsFinite(value.Z))
            throw new ArgumentOutOfRangeException(name, "Exclusion coordinates must be finite.");
    }
}

/// <summary>A semantic activity and its non-negative selection weight.</summary>
public sealed record ExtremusActivity
{
    public ExtremusActivity(string id, double weight)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Activity id must not be empty.", nameof(id));
        if (!double.IsFinite(weight) || weight <= 0d)
            throw new ArgumentOutOfRangeException(nameof(weight), "Activity weight must be finite and positive.");
        Id = id;
        Weight = weight;
    }

    public string Id { get; }
    public double Weight { get; }
}

/// <summary>Versioned input settings for a deterministic population bake.</summary>
public sealed record ExtremusBakeRequest
{
    public ExtremusBakeRequest(string profileId, int profileVersion, string catalogueVersion,
        ExtremusRegion region, ulong seed, int populationCount,
        IEnumerable<ExtremusActivity> activities, IEnumerable<ExtremusExclusion>? exclusions = null,
        int maximumPlacementAttemptsPerInstance = 128)
    {
        if (string.IsNullOrWhiteSpace(profileId))
            throw new ArgumentException("Profile id must not be empty.", nameof(profileId));
        if (profileVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(profileVersion));
        if (string.IsNullOrWhiteSpace(catalogueVersion))
            throw new ArgumentException("Catalogue version must not be empty.", nameof(catalogueVersion));
        ArgumentNullException.ThrowIfNull(region);
        if (populationCount < 0)
            throw new ArgumentOutOfRangeException(nameof(populationCount));
        if (maximumPlacementAttemptsPerInstance <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumPlacementAttemptsPerInstance));
        ArgumentNullException.ThrowIfNull(activities);

        var activityArray = activities.ToArray();
        if (activityArray.Length == 0 && populationCount > 0)
            throw new ArgumentException("At least one activity is required for a non-empty population.", nameof(activities));
        if (activityArray.Any(x => x is null))
            throw new ArgumentException("Activities must not contain null entries.", nameof(activities));
        if (activityArray.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != activityArray.Length)
            throw new ArgumentException("Activity ids must be unique.", nameof(activities));

        var exclusionArray = (exclusions ?? []).ToArray();
        if (exclusionArray.Any(x => x is null))
            throw new ArgumentException("Exclusions must not contain null entries.", nameof(exclusions));
        if (exclusionArray.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() != exclusionArray.Length)
            throw new ArgumentException("Exclusion ids must be unique.", nameof(exclusions));

        ProfileId = profileId;
        ProfileVersion = profileVersion;
        CatalogueVersion = catalogueVersion;
        Region = region;
        Seed = seed;
        PopulationCount = populationCount;
        Activities = Array.AsReadOnly(activityArray.OrderBy(x => x.Id, StringComparer.Ordinal).ToArray());
        Exclusions = Array.AsReadOnly(exclusionArray.OrderBy(x => x.Id, StringComparer.Ordinal).ToArray());
        MaximumPlacementAttemptsPerInstance = maximumPlacementAttemptsPerInstance;
    }

    public string ProfileId { get; }
    public int ProfileVersion { get; }
    public string CatalogueVersion { get; }
    public ExtremusRegion Region { get; }
    public ulong Seed { get; }
    public int PopulationCount { get; }
    public IReadOnlyList<ExtremusActivity> Activities { get; }
    public IReadOnlyList<ExtremusExclusion> Exclusions { get; }
    public int MaximumPlacementAttemptsPerInstance { get; }
}

/// <summary>One deterministic crowd member. This is representation input, not an authoritative actor.</summary>
public sealed record ExtremusPopulationInstance(
    string StableId,
    Vector3 Position,
    float HeadingRadians,
    float Scale,
    string ActivityId,
    double Phase);

/// <summary>Provenance for a reproducible CPU reference population bake.</summary>
public sealed record ExtremusBakeManifest(
    int AlgorithmVersion,
    string ProfileId,
    int ProfileVersion,
    string CatalogueVersion,
    string RegionId,
    ulong Seed,
    int RequestedCount,
    int BakedCount,
    string RepresentationKind);

/// <summary>Immutable output from the engine-neutral CPU reference baker.</summary>
public sealed record ExtremusBakeResult(
    ExtremusBakeManifest Manifest,
    IReadOnlyList<ExtremusPopulationInstance> Instances);

/// <summary>
/// Deterministic CPU reference population baker. It generates bounded placements and semantic
/// activity assignments; it does not solve collisions, routes, contact, animation, or rendering.
/// Hash and mapping semantics are versioned by AlgorithmVersion.
/// </summary>
public static class ExtremusCrowdBaker
{
    public const int AlgorithmVersion = 1;
    private const ulong OffsetBasis = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    /// <summary>Bakes a reproducible set of population instances from a versioned request.</summary>
    public static ExtremusBakeResult Bake(ExtremusBakeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var instances = new ExtremusPopulationInstance[request.PopulationCount];
        var totalWeight = request.Activities.Sum(x => x.Weight);
        if (!double.IsFinite(totalWeight) || totalWeight <= 0d)
            throw new ArgumentOutOfRangeException(nameof(request), "Activity weights must have a finite positive sum.");

        for (var index = 0; index < instances.Length; index++)
        {
            var stableId = "ext-" + HashText(request.ProfileId, request.ProfileVersion,
                request.CatalogueVersion, request.Region.Id, request.Seed, index).ToString("x16", CultureInfo.InvariantCulture);
            var placed = false;
            Vector3 position = default;
            ulong placementHash = 0UL;

            for (var attempt = 0; attempt < request.MaximumPlacementAttemptsPerInstance; attempt++)
            {
                placementHash = HashText(stableId, "placement", attempt);
                position = new Vector3(
                    Sample(request.Region.Minimum.X, request.Region.Maximum.X, placementHash, 0xD1B54A32D192ED03UL),
                    Sample(request.Region.Minimum.Y, request.Region.Maximum.Y, placementHash, 0x94D049BB133111EBUL),
                    Sample(request.Region.Minimum.Z, request.Region.Maximum.Z, placementHash, 0xBF58476D1CE4E5B9UL));
                if (request.Exclusions.All(x => !x.Contains(position)))
                {
                    placed = true;
                    break;
                }
            }

            if (!placed)
                throw new InvalidOperationException(
                    $"Could not place population instance {index} in region '{request.Region.Id}' after {request.MaximumPlacementAttemptsPerInstance} deterministic attempts.");

            var activityHash = HashText(stableId, "activity");
            var activityUnit = UnitInterval(Mix(activityHash, 0xA0761D6478BD642FUL));
            var target = activityUnit * totalWeight;
            var activityId = request.Activities[^1].Id;
            foreach (var activity in request.Activities)
            {
                target -= activity.Weight;
                if (target < 0d)
                {
                    activityId = activity.Id;
                    break;
                }
            }

            var headingUnit = UnitInterval(Mix(placementHash, 0xE7037ED1A0B428DBUL));
            var phase = UnitInterval(Mix(activityHash, 0x8EBC6AF09C88C6E3UL));
            var scaleUnit = UnitInterval(Mix(placementHash, 0x589965CC75374CC3UL));
            instances[index] = new ExtremusPopulationInstance(stableId, position,
                (float)(headingUnit * (2d * Math.PI)), (float)(0.85d + scaleUnit * 0.3d), activityId, phase);
        }

        return new ExtremusBakeResult(
            new ExtremusBakeManifest(AlgorithmVersion, request.ProfileId, request.ProfileVersion,
                request.CatalogueVersion, request.Region.Id, request.Seed, request.PopulationCount,
                instances.Length, "cpu-reference-instances-v1"),
            Array.AsReadOnly(instances));
    }

    private static float Sample(float minimum, float maximum, ulong hash, ulong salt) =>
        (float)(minimum + ((maximum - minimum) * UnitInterval(Mix(hash, salt))));

    private static double UnitInterval(ulong value) =>
        (value >> 11) * (1d / 9007199254740992d);

    private static ulong HashText(params object[] values)
    {
        var hash = OffsetBasis;
        foreach (var value in values)
        {
            var text = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
            foreach (var octet in Encoding.UTF8.GetBytes(text))
            {
                hash ^= octet;
                hash *= Prime;
            }
            hash ^= 0xFF;
            hash *= Prime;
        }
        return Mix(hash, 0x9E3779B97F4A7C15UL);
    }

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
}
