using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulPlanarIkControllerTests
{
    private static readonly JointAngleLimits FullTurn = new(-MathF.PI, MathF.PI);

    [Fact]
    public void AdvancesRetainedHingesAndReportsMeasuredEndpoint()
    {
        var (controller, shoulder, elbow) = CreateController();

        var first = controller.Advance(
            Vector2.Zero, new Vector2(1f, 1f), 0.1f, 0.1f, 0.1f);

        Assert.Equal(StatefulPlanarIkStatus.Moving, first.Status);
        Assert.InRange(MathF.Abs(shoulder.Hinge.AngleRadians), 0f, 0.010001f);
        Assert.InRange(MathF.Abs(elbow.Hinge.AngleRadians), 0f, 0.010001f);
        Assert.Equal(elbow.WorldEnd, first.EndEffector);
        Assert.InRange(Vector2.Distance(first.EndEffector, first.RequestedSolution.Target), 0f, 2f);

        var settled = controller.Advance(
            Vector2.Zero, new Vector2(1f, 1f), 1f, MathF.PI, MathF.PI);

        Assert.Equal(StatefulPlanarIkStatus.Reached, settled.Status);
        Assert.True(settled.ShoulderStep.TargetReached);
        Assert.True(settled.ElbowStep.TargetReached);
        Assert.InRange(settled.ErrorDistance, 0f, 1e-4f);
        Assert.InRange(Vector2.Distance(settled.EndEffector, new Vector2(1f, 1f)), 0f, 1e-4f);
    }

    [Fact]
    public void ReportsApproximateWhenTargetIsUnreachable()
    {
        var (controller, _, _) = CreateController();

        var target = new Vector2(3f, 0f);
        var step = controller.Advance(
            Vector2.Zero, target, 1f, MathF.PI, MathF.PI);

        Assert.Equal(StatefulPlanarIkStatus.Approximate, step.Status);
        Assert.False(step.RequestedSolution.TargetReached);
        Assert.InRange(step.ErrorDistance, 0.9999f, 1.0001f);
        Assert.True(Vector2.Distance(step.EndEffector, target) > 0.99f);
    }

    [Fact]
    public void ConvertsWorldTargetToRootLocalGeometryAndReportsWorldEndpoint()
    {
        var (controller, _, _) = CreateController();
        var anchor = new Vector2(10f, 5f);
        var target = new Vector2(11f, 6f);

        var step = controller.Advance(anchor, target, 1f, MathF.PI, MathF.PI);

        Assert.Equal(StatefulPlanarIkStatus.Reached, step.Status);
        Assert.InRange(Vector2.Distance(step.EndEffector, target), 0f, 1e-4f);
        Assert.InRange(step.ErrorDistance, 0f, 1e-4f);
    }

    [Fact]
    public void RejectsIncorrectHierarchyAndInvalidStepInputs()
    {
        var root = CreateNode("root");
        var unrelated = CreateNode("unrelated");
        Assert.Throws<ArgumentException>(() => new StatefulPlanarIkController(
            root, unrelated, FullTurn, FullTurn));

        var elbow = CreateNode("elbow");
        root.AddChild(elbow);
        var controller = new StatefulPlanarIkController(root, elbow, FullTurn, FullTurn);

        Assert.Throws<ArgumentOutOfRangeException>(() => controller.Advance(
            Vector2.Zero, Vector2.One, -1f, 1f, 1f));
        Assert.Throws<ArgumentOutOfRangeException>(() => controller.Advance(
            Vector2.Zero, new Vector2(float.NaN, 0f), 1f, 1f, 1f));
    }

    private static (StatefulPlanarIkController Controller, StatefulJointNode Shoulder, StatefulJointNode Elbow)
        CreateController()
    {
        var shoulder = CreateNode("shoulder");
        var elbow = CreateNode("elbow");
        shoulder.AddChild(elbow);
        return (new StatefulPlanarIkController(shoulder, elbow, FullTurn, FullTurn), shoulder, elbow);
    }

    private static StatefulJointNode CreateNode(string name) =>
        new(name, 1f, new StatefulHinge(0f, -MathF.PI, MathF.PI));
}
