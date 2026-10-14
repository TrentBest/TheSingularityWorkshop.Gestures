using System.Numerics;

namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A planar articulated link with one retained local hinge angle and zero or more child links.
/// Local angles are relative to the parent link's world angle; each link extends along its local +X axis.
/// </summary>
/// <remarks>
/// This research prototype supports full reevaluation and dirty-path incremental transform updates.
/// It does not perform collision detection, physical integration, contact evaluation, or parallel/GPU processing.
/// </remarks>
public sealed class StatefulJointNode
{
    private readonly List<StatefulJointNode> _children = [];
    private readonly IReadOnlyList<StatefulJointNode> _readOnlyChildren;
    private bool _isDirty = true;
    private float _lastEvaluatedLocalAngle;
    private float _lastParentWorldAngle;

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
        Hinge.AngleChanged += MarkAncestorsDirty;
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

    /// <summary>World-space start position from the most recent hierarchy evaluation.</summary>
    public Vector2 WorldStart { get; private set; }

    /// <summary>World-space endpoint from the most recent hierarchy evaluation.</summary>
    public Vector2 WorldEnd { get; private set; }

    /// <summary>Accumulated world angle from the most recent hierarchy evaluation.</summary>
    public float WorldAngleRadians { get; private set; }

    /// <summary>Whether world-space output has been calculated by a hierarchy evaluation.</summary>
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
        MarkAncestorsDirty();
    }

    /// <summary>
    /// Fully evaluates this hierarchy from a world-space root anchor. Only a root node may start evaluation.
    /// Every descendant is recalculated, providing the correctness baseline for incremental evaluation.
    /// </summary>
    public void EvaluateFull(Vector2 rootAnchor)
    {
        ValidateRootAndAnchor(rootAnchor);
        try
        {
            EvaluateDescendantsFull(rootAnchor, 0f);
        }
        catch
        {
            MarkSubtreeDirty();
            MarkAncestorsDirty();
            throw;
        }
    }

    /// <summary>
    /// Recalculates only nodes whose local angle or incoming parent transform changed, plus affected descendants.
    /// Returns the number of nodes whose world transforms were recalculated; an unchanged hierarchy returns zero.
    /// </summary>
    public int EvaluateIncremental(Vector2 rootAnchor)
    {
        ValidateRootAndAnchor(rootAnchor);
        try
        {
            return EvaluateDescendantsIncremental(rootAnchor, 0f);
        }
        catch
        {
            MarkSubtreeDirty();
            MarkAncestorsDirty();
            throw;
        }
    }

    private void ValidateRootAndAnchor(Vector2 rootAnchor)
    {
        if (Parent is not null)
            throw new InvalidOperationException("Hierarchy evaluation must begin at the root node.");
        if (!float.IsFinite(rootAnchor.X) || !float.IsFinite(rootAnchor.Y))
            throw new ArgumentOutOfRangeException(nameof(rootAnchor), "Anchor coordinates must be finite.");
    }

    private void EvaluateDescendantsFull(Vector2 start, float parentWorldAngleRadians)
    {
        CalculateWorldPose(start, parentWorldAngleRadians);
        foreach (var child in _children)
            child.EvaluateDescendantsFull(WorldEnd, WorldAngleRadians);
    }

    private int EvaluateDescendantsIncremental(Vector2 start, float parentWorldAngleRadians)
    {
        var wasDirty = _isDirty;
        var needsOwnEvaluation = !HasEvaluated
            || start != WorldStart
            || parentWorldAngleRadians != _lastParentWorldAngle
            || Hinge.AngleRadians != _lastEvaluatedLocalAngle;

        if (!wasDirty && !needsOwnEvaluation)
            return 0;

        var recalculated = 0;
        if (needsOwnEvaluation)
        {
            CalculateWorldPose(start, parentWorldAngleRadians);
            recalculated = 1;
        }
        else
        {
            _isDirty = false;
        }

        foreach (var child in _children)
            recalculated += child.EvaluateDescendantsIncremental(WorldEnd, WorldAngleRadians);

        return recalculated;
    }

    private void CalculateWorldPose(Vector2 start, float parentWorldAngleRadians)
    {
        WorldStart = start;
        _lastParentWorldAngle = parentWorldAngleRadians;
        _lastEvaluatedLocalAngle = Hinge.AngleRadians;
        WorldAngleRadians = parentWorldAngleRadians + Hinge.AngleRadians;
        if (!float.IsFinite(WorldAngleRadians))
            throw new InvalidOperationException("Accumulated world angle is not finite.");

        WorldEnd = start + new Vector2(
            LinkLength * MathF.Cos(WorldAngleRadians),
            LinkLength * MathF.Sin(WorldAngleRadians));
        if (!float.IsFinite(WorldEnd.X) || !float.IsFinite(WorldEnd.Y))
            throw new InvalidOperationException("Evaluated world position is not finite.");

        HasEvaluated = true;
        _isDirty = false;
    }

    private void MarkAncestorsDirty()
    {
        for (StatefulJointNode? node = this; node is not null; node = node.Parent)
            node._isDirty = true;
    }

    private void MarkSubtreeDirty()
    {
        _isDirty = true;
        foreach (var child in _children)
            child.MarkSubtreeDirty();
    }
}
