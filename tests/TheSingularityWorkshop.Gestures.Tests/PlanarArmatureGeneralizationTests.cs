using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Reproducible held-out sweep for the deterministic two-link baseline.
/// The fixed PRNG seed makes every sampled configuration and target repeatable.
/// </summary>
public sealed class PlanarArmatureGeneralizationTests
{
    private const int SampleCount = 256;
    private const int MaximumSteps = 32;
    private const float DeltaTimeSeconds = 0.1f;
    private const float TargetTolerance = 0.01f;

    [Fact]
    public void BaselineConvergesAcrossDeterministicHeldOutConfigurationsAndTargets()
    {
        var random = new DeterministicRandom(0xC1A1_2026_1120UL);
        var limits = new JointAngleLimits(-1.4f, 1.4f);
        var stepsToTarget = new int[SampleCount];
        var maximumFinalError = 0f;

        for (var sample = 0; sample < SampleCount; sample++)
        {
            var shoulderLength = random.NextFloat(0.5f, 2.5f);
            var elbowLength = random.NextFloat(0.5f, 2.5f);
            var anchor = new Vector2(
                random.NextFloat(-5f, 5f),
                random.NextFloat(-5f, 5f));
            var targetShoulder = random.NextFloat(-1.2f, 1.2f);
            var targetElbow = random.NextFloat(-1.2f, 1.2f);
            var initialShoulder = random.NextFloat(-1.2f, 1.2f);
            var initialElbow = random.NextFloat(-1.2f, 1.2f);

            var definition = new PlanarArmatureDefinition(
                $"held-out-{sample}",
                $"shoulder-{sample}",
                $"upper-{sample}",
                $"elbow-{sample}",
                $"lower-{sample}",
                $"end-effector-{sample}",
                anchor,
                shoulderLength,
                elbowLength,
                limits,
                limits,
                initialShoulder,
                initialElbow,
                4f,
                4f);

            var localTarget = PlanarKinematics.ForwardKinematics(
                shoulderLength, elbowLength, targetShoulder, targetElbow);
            var worldTarget = anchor + localTarget;
            var armature = definition.CreateArmature();
            var initial = armature.Reset($"episode-{sample}", (ulong)sample);
            var record = PlanarArmatureRunRecord.Start(
                armature, initial, worldTarget, DeltaTimeSeconds, TargetTolerance);

            for (var step = 0; step < MaximumSteps && !record.TargetReached; step++)
            {
                var action = PlanarArmatureBaselinePolicy.CreateAction(
                    definition, armature.Observe(), worldTarget);
                record = record.Append(armature.Advance(action, DeltaTimeSeconds));
            }

            Assert.True(
                record.TargetReached,
                $"Sample {sample} did not converge. Final error: {record.FinalTargetError:R}; " +
                $"links: ({shoulderLength:R}, {elbowLength:R}); " +
                $"target angles: ({targetShoulder:R}, {targetElbow:R}); " +
                $"initial angles: ({initialShoulder:R}, {initialElbow:R}).");

            Assert.NotNull(record.Metrics.StepsToTarget);
            Assert.InRange(record.Metrics.StepsToTarget!.Value, 0, MaximumSteps);
            stepsToTarget[sample] = record.Metrics.StepsToTarget.Value;
            maximumFinalError = MathF.Max(maximumFinalError, record.FinalTargetError);
        }

        Array.Sort(stepsToTarget);
        var meanSteps = stepsToTarget.Average();
        var p95Steps = stepsToTarget[(int)Math.Ceiling(SampleCount * 0.95) - 1];

        Assert.Equal(SampleCount, stepsToTarget.Length);
        Assert.InRange(meanSteps, 0d, MaximumSteps);
        Assert.InRange(p95Steps, 0, MaximumSteps);
        Assert.InRange(maximumFinalError, 0f, TargetTolerance);
    }

    [Fact]
    public void BaselineDoesNotReportAnOutOfReachTargetAsReached()
    {
        var limits = new JointAngleLimits(-1.4f, 1.4f);
        var definition = new PlanarArmatureDefinition(
            "unreachable-control",
            "shoulder",
            "upper",
            "elbow",
            "lower",
            "tip",
            new Vector2(3f, -2f),
            1f,
            1f,
            limits,
            limits,
            0f,
            0f,
            4f,
            4f);
        var armature = definition.CreateArmature();
        var initial = armature.Reset("unreachable-episode", 1);
        var target = definition.RootAnchor + new Vector2(2.5f, 0f);
        var record = PlanarArmatureRunRecord.Start(
            armature, initial, target, DeltaTimeSeconds, TargetTolerance);

        for (var step = 0; step < MaximumSteps && !record.TargetReached; step++)
        {
            var action = PlanarArmatureBaselinePolicy.CreateAction(
                definition, armature.Observe(), target);
            record = record.Append(armature.Advance(action, DeltaTimeSeconds));
        }

        Assert.False(record.TargetReached);
        Assert.True(record.FinalTargetError > TargetTolerance);
        Assert.Null(record.Metrics.StepsToTarget);
    }

    private sealed class DeterministicRandom
    {
        private ulong _state;

        public DeterministicRandom(ulong seed)
        {
            if (seed == 0)
                throw new ArgumentOutOfRangeException(nameof(seed));
            _state = seed;
        }

        public float NextFloat(float minimum, float maximum)
        {
            _state ^= _state << 13;
            _state ^= _state >> 7;
            _state ^= _state << 17;
            var unit = (_state >> 40) / 16_777_215f;
            return minimum + (maximum - minimum) * unit;
        }
    }
}
