# Stateful Hinge Prototype

## Purpose

This is the first executable experiment in the Stateful IK research track. It takes the theory's smallest useful unit—a retained joint angle—and makes its transition rules testable before introducing parent/child hierarchy or physical contact.

## Contract

`StatefulHinge` stores one angle in radians and accepts a requested angle, elapsed simulation time, and maximum angular speed for each update.

- The initial angle must lie inside the configured inclusive legal range.
- Each update moves toward the requested angle by no more than speed × elapsed time.
- An out-of-range request is clamped to the nearest legal angle, but the original request remains visible and `TargetReached` remains false when the request itself was not reached.
- `JointLimitConstrained` tells the caller when the requested angle fell outside the configured interval.
- `JointMotionState` classifies the *observed angle change* as `MovingPositive`, `MovingNegative`, or `Stationary`, using a configurable angular tolerance.
- Zero elapsed time causes no movement. Negative elapsed time, negative speed, non-finite inputs, and invalid construction ranges are rejected.

Positive and negative are relative to the hinge's declared coordinate convention; they do not inherently mean world-left, world-right, up, or down.

## What this prototype does not do

This is a deterministic, speed-limited kinematic controller, not a physical hinge simulation. It does not integrate torque, inertia, acceleration, collisions, contact forces, friction, adhesion, balance, or damage. A target request is not proof of physical success. Those responsibilities must be introduced through explicit contracts and adapters rather than implied by this class.

It also does not yet implement a hierarchy, parent-to-child invalidation, retained velocity, external impulses, or incremental-vs-full evaluation. The next research step is a small parent/child hierarchy with tests that compare incremental results against full reevaluation.

## Tests and next decisions

The unit tests cover bounded progress, all three movement classifications, target completion, unreachable-by-limit requests, zero elapsed time, and invalid inputs. Before expanding the public API, review whether the motion enum and result fields express the needed contract without conflating kinematic limits with physical contact or stability.

This experiment is a correctness baseline. No performance advantage is claimed.
