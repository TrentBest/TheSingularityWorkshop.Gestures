namespace TheSingularityWorkshop.Gestures;

/// <summary>A sample comparing a requested scalar target with direct following and spring-damper response.</summary>
public readonly record struct MotionResponseSample(
    double TimeSeconds,
    double TargetPosition,
    double DirectPosition,
    double SpringPosition,
    double SpringVelocity);

/// <summary>
/// Summary of a reproducible target-tracking experiment. Errors are dimensionless coordinate units;
/// settling time is measured from the end of the target ramp.
/// </summary>
public sealed record MotionResponseComparisonResult(
    IReadOnlyList<MotionResponseSample> Samples,
    double MeanAbsoluteTrackingError,
    double MaximumAbsoluteTrackingError,
    double PeakOvershoot,
    double? SettlingTimeSeconds);

/// <summary>
/// Builds a deterministic one-dimensional comparison between direct target following and a
/// stateful spring-damper. This is an experiment harness, not a physical or articulated-body solver.
/// </summary>
public static class MotionResponseComparison
{
    /// <summary>
    /// Ramps the target from zero to one, then holds it. Both methods receive the same target
    /// samples; direct following is the zero-lag reference and spring response is numerically integrated.
    /// </summary>
    /// <param name="rampDurationSeconds">Positive duration of the linear target ramp.</param>
    /// <param name="holdDurationSeconds">Non-negative duration to observe the stationary target after the ramp.</param>
    /// <param name="stepSeconds">Fixed positive timestep used to sample and advance the response.</param>
    /// <param name="parameters">Spring-damper parameters.</param>
    /// <param name="settlingTolerance">Positive position and velocity tolerance used for settling detection.</param>
    /// <returns>Samples and separate tracking/overshoot/settling metrics.</returns>
    public static MotionResponseComparisonResult Run(
        double rampDurationSeconds,
        double holdDurationSeconds,
        double stepSeconds,
        SpringDamperParameters parameters,
        double settlingTolerance = 0.01d)
    {
        ValidatePositiveFinite(rampDurationSeconds, nameof(rampDurationSeconds));
        if (!double.IsFinite(holdDurationSeconds) || holdDurationSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(holdDurationSeconds), "Hold duration must be finite and non-negative.");
        ValidatePositiveFinite(stepSeconds, nameof(stepSeconds));
        ValidatePositiveFinite(settlingTolerance, nameof(settlingTolerance));
        parameters.Validate();

        var totalDuration = rampDurationSeconds + holdDurationSeconds;
        if (!double.IsFinite(totalDuration))
            throw new ArgumentOutOfRangeException(nameof(holdDurationSeconds), "Combined duration must be finite.");
        var stepCountDouble = Math.Ceiling(totalDuration / stepSeconds);
        if (!double.IsFinite(stepCountDouble) || stepCountDouble > int.MaxValue - 1d)
            throw new ArgumentOutOfRangeException(nameof(stepSeconds), "The requested experiment has too many steps.");

        var stepCount = (int)stepCountDouble;
        var samples = new MotionResponseSample[stepCount + 1];
        var motion = new ScalarSpringDamper();
        var absoluteErrorTotal = 0d;
        var maximumAbsoluteError = 0d;
        var peakOvershoot = 0d;

        for (var i = 0; i <= stepCount; i++)
        {
            var time = i * stepSeconds;
            var target = Math.Clamp(time / rampDurationSeconds, 0d, 1d);
            if (i > 0)
                motion.Advance(target, stepSeconds, parameters);

            var error = Math.Abs(target - motion.Position);
            absoluteErrorTotal += error;
            maximumAbsoluteError = Math.Max(maximumAbsoluteError, error);
            peakOvershoot = Math.Max(peakOvershoot, Math.Max(0d, motion.Position - 1d));
            samples[i] = new MotionResponseSample(time, target, target, motion.Position, motion.Velocity);
        }

        double? settlingTime = FindSettlingTime(samples, rampDurationSeconds, settlingTolerance, stepSeconds);
        return new MotionResponseComparisonResult(
            samples,
            absoluteErrorTotal / samples.Length,
            maximumAbsoluteError,
            peakOvershoot,
            settlingTime);
    }

    private static double? FindSettlingTime(
        IReadOnlyList<MotionResponseSample> samples,
        double rampDurationSeconds,
        double tolerance,
        double stepSeconds)
    {
        // Require 100 ms of consecutive samples inside both position and velocity tolerances.
        var requiredSamples = Math.Max(1, (int)Math.Ceiling(0.1d / stepSeconds));
        for (var start = 0; start < samples.Count; start++)
        {
            var first = samples[start];
            if (first.TimeSeconds < rampDurationSeconds ||
                Math.Abs(first.SpringPosition - 1d) > tolerance ||
                Math.Abs(first.SpringVelocity) > tolerance)
                continue;

            var stable = true;
            for (var offset = 1; offset < requiredSamples; offset++)
            {
                var index = start + offset;
                if (index >= samples.Count ||
                    Math.Abs(samples[index].SpringPosition - 1d) > tolerance ||
                    Math.Abs(samples[index].SpringVelocity) > tolerance)
                {
                    stable = false;
                    break;
                }
            }

            if (stable)
                return first.TimeSeconds - rampDurationSeconds;
        }

        return null;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value) || value <= 0d)
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and positive.");
    }
}
