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
    public void IncrementalEvaluationRecomputesOnlyAffectedPathAndMatchesFullEvaluation()
    {
        var rootHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var childHinge = new StatefulHinge(MathF.PI / 2f, -MathF.PI, MathF.PI);
        var leafHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var root = new StatefulJointNode("root", 2f, rootHinge);
        var child = new StatefulJointNode("child", 1f, childHinge);
        var leaf = new StatefulJointNode("leaf", 1f, leafHinge);
        root.AddChild(child);
        child.AddChild(leaf);
        var anchor = new Vector2(10f, 0f);

        Assert.Equal(3, root.EvaluateIncremental(anchor));
        Assert.Equal(0, root.EvaluateIncremental(anchor));

        childHinge.Advance(MathF.PI / 4f, 1f, MathF.PI / 4f);
        Assert.Equal(2, root.EvaluateIncremental(anchor));
        AssertNear(new Vector2(12f, 0f), root.WorldEnd);
        AssertNear(new Vector2(12f + MathF.Sqrt(0.5f), MathF.Sqrt(0.5f)), child.WorldEnd);
        AssertNear(new Vector2(12f + MathF.Sqrt(2f), MathF.Sqrt(2f)), leaf.WorldEnd);

        var incrementalPoses = new[]
        {
            (root.WorldStart, root.WorldEnd, root.WorldAngleRadians),
            (child.WorldStart, child.WorldEnd, child.WorldAngleRadians),
            (leaf.WorldStart, leaf.WorldEnd, leaf.WorldAngleRadians)
        };
        root.EvaluateFull(anchor);
        var fullPoses = new[]
        {
            (root.WorldStart, root.WorldEnd, root.WorldAngleRadians),
            (child.WorldStart, child.WorldEnd, child.WorldAngleRadians),
            (leaf.WorldStart, leaf.WorldEnd, leaf.WorldAngleRadians)
        };

        for (var i = 0; i < incrementalPoses.Length; i++)
        {
            AssertNear(incrementalPoses[i].Item1, fullPoses[i].Item1);
            AssertNear(incrementalPoses[i].Item2, fullPoses[i].Item2);
            Assert.InRange(MathF.Abs(incrementalPoses[i].Item3 - fullPoses[i].Item3), 0f, 1e-5f);
        }
    }

    [Fact]
    public void IncrementalEvaluationLeavesUnaffectedSiblingSubtreeUnrecalculated()
    {
        var root = CreateNode("root");
        var leftHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var rightHinge = new StatefulHinge(0f, -MathF.PI, MathF.PI);
        var left = new StatefulJointNode("left", 1f, leftHinge);
        var right = new StatefulJointNode("right", 1f, rightHinge);
        var leftLeaf = CreateNode("left-leaf");
        var rightLeaf = CreateNode("right-leaf");
        root.AddChild(left);
        root.AddChild(right);
        left.AddChild(leftLeaf);
        right.AddChild(rightLeaf);
        var anchor = Vector2.Zero;

        Assert.Equal(5, root.EvaluateIncremental(anchor));
        var rightEndBefore = right.WorldEnd;
        var rightLeafEndBefore = rightLeaf.WorldEnd;

        leftHinge.Advance(MathF.PI / 2f, 1f, MathF.PI / 2f);
        Assert.Equal(2, root.EvaluateIncremental(anchor));

        Assert.Equal(rightEndBefore, right.WorldEnd);
        Assert.Equal(rightLeafEndBefore, rightLeaf.WorldEnd);
        AssertNear(new Vector2(1f, 1f), left.WorldEnd);
        AssertNear(new Vector2(1f, 2f), leftLeaf.WorldEnd);
    }

    [Fact]
    public void IncrementalEvaluationRecalculatesDescendantsWhenRootAnchorMoves()
    {
        var root = CreateNode("root");
        var child = CreateNode("child");
        root.AddChild(child);

        Assert.Equal(2, root.EvaluateIncremental(Vector2.Zero));
        Assert.Equal(2, root.EvaluateIncremental(new Vector2(3f, 4f)));
        AssertNear(new Vector2(4f, 4f), root.WorldEnd);
        AssertNear(new Vector2(5f, 4f), child.WorldEnd);
    }

    [Fact]
    public void IncrementalEvaluationFindsChildAddedAfterInitialEvaluation()
    {
        var root = CreateNode("root");
        Assert.Equal(1, root.EvaluateIncremental(Vector2.Zero));

        var child = CreateNode("child");
        root.AddChild(child);

        Assert.Equal(1, root.EvaluateIncremental(Vector2.Zero));
        Assert.True(child.HasEvaluated);
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
