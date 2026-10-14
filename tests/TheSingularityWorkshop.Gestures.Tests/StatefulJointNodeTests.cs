using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulJointNodeTests
{
    [Fact]
    public void FullEvaluationPropagatesParentMotionToAllDescendants()
    {
        var rootHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var childHinge = new StatefulHinge(MathF.PI / 2f, -MathF.PI, MathF.PI);
        var leafHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var root = new StatefulJointNode("root", 2f, rootHinge);
        var child = new StatefulJointNode("child", 1f, childHinge);
        var leaf = new StatefulJointNode("leaf", 1f, leafHinge);
        root.AddChild(child);
        child.AddChild(leaf);

        root.EvaluateFull(new Vector2(10f, 0f));

        AssertNear(new Vector2(12f, 0f), root.WorldEnd);
        AssertNear(new Vector2(12f, 1f), child.WorldEnd);
        AssertNear(new Vector2(12f, 2f), leaf.WorldEnd);
        Assert.True(root.HasEvaluated);
        Assert.True(child.HasEvaluated);
        Assert.True(leaf.HasEvaluated);

        rootHinge.Advance(MathF.PI / 2f, 1f, MathF.PI / 2f);
        root.EvaluateFull(new Vector2(10f, 0f));

        AssertNear(new Vector2(10f, 2f), root.WorldEnd);
        AssertNear(new Vector2(9f, 2f), child.WorldEnd);
        AssertNear(new Vector2(8f, 2f), leaf.WorldEnd);
        Assert.InRange(MathF.Abs(child.WorldAngleRadians - MathF.PI), 0f, 1e-5f);
        Assert.InRange(MathF.Abs(leaf.WorldAngleRadians - MathF.PI), 0f, 1e-5f);
    }

    [Fact]
    public void RejectsCyclesAndChildrenAlreadyAttachedToAnotherParent()
    {
        var root = CreateNode("root");
        var child = CreateNode("child");
        var other = CreateNode("other");
        root.AddChild(child);

        Assert.Throws<ArgumentException>(() => root.AddChild(root));
        Assert.Throws<ArgumentException>(() => other.AddChild(child));
        Assert.Throws<ArgumentException>(() => child.AddChild(root));
    }

    [Fact]
    public void FullEvaluationMustStartAtRootAndUseFiniteAnchor()
    {
        var root = CreateNode("root");
        var child = CreateNode("child");
        root.AddChild(child);

        Assert.Throws<InvalidOperationException>(() => child.EvaluateFull(Vector2.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => root.EvaluateFull(new Vector2(float.NaN, 0f)));
    }

    [Fact]
    public void RejectsInvalidLinkLengthsAndNames()
    {
        var hinge = new StatefulHinge(0f, -1f, 1f);

        Assert.Throws<ArgumentException>(() => new StatefulJointNode(" ", 1f, hinge));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulJointNode("joint", 0f, hinge));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StatefulJointNode("joint", float.PositiveInfinity, hinge));
    }

    private static StatefulJointNode CreateNode(string name) =>
        new(name, 1f, new StatefulHinge(0f, -MathF.PI, MathF.PI));

    private static void AssertNear(Vector2 expected, Vector2 actual) =>
        Assert.InRange(Vector2.Distance(expected, actual), 0f, 1e-5f);
}
