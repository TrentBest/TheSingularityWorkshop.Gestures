namespace TheSingularityWorkshop.Gestures;

/// <summary>Structural integrity of a joint whose loads are assessed by an external physics model.</summary>
public enum JointIntegrityState
{
    /// <summary>The joint has not exceeded its configured failure thresholds.</summary>
    Intact,
    /// <summary>The joint has exceeded at least one configured failure threshold.</summary>
    Broken
}

/// <summary>Outcome of submitting an externally measured joint load.</summary>
/// <param name="Integrity">Integrity after processing the observation.</param>
/// <param name="BrokeThisStep">Whether this observation caused the transition from intact to broken.</param>
/// <param name="ForceMagnitudeNewtons">Externally measured or computed resultant force magnitude.</param>
/// <param name="TorqueMagnitudeNewtonMeters">Externally measured or computed resultant torque magnitude.</param>
public readonly record struct JointBreakResult(
    JointIntegrityState Integrity,
    bool BrokeThisStep,
    float ForceMagnitudeNewtons,
    float TorqueMagnitudeNewtonMeters);

/// <summary>
/// Retains structural failure state and compares loads supplied by the owning physics/domain model
/// against explicit force and torque thresholds.
/// </summary>
/// <remarks>
/// This type does not calculate forces, integrate dynamics, detect collisions, or simulate tissue.
/// The physics/domain owner supplies the load observation. Exceeding either threshold irreversibly
/// marks this instance broken; the owner decides how that outcome affects constraints, animation,
/// injuries, dismemberment, debris, or other domain consequences.
/// </remarks>
public sealed class StatefulBreakableJoint
{
    /// <summary>Creates a breakable joint with positive force and torque failure thresholds.</summary>
    public StatefulBreakableJoint(
        float breakForceNewtons,
        float breakTorqueNewtonMeters)
    {
        ValidatePositiveFinite(breakForceNewtons, nameof(breakForceNewtons));
        ValidatePositiveFinite(breakTorqueNewtonMeters, nameof(breakTorqueNewtonMeters));

        BreakForceNewtons = breakForceNewtons;
        BreakTorqueNewtonMeters = breakTorqueNewtonMeters;
    }

    /// <summary>Force magnitude above which the joint fails, in newtons.</summary>
    public float BreakForceNewtons { get; }

    /// <summary>Torque magnitude above which the joint fails, in newton-metres.</summary>
    public float BreakTorqueNewtonMeters { get; }

    /// <summary>Retained structural integrity; once broken, this instance remains broken.</summary>
    public JointIntegrityState Integrity { get; private set; } = JointIntegrityState.Intact;

    /// <summary>
    /// Processes one force/torque observation supplied by the external physics/domain owner.
    /// A strict threshold exceedance of either measure breaks the joint; equality does not.
    /// </summary>
    public JointBreakResult ObserveLoad(
        float forceMagnitudeNewtons,
        float torqueMagnitudeNewtonMeters)
    {
        ValidateNonNegativeFinite(forceMagnitudeNewtons, nameof(forceMagnitudeNewtons));
        ValidateNonNegativeFinite(torqueMagnitudeNewtonMeters, nameof(torqueMagnitudeNewtonMeters));

        var brokeThisStep = Integrity == JointIntegrityState.Intact
            && (forceMagnitudeNewtons > BreakForceNewtons
                || torqueMagnitudeNewtonMeters > BreakTorqueNewtonMeters);

        if (brokeThisStep)
            Integrity = JointIntegrityState.Broken;

        return new JointBreakResult(
            Integrity,
            brokeThisStep,
            forceMagnitudeNewtons,
            torqueMagnitudeNewtonMeters);
    }

    private static void ValidatePositiveFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value <= 0f)
            throw new ArgumentOutOfRangeException(parameterName, "Threshold must be finite and positive.");
    }

    private static void ValidateNonNegativeFinite(float value, string parameterName)
    {
        if (!float.IsFinite(value) || value < 0f)
            throw new ArgumentOutOfRangeException(parameterName, "Load magnitude must be finite and non-negative.");
    }
}
