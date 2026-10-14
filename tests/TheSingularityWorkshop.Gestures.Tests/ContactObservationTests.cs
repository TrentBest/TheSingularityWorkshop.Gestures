using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ContactObservationTests
{
    [Fact]
    public void StoresContactAndLoadAssessmentReportedByExternalModel()
    {
        var observation = new ContactObservation(
            ContactPresence.Contact,
            ContactLoadOutcome.Slipping,
            contactId: "granite-hold-17",
            normalForceNewtons: 120f,
            tangentialLoadNewtons: 90f,
            availableTangentialForceNewtons: 60f);

        Assert.Equal(ContactPresence.Contact, observation.Presence);
        Assert.Equal(ContactLoadOutcome.Slipping, observation.LoadOutcome);
        Assert.Equal("granite-hold-17", observation.ContactId);
        Assert.Equal(120f, observation.NormalForceNewtons);
        Assert.Equal(90f, observation.TangentialLoadNewtons);
        Assert.Equal(60f, observation.AvailableTangentialForceNewtons);
    }

    [Fact]
    public void DoesNotAllowLoadSupportWithoutActiveContact()
    {
        Assert.Throws<ArgumentException>(() =>
            new ContactObservation(ContactPresence.NoContact, ContactLoadOutcome.Supported));
        Assert.Throws<ArgumentException>(() =>
            new ContactObservation(ContactPresence.Released, ContactLoadOutcome.Slipping));
        Assert.Throws<ArgumentException>(() =>
            new ContactObservation(ContactPresence.Unknown, ContactLoadOutcome.Unsupported));
    }

    [Fact]
    public void RejectsInvalidForceMagnitudesAndEnumValues()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported, normalForceNewtons: -1f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported, tangentialLoadNewtons: float.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported, availableTangentialForceNewtons: float.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ContactObservation((ContactPresence)999, ContactLoadOutcome.Unknown));
    }

    [Fact]
    public void JointRetainsExternalContactFeedbackWithoutFabricatingMotion()
    {
        var hinge = new StatefulHinge(0f, -1f, 1f);
        var joint = new StatefulJointNode("hand", 1f, hinge);
        Assert.Equal(1, joint.EvaluateIncremental(System.Numerics.Vector2.Zero));
        var originalEnd = joint.WorldEnd;
        var observation = new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Unsupported);

        joint.ObserveContact(observation);

        Assert.Equal(observation, joint.LastContactObservation);
        Assert.Equal(0, joint.EvaluateIncremental(System.Numerics.Vector2.Zero));
        Assert.Equal(originalEnd, joint.WorldEnd);
    }
}
