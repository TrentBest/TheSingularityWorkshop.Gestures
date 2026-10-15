namespace TheSingularityWorkshop.Gestures;

using System.Numerics;

/// <summary>Named boundary through which an experience admits or releases crowd traffic.</summary>
public sealed record CrowdFlowGate
{
    public CrowdFlowGate(string id, Vector2 position)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Gate id must not be empty.", nameof(id));
        if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
            throw new ArgumentOutOfRangeException(nameof(position), "Gate position must be finite.");
        Id = id;
        Position = position;
    }

    public string Id { get; }
    /// <summary>Experience-local planar position; coordinate meaning and units belong to the host.</summary>
    public Vector2 Position { get; }
}

/// <summary>A host-authored path between an entrance and exit gate.</summary>
public sealed class CrowdFlowRoute
{
    public CrowdFlowRoute(string id, string entranceGateId, string exitGateId,
        IEnumerable<Vector2>? waypoints = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Route id must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(entranceGateId))
            throw new ArgumentException("Entrance gate id must not be empty.", nameof(entranceGateId));
        if (string.IsNullOrWhiteSpace(exitGateId))
            throw new ArgumentException("Exit gate id must not be empty.", nameof(exitGateId));
        if (StringComparer.Ordinal.Equals(entranceGateId, exitGateId))
            throw new ArgumentException("A route must connect distinct entrance and exit gates.");
        Id = id;
        EntranceGateId = entranceGateId;
        ExitGateId = exitGateId;
        var points = waypoints?.ToArray() ?? [];
        if (points.Any(p => !float.IsFinite(p.X) || !float.IsFinite(p.Y)))
            throw new ArgumentOutOfRangeException(nameof(waypoints), "Waypoint coordinates must be finite.");
        Waypoints = Array.AsReadOnly(points);
    }

    public string Id { get; }
    public string EntranceGateId { get; }
    public string ExitGateId { get; }
    /// <summary>Optional host-authored intermediate points, in traversal order.</summary>
    public IReadOnlyList<Vector2> Waypoints { get; }
}

/// <summary>Four common crowd intents, independent of the host's world or rendering backend.</summary>
public enum CrowdTraversalMode
{
    IndividualUnhurried,
    IndividualHurried,
    GroupUnhurried,
    GroupHurried
}

/// <summary>High-level behavior tuning for one crowd intent.</summary>
public sealed record CrowdTraversalProfile
{
    public CrowdTraversalProfile(CrowdTraversalMode mode, float speedMultiplier,
        float personalSpaceRadius, float groupCohesion)
    {
        if (!Enum.IsDefined(mode))
            throw new ArgumentOutOfRangeException(nameof(mode));
        if (!float.IsFinite(speedMultiplier) || speedMultiplier <= 0f)
            throw new ArgumentOutOfRangeException(nameof(speedMultiplier), "Speed multiplier must be finite and positive.");
        if (!float.IsFinite(personalSpaceRadius) || personalSpaceRadius < 0f)
            throw new ArgumentOutOfRangeException(nameof(personalSpaceRadius), "Personal-space radius must be finite and non-negative.");
        if (!float.IsFinite(groupCohesion) || groupCohesion < 0f || groupCohesion > 1f)
            throw new ArgumentOutOfRangeException(nameof(groupCohesion), "Group cohesion must be in [0, 1].");
        if (mode is CrowdTraversalMode.IndividualUnhurried or CrowdTraversalMode.IndividualHurried && groupCohesion != 0f)
            throw new ArgumentException("Individual profiles must use zero group cohesion.", nameof(groupCohesion));
        Mode = mode;
        SpeedMultiplier = speedMultiplier;
        PersonalSpaceRadius = personalSpaceRadius;
        GroupCohesion = groupCohesion;
    }

    public CrowdTraversalMode Mode { get; }
    /// <summary>Multiplier applied to the host/entity's baseline speed, not a world-units-per-second value.</summary>
    public float SpeedMultiplier { get; }
    /// <summary>Desired separation tuning for a future collision/avoidance solver; not a collision guarantee.</summary>
    public float PersonalSpaceRadius { get; }
    /// <summary>Group-following strength in [0,1]; must be zero for individual modes.</summary>
    public float GroupCohesion { get; }
}

/// <summary>Lifecycle labels a host can map to FSM_API state groups.</summary>
public enum CrowdTraversalState
{
    WaitingForEntry,
    JoiningFlow,
    Traversing,
    Yielding,
    RejoiningFlow,
    ApproachingExit,
    Completed,
    Replanning
}

/// <summary>
/// Immutable experience-authored crowd configuration. It names the gates and routes; it does not
/// own world authority, collision resolution, spawn schedules, or the simulation clock.
/// </summary>
public sealed class CrowdFlowScenario
{
    private readonly IReadOnlyDictionary<string, CrowdFlowGate> _gates;
    private readonly IReadOnlyDictionary<string, CrowdFlowRoute> _routes;

    public CrowdFlowScenario(string id, IEnumerable<CrowdFlowGate> gates, IEnumerable<CrowdFlowRoute> routes)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Scenario id must not be empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(gates);
        ArgumentNullException.ThrowIfNull(routes);
        Id = id;
        var gateMap = new Dictionary<string, CrowdFlowGate>(StringComparer.Ordinal);
        foreach (var gate in gates)
        {
            ArgumentNullException.ThrowIfNull(gate);
            if (!gateMap.TryAdd(gate.Id, gate))
                throw new ArgumentException($"Duplicate gate id '{gate.Id}'.", nameof(gates));
        }
        var routeMap = new Dictionary<string, CrowdFlowRoute>(StringComparer.Ordinal);
        foreach (var route in routes)
        {
            ArgumentNullException.ThrowIfNull(route);
            if (!gateMap.ContainsKey(route.EntranceGateId) || !gateMap.ContainsKey(route.ExitGateId))
                throw new ArgumentException($"Route '{route.Id}' references an unknown gate.", nameof(routes));
            if (!routeMap.TryAdd(route.Id, route))
                throw new ArgumentException($"Duplicate route id '{route.Id}'.", nameof(routes));
        }
        _gates = new System.Collections.ObjectModel.ReadOnlyDictionary<string, CrowdFlowGate>(gateMap);
        _routes = new System.Collections.ObjectModel.ReadOnlyDictionary<string, CrowdFlowRoute>(routeMap);
    }

    public string Id { get; }
    public IReadOnlyDictionary<string, CrowdFlowGate> Gates => _gates;
    public IReadOnlyDictionary<string, CrowdFlowRoute> Routes => _routes;

    public CrowdFlowRoute GetRoute(string routeId) =>
        _routes.TryGetValue(routeId, out var route)
            ? route
            : throw new KeyNotFoundException($"Unknown crowd route '{routeId}'.");
}
