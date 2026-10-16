# Scalar Spring-Damper Reference

**Status: implemented experimental CPU primitive; not a physical simulation.**

This reference makes one part of the Gestures motion-equation research executable. A Gesture pose path can provide a target coordinate; the spring-damper retains position and velocity and responds over time instead of teleporting the coordinate to each new target.

## Equation and integration

\[
\ddot{x}=\frac{k(x_{target}-x)-c\dot{x}}{m}
\]

The implementation uses semi-implicit Euler for each caller-supplied step:

1. Calculate acceleration from the current position, velocity, target, effective mass, stiffness, and damping.
2. Update velocity using that acceleration.
3. Update position using the new velocity.

The implementation is deterministic for the same initial state, parameter values, target sequence, and timestep sequence. It is stateful: changing timestep or step order can change the result. Use a fixed, sufficiently small timestep and test the selected parameter ranges.

## API

- **SpringDamperParameters** validates positive finite effective mass and finite non-negative stiffness and damping.
- **ScalarSpringDamper** retains one position and velocity, advances toward a supplied target, and can be reset.
- A zero-duration step is a no-op.
- Invalid inputs are rejected before changing state. If arithmetic produces a non-finite result, the step throws and preserves the previous state.

## Executable comparison

`MotionResponseComparison.Run(...)` supplies the same linear ramp-and-hold target to direct following and the spring-damper response. It records target position, direct position, spring position, and spring velocity, then reports mean and maximum absolute tracking error, peak overshoot, and observed settling time separately.

See [Motion Response Comparison](MOTION_RESPONSE_COMPARISON.md) for the exact trajectory, metric definitions, and limits. The harness is deterministic and covered by unit tests. It is a numerical comparison, not a benchmark of runtime performance and not evidence that either response is physically correct.

## What this proves—and what it does not

The tests cover fixed-step convergence for one damped parameter set, deterministic replay, zero-duration behavior, parameter validation, and state preservation after invalid input or numerical overflow. Comparison tests cover deterministic sample generation, direct-following identity, observed settling, and the case where the observation window is too short to establish settling.

This is a **one-dimensional response primitive**, not yet wired into GestureEvaluator and not an articulated-body solver. It does not calculate a physical mass distribution, joint torque, collision, contact, constraints, or balance. Caller-supplied units must be consistent; the API does not impose a universal physical unit system. No claim is made that the sample parameters represent human biomechanics or that this method is optimal for every target trajectory.

The next useful experiment is to compare this response with other integrators across more declared target trajectories and parameter ranges, then visualize the same seeds under wider and narrower experience profiles. The existing Gesture evaluator remains the unchanged deterministic keyframe-interpolation baseline until those comparisons justify a separately reviewed integration contract.
