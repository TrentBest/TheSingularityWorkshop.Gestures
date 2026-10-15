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

## Motion-law research track — scalar response prototype

- [x] Add a one-dimensional stateful spring-damper reference with validated effective mass, stiffness, and damping.
- [x] Specify semi-implicit Euler stepping and preserve retained state on invalid input or non-finite computed output.
- [x] Test deterministic replay, fixed-step convergence for a declared parameter set, zero-duration behavior, and validation failures.
- [x] Document that this is a scalar response primitive, not a physical solver and not yet integrated into GestureEvaluator.
- [ ] Compare direct interpolation and candidate response laws across declared target trajectories, timesteps, and parameter ranges.
- [ ] Add seeded experience-profile parameter sampling and a side-by-side population visualization only after the parameter ranges and comparison metrics are explicit.
- [ ] Decide whether the eventual motion-law contract should be stateful, absolute-time evaluable, or expose both as separate capabilities.

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
