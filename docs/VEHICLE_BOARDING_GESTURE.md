# Vehicle boarding Gesture example

This example describes an idealized boarding pose progression using the current engine-neutral Gesture API. It is a data contract and interpolation example—not a vehicle controller, animation rig, or physical boarding simulation.

## What the example contains

The test fixture `vehicle.board` contains three normalized keyframes:

1. **Approach:** a standing pose near the vehicle entry.
2. **Reach and transfer:** hands move toward provider-defined grip points while the body rises.
3. **Settle:** the authored pose moves toward a seated configuration.

The transform names (`root`, `pelvis`, `left_hand`, `right_hand`, and feet) are illustrative labels, not a required humanoid skeleton. A consuming rig maps whichever names it supports.

Three semantic anchors express intent without hard-coding a vehicle layout:

- `entry-handhold` — attached to the provider's `right_hand` transform.
- `vehicle-grip` — attached to the provider's `left_hand` transform.
- `seat-target` — attached to the provider's `pelvis` transform.

Anchor offsets are authored locally. A consumer resolves them against its own character and vehicle data; the Gesture library does not discover a door, move a vehicle, or guarantee that the anchors can physically meet.

## Run the focused tests

```sh
dotnet test TheSingularityWorkshop.Gestures.slnx --configuration Release --filter FullyQualifiedName~VehicleBoardingGestureTests
```

## Responsibility boundary

`GestureEvaluator` interpolates the authored local transforms and reports normalized progression. Reaching progression 1 means the authored procedure reached its final keyframe; it does **not** prove that boarding succeeded.

The owning application or simulation must decide whether the vehicle is present and accessible, resolve moving anchors, handle collision and hand contact, determine support and balance, and confirm seat occupancy. If a grip is missed or the vehicle moves, the caller owns interruption, recovery, and whether to retry. These outcomes must not be inferred from pose reachability alone.

This example intentionally does not define universal rig metadata, vehicle dimensions, collision rules, root-motion policy, or FSM ownership.
