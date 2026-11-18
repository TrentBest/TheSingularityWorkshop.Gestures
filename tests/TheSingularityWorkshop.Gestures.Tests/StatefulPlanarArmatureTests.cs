using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;
namespace TheSingularityWorkshop.Gestures.Tests;
public sealed class StatefulPlanarArmatureTests
{
    private static readonly JointAngleLimits Limits=new(-MathF.PI/2f,MathF.PI/2f);
    [Fact] public void ResetRestoresInitialPoseAndStableIdentities()
    {
        var a=Definition().CreateArmature(); var first=a.Reset("episode",42); a.Advance(Action("episode",0,2f,.5f),.1f); var reset=a.Reset("episode",42);
        Assert.Equal(1,first.ContractVersion); Assert.Equal("shoulder-joint",first.ShoulderJointId); Assert.Equal("elbow-joint",first.ElbowJointId); Assert.Equal("hand-tip",first.EndEffectorId);
        Assert.Equal(0,reset.StepIndex); Assert.Equal(first.AchievedEndEffectorPosition,reset.AchievedEndEffectorPosition); Assert.Equal(first.ShoulderAngleRadians,reset.ShoulderAngleRadians);
    }
    [Fact] public void ReportsRequestedAcceptedAndAchievedValuesSeparately()
    {
        var a=Definition().CreateArmature(); a.Reset("ep",1); var r=a.Advance(Action("ep",0,2f,.5f),.1f);
        Assert.Equal(2f,r.RequestedAction.ShoulderTargetRadians); Assert.Equal(Limits.MaximumRadians,r.AcceptedShoulderTargetRadians); Assert.True(r.ShoulderStep.JointLimitConstrained);
        Assert.Equal(.1f,r.ShoulderStep.AngleRadians,5); Assert.Equal(.2f,r.ElbowStep.AngleRadians,5); Assert.Equal(1,r.Observation.StepIndex); Assert.Equal(a.Elbow.WorldEnd,r.Observation.AchievedEndEffectorPosition);
    }
    [Fact] public void RejectsInvalidEnvelopeBeforeMutatingState()
    {
        var a=Definition().CreateArmature(); a.Reset("ep",1); var before=a.Observe();
        Assert.Throws<ArgumentException>(()=>a.Advance(new(2,"ep",0,PlanarArmatureActionMode.JointAngleTargets,0,0),1));
        Assert.Throws<ArgumentException>(()=>a.Advance(new(1,"other",0,PlanarArmatureActionMode.JointAngleTargets,0,0),1));
        Assert.Throws<ArgumentException>(()=>a.Advance(new(1,"ep",1,PlanarArmatureActionMode.JointAngleTargets,0,0),1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>a.Advance(new(1,"ep",0,PlanarArmatureActionMode.JointAngleTargets,float.NaN,0),1));
        Assert.Throws<ArgumentException>(()=>a.Advance(new(1,"ep",0,(PlanarArmatureActionMode)99,0,0),1));
        Assert.Throws<ArgumentOutOfRangeException>(()=>a.Advance(Action("ep",0,1,1),-1));
        Assert.Equal(before,a.Observe());
    }
    [Fact] public void OverflowInEitherJointSpeedBudgetRejectsWholeStepBeforeMutation()\n    {\n        var d=new PlanarArmatureDefinition("overflow","sj","sl","ej","el","tip",Vector2.Zero,1,1,Limits,Limits,0,0,1,2);\n        var a=d.CreateArmature(); a.Reset("ep",1); var before=a.Observe();\n        Assert.Throws<ArgumentOutOfRangeException>(()=>a.Advance(Action("ep",0,1,1),float.MaxValue));\n        Assert.Equal(before,a.Observe());\n    }\n    [Fact] public void ReplaysSameActionSequenceDeterministicallyAfterReset()
    {
        var a=Definition().CreateArmature(); var first=Run(a); a.Reset("replay",123); var second=Run(a); Assert.Equal(first,second);
    }
    [Fact] public void RequiresResetBeforeObservationOrStep()
    {
        var a=Definition().CreateArmature(); Assert.Throws<InvalidOperationException>(()=>a.Observe());
        Assert.Throws<InvalidOperationException>(()=>a.Advance(Action("ep",0,0,0),0));
    }
    private static PlanarArmatureObservation[] Run(StatefulPlanarArmature a)
    {
        a.Reset("replay",123); var result=new List<PlanarArmatureObservation>();
        for(var i=0;i<5;i++) result.Add(a.Advance(Action("replay",i,.4f+i*.1f,-.3f+i*.05f),.1f).Observation);
        return result.ToArray();
    }
    private static PlanarArmatureAction Action(string episode,long step,float shoulder,float elbow)=>new(1,episode,step,PlanarArmatureActionMode.JointAngleTargets,shoulder,elbow);
    private static PlanarArmatureDefinition Definition()=>new("two-link-reach","shoulder-joint","upper-arm","elbow-joint","forearm","hand-tip",new Vector2(2,3),1,1,Limits,Limits,0,0,1,2);
}