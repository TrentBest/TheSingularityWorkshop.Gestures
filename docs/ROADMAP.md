# Roadmap

## Milestone 0 — Reference contract (current)

- [x] Engine-neutral .NET 8 library.
- [x] Validated normalized keyframes and named anchors.
- [x] CPU interpolation for position, rotation, and scale.
- [x] Deterministic bounded speed and root-offset variation.
- [x] Unit tests for core invariants and repeatability.
- [x] CI build/test/pack artifact; NuGet publishing disabled.
- [ ] Confirm CI is green on the initial pull request.
- [ ] Review public API names and package metadata before alpha publication.

## Milestone 1 — Real domain examples

- [ ] Add an illustrative soldier movement Gesture with a rig-independent sample rig.
- [x] Add a first provider-owned starport departure task graph with role branching, ground-crew return, access sealing, manifest verification, and explicit clearance/departure gates.
- [ ] Turn the starport task graph into actual provider-owned Gesture procedures using resolved boarding, hose, cover, landing-gear, and station anchors.
- [ ] Specify interruption, reversal, pause/resume, and re-entry semantics without owning the FSM.
- [ ] Decide whether timing curves belong in the definition or remain a caller policy.
- [ ] Add deterministic golden test vectors and numerical tolerance policy.

## Starport vertical slice

- [x] Validate the departure task DAG and reject duplicate IDs, unknown dependencies, and cycles.
- [x] Evaluate ready/blocked tasks from host-supplied outcomes and gate clearance on explicit crew/equipment/access/manifest evidence.
- [x] Test role branching, failed-ground-task blocking, missing readiness evidence, and ATC clearance ordering.
- [ ] Implement the deterministic fixed-step crowd solver needed for approach routes, queues, bottlenecks, yielding, and replay.
- [ ] Add executable boarding and ground-service Gestures with provider-owned anchors and failure recovery.
- [ ] Connect host/Physics observations for contact, connector release, cover security, obstruction, and door sealing.
- [ ] Demonstrate success plus injected failure/recovery in a consumer; keep the WebPage experience a clearly labeled reference until it consumes authoritative evidence.

## Milestone 2 — Rig and constraints

- [ ] Define optional hierarchy metadata without requiring a universal humanoid skeleton.
- [ ] Evaluate root-motion extraction versus root presentation offset.
- [ ] Define attachment/anchor resolution expectations for consumers.
- [ ] Investigate IK and constraints as optional capabilities, not mandatory core dependencies.
- [ ] Add serialization only after the model and compatibility policy stabilize.

## Milestone 3 — Renderer and GPU validation

- [ ] Agree on a narrow evaluated-pose interchange contract with Renderer.
- [ ] Produce CPU reference outputs for representative gestures and population sizes.
- [ ] Prototype GPU evaluation behind Visualize/backend-specific code.
- [ ] Compare CPU and GPU results, memory traffic, dispatch cost, and event-horizon fidelity.
- [ ] Evaluate compact textures against structured/storage buffers using measured workloads.

## Release gate

Before publishing: CI build, tests, and package artifact must pass; public API and warning review must complete; dependencies and package contents must be audited; version increment must be reviewed; README and XML API documentation must be accurate; and the maintainer must explicitly authorize publication.

A green build alone does not mean the Gesture schema is frozen or that GPU parity has been demonstrated.
