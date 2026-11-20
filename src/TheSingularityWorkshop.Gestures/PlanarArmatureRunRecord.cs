using System.Collections.ObjectModel;
using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>Versioned, serializable-by-consumer record of a deterministic planar armature evaluation.</summary>
public sealed class PlanarArmatureRunRecord
{
    public const int CurrentSchemaVersion = 1;

    private PlanarArmatureRunRecord(
        PlanarArmatureDefinition definition,
        string scenarioId,
        string episodeId,
        ulong seed,
        float deltaTimeSeconds,
        Vector2 worldTarget,
        float targetTolerance,
        PlanarArmatureObservation initialObservation,
        IReadOnlyList<PlanarArmatureRunStep> steps)
    {
        Definition = definition;
        ScenarioId = scenarioId;
        EpisodeId = episodeId;
        Seed = seed;
        DeltaTimeSeconds = deltaTimeSeconds;
        WorldTarget = worldTarget;
        TargetTolerance = targetTolerance;
        InitialObservation = initialObservation;
        Steps = steps;
    }

    public int SchemaVersion => CurrentSchemaVersion;
    public PlanarArmatureDefinition Definition { get; }
    public string ScenarioId { get; }
    public string EpisodeId { get; }
    public ulong Seed { get; }
    public float DeltaTimeSeconds { get; }
    public Vector2 WorldTarget { get; }
    public float TargetTolerance { get; }
    public PlanarArmatureObservation InitialObservation { get; }
    public IReadOnlyList<PlanarArmatureRunStep> Steps { get; }
    public PlanarArmatureObservation FinalObservation => Steps.Count == 0 ? InitialObservation : Steps[^1].Observation;
    public float FinalTargetError => Vector2.Distance(FinalObservation.AchievedEndEffectorPosition, WorldTarget);
    public bool TargetReached => FinalTargetError <= TargetTolerance;
    public PlanarArmatureRunMetrics Metrics => CalculateMetrics();

    private PlanarArmatureRunMetrics CalculateMetrics()
    {
        var initialError = Vector2.Distance(InitialObservation.AchievedEndEffectorPosition, WorldTarget);
        var finalError = FinalTargetError;
        var maximumError = initialError;
        var meanError = initialError;
        int? stepsToTarget = initialError <= TargetTolerance ? 0 : null;

        for (var i = 0; i < Steps.Count; i++)
        {
            var error = Steps[i].TargetError;
            maximumError = MathF.Max(maximumError, error);
            meanError += (error - meanError) / (i + 2);
            if (stepsToTarget is null && error <= TargetTolerance)
                stepsToTarget = i + 1;
        }

        return new PlanarArmatureRunMetrics(
            Steps.Count, initialError, finalError, maximumError, meanError, stepsToTarget);
    }

    /// <summary>Begins a run record from the exact reset observation used by the evaluator.</summary>
    public static PlanarArmatureRunRecord Start(
        PlanarArmatureDefinition definition,
        PlanarArmatureObservation initialObservation,
        Vector2 worldTarget,
        float deltaTimeSeconds,
        float targetTolerance)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (initialObservation.ContractVersion != StatefulPlanarArmature.CurrentContractVersion
            || string.IsNullOrWhiteSpace(initialObservation.ScenarioId)
            || string.IsNullOrWhiteSpace(initialObservation.EpisodeId)
            || initialObservation.StepIndex != 0
            || !StringComparer.Ordinal.Equals(initialObservation.ScenarioId, definition.ScenarioId)
            || !StringComparer.Ordinal.Equals(initialObservation.ShoulderJointId, definition.ShoulderJointId)
            || !StringComparer.Ordinal.Equals(initialObservation.ElbowJointId, definition.ElbowJointId)
            || !StringComparer.Ordinal.Equals(initialObservation.EndEffectorId, definition.EndEffectorId)
            || initialObservation.RootAnchor != definition.RootAnchor
            || initialObservation.ShoulderLimits != definition.ShoulderLimits
            || initialObservation.ElbowLimits != definition.ElbowLimits
            || initialObservation.ShoulderAngleRadians != definition.InitialShoulderAngleRadians
            || initialObservation.ElbowAngleRadians != definition.InitialElbowAngleRadians)
            throw new ArgumentException("A run must start from a valid step-zero armature observation.", nameof(initialObservation));
        Finite(worldTarget, nameof(worldTarget));
        if (!float.IsFinite(Vector2.Distance(initialObservation.AchievedEndEffectorPosition, worldTarget)))
            throw new ArgumentOutOfRangeException(nameof(worldTarget), "Initial target error must be finite.");
        if (!float.IsFinite(deltaTimeSeconds) || deltaTimeSeconds <= 0f)
            throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
        if (!float.IsFinite(targetTolerance) || targetTolerance < 0f)
            throw new ArgumentOutOfRangeException(nameof(targetTolerance));

        return new PlanarArmatureRunRecord(
            definition,
            initialObservation.ScenarioId,
            initialObservation.EpisodeId,
            initialObservation.Seed,
            deltaTimeSeconds,
            worldTarget,
            targetTolerance,
            initialObservation,
            Array.AsReadOnly(Array.Empty<PlanarArmatureRunStep>()));
    }

    /// <summary>Returns a new record with one validated step; the original record remains unchanged.</summary>
    public PlanarArmatureRunRecord Append(PlanarArmatureStepResult result)
    {
        var expectedStep = InitialObservation.StepIndex + Steps.Count + 1;
        var observation = result.Observation;
        if (result.RequestedAction.ContractVersion != StatefulPlanarArmature.CurrentContractVersion
            || result.RequestedAction.Mode != PlanarArmatureActionMode.JointAngleTargets
            || !StringComparer.Ordinal.Equals(result.RequestedAction.EpisodeId, EpisodeId)
            || !StringComparer.Ordinal.Equals(observation.EpisodeId, EpisodeId)
            || !StringComparer.Ordinal.Equals(observation.ScenarioId, ScenarioId)
            || observation.ContractVersion != StatefulPlanarArmature.CurrentContractVersion
            || observation.StepIndex != expectedStep
            || result.RequestedAction.ExpectedStepIndex != expectedStep - 1)
            throw new ArgumentException("Step result does not continue this run's episode and sequence.", nameof(result));
        Finite(observation.AchievedEndEffectorPosition, nameof(result));
        if (!float.IsFinite(result.AcceptedShoulderTargetRadians)
            || !float.IsFinite(result.AcceptedElbowTargetRadians))
            throw new ArgumentOutOfRangeException(nameof(result), "Accepted joint targets must be finite.");
        var targetError = Vector2.Distance(observation.AchievedEndEffectorPosition, WorldTarget);
        if (!float.IsFinite(targetError))
            throw new ArgumentOutOfRangeException(nameof(result), "Measured target error must be finite.");

        var step = new PlanarArmatureRunStep(
            result.RequestedAction,
            result.AcceptedShoulderTargetRadians,
            result.AcceptedElbowTargetRadians,
            observation,
            targetError);
        var next = new PlanarArmatureRunStep[Steps.Count + 1];
        for (var i = 0; i < Steps.Count; i++)
            next[i] = Steps[i];
        next[^1] = step;

        return new PlanarArmatureRunRecord(
            Definition, ScenarioId, EpisodeId, Seed, DeltaTimeSeconds, WorldTarget, TargetTolerance,
            InitialObservation, new ReadOnlyCollection<PlanarArmatureRunStep>(next));
    }

    private static void Finite(Vector2 value, string name)
    {
        if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
            throw new ArgumentOutOfRangeException(name, "Coordinates must be finite.");
    }
}

/// <summary>One requested action and the corresponding accepted command and achieved observation.</summary>
/// <summary>Measured task metrics; no velocity-derived smoothness or physical overshoot is inferred.</summary>
public readonly record struct PlanarArmatureRunMetrics(
    int StepsExecuted,
    float InitialTargetError,
    float FinalTargetError,
    float MaximumTargetError,
    float MeanTargetError,
    int? StepsToTarget);

public sealed record PlanarArmatureRunStep(
    PlanarArmatureAction RequestedAction,
    float AcceptedShoulderTargetRadians,
    float AcceptedElbowTargetRadians,
    PlanarArmatureObservation Observation,
    float TargetError);
