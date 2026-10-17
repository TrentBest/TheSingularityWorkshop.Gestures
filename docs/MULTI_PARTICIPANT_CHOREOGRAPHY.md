# Multi-participant choreography: from crowds to coordinated activities

> **Status: architecture contract and implementation plan.** This document generalizes the existing per-participant crowd-response scheduler. It does not claim that party simulation, partner dancing, group formation control, or a general crowd solver is implemented.

## The missing distinction

A crowd response answers: **when should each participant begin responding to a shared event?**

A coordinated activity answers a larger set of questions:

- Which participants are taking part, and which are merely observing?
- What activity or shared event are they participating in?
- What role does each participant hold?
- Which participants are paired, grouped, leading, following, serving, or waiting?
- Which actions must happen together, in sequence, or only after a prerequisite?
- Which shared clock or external beat governs timing?
- What spatial relationships should be maintained?
- What happens when a participant is late, unavailable, interrupted, blocked, or leaves?
- What authoritative observations prove that an action succeeded?

The scheduler is a useful primitive inside this system, not the whole system.

## A reusable coordination model

A future coordinator should describe an activity using versioned, data-driven records. These are conceptual names, not public API commitments.

| Concept | Responsibility |
|---|---|
| Activity definition | Identifies the kind of shared activity and its versioned rules |
| Session instance | One occurrence of the activity, with stable identity and a declared timebase |
| Participant assignment | Binds stable participant IDs to roles and optional group/pair membership |
| Action/phrase | A reusable action request, such as greet, turn, clap, step, offer, receive, or reset |
| Dependency/gate | Declares prerequisites and conditions for advancing |
| Synchronization cue | Connects an action to an event, shared clock, beat, count, or another participant's phase |
| Relationship/formation | Declares desired relative position, orientation, spacing, or partner relation |
| Variation policy | Defines bounded, stable individual variation without destroying the shared pattern |
| Observation/result | Records what the consumer or simulation actually observed; distinguishes intent from achieved outcome |

Use stable IDs, explicit versions, units, and validation. Input enumeration order must not change participant assignment or scheduling. Re-evaluating a session must not reroll choices. Wall-clock time and render-frame count are not authoritative activity clocks.

Do not force every activity to use every concept. A simple crowd reaction may need only an event, participants, and bounded delays. A partner dance needs pair assignments and shared musical timing. A team procedure needs role dependencies and evidence gates.

## Three representative activities

### 1. Party: social activity, not a single crowd animation

A party is a changing set of overlapping activities, not one synchronized reaction. A participant might arrive, find a host, greet a friend, join a conversation, move toward refreshments, watch a dancer, join a dance, or leave.

The session needs participant intent and activity transitions, with policies for invitations, social relationships, group size, available space, and interruption. It should not prescribe a complete social life or invent a participant's authoritative personality. Domain/experience systems supply those inputs; the coordinator schedules compatible actions and reports blocked or unassigned activity honestly.

A first demonstrator could contain a host, two arriving guests, a conversation pair, and an optional dance invitation. Each participant keeps a stable identity while changing activities. The SVG should be generated from the actual assignment/schedule output and show roles, activity transitions, and waiting/blocked states—not merely draw people around a room.

### 2. Dancing: shared timing plus partner relationships

A dance is not just several gestures started at similar times. It has an external or session-owned beat clock, phrases/counts, participant or pair assignments, and a choreography that may be synchronized, canonically offset, or deliberately responsive.

Represent at least:
- Music/beat timebase and its origin; never derive musical phase from render FPS.
- Versioned choreography phrases and action cues.
- Stable dancer IDs, pair/group membership, and lead/follow or other role semantics where applicable.
- Relative spatial goals and orientation where the choreography requires them.
- Bounded, deterministic expression variation that preserves the phrase and partner coordination.
- Interruption, missed cue, partner unavailable, and safe re-entry behavior.

Gestures evaluates each requested motion procedure. A coordinator aligns those procedures to the phrase and participant relationships. Physics/host observations determine whether a step was blocked, contact occurred, balance was lost, or a partner could not be reached. A scheduled step is not proof that the dancer physically completed it.

The first dance proof should be small: two participants, a short beat grid, a three-phrase sequence, a clear partner relationship, and a replayable result. Add a group dance only after pair timing and interruption behavior are testable.

### 3. Team task: roles, dependencies, and evidence

A team carrying equipment, boarding a vehicle, setting a stage, or handling a shared object needs role assignment and a dependency graph. One participant may stabilize an object while another releases a latch; the next action must wait for observed release rather than a timer alone.

The coordinator owns the plan and readiness gates, not the physics. Gestures supplies reusable individual procedures; the provider supplies semantic anchors and task-specific rules; the host/FSM may manage lifecycle; Physics supplies contact, obstruction, force, and achieved-state observations. A blocked action remains blocked until an explicit policy or observation permits recovery.

The existing starport departure task graph is a nearby reference for this distinction: task readiness and clearance gates are not the same as multi-agent pathfinding or physical contact resolution.

## Relationship to the existing crowd scheduler

Keep `CrowdResponseScheduler` small and pure. It already answers a useful question: given one event, a policy, and one stable participant identity, what bounded intent time and variation parameters should that participant receive?

A future activity coordinator can call or compose this primitive for independent participant responses. It must not silently retrofit pair relationships, music clocks, formations, or task dependencies into the crowd policy. Those are separate semantics and should have their own contracts and tests.

Generalization should preserve the following distinctions:

1. **Intent:** what the activity asks a participant to do.
2. **Scheduling:** when the requested action or phrase is eligible to start.
3. **Evaluation:** the pose/procedure output from Gestures at a given time.
4. **Achieved state:** what the host/Physics actually observed.
5. **Presentation:** what Renderer/Visualize choose to show at the current observer-relative fidelity.
6. **Audio:** authored music/dialogue and physical sound events have distinct ownership and timing.

## Ownership boundaries

- **Activity/scene coordinator:** session lifecycle, participant assignment, role/pair/group relations, shared clocks, action dependencies, retries and explicit cancellation/re-entry policy.
- **CrowdResponseScheduler:** deterministic per-identity timing/variation for a single shared response event.
- **Gestures:** reusable procedure definitions, semantic anchors, CPU reference evaluation, bounded stable motion variation, evaluated pose.
- **Domain provider:** party rules, dance vocabulary/choreography, starport task semantics, participant capability/experience and anchor data.
- **FSM/API or host:** optional lifecycle orchestration and authoritative session integration; not a dependency required by Gestures.
- **Physics:** movement feasibility, contact, collisions, forces, blocked paths, achieved physical outcomes and any physical acoustic model.
- **Renderer:** observer-relative representation/fidelity with phase continuity.
- **Visualize:** GPU-facing presentation.
- **Audio/scene presentation:** authored music/dialogue and mixing/propagation of sound events.

Do not put a universal social simulation, a complete choreography language, pathfinding, a physics solver, audio synthesis, or renderer-specific representations into the Gestures core.

## Implementation sequence

1. [ ] Specify a minimal versioned activity/session/participant/role/clock/output contract; avoid implementing a generic framework before two concrete fixtures expose the common core.
2. [ ] Add a two-person dance fixture with explicit beat times, pair identity, three phrases, interruption, and deterministic replay.
3. [ ] Add tests for input-order independence, stable resampling, missed cues, participant removal, pause/resume, and re-entry.
4. [ ] Add a small party fixture with overlapping activity groups and explicit waiting/blocked transitions.
5. [ ] Reuse the same activity vocabulary for a team task only where its dependency/evidence semantics match; do not force physical tasks into social activity rules.
6. [ ] Generate living SVGs from actual schedule and evaluator outputs, and label conceptual art separately.
7. [ ] Integrate in a consumer and profile only after the semantics and replay tests are accepted.

## Acceptance criteria before calling it general

- A participant can belong to a session, pair, or group without identity being derived from collection order.
- Two participants can coordinate while retaining independent motion and bounded variation.
- Shared timing is explicit and stable across render rates.
- The model distinguishes simultaneous, sequential, and responsive actions.
- Interruption, unavailability, and failed physical outcomes do not silently advance the plan.
- Replay from the same versioned inputs reproduces the same schedule; numerical pose tolerances are documented separately.
- Crowd response, social activity, partner choreography, and physical team tasks remain distinct policies over shared primitives, not one overloaded mode flag.
- Demonstrations and SVGs state whether they show specified, implemented, tested, measured, or merely conceptual behavior.

The goal is **coordinated individuality**: people participate in the same activity without becoming synchronized clones, and the system can explain both what it intended and what actually happened.
