using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class GestureTargetDiscoveryTests
{
    [Fact]
    public void DogQueryReturnsDeterministicLocalIndexesAndVersionedIdentities()
    {
        var profiles = new[]
        {
            new GestureTargetProfile("zoo", "large-dog", "2", "dog", ["walk", "sit"], ["dog.walk", "dog.sit"]),
            new GestureTargetProfile("aviary", "songbird", "1", "bird", ["hop", "fly"], ["bird.fly"]),
            new GestureTargetProfile("kennel", "companion-dog", "3", "dog", ["walk", "sit", "shake-off"], ["dog.walk"])
        };

        var result = GestureTargetDiscovery.Find(profiles.Reverse(),
            new GestureTargetQuery("dog", ["walk"]));

        Assert.Equal(new[] { 0, 1 }, result.Select(match => match.Index));
        Assert.Equal(new[] { "kennel", "zoo" }, result.Select(match => match.Profile.ProviderId));
        Assert.Equal("companion-dog", result[0].Profile.OfferId);
        Assert.Equal("3", result[0].Profile.Version);
        Assert.Equal(new[] { "dog.walk" }, result[0].Profile.GestureIds);
    }

    [Fact]
    public void RequiresEveryRequestedCapabilityAndExactTargetKind()
    {
        var profiles = new[]
        {
            new GestureTargetProfile("a", "dog-walk", "1", "dog", ["walk"]),
            new GestureTargetProfile("b", "dog-sit", "1", "dog", ["sit"]),
            new GestureTargetProfile("c", "bird-walk", "1", "bird", ["walk", "sit"])
        };

        var result = GestureTargetDiscovery.Find(profiles,
            new GestureTargetQuery("dog", ["walk", "sit"]));

        Assert.Empty(result);
    }

    [Fact]
    public void RejectsDuplicateDurableIdentityAndInvalidNames()
    {
        var profile = new GestureTargetProfile("provider", "dog", "1", "dog");
        Assert.Throws<ArgumentException>(() =>
            GestureTargetDiscovery.Find([profile, profile], new GestureTargetQuery("dog")));
        Assert.Throws<ArgumentException>(() => new GestureTargetQuery("dog", ["walk", "walk"]));
        Assert.Throws<ArgumentException>(() => new GestureTargetProfile("", "dog", "1", "dog"));
    }

    [Fact]
    public void EmptyQueryReturnsNoMatchesWithoutInventingProviders()
    {
        Assert.Empty(GestureTargetDiscovery.Find([], new GestureTargetQuery("dog")));
    }
}
