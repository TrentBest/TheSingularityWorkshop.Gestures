using System.Numerics;
using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ExtremusCrowdBakerTests
{
    private static ExtremusRegion Region => new("starport-apron",
        new Vector3(-100f, 0f, -50f), new Vector3(100f, 20f, 50f));

    private static ExtremusBakeRequest Request(ulong seed = 42UL, int count = 128,
        IEnumerable<ExtremusActivity>? activities = null, IEnumerable<ExtremusExclusion>? exclusions = null) =>
        new("starport-departure", 1, "catalogue-test-v1", Region, seed, count,
            activities ?? [
                new ExtremusActivity("boarding", 3d),
                new ExtremusActivity("waiting", 5d),
                new ExtremusActivity("walking", 2d)
            ], exclusions);

    [Fact]
    public void SameRequestProducesExactlyTheSameManifestAndInstances()
    {
        var request = Request();

        var first = ExtremusCrowdBaker.Bake(request);
        var second = ExtremusCrowdBaker.Bake(request);

        Assert.Equal(first.Manifest, second.Manifest);
        Assert.Equal(first.Instances, second.Instances);
    }

    [Fact]
    public void InputEnumerationOrderDoesNotChangePopulation()
    {
        var activities = new[]
        {
            new ExtremusActivity("boarding", 3d),
            new ExtremusActivity("waiting", 5d),
            new ExtremusActivity("walking", 2d)
        };
        var reversed = activities.Reverse().ToArray();

        var first = ExtremusCrowdBaker.Bake(Request(activities: activities));
        var second = ExtremusCrowdBaker.Bake(Request(activities: reversed));

        Assert.Equal(first.Instances, second.Instances);
    }

    [Fact]
    public void DifferentSeedsProduceDifferentPopulations()
    {
        var first = ExtremusCrowdBaker.Bake(Request(seed: 42UL));
        var second = ExtremusCrowdBaker.Bake(Request(seed: 43UL));

        Assert.NotEqual(first.Instances, second.Instances);
        Assert.NotEqual(first.Manifest.Seed, second.Manifest.Seed);
    }

    [Fact]
    public void EveryInstanceHasStableIdentityAndRemainsInsideRegion()
    {
        var result = ExtremusCrowdBaker.Bake(Request(count: 256));

        Assert.Equal(256, result.Manifest.RequestedCount);
        Assert.Equal(256, result.Manifest.BakedCount);
        Assert.Equal(256, result.Instances.Select(x => x.StableId).Distinct(StringComparer.Ordinal).Count());

        foreach (var instance in result.Instances)
        {
            Assert.InRange(instance.Position.X, -100f, 100f);
            Assert.InRange(instance.Position.Y, 0f, 20f);
            Assert.InRange(instance.Position.Z, -50f, 50f);
            Assert.InRange(instance.HeadingRadians, 0f, (float)(2d * Math.PI));
            Assert.InRange(instance.Scale, 0.85f, 1.15f);
            Assert.InRange(instance.Phase, 0d, 1d);
            Assert.Contains(instance.ActivityId, new[] { "boarding", "waiting", "walking" });
        }
    }

    [Fact]
    public void InstancesAvoidDomainSuppliedExclusionEnvelopes()
    {
        var exclusion = new ExtremusExclusion("service-clearance",
            new Vector3(-20f, 0f, -20f), new Vector3(20f, 20f, 20f));
        var result = ExtremusCrowdBaker.Bake(Request(count: 256, exclusions: [exclusion]));

        foreach (var instance in result.Instances)
        {
            var p = instance.Position;
            Assert.False(p.X >= -20f && p.X <= 20f && p.Y >= 0f && p.Y <= 20f
                && p.Z >= -20f && p.Z <= 20f);
        }
    }

    [Fact]
    public void FullyExcludedRegionFailsExplicitlyInsteadOfSilentlyDroppingPeople()
    {
        var exclusion = new ExtremusExclusion("whole-region",
            new Vector3(-100f, 0f, -50f), new Vector3(100f, 20f, 50f));

        Assert.Throws<InvalidOperationException>(() =>
            ExtremusCrowdBaker.Bake(Request(count: 1, exclusions: [exclusion])));
    }

    [Fact]
    public void EmptyPopulationDoesNotRequireActivitiesAndStillHasManifest()
    {
        var request = new ExtremusBakeRequest("empty-profile", 1, "catalogue-v1", Region,
            0UL, 0, Array.Empty<ExtremusActivity>());

        var result = ExtremusCrowdBaker.Bake(request);

        Assert.Empty(result.Instances);
        Assert.Equal(0, result.Manifest.BakedCount);
        Assert.Equal(0, result.Manifest.RequestedCount);
    }

    [Fact]
    public void InvalidRegionActivityAndRequestInputsAreRejected()
    {
        Assert.Throws<ArgumentException>(() => new ExtremusRegion("bad",
            Vector3.One, Vector3.One));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtremusRegion("bad",
            new Vector3(float.NaN, 0f, 0f), Vector3.One));
        Assert.Throws<ArgumentException>(() => new ExtremusExclusion("bad",
            Vector3.One, Vector3.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtremusActivity("bad", 0d));
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtremusActivity("bad", double.NaN));
        Assert.Throws<ArgumentException>(() => Request(count: 1, activities: []));
        Assert.Throws<ArgumentException>(() => Request(count: 1, activities: [
            new ExtremusActivity("same", 1d), new ExtremusActivity("same", 2d)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => Request(count: -1));
    }
}
