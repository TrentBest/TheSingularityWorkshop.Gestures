namespace TheSingularityWorkshop.Gestures;

/// <summary>A named attachment or interaction point relative to a provider-owned transform.</summary>
public sealed record GestureAnchor(string Name, string ParentJoint, PoseTransform LocalTransform)
{
    /// <summary>Validates required identifiers.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Anchor name must not be empty.", nameof(Name));
        if (string.IsNullOrWhiteSpace(ParentJoint))
            throw new ArgumentException("Anchor parent joint must not be empty.", nameof(ParentJoint));
    }
}
