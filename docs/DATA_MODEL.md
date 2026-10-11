# Gesture Data Model

## Normalized keyframes

Each keyframe has a time in the closed interval [0, 1]. A valid definition contains at least two frames, starts exactly at 0, ends exactly at 1, and uses strictly increasing times. This makes evaluation independent of the real duration chosen by the caller.

The transform map is keyed by stable rig transform names. This first version does not impose a humanoid skeleton or assume a fixed number of joints. A provider may use a soldier rig, a vehicle mechanism, a hatch, or another named transform hierarchy.

## Transform interpolation

For a segment with normalized endpoints t0 and t1, interpolation amount is:

u = clamp((t - t0) / (t1 - t0), 0, 1)

- Position: linear interpolation.
- Scale: linear interpolation.
- Rotation: quaternion spherical interpolation, after normalizing inputs.

If a transform exists in only one endpoint, the current reference evaluator holds that transform across the segment. Consumers should not use a missing transform as an implicit instruction to delete a joint.

## Anchors

An anchor has a semantic name, a parent transform identifier, and a local transform. Examples might include cockpit-entry, left-hand-grip, ladder-rung-03, or operator-seat.

Anchors express the spatial relationships needed to stage a procedure. They do not guarantee that the target is reachable or collision-free. The world/physics layer remains responsible for those questions.

## Evaluated output

EvaluatedPose contains normalized progress, a completion indication based on elapsed time and configured duration, the evaluated local transform map, and a separate deterministic root offset for individual variation.

The root offset is a presentation input, not a command to move the authoritative entity. If gameplay requires actual locomotion, that must be computed and committed by the simulation/behavior layer.

## Deliberately not frozen yet

The first model intentionally does not define a universal humanoid bone schema, collision or IK solving, event tracks, a GPU texture/buffer layout, network serialization, a full constraint solver, or automatic FSM construction. These require concrete consumer scenarios and tests before they become package commitments.
