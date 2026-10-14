using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ContactRecoveryTargetSelectionTests
{
    [Fact]
    public void WaitsForObservationWithoutCallingSelector()
    {
        var selector = new RecordingSelector();
        var current = new Vector2(2f, 3f);

        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            null, current, selector, out var action, out var next);

        Assert.False(selected);
        Assert.Equal(ContactRecoveryAction.AwaitObservation, action);
        Assert.Equal(current, next);
        Assert.Null(selector.LastAction);
    }

    [Fact]
    public void KeepsCurrentTargetWhenExternalModelReportsSupport()
    {
        var selector = new RecordingSelector();
        var current = new Vector2(2f, 3f);
        var observation = new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported);

        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            observation, current, selector, out var action, out var next);

        Assert.True(selected);
        Assert.Equal(ContactRecoveryAction.ContinueCurrentTarget, action);
        Assert.Equal(current, next);
        Assert.Null(selector.LastAction);
    }

    [Theory]
    [InlineData(ContactPresence.NoContact, ContactLoadOutcome.Unknown, ContactRecoveryAction.SeekContact)]
    [InlineData(ContactPresence.Released, ContactLoadOutcome.NotEvaluated, ContactRecoveryAction.ReacquireContact)]
    [InlineData(ContactPresence.Contact, ContactLoadOutcome.Slipping, ContactRecoveryAction.SeekAlternativeContact)]
    [InlineData(ContactPresence.Contact, ContactLoadOutcome.Unsupported, ContactRecoveryAction.RepositionToReduceLoad)]
    public void DelegatesRecoveryTargetChoiceToCaller(
        ContactPresence presence,
        ContactLoadOutcome outcome,
        ContactRecoveryAction expectedAction)
    {
        var selector = new RecordingSelector { Candidate = new Vector2(5f, 8f) };
        var current = new Vector2(2f, 3f);
        var observation = new ContactObservation(presence, outcome);

        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            observation, current, selector, out var action, out var next);

        Assert.True(selected);
        Assert.Equal(expectedAction, action);
        Assert.Equal(expectedAction, selector.LastAction);
        Assert.Same(observation, selector.LastObservation);
        Assert.Equal(current, selector.LastCurrentTarget);
        Assert.Equal(selector.Candidate, next);
    }

    [Fact]
    public void DoesNotReplaceTargetWhenCallerCannotSelectOne()
    {
        var selector = new RecordingSelector { CanSelect = false };
        var current = new Vector2(2f, 3f);
        var observation = new ContactObservation(ContactPresence.NoContact, ContactLoadOutcome.Unknown);

        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            observation, current, selector, out _, out var next);

        Assert.False(selected);
        Assert.Equal(current, next);
    }

    [Fact]
    public void RejectsNonFiniteCurrentTarget()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ContactRecoveryTargetSelection.TrySelectNextTarget(
                new ContactObservation(ContactPresence.NoContact, ContactLoadOutcome.Unknown),
                new Vector2(float.NaN, 0f), new RecordingSelector(), out _, out _));
    }

    [Fact]
    public void RejectsNonFiniteSelectedTargetWithoutReplacingCurrentTarget()
    {
        var selector = new RecordingSelector { Candidate = new Vector2(float.PositiveInfinity, 0f) };
        var current = new Vector2(2f, 3f);
        var observation = new ContactObservation(ContactPresence.NoContact, ContactLoadOutcome.Unknown);

        var selected = ContactRecoveryTargetSelection.TrySelectNextTarget(
            observation, current, selector, out _, out var next);

        Assert.False(selected);
        Assert.Equal(current, next);
    }

    private sealed class RecordingSelector : IContactRecoveryTargetSelector
    {
        public bool CanSelect { get; init; } = true;
        public Vector2 Candidate { get; init; }
        public ContactRecoveryAction? LastAction { get; private set; }
        public ContactObservation? LastObservation { get; private set; }
        public Vector2 LastCurrentTarget { get; private set; }

        public bool TrySelectTarget(
            ContactRecoveryAction action,
            ContactObservation observation,
            Vector2 currentTarget,
            out Vector2 target)
        {
            LastAction = action;
            LastObservation = observation;
            LastCurrentTarget = currentTarget;
            target = Candidate;
            return CanSelect;
        }
    }
}
