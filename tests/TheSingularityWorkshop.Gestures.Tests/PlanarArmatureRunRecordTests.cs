using System.Numerics;
using System.Text.Json;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class PlanarArmatureRunRecordTests
{
    [Fact]
    public void RecordsDeterministicReplayAndSerializesItsEvidence()
    {
        var first = Run();
        var second = Run();

        Assert.Equal(PlanarArmatureRunRecord.CurrentSchemaVersion, first.SchemaVersion);
        Assert.Equal(first.ScenarioId, second.ScenarioId);
        Assert.Equal(first.EpisodeId, second.EpisodeId);
        Assert.Equal(first.Seed, second.Seed);
        Assert.Equal(first.Steps, second.Steps);
        Assert.Equal(first.FinalTargetError, second.FinalTargetError);
        Assert.Equal(3, first.Steps.Count);
        Assert.Equal(3, first.FinalObservation.StepIndex);
        Assert.Equal(3, first.Metrics.StepsExecuted);
        Assert.True(first.Metrics.InitialTargetError > 0f);
        Assert.True(first.Metrics.MaximumTargetError >= first.Metrics.InitialTargetError);
        Assert.True(float.IsFinite(first.Metrics.MeanTargetError));
        Assert.Null(first.Metrics.StepsToTarget);

        var json = JsonSerializer.Serialize(first);
        Assert.Contains("\"SchemaVersion\":1", json);
        Assert.Contains("\"RequestedAction\"", json);
        Assert.Contains("\"AcceptedShoulderTargetRadians\"", json);
        Assert.Contains("\"TargetError\"", json);
        Assert.Contains("\"ShoulderLinkLength\":1", json);
    }

    [Fact]
    public void AppendIsImmutableAndRejectsWrongEpisodeOrSequence()
    {
        var definition = Definition();
        var armature = definition.CreateArmature();
        var initial = armature.Reset("record-episode", 7);
        var record = PlanarArmatureRunRecord.Start(armature, initial, new Vector2(1f, 1f), 0.1f, 0.01f);
        var action = PlanarArmatureBaselinePolicy.CreateAction(definition, initial, new Vector2(1f, 1f));
        var result = armature.Advance(action, 0.1f);
        var updated = record.Append(result);

        Assert.Empty(record.Steps);
        Assert.Single(updated.Steps);
        Assert.Throws<ArgumentException>(() => updated.Append(result));
        Assert.Throws<ArgumentException>(() => record.Append(result with { DeltaTimeSeconds = 0.2f }));
    }

    [Fact]
    public void MetricsReportsFirstStepWithinTolerance()
    {
        var definition = Definition();
        var armature = definition.CreateArmature();
        var target = new Vector2(1f, 1f);
        var observation = armature.Reset("metrics-episode", 11);
        var record = PlanarArmatureRunRecord.Start(armature, observation, target, 0.1f, 0.01f);

        for (var i = 0; i < 10; i++)
        {
            var action = PlanarArmatureBaselinePolicy.CreateAction(definition, armature.Observe(), target);
            record = record.Append(armature.Advance(action, 0.1f));
        }

        Assert.Equal(8, record.Metrics.StepsToTarget);
        Assert.Equal(10, record.Metrics.StepsExecuted);
    }

    [Fact]
    public void StartRejectsInvalidTimingAndNonFiniteTargets()
    {
        var armature = Definition().CreateArmature();
        var observation = armature.Reset("episode", 1);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarArmatureRunRecord.Start(armature, observation, Vector2.One, 0f, 0.01f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarArmatureRunRecord.Start(armature, observation, new Vector2(float.NaN, 0f), 0.1f, 0.01f));
        var mismatched = new PlanarArmatureDefinition(
            "run-record-fixture", "shoulder", "upper", "elbow", "lower", "tip",
            Vector2.Zero, 2f, 1f,
            new JointAngleLimits(-MathF.PI, MathF.PI),
            new JointAngleLimits(-MathF.PI, MathF.PI),
            0f, 0f, 2f, 2f);
        var mismatchedArmature = mismatched.CreateArmature();
        mismatchedArmature.Reset("episode", 1);
        Assert.Throws<ArgumentException>(() =>
            PlanarArmatureRunRecord.Start(mismatchedArmature, observation, Vector2.One, 0.1f, 0.01f));
    }

    private static PlanarArmatureRunRecord Run()
    {
        var definition = Definition();
        var armature = definition.CreateArmature();
        var observation = armature.Reset("record-episode", 7);
        var target = new Vector2(1f, 1f);
        var record = PlanarArmatureRunRecord.Start(armature, observation, target, 0.1f, 0.01f);
        for (var i = 0; i < 3; i++)
        {
            var action = PlanarArmatureBaselinePolicy.CreateAction(definition, armature.Observe(), target);
            record = record.Append(armature.Advance(action, 0.1f));
        }
        return record;
    }

    private static PlanarArmatureDefinition Definition()
    {
        var limits = new JointAngleLimits(-MathF.PI, MathF.PI);
        return new PlanarArmatureDefinition(
            "run-record-fixture", "shoulder", "upper", "elbow", "lower", "tip",
            Vector2.Zero, 1f, 1f, limits, limits, 0f, 0f, 2f, 2f);
    }
}
