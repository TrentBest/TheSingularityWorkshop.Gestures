namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Runs a fixed, ordered set of single-gesture sessions and transfers unused host time across step boundaries.
/// This coordinates pose procedures only; it does not resolve prerequisites, blend poses, or assert physical success.
/// </summary>
public sealed class GestureSequencePlayback
{
    private readonly IReadOnlyList<GesturePlayback> _steps;
    private int _currentStepIndex;

    /// <summary>Creates a sequence from distinct, ready-to-start playback sessions.</summary>
    public GestureSequencePlayback(IEnumerable<GesturePlayback> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        var snapshot = steps.ToArray();
        if (snapshot.Length == 0)
            throw new ArgumentException("A sequence requires at least one playback step.", nameof(steps));
        if (snapshot.Any(step => step is null))
            throw new ArgumentException("Sequence steps must not contain null.", nameof(steps));
        if (snapshot.Distinct(ReferenceEqualityComparer.Instance).Count() != snapshot.Length)
            throw new ArgumentException("Each sequence step must be a distinct playback session.", nameof(steps));
        if (snapshot.Any(step => step.State != GesturePlaybackState.Ready))
            throw new ArgumentException("Every sequence step must be ready and unused.", nameof(steps));

        _steps = Array.AsReadOnly(snapshot);
    }

    /// <summary>Sequence lifecycle state.</summary>
    public GesturePlaybackState State { get; private set; } = GesturePlaybackState.Ready;

    /// <summary>Zero-based index of the current step; remains on the final step after completion.</summary>
    public int CurrentStepIndex => _currentStepIndex;

    /// <summary>Number of playback steps in this sequence.</summary>
    public int StepCount => _steps.Count;

    /// <summary>Current step's latest pose, or null before the sequence starts.</summary>
    public EvaluatedPose? CurrentPose => State == GesturePlaybackState.Ready
        ? null
        : _steps[_currentStepIndex].CurrentPose;

    /// <summary>Starts the first step and returns its initial pose.</summary>
    public EvaluatedPose Start()
    {
        RequireState(GesturePlaybackState.Ready, "Only a ready sequence can start.");
        State = GesturePlaybackState.Playing;
        return _steps[_currentStepIndex].Start();
    }

    /// <summary>
    /// Advances the sequence by host-supplied seconds. Any time left after a step completes is
    /// applied to subsequent steps in order, including multiple boundaries within one call.
    /// </summary>
    public EvaluatedPose Advance(double deltaSeconds)
    {
        RequireState(GesturePlaybackState.Playing, "Only a playing sequence can advance.");
        if (!double.IsFinite(deltaSeconds) || deltaSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be finite and non-negative.");

        var remaining = deltaSeconds;
        while (true)
        {
            var step = _steps[_currentStepIndex];
            var untilCompletion = Math.Max(0d, step.CompletionTimeSeconds - step.ElapsedSeconds);
            if (remaining < untilCompletion)
                return step.Advance(remaining);

            step.Advance(untilCompletion);
            remaining -= untilCompletion;
            if (step.State != GesturePlaybackState.Completed)
                return step.CurrentPose!;

            if (_currentStepIndex == _steps.Count - 1)
            {
                State = GesturePlaybackState.Completed;
                return step.CurrentPose!;
            }

            _currentStepIndex++;
            var nextPose = _steps[_currentStepIndex].Start();
            if (remaining <= 0d)
                return nextPose;
        }
    }

    /// <summary>Pauses the active step without discarding sequence position or elapsed time.</summary>
    public void Pause()
    {
        RequireState(GesturePlaybackState.Playing, "Only a playing sequence can pause.");
        _steps[_currentStepIndex].Pause();
        State = GesturePlaybackState.Paused;
    }

    /// <summary>Resumes the active step from its retained elapsed time.</summary>
    public void Resume()
    {
        RequireState(GesturePlaybackState.Paused, "Only a paused sequence can resume.");
        _steps[_currentStepIndex].Resume();
        State = GesturePlaybackState.Playing;
    }

    /// <summary>Cancels the sequence and its active step. Cancellation is terminal.</summary>
    public void Cancel()
    {
        if (State is not (GesturePlaybackState.Playing or GesturePlaybackState.Paused))
            throw new InvalidOperationException("Only a playing or paused sequence can be cancelled.");
        _steps[_currentStepIndex].Cancel();
        State = GesturePlaybackState.Cancelled;
    }

    private void RequireState(GesturePlaybackState expected, string message)
    {
        if (State != expected)
            throw new InvalidOperationException(message);
    }
}
