using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ExperienceDefinitionBuilderTests
{
    [Fact]
    public void BuildsVersionedExperienceWithClassificationsRolesAndGestureReferences()
    {
        var definition = ExperienceDefinitionBuilder.Define("public-space.flash-mob")
            .Version(2)
            .ClassifyAs("social", "performance")
            .Phase("gather")
                .AssignRole("initiator", 1, 3)
                .OfferGesture("notice-the-cue")
                .EndPhase()
            .Phase("perform")
                .AssignRole("participant", 1, 500)
                .OfferGesture("dance.phrase-a")
                .EndPhase()
            .Build();

        Assert.Equal("public-space.flash-mob", definition.Id);
        Assert.Equal(2, definition.Version);
        Assert.Equal(new[] { "performance", "social" }, definition.Classifications);
        Assert.Equal(new[] { "gather", "perform" }, definition.Phases.Select(phase => phase.Id));
        Assert.Equal(new ExperienceRoleSlot("participant", 1, 500), Assert.Single(definition.Phases[1].Roles));
        Assert.Equal(new[] { "dance.phrase-a" }, definition.Phases[1].GestureIds);
    }

    [Fact]
    public void BuildReturnsDefensivelyReadOnlyCollections()
    {
        var definition = ExperienceDefinitionBuilder.Define("activity.tag")
            .ClassifyAs("game")
            .Phase("round")
                .AssignRole("runner", 1, 20)
                .OfferGesture("run")
                .EndPhase()
            .Build();

        Assert.IsAssignableFrom<IReadOnlyList<string>>(definition.Classifications);
        Assert.IsAssignableFrom<IReadOnlyList<ExperiencePhaseDefinition>>(definition.Phases);
        Assert.IsAssignableFrom<IReadOnlyList<ExperienceRoleSlot>>(definition.Phases[0].Roles);
    }

    [Fact]
    public void RejectsEmptyDefinitionAndIncompletePhase()
    {
        Assert.Throws<InvalidOperationException>(() =>
            ExperienceDefinitionBuilder.Define("activity.empty").Build());

        var builder = ExperienceDefinitionBuilder.Define("activity.incomplete");
        builder.Phase("empty");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void RejectsInvalidVersionsIdentifiersRoleBoundsAndDuplicates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ExperienceDefinitionBuilder.Define("activity.version").Version(0));

        Assert.Throws<ArgumentException>(() =>
            ExperienceDefinitionBuilder.Define(" "));

        Assert.Throws<ArgumentException>(() =>
            ExperienceDefinitionBuilder.Define("activity.duplicate")
                .ClassifyAs("social", "social"));

        var builder = ExperienceDefinitionBuilder.Define("activity.roles");
        var phase = builder.Phase("round");
        Assert.Throws<ArgumentOutOfRangeException>(() => phase.AssignRole("runner", -1, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => phase.AssignRole("runner", 2, 1));
        phase.AssignRole("runner", 1, 2);
        Assert.Throws<ArgumentException>(() => phase.AssignRole("runner", 1, 2));
    }

    [Fact]
    public void RejectsDuplicatePhaseIdsAndGestureReferences()
    {
        var builder = ExperienceDefinitionBuilder.Define("activity.duplicate-phase");
        builder.Phase("round").OfferGesture("run").EndPhase();
        Assert.Throws<ArgumentException>(() => builder.Phase("round"));

        var second = ExperienceDefinitionBuilder.Define("activity.duplicate-gesture");
        var phase = second.Phase("round");
        phase.OfferGesture("run");
        Assert.Throws<ArgumentException>(() => phase.OfferGesture("run"));
    }
}
