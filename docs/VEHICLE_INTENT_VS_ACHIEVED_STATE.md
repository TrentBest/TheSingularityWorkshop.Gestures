# Vehicle intent versus achieved state

> **Status: proposed reference fixture, not an implemented flight model.** This document scopes a concrete consumer example for Gestures. It does not add vehicle dynamics, control laws, or authoritative state to the Gestures core.

## The flight-simulator problem

A vehicle has at least two distinct representations in a flight experience:

- **Intended / nominal behavior:** what the selected procedure or maneuver asks the vehicle to do, represented by a reusable Gesture or procedure and its parameters.
- **Achieved / observed behavior:** what the vehicle is actually doing after player input, simulation constraints, damage, environment, and control response have taken effect.

These representations must not be collapsed into one pose. A maneuver can describe a desired roll, pitch, translation, docking approach, landing sequence, or subsystem operation while the player's actual vehicle deviates from it. Gestures can provide a reference and help expose the difference; it must not overwrite the achieved state or claim the maneuver succeeded.

## Responsibility split

| Concern | Owner |
| --- | --- |
| Reusable procedure, keyframes, declared anchors and requirements | Gestures provider |
| Player intent, selected maneuver, and whether to follow or abandon it | Flight experience / controller |
| Mapping named Gesture transforms or anchors to a vehicle model | Consumer-owned adapter |
| Vehicle state, forces, control response, contact, damage, and feasibility | Authoritative vehicle simulation / Physics |
| Displaying intended-versus-achieved differences | Experience / Renderer / presentation layer |
| Selecting and configuring participating MicroBundles | Higher-level manifest and composition boundary |
| Resolving bundle requirements and declared interoperability | Domain contracts and higher-level arbitration |

A declared capability such as `docking` or `landing` is not proof that two providers are compatible. The composition must validate domain requirements where those are declared; unknown compatibility remains unresolved rather than implicitly approved. Even a compatible pairing only permits an attempt—the simulation decides what happens.

## Minimal vertical slice

The first fixture should be intentionally small and CPU-testable:

1. Define one vehicle maneuver reference with stable identity, version, named transforms/anchors, and explicit units/timebase in the fixture contract.
2. Feed the reference and a separate achieved-state sample into a comparison function owned by the fixture or experience adapter.
3. Produce a read-only comparison record: reference identity/time, achieved-state identity/time, comparable fields, deltas, and any fields that cannot be compared.
4. Demonstrate both a close tracking case and a player-divergence case. Divergence must change the comparison result, not mutate the vehicle state.
5. Include a missing-transform or incompatible-unit case and return an explicit unresolved/rejected result instead of silently comparing unlike values.
6. If the maneuver requires another MicroBundle (for example, docking hardware or station services), exercise composition separately: matching capability names alone must not pass compatibility validation.
7. Test deterministic output for identical inputs and verify the reference evaluator never writes to authoritative achieved state.

## What this fixture must not imply

- It is not a flight dynamics or autopilot implementation.
- A reference pose is not a physical prediction unless a separate validated model establishes that meaning.
- A target transform match does not prove the vehicle can reach that transform.
- A docking/landing procedure is not successful because its Gesture reached the final keyframe.
- Gestures does not own the manifest, per-bundle configuration format, global arbitration policy, or vehicle simulation.
- The fixture must not add a direct Gestures-to-Physics project dependency; use a consumer-owned adapter or a neutral MicroBundle contract.

## Acceptance evidence

The fixture is ready to inform a public contract only when tests and inspectable output establish:

- stable reference identity and deterministic evaluation;
- separate immutable inputs for intended and achieved state;
- meaningful deviation output with explicit units and time alignment;
- explicit unresolved results for missing or incompatible comparison data;
- no authority transfer from Gestures to the vehicle simulation;
- no claim of physical success based only on procedure completion.

This reference should be used to derive the smallest useful integration contract. Do not generalize it into a universal vehicle schema before a real consumer demonstrates which fields and compatibility checks are essential.
