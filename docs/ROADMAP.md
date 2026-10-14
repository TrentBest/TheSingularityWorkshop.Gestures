# Roadmap

## Milestone 0 — Reference contract (current)

- [x] Engine-neutral .NET 8 library.
- [x] Validated normalized keyframes and named anchors.
- [x] CPU interpolation for position, rotation, and scale.
- [x] Deterministic bounded speed and root-offset variation.
- [x] Unit tests for core invariants and repeatability.
- [x] First constrained two-link planar IK CPU reference, with deterministic geometry tests.
- [x] Accessible IK theory guide with animated SVG geometry and explicit implementation boundaries.
- [x] CI build/test/pack artifact; NuGet publishing disabled.
- [x] Confirm CI is green on the IK reference pull request after edge-case tests.
- [ ] Review public API names and package metadata before alpha publication.

## Milestone 1 — Real domain examples

- [ ] Add an illustrative soldier movement Gesture with a rig-independent sample rig.
- [ ] Add a provider-owned interaction example, such as boarding a vehicle through named anchors.
- [ ] Specify interruption, reversal, pause/resume, and re-entry semantics without owning the FSM.
- [ ] Decide whether timing curves belong in the definition or remain a caller policy.
- [ ] Add deterministic golden test vectors and numerical tolerance policy.

## Milestone 2 — Rig and constraints

- [ ] Define optional hierarchy metadata without requiring a universal humanoid skeleton.
- [ ] Evaluate root-motion extraction versus root presentation offset.
- [ ] Define attachment/anchor resolution expectations for consumers.
- [ ] Investigate how IK and constraints should remain optional capabilities, not mandatory core dependencies.
- [ ] Extend the CPU reference beyond planar two-link geometry only when a concrete consumer and tests justify it.
- [ ] Add serialization only after the model and compatibility policy stabilize.

## Milestone 3 — Renderer and GPU validation

- [ ] Agree on a narrow evaluated-pose interchange contract with Renderer.
- [ ] Produce CPU reference outputs for representative gestures and population sizes.
- [ ] Prototype GPU evaluation behind Visualize/backend-specific code.
- [ ] Compare CPU and GPU results, memory traffic, dispatch cost, and event-horizon fidelity.
- [ ] Evaluate compact textures against structured/storage buffers using measured workloads.
- [ ] Measure the workload crossover where GPU batching beats CPU evaluation; include dispatch and data-transfer costs.

## Release gate

Before publishing: CI build, tests, and package artifact must pass; public API and warning review must complete; dependencies and package contents must be audited; version increment must be reviewed; README and XML API documentation must be accurate; and the maintainer must explicitly authorize publication.

A green build alone does not mean the Gesture schema is frozen or that GPU parity has been demonstrated.
