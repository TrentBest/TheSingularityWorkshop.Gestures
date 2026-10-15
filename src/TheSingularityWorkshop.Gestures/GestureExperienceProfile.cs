namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Bounded ranges from which a stable per-entity <see cref="GestureVariation"/> can be sampled.
/// The ranges describe variation around an ideal Gesture; they do not measure competence.
/// </summary>
public sealed record GestureExperienceProfile
{
    /// <summary>Creates a validated experience profile with inclusive lower and upper bounds.</summary>
    public GestureExperienceProfile(double minimumSpeedDeviation, double maximumSpeedDeviation,
        float minimumLateralOffset, float maximumLateralOffset)
    {
        if (!double.IsFinite(minimumSpeedDeviation) || minimumSpeedDeviation < 0d || minimumSpeedDeviation >= 1d)
            throw new ArgumentOutOfRangeException(nameof(minimumSpeedDeviation));
        if (!double.IsFinite(maximumSpeedDeviation) || maximumSpeedDeviation < 0d || maximumSpeedDeviation >= 1d)
            throw new ArgumentOutOfRangeException(nameof(maximumSpeedDeviation));
        if (minimumSpeedDeviation > maximumSpeedDeviation)
            throw new ArgumentException("Minimum speed deviation must not exceed maximum.", nameof(minimumSpeedDeviation));
        if (!float.IsFinite(minimumLateralOffset) || minimumLateralOffset < 0f)
            throw new ArgumentOutOfRangeException(nameof(minimumLateralOffset));
        if (!float.IsFinite(maximumLateralOffset) || maximumLateralOffset < 0f)
            throw new ArgumentOutOfRangeException(nameof(maximumLateralOffset));
        if (minimumLateralOffset > maximumLateralOffset)
            throw new ArgumentException("Minimum lateral offset must not exceed maximum.", nameof(minimumLateralOffset));

        MinimumSpeedDeviation = minimumSpeedDeviation;
        MaximumSpeedDeviation = maximumSpeedDeviation;
        MinimumLateralOffset = minimumLateralOffset;
        MaximumLateralOffset = maximumLateralOffset;
    }

    /// <summary>Gets the inclusive lower bound for fractional speed deviation.</summary>
    public double MinimumSpeedDeviation { get; }
    /// <summary>Gets the inclusive upper bound for fractional speed deviation.</summary>
    public double MaximumSpeedDeviation { get; }
    /// <summary>Gets the inclusive lower bound for absolute lateral-offset magnitude.</summary>
    public float MinimumLateralOffset { get; }
    /// <summary>Gets the inclusive upper bound for absolute lateral-offset magnitude.</summary>
    public float MaximumLateralOffset { get; }

    /// <summary>The same profile and identity seed always produce the same bounded variation.</summary>
    public GestureVariation Sample(ulong identitySeed)
    {
        var speedUnit = ToUnitInterval(Mix(identitySeed ^ 0xA0761D6478BD642FUL));
        var lateralUnit = ToUnitInterval(Mix(identitySeed ^ 0xE7037ED1A0B428DBUL));
        return new GestureVariation
        {
            MaximumSpeedDeviation = MinimumSpeedDeviation +
                ((MaximumSpeedDeviation - MinimumSpeedDeviation) * speedUnit),
            MaximumLateralOffset = (float)(MinimumLateralOffset +
                ((MaximumLateralOffset - MinimumLateralOffset) * lateralUnit))
        };
    }

    private static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }

    private static double ToUnitInterval(ulong value) => (value >> 11) * (1.0 / (1UL << 53));
}
