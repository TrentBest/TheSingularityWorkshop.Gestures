# Stateful Hinge Prototype

## Purpose

This is the first executable experiment in the Stateful IK research track. It now includes a retained hinge, a planar parent/child hierarchy, dirty-path incremental evaluation, and a data-only handoff for externally measured contact outcomes. It still does not implement the physical model that produces those observations.

## Contract

`StatefulHinge` stores one angle in radians and accepts a requested angle, elapsed simulation time, and maximum angular speed for each update.

- The initial angle must lie inside the configured inclusive legal range.
- Each update moves toward the requested angle by no more than speed × elapsed time.
- The legal minimum and maximum are exposed as read-only values so callers can verify that geometric constraints agree with retained hinge constraints.
- An out-of-range request is clamped to the nearest legal angle, but the original request remains visible and `TargetReached` remains false when the request itself was not reached.
- `JointLimitConstrained` tells the caller when the requested angle fell outside the configured interval.
- `JointMotionState` classifies the *observed angle change* as `MovingPositive`, `MovingNegative`, or `Stationary`, using a configurable angular tolerance.
- Zero elapsed time causes no movement. Negative elapsed time, negative speed, non-finite inputs, and invalid construction ranges are rejected.

Positive and negative are relative to the hinge's declared coordinate convention; they do not inherently mean world-left, world-right, up, or down.

## What this prototype does not do

This is a deterministic, speed-limited kinematic controller, not a physical hinge simulation. It does not integrate torque, inertia, acceleration, collisions, contact forces, friction, adhesion, balance, or damage; contact is only retained as an external observation. A target request is not proof of physical success. Those responsibilities must be introduced through explicit contracts and adapters rather than implied by this class.

The companion `StatefulJointNode` prototype connects retained hinges into a planar parent/child hierarchy. Each link extends along local +X; a child's local angle is added to its parent's world angle, and the child starts at the parent's endpoint. `EvaluateFull` recalculates every node and acts as a correctness reference. `EvaluateIncremental` uses hinge-change notifications and ancestor dirty flags to recalculate only changed transforms and affected descendants; it returns the number of transforms recalculated. Unchanged hierarchies return zero, and adding a child invalidates the relevant path. The hierarchy rejects cycles, multiple parents, non-finite anchors, and evaluation starting below the root.

Incremental evaluation is an early correctness prototype, not yet a demonstrated performance win. It does not implement retained velocity, external impulses, collision, contact, or physical integration. It is a kinematic transform hierarchy, not a climbing/contact solver.


## Closed-loop kinematic controller

`StatefulPlanarIkController` now connects the geometric reference to the retained hinge and hierarchy prototypes. Each `Advance` call:

1. Converts the requested world-space target to the root-anchor-relative planar frame.
2. Computes a constrained two-link reference solution.
3. Advances each retained hinge by no more than its configured angular-speed limit and the elapsed time.
4. Evaluates the hierarchy and measures the actual world-space endpoint.
5. Reports `Reached`, `Moving`, or `Approximate` from the measured endpoint and reference reachability.

The returned geometric solution is the requested kinematic destination; the returned endpoint and error describe the pose actually produced by this update. A reachable target can therefore report `Moving` while the hinges are still progressing. A target outside the constrained solver's reach reports `Approximate`, along with its remaining error. The status is strictly kinematic: it does not mean that a hand touched a surface, that a contact can bear load, or that a physical body is stable.

This is the first connected control loop, not yet a complete feedback controller. It does not consume contact observations to select alternate targets, optimize whole-body balance, integrate dynamics, or solve 3D/multi-chain IK. Those are separate research steps and should be added with explicit tests rather than implied by the current API.

The tests cover speed-limited progress, convergence to a reachable target, honest reporting of an unreachable target, world-anchor translation, invalid input, hierarchy validation, and rejection of solver/hinge limit mismatches. The controller rejects mismatched ranges at construction instead of allowing a geometric solution that the retained hinges can never reach.

## Contact is an observation, not an IK calculation

`ContactObservation` is a data-only handoff from the external physics or domain owner. It records whether contact exists, whether the current load is supported/slipping/unsupported, and optional reported force magnitudes. A load-bearing outcome without an active contact is rejected as contradictory input.

The `AvailableTangentialForceNewtons` value is supplied by the external model; Gestures does not derive it from friction coefficients, normal force, grip, or adhesion. A specialized climbing model may include adhesion, but that mechanism must be implemented and validated by its owner. The hierarchy can retain the latest observation on a joint node, but observing contact does not itself move the joint or fabricate a recovery action. A controller must decide what to request next, and the physics owner remains authoritative for the next measured result.

This is an integration seam, not a physics engine. Contact geometry, multi-contact balance, collision resolution, and whole-body response remain outside this prototype.

## Contact-informed recovery recommendations

`ContactRecoveryPolicy.Recommend` translates an optional `ContactObservation` into a deterministic next-intent recommendation:

- Missing, unknown, or unevaluated observations return `AwaitObservation`.
- `NoContact` recommends `SeekContact`; `Released` recommends `ReacquireContact`.
- An externally assessed `Supported` contact recommends `ContinueCurrentTarget`.
- `Slipping` recommends `SeekAlternativeContact`; `Unsupported` recommends `RepositionToReduceLoad`.

This deliberately stops at a recommendation. It does not produce target coordinates, infer force or friction, move a joint, or claim a recovery succeeded. The calling domain chooses and executes the next target; the external physics/domain owner reports the resulting contact outcome. The mapping is a small deterministic policy seam, not a whole-body balance or climbing controller.

## Tests and next decisions

The unit tests cover bounded hinge progress, all three movement classifications, target completion, joint-limit constraints, zero elapsed time, invalid inputs, parent-to-descendant propagation, no-op updates, child-only changes, root-anchor changes, branched hierarchies, a deterministic sequence compared against full reevaluation, a 256-joint chain, recovery after a non-finite derived transform, the data-only external contact observation contract, and deterministic recommendations for supported, slipping, unsupported, absent, released, and unknown contact states. Next, connect recommendations to caller-owned target selection, broaden failure recovery, and benchmark update counts and total runtime before making performance claims.

This experiment is a correctness baseline. No performance advantage is claimed.
