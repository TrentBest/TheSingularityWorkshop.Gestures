using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureOfferSelectorTests
{
    [Fact]
    public void FiltersByActorCapabilitiesAndAvailableAnchorsWithExplicitReasons()
    {
        var board = new GestureOffer(CreateGesture("board", "hatch"), ["climb", "balance"], ["hatch"]);
        var wave = new GestureOffer(CreateGesture("wave"), ["upper-body-control"]);
        var reach = new GestureOffer(CreateGesture("reach", "target"), ["reach"]);

        var result = GestureOfferSelector.Evaluate(
            [reach, board, wave],
            ["reach", "upper-body-control"],
            ["target"]);

        Assert.Equal(new[] { "reach", "wave" }, result.Eligible.Select(item => item.Definition.Id));
        var rejected = Assert.Single(result.Rejected);
        Assert.Equal("board", rejected.Offer.Definition.Id);
        Assert.Equal(new[] { "balance", "climb" }, rejected.MissingActorCapabilities);
        Assert.Equal(new[] { "hatch" }, rejected.MissingAnchorNames);
    }

    [Fact]
    public void SelectionOrderingDoesNotDependOnProviderEnumerationOrder()
    {
        var offers = new[]
        {
            new GestureOffer(CreateGesture("zeta")),
            new GestureOffer(CreateGesture("alpha")),
            new GestureOffer(CreateGesture("middle"))
        };

        var forward = GestureOfferSelector.Evaluate(offers, [], []).Eligible
            .Select(item => item.Definition.Id).ToArray();
        var reverse = GestureOfferSelector.Evaluate(offers.Reverse(), [], []).Eligible
            .Select(item => item.Definition.Id).ToArray();

        Assert.Equal(new[] { "alpha", "middle", "zeta" }, forward);
        Assert.Equal(forward, reverse);
    }

    [Fact]
    public void RejectsDuplicateOfferIdsAndUnknownRequiredAnchors()
    {
        var first = new GestureOffer(CreateGesture("same"));
        var second = new GestureOffer(CreateGesture("same"));

        Assert.Throws<ArgumentException>(() =>
            GestureOfferSelector.Evaluate([first, second], [], []));
        Assert.Throws<ArgumentException>(() =>
            new GestureOffer(CreateGesture("invalid"), requiredAnchorNames: ["missing"]));
    }

    [Fact]
    public void RejectsInvalidRequirementAndContextNames()
    {
        Assert.Throws<ArgumentException>(() =>
            new GestureOffer(CreateGesture("duplicate"), ["climb", "climb"]));
        Assert.Throws<ArgumentException>(() =>
            GestureOfferSelector.Evaluate([], [" "], []));
        Assert.Throws<ArgumentException>(() =>
            GestureOfferSelector.Evaluate([], [], [""]));
    }

    private static GestureDefinition CreateGesture(string id, params string[] anchors) => new(
        id,
        [
            Frame(0d),
            Frame(1d)
        ],
        anchors.Select(name => new GestureAnchor(name, "test-provider", Pose())));

    private static GestureKeyframe Frame(double time) =>
        new(time, new Dictionary<string, PoseTransform>
        {
            ["root"] = Pose()
        });

    private static PoseTransform Pose() =>
        new(Vector3.Zero, Quaternion.Identity, Vector3.One);
}
