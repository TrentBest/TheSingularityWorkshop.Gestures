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

        var json = JsonSerializer.Serialize(first);
        Assert.Contains("\"SchemaVersion\":1", json);
        Assert.Contains("\"RequestedAction\"", json);
        Assert.Contains("\"AcceptedShoulderTargetRadians\"", json);
        Assert.Contains("\"TargetError\"", json);
    }

    [Fact]
    public void AppendIsImmutableAndRejectsWrongEpisodeOrSequence()
    {
        var definition = Definition();
        var armature = definition.CreateArmature();
        var initial = armature.Reset("record-episode", 7);
        var record = PlanarArmatureRunRecord.Start(definition, initial, new Vector2(1f, 1f), 0.1f, 0.01f);
        var action = PlanarArmatureBaselinePolicy.CreateAction(definition, initial, new Vector2(1f, 1f));
        var result = armature.Advance(action, 0.1f);
        var updated = record.Append(result);

        Assert.Empty(record.Steps);
        Assert.Single(updated.Steps);
        Assert.Throws<ArgumentException>(() => updated.Append(result));
    }

    [Fact]
    public void StartRejectsInvalidTimingAndNonFiniteTargets()
    {
        var observation = Definition().CreateArmature().Reset("episode", 1);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarArmatureRunRecord.Start(Definition(), observation, Vector2.One, 0f, 0.01f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            PlanarArmatureRunRecord.Start(Definition(), observation, new Vector2(float.NaN, 0f), 0.1f, 0.01f));
    }

    private static PlanarArmatureRunRecord Run()
    {
        var definition = Definition();
        var armature = definition.CreateArmature();
        var observation = armature.Reset("record-episode", 7);
        var target = new Vector2(1f, 1f);
        var record = PlanarArmatureRunRecord.Start(definition, observation, target, 0.1f, 0.01f);
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
