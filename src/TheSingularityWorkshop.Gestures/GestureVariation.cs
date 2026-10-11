namespace TheSingularityWorkshop.Gestures;

/// <summary>Bounds for deterministic per-identity variation around an ideal Gesture.</summary>
public sealed record GestureVariation
{
    /// <summary>Maximum fractional speed deviation. Must be in [0, 1).</summary>
    public double MaximumSpeedDeviation { get; init; }
    /// <summary>Maximum absolute local X offset returned as a root/presentation offset.</summary>
    public float MaximumLateralOffset { get; init; }

    /// <summary>Validates bounds before evaluation.</summary>
    public void Validate()
    {
        if (!double.IsFinite(MaximumSpeedDeviation) || MaximumSpeedDeviation < 0d || MaximumSpeedDeviation >= 1d)
            throw new ArgumentOutOfRangeException(nameof(MaximumSpeedDeviation), "Speed deviation must be in [0, 1).");
        if (!float.IsFinite(MaximumLateralOffset) || MaximumLateralOffset < 0f)
            throw new ArgumentOutOfRangeException(nameof(MaximumLateralOffset), "Lateral offset must be finite and non-negative.");
    }

    /// <summary>No individual variation.</summary>
    public static GestureVariation None { get; } = new();
}
