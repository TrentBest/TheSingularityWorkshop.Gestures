namespace TheSingularityWorkshop.Gestures;
public enum JointMotionState { MovingPositive, MovingNegative, Stationary }
public readonly record struct StatefulHingeStep(float PreviousAngleRadians,float AngleRadians,float RequestedAngleRadians,JointMotionState MotionState,bool TargetReached,bool JointLimitConstrained);
/// <summary>Deterministic speed-limited kinematic hinge; this is not a physical joint solver.</summary>
public sealed class StatefulHinge
{
    private readonly float _minimumRadians,_maximumRadians,_stationaryToleranceRadians;
    public StatefulHinge(float initialAngleRadians,float minimumRadians,float maximumRadians,float stationaryToleranceRadians=1e-5f)
    {
        ValidateFinite(initialAngleRadians,nameof(initialAngleRadians)); ValidateFinite(minimumRadians,nameof(minimumRadians)); ValidateFinite(maximumRadians,nameof(maximumRadians)); ValidateFinite(stationaryToleranceRadians,nameof(stationaryToleranceRadians));
        if(minimumRadians>maximumRadians) throw new ArgumentException("Minimum angle must not exceed maximum angle.");
        if(initialAngleRadians<minimumRadians||initialAngleRadians>maximumRadians) throw new ArgumentOutOfRangeException(nameof(initialAngleRadians));
        if(stationaryToleranceRadians<0f) throw new ArgumentOutOfRangeException(nameof(stationaryToleranceRadians));
        _minimumRadians=minimumRadians; _maximumRadians=maximumRadians; _stationaryToleranceRadians=stationaryToleranceRadians; AngleRadians=initialAngleRadians;
    }
    public float AngleRadians {get;private set;}
    public float MinimumRadians=>_minimumRadians;
    public float MaximumRadians=>_maximumRadians;
    internal event Action? AngleChanged;
    /// <summary>Restores a legal retained angle and invalidates dependent hierarchy transforms.</summary>
    public void Reset(float angleRadians)
    {
        ValidateFinite(angleRadians,nameof(angleRadians));
        if(angleRadians<_minimumRadians||angleRadians>_maximumRadians) throw new ArgumentOutOfRangeException(nameof(angleRadians));
        if(AngleRadians==angleRadians) return;
        AngleRadians=angleRadians; AngleChanged?.Invoke();
    }
    public StatefulHingeStep Advance(float requestedAngleRadians,float deltaTimeSeconds,float maximumAngularSpeedRadiansPerSecond)
    {
        ValidateFinite(requestedAngleRadians,nameof(requestedAngleRadians)); ValidateFinite(deltaTimeSeconds,nameof(deltaTimeSeconds)); ValidateFinite(maximumAngularSpeedRadiansPerSecond,nameof(maximumAngularSpeedRadiansPerSecond));
        if(deltaTimeSeconds<0f) throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
        if(maximumAngularSpeedRadiansPerSecond<0f) throw new ArgumentOutOfRangeException(nameof(maximumAngularSpeedRadiansPerSecond));
        var previous=AngleRadians; var constrained=Math.Clamp(requestedAngleRadians,_minimumRadians,_maximumRadians);
        var maximumStep=maximumAngularSpeedRadiansPerSecond*deltaTimeSeconds;
        if(!float.IsFinite(maximumStep)) throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds),"Angular speed budget must be finite.");
        AngleRadians=Math.Clamp(previous+Math.Clamp(constrained-previous,-maximumStep,maximumStep),_minimumRadians,_maximumRadians);
        if(AngleRadians!=previous) AngleChanged?.Invoke();
        var change=AngleRadians-previous;
        var motion=change>_stationaryToleranceRadians?JointMotionState.MovingPositive:change< -_stationaryToleranceRadians?JointMotionState.MovingNegative:JointMotionState.Stationary;
        var limited=requestedAngleRadians<_minimumRadians||requestedAngleRadians>_maximumRadians;
        return new(previous,AngleRadians,requestedAngleRadians,motion,!limited&&MathF.Abs(AngleRadians-requestedAngleRadians)<=_stationaryToleranceRadians,limited);
    }
    private static void ValidateFinite(float value,string name) {if(!float.IsFinite(value)) throw new ArgumentOutOfRangeException(name,"Value must be finite.");}
}