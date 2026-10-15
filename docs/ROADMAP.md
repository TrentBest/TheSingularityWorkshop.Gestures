# Roadmap

## Milestone 0 — Reference contract (current)

- [x] Engine-neutral .NET 8 library.
- [x] Validated normalized keyframes and named anchors.
- [x] CPU interpolation for position, rotation, and scale.
- [x] Deterministic bounded speed and root-offset variation.
- [x] Unit tests for core invariants and repeatability.
- [x] First constrained two-link planar IK CPU reference, with deterministic geometry tests.
- [x] Accessible IK theory guide with animated SVG geometry and explicit implementation boundaries.
- [x] Stateful IK research theory, animated hierarchy concept, and tracked research issue; the full architecture remains unimplemented and unbenchmarked.
- [x] CI build/test/pack artifact; NuGet publishing disabled.
- [x] Confirm CI is green on the IK reference pull request after edge-case tests.
- [x] Require solver angle limits to exactly match retained hinge limits; test rejection of mismatches.
- [ ] Review public API names and package metadata before alpha publication.
- [x] Add an initial agent-independent movement-course geometry contract with stable element IDs, ordered regions, consistent units, and validated axis-aligned bounds.

## Stateful IK research track — single-joint prototype stage

- [x] Define a minimal joint angle controller separately from its three-way motion classification (positive, negative, stationary).
- [x] Specify and test parent-to-descendant transform propagation in a full-reevaluation planar hierarchy.
- [x] Prototype dirty-path incremental invalidation and compare results with full reevaluation for representative updates.
- [x] Compare incremental and full evaluation across a deterministic event sequence and branched hierarchy.
- [x] Exercise incremental propagation through a 256-joint chain.
- [x] Test recovery after an incremental evaluation detects a non-finite derived transform.
- [x] Define a data-only contact observation handoff from the external physics/domain owner; Gestures does not infer physical contact or support.
- [ ] Fully separate and integrate geometric reachability, control intent, physical integration, collision/contact, and domain-owned damage.
- [x] Add a deterministic contact-observation-to-recovery recommendation policy, with tests for supported, slipping, unsupported, absent, released, and unknown states.
- [x] Connect external contact observations to domain-owned target selection and the stateful planar IK loop; defer motion when no usable observation/target is available, and test that failed selection does not mutate the retained pose. Contact, load-bearing, slip, and release remain physics/domain-owned (rock-climbing example).
- [x] Prototype a single stateful hinge without assuming an FSM_API dependency; document its contract and limitations.
- [x] Connect constrained two-link IK to retained hinges and measured hierarchy output in a speed-limited kinematic control loop; test reachable convergence, unreachable targets, and world-anchor offsets.
- [x] Build a small parent/child hierarchy with explicit transform propagation and an initial dirty-path incremental update.
- [x] Compare incremental updates with full reevaluation for correctness before performance claims.
- [ ] Benchmark 1–10,000 joints and record total-step cost, allocations, update ratios, error, and failure behavior.

Tracked in [Stateful IK research issue #11](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/issues/11).

## Movement-course research track — authored challenge contract

- [x] Define engine-neutral ordered course elements for approach/acceleration zones, hurdles, clearance and landing zones, continuation, gaps, vaults, balance sections, climbs, checkpoints, and finish markers.
- [x] Validate stable course-local IDs, finite centers, positive finite extents, and immutable authored order.
- [x] Define caller-supplied outcome observations and a deterministic evaluator that never treats target reachability as proof of course completion; the data contract does not authenticate evidence provenance.
- [x] Add a minimal hurdle-course fixture and tests for clear, collision, missed hurdle, unsafe landing, missing evidence, and indeterminate evidence.
- [ ] Add deterministic seeded course generation and a serialization contract only after the authored model stabilizes.

Tracked in [movement-course research issue #12](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/issues/12).

## Milestone 1 — Real domain examples

- [ ] Demonstrate skill as a movement distribution, not only a scalar rating: render a ghost ideal pose/aim line, show narrow and broad fuzzy movement envelopes, and compare the resulting outcome distributions on a target. Keep pose error, movement variance, and outcome spread as separate measurements; use explicitly seeded/supplied samples and label the demo as illustrative rather than physical validation.
- [x] Add a deterministic, seeded 2D sample model with separate mean pose error, movement variance, and outcome spread metrics plus repeatability/parameter tests; this is model groundwork, not the interactive visual demonstration.
- [x] Add a deterministic SVG comparison renderer that draws generated movement/outcome samples, an ideal ghost pose/aim line, a bullseye, and separately reported metrics; the renderer returns SVG text but is not yet wired to an interactive host.
- [x] Add a first static [skill-distribution concept visual](assets/skill-distribution-concept.svg) showing the intended comparison and the distinction between movement samples and target outcomes; this is a design artifact, not the implemented interactive demo.
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
