using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class StatefulJointNodeGeneratedParityTests
{
    [Fact]
    public void IncrementalEvaluationMatchesFullEvaluationAcrossGeneratedBranchingEvents()
    {
        const int nodeCount = 127;
        const int eventCount = 256;
        var nodes = new StatefulJointNode[nodeCount];
        var hinges = new StatefulHinge[nodeCount];

        for (var i = 0; i < nodeCount; i++)
        {
            var initialAngle = ((i % 9) - 4) * 0.05f;
            hinges[i] = new StatefulHinge(initialAngle, -1f, 1f);
            nodes[i] = new StatefulJointNode(
                $"joint-{i}",
                0.5f + (i % 5) * 0.1f,
                hinges[i]);
            if (i > 0)
                nodes[(i - 1) / 2].AddChild(nodes[i]);
        }

        var root = nodes[0];
        var anchor = new Vector2(3f, -2f);
        Assert.Equal(nodeCount, root.EvaluateIncremental(anchor));

        // Use an explicit integer generator so this test's event sequence is independent of runtime RNG details.
        uint state = 0xC0FFEEu;
        for (var eventIndex = 0; eventIndex < eventCount; eventIndex++)
        {
            var selectedIndex = (int)(Next(ref state) % nodeCount);
            var target = NextSignedUnit(ref state) * 0.95f;
            hinges[selectedIndex].Advance(target, 1f, 2f);

            if (eventIndex % 11 == 10)
            {
                var x = NextSignedUnit(ref state) * 10f;
                var y = NextSignedUnit(ref state) * 10f;
                anchor = new Vector2(x, y);
            }

            var recalculated = root.EvaluateIncremental(anchor);
            Assert.InRange(recalculated, 0, nodeCount);

            var incrementalPoses = nodes
                .Select(node => (node.WorldStart, node.WorldEnd, node.WorldAngleRadians))
                .ToArray();

            root.EvaluateFull(anchor);
            for (var nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                AssertNear(incrementalPoses[nodeIndex].WorldStart, nodes[nodeIndex].WorldStart);
                AssertNear(incrementalPoses[nodeIndex].WorldEnd, nodes[nodeIndex].WorldEnd);
                Assert.InRange(
                    MathF.Abs(incrementalPoses[nodeIndex].WorldAngleRadians - nodes[nodeIndex].WorldAngleRadians),
                    0f,
                    1e-5f);
            }
        }
    }

    private static uint Next(ref uint state)
    {
        state = unchecked(state * 1664525u + 1013904223u);
        return state;
    }

    private static float NextSignedUnit(ref uint state)
    {
        var fraction = (Next(ref state) >> 8) / 16777215f;
        return fraction * 2f - 1f;
    }

    private static void AssertNear(Vector2 expected, Vector2 actual)
    {
        Assert.InRange(Vector2.Distance(expected, actual), 0f, 1e-4f);
    }
}
