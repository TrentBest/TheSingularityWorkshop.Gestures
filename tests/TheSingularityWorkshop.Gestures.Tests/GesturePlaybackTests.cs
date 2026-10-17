using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GesturePlaybackTests
{
    [Fact]
    public void StartAndAdvanceDriveTheStatelessEvaluatorThroughCompletion()
    {
        var playback = CreatePlayback();

        var initial = playback.Start();
        Assert.Equal(GesturePlaybackState.Playing, playback.State);
        Assert.Equal(0d, initial.NormalizedProgress);
        Assert.Equal(0d, playback.ElapsedSeconds);

        var middle = playback.Advance(0.4d);
        Assert.Equal(GesturePlaybackState.Playing, playback.State);
        Assert.Equal(0.4d, middle.NormalizedProgress, 6);
        Assert.Equal(4f, middle.LocalTransforms["root"].Position.X, 5);

        var final = playback.Advance(0.6d);
        Assert.Equal(GesturePlaybackState.Completed, playback.State);
        Assert.True(final.IsComplete);
        Assert.Equal(10f, final.LocalTransforms["root"].Position.X);
        Assert.Equal(1d, final.NormalizedProgress);
    }

    [Fact]
    public void PauseAndResumePreserveElapsedTimeAndPose()
    {
        var playback = CreatePlayback();
        playback.Start();
        var beforePause = playback.Advance(0.3d);

        playback.Pause();

        Assert.Equal(GesturePlaybackState.Paused, playback.State);
        Assert.Equal(0.3d, playback.ElapsedSeconds, 6);
        Assert.Same(beforePause, playback.CurrentPose);
        Assert.Throws<InvalidOperationException>(() => playback.Advance(0.1d));

        playback.Resume();
        var afterResume = playback.Advance(0.2d);

        Assert.Equal(GesturePlaybackState.Playing, playback.State);
        Assert.Equal(0.5d, playback.ElapsedSeconds, 6);
        Assert.Equal(0.5d, afterResume.NormalizedProgress, 6);
    }

    [Fact]
    public void CancellationIsTerminalAndPreservesLastSample()
    {
        var playback = CreatePlayback();
        playback.Start();
        var lastPose = playback.Advance(0.2d);

        playback.Cancel();

        Assert.Equal(GesturePlaybackState.Cancelled, playback.State);
        Assert.Same(lastPose, playback.CurrentPose);
        Assert.Throws<InvalidOperationException>(() => playback.Advance(0.1d));
        Assert.Throws<InvalidOperationException>(() => playback.Resume());
        Assert.Throws<InvalidOperationException>(() => playback.Start());
    }

    [Fact]
    public void CompletedSessionCannotBeAdvancedOrRestarted()
    {
        var playback = CreatePlayback();
        playback.Start();
        playback.Advance(1d);

        Assert.Equal(GesturePlaybackState.Completed, playback.State);
        Assert.Throws<InvalidOperationException>(() => playback.Advance(0d));
        Assert.Throws<InvalidOperationException>(() => playback.Start());
        Assert.Throws<InvalidOperationException>(() => playback.Cancel());
    }

    [Fact]
    public void ZeroDeltaIsAllowedButNegativeOrNonFiniteDeltaIsRejected()
    {
        var playback = CreatePlayback();
        playback.Start();

        Assert.Equal(0d, playback.Advance(0d).NormalizedProgress);
        Assert.Throws<ArgumentOutOfRangeException>(() => playback.Advance(-0.1d));
        Assert.Throws<ArgumentOutOfRangeException>(() => playback.Advance(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => playback.Advance(double.PositiveInfinity));
    }

    [Fact]
    public void InvalidDurationAndVariationAreRejectedAtConstruction()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GesturePlayback(CreateGesture(), 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new GesturePlayback(
            CreateGesture(), 1d, new GestureVariation { MaximumSpeedDeviation = 1d }));
    }

    [Fact]
    public void SpeedVariationCanCompleteBeforeTheUnscaledDuration()
    {
        var variation = new GestureVariation { MaximumSpeedDeviation = 0.2d };
        var playback = new GesturePlayback(CreateGesture(), 1d, variation, 0UL);

        playback.Start();
        var final = playback.Advance(1d);

        Assert.True(final.IsComplete);
        Assert.Equal(GesturePlaybackState.Completed, playback.State);
    }

    private static GesturePlayback CreatePlayback() => new(CreateGesture(), 1d);

    private static GestureDefinition CreateGesture() => new("test.playback", new[]
    {
        new GestureKeyframe(0d, new Dictionary<string, PoseTransform>
        {
            ["root"] = PoseTransform.Identity
        }),
        new GestureKeyframe(1d, new Dictionary<string, PoseTransform>
        {
            ["root"] = new PoseTransform(new Vector3(10f, 0f, 0f), Quaternion.Identity, Vector3.One)
        })
    });
}
