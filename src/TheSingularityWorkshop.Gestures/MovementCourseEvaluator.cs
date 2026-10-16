namespace TheSingularityWorkshop.Gestures;

/// <summary>
/// A result reported by the system responsible for observing a course element.
/// Gestures does not create physical evidence; the caller supplies observations from
/// the authoritative simulation or other explicitly identified evaluator.
/// </summary>
public enum MovementCourseOutcome
{
    Cleared,
    Missed,
    Collision,
    UnsafeLanding,
    Blocked,
    Indeterminate
}

/// <summary>Aggregate result for a course evaluated in its authored element order.</summary>
public enum MovementCourseEvaluationStatus
{
    Completed,
    Failed,
    Incomplete,
    Indeterminate
}

/// <summary>
/// A data-only observation for one course element. The evidence source is a caller-defined
/// label for traceability; this type does not authenticate that source or prove physics.
/// </summary>
public sealed record MovementCourseObservation
{
    public MovementCourseObservation(
        string elementId,
        MovementCourseOutcome outcome,
        string? evidenceSource = null)
    {
        if (string.IsNullOrWhiteSpace(elementId))
            throw new ArgumentException("Element ID must not be empty.", nameof(elementId));
        if (!Enum.IsDefined(outcome))
            throw new ArgumentOutOfRangeException(nameof(outcome), "Unknown course outcome.");

        ElementId = elementId;
        Outcome = outcome;
        EvidenceSource = string.IsNullOrWhiteSpace(evidenceSource) ? null : evidenceSource;
    }

    public string ElementId { get; }

    public MovementCourseOutcome Outcome { get; }

    public string? EvidenceSource { get; }
}

/// <summary>
/// The deterministic result of evaluating externally supplied observations against a course.
/// </summary>
/// <remarks>
/// The first non-cleared or missing element in authored order determines the result.
/// A reachable IK target is not an observation and cannot complete a course.
/// </remarks>
public sealed record MovementCourseEvaluation(
    MovementCourseEvaluationStatus Status,
    string? ElementId,
    MovementCourseOutcome? Outcome);

/// <summary>
/// Evaluates ordered course observations without simulating movement or interpreting geometry.
/// </summary>
public static class MovementCourseEvaluator
{
    /// <summary>
    /// Evaluates each course element in authored order. Missing evidence is incomplete;
    /// indeterminate evidence remains indeterminate; any other non-cleared outcome fails.
    /// Unknown and duplicate observation IDs are rejected rather than silently ignored.
    /// </summary>
    public static MovementCourseEvaluation Evaluate(
        MovementCourse course,
        IEnumerable<MovementCourseObservation> observations)
    {
        ArgumentNullException.ThrowIfNull(course);
        ArgumentNullException.ThrowIfNull(observations);

        var courseElementIds = course.Elements
            .Select(element => element.Id)
            .ToHashSet(StringComparer.Ordinal);
        var byId = new Dictionary<string, MovementCourseObservation>(StringComparer.Ordinal);
        foreach (var observation in observations)
        {
            if (observation is null)
                throw new ArgumentException("Observations must not contain null entries.", nameof(observations));
            if (!courseElementIds.Contains(observation.ElementId))
                throw new ArgumentException(
                    $"Observation references unknown course element '{observation.ElementId}'.",
                    nameof(observations));
            if (!byId.TryAdd(observation.ElementId, observation))
                throw new ArgumentException(
                    $"Duplicate observation for course element '{observation.ElementId}'.",
                    nameof(observations));
        }

        foreach (var element in course.Elements)
        {
            if (!byId.TryGetValue(element.Id, out var observation))
                return new MovementCourseEvaluation(
                    MovementCourseEvaluationStatus.Incomplete, element.Id, null);

            if (observation.Outcome == MovementCourseOutcome.Indeterminate)
                return new MovementCourseEvaluation(
                    MovementCourseEvaluationStatus.Indeterminate, element.Id, observation.Outcome);

            if (observation.Outcome != MovementCourseOutcome.Cleared)
                return new MovementCourseEvaluation(
                    MovementCourseEvaluationStatus.Failed, element.Id, observation.Outcome);
        }

        return new MovementCourseEvaluation(MovementCourseEvaluationStatus.Completed, null, null);
    }
}
