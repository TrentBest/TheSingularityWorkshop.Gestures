namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// One participant's unwrapped phase supplied by a host timing source.
/// Gestures measures coordination; it does not own or advance the oscillator.
/// </summary>
public readonly record struct GesturePhaseObservation
{
    public GesturePhaseObservation(string participantId, double phaseRadians)
    {
        if (string.IsNullOrWhiteSpace(participantId))
            throw new ArgumentException("A stable participant ID is required.", nameof(participantId));
        if (!double.IsFinite(phaseRadians))
            throw new ArgumentOutOfRangeException(nameof(phaseRadians), "Phase must be finite.");

        ParticipantId = participantId;
        PhaseRadians = phaseRadians;
    }

    public string ParticipantId { get; }
    public double PhaseRadians { get; }
}

/// <summary>A participant phase normalized for a synchronization measurement.</summary>
public readonly record struct GestureParticipantPhase(
    string ParticipantId,
    double PhaseRadians,
    double WrappedPhaseRadians);

/// <summary>
/// Phase-coherence measurement for one observation instant.
/// The order parameter measures concentration, not coupling or physical entrainment.
/// </summary>
public sealed record GesturePhaseSynchronizationResult(
    IReadOnlyList<GestureParticipantPhase> Participants,
    double OrderParameterMagnitude,
    double MeanPhaseRadians,
    bool HasDefinedMeanPhase);

/// <summary>
/// Measures phase concentration from host-supplied observations using the complex order parameter.
/// This is an observation-only choreography metric: it does not create, couple, or advance clocks.
/// </summary>
public static class GesturePhaseSynchronization
{
    /// <summary>
    /// Evaluates one population snapshot. Participants are sorted by ordinal stable ID so
    /// equivalent input sets produce the same output ordering regardless of enumeration order.
    /// </summary>
    /// <param name="observations">Unwrapped phase observations from a host timing source.</param>
    /// <param name="maximumParticipants">A positive bound on the accepted population size.</param>
    public static GesturePhaseSynchronizationResult Evaluate(
        IEnumerable<GesturePhaseObservation> observations,
        int maximumParticipants = 100_000)
    {
        ArgumentNullException.ThrowIfNull(observations);
        if (maximumParticipants <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumParticipants));

        var ordered = observations
            .OrderBy(observation => observation.ParticipantId, StringComparer.Ordinal)
            .ToArray();

        if (ordered.Length == 0)
            throw new ArgumentException("At least one participant phase is required.", nameof(observations));
        if (ordered.Length > maximumParticipants)
            throw new InvalidOperationException(
                $"The population exceeds its configured participant budget of {maximumParticipants}.");

        var participants = new GestureParticipantPhase[ordered.Length];
        var cosineSum = 0d;
        var sineSum = 0d;

        for (var index = 0; index < ordered.Length; index++)
        {
            var observation = ordered[index];
            if (string.IsNullOrWhiteSpace(observation.ParticipantId))
                throw new ArgumentException("Every observation requires a stable participant ID.", nameof(observations));
            if (!double.IsFinite(observation.PhaseRadians))
                throw new ArgumentOutOfRangeException(nameof(observations), "Every phase must be finite.");
            if (index > 0 && string.Equals(
                ordered[index - 1].ParticipantId,
                observation.ParticipantId,
                StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Participant IDs must be unique using ordinal comparison.", nameof(observations));
            }

            var wrapped = WrapPositive(observation.PhaseRadians);
            participants[index] = new GestureParticipantPhase(
                observation.ParticipantId,
                observation.PhaseRadians,
                wrapped);
            cosineSum += Math.Cos(wrapped);
            sineSum += Math.Sin(wrapped);
        }

        var meanCosine = cosineSum / ordered.Length;
        var meanSine = sineSum / ordered.Length;
        var magnitude = Math.Sqrt((meanCosine * meanCosine) + (meanSine * meanSine));
        if (!double.IsFinite(magnitude))
            throw new OverflowException("The phase-coherence measurement is not finite.");

        var hasDefinedMeanPhase = magnitude > 1e-15;
        var meanPhase = hasDefinedMeanPhase ? Math.Atan2(meanSine, meanCosine) : 0d;
        if (!double.IsFinite(meanPhase))
            throw new OverflowException("The mean phase is not finite.");

        return new GesturePhaseSynchronizationResult(
            Array.AsReadOnly(participants),
            Math.Clamp(magnitude, 0d, 1d),
            meanPhase,
            hasDefinedMeanPhase);
    }

    /// <summary>Wraps finite phase into [0, 2π).</summary>
    public static double WrapPositive(double phaseRadians)
    {
        if (!double.IsFinite(phaseRadians))
            throw new ArgumentOutOfRangeException(nameof(phaseRadians), "Phase must be finite.");

        var twoPi = 2d * Math.PI;
        var wrapped = phaseRadians % twoPi;
        if (wrapped < 0d)
            wrapped += twoPi;

        // Canonicalize whole-cycle values that round a few ULPs below 2π.
        var boundaryTolerance = Math.Min(
            1e-12,
            16d * 2.2204460492503131e-16 * Math.Max(1d, Math.Abs(phaseRadians)));
        return twoPi - wrapped <= boundaryTolerance ? 0d : wrapped;
    }
}
