using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class MovementCourseEvaluatorTests
{
    private static MovementCourse CreateCourse() => new(
        "hurdle-practice",
        [
            new MovementCourseElement("approach", MovementCourseElementKind.ApproachZone,
                new Vector3(0f, 0f, 0f), new Vector3(2f, 1f, 2f)),
            new MovementCourseElement("hurdle-01", MovementCourseElementKind.Hurdle,
                new Vector3(3f, 0.5f, 0f), new Vector3(0.2f, 1f, 2f)),
            new MovementCourseElement("landing", MovementCourseElementKind.LandingZone,
                new Vector3(5f, 0f, 0f), new Vector3(2f, 0.2f, 2f))
        ]);

    [Fact]
    public void AllClearedObservationsCompleteCourseRegardlessOfInputCollectionOrder()
    {
        var result = MovementCourseEvaluator.Evaluate(CreateCourse(),
        [
            new MovementCourseObservation("landing", MovementCourseOutcome.Cleared, "physics"),
            new MovementCourseObservation("hurdle-01", MovementCourseOutcome.Cleared, "physics"),
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared, "physics")
        ]);

        Assert.Equal(MovementCourseEvaluationStatus.Completed, result.Status);
        Assert.Null(result.ElementId);
        Assert.Null(result.Outcome);
    }

    [Theory]
    [InlineData(MovementCourseOutcome.Missed)]
    [InlineData(MovementCourseOutcome.Collision)]
    [InlineData(MovementCourseOutcome.UnsafeLanding)]
    [InlineData(MovementCourseOutcome.Blocked)]
    public void NonClearedPhysicalOutcomeFailsAtFirstElement(MovementCourseOutcome outcome)
    {
        var result = MovementCourseEvaluator.Evaluate(CreateCourse(),
        [
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared),
            new MovementCourseObservation("hurdle-01", outcome),
            new MovementCourseObservation("landing", MovementCourseOutcome.Cleared)
        ]);

        Assert.Equal(MovementCourseEvaluationStatus.Failed, result.Status);
        Assert.Equal("hurdle-01", result.ElementId);
        Assert.Equal(outcome, result.Outcome);
    }

    [Fact]
    public void MissingObservationIsIncompleteRatherThanSuccessful()
    {
        var result = MovementCourseEvaluator.Evaluate(CreateCourse(),
        [
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared)
        ]);

        Assert.Equal(MovementCourseEvaluationStatus.Incomplete, result.Status);
        Assert.Equal("hurdle-01", result.ElementId);
        Assert.Null(result.Outcome);
    }

    [Fact]
    public void IndeterminateEvidenceIsNotTreatedAsFailureOrSuccess()
    {
        var result = MovementCourseEvaluator.Evaluate(CreateCourse(),
        [
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared),
            new MovementCourseObservation("hurdle-01", MovementCourseOutcome.Indeterminate),
            new MovementCourseObservation("landing", MovementCourseOutcome.Cleared)
        ]);

        Assert.Equal(MovementCourseEvaluationStatus.Indeterminate, result.Status);
        Assert.Equal("hurdle-01", result.ElementId);
        Assert.Equal(MovementCourseOutcome.Indeterminate, result.Outcome);
    }

    [Fact]
    public void EarlierFailureTakesPrecedenceOverLaterMissingEvidence()
    {
        var result = MovementCourseEvaluator.Evaluate(CreateCourse(),
        [
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared),
            new MovementCourseObservation("hurdle-01", MovementCourseOutcome.Collision)
        ]);

        Assert.Equal(MovementCourseEvaluationStatus.Failed, result.Status);
        Assert.Equal("hurdle-01", result.ElementId);
    }

    [Fact]
    public void RejectsUnknownAndDuplicateObservationIds()
    {
        var course = CreateCourse();

        Assert.Throws<ArgumentException>(() => MovementCourseEvaluator.Evaluate(course,
        [
            new MovementCourseObservation("not-in-course", MovementCourseOutcome.Cleared)
        ]));

        Assert.Throws<ArgumentException>(() => MovementCourseEvaluator.Evaluate(course,
        [
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared),
            new MovementCourseObservation("approach", MovementCourseOutcome.Cleared)
        ]));
    }

    [Fact]
    public void ObservationRequiresElementIdAndDefinedOutcome()
    {
        Assert.Throws<ArgumentException>(() =>
            new MovementCourseObservation(" ", MovementCourseOutcome.Cleared));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MovementCourseObservation("hurdle", (MovementCourseOutcome)999));
    }
}
