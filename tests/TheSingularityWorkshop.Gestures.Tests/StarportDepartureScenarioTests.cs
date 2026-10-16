using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StarportDepartureScenarioTests
{
    [Fact]
    public void StandardDepartureHasRoleBranchesAndSafeReturnBeforeSealing()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();

        Assert.Equal("starport.departure.v1", scenario.Id);
        Assert.Equal(12, scenario.Tasks.Count);
        Assert.Equal(StarportCrewRole.FlightCrew, scenario.GetTask("flight.board")!.Role);
        Assert.Equal(StarportCrewRole.GroundCrew, scenario.GetTask("ground.clear-covers")!.Role);
        Assert.Equal(
            new[] { "ground.disconnect-services", "ground.clear-covers", "ground.clear-landing-gear", "ground.secure-mass-driver" },
            scenario.GetTask("ground.return-to-safe-area")!.Dependencies);
        Assert.Contains("ground.return-to-safe-area", scenario.GetTask("craft.seal-access")!.Dependencies);
    }

    [Fact]
    public void ScenarioRejectsDuplicateAndUnknownTaskReferences()
    {
        Assert.Throws<ArgumentException>(() => new StarportDepartureScenario("duplicate",
        [
            new("same", StarportCrewRole.AllCrew, "first"),
            new("same", StarportCrewRole.GroundCrew, "second")
        ]));

        Assert.Throws<ArgumentException>(() => new StarportDepartureScenario("unknown",
        [
            new("one", StarportCrewRole.AllCrew, "one", ["missing"])
        ]));
    }

    [Fact]
    public void ScenarioRejectsDependencyCycles()
    {
        Assert.Throws<ArgumentException>(() => new StarportDepartureScenario("cycle",
        [
            new("a", StarportCrewRole.AllCrew, "a", ["b"]),
            new("b", StarportCrewRole.AllCrew, "b", ["a"])
        ]));
    }

    [Fact]
    public void InitialEvaluationOnlyMakesMusterTaskReady()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();

        var result = StarportDepartureEvaluator.Evaluate(
            scenario,
            new Dictionary<string, StarportTaskStatus>(),
            StarportDepartureEvidence.None);

        Assert.Equal(new[] { "crew.rally" }, result.ReadyTaskIds);
        Assert.False(result.MayRequestAtcClearance);
        Assert.False(result.MayDepart);
    }

    [Fact]
    public void FlightCrewAndGroundCrewCanBranchAfterSharedRouting()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();
        var outcomes = new Dictionary<string, StarportTaskStatus>
        {
            ["crew.rally"] = StarportTaskStatus.Completed,
            ["crew.route-to-door"] = StarportTaskStatus.Completed
        };

        var result = StarportDepartureEvaluator.Evaluate(scenario, outcomes, StarportDepartureEvidence.None);

        Assert.Contains("flight.board", result.ReadyTaskIds);
        Assert.Contains("ground.disconnect-services", result.ReadyTaskIds);
        Assert.Contains("ground.clear-covers", result.ReadyTaskIds);
        Assert.Contains("ground.clear-landing-gear", result.ReadyTaskIds);
        Assert.Contains("ground.secure-mass-driver", result.ReadyTaskIds);
        Assert.DoesNotContain("ground.return-to-safe-area", result.ReadyTaskIds);
    }

    [Fact]
    public void FailedGroundTaskPreventsDownstreamReturnAndSeal()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();
        var outcomes = new Dictionary<string, StarportTaskStatus>
        {
            ["crew.rally"] = StarportTaskStatus.Completed,
            ["crew.route-to-door"] = StarportTaskStatus.Completed,
            ["ground.disconnect-services"] = StarportTaskStatus.Completed,
            ["ground.clear-covers"] = StarportTaskStatus.Completed,
            ["ground.clear-landing-gear"] = StarportTaskStatus.Failed,
            ["ground.secure-mass-driver"] = StarportTaskStatus.Completed
        };

        var result = StarportDepartureEvaluator.Evaluate(scenario, outcomes, StarportDepartureEvidence.None);

        Assert.DoesNotContain("ground.return-to-safe-area", result.ReadyTaskIds);
        Assert.DoesNotContain("craft.seal-access", result.ReadyTaskIds);
        Assert.False(result.MayRequestAtcClearance);
    }

    [Fact]
    public void ClearanceCannotBeRequestedWithoutAllTaskAndExternalEvidence()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();
        var outcomes = CompleteAllButClearanceAndDeparture(scenario);
        var allPreClearanceEvidence = StarportDepartureEvidence.FlightCrewAccountedFor |
                                     StarportDepartureEvidence.GroundEquipmentClear |
                                     StarportDepartureEvidence.GroundCrewReturned |
                                     StarportDepartureEvidence.AccessSealed |
                                     StarportDepartureEvidence.ManifestVerified;

        var withoutManifest = StarportDepartureEvaluator.Evaluate(
            scenario, outcomes, allPreClearanceEvidence & ~StarportDepartureEvidence.ManifestVerified);
        Assert.False(withoutManifest.MayRequestAtcClearance);
        Assert.DoesNotContain("control.request-atc", withoutManifest.ReadyTaskIds);

        var ready = StarportDepartureEvaluator.Evaluate(scenario, outcomes, allPreClearanceEvidence);
        Assert.True(ready.MayRequestAtcClearance);
        Assert.Contains("control.request-atc", ready.ReadyTaskIds);
        Assert.DoesNotContain("control.depart", ready.ReadyTaskIds);
        Assert.False(ready.MayDepart);
    }

    [Fact]
    public void DepartureRequiresCompletedClearanceRequestAndAtcEvidence()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();
        var outcomes = CompleteAllButClearanceAndDeparture(scenario);
        var evidence = StarportDepartureEvidence.FlightCrewAccountedFor |
                       StarportDepartureEvidence.GroundEquipmentClear |
                       StarportDepartureEvidence.GroundCrewReturned |
                       StarportDepartureEvidence.AccessSealed |
                       StarportDepartureEvidence.ManifestVerified |
                       StarportDepartureEvidence.AtcClearanceGranted;

        var beforeRequest = StarportDepartureEvaluator.Evaluate(scenario, outcomes, evidence);
        Assert.False(beforeRequest.MayDepart);
        Assert.DoesNotContain("control.depart", beforeRequest.ReadyTaskIds);

        outcomes["control.request-atc"] = StarportTaskStatus.Completed;
        var afterRequest = StarportDepartureEvaluator.Evaluate(scenario, outcomes, evidence);
        Assert.True(afterRequest.MayDepart);
        Assert.Contains("control.depart", afterRequest.ReadyTaskIds);
    }

    [Fact]
    public void EvaluatorRejectsUnknownTaskAndUndefinedEvidence()
    {
        var scenario = StarportDepartureScenario.CreateStandardDeparture();

        Assert.Throws<ArgumentException>(() => StarportDepartureEvaluator.Evaluate(
            scenario,
            new Dictionary<string, StarportTaskStatus> { ["not-in-scenario"] = StarportTaskStatus.Completed },
            StarportDepartureEvidence.None));

        Assert.Throws<ArgumentOutOfRangeException>(() => StarportDepartureEvaluator.Evaluate(
            scenario,
            new Dictionary<string, StarportTaskStatus>(),
            (StarportDepartureEvidence)(1 << 20)));
    }

    private static Dictionary<string, StarportTaskStatus> CompleteAllButClearanceAndDeparture(
        StarportDepartureScenario scenario)
    {
        return scenario.Tasks
            .Where(task => task.Id is not "control.request-atc" and not "control.depart")
            .ToDictionary(task => task.Id, _ => StarportTaskStatus.Completed, StringComparer.Ordinal);
    }
}
