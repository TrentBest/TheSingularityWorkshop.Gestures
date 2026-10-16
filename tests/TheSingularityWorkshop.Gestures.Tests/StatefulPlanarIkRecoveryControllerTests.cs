using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulPlanarIkRecoveryControllerTests
{
    private static readonly JointAngleLimits FullTurn = new(-MathF.PI, MathF.PI);

    [Fact]
    public void DefersKinematicMutationUntilAnObservationAndTargetAreAvailable()
    {
        var (kinematics, shoulder, elbow) = CreateKinematics();
        var selector = new RecordingSelector { CanSelect = false };
        var initialTarget = new Vector2(1f, 1f);
        var controller = new StatefulPlanarIkRecoveryController(kinematics, selector, initialTarget);

        var result = controller.Advance(
            null, Vector2.Zero, 1f, MathF.PI, MathF.PI);

        Assert.False(result.TargetSelected);
        Assert.Equal(ContactRecoveryAction.AwaitObservation, result.RecoveryAction);
        Assert.Null(result.KinematicStep);
        Assert.Equal(initialTarget, result.CurrentTarget);
        Assert.Equal(initialTarget, controller.CurrentTarget);
        Assert.Equal(0f, shoulder.Hinge.AngleRadians);
        Assert.Equal(0f, elbow.Hinge.AngleRadians);
        Assert.Null(selector.LastAction);
    }

    [Fact]
    public void SupportedContactKeepsCurrentTargetAndAdvancesKinematics()
    {
        var (kinematics, _, _) = CreateKinematics();
        var selector = new RecordingSelector();
        var target = new Vector2(1f, 1f);
        var controller = new StatefulPlanarIkRecoveryController(kinematics, selector, target);
        var observation = new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Supported);

        var result = controller.Advance(
            observation, Vector2.Zero, 1f, MathF.PI, MathF.PI);

        Assert.True(result.TargetSelected);
        Assert.Equal(ContactRecoveryAction.ContinueCurrentTarget, result.RecoveryAction);
        Assert.Equal(target, result.CurrentTarget);
        Assert.Equal(target, controller.CurrentTarget);
        Assert.NotNull(result.KinematicStep);
        Assert.Equal(target, result.KinematicStep.Value.RequestedSolution.Target);
        Assert.Null(selector.LastAction);
    }

    [Fact]
    public void SlippingContactUsesDomainSelectedTargetBeforeAdvancing()
    {
        var (kinematics, _, _) = CreateKinematics();
        var currentTarget = new Vector2(1f, 1f);
        var replacementTarget = new Vector2(0f, 2f);
        var selector = new RecordingSelector { Candidate = replacementTarget };
        var controller = new StatefulPlanarIkRecoveryController(kinematics, selector, currentTarget);
        var observation = new ContactObservation(
            ContactPresence.Contact,
            ContactLoadOutcome.Slipping,
            contactId: "hold-1");

        var result = controller.Advance(
            observation, Vector2.Zero, 0.1f, 0.1f, 0.1f);

        Assert.True(result.TargetSelected);
        Assert.Equal(ContactRecoveryAction.SeekAlternativeContact, result.RecoveryAction);
        Assert.Equal(replacementTarget, result.CurrentTarget);
        Assert.Equal(replacementTarget, controller.CurrentTarget);
        Assert.Equal(replacementTarget, result.KinematicStep!.Value.RequestedSolution.Target);
        Assert.Equal(ContactRecoveryAction.SeekAlternativeContact, selector.LastAction);
        Assert.Same(observation, selector.LastObservation);
        Assert.Equal(currentTarget, selector.LastCurrentTarget);
    }

    [Fact]
    public void FailedTargetSelectionDoesNotAdvanceOrReplaceCurrentTarget()
    {
        var (kinematics, shoulder, elbow) = CreateKinematics();
        var currentTarget = new Vector2(1f, 1f);
        var selector = new RecordingSelector { CanSelect = false };
        var controller = new StatefulPlanarIkRecoveryController(kinematics, selector, currentTarget);
        var observation = new ContactObservation(ContactPresence.Contact, ContactLoadOutcome.Unsupported);

        var result = controller.Advance(
            observation, Vector2.Zero, 1f, MathF.PI, MathF.PI);

        Assert.False(result.TargetSelected);
        Assert.Equal(ContactRecoveryAction.RepositionToReduceLoad, result.RecoveryAction);
        Assert.Null(result.KinematicStep);
        Assert.Equal(currentTarget, result.CurrentTarget);
        Assert.Equal(currentTarget, controller.CurrentTarget);
        Assert.Equal(0f, shoulder.Hinge.AngleRadians);
        Assert.Equal(0f, elbow.Hinge.AngleRadians);
    }

    [Fact]
    public void RejectsInvalidConstructionInputs()
    {
        var (kinematics, _, _) = CreateKinematics();
        var selector = new RecordingSelector();

        Assert.Throws<ArgumentNullException>(() =>
            new StatefulPlanarIkRecoveryController(null!, selector, Vector2.Zero));
        Assert.Throws<ArgumentNullException>(() =>
            new StatefulPlanarIkRecoveryController(kinematics, null!, Vector2.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new StatefulPlanarIkRecoveryController(kinematics, selector, new Vector2(float.NaN, 0f)));
    }

    private static (StatefulPlanarIkController Controller, StatefulJointNode Shoulder, StatefulJointNode Elbow)
        CreateKinematics()
    {
        var shoulder = CreateNode("shoulder");
        var elbow = CreateNode("elbow");
        shoulder.AddChild(elbow);
        return (new StatefulPlanarIkController(shoulder, elbow, FullTurn, FullTurn), shoulder, elbow);
    }

    private static StatefulJointNode CreateNode(string name) =>
        new(name, 1f, new StatefulHinge(0f, -MathF.PI, MathF.PI));

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
