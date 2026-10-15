using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// A small executable proof that the existing CPU Gesture evaluator can describe
/// performer and audience procedures without embedding concert rules or audio state.
/// This is a data/evaluator fixture, not a live concert or sound simulation.
/// </summary>
public sealed class ConcertPerformanceGestureTests
{
    [Fact]
    public void LeadVocalProcedureHasProviderDefinedMicrophoneAndPerformerAnchors()
    {
        var gesture = CreateLeadVocalPhrase();

        Assert.Equal("performance.lead-vocal-phrase", gesture.Id);
        Assert.Contains(gesture.Anchors, anchor => anchor.Name == "microphone-mouth");
        Assert.Equal("head", gesture.Anchors.Single(anchor => anchor.Name == "microphone-mouth").ParentJoint);
        Assert.Contains(gesture.Anchors, anchor => anchor.Name == "microphone-grip");
        Assert.Equal("left_hand", gesture.Anchors.Single(anchor => anchor.Name == "microphone-grip").ParentJoint);

        var pose = GestureEvaluator.Evaluate(gesture, elapsedSeconds: 1d, durationSeconds: 2d);

        Assert.Equal(0.5d, pose.NormalizedProgress);
        Assert.False(pose.IsComplete);
        Assert.Contains("head", pose.LocalTransforms.Keys);
        Assert.Contains("left_hand", pose.LocalTransforms.Keys);
        Assert.Contains("torso", pose.LocalTransforms.Keys);
    }

    [Fact]
    public void AudienceClapIsAnEvaluatedProcedureNotAnAudioOrCrowdOutcome()
    {
        var gesture = CreateAudienceClap();

        var early = GestureEvaluator.Evaluate(gesture, elapsedSeconds: 0.25d, durationSeconds: 1d);
        var late = GestureEvaluator.Evaluate(gesture, elapsedSeconds: 0.75d, durationSeconds: 1d);

        Assert.Equal(0.25d, early.NormalizedProgress);
        Assert.Equal(0.75d, late.NormalizedProgress);
        Assert.NotEqual(early.LocalTransforms["left_hand"].Position, late.LocalTransforms["left_hand"].Position);

        // The result describes pose progression only. It contains no claim that
        // a sound was emitted, the hands made contact, or the clap matched a beat.
        Assert.False(early.IsComplete);
        Assert.False(late.IsComplete);
    }

    [Fact]
    public void SharedProcedureKeepsParticipantProgressAndVariationIndependent()
    {
        var gesture = CreateAudienceClap();
        var variation = new GestureVariation
        {
            MaximumSpeedDeviation = 0.12d,
            MaximumLateralOffset = 0.04f
        };

        var participantA = GestureEvaluator.Evaluate(gesture, 0.4d, 1d, variation, identitySeed: 101UL);
        var participantAReplay = GestureEvaluator.Evaluate(gesture, 0.4d, 1d, variation, identitySeed: 101UL);
        var participantB = GestureEvaluator.Evaluate(gesture, 0.7d, 1d, variation, identitySeed: 202UL);

        Assert.Equal(participantA.NormalizedProgress, participantAReplay.NormalizedProgress);
        Assert.Equal(participantA.RootOffset, participantAReplay.RootOffset);
        Assert.Equal(participantA.LocalTransforms["left_hand"], participantAReplay.LocalTransforms["left_hand"]);

        Assert.NotEqual(participantA.NormalizedProgress, participantB.NormalizedProgress);
        Assert.InRange(Math.Abs(participantA.RootOffset.X), 0f, variation.MaximumLateralOffset);
        Assert.InRange(Math.Abs(participantB.RootOffset.X), 0f, variation.MaximumLateralOffset);
        Assert.InRange(participantA.NormalizedProgress, 0d, 1d);
        Assert.InRange(participantB.NormalizedProgress, 0d, 1d);
    }

    [Fact]
    public void GestureCompletionMeansPoseTimelineCompletionNotShowCompletion()
    {
        var gesture = CreateLeadVocalPhrase();

        var pose = GestureEvaluator.Evaluate(gesture, elapsedSeconds: 5d, durationSeconds: 2d);

        Assert.True(pose.IsComplete);
        Assert.Equal(1d, pose.NormalizedProgress);

        // Event lifecycle, vocal/audio output, and performance success are
        // deliberately not represented by EvaluatedPose.
        Assert.DoesNotContain("show-complete", pose.LocalTransforms.Keys);
        Assert.DoesNotContain("audio-success", pose.LocalTransforms.Keys);
    }

    private static GestureDefinition CreateLeadVocalPhrase()
    {
        return new GestureDefinition(
            "performance.lead-vocal-phrase",
            [
                Frame(0d,
                    ("torso", At(0f, 0f, 0f)),
                    ("head", At(0f, 1.55f, 0.08f)),
                    ("left_hand", At(-0.22f, 1.1f, 0.18f)),
                    ("right_hand", At(0.24f, 1.0f, 0.16f))),
                Frame(0.5d,
                    ("torso", At(0f, 0.02f, 0.04f)),
                    ("head", At(0f, 1.54f, 0.16f)),
                    ("left_hand", At(-0.18f, 1.2f, 0.28f)),
                    ("right_hand", At(0.32f, 1.12f, 0.2f))),
                Frame(1d,
                    ("torso", At(0f, 0f, 0f)),
                    ("head", At(0f, 1.55f, 0.08f)),
                    ("left_hand", At(-0.22f, 1.1f, 0.18f)),
                    ("right_hand", At(0.24f, 1.0f, 0.16f)))
            ],
            [
                new GestureAnchor("microphone-mouth", "head", At(0f, 0f, 0.14f)),
                new GestureAnchor("microphone-grip", "left_hand", At(0f, 0f, 0f)),
                new GestureAnchor("audience-focus", "head", At(0f, 0f, 1f))
            ]);
    }

    private static GestureDefinition CreateAudienceClap()
    {
        return new GestureDefinition(
            "audience.clap",
            [
                Frame(0d,
                    ("torso", At(0f, 0f, 0f)),
                    ("left_hand", At(-0.32f, 1.05f, 0.08f)),
                    ("right_hand", At(0.32f, 1.05f, 0.08f))),
                Frame(0.5d,
                    ("torso", At(0f, 0f, 0f)),
                    ("left_hand", At(-0.04f, 1.12f, 0.2f)),
                    ("right_hand", At(0.04f, 1.12f, 0.2f))),
                Frame(1d,
                    ("torso", At(0f, 0f, 0f)),
                    ("left_hand", At(-0.32f, 1.05f, 0.08f)),
                    ("right_hand", At(0.32f, 1.05f, 0.08f)))
            ],
            [
                new GestureAnchor("left-palm", "left_hand", At(0f, 0f, 0f)),
                new GestureAnchor("right-palm", "right_hand", At(0f, 0f, 0f))
            ]);
    }

    private static GestureKeyframe Frame(
        double time,
        params (string Name, PoseTransform Transform)[] values) =>
        new(time, values.ToDictionary(value => value.Name, value => value.Transform, StringComparer.Ordinal));

    private static PoseTransform At(float x, float y, float z) =>
        new(new Vector3(x, y, z), Quaternion.Identity, Vector3.One);
}
