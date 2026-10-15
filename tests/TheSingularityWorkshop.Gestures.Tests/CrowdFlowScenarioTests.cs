using System.Numerics;
using Xunit;
using TheSingularityWorkshop.Gestures;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class CrowdFlowScenarioTests
{
    [Fact]
    public void ScenarioAcceptsExperienceAuthoredRoutes()
    {
        var scenario = new CrowdFlowScenario("busy-shop",
        [
            new CrowdFlowGate("sidewalk-west", new Vector2(0, 0)),
            new CrowdFlowGate("shop-interior", new Vector2(4, 0)),
            new CrowdFlowGate("garage-ramp", new Vector2(0, 8))
        ],
        [
            new CrowdFlowRoute("enter-shop", "sidewalk-west", "shop-interior",
                [new Vector2(1, 0), new Vector2(2, 0)]),
            new CrowdFlowRoute("to-garage", "sidewalk-west", "garage-ramp")
        ]);

        Assert.Equal(3, scenario.Gates.Count);
        Assert.Equal(2, scenario.Routes.Count);
        Assert.Equal("shop-interior", scenario.GetRoute("enter-shop").ExitGateId);
        Assert.Equal(new Vector2(2, 0), scenario.GetRoute("enter-shop").Waypoints[1]);
    }

    [Fact]
    public void ScenarioRejectsRoutesWithUnknownGates()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowScenario("bad",
            [new CrowdFlowGate("sidewalk", Vector2.Zero)],
            [new CrowdFlowRoute("missing-exit", "sidewalk", "shop")]));
    }

    [Fact]
    public void ScenarioRejectsDuplicateGateAndRouteIds()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowScenario("bad",
            [new CrowdFlowGate("same", Vector2.Zero), new CrowdFlowGate("same", Vector2.One)], []));
        Assert.Throws<ArgumentException>(() => new CrowdFlowScenario("bad",
            [new CrowdFlowGate("a", Vector2.Zero), new CrowdFlowGate("b", Vector2.One)],
            [new CrowdFlowRoute("same", "a", "b"), new CrowdFlowRoute("same", "b", "a")]));
    }

    [Fact]
    public void RouteRejectsInvalidWaypointsAndIdenticalGates()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowRoute("loop", "a", "a"));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CrowdFlowRoute("bad", "a", "b",
            [new Vector2(float.NaN, 0)]));
    }

    [Theory]
    [InlineData(CrowdTraversalMode.IndividualUnhurried, 1f, 0.3f, 0f)]
    [InlineData(CrowdTraversalMode.IndividualHurried, 1.5f, 0.2f, 0f)]
    [InlineData(CrowdTraversalMode.GroupUnhurried, 1f, 0.4f, 0.35f)]
    [InlineData(CrowdTraversalMode.GroupHurried, 1.5f, 0.3f, 0.2f)]
    public void TraversalProfilesRepresentFourCommonIntents(
        CrowdTraversalMode mode, float speed, float personalSpace, float cohesion)
    {
        var profile = new CrowdTraversalProfile(mode, speed, personalSpace, cohesion);
        Assert.Equal(mode, profile.Mode);
        Assert.Equal(speed, profile.SpeedMultiplier);
        Assert.Equal(personalSpace, profile.PersonalSpaceRadius);
        Assert.Equal(cohesion, profile.GroupCohesion);
    }

    [Fact]
    public void IndividualProfilesCannotClaimGroupCohesion()
    {
        Assert.Throws<ArgumentException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.IndividualUnhurried, 1f, 0.3f, 0.5f));
    }

    [Fact]
    public void ProfileRejectsInvalidTuning()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupHurried, 0f, 0.3f, 0.2f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupHurried, 1f, -1f, 0.2f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupHurried, 1f, 0.3f, 1.1f));
    }

    [Fact]
    public void GateRejectsBlankIdsAndNonFinitePositions()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowGate(" ", Vector2.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdFlowGate("bad-position", new Vector2(float.PositiveInfinity, 0)));
    }

    [Fact]
    public void RouteRejectsBlankIdentifiers()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowRoute("", "a", "b"));
        Assert.Throws<ArgumentException>(() => new CrowdFlowRoute("route", " ", "b"));
        Assert.Throws<ArgumentException>(() => new CrowdFlowRoute("route", "a", ""));
    }

    [Fact]
    public void ScenarioRejectsMissingInputsAndNullEntries()
    {
        Assert.Throws<ArgumentException>(() => new CrowdFlowScenario("", [], []));
        Assert.Throws<ArgumentNullException>(() => new CrowdFlowScenario("scenario", null!, []));
        Assert.Throws<ArgumentNullException>(() => new CrowdFlowScenario("scenario", [], null!));
        Assert.Throws<ArgumentNullException>(() => new CrowdFlowScenario("scenario", [null!], []));
        Assert.Throws<ArgumentNullException>(() => new CrowdFlowScenario("scenario",
            [new CrowdFlowGate("a", Vector2.Zero), new CrowdFlowGate("b", Vector2.One)], [null!]));
    }

    [Fact]
    public void ScenarioRejectsUnknownRouteLookup()
    {
        var scenario = new CrowdFlowScenario("scenario",
            [new CrowdFlowGate("a", Vector2.Zero), new CrowdFlowGate("b", Vector2.One)],
            [new CrowdFlowRoute("route", "a", "b")]);

        Assert.Throws<KeyNotFoundException>(() => scenario.GetRoute("missing"));
    }

    [Fact]
    public void ProfileRejectsUndefinedModesAndNonFiniteTuning()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile((CrowdTraversalMode)999, 1f, 0.3f, 0f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupUnhurried, float.NaN, 0.3f, 0.2f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupUnhurried, 1f, float.PositiveInfinity, 0.2f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdTraversalProfile(CrowdTraversalMode.GroupUnhurried, 1f, 0.3f, float.NaN));
    }
}
