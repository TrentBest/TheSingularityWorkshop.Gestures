# Motion Response Comparison

**Status: executable deterministic experiment; illustrative coordinates, not physical validation.**

`MotionResponseComparison.Run(...)` compares two ways to represent the same one-dimensional target trajectory:

1. **Direct following:** output position equals the requested target at each sample. This is the zero-lag reference.
2. **Spring-damper:** the same target samples are supplied to `ScalarSpringDamper`, whose position and velocity are advanced with the selected fixed timestep and parameters.

The target ramps linearly from 0 to 1 over the requested ramp duration, then remains at 1 for the observation window. The experiment uses one fixed step size and records the target, direct position, spring position, and spring velocity at each sample.

## Reported metrics

- **Mean absolute tracking error:** average absolute difference between the target and spring position across all recorded samples, including the ramp and hold.
- **Maximum absolute tracking error:** largest absolute target-to-spring position difference in the same window.
- **Peak overshoot:** greatest spring position above the final target value of 1; it is zero if the response never exceeds that value.
- **Settling time:** time after the ramp ends when both position and velocity remain within the configured tolerance for a consecutive 100 ms observation. `null` means the supplied observation window did not demonstrate settling; it does not mean the response will never settle.

The direct-following series is recorded alongside the spring response so the target input and zero-lag reference can be inspected sample by sample. The summary tracking-error metrics describe the spring response only.

## Reproducibility and limits

Identical inputs produce identical samples and metrics. The experiment is sensitive to timestep and the spring parameters, so record them with any comparison. A small timestep and a stable parameter range are necessary for a meaningful numerical experiment.

All values are dimensionless coordinate units and seconds. This does not represent a body, joint, contact, collision, muscle, physical force measurement, or validated human movement. It does not claim that spring-damper response is better than direct following: direct following has no lag by construction, while the spring response adds lag and may overshoot. The point is to make those trade-offs measurable before selecting a motion law.

The experiment currently compares one linear ramp-and-hold trajectory. It does not yet implement seeded experience-profile sampling, multidimensional pose response, alternative integrators, or population visualization.
