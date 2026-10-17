using Xunit;
using TheSingularityWorkshop.Gestures;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class CrowdResponseSchedulerTests
{
    private static CrowdResponsePolicy Policy => new(
        "balcony-reveal", 1, 0.25, 1.75, 1, 6, 0.2, 0.9);

    [Fact]
    public void FixedReferenceVectorPinsVersionOneHashAndMapping()
    {
        var result = CrowdResponseScheduler.Schedule(
            new CrowdResponseRequest("speech-01", 12d, "soldier-007", 42UL), Policy);

        Assert.Equal("speech-01", result.EventId);
        Assert.Equal("balcony-reveal", result.PolicyId);
        Assert.Equal(1, result.PolicyVersion);
        Assert.Equal("soldier-007", result.ParticipantId);
        Assert.Equal(1.5896829969963577d, result.DelaySeconds, 12);
        Assert.Equal(13.589682996996357d, result.IntentTimeSeconds, 12);
        Assert.Equal(2, result.Variant);
        Assert.Equal(0.35261578313417635d, result.Intensity, 12);
    }

    [Fact]
    public void SameInputsReplayIdenticallyWithoutWallClockDependence()
    {
        var request = new CrowdResponseRequest("speech-01", 12d, "soldier-007", 42UL);

        Assert.Equal(
            CrowdResponseScheduler.Schedule(request, Policy),
            CrowdResponseScheduler.Schedule(request, Policy));
    }

    [Fact]
    public void ParticipantEnumerationOrderDoesNotAffectPerIdentityResults()
    {
        var requests = new[]
        {
            new CrowdResponseRequest("speech-01", 12d, "soldier-007", 42UL),
            new CrowdResponseRequest("speech-01", 12d, "soldier-008", 42UL),
            new CrowdResponseRequest("speech-01", 12d, "soldier-009", 42UL)
        };

        var forward = requests.ToDictionary(x => x.ParticipantId, x => CrowdResponseScheduler.Schedule(x, Policy));
        var reverse = requests.Reverse().ToDictionary(x => x.ParticipantId, x => CrowdResponseScheduler.Schedule(x, Policy));

        Assert.Equal(forward, reverse);
    }

    [Fact]
    public void OutputStaysInsideDeclaredBoundsAndUsesSuppliedClock()
    {
        var request = new CrowdResponseRequest("event", -4d, "agent", 99UL);
        var result = CrowdResponseScheduler.Schedule(request, Policy);

        Assert.InRange(result.DelaySeconds, 0.25d, 1.75d);
        Assert.InRange(result.IntentTimeSeconds, -3.75d, -2.25d);
        Assert.InRange(result.Variant, 1, 6);
        Assert.InRange(result.Intensity, 0.2d, 0.9d);
    }

    [Fact]
    public void InvalidPolicyAndRequestInputsAreRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            new CrowdResponsePolicy(" ", 1, 0d, 1d, 0, 1, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 0, 0d, 1d, 0, 1, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 1, double.NaN, 1d, 0, 1, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 1, 2d, 1d, 0, 1, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 1, 0d, 1d, 2, 1, 0d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 1, 0d, 1d, 0, 1, -1d, 1d));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponsePolicy("policy", 1, 0d, 1d, 0, 1, 0d, double.PositiveInfinity));
        Assert.Throws<ArgumentException>(() =>
            new CrowdResponseRequest("", 0d, "agent", 0UL));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CrowdResponseRequest("event", double.NaN, "agent", 0UL));
        Assert.Throws<ArgumentException>(() =>
            new CrowdResponseRequest("event", 0d, " ", 0UL));
    }

    [Fact]
    public void OverflowedScheduledTimeIsRejected()
    {
        var request = new CrowdResponseRequest("event", double.MaxValue, "agent", 1UL);
        var policy = new CrowdResponsePolicy("policy", 1, 1d, 2d, 0, 1, 0d, 1d);

        Assert.Throws<ArgumentOutOfRangeException>(() => CrowdResponseScheduler.Schedule(request, policy));
    }

    [Fact]
    public void ScheduleContainsIntentOnlyAndDoesNotInventAcousticArrival()
    {
        var result = CrowdResponseScheduler.Schedule(
            new CrowdResponseRequest("speech-01", 12d, "soldier-007", 42UL), Policy);

        Assert.Equal(13.589682996996357d, result.IntentTimeSeconds, 12);
        Assert.DoesNotContain("Arrival", typeof(CrowdResponseSchedule).GetProperties().Select(x => x.Name));
    }
}
