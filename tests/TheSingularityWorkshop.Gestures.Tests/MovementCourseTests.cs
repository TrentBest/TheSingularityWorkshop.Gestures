using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class MovementCourseTests
{
    [Fact]
    public void CoursePreservesAuthoredOrderAndGeometry()
    {
        var approach = new MovementCourseElement(
            "approach", MovementCourseElementKind.AccelerationZone,
            new Vector3(2f, 0f, 0f), new Vector3(4f, 1f, 2f));
        var hurdle = new MovementCourseElement(
            "hurdle-01", MovementCourseElementKind.Hurdle,
            new Vector3(6f, 0.5f, 0f), new Vector3(0.2f, 1f, 2f));
        var landing = new MovementCourseElement(
            "landing", MovementCourseElementKind.LandingZone,
            new Vector3(9f, 0f, 0f), new Vector3(3f, 0.2f, 2f));

        var course = new MovementCourse("110m-practice", [approach, hurdle, landing]);

        Assert.Equal("110m-practice", course.Id);
        Assert.Equal("meters", course.Units);
        Assert.Equal(["approach", "hurdle-01", "landing"], course.Elements.Select(element => element.Id));
        Assert.Same(hurdle, course.Elements[1]);
        Assert.Equal(new Vector3(0.2f, 1f, 2f), course.Elements[1].Size);
    }

    [Fact]
    public void CourseCopiesInputCollectionSoLaterAuthoringChangesDoNotMutateIt()
    {
        var elements = new List<MovementCourseElement>
        {
            new("start", MovementCourseElementKind.ApproachZone, Vector3.Zero, Vector3.One)
        };

        var course = new MovementCourse("practice", elements);
        elements.Add(new MovementCourseElement(
            "finish", MovementCourseElementKind.Finish, Vector3.One, Vector3.One));

        Assert.Single(course.Elements);
    }

    [Fact]
    public void CourseRejectsDuplicateStableElementIds()
    {
        var first = new MovementCourseElement(
            "obstacle", MovementCourseElementKind.Hurdle, Vector3.Zero, Vector3.One);
        var second = new MovementCourseElement(
            "obstacle", MovementCourseElementKind.Gap, Vector3.One, Vector3.One);

        Assert.Throws<ArgumentException>(() => new MovementCourse("bad", [first, second]));
    }

    [Theory]
    [InlineData(float.NaN, 1f, 1f)]
    [InlineData(1f, float.PositiveInfinity, 1f)]
    [InlineData(1f, 1f, float.NegativeInfinity)]
    public void ElementRejectsNonFiniteCenter(float x, float y, float z)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MovementCourseElement("element", MovementCourseElementKind.Hurdle,
                new Vector3(x, y, z), Vector3.One));
    }

    [Theory]
    [InlineData(0f, 1f, 1f)]
    [InlineData(1f, -1f, 1f)]
    [InlineData(1f, 1f, float.NaN)]
    public void ElementRejectsNonPositiveOrNonFiniteSize(float x, float y, float z)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new MovementCourseElement("element", MovementCourseElementKind.Hurdle,
                Vector3.Zero, new Vector3(x, y, z)));
    }

    [Fact]
    public void CourseRejectsEmptyElementsAndUnits()
    {
        Assert.Throws<ArgumentException>(() => new MovementCourse("empty", []));
        Assert.Throws<ArgumentException>(() => new MovementCourse("course", [
            new MovementCourseElement("hurdle", MovementCourseElementKind.Hurdle,
                Vector3.Zero, Vector3.One)], " "));
    }
}
