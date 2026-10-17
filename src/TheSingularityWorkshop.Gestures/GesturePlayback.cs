namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Owns the caller-side lifecycle of one Gesture playback while leaving pose sampling stateless.
/// This coordinator advances only when the host supplies elapsed delta time; it does not run a clock,
/// schedule work, blend procedures, resolve anchors, or infer physical outcomes.
/// </summary>
public sealed class GesturePlayback
{
    private readonly GestureDefinition _definition;
    private readonly double _durationSeconds;
    private readonly GestureVariation _variation;
    private readonly ulong _identitySeed;
    private double _elapsedSeconds;

    /// <summary>Creates a single-use playback session for a validated Gesture definition.</summary>
    public GesturePlayback(
        GestureDefinition definition,
        double durationSeconds,
        GestureVariation? variation = null,
        ulong identitySeed = 0)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (!double.IsFinite(durationSeconds) || durationSeconds <= 0d)
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be finite and positive.");

        _definition = definition;
        _durationSeconds = durationSeconds;
        _variation = variation ?? GestureVariation.None;
        _variation.Validate();
        _identitySeed = identitySeed;
    }

    /// <summary>Current lifecycle state. A session can be started once and then paused, resumed, completed, or cancelled.</summary>
    public GesturePlaybackState State { get; private set; } = GesturePlaybackState.Ready;

    /// <summary>Unscaled elapsed seconds accumulated from host-supplied deltas.</summary>
    public double ElapsedSeconds => _elapsedSeconds;

    /// <summary>Most recently evaluated pose, or null before Start.</summary>
    public EvaluatedPose? CurrentPose { get; private set; }

    /// <summary>Samples the initial pose and transitions Ready to Playing.</summary>
    public EvaluatedPose Start()
    {
        RequireState(GesturePlaybackState.Ready, "Only a ready playback can start.");
        var pose = EvaluateCurrentTime();
        CurrentPose = pose;
        State = pose.IsComplete ? GesturePlaybackState.Completed : GesturePlaybackState.Playing;
        return pose;
    }

    /// <summary>
    /// Advances the playback by a finite, non-negative host-supplied delta and returns the evaluated pose.
    /// A speed variation can make completion occur before or after the unmodified duration.
    /// </summary>
    public EvaluatedPose Advance(double deltaSeconds)
    {
        RequireState(GesturePlaybackState.Playing, "Only a playing playback can advance.");
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be finite and non-negative.");

        var nextElapsed = _elapsedSeconds + deltaSeconds;
        if (!double.IsFinite(nextElapsed))
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Accumulated elapsed time must remain finite.");

        _elapsedSeconds = nextElapsed;
        var pose = EvaluateCurrentTime();
        CurrentPose = pose;
        if (pose.IsComplete)
            State = GesturePlaybackState.Completed;
        return pose;
    }

    /// <summary>Pauses a playing session without discarding accumulated time or the current pose.</summary>
    public void Pause()
    {
        RequireState(GesturePlaybackState.Playing, "Only a playing playback can pause.");
        State = GesturePlaybackState.Paused;
    }

    /// <summary>Resumes a paused session from its preserved elapsed time.</summary>
    public void Resume()
    {
        RequireState(GesturePlaybackState.Paused, "Only a paused playback can resume.");
        State = GesturePlaybackState.Playing;
    }

    /// <summary>Cancels a playing or paused session. Cancellation is terminal; create a new session to restart.</summary>
    public void Cancel()
    {
        if (State is not (GesturePlaybackState.Playing or GesturePlaybackState.Paused))
            throw new InvalidOperationException("Only a playing or paused playback can be cancelled.");
        State = GesturePlaybackState.Cancelled;
    }

    private EvaluatedPose EvaluateCurrentTime() =>
        GestureEvaluator.Evaluate(_definition, _elapsedSeconds, _durationSeconds, _variation, _identitySeed);

    private void RequireState(GesturePlaybackState expected, string message)
    {
        if (State != expected)
            throw new InvalidOperationException(message);
    }
}

/// <summary>Lifecycle states for one single-use Gesture playback session.</summary>
public enum GesturePlaybackState
{
    /// <summary>Constructed but not started.</summary>
    Ready,
    /// <summary>Accepts host-supplied time deltas.</summary>
    Playing,
    /// <summary>Retains elapsed time and pose until resumed or cancelled.</summary>
    Paused,
    /// <summary>The evaluator reached its final normalized progress.</summary>
    Completed,
    /// <summary>The host cancelled the session before completion.</summary>
    Cancelled
}
