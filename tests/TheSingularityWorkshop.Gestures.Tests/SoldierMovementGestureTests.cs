using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// A rig-independent sample soldier procedure. The named transforms are an example
/// contract only; consumers map them to their own rig and own locomotion/physics.
/// </summary>
public sealed class SoldierMovementGestureTests
{
    [Fact]
    public void TacticalAdvanceHasStableRigIndependentTransformNames()
    {
        var gesture = CreateTacticalAdvance();

        Assert.Equal("soldier.tactical-advance", gesture.Id);
        Assert.Equal(3, gesture.Keyframes.Count);
        Assert.Contains("root", gesture.Keyframes[0].LocalTransforms.Keys);
        Assert.Contains("left_foot", gesture.Keyframes[1].LocalTransforms.Keys);
        Assert.Contains("right_foot", gesture.Keyframes[1].LocalTransforms.Keys);
        Assert.Contains("left_hand", gesture.Keyframes[2].LocalTransforms.Keys);
        Assert.Contains("right_hand", gesture.Keyframes[2].LocalTransforms.Keys);
    }

    [Fact]
    public void TacticalAdvanceEvaluatesDeterministicallyAtTheSameProgress()
    {
        var gesture = CreateTacticalAdvance();

        var first = GestureEvaluator.Evaluate(gesture, 0.5, 1.0);
        var second = GestureEvaluator.Evaluate(gesture, 0.5, 1.0);

        Assert.Equal(first.NormalizedProgress, second.NormalizedProgress);
        Assert.Equal(first.LocalTransforms["root"], second.LocalTransforms["root"]);
        Assert.Equal(first.LocalTransforms["left_foot"], second.LocalTransforms["left_foot"]);
    }

    [Fact]
    public void AnchorsRemainProviderDefinedRatherThanAssumingAUniversalRig()
    {
        var gesture = CreateTacticalAdvance();

        Assert.Contains(gesture.Anchors, anchor => anchor.Name == "weapon-support");
        Assert.Equal("left_hand", gesture.Anchors.Single(anchor => anchor.Name == "weapon-support").ParentJoint);
    }

    private static GestureDefinition CreateTacticalAdvance()
    {
        static PoseTransform At(float x, float y, float z) =>
            new(new Vector3(x, y, z), Quaternion.Identity, Vector3.One);

        static GestureKeyframe Frame(double time, params (string Name, PoseTransform Transform)[] values) =>
            new(time, values.ToDictionary(value => value.Name, value => value.Transform, StringComparer.Ordinal));

        return new GestureDefinition(
            "soldier.tactical-advance",
            [
                Frame(0.0,
                    ("root", At(0, 0, 0)),
                    ("pelvis", At(0, 0.9f, 0)),
                    ("left_foot", At(-0.12f, 0, 0)),
                    ("right_foot", At(0.12f, 0, 0)),
                    ("left_hand", At(-0.25f, 1.15f, 0.25f)),
                    ("right_hand", At(0.25f, 1.15f, 0.25f))),
                Frame(0.5,
                    ("root", At(0, 0, 0.45f)),
                    ("pelvis", At(0, 0.88f, 0.45f)),
                    ("left_foot", At(-0.12f, 0, 0.62f)),
                    ("right_foot", At(0.12f, 0, 0.28f)),
                    ("left_hand", At(-0.25f, 1.13f, 0.7f)),
                    ("right_hand", At(0.25f, 1.13f, 0.7f))),
                Frame(1.0,
                    ("root", At(0, 0, 0.9f)),
                    ("pelvis", At(0, 0.9f, 0.9f)),
                    ("left_foot", At(-0.12f, 0, 0.78f)),
                    ("right_foot", At(0.12f, 0, 1.02f)),
                    ("left_hand", At(-0.25f, 1.15f, 1.15f)),
                    ("right_hand", At(0.25f, 1.15f, 1.15f)))
            ],
            [
                new GestureAnchor("weapon-support", "left_hand", At(0, 0, 0)),
                new GestureAnchor("weapon-control", "right_hand", At(0, 0, 0))
            ]);
    }
}
