using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Executable consumer-side scenario contract for Star Port routing. The fixture models
/// orchestration decisions only; it does not simulate navigation, equipment, physics, or ATC.
/// </summary>
public sealed class StarPortRoutingScenarioTests
{
    [Fact]
    public void AllCrewUseCommonWaypointBeforeRoleSpecificRouting()
    {
        var scenario = StarPortScenario.Create(groundCrew: 8, flightCrew: 16);

        foreach (var agent in scenario.Agents)
            Assert.Equal(StarPortStage.CommonAccessWaypoint, scenario.RouteAtCommonWaypoint(agent.Id));

        Assert.Equal(8, scenario.Agents.Count(a => a.Role == CrewRole.Ground));
        Assert.Equal(16, scenario.Agents.Count(a => a.Role == CrewRole.Flight));
        Assert.Equal(StarPortRoute.GroundService, scenario.RouteByRole("ground-01"));
        Assert.Equal(StarPortRoute.BoardingRamp, scenario.RouteByRole("flight-01"));
    }

    [Fact]
    public void GroundTasksRequireAuthoritativeEvidenceBeforeReturnAndDoorSeal()
    {
        var scenario = StarPortScenario.Create(groundCrew: 1, flightCrew: 1);
        scenario.RouteAtCommonWaypoint("ground-01");
        scenario.RouteByRole("ground-01");

        scenario.AssignGroundTask("ground-01", "landing-gear-pressure");
        scenario.ReportTaskEvidence("ground-01", TaskEvidence.Indeterminate);
        Assert.Equal(StarPortStage.ServiceTask, scenario.StageOf("ground-01"));
        Assert.False(scenario.CanSealAccessDoor);

        scenario.ReportTaskEvidence("ground-01", TaskEvidence.Passed);
        Assert.Equal(StarPortStage.ReturnToAccessDoor, scenario.StageOf("ground-01"));
        Assert.False(scenario.CanSealAccessDoor);

        scenario.ConfirmReturnedAndClear("ground-01");
        Assert.True(scenario.CanSealAccessDoor);
        scenario.SealAccessDoor();
        Assert.True(scenario.AccessDoorSealed);
    }

    [Fact]
    public void FlightCrewMustBeBoardedAndAssignedStationsVerified()
    {
        var scenario = StarPortScenario.Create(groundCrew: 0, flightCrew: 2);
        foreach (var agent in scenario.Agents)
        {
            scenario.RouteAtCommonWaypoint(agent.Id);
            scenario.RouteByRole(agent.Id);
            scenario.AssignFlightStation(agent.Id, $"station-{agent.Id}");
        }

        Assert.False(scenario.CanSealAccessDoor);
        scenario.ConfirmBoardedAndStationOccupied("flight-01");
        Assert.False(scenario.CanSealAccessDoor);
        scenario.ConfirmBoardedAndStationOccupied("flight-02");
        Assert.True(scenario.CanSealAccessDoor);
    }

    [Fact]
    public void ReadinessAndDoorSealDoNotImplyAtcClearance()
    {
        var scenario = StarPortScenario.Create(groundCrew: 0, flightCrew: 1);
        scenario.RouteAtCommonWaypoint("flight-01");
        scenario.RouteByRole("flight-01");
        scenario.AssignFlightStation("flight-01", "pilot-seat");
        scenario.ConfirmBoardedAndStationOccupied("flight-01");
        scenario.SealAccessDoor();

        scenario.SetDepartureReadiness(true);
        Assert.True(scenario.DepartureReady);
        Assert.False(scenario.AtcClearanceGranted);
        Assert.False(scenario.MayDepart);

        scenario.SetAtcClearance(false);
        Assert.False(scenario.MayDepart);
        scenario.SetAtcClearance(true);
        Assert.True(scenario.MayDepart);
    }

    [Fact]
    public void MissingEvidenceFailureAndBlockedRouteNeverCountAsCompletion()
    {
        var scenario = StarPortScenario.Create(groundCrew: 1, flightCrew: 0);
        scenario.RouteAtCommonWaypoint("ground-01");
        scenario.RouteByRole("ground-01");
        scenario.AssignGroundTask("ground-01", "mass-driver-cover");

        Assert.Throws<InvalidOperationException>(() => scenario.ConfirmReturnedAndClear("ground-01"));
        scenario.ReportTaskEvidence("ground-01", TaskEvidence.Failed);
        Assert.Equal(StarPortStage.NeedsResolution, scenario.StageOf("ground-01"));
        Assert.False(scenario.CanSealAccessDoor);

        scenario.MarkRouteBlocked("ground-01");
        Assert.Equal(StarPortStage.NeedsResolution, scenario.StageOf("ground-01"));
    }

    [Fact]
    public void GesturePoseCompletionIsNotAcceptedAsTaskEvidence()
    {
        var gesture = new GestureDefinition(
            "crew.inspect",
            [
                new GestureKeyframe(0.0, new Dictionary<string, PoseTransform>()),
                new GestureKeyframe(1.0, new Dictionary<string, PoseTransform>())
            ]);
        var pose = GestureEvaluator.Evaluate(gesture, 1.0, 1.0);

        Assert.True(pose.IsComplete);
        var scenario = StarPortScenario.Create(groundCrew: 1, flightCrew: 0);
        scenario.RouteAtCommonWaypoint("ground-01");
        scenario.RouteByRole("ground-01");
        scenario.AssignGroundTask("ground-01", "landing-gear-pressure");
        Assert.Throws<InvalidOperationException>(() => scenario.AcceptPoseAsTaskEvidence(pose));
        Assert.Equal(StarPortStage.ServiceTask, scenario.StageOf("ground-01"));
    }

    private enum CrewRole { Ground, Flight }
    private enum StarPortRoute { GroundService, BoardingRamp }
    private enum StarPortStage
    {
        Muster,
        CommonAccessWaypoint,
        ServiceTask,
        ReturnToAccessDoor,
        Boarding,
        AtAssignedStation,
        ClearOfAccess,
        NeedsResolution
    }
    private enum TaskEvidence { Passed, Failed, Indeterminate }

    private sealed record CrewAgent(string Id, CrewRole Role);

    /// <summary>Intentionally small fixture, not a production scheduler or world simulation.</summary>
    private sealed class StarPortScenario
    {
        private readonly Dictionary<string, StarPortStage> _stages;
        private readonly Dictionary<string, string> _groundTasks = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _flightStations = new(StringComparer.Ordinal);
        private readonly HashSet<string> _clearAgents = new(StringComparer.Ordinal);
        private readonly HashSet<string> _boardedAgents = new(StringComparer.Ordinal);

        private StarPortScenario(IReadOnlyList<CrewAgent> agents)
        {
            Agents = agents;
            _stages = agents.ToDictionary(a => a.Id, _ => StarPortStage.Muster, StringComparer.Ordinal);
        }

        public IReadOnlyList<CrewAgent> Agents { get; }
        public bool AccessDoorSealed { get; private set; }
        public bool DepartureReady { get; private set; }
        public bool AtcClearanceGranted { get; private set; }
        public bool MayDepart => AccessDoorSealed && DepartureReady && AtcClearanceGranted;
        public bool CanSealAccessDoor =>
            Agents.All(a => a.Role == CrewRole.Ground
                ? _clearAgents.Contains(a.Id)
                : _boardedAgents.Contains(a.Id));

        public static StarPortScenario Create(int groundCrew, int flightCrew)
        {
            if (groundCrew < 0) throw new ArgumentOutOfRangeException(nameof(groundCrew));
            if (flightCrew < 0) throw new ArgumentOutOfRangeException(nameof(flightCrew));
            var agents = Enumerable.Range(1, groundCrew)
                .Select(i => new CrewAgent($"ground-{i:00}", CrewRole.Ground))
                .Concat(Enumerable.Range(1, flightCrew)
                    .Select(i => new CrewAgent($"flight-{i:00}", CrewRole.Flight)))
                .ToArray();
            if (agents.Length == 0)
                throw new ArgumentException("Scenario requires at least one crew member.");
            return new StarPortScenario(Array.AsReadOnly(agents));
        }

        public StarPortStage RouteAtCommonWaypoint(string id)
        {
            RequireStage(id, StarPortStage.Muster);
            _stages[id] = StarPortStage.CommonAccessWaypoint;
            return _stages[id];
        }

        public StarPortRoute RouteByRole(string id)
        {
            RequireStage(id, StarPortStage.CommonAccessWaypoint);
            var agent = Find(id);
            if (agent.Role == CrewRole.Ground)
            {
                _stages[id] = StarPortStage.ServiceTask;
                return StarPortRoute.GroundService;
            }
            _stages[id] = StarPortStage.Boarding;
            return StarPortRoute.BoardingRamp;
        }

        public void AssignGroundTask(string id, string taskId)
        {
            RequireRole(id, CrewRole.Ground);
            RequireStage(id, StarPortStage.ServiceTask);
            if (string.IsNullOrWhiteSpace(taskId)) throw new ArgumentException("Task ID is required.", nameof(taskId));
            _groundTasks[id] = taskId;
        }

        public void ReportTaskEvidence(string id, TaskEvidence evidence)
        {
            RequireStage(id, StarPortStage.ServiceTask);
            if (!_groundTasks.ContainsKey(id))
                throw new InvalidOperationException("No ground task is assigned.");
            if (evidence == TaskEvidence.Passed)
                _stages[id] = StarPortStage.ReturnToAccessDoor;
            else if (evidence == TaskEvidence.Failed)
                _stages[id] = StarPortStage.NeedsResolution;
            // Indeterminate evidence leaves the task in progress.
        }

        public void ConfirmReturnedAndClear(string id)
        {
            RequireStage(id, StarPortStage.ReturnToAccessDoor);
            _clearAgents.Add(id);
            _stages[id] = StarPortStage.ClearOfAccess;
        }

        public void AssignFlightStation(string id, string stationId)
        {
            RequireRole(id, CrewRole.Flight);
            RequireStage(id, StarPortStage.Boarding);
            if (string.IsNullOrWhiteSpace(stationId)) throw new ArgumentException("Station ID is required.", nameof(stationId));
            _flightStations[id] = stationId;
        }

        public void ConfirmBoardedAndStationOccupied(string id)
        {
            RequireRole(id, CrewRole.Flight);
            RequireStage(id, StarPortStage.Boarding);
            if (!_flightStations.ContainsKey(id))
                throw new InvalidOperationException("No flight station is assigned.");
            _boardedAgents.Add(id);
            _stages[id] = StarPortStage.AtAssignedStation;
        }

        public void MarkRouteBlocked(string id)
        {
            _ = Find(id);
            _stages[id] = StarPortStage.NeedsResolution;
            _clearAgents.Remove(id);
            _boardedAgents.Remove(id);
        }

        public void SealAccessDoor()
        {
            if (!CanSealAccessDoor)
                throw new InvalidOperationException("Required crew are not all clear or verified at their assigned stations.");
            AccessDoorSealed = true;
        }

        public void SetDepartureReadiness(bool ready) => DepartureReady = ready;
        public void SetAtcClearance(bool granted) => AtcClearanceGranted = granted;

        public StarPortStage StageOf(string id) => _stages.TryGetValue(id, out var stage)
            ? stage : throw new ArgumentException($"Unknown crew ID '{id}'.", nameof(id));

        public void AcceptPoseAsTaskEvidence(EvaluatedPose pose)
        {
            ArgumentNullException.ThrowIfNull(pose);
            throw new InvalidOperationException("Pose progression is not authoritative task evidence.");
        }

        private CrewAgent Find(string id) =>
            Agents.FirstOrDefault(a => string.Equals(a.Id, id, StringComparison.Ordinal))
            ?? throw new ArgumentException($"Unknown crew ID '{id}'.", nameof(id));

        private void RequireRole(string id, CrewRole role)
        {
            if (Find(id).Role != role)
                throw new InvalidOperationException($"Crew member '{id}' is not assigned role '{role}'.");
        }

        private void RequireStage(string id, StarPortStage expected)
        {
            var actual = StageOf(id);
            if (actual != expected)
                throw new InvalidOperationException($"Crew member '{id}' is in stage '{actual}', expected '{expected}'.");
        }
    }
}
