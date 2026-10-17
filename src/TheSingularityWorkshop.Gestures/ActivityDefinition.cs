namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// Immutable, versioned description of a reusable multi-step activity.
/// This definition is data only; it does not own a live session or execute actions.
/// </summary>
public sealed class ActivityDefinition
{
    internal ActivityDefinition(string id, int version, IReadOnlyList<string> classifications,
        IReadOnlyList<ActivityPhaseDefinition> phases)
    {
        Id = id;
        Version = version;
        Classifications = classifications;
        Phases = phases;
    }

    /// <summary>Stable identifier for this kind of activity; it is not a nine-layer ontology coordinate.</summary>
    public string Id { get; }

    /// <summary>Positive schema/content version selected by the author.</summary>
    public int Version { get; }

    /// <summary>Activity-level classification terms, ordered ordinally; these are not canonical ontology coordinates.</summary>
    public IReadOnlyList<string> Classifications { get; }

    /// <summary>Ordered phases that describe the activity's authored progression.</summary>
    public IReadOnlyList<ActivityPhaseDefinition> Phases { get; }
}

/// <summary>A named phase in an activity definition.</summary>
public sealed class ActivityPhaseDefinition
{
    internal ActivityPhaseDefinition(string id, IReadOnlyList<ActivityRoleSlot> roles,
        IReadOnlyList<string> gestureIds)
    {
        Id = id;
        Roles = roles;
        GestureIds = gestureIds;
    }

    /// <summary>Stable identifier unique within the activity.</summary>
    public string Id { get; }

    /// <summary>Role slots declared for this phase.</summary>
    public IReadOnlyList<ActivityRoleSlot> Roles { get; }

    /// <summary>Stable Gesture IDs referenced by this phase, in authored order.</summary>
    public IReadOnlyList<string> GestureIds { get; }
}

/// <summary>Participant-count bounds for one named role in a phase.</summary>
public sealed record ActivityRoleSlot(string Id, int MinimumParticipants, int MaximumParticipants);

/// <summary>
/// Fluent authoring entry point for immutable activity definitions.
/// Build validates structural invariants before returning the data model.
/// </summary>
public sealed class ActivityDefinitionBuilder
{
    private readonly string _id;
    private int _version = 1;
    private readonly HashSet<string> _classifications = new(StringComparer.Ordinal);
    private readonly List<ActivityPhaseDefinition> _phases = [];
    private PhaseBuilder? _activePhase;

    private ActivityDefinitionBuilder(string id)
    {
        _id = RequireName(id, nameof(id));
    }

    /// <summary>Starts authoring an activity with a stable activity identifier.</summary>
    public static ActivityDefinitionBuilder Define(string id) => new(id);

    /// <summary>Sets the positive content/schema version.</summary>
    public ActivityDefinitionBuilder Version(int version)
    {
        EnsureNoActivePhase();
        if (version < 1)
            throw new ArgumentOutOfRangeException(nameof(version), "Activity version must be positive.");
        _version = version;
        return this;
    }

    /// <summary>Adds activity-level classification terms without assigning canonical ontology coordinates.</summary>
    public ActivityDefinitionBuilder ClassifyAs(params string[] terms)
    {
        EnsureNoActivePhase();
        ArgumentNullException.ThrowIfNull(terms);
        foreach (var term in terms)
        {
            var name = RequireName(term, nameof(terms));
            if (!_classifications.Add(name))
                throw new ArgumentException($"Duplicate classification term '{name}'.", nameof(terms));
        }
        return this;
    }

    /// <summary>Starts a phase. Call EndPhase before starting another phase or building.</summary>
    public PhaseBuilder Phase(string id)
    {
        EnsureNoActivePhase();
        var name = RequireName(id, nameof(id));
        if (_phases.Any(phase => string.Equals(phase.Id, name, StringComparison.Ordinal)))
            throw new ArgumentException($"Duplicate phase ID '{name}'.", nameof(id));
        _activePhase = new PhaseBuilder(this, name);
        return _activePhase;
    }

    /// <summary>Validates and returns an immutable definition.</summary>
    public ActivityDefinition Build()
    {
        EnsureNoActivePhase();
        if (_phases.Count == 0)
            throw new InvalidOperationException("An activity must contain at least one phase.");
        return new ActivityDefinition(
            _id,
            _version,
            Array.AsReadOnly(_classifications.OrderBy(term => term, StringComparer.Ordinal).ToArray()),
            Array.AsReadOnly(_phases.ToArray()));
    }

    private void CompletePhase(PhaseBuilder phase, string id, IReadOnlyList<ActivityRoleSlot> roles,
        IReadOnlyList<string> gestureIds)
    {
        EnsureActivePhase(phase);
        if (roles.Count == 0 && gestureIds.Count == 0)
            throw new InvalidOperationException($"Phase '{id}' must declare at least one role or Gesture reference.");
        _phases.Add(new ActivityPhaseDefinition(id, Array.AsReadOnly(roles.ToArray()),
            Array.AsReadOnly(gestureIds.ToArray())));
        _activePhase = null;
    }

    private void EnsureNoActivePhase()
    {
        if (_activePhase is not null)
            throw new InvalidOperationException("Complete the active phase with EndPhase before continuing.");
    }

    private void EnsureActivePhase(PhaseBuilder phase)
    {
        if (!ReferenceEquals(_activePhase, phase))
            throw new InvalidOperationException("This phase builder is no longer active.");
    }

    private static string RequireName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Identifiers and ontology terms must not be empty.", parameterName);
        return value;
    }

    /// <summary>Fluent editor for the currently open phase.</summary>
    public sealed class PhaseBuilder
    {
        private readonly ActivityDefinitionBuilder _owner;
        private readonly string _id;
        private readonly List<ActivityRoleSlot> _roles = [];
        private readonly HashSet<string> _roleIds = new(StringComparer.Ordinal);
        private readonly List<string> _gestureIds = [];
        private readonly HashSet<string> _gestureIdSet = new(StringComparer.Ordinal);

        internal PhaseBuilder(ActivityDefinitionBuilder owner, string id)
        {
            _owner = owner;
            _id = id;
        }

        /// <summary>Declares a role and its inclusive participant-count bounds.</summary>
        public PhaseBuilder AssignRole(string id, int min, int max)
        {
            _owner.EnsureActivePhase(this);
            var name = RequireName(id, nameof(id));
            if (min < 0)
                throw new ArgumentOutOfRangeException(nameof(min), "Minimum participants cannot be negative.");
            if (max < min)
                throw new ArgumentOutOfRangeException(nameof(max), "Maximum participants cannot be less than minimum.");
            if (!_roleIds.Add(name))
                throw new ArgumentException($"Duplicate role ID '{name}' in phase '{_id}'.", nameof(id));
            _roles.Add(new ActivityRoleSlot(name, min, max));
            return this;
        }

        /// <summary>References a reusable Gesture by stable ID without embedding provider logic.</summary>
        public PhaseBuilder OfferGesture(string gestureId)
        {
            _owner.EnsureActivePhase(this);
            var name = RequireName(gestureId, nameof(gestureId));
            if (!_gestureIdSet.Add(name))
                throw new ArgumentException($"Duplicate Gesture reference '{name}' in phase '{_id}'.", nameof(gestureId));
            _gestureIds.Add(name);
            return this;
        }

        /// <summary>Closes this phase and returns to the experience builder.</summary>
        public ActivityDefinitionBuilder EndPhase()
        {
            _owner.CompletePhase(this, _id, _roles, _gestureIds);
            return _owner;
        }
    }
}
