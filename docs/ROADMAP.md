# Roadmap

Historical research detail from the planar IK reference branch is preserved in the [research roadmap archive](ROADMAP_RESEARCH_ARCHIVE.md); this file remains the current status summary.

See [Domain Completion Criteria](DOMAIN_COMPLETION_CRITERIA.md) for the explicit gates required before claiming the Gestures domain is complete.

## Milestone 0 — Reference contract (current)

- [x] Engine-neutral .NET 8 library.
- [x] Validated normalized keyframes and named anchors.
- [x] CPU interpolation for position, rotation, and scale.
- [x] Deterministic bounded speed and root-offset variation.
- [x] Unit tests for core invariants and repeatability.
- [x] Document evaluator timing boundaries and caller-owned lifecycle responsibilities; test negative time, invalid timing inputs, endpoint clamping, and completion.
- [x] Fixed-input deterministic reference vectors and documented numerical tolerances.
- [x] CI build/test/pack artifact; NuGet publishing disabled.
- [ ] Confirm CI is green on the initial pull request.
- [ ] Review public API names and package metadata before alpha publication.

## Motion response reference track

- [x] Define a bounded scalar spring-damper tracking reference with explicit units, stable inputs, and deterministic fixed-step semantics.
- [x] Compare direct kinematic interpolation with spring-damper response as distinct behavior models.
- [x] Define experience-profile ranges as stable per-entity parameters rather than per-frame randomization.
- [ ] Validate response envelopes and stability across declared timestep/parameter ranges before considering broader integration.
- [ ] Connect any future articulated response to Physics-owned achieved state and contact evidence; do not claim a general physics solver in Gestures.

## Crowd-flow reference track

- [x] Add experience-authored gates/routes with validated IDs and host-local waypoints.
- [x] Define individual/group × unhurried/hurried intent profiles and lifecycle state vocabulary.
- [x] Document the ownership boundary and explicitly mark crowd solving as future work.
- [ ] Implement a deterministic CPU reference stepper with per-agent state, yielding/local avoidance, and group cohesion.
- [ ] Test route following, crowded bottlenecks, group arrival, and deterministic replay before any population-scale claims.

## Milestone 1 — Real domain examples

- [ ] Add an illustrative soldier movement Gesture with a rig-independent sample rig.
- [x] Add a first provider-owned starport departure task graph with role branching, ground-crew return, access sealing, manifest verification, and explicit clearance/departure gates.
- [x] Add three CPU-evaluable starport pose-sequence fixtures for ramp boarding, service-umbilical handling, and cover handling, with provider-owned semantic anchors.
- [ ] Turn those initial pose fixtures into provider-validated procedures using resolved boarding, hose, cover, landing-gear, and station anchors; add articulated/contact-aware outcomes.
- [ ] Specify procedure-coordinator behavior for interruption, reversal, pause/resume, re-entry, and control handoff without owning the consumer's FSM.
- [ ] Decide whether timing curves belong in the definition or remain a caller policy.

## Starport vertical slice

- [x] Validate the departure task DAG and reject duplicate IDs, unknown dependencies, and cycles.
- [x] Evaluate ready/blocked tasks from host-supplied outcomes and gate clearance on explicit crew/equipment/access/manifest evidence.
- [x] Test role branching, failed-ground-task blocking, missing readiness evidence, and ATC clearance ordering.
- [ ] Implement the deterministic fixed-step crowd solver needed for approach routes, queues, bottlenecks, yielding, and replay.
- [ ] Add executable boarding and ground-service Gestures with provider-owned anchors and failure recovery.
- [ ] Connect host/Physics observations for contact, connector release, cover security, obstruction, and door sealing.
- [ ] Demonstrate success plus injected failure/recovery in a consumer; keep the WebPage experience a clearly labeled reference until it consumes authoritative evidence.

## Speech scene and city-scale population vertical slice

- [x] Record the moniker-to-balcony narrative, advisor beats, public-screen echo, crowd recognition wave, and ownership boundaries in [Speech Scene and Soldier Population](SPEECH_SCENE_POPULATION.md).
- [x] Add an explicitly conceptual animated SVG to explain the intended scene and event-horizon representation tiers; do not mislabel it as runtime evidence.
- [x] Include the conceptual speech-scene SVG in CI XML parsing and whitespace checks; this validates file hygiene, not runtime behavior.
- [x] Specify deterministic crowd-response inputs, timing separation, stable identity variation, replay semantics, test gates, and living-SVG evidence requirements in [Deterministic Crowd-Response Scheduling](CROWD_RESPONSE_SCHEDULING.md).
- [x] Add the first pure deterministic CPU crowd-response scheduler with per-participant intent output, stable bounded variation, and a version-pinned hash/mapping contract.
- [x] Verify scheduler reference-vector, replay, order-independence, bounds, invalid-input, and overflow tests in CI.
- [ ] Implement and test the advisor salute/address procedure with explicit progress, interruption, and completion semantics.
- [ ] Implement and test shoulder-contact / guided-threshold choreography against provider-supplied anchors; keep physical contact outcomes with the host/Physics.
- [ ] Prototype distance-aware cheer arrival using source/listener distance and a documented sound-speed approximation or authoritative acoustic result.
- [ ] Demonstrate a public-screen echo driven by the same player action/timebase with explicit display latency.
- [ ] Prototype observer-relative representation tiers with hysteresis and no identity/gesture reset on tier transitions.
- [x] Generate and validate a living SVG from actual scheduler output in CI; keep concept art separate from runtime evidence.
- [ ] Profile staged population sizes; record CPU cost, upload bytes, GPU time, and visible representation count before making scale claims.


## Singularity Studios provider track

- [x] Document the proposed ownership boundary and [Gestures procedure plan](SINGULARITY_STUDIOS_GESTURE_PLAN.md).
- [x] Publish a matching [WebPageLLM integration handoff](https://github.com/TrentBest/WebPage/blob/development/docs/integrations/SINGULARITY_STUDIOS_WEBPAGELLM_HANDOFF.md) on WebPage development.
- [x] Publish the [AnyApp desktop consumer handoff](https://github.com/TrentBest/AnyApp/blob/development/docs/integrations/SINGULARITY_STUDIOS_ANYAPP_HANDOFF.md) and cross-link the host briefs.
- [x] Inspect the current WebPage host page/project dependencies and AnyApp manifest, runtime composition, catalog, and desktop proof boundaries before proposing a shared contract.
- [x] Publish a repository-specific [FSM_COS LLM handoff](TheSingularityWorkshop.FSM_COS_LLM.md) requesting the real manifest-import, bundle identity, catalog, and configuration-source contract before authoring runtime files.
- [ ] Agree on a versioned provider-owned stage/take fixture and consumer boundary; the host briefs are coordination proposals, not a finalized API.
- [ ] Define provider-owned stage/rig anchors and a reusable 30-second take fixture.
- [ ] Add test-backed performer and camera/equipment procedure examples using existing engine-neutral contracts; do not claim physical outcomes without host/Physics observations.
- [ ] Generate a living timeline/overhead SVG from actual procedure outputs and task schedule data.
- [ ] Validate anchor rebinding, blocked-path reporting, deterministic replay, and task gates before expanding to large indoor battle stages.

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
