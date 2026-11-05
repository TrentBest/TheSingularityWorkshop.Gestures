namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Immutable progression for tightening individual marching variation at completed-step boundaries.
/// This describes gesture intent only; it does not coordinate agents or simulate physical motion.
/// </summary>
public sealed record MarchingCohesionProgression
{
    /// <summary>Creates a progression whose variation tightens after every second completed march step.</summary>
    /// <param name="initialVariation">The initial wide-range per-agent variation.</param>
    /// <param name="minimumVariation">The non-zero variation floor retained by every agent.</param>
    /// <param name="tighteningFactor">Multiplier applied to each variation bound at a tightening boundary, in (0, 1).</param>
    public MarchingCohesionProgression(
        GestureVariation initialVariation,
        GestureVariation minimumVariation,
        double tighteningFactor = 0.8d)
    {
        ArgumentNullException.ThrowIfNull(initialVariation);
        ArgumentNullException.ThrowIfNull(minimumVariation);
        initialVariation.Validate();
        minimumVariation.Validate();
        if (initialVariation.MaximumSpeedDeviation <= 0d ||
            initialVariation.MaximumLateralOffset <= 0f)
            throw new ArgumentException("Initial marching variation must be non-zero in both channels.", nameof(initialVariation));
        if (minimumVariation.MaximumSpeedDeviation <= 0d ||
            minimumVariation.MaximumLateralOffset <= 0f)
            throw new ArgumentException("Minimum marching variation must remain non-zero in both channels.", nameof(minimumVariation));
        if (minimumVariation.MaximumSpeedDeviation > initialVariation.MaximumSpeedDeviation ||
            minimumVariation.MaximumLateralOffset > initialVariation.MaximumLateralOffset)
            throw new ArgumentException("Minimum variation cannot exceed initial variation.", nameof(minimumVariation));
        if (!double.IsFinite(tighteningFactor) || tighteningFactor <= 0d || tighteningFactor >= 1d)
            throw new ArgumentOutOfRangeException(nameof(tighteningFactor), "Tightening factor must be in (0, 1).");

        InitialVariation = initialVariation;
        MinimumVariation = minimumVariation;
        TighteningFactor = tighteningFactor;
    }

    public GestureVariation InitialVariation { get; }
    public GestureVariation MinimumVariation { get; }
    public double TighteningFactor { get; }

    /// <summary>Creates the initial state before any march step has completed.</summary>
    public MarchingCohesionState Start() =>
        new(this, 0, InitialVariation);

    internal GestureVariation Tighten(GestureVariation current) =>
        new()
        {
            MaximumSpeedDeviation = Math.Max(
                MinimumVariation.MaximumSpeedDeviation,
                current.MaximumSpeedDeviation * TighteningFactor),
            MaximumLateralOffset = Math.Max(
                MinimumVariation.MaximumLateralOffset,
                current.MaximumLateralOffset * (float)TighteningFactor)
        };
}

/// <summary>
/// Immutable state advanced only by explicitly reporting a completed march step.
/// Partial-step evaluation must keep using the same state and variation.
/// </summary>
public sealed record MarchingCohesionState
{
    internal MarchingCohesionState(
        MarchingCohesionProgression progression,
        int completedMarchSteps,
        GestureVariation currentVariation)
    {
        Progression = progression;
        CompletedMarchSteps = completedMarchSteps;
        CurrentVariation = currentVariation;
    }

    public MarchingCohesionProgression Progression { get; }
    public int CompletedMarchSteps { get; }
    public GestureVariation CurrentVariation { get; }

    /// <summary>
    /// Reports one completed march step. Variation tightens after steps 2, 4, 6, and so on;
    /// no intermediate pose evaluation can change the current variation.
    /// </summary>
    public MarchingCohesionState CompleteMarchStep()
    {
        var completed = checked(CompletedMarchSteps + 1);
        var variation = completed % 2 == 0
            ? Progression.Tighten(CurrentVariation)
            : CurrentVariation;
        return new MarchingCohesionState(Progression, completed, variation);
    }
}
