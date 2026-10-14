# Planar kinematics reference

This small CPU reference is the first mathematical step toward constrained articulation. It is intentionally narrower than a general rig system: a fixed anchor at the origin, two links with explicit lengths, two revolute joints, and a target in a 2D plane.

## What is implemented

- **Forward kinematics:** compute the end-effector position from link lengths and joint angles.
- **Two-link inverse kinematics:** evaluate both elbow configurations using the law of cosines.
- **Explicit joint limits:** clamp candidate angles to caller-provided ranges, then recompute the actual end-effector position from those constrained angles.
- **Honest unreachable-target handling:** return the best of the two constrained candidates, its measured positional error, and a `TargetReached` flag. A clamped pose is not presented as a successful solve.
- **Deterministic reference behavior:** the same finite inputs produce the same output; the implementation has no engine, renderer, FSM, or GPU dependency.

Angles are in radians. The shoulder angle is relative to +X; the elbow angle is relative to the first link. This implementation treats the fixed anchor as `(0, 0)`.

## What is not implemented

This is **not** a 3D IK solver, a hierarchical rig evaluator, a collision-aware solver, a dynamics simulation, a GPU implementation, or a benchmark. It does not calculate contact forces, suspension, tire behavior, or momentum. The two-link model is a compact testable reference contract—not a claim that the full Gestures evaluator now solves IK.

## Why this order

1. Make the geometry explicit and testable on the CPU.
2. Verify reachable targets, unreachable targets, anchor stability, and joint limits.
3. Use this implementation as a correctness reference for any later batched implementation.
4. Only then evaluate GPU data layout and dispatch overhead against realistic batches; small workloads may remain cheaper on the CPU.
5. Introduce fidelity tiers only when their state, update frequency, approximations, and transition rules can be tested.

A GPU implementation should be judged on end-to-end workload cost and correctness against this reference, not on the word “GPU” alone.
