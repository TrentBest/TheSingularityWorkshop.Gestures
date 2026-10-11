namespace TheSingularityWorkshop.Gestures;

/// <summary>Data describing an ideal physical procedure, independent of simulation and presentation.</summary>
public sealed class GestureDefinition
{
    /// <summary>Creates a Gesture with normalized keyframes and optional semantic anchors.</summary>
    public GestureDefinition(string id, IEnumerable<GestureKeyframe> keyframes,
        IEnumerable<GestureAnchor>? anchors = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Gesture id must not be empty.", nameof(id));
        ArgumentNullException.ThrowIfNull(keyframes);
        Id = id;
        Keyframes = keyframes.ToArray();
        if (Keyframes.Count < 2)
            throw new ArgumentException("A Gesture requires at least two keyframes.", nameof(keyframes));
        if (Keyframes[0].Time != 0d || Keyframes[^1].Time != 1d)
            throw new ArgumentException("Gesture keyframes must begin at time 0 and end at time 1.", nameof(keyframes));
        for (var i = 1; i < Keyframes.Count; i++)
            if (Keyframes[i - 1].Time >= Keyframes[i].Time)
                throw new ArgumentException("Keyframe times must be strictly increasing.", nameof(keyframes));

        Anchors = anchors?.ToArray() ?? [];
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var anchor in Anchors)
        {
            ArgumentNullException.ThrowIfNull(anchor);
            anchor.Validate();
            if (!names.Add(anchor.Name))
                throw new ArgumentException($"Duplicate anchor name '{anchor.Name}'.", nameof(anchors));
        }
    }

    /// <summary>Stable semantic identifier for this Gesture definition.</summary>
    public string Id { get; }
    /// <summary>Ordered keyframes from progression 0 to 1.</summary>
    public IReadOnlyList<GestureKeyframe> Keyframes { get; }
    /// <summary>Provider-defined interaction anchors used by consumers to align the procedure.</summary>
    public IReadOnlyList<GestureAnchor> Anchors { get; }
}
