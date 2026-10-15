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
