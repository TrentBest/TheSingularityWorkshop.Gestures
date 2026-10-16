namespace TheSingularityWorkshop.Gestures;

/// <summary>Operational role assigned to a starport departure task.</summary>
public enum StarportCrewRole
{
    /// <summary>Personnel who board and operate the departing spacecraft.</summary>
    FlightCrew,
    /// <summary>Personnel who prepare, clear, and secure the departure area.</summary>
    GroundCrew,
    /// <summary>Departure-control tasks that require external authorization.</summary>
    FlightControl,
    /// <summary>A shared task that routes or coordinates all assigned personnel.</summary>
    AllCrew
}

/// <summary>Observed lifecycle state of a choreography task, supplied by its consumer.</summary>
public enum StarportTaskStatus
{
    /// <summary>The task has not started.</summary>
    NotStarted,
    /// <summary>The task is underway but has not produced completion evidence.</summary>
    InProgress,
    /// <summary>The task has completed according to the authoritative host.</summary>
    Completed,
    /// <summary>The task failed according to the authoritative host.</summary>
    Failed
}

/// <summary>Externally observed prerequisites for clearance and departure.</summary>
[Flags]
public enum StarportDepartureEvidence
{
    /// <summary>No external prerequisite has been confirmed.</summary>
    None = 0,
    /// <summary>Every assigned flight-crew member is accounted for aboard the craft.</summary>
    FlightCrewAccountedFor = 1 << 0,
    /// <summary>Ground equipment, hoses, covers, and landing-area obstructions are clear.</summary>
    GroundEquipmentClear = 1 << 1,
    /// <summary>Ground crew have returned to the designated safe assembly area.</summary>
    GroundCrewReturned = 1 << 2,
    /// <summary>Boarding access and relevant service interfaces are secured.</summary>
    AccessSealed = 1 << 3,
    /// <summary>The departure manifest and readiness checks are verified.</summary>
    ManifestVerified = 1 << 4,
    /// <summary>Air-traffic / departure-control clearance has been granted.</summary>
    AtcClearanceGranted = 1 << 5
}

/// <summary>A single authored step in a provider-owned starport departure procedure.</summary>
public sealed class StarportTaskDefinition
{
    /// <summary>Creates a task with stable identity, role, action label, and prerequisite task IDs.</summary>
    public StarportTaskDefinition(string id, StarportCrewRole role, string action,
        IEnumerable<string>? dependencies = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Task id must not be empty.", nameof(id));
        if (!Enum.IsDefined(role))
            throw new ArgumentOutOfRangeException(nameof(role));
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Task action must not be empty.", nameof(action));

        Id = id;
        Role = role;
        Action = action;
        Dependencies = Array.AsReadOnly((dependencies ?? []).ToArray());
        if (Dependencies.Any(string.IsNullOrWhiteSpace))
            throw new ArgumentException("Dependency IDs must not be empty.", nameof(dependencies));
        if (Dependencies.Contains(id, StringComparer.Ordinal))
            throw new ArgumentException("A task cannot depend on itself.", nameof(dependencies));
        if (Dependencies.Distinct(StringComparer.Ordinal).Count() != Dependencies.Count)
            throw new ArgumentException("Dependency IDs must be unique.", nameof(dependencies));
    }

    /// <summary>Stable ID used to connect task dependencies and consumer observations.</summary>
    public string Id { get; }
    /// <summary>Role responsible for performing or authorizing this task.</summary>
    public StarportCrewRole Role { get; }
    /// <summary>Human-readable action intent; it does not assert that the action succeeded.</summary>
    public string Action { get; }
    /// <summary>Task IDs that must be completed before this task can start.</summary>
    public IReadOnlyList<string> Dependencies { get; }
}

/// <summary>
/// A validated, engine-neutral departure choreography. It describes work and ordering;
/// the host remains authoritative for routes, physical outcomes, personnel, and permissions.
/// </summary>
public sealed class StarportDepartureScenario
{
    private readonly IReadOnlyDictionary<string, StarportTaskDefinition> _tasks;

    /// <summary>Creates a departure scenario and validates its task graph.</summary>
    public StarportDepartureScenario(string id, IEnumerable<StarportTaskDefinition> tasks)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Scenario id must not be empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(tasks);

        var materialized = tasks.ToArray();
        if (materialized.Length == 0)
            throw new ArgumentException("A departure scenario requires at least one task.", nameof(tasks));
        if (materialized.Any(task => task is null))
            throw new ArgumentException("Task entries must not be null.", nameof(tasks));

        var map = new Dictionary<string, StarportTaskDefinition>(StringComparer.Ordinal);
        foreach (var task in materialized)
            if (!map.TryAdd(task.Id, task))
                throw new ArgumentException($"Duplicate task ID '{task.Id}'.", nameof(tasks));

        foreach (var task in materialized)
            foreach (var dependency in task.Dependencies)
                if (!map.ContainsKey(dependency))
                    throw new ArgumentException($"Task '{task.Id}' references unknown dependency '{dependency}'.", nameof(tasks));

        EnsureAcyclic(map);
        Id = id;
        Tasks = Array.AsReadOnly(materialized);
        _tasks = new System.Collections.ObjectModel.ReadOnlyDictionary<string, StarportTaskDefinition>(map);
    }

    /// <summary>Stable scenario identifier.</summary>
    public string Id { get; }
    /// <summary>Tasks in authored order; consumers should use IDs, not array positions, as identity.</summary>
    public IReadOnlyList<StarportTaskDefinition> Tasks { get; }

    /// <summary>Gets a task by stable ID, or null if the ID is not part of this scenario.</summary>
    public StarportTaskDefinition? GetTask(string id) =>
        id is not null && _tasks.TryGetValue(id, out var task) ? task : null;

    /// <summary>Creates the first starport departure slice with role branching and explicit return/seal/clearance order.</summary>
    public static StarportDepartureScenario CreateStandardDeparture() => new(
        "starport.departure.v1",
        [
            new("crew.rally", StarportCrewRole.AllCrew, "Assemble assigned personnel at the departure muster point."),
            new("crew.route-to-door", StarportCrewRole.AllCrew, "Route all personnel to the spacecraft access door.", ["crew.rally"]),
            new("flight.board", StarportCrewRole.FlightCrew, "Board through the access ramp and take assigned stations.", ["crew.route-to-door"]),
            new("ground.disconnect-services", StarportCrewRole.GroundCrew, "Disconnect and stow service hoses and umbilicals.", ["crew.route-to-door"]),
            new("ground.clear-covers", StarportCrewRole.GroundCrew, "Remove and secure hatch, engine, and service covers.", ["crew.route-to-door"]),
            new("ground.clear-landing-gear", StarportCrewRole.GroundCrew, "Complete the landing-gear and departure-clearance checks.", ["crew.route-to-door"]),
            new("ground.secure-mass-driver", StarportCrewRole.GroundCrew, "Secure mass-driver / launch-interface covers where fitted.", ["crew.route-to-door"]),
            new("ground.return-to-safe-area", StarportCrewRole.GroundCrew, "Return ground crew to the designated safe assembly area.",
                ["ground.disconnect-services", "ground.clear-covers", "ground.clear-landing-gear", "ground.secure-mass-driver"]),
            new("craft.seal-access", StarportCrewRole.FlightCrew, "Seal the access route after all flight crew are aboard and ground crew are clear.",
                ["flight.board", "ground.return-to-safe-area"]),
            new("craft.verify-manifest", StarportCrewRole.FlightCrew, "Verify crew manifest, station readiness, and departure checklist.", ["craft.seal-access"]),
            new("control.request-atc", StarportCrewRole.FlightControl, "Request departure clearance only after readiness evidence is complete.", ["craft.verify-manifest"]),
            new("control.depart", StarportCrewRole.FlightControl, "Release the departure only after explicit clearance is granted.", ["control.request-atc"])
        ]);

    private static void EnsureAcyclic(IReadOnlyDictionary<string, StarportTaskDefinition> tasks)
    {
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string id)
        {
            if (visited.Contains(id))
                return;
            if (!visiting.Add(id))
                throw new ArgumentException($"Task dependencies contain a cycle through '{id}'.", nameof(tasks));

            foreach (var dependency in tasks[id].Dependencies)
                Visit(dependency);

            visiting.Remove(id);
            visited.Add(id);
        }

        foreach (var id in tasks.Keys)
            Visit(id);
    }
}

/// <summary>Read-only result of evaluating which tasks may start and whether departure is authorized.</summary>
public sealed class StarportDepartureEvaluation
{
    internal StarportDepartureEvaluation(
        IReadOnlyList<string> readyTaskIds,
        IReadOnlyList<string> blockedTaskIds,
        bool mayRequestAtcClearance,
        bool mayDepart)
    {
        ReadyTaskIds = readyTaskIds;
        BlockedTaskIds = blockedTaskIds;
        MayRequestAtcClearance = mayRequestAtcClearance;
        MayDepart = mayDepart;
    }

    /// <summary>Tasks whose dependencies are completed and whose own status is NotStarted.</summary>
    public IReadOnlyList<string> ReadyTaskIds { get; }
    /// <summary>Tasks currently in progress or failed, plus not-started tasks with unmet dependencies.</summary>
    public IReadOnlyList<string> BlockedTaskIds { get; }
    /// <summary>True only when all pre-clearance external evidence and required tasks are complete.</summary>
    public bool MayRequestAtcClearance { get; }
    /// <summary>True only when pre-clearance evidence, ATC clearance, and the clearance task are complete.</summary>
    public bool MayDepart { get; }
}

/// <summary>Evaluates a scenario against caller-supplied task outcomes and authoritative world evidence.</summary>
public static class StarportDepartureEvaluator
{
    /// <summary>
    /// Evaluates readiness without simulating crew, routes, equipment, physics, or ATC.
    /// Unknown outcome IDs are rejected rather than silently ignored.
    /// </summary>
    public static StarportDepartureEvaluation Evaluate(
        StarportDepartureScenario scenario,
        IReadOnlyDictionary<string, StarportTaskStatus> outcomes,
        StarportDepartureEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(outcomes);

        const StarportDepartureEvidence allEvidence =
            StarportDepartureEvidence.FlightCrewAccountedFor |
            StarportDepartureEvidence.GroundEquipmentClear |
            StarportDepartureEvidence.GroundCrewReturned |
            StarportDepartureEvidence.AccessSealed |
            StarportDepartureEvidence.ManifestVerified |
            StarportDepartureEvidence.AtcClearanceGranted;
        if ((evidence & ~allEvidence) != 0)
            throw new ArgumentOutOfRangeException(nameof(evidence), "Evidence contains undefined flags.");

        foreach (var pair in outcomes)
        {
            if (scenario.GetTask(pair.Key) is null)
                throw new ArgumentException($"Outcome references unknown task '{pair.Key}'.", nameof(outcomes));
            if (!Enum.IsDefined(pair.Value))
                throw new ArgumentException($"Outcome for '{pair.Key}' has an undefined status.", nameof(outcomes));
        }

        bool Completed(string id) => outcomes.TryGetValue(id, out var status) && status == StarportTaskStatus.Completed;

        var preClearanceEvidence = StarportDepartureEvidence.FlightCrewAccountedFor |
                                   StarportDepartureEvidence.GroundEquipmentClear |
                                   StarportDepartureEvidence.GroundCrewReturned |
                                   StarportDepartureEvidence.AccessSealed |
                                   StarportDepartureEvidence.ManifestVerified;
        var requiredBeforeClearance = new[]
        {
            "crew.rally",
            "crew.route-to-door",
            "flight.board",
            "ground.disconnect-services",
            "ground.clear-covers",
            "ground.clear-landing-gear",
            "ground.secure-mass-driver",
            "ground.return-to-safe-area",
            "craft.seal-access",
            "craft.verify-manifest"
        };
        var mayRequest = (evidence & preClearanceEvidence) == preClearanceEvidence &&
                         requiredBeforeClearance.All(Completed);
        var mayDepart = mayRequest &&
                        (evidence & StarportDepartureEvidence.AtcClearanceGranted) != 0 &&
                        Completed("control.request-atc");

        bool TaskGateSatisfied(string id) => id switch
        {
            "control.request-atc" => mayRequest,
            "control.depart" => mayDepart,
            _ => true
        };

        var ready = new List<string>();
        var blocked = new List<string>();
        foreach (var task in scenario.Tasks)
        {
            var status = outcomes.TryGetValue(task.Id, out var observed)
                ? observed
                : StarportTaskStatus.NotStarted;
            if (status == StarportTaskStatus.NotStarted &&
                task.Dependencies.All(Completed) &&
                TaskGateSatisfied(task.Id))
                ready.Add(task.Id);
            else if (status != StarportTaskStatus.Completed)
                blocked.Add(task.Id);
        }

        return new StarportDepartureEvaluation(
            Array.AsReadOnly(ready.ToArray()),
            Array.AsReadOnly(blocked.ToArray()),
            mayRequest,
            mayDepart);
    }
}
