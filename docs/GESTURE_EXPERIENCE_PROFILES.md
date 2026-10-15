# Seeded experience-profile sampling

## Purpose

`GestureExperienceProfile` maps authored parameter ranges to one reproducible `GestureVariation` for an entity. It bridges experience ranges to the evaluator's existing variation inputs without changing the ideal Gesture.

## Contract

- The caller authors minimum and maximum ranges for speed deviation and lateral-offset magnitude.
- Speed deviation stays in `[0, 1)`, keeping the evaluator's speed multiplier positive.
- Lateral-offset magnitude is finite and non-negative.
- `Sample(identitySeed)` uses a fixed integer-mixing algorithm, not runtime `Random`; the same profile and seed return the same values.
- Different seeds normally yield different values when a range has nonzero width. A zero-width range always yields its fixed value.
- Sample once for an entity/experience assignment and reuse that `GestureVariation` for its gestures. Use a stable identity or explicitly versioned assignment seed, not a frame counter.

## Novice and veteran as authored examples

A novice profile may use wider ranges; a veteran profile may use narrower ranges. Those are caller-authored design choices, not built-in labels or empirical claims. For example, a controlled practice scene could compare:

- Novice illustration: speed deviation range `[0.05, 0.25]` and lateral magnitude range `[0.10, 0.50]`.
- Veteran illustration: speed deviation range `[0.01, 0.06]` and lateral magnitude range `[0.02, 0.12]`.

These values are demonstration parameters only. They are not calibrated measurements of martial-arts skill, balance, accuracy, or safety. The profile does not score people or change the ideal Gesture definition.

## Current boundaries

Sampling is deterministic and bounded, but the existing `GestureEvaluator` currently applies only speed and a root X offset; it does not perturb individual joints or keyframes. A tighter range means less variation in these supported channels, not inherently better movement. Pose-specific channels, seeded per-joint variation, contact, recovery, and partner coordination require separately specified contracts and tests.

The profile sampler does not own time, the FSM, rendering, or physics. It is a reusable CPU-side data primitive and has no external dependencies.
