using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Small, illustrative provider-independent pose sequences used to start the starport experience.
/// These are authored CPU reference inputs, not solved IK, contact, collision, or biomechanics.
/// </summary>
public static class StarportGestureCatalog
{
    /// <summary>
    /// Creates a boarding-ramp pose sequence with semantic anchors for the craft provider to resolve.
    /// Coordinates are local illustrative units; the provider defines world units and anchor transforms.
    /// </summary>
    public static GestureDefinition CreateBoardingRampGesture() => new(
        "starport.flight-crew.board-ramp.v1",
        [
            Frame(0d,
                ("root", At(0f, 0f, 0f)),
                ("left-hand", At(0.35f, 1.15f, 0.1f)),
                ("right-hand", At(-0.35f, 1.15f, 0.1f)),
                ("left-foot", At(-0.2f, 0f, 0f)),
                ("right-foot", At(0.2f, 0f, -0.2f))),
            Frame(0.5d,
                ("root", At(0f, 0.55f, 0.35f)),
                ("left-hand", At(0.35f, 1.4f, 0.3f)),
                ("right-hand", At(-0.35f, 1.4f, 0.3f)),
                ("left-foot", At(-0.2f, 0.25f, 0.2f)),
                ("right-foot", At(0.2f, 0.05f, 0f))),
            Frame(1d,
                ("root", At(0f, 1.1f, 0.8f)),
                ("left-hand", At(0.3f, 1.45f, 0.75f)),
                ("right-hand", At(-0.3f, 1.45f, 0.75f)),
                ("left-foot", At(-0.18f, 0.9f, 0.8f)),
                ("right-foot", At(0.18f, 0.85f, 0.65f)))
        ],
        [
            new GestureAnchor("ramp-rail-left", "craft-ramp", At(0.35f, 1.2f, 0.1f)),
            new GestureAnchor("ramp-rail-right", "craft-ramp", At(-0.35f, 1.2f, 0.1f)),
            new GestureAnchor("hatch-threshold", "craft-access-door", At(0f, 1.1f, 0.8f)),
            new GestureAnchor("interior-entry", "craft-interior", At(0f, 1.1f, 1.1f))
        ]);

    /// <summary>
    /// Creates a service-umbilical handling pose sequence with provider-owned connector and stow anchors.
    /// The sequence does not prove that a connector released or that the cable is physically stowed.
    /// </summary>
    public static GestureDefinition CreateServiceUmbilicalGesture() => new(
        "starport.ground-crew.stow-umbilical.v1",
        [
            Frame(0d,
                ("root", At(0f, 0f, 0f)),
                ("left-hand", At(0.3f, 1f, 0.45f)),
                ("right-hand", At(-0.3f, 1f, 0.45f)),
                ("tool", At(0f, 0.7f, 0.4f))),
            Frame(0.5d,
                ("root", At(0f, 0f, 0.05f)),
                ("left-hand", At(0.25f, 0.9f, 0.65f)),
                ("right-hand", At(-0.25f, 0.9f, 0.65f)),
                ("tool", At(0f, 0.75f, 0.7f))),
            Frame(1d,
                ("root", At(0f, 0f, -0.15f)),
                ("left-hand", At(0.25f, 0.8f, 0.1f)),
                ("right-hand", At(-0.25f, 0.8f, 0.1f)),
                ("tool", At(0f, 0.5f, 0.05f)))
        ],
        [
            new GestureAnchor("service-connector", "craft-service-port", At(0f, 0.7f, 0.4f)),
            new GestureAnchor("umbilical-stow-point", "ground-service-rack", At(0f, 0.5f, 0.05f))
        ]);

    /// <summary>Creates a ground-crew cover-handling pose sequence with a craft-provided cover anchor.</summary>
    public static GestureDefinition CreateServiceCoverGesture() => new(
        "starport.ground-crew.secure-service-cover.v1",
        [
            Frame(0d,
                ("root", At(0f, 0f, 0f)),
                ("left-hand", At(0.3f, 1.1f, 0.3f)),
                ("right-hand", At(-0.3f, 1.1f, 0.3f)),
                ("cover", At(0f, 1.2f, 0.45f))),
            Frame(0.5d,
                ("root", At(0f, 0f, 0.1f)),
                ("left-hand", At(0.25f, 0.95f, 0.55f)),
                ("right-hand", At(-0.25f, 0.95f, 0.55f)),
                ("cover", At(0f, 1f, 0.6f))),
            Frame(1d,
                ("root", At(0f, 0f, -0.1f)),
                ("left-hand", At(0.25f, 0.8f, 0.25f)),
                ("right-hand", At(-0.25f, 0.8f, 0.25f)),
                ("cover", At(0f, 0.85f, 0.2f)))
        ],
        [
            new GestureAnchor("cover-handle-left", "service-cover", At(0.3f, 1.1f, 0.3f)),
            new GestureAnchor("cover-handle-right", "service-cover", At(-0.3f, 1.1f, 0.3f)),
            new GestureAnchor("cover-storage-or-lock-point", "craft-service-panel", At(0f, 0.85f, 0.2f))
        ]);

    private static GestureKeyframe Frame(
        double time,
        params (string Name, PoseTransform Transform)[] transforms) =>
        new(time, transforms.ToDictionary(item => item.Name, item => item.Transform, StringComparer.Ordinal));

    private static PoseTransform At(float x, float y, float z) =>
        new(new Vector3(x, y, z), Quaternion.Identity, Vector3.One);
}
