using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// The kind of region or obstacle represented by a movement-course element.
/// Geometry describes the challenge; it does not prove that an agent has completed it.
/// </summary>
public enum MovementCourseElementKind
{
    ApproachZone,
    AccelerationZone,
    Hurdle,
    ClearanceZone,
    LandingZone,
    ContinuationZone,
    Gap,
    Vault,
    BalanceSection,
    Climb,
    Checkpoint,
    Finish
}

/// <summary>
/// An engine-neutral, axis-aligned course element. Position is the center and size is the
/// full extent on each axis, expressed in caller-defined consistent world units.
/// </summary>
public sealed record MovementCourseElement
{
    public MovementCourseElement(
        string id,
        MovementCourseElementKind kind,
        Vector3 center,
        Vector3 size)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Element ID must not be empty.", nameof(id));
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind), "Unknown course element kind.");
        if (!IsFinite(center))
            throw new ArgumentOutOfRangeException(nameof(center), "Center components must be finite.");
        if (!IsFinite(size) || size.X <= 0f || size.Y <= 0f || size.Z <= 0f)
            throw new ArgumentOutOfRangeException(nameof(size), "Size components must be finite and positive.");

        Id = id;
        Kind = kind;
        Center = center;
        Size = size;
    }

    /// <summary>Stable, course-local identifier.</summary>
    public string Id { get; }

    public MovementCourseElementKind Kind { get; }

    /// <summary>Center of the element's axis-aligned bounds.</summary>
    public Vector3 Center { get; }

    /// <summary>Full X/Y/Z extents in the course's declared world units.</summary>
    public Vector3 Size { get; }

    private static bool IsFinite(Vector3 value) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);
}

/// <summary>
/// An ordered, agent-independent movement challenge. The course stores authored geometry
/// only; agent planning and physics-confirmed completion belong to separate owners.
/// </summary>
/// <remarks>
/// Coordinates use consistent caller-defined units and an axis-aligned bounding-box convention.
/// This first contract does not define collision shapes, navigation, timing, scoring, serialization,
/// procedural generation, or physical success criteria.
/// </remarks>
public sealed class MovementCourse
{
    private readonly IReadOnlyList<MovementCourseElement> _elements;

    public MovementCourse(string id, IEnumerable<MovementCourseElement> elements, string units = "meters")
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Course ID must not be empty.", nameof(id));
        if (string.IsNullOrWhiteSpace(units))
            throw new ArgumentException("Units must not be empty.", nameof(units));
        ArgumentNullException.ThrowIfNull(elements);

        var copy = elements.ToArray();
        if (copy.Length == 0)
            throw new ArgumentException("A course must contain at least one element.", nameof(elements));
        if (copy.Any(element => element is null))
            throw new ArgumentException("Course elements must not contain null entries.", nameof(elements));

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var element in copy)
        {
            if (!ids.Add(element.Id))
                throw new ArgumentException($"Duplicate course element ID '{element.Id}'.", nameof(elements));
        }

        Id = id;
        Units = units;
        _elements = Array.AsReadOnly(copy);
    }

    /// <summary>Stable course identifier, suitable for authored or saved content.</summary>
    public string Id { get; }

    /// <summary>Label for the consistent units used by element geometry.</summary>
    public string Units { get; }

    /// <summary>Elements in authored traversal order. The collection cannot be mutated by consumers.</summary>
    public IReadOnlyList<MovementCourseElement> Elements => _elements;
}
