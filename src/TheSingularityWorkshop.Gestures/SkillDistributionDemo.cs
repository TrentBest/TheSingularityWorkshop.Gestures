namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A deterministic, illustrative 2D sample model for explaining how movement variation
/// and outcome spread differ. This is not a physical, ballistic, or weapon model.
/// </summary>
public static class SkillDistributionDemo
{
    /// <summary>
    /// Generates reproducible samples around an ideal origin. Movement offsets are
    /// bounded by <paramref name="movementSpread"/>; independent outcome noise is
    /// bounded by <paramref name="outcomeNoise"/> and added to each movement offset.
    /// All distances are arbitrary, dimensionless illustration units.
    /// </summary>
    public static SkillDistributionResult Generate(
        int seed,
        int sampleCount,
        double movementSpread,
        double outcomeNoise)
    {
        if (sampleCount < 1)
            throw new ArgumentOutOfRangeException(nameof(sampleCount), "At least one sample is required.");
        if (!double.IsFinite(movementSpread) || movementSpread < 0)
            throw new ArgumentOutOfRangeException(nameof(movementSpread), "Spread must be finite and non-negative.");
        if (!double.IsFinite(outcomeNoise) || outcomeNoise < 0)
            throw new ArgumentOutOfRangeException(nameof(outcomeNoise), "Noise must be finite and non-negative.");

        var random = new DeterministicRandom(unchecked((uint)seed));
        var samples = new SkillDistributionSample[sampleCount];
        var movementMeanX = 0d;
        var movementMeanY = 0d;
        var outcomeMeanX = 0d;
        var outcomeMeanY = 0d;
        var poseErrorTotal = 0d;

        for (var i = 0; i < sampleCount; i++)
        {
            var movementX = (random.NextUnit() * 2d - 1d) * movementSpread;
            var movementY = (random.NextUnit() * 2d - 1d) * movementSpread;
            var outcomeX = movementX + (random.NextUnit() * 2d - 1d) * outcomeNoise;
            var outcomeY = movementY + (random.NextUnit() * 2d - 1d) * outcomeNoise;

            samples[i] = new SkillDistributionSample(movementX, movementY, outcomeX, outcomeY);
            movementMeanX += movementX;
            movementMeanY += movementY;
            outcomeMeanX += outcomeX;
            outcomeMeanY += outcomeY;
            poseErrorTotal += Math.Sqrt(movementX * movementX + movementY * movementY);
        }

        movementMeanX /= sampleCount;
        movementMeanY /= sampleCount;
        outcomeMeanX /= sampleCount;
        outcomeMeanY /= sampleCount;

        var movementVariance = 0d;
        var outcomeSpread = 0d;
        foreach (var sample in samples)
        {
            var movementDx = sample.MovementX - movementMeanX;
            var movementDy = sample.MovementY - movementMeanY;
            var outcomeDx = sample.OutcomeX - outcomeMeanX;
            var outcomeDy = sample.OutcomeY - outcomeMeanY;
            movementVariance += movementDx * movementDx + movementDy * movementDy;
            outcomeSpread += outcomeDx * outcomeDx + outcomeDy * outcomeDy;
        }

        return new SkillDistributionResult(
            seed,
            samples,
            poseErrorTotal / sampleCount,
            movementVariance / sampleCount,
            outcomeSpread / sampleCount,
            movementMeanX,
            movementMeanY,
            outcomeMeanX,
            outcomeMeanY);
    }

    private struct DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed) => _state = seed == 0 ? 0x9E3779B9u : seed;

        public double NextUnit()
        {
            // Xorshift32: fixed algorithm, no runtime-dependent Random implementation.
            var value = _state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            _state = value;
            return value / ((double)uint.MaxValue + 1d);
        }
    }
}

/// <summary>One illustrative movement sample and its separately perturbed outcome.</summary>
public readonly record struct SkillDistributionSample(
    double MovementX,
    double MovementY,
    double OutcomeX,
    double OutcomeY);

/// <summary>
/// Summary statistics from <see cref="SkillDistributionDemo"/>.
/// Variances are mean squared 2D distances, in squared arbitrary illustration units.
/// </summary>
public sealed record SkillDistributionResult(
    int Seed,
    IReadOnlyList<SkillDistributionSample> Samples,
    double MeanPoseError,
    double MovementVariance,
    double OutcomeSpread,
    double MeanMovementX,
    double MeanMovementY,
    double MeanOutcomeX,
    double MeanOutcomeY);
