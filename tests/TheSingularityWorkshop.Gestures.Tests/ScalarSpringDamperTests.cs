using TheSingularityWorkshop.Gestures;
using Xunit;

namespace TheSingularityWorkshop.Gestures.Tests;

public sealed class ScalarSpringDamperTests
{
    [Fact]
    public void RepeatedFixedStepsConvergeTowardStationaryTarget()
    {
        var motion = new ScalarSpringDamper();
        var parameters = new SpringDamperParameters(effectiveMass: 1d, stiffness: 40d, damping: 12d);

        for (var i = 0; i < 1200; i++)
            motion.Advance(target: 1d, elapsedSeconds: 1d / 120d, parameters);

        Assert.InRange(motion.Position, 0.999d, 1.001d);
        Assert.InRange(motion.Velocity, -0.001d, 0.001d);
    }

    [Fact]
    public void IdenticalInitialStateAndStepSequenceAreDeterministic()
    {
        var first = new ScalarSpringDamper(initialPosition: -0.25d, initialVelocity: 0.1d);
        var second = new ScalarSpringDamper(initialPosition: -0.25d, initialVelocity: 0.1d);
        var parameters = new SpringDamperParameters(2d, 30d, 8d);

        for (var i = 0; i < 300; i++)
        {
            var target = i < 100 ? 1d : -0.5d;
            Assert.Equal(first.Advance(target, 1d / 120d, parameters),
                second.Advance(target, 1d / 120d, parameters));
            Assert.Equal(first.Velocity, second.Velocity);
        }
    }

    [Fact]
    public void ZeroDurationDoesNotChangeRetainedState()
    {
        var motion = new ScalarSpringDamper(initialPosition: 2d, initialVelocity: -3d);

        var position = motion.Advance(target: 100d, elapsedSeconds: 0d,
            new SpringDamperParameters(1d, 20d, 4d));

        Assert.Equal(2d, position);
        Assert.Equal(2d, motion.Position);
        Assert.Equal(-3d, motion.Velocity);
    }

    [Fact]
    public void InvalidStepDoesNotMutateRetainedState()
    {
        var motion = new ScalarSpringDamper(initialPosition: 2d, initialVelocity: -3d);
        var parameters = new SpringDamperParameters(1d, 20d, 4d);

        Assert.Throws<ArgumentOutOfRangeException>(() => motion.Advance(5d, double.NaN, parameters));
        Assert.Throws<ArgumentOutOfRangeException>(() => motion.Advance(5d, -0.1d, parameters));

        Assert.Equal(2d, motion.Position);
        Assert.Equal(-3d, motion.Velocity);
    }

    [Fact]
    public void DefaultStructParametersAreRejectedEvenForZeroDuration()
    {
        var motion = new ScalarSpringDamper(initialPosition: 2d, initialVelocity: 3d);

        Assert.Throws<ArgumentException>(() => motion.Advance(10d, 0d, default));

        Assert.Equal(2d, motion.Position);
        Assert.Equal(3d, motion.Velocity);
    }

    [Fact]
    public void NonFiniteComputedStateThrowsWithoutMutatingRetainedState()
    {
        var motion = new ScalarSpringDamper(initialPosition: -double.MaxValue);
        var parameters = new SpringDamperParameters(1d, 1d, 0d);

        Assert.Throws<ArithmeticException>(() => motion.Advance(double.MaxValue, 1d, parameters));

        Assert.Equal(-double.MaxValue, motion.Position);
        Assert.Equal(0d, motion.Velocity);
    }

    [Theory]
    [InlineData(0d, 1d, 1d)]
    [InlineData(double.NaN, 1d, 1d)]
    [InlineData(1d, -1d, 1d)]
    [InlineData(1d, 1d, double.PositiveInfinity)]
    public void InvalidParametersAreRejected(double mass, double stiffness, double damping)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpringDamperParameters(mass, stiffness, damping));
    }

    [Fact]
    public void ResetValidatesBeforeReplacingState()
    {
        var motion = new ScalarSpringDamper(initialPosition: 2d, initialVelocity: 3d);

        Assert.Throws<ArgumentOutOfRangeException>(() => motion.Reset(double.NaN, 0d));

        Assert.Equal(2d, motion.Position);
        Assert.Equal(3d, motion.Velocity);
    }
}
