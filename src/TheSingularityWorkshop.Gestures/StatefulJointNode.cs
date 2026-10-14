using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A planar articulated link with one retained local hinge angle and zero or more child links.
/// Local angles are relative to the parent link's world angle; each link extends along its local +X axis.
/// </summary>
/// <remarks>
/// This is a full-reevaluation correctness prototype. It does not perform incremental invalidation,
/// collision detection, physical integration, contact evaluation, or parallel/GPU processing.
/// </remarks>
public sealed class StatefulJointNode
{
    private readonly List<StatefulJointNode> _children = [];
    private readonly IReadOnlyList<StatefulJointNode> _readOnlyChildren;

    /// <summary>Creates a joint node with a stable diagnostic name, link length, and retained hinge controller.</summary>
    public StatefulJointNode(string name, float linkLength, StatefulHinge hinge)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Joint name must not be empty.", nameof(name));
        if (!float.IsFinite(linkLength) || linkLength <= 0f)
            throw new ArgumentOutOfRangeException(nameof(linkLength), "Link length must be finite and positive.");
        ArgumentNullException.ThrowIfNull(hinge);

        Name = name;
        LinkLength = linkLength;
        Hinge = hinge;
        _readOnlyChildren = _children.AsReadOnly();
    }

    /// <summary>Diagnostic name for this node.</summary>
    public string Name { get; }

    /// <summary>Length of this planar link.</summary>
    public float LinkLength { get; }

    /// <summary>Retained controller for this joint's local angle.</summary>
    public StatefulHinge Hinge { get; }

    /// <summary>Parent node, or null for a hierarchy root.</summary>
    public StatefulJointNode? Parent { get; private set; }

    /// <summary>Child links evaluated from this link's endpoint.</summary>
    public IReadOnlyList<StatefulJointNode> Children => _readOnlyChildren;

    /// <summary>World-space start position from the most recent full evaluation.</summary>
    public Vector2 WorldStart { get; private set; }

    /// <summary>World-space endpoint from the most recent full evaluation.</summary>
    public Vector2 WorldEnd { get; private set; }

    /// <summary>Accumulated world angle from the most recent full evaluation.</summary>
    public float WorldAngleRadians { get; private set; }

    /// <summary>Whether world-space output has been calculated by a full evaluation.</summary>
    public bool HasEvaluated { get; private set; }

    /// <summary>Adds a child link, rejecting self-links, cycles, and nodes already attached elsewhere.</summary>
    public void AddChild(StatefulJointNode child)
    {
        ArgumentNullException.ThrowIfNull(child);
        if (ReferenceEquals(child, this))
            throw new ArgumentException("A joint cannot be its own child.", nameof(child));
        if (child.Parent is not null)
            throw new ArgumentException("The child already belongs to a hierarchy.", nameof(child));

        for (StatefulJointNode? ancestor = this; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor, child))
                throw new ArgumentException("Adding this child would create a hierarchy cycle.", nameof(child));
        }

        child.Parent = this;
        _children.Add(child);
    }

    /// <summary>
    /// Fully evaluates this hierarchy from a world-space root anchor. Only a root node may start evaluation.
    /// The method intentionally recalculates every descendant to provide a correctness baseline.
    /// </summary>
    public void EvaluateFull(Vector2 rootAnchor)
    {
        if (Parent is not null)
            throw new InvalidOperationException("Full hierarchy evaluation must begin at the root node.");
        if (!float.IsFinite(rootAnchor.X) || !float.IsFinite(rootAnchor.Y))
            throw new ArgumentOutOfRangeException(nameof(rootAnchor), "Anchor coordinates must be finite.");

        EvaluateDescendants(rootAnchor, 0f);
    }

    private void EvaluateDescendants(Vector2 start, float parentWorldAngleRadians)
    {
        WorldStart = start;
        WorldAngleRadians = parentWorldAngleRadians + Hinge.AngleRadians;
        if (!float.IsFinite(WorldAngleRadians))
            throw new InvalidOperationException("Accumulated world angle is not finite.");

        WorldEnd = start + new Vector2(
            LinkLength * MathF.Cos(WorldAngleRadians),
            LinkLength * MathF.Sin(WorldAngleRadians));
        if (!float.IsFinite(WorldEnd.X) || !float.IsFinite(WorldEnd.Y))
            throw new InvalidOperationException("Evaluated world position is not finite.");

        HasEvaluated = true;
        foreach (var child in _children)
            child.EvaluateDescendants(WorldEnd, WorldAngleRadians);
    }
}
