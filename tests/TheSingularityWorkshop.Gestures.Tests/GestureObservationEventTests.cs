using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureObservationEventTests
{
    [Fact]
    public void RecordsSharedOccurrenceWithoutChoosingObserverReactions()
    {
        var observed = new GestureObservationEvent(
            "street-performance-17",
            "street-performance",
            42.5d,
            new[] { "performer-2", "performer-1" });

        Assert.Equal("street-performance-17", observed.EventId);
        Assert.Equal("street-performance", observed.EventKind);
        Assert.Equal(42.5d, observed.OccurredAtSeconds);
        Assert.Equal(new[] { "performer-2", "performer-1" }, observed.ParticipantIds);
        Assert.Null(observed.CausedByEventId);
        Assert.DoesNotContain("Reaction", typeof(GestureObservationEvent).GetProperties().Select(x => x.Name));
        Assert.DoesNotContain("Emotion", typeof(GestureObservationEvent).GetProperties().Select(x => x.Name));
    }

    [Fact]
    public void SupportsCausalChainsWithoutOwningParentEvent()
    {
        var reaction = new GestureObservationEvent(
            "spectator-approaches-1",
            "approach-performance",
            43d,
            new[] { "spectator-9" },
            causedByEventId: "street-performance-17");

        Assert.Equal("street-performance-17", reaction.CausedByEventId);
        Assert.Single(reaction.ParticipantIds);
    }

    [Fact]
    public void ParticipantCollectionIsCopiedAndReadOnly()
    {
        var ids = new List<string> { "actor-1", "actor-2" };
        var observed = new GestureObservationEvent("event-1", "paired-motion", 0d, ids);
        ids[0] = "changed";

        Assert.Equal(new[] { "actor-1", "actor-2" }, observed.ParticipantIds);
        Assert.IsAssignableFrom<IReadOnlyList<string>>(observed.ParticipantIds);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)observed.ParticipantIds)[0] = "changed");
    }

    [Fact]
    public void RejectsInvalidIdentityTimeParticipantsAndSelfCausation()
    {
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("", "kind", 0d, new[] { "actor" }));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", " ", 0d, new[] { "actor" }));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GestureObservationEvent("event", "kind", double.NaN, new[] { "actor" }));
        Assert.Throws<ArgumentNullException>(() => new GestureObservationEvent("event", "kind", 0d, null!));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", "kind", 0d, Array.Empty<string>()));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", "kind", 0d, new[] { "actor", "actor" }));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", "kind", 0d, new[] { "actor", " " }));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", "kind", 0d, new[] { "actor" }, " "));
        Assert.Throws<ArgumentException>(() => new GestureObservationEvent("event", "kind", 0d, new[] { "actor" }, "event"));
    }
}
