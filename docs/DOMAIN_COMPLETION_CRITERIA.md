# Gestures Domain Completion Criteria

## Purpose

This document defines what must be implemented and demonstrated before TheSingularityWorkshop.Gestures can be described as a complete reusable movement-and-choreography domain. It distinguishes an API/data contract from executable behavior and from measured integration.

**Current status: early alpha foundation.** The current CPU pose evaluator, bounded seeded variation, scalar spring-damper exploration, motion-response comparison, and crowd-flow contracts are useful slices. They do not yet constitute the complete domain described below.

## Completion levels

- **Specified** — vocabulary, ownership, inputs, outputs, and invariants are documented.
- **Implemented** — executable code exists in the repository.
- **Verified** — automated tests establish expected behavior and edge cases.
- **Demonstrated** — a reproducible end-to-end sample shows the behavior in a real consumer context.
- **Measured** — correctness and cost are recorded for a named workload and environment.

A feature is not complete merely because its type, roadmap entry, diagram, or example configuration exists.

## Domain gates

### 1. Gesture definition and evaluation

- [x] Normalized keyframes and named anchors exist.
- [x] CPU interpolation exists for translation, rotation, and scale.
- [ ] Define stable semantics for duration, time outside the interval, looping, pause/resume, interruption, reversal, cancellation, and completion.
- [ ] Define blending/composition rules when a gesture is interrupted or another procedure takes control.
- [ ] Establish golden reference vectors and numerical tolerance policy for evaluation.
- [ ] Verify malformed data, boundary times, and deterministic replay across supported runtimes.

**Exit evidence:** a documented lifecycle contract, executable tests for every transition, and stable expected pose outputs.

### 2. Motion response and physical plausibility

- [x] Scalar spring-damper and motion-response comparison references exist on the separate motion-response work branch.
- [ ] Decide which response models are stable Gestures contracts versus exploratory reference math.
- [ ] Extend beyond a single scalar channel where justified: multidimensional root response, velocity/acceleration constraints, and parameterized response curves.
- [ ] Define how an ideal trajectory interacts with momentum, target changes, stopping, and reversals.
- [ ] Compare against analytic or independently calculated cases; document integration error and stability limits.
- [ ] Keep authoritative forces, contacts, and material/world physics in the physics/world owner rather than silently turning Gestures into a general physics engine.

**Exit evidence:** a tested, bounded motion-response contract with documented units, timestep assumptions, error behavior, and explicit ownership of authoritative physics.

### 3. Individual capability and deterministic variation

- [x] Stable seeded variation exists for bounded speed and lateral root offset.
- [ ] Generalize profiles beyond two channels without allowing uncontrolled or unbounded output.
- [ ] Define how experience, skill, fatigue, injury/impairment, load, body proportions, and equipment can modify capability when supplied by the host.
- [ ] Specify stable identity/seed derivation, replay guarantees, and compatibility expectations when profiles evolve.
- [ ] Test that variation is repeatable, bounded, distinguishable between identities, and not regenerated as per-frame noise.

**Exit evidence:** profile-driven variation across multiple meaningful motion channels, with deterministic tests and a documented extension/versioning policy.

### 4. Pose hierarchy, IK, and contact constraints

- [ ] Define optional hierarchy/joint metadata without requiring every entity to use a humanoid skeleton.
- [ ] Implement a minimal rig-independent pose/joint representation or a narrow adapter contract for one.
- [ ] Specify IK/constraint inputs, outputs, solver failure, unreachable targets, and priorities.
- [ ] Model contact intent and outcomes: reach, grip, foot/hand placement, slip, obstruction, missed contact, and release.
- [ ] Define force/capability thresholds as inputs or outcomes, without claiming physical breakage unless the authoritative physics owner validates it.
- [ ] Test reachable/unreachable targets, degenerate geometry, singular cases, and constraint conflicts.

**Exit evidence:** at least one reproducible articulated interaction that resolves targets and handles failure explicitly, while leaving rendering and world collision authority outside the core.

### 5. Provider-owned procedures and interaction anchors

- [x] Named anchors and pose keyframes provide an initial vocabulary.
- [ ] Define provider contracts for procedures such as entering/exiting a vehicle, climbing a hatch, operating a mechanism, and handling tools.
- [ ] Specify anchor resolution, missing/stale anchors, approach alignment, handoff of control, and exit conditions.
- [ ] Demonstrate a vehicle boarding procedure whose ideal motion comes from the vehicle/provider and whose execution varies with entity capability.
- [ ] Specify how a consumer requests, advances, interrupts, and observes a procedure without Gestures taking ownership of the consumer's FSM or world state.

**Exit evidence:** one complete provider-to-evaluator-to-consumer example with success, interruption, and failure paths.

### 6. Crowd traversal and group behavior

- [x] Experience-authored gates/routes, four initial traversal intents, tuning, and lifecycle vocabulary are defined in the crowd-flow contract branch.
- [ ] Implement a deterministic fixed-step CPU reference stepper and explicit per-agent runtime state.
- [ ] Implement route progression, obstacle/occupancy queries through host-supplied interfaces, yielding, local avoidance, joining/rejoining, and exit completion.
- [ ] Implement queues and bottleneck handling; specify deadlock detection/recovery and safe fallback behavior.
- [ ] Implement group membership, shared intent/destination, loose cohesion, splitting/rejoining, and non-synchronized individual variation.
- [ ] Test head-on traffic, narrow doors, cross-flow, blocked routes, groups meeting, sudden destination changes, and deterministic replay.
- [ ] Benchmark representative populations and report quality/cost together; only then consider spatial indexing, packed storage, or GPU evaluation.

**Exit evidence:** reproducible crowd scenarios with measured behavior, no hidden dependence on city-specific topology, and explicit world/physics authority boundaries.

### 7. Choreography and multi-entity coordination

- [ ] Define sequence/role contracts for coordinated procedures, including prerequisites, dependencies, timing windows, and handoff conditions.
- [ ] Support staggered starts and event-driven synchronization without forcing every participant onto one timeline.
- [ ] Demonstrate a small starport sequence: ground crew clears equipment/covers, flight crew boards, and each action respects named anchors and role ownership.
- [ ] Demonstrate recovery when a participant is delayed, absent, blocked, or fails a step.
- [ ] Reuse the same primitives for non-biological or mechanical actors where the procedure fits.

**Exit evidence:** one multi-role sequence that can be replayed deterministically and recovers from at least one injected failure.

### 8. Presentation and consumer interoperability

- [ ] Specify a narrow evaluated-pose/output contract suitable for Renderer and other consumers.
- [ ] Keep presentation output separate from authoritative world position, collision, and simulation state.
- [ ] Establish CPU reference outputs and parity tolerances before implementing a GPU evaluator.
- [ ] Demonstrate LOD/fidelity changes without changing the semantic outcome of the procedure or producing visible discontinuities.
- [ ] Keep core free of Unity, WPF, renderer, GPU API, and city/game-specific dependencies.
- [ ] Provide a sample consumer and adapter guidance without requiring every consumer to adopt the same renderer or rig.

**Exit evidence:** an engine-neutral sample and a consumer integration that can compare presentation output against the CPU reference.

### 9. Data durability, observability, and maintainability

- [ ] Define schema/version compatibility before freezing serialized gesture data.
- [ ] Add serialization round-trip tests only after the model and compatibility policy stabilize.
- [ ] Provide useful diagnostic output for invalid definitions, unresolved anchors, failed constraints, stalled agents, and interrupted procedures.
- [ ] Review public API naming, XML documentation, nullable contracts, warnings, package metadata, and dependency boundaries.
- [ ] Document guarantees versus non-guarantees and keep roadmap/status aligned with actual code.

**Exit evidence:** versioned data contracts, actionable diagnostics, clean CI, and a release artifact whose contents and dependencies have been inspected.

## Minimum bar for saying "domain complete"

A complete-domain claim requires, at minimum:

1. The lifecycle and deterministic evaluation contract is specified and tested.
2. Individual capability variation is meaningful, bounded, and reproducible.
3. At least one articulated/contact-aware procedure is executable and handles failure.
4. Provider-owned interaction anchors work end-to-end in a consumer.
5. Crowd traversal has an executable CPU solver with route following, avoidance/yielding, group behavior, and deterministic replay.
6. At least one coordinated multi-entity choreography is demonstrated with recovery.
7. Consumer output has a defined contract, and CPU reference results are reproducible.
8. CI, API/docs review, dependency audit, and reproducible examples support the release claim.

GPU execution, massive-population scale, advanced biomechanics, and a complete physics engine are **not automatically required** to claim the core domain is implemented. They require separate, explicit capability claims backed by measurements. Likewise, an implemented reference solver does not imply any particular population scale until benchmarked.

## Recommended order

1. Stabilize lifecycle/time semantics and golden evaluator tests.
2. Complete the deterministic CPU crowd stepper; this converts the current crowd contract into behavior.
3. Build one provider-owned boarding/interaction procedure with anchors, capability variation, interruption, and failure.
4. Add a minimal articulated/contact adapter and one testable IK/constraint path.
5. Compose a small multi-role choreography and demonstrate it end-to-end.
6. Establish the Renderer/Visualize output boundary and parity test before GPU work.
7. Close diagnostics, versioning, API review, benchmarks, and release evidence.

Keep each milestone independently testable. Do not couple core Gestures to FSM_COS, WebPage, Renderer, Visualize, or FSM_Physics; use narrow contracts and consumer-side adapters.
