namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Deterministic, caller-supplied unit samples for per-identity Gesture variation.
/// Each channel must be finite and in the half-open interval [0, 1).
/// </summary>
/// <remarks>
/// Gestures deliberately does not prescribe a random-number generator. A host may
/// derive these samples from stable identity, logical tick, and purpose keys, then
/// pass the resulting values to the CPU reference evaluator.
/// </remarks>
public readonly record struct GestureVariationSample(double SpeedUnit, double LateralUnit)
{
    /// <summary>Validates both unit-interval samples.</summary>
    public void Validate()
    {
        if (!double.IsFinite(SpeedUnit) || SpeedUnit < 0d || SpeedUnit >= 1d)
            throw new ArgumentOutOfRangeException(nameof(SpeedUnit), "Speed sample must be finite and in [0, 1).");
        if (!double.IsFinite(LateralUnit) || LateralUnit < 0d || LateralUnit >= 1d)
            throw new ArgumentOutOfRangeException(nameof(LateralUnit), "Lateral sample must be finite and in [0, 1).");
    }
}
