# Starport Departure Experience

## Intent

Build a repeatable, inspectable starport departure that exercises Gestures as a physical-procedure domain rather than as a collection of disconnected animation clips. The first vertical slice is a spacecraft departure from muster through role routing, ground-service clearance, boarding, sealing, manifest verification, external departure clearance, and release.

The experience is deliberately ambitious in scope, but each capability must be marked **Specified**, **Implemented**, **Verified**, **Demonstrated**, or **Measured** according to its evidence. A task graph is not yet a crowd simulator, and a completed choreography task is not proof of physical success unless the host supplies that evidence.

The WebPage integration handoff is tracked separately in [WebPage PR #119](https://github.com/TrentBest/WebPage/pull/119); that PR is a documentation handoff, not a browser implementation.

## The departure story

1. All assigned personnel gather at a named muster point.
2. Everyone follows the shared route to the spacecraft's access door.
3. A role router branches the group:
   - **Flight crew** board via the ramp, move to assigned stations, and report accounted-for status.
   - **Ground crew** disconnect and stow service hoses/umbilicals, clear covers, complete landing-gear/departure checks, and secure mass-driver or launch-interface covers where fitted.
4. Ground crew return to the designated safe assembly area. The last flight-crew member must be aboard before the access route is sealed.
5. The flight crew verifies the manifest and readiness checklist.
6. Departure control may request ATC clearance only after all pre-clearance tasks and explicit readiness evidence are present.
7. Departure may be released only after the clearance request is complete and the host reports that ATC/departure-control clearance has actually been granted.

No branch may skip a failed prerequisite. If a hose is still connected, a cover is missing, a crew member is unaccounted for, the access door cannot seal, or clearance is denied, the experience remains in a blocked/recovery state rather than claiming success.

## Current implementation slice

The engine-neutral Gestures library now contains:

- `StarportDepartureScenario`: immutable task graph with stable IDs, role ownership, action labels, and dependencies.
- `CreateStandardDeparture()`: a first scenario fixture with shared muster/routing, parallel flight-crew and ground-crew work, ground-crew return, access sealing, manifest verification, clearance request, and departure release.
- `StarportDepartureEvaluator`: deterministic evaluation of ready tasks, blocked tasks, clearance eligibility, and departure eligibility from caller-supplied task outcomes and external evidence.
- `StarportDepartureEvidence`: explicit flags for all flight crew accounted for, equipment clear, ground crew returned, access sealed, manifest verified, and ATC clearance granted.
- Unit tests for role branching, dependency cycles and references, blocked recovery after a failed ground task, missing evidence, and the final clearance gate.

The pose samples are deliberately authored reference sequences with illustrative local units. The current evaluator interpolates named transforms independently; it does not compose a skeleton, solve IK, establish grip/contact, or verify equipment state. They are executable inputs to the CPU evaluator, not proof that a person can perform the procedure. The task graph is an executable choreography/readiness contract, **not** a simulation of people or equipment. The host still owns routes, collision/obstruction checks, physical contact, crew identity/counts, task observation, equipment state, access hardware, and actual ATC authorization. The evaluator deliberately does not infer those facts from a task label.

## Experience topology and anchors

The city/venue provider should supply stable IDs and resolved anchors rather than baking world coordinates into Gestures. A full scene should include:

- Muster / briefing point and a shared approach corridor.
- Spacecraft access-door threshold, ramp/ladder, and interior station anchors.
- Named service connections, hose stow points, cover storage points, landing-gear inspection points, and launch-interface / mass-driver cover anchors where the craft supports them.
- Ground-crew safe assembly zone and the boundary that proves the departure envelope is clear.
- Access seal/door state, manifest/checklist station, and departure-control handoff.
- Alternate route and recovery/hold points for blocked or inaccessible anchors.

Anchor resolution and world geometry belong to the provider/host. If a required anchor is missing or stale, the interaction must report a recoverable failure; it must not invent a substitute coordinate.

## Follow-on milestones

### A. Physical procedures

Extend the initial boarding, service-umbilical, and cover reference sequences into provider-owned procedures using resolved craft anchors and richer authored tracks. Add approach/ramp alignment, entering and seating at a station, inspecting landing gear, carrying a tool, and returning to the safe zone. Validate each procedure against a concrete provider anchor catalogue. Add capability profiles and deterministic per-identity variation without changing the procedure's safety invariants.

### B. Articulation and contact

Use the existing optional planar IK / hierarchy work as a CPU reference where appropriate. The host's Physics owner supplies contact, support, obstruction, and load observations. Gestures may evaluate reachability and kinematic intent, but it must not claim that a hand gripped a coupling, a boot supports body weight, a cover is secured, or a door sealed unless the relevant provider/physics/domain system confirms it.

### C. Crowd and role routing

Connect shared route-to-door staging to the crowd-flow contract and then to an executable fixed-step crowd reference. Include a ramp bottleneck, a narrow service corridor, cross-flow near the hatch, group cohesion, yielding, blocked routes, queueing, and deterministic replay. The current crowd-flow contract is not yet a crowd solver; do not claim those behaviors until implemented and tested.

### D. Failure and recovery

Inject representative failures: absent crew member, blocked ramp, stale hose anchor, failed connector release, cover not accounted for, inaccessible landing-gear inspection point, ground crew not returned, door failing to seal, manifest mismatch, denied clearance, and clearance revoked before departure. Each case must identify the owning authority, the blocked tasks, and the next safe action without fabricating physical outcomes.

### E. Consumer demonstration

Produce a small reproducible CPU demonstration first, with a task timeline, role lanes, resolved anchor names, evaluated poses, observed outcomes, and explicit readiness gates. A later WebPage experience can visualize this same scenario and offer a step-by-step replay, but browser visuals must not be presented as a physics simulation unless they consume actual verified simulation output.

## WebPageLLM handoff

**Ask:** build a discoverable, compact Starport Departure experience in WebPage/Singularity City that consumes the scenario and evidence contract rather than reimplementing it.

- Start with a focused Starport / Departure Operations panel reachable from the city transportation/port experience.
- Show a small schematic: muster, spacecraft door/ramp, service apron, ground-safe zone, readiness/manifest station, and departure-control handoff.
- Display separate flight-crew and ground-crew lanes with shared approach, role branch, independent tasks, ground-crew return, hatch sealing, manifest, ATC, and release gates.
- Make task states visible: waiting, ready, in progress, completed, failed/blocked. A failed prerequisite must visibly block descendants.
- Show the exact external evidence required for clearance and the reason clearance/departure is blocked.
- Use stable task, role, district, building, craft, and anchor IDs from the canonical provider model. Do not hardcode world coordinates in Gestures or create a competing city topology.
- Keep the browser preview explicitly labeled as a choreography/reference visualization until it is connected to authoritative host/Physics evidence.
- Provide accessible text/status in addition to color, preserve the existing WebPage viewport/no-main-panel-scroll constraints, and lazy-load heavier scene content.
- Use a fixture adapter while the consumer contract is evolving; do not add a dependency on an unpublished Gestures or Physics package just to compile WebPage.

**Ownership:** Gestures owns reusable procedure/task semantics and deterministic reference evaluation; the craft/provider owns its anchor catalogue and equipment-specific procedures; FSM_API may own consumer state progression without becoming a Gestures dependency; Physics/world simulation owns authoritative contact, collision, and equipment outcomes; WebPage owns navigation and presentation; Renderer/Visualize own later observer-relative fidelity and GPU presentation.

## Completion evidence

The starport slice is not domain-complete until a deterministic end-to-end run can show both a successful departure and injected failure/recovery cases. Record separate evidence for:

- Scenario graph validation and readiness-gate unit tests.
- Gesture evaluation for actual role actions.
- Anchor resolution and physical outcome observations from the owning provider/Physics system.
- Crowd behavior and bottleneck replay.
- Consumer output parity with CPU reference values.
- CI result for the exact branch head.

Do not claim measured population scale, physics validity, GPU parity, or deployed WebPage integration from the task graph alone.
