using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Cross-contract fixture: provider eligibility is not actor intent, pose output is not
/// an observation, and an observation is not an authoritative domain outcome.
/// The observation/outcome records below are fixture-local boundary examples, not package APIs.
/// </summary>
public sealed class ProviderToSurpsInteractionFixtureTests
{
    [Fact]
    public void ActorFiltersProviderOffersChoosesIntentAndHostAdvancesOnlyChosenProcedure()
    {
        var ladder = new GestureOffer(
            CreateGesture("climb-ladder", "ladder-rung"),
            requiredActorCapabilities: ["climb", "balance"],
            requiredAnchorNames: ["ladder-rung"]);
        var sword = new GestureOffer(
            CreateGesture("draw-sword", "sword-grip"),
            requiredActorCapabilities: ["wield"],
            requiredAnchorNames: ["sword-grip"]);

        var actor = GestureOfferSelector.Evaluate(
            [sword, ladder],
            actorCapabilities: ["climb", "balance"],
            availableAnchorNames: ["ladder-rung"]);

        Assert.Equal("climb-ladder", Assert.Single(actor.Eligible).Definition.Id);
        var rejected = Assert.Single(actor.Rejected);
        Assert.Equal("draw-sword", rejected.Offer.Definition.Id);
        Assert.Equal(new[] { "wield" }, rejected.MissingActorCapabilities);
        Assert.Equal(new[] { "sword-grip" }, rejected.MissingAnchorNames);

        // SURPS/consumer policy supplies the intent. The selector only returns candidates.
        const string selectedIntent = "climb-ladder";
        var selectedOffer = Assert.Single(actor.Eligible, offer => offer.Definition.Id == selectedIntent);
        var playback = new GesturePlayback(selectedOffer.Definition, durationSeconds: 2d);

        var initialPose = playback.Start();
        Assert.Equal(GesturePlaybackState.Playing, playback.State);
        Assert.Equal(0d, initialPose.NormalizedProgress);

        // The host, not Gestures, supplies time. One second of a two-second procedure advances halfway.
        var midPose = playback.Advance(1d);
        Assert.Equal(GesturePlaybackState.Playing, playback.State);
        Assert.Equal(0.5d, midPose.NormalizedProgress, precision: 6);

        // A sampled pose is presentation/evaluation data. It does not assert contact or success.
        var observation = new FixtureObservation(selectedIntent, "ladder-rung", "contact-reported");
        var physicalOutcome = new FixturePhysicalOutcome(selectedIntent, "climb-completed", authoritative: false);
        Assert.NotSame(observation, physicalOutcome);
        Assert.NotEqual(observation.Kind, physicalOutcome.Result);
        Assert.False(physicalOutcome.Authoritative);
        Assert.False(midPose.IsComplete);
    }

    [Fact]
    public void MissingActorCapabilityAndMissingAnchorAreReportedIndependently()
    {
        var ladder = new GestureOffer(
            CreateGesture("climb-ladder", "ladder-rung"),
            requiredActorCapabilities: ["climb", "balance"],
            requiredAnchorNames: ["ladder-rung"]);

        var result = GestureOfferSelector.Evaluate(
            [ladder],
            actorCapabilities: ["climb"],
            availableAnchorNames: []);

        var rejection = Assert.Single(result.Rejected);
        Assert.Equal(new[] { "balance" }, rejection.MissingActorCapabilities);
        Assert.Equal(new[] { "ladder-rung" }, rejection.MissingAnchorNames);
        Assert.Empty(result.Eligible);
    }

    private static GestureDefinition CreateGesture(string id, string anchorName) => new(
        id,
        [
            Frame(0d),
            Frame(1d)
        ],
        [new GestureAnchor(anchorName, "fixture-provider", Pose())]);

    private static GestureKeyframe Frame(double time) =>
        new(time, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose()
        });

    private static PoseTransform Pose() =>
        new(Vector3.Zero, Quaternion.Identity, Vector3.One);

    private sealed record FixtureObservation(string ProcedureId, string Subject, string Kind);

    private sealed record FixturePhysicalOutcome(
        string ProcedureId, string Result, bool Authoritative);
}
