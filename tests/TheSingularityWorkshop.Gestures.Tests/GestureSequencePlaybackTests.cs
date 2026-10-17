using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureSequencePlaybackTests
{
    [Fact]
    public void CarriesUnusedDeltaAcrossStepBoundaries()
    {
        var first = CreatePlayback("first", 0f, 10f);
        var second = CreatePlayback("second", 10f, 20f);
        var sequence = new GestureSequencePlayback(new[] { first, second });

        sequence.Start();
        var pose = sequence.Advance(0.4d);
        Assert.Equal(0.4d, pose.NormalizedProgress, 6);
        Assert.Equal(0, sequence.CurrentStepIndex);

        pose = sequence.Advance(0.8d);

        Assert.Equal(1, sequence.CurrentStepIndex);
        Assert.Equal(GesturePlaybackState.Playing, sequence.State);
        Assert.Equal(0.2d, pose.NormalizedProgress, 6);
        Assert.Equal(0.2d, second.ElapsedSeconds, 6);
        Assert.Equal(1d, first.ElapsedSeconds, 6);
    }

    [Fact]
    public void OneLargeDeltaCanCompleteSeveralSteps()
    {
        var sequence = new GestureSequencePlayback(new[]
        {
            CreatePlayback("first", 0f, 1f),
            CreatePlayback("second", 1f, 2f),
            CreatePlayback("third", 2f, 3f)
        });

        sequence.Start();
        var final = sequence.Advance(3.5d);

        Assert.Equal(GesturePlaybackState.Completed, sequence.State);
        Assert.Equal(2, sequence.CurrentStepIndex);
        Assert.Equal(1d, final.NormalizedProgress);
        Assert.Equal(1d, sequence.CurrentPose!.NormalizedProgress);
    }

    [Fact]
    public void PauseAndResumePreserveActiveStepAndElapsedTime()
    {
        var first = CreatePlayback("first", 0f, 1f);
        var second = CreatePlayback("second", 1f, 2f);
        var sequence = new GestureSequencePlayback(new[] { first, second });
        sequence.Start();
        sequence.Advance(1.25d);

        sequence.Pause();

        Assert.Equal(GesturePlaybackState.Paused, sequence.State);
        Assert.Equal(1, sequence.CurrentStepIndex);
        Assert.Equal(0.25d, second.ElapsedSeconds, 6);
        Assert.Throws<InvalidOperationException>(() => sequence.Advance(0.1d));

        sequence.Resume();
        var pose = sequence.Advance(0.25d);

        Assert.Equal(GesturePlaybackState.Completed, sequence.State);
        Assert.True(pose.IsComplete);
    }

    [Fact]
    public void CancellingSequenceCancelsOnlyTheActiveStep()
    {
        var first = CreatePlayback("first", 0f, 1f);
        var second = CreatePlayback("second", 1f, 2f);
        var sequence = new GestureSequencePlayback(new[] { first, second });
        sequence.Start();
        sequence.Advance(0.2d);

        sequence.Cancel();

        Assert.Equal(GesturePlaybackState.Cancelled, sequence.State);
        Assert.Equal(GesturePlaybackState.Cancelled, first.State);
        Assert.Equal(GesturePlaybackState.Ready, second.State);
        Assert.Throws<InvalidOperationException>(() => sequence.Resume());
    }

    [Fact]
    public void RejectsEmptyOrReusedPlaybackSessions()
    {
        Assert.Throws<ArgumentException>(() => new GestureSequencePlayback(Array.Empty<GesturePlayback>()));
        var playback = CreatePlayback("only", 0f, 1f);
        Assert.Throws<ArgumentException>(() => new GestureSequencePlayback(new[] { playback, playback }));

        playback.Start();
        Assert.Throws<ArgumentException>(() => new GestureSequencePlayback(new[] { playback }));
    }

    [Fact]
    public void SeededSpeedVariationTransfersTimeAtItsActualCompletionBoundary()
    {
        var first = new GesturePlayback(
            CreateGesture("first", 0f, 1f),
            1d,
            new GestureVariation { MaximumSpeedDeviation = 0.2d },
            0UL);
        var second = CreatePlayback("second", 1f, 2f);
        var sequence = new GestureSequencePlayback(new[] { first, second });
        sequence.Start();

        var firstCompletion = first.CompletionTimeSeconds;
        var pose = sequence.Advance(firstCompletion + 0.1d);

        Assert.Equal(1, sequence.CurrentStepIndex);
        Assert.Equal(0.1d, pose.NormalizedProgress, 5);
        Assert.Equal(0.1d, second.ElapsedSeconds, 5);
    }

    private static GesturePlayback CreatePlayback(string id, float start, float end) =>
        new(CreateGesture(id, start, end), 1d);

    private static GestureDefinition CreateGesture(string id, float start, float end) => new(id, new[]
    {
        new GestureKeyframe(0d, new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(start, 0f, 0f), Quaternion.Identity, Vector3.One)
        }),
        new GestureKeyframe(1d, new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(end, 0f, 0f), Quaternion.Identity, Vector3.One)
        })
    });
}
