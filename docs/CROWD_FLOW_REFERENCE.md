# Crowd Flow Reference

## The intended division of responsibility

An experience owns *where traffic enters and leaves*. Gestures owns the reusable crowd-traversal intent and its behavior vocabulary. The host/world remains authoritative for geometry, access, collision, and actual movement.

For example, a shop experience defines a sidewalk entrance gate, a shop exit gate, and a route through the doorway. A city experience can define other gates linking a sidewalk to a parking garage, transit concourse, plaza, or storefront. The same crowd behavior model can serve each route without hardcoding any particular city's topology.

## Initial code contract

- `CrowdFlowGate`: stable ID and experience-local planar position.
- `CrowdFlowRoute`: stable route ID, named entrance and exit gates, and optional intermediate waypoints.
- `CrowdFlowScenario`: validates unique gate/route IDs and that each route references existing gates.
- `CrowdTraversalMode`: individual unhurried, individual hurried, group unhurried, and group hurried.
- `CrowdTraversalProfile`: baseline-speed multiplier, desired personal-space radius, and group-cohesion tuning.
- `CrowdTraversalState`: lifecycle vocabulary for waiting, joining, traversing, yielding, rejoining, exiting, completion, and replanning. A host can map these to FSM_API state groups.

## Example scenario

A busy shop can author these gates and routes:

- `sidewalk-west` -> `shop-interior`: customers enter.
- `shop-interior` -> `sidewalk-east`: customers leave.
- `sidewalk-north` -> `garage-ramp`: pedestrians head to the parking garage.
- `garage-lobby` -> `sidewalk-south`: pedestrians emerge from the garage.

These are experience-authored topology, not fixed assumptions inside Gestures. Routes can include intermediate points where a host knows the path must bend around a display, queue, column, or curb.

## What the four intents mean

| Intent | Expected policy |
| --- | --- |
| Individual, unhurried | Comfortable pace; allow natural route choice and yielding. |
| Individual, hurried | Higher desired speed; still respect obstacles and right-of-way rules. |
| Group, unhurried | Shared destination and pace with loose cohesion; members need not march in lockstep. |
| Group, hurried | Higher desired speed while preserving group awareness and avoiding teleport-like convergence. |

These are intent profiles, not four canned animations. The same person can transition between them as their context changes. Stable per-identity variation should prevent the crowd from looking synchronized.

## Important boundary: what this reference does not claim

This first slice is a validated data contract, not yet a crowd solver. It does **not** implement spawn scheduling, queueing, pathfinding, collision avoidance, deadlock resolution, world occupancy, group membership updates, or a large-population performance benchmark. The state names and tuning values are ready for those behaviors to be added behind explicit interfaces; they do not pretend those behaviors already exist.

The next executable milestone is a deterministic CPU reference stepper over host-supplied routes, with explicit per-agent state, local avoidance/yielding, group cohesion, and reproducible crowd snapshots. Only after the reference behavior is tested should packed/GPU layouts or event-driven update rates be selected.
