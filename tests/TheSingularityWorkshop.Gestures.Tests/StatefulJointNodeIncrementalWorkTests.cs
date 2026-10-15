using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulJointNodeIncrementalWorkTests
{
    [Fact]
    public void UnchangedHierarchyPerformsNoTransformRecalculations()
    {
        var root = BuildBranchingHierarchy(out _);
        Assert.Equal(7, root.EvaluateIncremental(Vector2.Zero));

        Assert.Equal(0, root.EvaluateIncremental(Vector2.Zero));
    }

    [Fact]
    public void LeafHingeChangeRecalculatesOnlyTheLeafTransform()
    {
        var root = BuildBranchingHierarchy(out var hinges);
        root.EvaluateIncremental(Vector2.Zero);

        // A leaf's local angle changes its own endpoint and orientation, not its ancestors.
        hinges[6].Advance(0.5f, 1f, 1f);

        Assert.Equal(1, root.EvaluateIncremental(Vector2.Zero));
        Assert.Equal(0, root.EvaluateIncremental(Vector2.Zero));
    }

    [Fact]
    public void RootAnchorChangeRecalculatesEveryNodeWhoseWorldPositionMoves()
    {
        var root = BuildBranchingHierarchy(out _);
        root.EvaluateIncremental(Vector2.Zero);

        Assert.Equal(7, root.EvaluateIncremental(new Vector2(2f, -3f)));
    }


    [Fact]
    public void InternalHingeChangeRecalculatesItsDescendantsButNotSiblingBranches()
    {
        var root = BuildBranchingHierarchy(out var hinges);
        root.EvaluateIncremental(Vector2.Zero);

        var unaffectedSibling = root.Children[1];
        var siblingStartBefore = unaffectedSibling.WorldStart;
        var siblingEndBefore = unaffectedSibling.WorldEnd;
        var affectedBranch = root.Children[0];
        var affectedGrandchildEndBefore = affectedBranch.Children[0].WorldEnd;

        hinges[1].Advance(0.5f, 1f, 1f);

        // The changed joint and its two descendants change; the other root branch does not.
        Assert.Equal(3, root.EvaluateIncremental(Vector2.Zero));
        Assert.Equal(siblingStartBefore, unaffectedSibling.WorldStart);
        Assert.Equal(siblingEndBefore, unaffectedSibling.WorldEnd);
        Assert.NotEqual(affectedGrandchildEndBefore, affectedBranch.Children[0].WorldEnd);

        var incrementalEnds = CaptureEnds(root);
        root.EvaluateFull(Vector2.Zero);
        Assert.Equal(incrementalEnds, CaptureEnds(root));
        Assert.Equal(0, root.EvaluateIncremental(Vector2.Zero));
    }

    private static Vector2[] CaptureEnds(StatefulJointNode root)
    {
        var ends = new List<Vector2>();
        CaptureEnds(root, ends);
        return ends.ToArray();
    }

    private static void CaptureEnds(StatefulJointNode node, List<Vector2> ends)
    {
        ends.Add(node.WorldEnd);
        foreach (var child in node.Children)
            CaptureEnds(child, ends);
    }

    private static StatefulJointNode BuildBranchingHierarchy(out StatefulHinge[] hinges)
    {
        var nodes = new StatefulJointNode[7];
        hinges = new StatefulHinge[nodes.Length];

        for (var i = 0; i < nodes.Length; i++)
        {
            hinges[i] = new StatefulHinge(0f, -1f, 1f);
            nodes[i] = new StatefulJointNode($"joint-{i}", 1f, hinges[i]);
            if (i > 0)
                nodes[(i - 1) / 2].AddChild(nodes[i]);
        }

        return nodes[0];
    }
}
