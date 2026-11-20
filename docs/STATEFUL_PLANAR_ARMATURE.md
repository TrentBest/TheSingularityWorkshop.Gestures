# Stateful planar armature contract

## Purpose and boundary

StatefulPlanarArmature is the first versioned, deterministic CPU reference fixture for a teachable two-link arm. It composes retained hinges and the planar hierarchy behind stable joint, link, and end-effector identities.

This is a **kinematic target controller**, not physical actuation. It advances local joint angles under declared angular-speed and legal-angle bounds. It does not calculate torque, force, inertia, acceleration, contact, load, energy, or physical success. No velocity is reported because this slice does not define a velocity state. No neural-network, Physics, UI, renderer, or host package is required.

## Definition and topology

PlanarArmatureDefinition fixes scenario identity; unique shoulder, elbow, link, and end-effector IDs; a world-space 2D root anchor; positive link lengths; finite inclusive joint ranges and initial angles; and maximum angular speed for each joint.

The topology is shoulder link → elbow link → end-effector position. Each link extends along local +X at zero angle; the elbow angle is relative to the shoulder link. The root anchor is fixed by the definition.

## Episode lifecycle

Create the armature, then call Reset(episodeId, seed) before observing or stepping. Reset restores exact initial angles, reevaluates the hierarchy, and sets step index to zero. The seed is recorded as run metadata; this first fixture does not consume it for random variation.

Each action must match the active episode, contract version, and next expected step. A rejected action leaves both joints and the step index unchanged. A successful action advances the step index once. Resetting and replaying the same ordered actions produces identical observations.

## Observation, action, and result

- PlanarArmatureObservation reports version, scenario/episode identity, seed, step index, root anchor, stable joint IDs, achieved angles, legal ranges, end-effector ID, and measured world-space endpoint.
- PlanarArmatureAction declares version, episode ID, expected step, action mode, and requested shoulder/elbow target angles.
- PlanarArmatureStepResult preserves the original request, separately reports range-clamped accepted targets, includes each bounded hinge transition, and returns the post-step observation.

Version 1 supports JointAngleTargets. The original request is never overwritten. Accepted targets are clamped to legal intervals; achieved angles may still differ because per-joint speed budgets limit movement during the current time step.

## Deliberate omissions

This first slice does not yet provide an end-effector-target action mode, learned or baseline policy, held-out generalization suite, serialized run-record format, batch/GPU path, or Physics adapter. Those remain follow-up work. External contact or physical evidence must cross an explicit consumer-owned adapter and remain distinguishable from kinematic observations.

The CPU fixture is a correctness reference, not a performance claim. Tests cover reset/replay determinism, stable identities, speed/range constraints, requested-versus-accepted-versus-achieved values, and rejection of stale, wrong-episode, wrong-version, unsupported-mode, and non-finite actions before mutation.


## Deterministic baseline and held-out fixtures

`PlanarArmatureBaselinePolicy` is a small, non-learned reference policy. Given a versioned observation and a world-space end-effector target, it validates scenario/topology identity and uses the constrained two-link IK solver to produce the next `JointAngleTargets` action. It does not mutate the armature, execute the action, or claim physical success.

The baseline policy tests vary link lengths, root anchors, initial joint angles, and target positions independently of a single training fixture. They include both reachable and unreachable geometric requests. The policy is deterministic for the same definition, observation, and target. These fixtures are an initial held-out correctness check, not a statistical generalization study or a learned-policy comparison.


## Reproducible planar-armature run records

Issue #32 now has an immutable `PlanarArmatureRunRecord` contract (schema version 1). A record captures the scenario and episode identities, reset seed, fixed timestep, world-space target, target tolerance, initial observation, and an ordered sequence of requested actions, accepted joint targets, achieved observations, and measured target errors. Appending a step returns a new record and rejects mismatched episodes or step sequences.

The record is composed of ordinary public .NET data contracts and can be serialized by a consumer with `System.Text.Json`. It deliberately records only values the kinematic reference actually produces. It does not add velocity, force, torque, contact, load, energy, or physical-success fields. A target is reached only when the measured end-effector position is within the caller-supplied tolerance; the record does not claim physical task completion.
