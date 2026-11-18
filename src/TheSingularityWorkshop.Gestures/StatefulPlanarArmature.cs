using System.Numerics;
namespace TheSingularityWorkshop.Gestures;
public enum PlanarArmatureActionMode:byte { JointAngleTargets=1 }
/// <summary>Immutable configuration for a stable two-link planar kinematic fixture.</summary>
public sealed class PlanarArmatureDefinition
{
    public PlanarArmatureDefinition(string scenarioId,string shoulderJointId,string shoulderLinkId,string elbowJointId,string elbowLinkId,string endEffectorId,Vector2 rootAnchor,float shoulderLinkLength,float elbowLinkLength,JointAngleLimits shoulderLimits,JointAngleLimits elbowLimits,float initialShoulderAngleRadians,float initialElbowAngleRadians,float maximumShoulderSpeedRadiansPerSecond,float maximumElbowSpeedRadiansPerSecond)
    {
        ScenarioId=Id(scenarioId,nameof(scenarioId)); ShoulderJointId=Id(shoulderJointId,nameof(shoulderJointId)); ShoulderLinkId=Id(shoulderLinkId,nameof(shoulderLinkId)); ElbowJointId=Id(elbowJointId,nameof(elbowJointId)); ElbowLinkId=Id(elbowLinkId,nameof(elbowLinkId)); EndEffectorId=Id(endEffectorId,nameof(endEffectorId));
        if(new[]{ShoulderJointId,ShoulderLinkId,ElbowJointId,ElbowLinkId,EndEffectorId}.Distinct(StringComparer.Ordinal).Count()!=5) throw new ArgumentException("Joint, link, and end-effector identities must be unique.");
        if(!float.IsFinite(rootAnchor.X)||!float.IsFinite(rootAnchor.Y)) throw new ArgumentOutOfRangeException(nameof(rootAnchor));
        Positive(shoulderLinkLength,nameof(shoulderLinkLength)); Positive(elbowLinkLength,nameof(elbowLinkLength)); Limits(shoulderLimits,nameof(shoulderLimits)); Limits(elbowLimits,nameof(elbowLimits));
        Within(initialShoulderAngleRadians,shoulderLimits,nameof(initialShoulderAngleRadians)); Within(initialElbowAngleRadians,elbowLimits,nameof(initialElbowAngleRadians));
        NonNegative(maximumShoulderSpeedRadiansPerSecond,nameof(maximumShoulderSpeedRadiansPerSecond)); NonNegative(maximumElbowSpeedRadiansPerSecond,nameof(maximumElbowSpeedRadiansPerSecond));
        RootAnchor=rootAnchor; ShoulderLinkLength=shoulderLinkLength; ElbowLinkLength=elbowLinkLength; ShoulderLimits=shoulderLimits; ElbowLimits=elbowLimits; InitialShoulderAngleRadians=initialShoulderAngleRadians; InitialElbowAngleRadians=initialElbowAngleRadians; MaximumShoulderSpeedRadiansPerSecond=maximumShoulderSpeedRadiansPerSecond; MaximumElbowSpeedRadiansPerSecond=maximumElbowSpeedRadiansPerSecond;
    }
    public string ScenarioId{get;} public string ShoulderJointId{get;} public string ShoulderLinkId{get;} public string ElbowJointId{get;} public string ElbowLinkId{get;} public string EndEffectorId{get;}
    public Vector2 RootAnchor{get;} public float ShoulderLinkLength{get;} public float ElbowLinkLength{get;} public JointAngleLimits ShoulderLimits{get;} public JointAngleLimits ElbowLimits{get;}
    public float InitialShoulderAngleRadians{get;} public float InitialElbowAngleRadians{get;} public float MaximumShoulderSpeedRadiansPerSecond{get;} public float MaximumElbowSpeedRadiansPerSecond{get;}
    public StatefulPlanarArmature CreateArmature()=>new(this);
    private static string Id(string s,string n)=>string.IsNullOrWhiteSpace(s)?throw new ArgumentException("Identity must not be empty.",n):s;
    private static void Positive(float v,string n){if(!float.IsFinite(v)||v<=0f)throw new ArgumentOutOfRangeException(n);}
    private static void NonNegative(float v,string n){if(!float.IsFinite(v)||v<0f)throw new ArgumentOutOfRangeException(n);}
    private static void Limits(JointAngleLimits l,string n){if(!float.IsFinite(l.MinimumRadians)||!float.IsFinite(l.MaximumRadians)||l.MinimumRadians>l.MaximumRadians)throw new ArgumentException("Joint limits must be finite and ordered.",n);}
    private static void Within(float a,JointAngleLimits l,string n){if(!float.IsFinite(a)||a<l.MinimumRadians||a>l.MaximumRadians)throw new ArgumentOutOfRangeException(n);}
}
/// <summary>Versioned observation of achieved kinematic state; velocity and contact are intentionally absent.</summary>
public readonly record struct PlanarArmatureObservation(int ContractVersion,string ScenarioId,string EpisodeId,ulong Seed,long StepIndex,Vector2 RootAnchor,string ShoulderJointId,float ShoulderAngleRadians,JointAngleLimits ShoulderLimits,string ElbowJointId,float ElbowAngleRadians,JointAngleLimits ElbowLimits,string EndEffectorId,Vector2 AchievedEndEffectorPosition);
/// <summary>Versioned per-joint target action bound to one episode and expected step.</summary>
public readonly record struct PlanarArmatureAction(int ContractVersion,string EpisodeId,long ExpectedStepIndex,PlanarArmatureActionMode Mode,float ShoulderTargetRadians,float ElbowTargetRadians);
/// <summary>Preserves requested action, range-accepted targets, bounded hinge transitions, and measured output.</summary>
public readonly record struct PlanarArmatureStepResult(PlanarArmatureAction RequestedAction,float AcceptedShoulderTargetRadians,float AcceptedElbowTargetRadians,StatefulHingeStep ShoulderStep,StatefulHingeStep ElbowStep,PlanarArmatureObservation Observation);
/// <summary>Deterministic CPU reference for a two-link planar kinematic arm; no physical actuation is modeled.</summary>
public sealed class StatefulPlanarArmature
{
    public const int CurrentContractVersion=1;
    private readonly PlanarArmatureDefinition _definition; private long _stepIndex; private string? _episodeId; private ulong _seed;
    internal StatefulPlanarArmature(PlanarArmatureDefinition d)
    {
        _definition=d;
        Shoulder=new StatefulJointNode(d.ShoulderLinkId,d.ShoulderLinkLength,new StatefulHinge(d.InitialShoulderAngleRadians,d.ShoulderLimits.MinimumRadians,d.ShoulderLimits.MaximumRadians));
        Elbow=new StatefulJointNode(d.ElbowLinkId,d.ElbowLinkLength,new StatefulHinge(d.InitialElbowAngleRadians,d.ElbowLimits.MinimumRadians,d.ElbowLimits.MaximumRadians));
        Shoulder.AddChild(Elbow); Shoulder.EvaluateFull(d.RootAnchor);
    }
    public StatefulJointNode Shoulder{get;} public StatefulJointNode Elbow{get;} public long StepIndex=>_stepIndex; public string? EpisodeId=>_episodeId; public ulong Seed=>_seed;
    /// <summary>Starts or restarts an episode at exact configured initial conditions.</summary>
    public PlanarArmatureObservation Reset(string episodeId,ulong seed)
    {
        if(string.IsNullOrWhiteSpace(episodeId)) throw new ArgumentException("Episode identity must not be empty.",nameof(episodeId));
        Shoulder.Hinge.Reset(_definition.InitialShoulderAngleRadians); Elbow.Hinge.Reset(_definition.InitialElbowAngleRadians); Shoulder.EvaluateFull(_definition.RootAnchor);
        _episodeId=episodeId; _seed=seed; _stepIndex=0; return Observe();
    }
    /// <summary>Validates the full envelope before mutation, advances bounded hinges, and reports measured pose.</summary>
    public PlanarArmatureStepResult Advance(PlanarArmatureAction action,float deltaTimeSeconds)
    {
        if(_episodeId is null) throw new InvalidOperationException("Reset must start an episode before stepping.");
        if(action.ContractVersion!=CurrentContractVersion) throw new ArgumentException("Unsupported action contract version.",nameof(action));
        if(!StringComparer.Ordinal.Equals(action.EpisodeId,_episodeId)) throw new ArgumentException("Action episode does not match the active episode.",nameof(action));
        if(action.ExpectedStepIndex!=_stepIndex) throw new ArgumentException("Action step index is not the next expected step.",nameof(action));
        if(action.Mode!=PlanarArmatureActionMode.JointAngleTargets) throw new ArgumentException("Unsupported action mode.",nameof(action));
        if(!float.IsFinite(action.ShoulderTargetRadians)||!float.IsFinite(action.ElbowTargetRadians)) throw new ArgumentOutOfRangeException(nameof(action),"Targets must be finite.");
        if(!float.IsFinite(deltaTimeSeconds)||deltaTimeSeconds<0f) throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
        // Preflight both joints before either hinge mutates; a later overflow must not leave a partial step.
        if(!float.IsFinite(_definition.MaximumShoulderSpeedRadiansPerSecond*deltaTimeSeconds)||!float.IsFinite(_definition.MaximumElbowSpeedRadiansPerSecond*deltaTimeSeconds)) throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds),"Both angular speed budgets must be finite.");
        var acceptedShoulder=Math.Clamp(action.ShoulderTargetRadians,_definition.ShoulderLimits.MinimumRadians,_definition.ShoulderLimits.MaximumRadians);
        var acceptedElbow=Math.Clamp(action.ElbowTargetRadians,_definition.ElbowLimits.MinimumRadians,_definition.ElbowLimits.MaximumRadians);
        var shoulderStep=Shoulder.Hinge.Advance(action.ShoulderTargetRadians,deltaTimeSeconds,_definition.MaximumShoulderSpeedRadiansPerSecond);
        var elbowStep=Elbow.Hinge.Advance(action.ElbowTargetRadians,deltaTimeSeconds,_definition.MaximumElbowSpeedRadiansPerSecond);
        Shoulder.EvaluateIncremental(_definition.RootAnchor); _stepIndex++;
        return new(action,acceptedShoulder,acceptedElbow,shoulderStep,elbowStep,Observe());
    }
    public PlanarArmatureObservation Observe()
    {
        if(_episodeId is null) throw new InvalidOperationException("Reset must start an episode before observing.");
        return new(CurrentContractVersion,_definition.ScenarioId,_episodeId,_seed,_stepIndex,_definition.RootAnchor,_definition.ShoulderJointId,Shoulder.Hinge.AngleRadians,_definition.ShoulderLimits,_definition.ElbowJointId,Elbow.Hinge.AngleRadians,_definition.ElbowLimits,_definition.EndEffectorId,Elbow.WorldEnd);
    }
}