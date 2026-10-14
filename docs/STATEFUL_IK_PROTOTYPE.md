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

The companion `StatefulJointNode` prototype connects retained hinges into a planar parent/child hierarchy. Each link extends along local +X; a child's local angle is added to its parent's world angle, and the child starts at the parent's endpoint. `EvaluateFull` recalculates every node and acts as a correctness reference. `EvaluateIncremental` uses hinge-change notifications and ancestor dirty flags to recalculate only changed transforms and affected descendants; it returns the number of transforms recalculated. Unchanged hierarchies return zero, and adding a child invalidates the relevant path. The hierarchy rejects cycles, multiple parents, non-finite anchors, and evaluation starting below the root.

Incremental evaluation is an early correctness prototype, not yet a demonstrated performance win. It does not implement retained velocity, external impulses, collision, contact, or physical integration. It is a kinematic transform hierarchy, not a climbing/contact solver.

## Tests and next decisions

The unit tests cover bounded hinge progress, all three movement classifications, target completion, joint-limit constraints, zero elapsed time, invalid inputs, parent-to-descendant propagation, no-op updates, child-only changes, root-anchor changes, branched hierarchies, and a deterministic sequence compared against full reevaluation. Next, stress-test deep hierarchies and invalid-state recovery, then benchmark update counts and total runtime before making performance claims.

This experiment is a correctness baseline. No performance advantage is claimed.
