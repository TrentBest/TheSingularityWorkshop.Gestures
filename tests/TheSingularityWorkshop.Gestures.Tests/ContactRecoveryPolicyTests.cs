using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ContactRecoveryPolicyTests
{
    [Fact]
    public void DefersWhenObservationIsMissingUnknownOrUnevaluated()
    {
        Assert.Equal(ContactRecoveryAction.AwaitObservation, ContactRecoveryPolicy.Recommend(null));
        Assert.Equal(
            ContactRecoveryAction.AwaitObservation,
            ContactRecoveryPolicy.Recommend(new ContactObservation(ContactPresence.Unknown, ContactLoadOutcome.Unknown)));
        Assert.Equal(
            ContactRecoveryAction.AwaitObservation,
            ContactRecoveryPolicy.Recommend(new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Unknown)));
        Assert.Equal(
            ContactRecoveryAction.AwaitObservation,
            ContactRecoveryPolicy.Recommend(new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.NotEvaluated)));
    }

    [Fact]
    public void RequestsContactWhenNoContactIsPresent()
    {
        var observation = new ContactObservation(ContactPresence.NoContact, ContactLoadOutcome.Unknown);

        Assert.Equal(ContactRecoveryAction.SeekContact, ContactRecoveryPolicy.Recommend(observation));
    }

    [Fact]
    public void RecommendsReacquisitionAfterContactWasReleased()
    {
        var observation = new ContactObservation(ContactPresence.Released, ContactLoadOutcome.NotEvaluated);

        Assert.Equal(ContactRecoveryAction.ReacquireContact, ContactRecoveryPolicy.Recommend(observation));
    }

    [Fact]
    public void KeepsTargetOnlyWhenExternalModelReportsSupportedContact()
    {
        var observation = new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported);

        Assert.Equal(ContactRecoveryAction.ContinueCurrentTarget, ContactRecoveryPolicy.Recommend(observation));
    }

    [Theory]
    [InlineData(ContactLoadOutcome.Slipping, ContactRecoveryAction.SeekAlternativeContact)]
    [InlineData(ContactLoadOutcome.Unsupported, ContactRecoveryAction.RepositionToReduceLoad)]
    public void MapsFailedSupportOutcomesToDistinctRecoveryIntent(
        ContactLoadOutcome outcome,
        ContactRecoveryAction expected)
    {
        var observation = new ContactObservation(ContactPresence.Contact, outcome);

        Assert.Equal(expected, ContactRecoveryPolicy.Recommend(observation));
    }
}
