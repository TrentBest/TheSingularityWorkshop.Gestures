using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// A deliberately feline, test-only articulated fixture. This is not an ontology
/// loader or a canonical armature schema; it proves that Gestures can evaluate
/// species-specific transform roles without assuming a humanoid skeleton.
/// </summary>
public sealed class FelineGestureFixtureTests
{
    [Fact]
    public void CatStalkUsesFelineArticulationAndEvaluatesDeterministically()
    {
        var gesture = CreateCatStalk();

        Assert.Equal("cat.stalk", gesture.Id);
        Assert.Equal(3, gesture.Keyframes.Count);

        var roles = gesture.Keyframes[0].LocalTransforms.Keys.ToHashSet(StringComparer.Ordinal);
        Assert.Contains("spine", roles);
        Assert.Contains("neck", roles);
        Assert.Contains("head", roles);
        Assert.Contains("left_forepaw", roles);
        Assert.Contains("right_forepaw", roles);
        Assert.Contains("left_hindpaw", roles);
        Assert.Contains("right_hindpaw", roles);
        Assert.Contains("tail_01", roles);
        Assert.Contains("tail_02", roles);
        Assert.DoesNotContain("left_hand", roles);
        Assert.DoesNotContain("right_foot", roles);

        var first = GestureEvaluator.Evaluate(gesture, 0.375d, 1d);
        var second = GestureEvaluator.Evaluate(gesture, 0.375d, 1d);

        Assert.Equal(first.NormalizedProgress, second.NormalizedProgress);
        Assert.Equal(first.LocalTransforms["head"], second.LocalTransforms["head"]);
        Assert.Equal(first.LocalTransforms["left_forepaw"], second.LocalTransforms["left_forepaw"]);
        Assert.Equal(first.LocalTransforms["tail_02"], second.LocalTransforms["tail_02"]);
    }

    private static GestureDefinition CreateCatStalk()
    {
        static PoseTransform At(float x, float y, float z, float pitch = 0f) =>
            new(new Vector3(x, y, z),
                Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch),
                Vector3.One);

        static GestureKeyframe Frame(double time, params (string Name, PoseTransform Transform)[] values) =>
            new(time, values.ToDictionary(value => value.Name, value => value.Transform, StringComparer.Ordinal));

        return new GestureDefinition(
            "cat.stalk",
            [
                Frame(0d,
                    ("root", At(0f, 0f, 0f)),
                    ("pelvis", At(0f, 0.32f, -0.18f)),
                    ("spine", At(0f, 0.38f, 0f)),
                    ("neck", At(0f, 0.48f, 0.18f)),
                    ("head", At(0f, 0.52f, 0.30f, -0.04f)),
                    ("left_forepaw", At(-0.16f, 0.02f, 0.24f)),
                    ("right_forepaw", At(0.16f, 0.02f, -0.08f)),
                    ("left_hindpaw", At(-0.14f, 0.02f, -0.34f)),
                    ("right_hindpaw", At(0.14f, 0.02f, -0.58f)),
                    ("tail_01", At(0f, 0.42f, -0.34f, 0.12f)),
                    ("tail_02", At(0f, 0.55f, -0.58f, 0.24f))),
                Frame(0.5d,
                    ("root", At(0f, 0f, 0.22f)),
                    ("pelvis", At(0f, 0.31f, 0.02f)),
                    ("spine", At(0f, 0.37f, 0.20f, -0.03f)),
                    ("neck", At(0f, 0.47f, 0.38f)),
                    ("head", At(0f, 0.51f, 0.50f, -0.02f)),
                    ("left_forepaw", At(-0.16f, 0.02f, 0.48f)),
                    ("right_forepaw", At(0.16f, 0.02f, 0.16f)),
                    ("left_hindpaw", At(-0.14f, 0.02f, -0.12f)),
                    ("right_hindpaw", At(0.14f, 0.02f, -0.34f)),
                    ("tail_01", At(0f, 0.43f, -0.12f, 0.16f)),
                    ("tail_02", At(0f, 0.57f, -0.34f, 0.30f))),
                Frame(1d,
                    ("root", At(0f, 0f, 0.44f)),
                    ("pelvis", At(0f, 0.32f, 0.26f)),
                    ("spine", At(0f, 0.38f, 0.44f)),
                    ("neck", At(0f, 0.48f, 0.62f, 0.02f)),
                    ("head", At(0f, 0.52f, 0.74f, 0.04f)),
                    ("left_forepaw", At(-0.16f, 0.02f, 0.70f)),
                    ("right_forepaw", At(0.16f, 0.02f, 0.48f)),
                    ("left_hindpaw", At(-0.14f, 0.02f, 0.10f)),
                    ("right_hindpaw", At(0.14f, 0.02f, -0.12f)),
                    ("tail_01", At(0f, 0.44f, 0.10f, 0.20f)),
                    ("tail_02", At(0f, 0.59f, -0.12f, 0.36f)))
            ]);
    }
}
