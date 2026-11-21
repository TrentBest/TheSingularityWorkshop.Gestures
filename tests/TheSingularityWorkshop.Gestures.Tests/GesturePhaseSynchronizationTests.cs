using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GesturePhaseSynchronizationTests
{
    [Fact]
    public void InPhaseParticipantsHaveUnitOrderParameter()
    {
        var result = GesturePhaseSynchronization.Evaluate(new[]
        {
            new GesturePhaseObservation("b", 0.75d),
            new GesturePhaseObservation("a", 0.75d),
            new GesturePhaseObservation("c", 0.75d)
        });

        Assert.Equal(1d, result.OrderParameterMagnitude, 12);
        Assert.True(result.HasDefinedMeanPhase);
        Assert.Equal(0.75d, result.MeanPhaseRadians, 12);
        Assert.Equal(new[] { "a", "b", "c" }, result.Participants.Select(item => item.ParticipantId));
    }

    [Fact]
    public void EquallySpacedParticipantsHaveNoDefinedMeanPhase()
    {
        var result = GesturePhaseSynchronization.Evaluate(new[]
        {
            new GesturePhaseObservation("a", 0d),
            new GesturePhaseObservation("b", 2d * Math.PI / 3d),
            new GesturePhaseObservation("c", 4d * Math.PI / 3d)
        });

        Assert.InRange(result.OrderParameterMagnitude, 0d, 1e-12);
        Assert.False(result.HasDefinedMeanPhase);
        Assert.Equal(0d, result.MeanPhaseRadians);
    }

    [Fact]
    public void EquivalentWholeCyclePhasesWrapToTheSameValue()
    {
        var result = GesturePhaseSynchronization.Evaluate(new[]
        {
            new GesturePhaseObservation("a", 0d),
            new GesturePhaseObservation("b", 4d * Math.PI),
            new GesturePhaseObservation("c", -2d * Math.PI)
        });

        Assert.Equal(1d, result.OrderParameterMagnitude, 12);
        Assert.All(result.Participants, participant =>
            Assert.Equal(0d, participant.WrappedPhaseRadians, 12));
    }

    [Fact]
    public void ReversingInputEnumerationDoesNotChangeTheResult()
    {
        var observations = new[]
        {
            new GesturePhaseObservation("zeta", -0.4d),
            new GesturePhaseObservation("alpha", 2.1d),
            new GesturePhaseObservation("mu", 5.7d)
        };

        var forward = GesturePhaseSynchronization.Evaluate(observations);
        var reverse = GesturePhaseSynchronization.Evaluate(observations.Reverse());

        Assert.Equal(forward.OrderParameterMagnitude, reverse.OrderParameterMagnitude, 14);
        Assert.Equal(forward.MeanPhaseRadians, reverse.MeanPhaseRadians, 14);
        Assert.Equal(
            forward.Participants.Select(item => (item.ParticipantId, item.PhaseRadians, item.WrappedPhaseRadians)),
            reverse.Participants.Select(item => (item.ParticipantId, item.PhaseRadians, item.WrappedPhaseRadians)));
    }

    [Fact]
    public void RejectsEmptyPopulationAndDuplicateIds()
    {
        Assert.Throws<ArgumentException>(() =>
            GesturePhaseSynchronization.Evaluate(Array.Empty<GesturePhaseObservation>()));

        Assert.Throws<ArgumentException>(() => GesturePhaseSynchronization.Evaluate(new[]
        {
            new GesturePhaseObservation("same", 0d),
            new GesturePhaseObservation("same", 1d)
        }));
    }

    [Fact]
    public void RejectsDefaultObservationAndInvalidBudget()
    {
        Assert.Throws<ArgumentException>(() =>
            GesturePhaseSynchronization.Evaluate(new[] { default(GesturePhaseObservation) }));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GesturePhaseSynchronization.Evaluate(
                new[] { new GesturePhaseObservation("a", 0d) }, maximumParticipants: 0));
        Assert.Throws<InvalidOperationException>(() =>
            GesturePhaseSynchronization.Evaluate(
                new[] { new GesturePhaseObservation("a", 0d), new GesturePhaseObservation("b", 1d) },
                maximumParticipants: 1));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void RejectsNonFinitePhase(double phaseRadians)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new GesturePhaseObservation("participant", phaseRadians));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GesturePhaseSynchronization.WrapPositive(phaseRadians));
    }

    [Fact]
    public void WrapsNegativePhaseIntoPositiveCycle()
    {
        var wrapped = GesturePhaseSynchronization.WrapPositive(-Math.PI / 2d);

        Assert.Equal(1.5d * Math.PI, wrapped, 12);
        Assert.InRange(wrapped, 0d, 2d * Math.PI);
    }
}
