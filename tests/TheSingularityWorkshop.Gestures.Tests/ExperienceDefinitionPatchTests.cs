using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ExperienceDefinitionPatchTests
{
    [Fact]
    public void PatchMatchesOnlyTheExpectedCurrentDefinitionRevision()
    {
        var current = CreateDefinition("activity.tag", 2);
        var replacement = CreateDefinition("activity.tag", 3);
        var patch = new ExperienceDefinitionPatch(current.Id, current.Version, replacement);

        Assert.True(patch.CanApplyTo(current));
        Assert.False(patch.CanApplyTo(CreateDefinition("activity.tag", 1)));
        Assert.False(patch.CanApplyTo(CreateDefinition("activity.tag", 3)));
        Assert.False(patch.CanApplyTo(CreateDefinition("activity.hide-and-seek", 2)));
    }

    [Fact]
    public void RejectsInvalidPatchIdentityAndVersion()
    {
        var replacement = CreateDefinition("activity.tag", 3);

        Assert.Throws<ArgumentException>(() =>
            new ExperienceDefinitionPatch(" ", 2, replacement));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ExperienceDefinitionPatch("activity.tag", 0, replacement));
        Assert.Throws<ArgumentException>(() =>
            new ExperienceDefinitionPatch("activity.hide-and-seek", 2, replacement));
        Assert.Throws<ArgumentException>(() =>
            new ExperienceDefinitionPatch("activity.tag", 3, replacement));
        Assert.Throws<ArgumentNullException>(() =>
            new ExperienceDefinitionPatch("activity.tag", 2, null!));
    }

    private static ExperienceDefinition CreateDefinition(string id, int version) =>
        ExperienceDefinitionBuilder.Define(id)
            .Version(version)
            .ClassifyAs("game")
            .Phase("round")
                .AssignRole("participant", 1, 20)
                .OfferGesture("move")
                .EndPhase()
            .Build();
}
