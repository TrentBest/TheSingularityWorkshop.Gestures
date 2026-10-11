# Determinism and Individual Variation

The goal is coordinated variation, not uncontrolled noise. Thousands of entities may perform one ideal procedure while retaining stable, bounded differences.

## Seed contract

The evaluator accepts a stable ulong identity/context seed. It uses a fixed integer mixing function to derive variation values. The same definition, elapsed time, duration, variation settings, and seed produce the same result on a given implementation/runtime.

Do not seed variation from a per-frame random generator. That creates temporal jitter and makes debugging and replay comparison difficult.

A consuming application should choose seed inputs deliberately. Entity identity is a reasonable base; a procedure instance, formation slot, or relevant context can be mixed in when a new repeatable variation is desired.

## Current bounded controls

- MaximumSpeedDeviation: fractional speed deviation in [0, 1). For example, 0.1 bounds speed to 0.9–1.1 of ideal speed.
- MaximumLateralOffset: maximum absolute X offset returned separately as a root/presentation offset.

Variation settings are validated before evaluation. They are bounds, not guarantees of a statistically uniform population distribution.

## Important limitations

- Stable seed variation is not a substitute for state-dependent behavior.
- A deterministic offset is not collision avoidance.
- The initial speed variation changes normalized progress at the same elapsed time; it does not change the ideal keyframe data.
- This first evaluator uses a fixed integer mixer. If the package later needs cross-language bit-for-bit parity, integer width, overflow semantics, float conversion, and test vectors must be specified explicitly.
- GPU floating-point math may differ slightly from CPU math. Define comparison tolerances and test edge cases such as endpoints and zero-length segments.

## Reference test vectors

The test suite establishes linear interpolation at half progress, clamping and completion after duration, same-seed repeatability, bounded speed and lateral variation across a seed sample, and rejection of invalid keyframe ordering and endpoints.

As the data contract matures, add published fixed-input/fixed-output vectors so independent CPU and GPU evaluators can validate themselves without sharing implementation code.
