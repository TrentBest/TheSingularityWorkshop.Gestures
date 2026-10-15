namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Parameters for the optional scalar spring-damper motion reference. This is a control-response
/// model, not a rigid-body or articulated-body physics solver.
/// </summary>
public readonly struct SpringDamperParameters
{
    /// <summary>Creates validated effective mass, stiffness, and damping parameters.</summary>
    /// <param name="effectiveMass">Positive effective mass or inertia in caller-defined consistent units.</param>
    /// <param name="stiffness">Non-negative restoring stiffness in caller-defined consistent units.</param>
    /// <param name="damping">Non-negative velocity damping in caller-defined consistent units.</param>
    public SpringDamperParameters(double effectiveMass, double stiffness, double damping)
    {
        if (!double.IsFinite(effectiveMass) || effectiveMass <= 0d)
            throw new ArgumentOutOfRangeException(nameof(effectiveMass), "Effective mass must be finite and positive.");
        if (!double.IsFinite(stiffness) || stiffness < 0d)
            throw new ArgumentOutOfRangeException(nameof(stiffness), "Stiffness must be finite and non-negative.");
        if (!double.IsFinite(damping) || damping < 0d)
            throw new ArgumentOutOfRangeException(nameof(damping), "Damping must be finite and non-negative.");

        EffectiveMass = effectiveMass;
        Stiffness = stiffness;
        Damping = damping;
    }

    /// <summary>Gets the positive effective mass or inertia.</summary>
    public double EffectiveMass { get; }

    /// <summary>Gets the non-negative restoring stiffness.</summary>
    public double Stiffness { get; }

    /// <summary>Gets the non-negative velocity damping.</summary>
    public double Damping { get; }

    internal void Validate()
    {
        if (!double.IsFinite(EffectiveMass) || EffectiveMass <= 0d)
            throw new ArgumentException("Effective mass must be finite and positive.", nameof(EffectiveMass));
        if (!double.IsFinite(Stiffness) || Stiffness < 0d)
            throw new ArgumentException("Stiffness must be finite and non-negative.", nameof(Stiffness));
        if (!double.IsFinite(Damping) || Damping < 0d)
            throw new ArgumentException("Damping must be finite and non-negative.", nameof(Damping));
    }
}

/// <summary>
/// Stateful one-dimensional spring-damper response toward a requested target.
/// The caller owns the clock, step size, target trajectory, and interpretation of the coordinate.
/// </summary>
/// <remarks>
/// Each positive-duration step uses semi-implicit Euler integration:
/// acceleration = (stiffness * (target - position) - damping * velocity) / effective mass.
/// Use a fixed, sufficiently small timestep and compare convergence for the chosen parameters.
/// This reference does not model collisions, contact, constraints, articulated coupling, or physical forces.
/// </remarks>
public sealed class ScalarSpringDamper
{
    /// <summary>Creates a response state at the supplied initial position and velocity.</summary>
    public ScalarSpringDamper(double initialPosition = 0d, double initialVelocity = 0d)
    {
        ValidateFinite(initialPosition, nameof(initialPosition));
        ValidateFinite(initialVelocity, nameof(initialVelocity));
        Position = initialPosition;
        Velocity = initialVelocity;
    }

    /// <summary>Gets the current scalar position.</summary>
    public double Position { get; private set; }

    /// <summary>Gets the current scalar velocity.</summary>
    public double Velocity { get; private set; }

    /// <summary>Replaces the retained position and velocity after validating both values.</summary>
    public void Reset(double position, double velocity = 0d)
    {
        ValidateFinite(position, nameof(position));
        ValidateFinite(velocity, nameof(velocity));
        Position = position;
        Velocity = velocity;
    }

    /// <summary>Advances the response toward a target by one caller-supplied timestep.</summary>
    /// <param name="target">Requested target coordinate for this step.</param>
    /// <param name="elapsedSeconds">Finite, non-negative timestep in seconds.</param>
    /// <param name="parameters">Validated response parameters.</param>
    /// <returns>The new retained position.</returns>
    public double Advance(double target, double elapsedSeconds, SpringDamperParameters parameters)
    {
        ValidateFinite(target, nameof(target));
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0d)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds), "Elapsed time must be finite and non-negative.");
        parameters.Validate();
        if (elapsedSeconds == 0d)
            return Position;

        var acceleration = (parameters.Stiffness * (target - Position) - parameters.Damping * Velocity)
            / parameters.EffectiveMass;
        var nextVelocity = Velocity + acceleration * elapsedSeconds;
        var nextPosition = Position + nextVelocity * elapsedSeconds;
        if (!double.IsFinite(acceleration) || !double.IsFinite(nextVelocity) || !double.IsFinite(nextPosition))
            throw new ArithmeticException("Spring-damper step produced a non-finite state; retained state was not changed.");

        Velocity = nextVelocity;
        Position = nextPosition;
        return Position;
    }

    private static void ValidateFinite(double value, string parameterName)
    {
        if (!double.IsFinite(value))
            throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
    }
}
