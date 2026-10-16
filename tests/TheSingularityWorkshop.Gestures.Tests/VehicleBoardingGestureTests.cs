using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

/// <summary>
/// Example boarding procedure whose transform names and interaction anchors are
/// provider-defined. The caller owns vehicle motion, collision, and boarding success.
/// </summary>
public sealed class VehicleBoardingGestureTests
{
    [Fact]
    public void BoardingGestureExposesSemanticAnchorsWithoutRequiringAVehicleRig()
    {
        var gesture = CreateVehicleBoarding();

        Assert.Equal("vehicle.board", gesture.Id);
        Assert.Equal(3, gesture.Keyframes.Count);
        Assert.Contains("root", gesture.Keyframes[0].LocalTransforms.Keys);
        Assert.Contains("right_hand", gesture.Keyframes[1].LocalTransforms.Keys);
        Assert.Contains("pelvis", gesture.Keyframes[2].LocalTransforms.Keys);
        Assert.Equal("right_hand", gesture.Anchors.Single(a => a.Name == "entry-handhold").ParentJoint);
        Assert.Equal("left_hand", gesture.Anchors.Single(a => a.Name == "vehicle-grip").ParentJoint);
        Assert.Equal("pelvis", gesture.Anchors.Single(a => a.Name == "seat-target").ParentJoint);
    }

    [Fact]
    public void BoardingGestureEvaluatesRepeatablyAtAuthoredProgress()
    {
        var gesture = CreateVehicleBoarding();

        var first = GestureEvaluator.Evaluate(gesture, 0.65, 1.0);
        var second = GestureEvaluator.Evaluate(gesture, 0.65, 1.0);

        Assert.Equal(first.NormalizedProgress, second.NormalizedProgress);
        Assert.Equal(first.LocalTransforms["right_hand"], second.LocalTransforms["right_hand"]);
        Assert.Equal(first.LocalTransforms["pelvis"], second.LocalTransforms["pelvis"]);
        Assert.False(first.IsComplete);
    }

    [Fact]
    public void BoardingPoseCompletionDoesNotClaimPhysicalBoardingSuccess()
    {
        var gesture = CreateVehicleBoarding();

        var pose = GestureEvaluator.Evaluate(gesture, 1.0, 1.0);

        Assert.True(pose.IsComplete);
        // GestureEvaluator reports progression only. Vehicle occupancy, collisions,
        // hand contact, and successful seating are owned by the calling domain.
        Assert.Contains(gesture.Anchors, anchor => anchor.Name == "seat-target");
    }

    private static GestureDefinition CreateVehicleBoarding()
    {
        static PoseTransform At(float x, float y, float z) =>
            new(new Vector3(x, y, z), Quaternion.Identity, Vector3.One);

        static GestureKeyframe Frame(double time, params (string Name, PoseTransform Transform)[] values) =>
            new(time, values.ToDictionary(value => value.Name, value => value.Transform, StringComparer.Ordinal));

        return new GestureDefinition(
            "vehicle.board",
            [
                Frame(0.0,
                    ("root", At(0, 0, 0)),
                    ("pelvis", At(0, 0.9f, 0)),
                    ("left_hand", At(-0.25f, 1.1f, 0)),
                    ("right_hand", At(0.25f, 1.1f, 0)),
                    ("left_foot", At(-0.12f, 0, 0)),
                    ("right_foot", At(0.12f, 0, 0))),
                Frame(0.5,
                    ("root", At(0, 0.2f, 0.15f)),
                    ("pelvis", At(0, 1.05f, 0.2f)),
                    ("left_hand", At(-0.3f, 1.25f, 0.25f)),
                    ("right_hand", At(0.25f, 1.35f, 0.3f)),
                    ("left_foot", At(-0.12f, 0.2f, 0.15f)),
                    ("right_foot", At(0.12f, 0.05f, 0.25f))),
                Frame(1.0,
                    ("root", At(0, 0.35f, 0.55f)),
                    ("pelvis", At(0, 0.65f, 0.7f)),
                    ("left_hand", At(-0.25f, 0.9f, 0.6f)),
                    ("right_hand", At(0.25f, 0.9f, 0.6f)),
                    ("left_foot", At(-0.12f, 0.1f, 0.7f)),
                    ("right_foot", At(0.12f, 0.1f, 0.7f)))
            ],
            [
                new GestureAnchor("entry-handhold", "right_hand", At(0, 0, 0)),
                new GestureAnchor("vehicle-grip", "left_hand", At(0, 0, 0)),
                new GestureAnchor("seat-target", "pelvis", At(0, 0, 0))
            ]);
    }
}
