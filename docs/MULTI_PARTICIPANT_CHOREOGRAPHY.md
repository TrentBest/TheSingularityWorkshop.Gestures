# The Gestures ecosystem: motion, coordination, and population execution

> **Status: architecture contract and implementation plan.** This document places the existing crowd-response scheduler in the larger Gestures ecosystem. It does not claim that party simulation, partner dancing, group formation control, a general crowd solver, or optimized population-scale execution is implemented.

## The system is larger than any one application

A crowd is both a **consumer of Gestures capabilities** and a **workload that drives Gestures ecosystem design**. Treating it only as an application misses the shared machinery needed to make large populations affordable; treating Gestures as only a crowd engine would make every other user inherit crowd-specific assumptions.

Think of a rollercoaster system: the vehicle, track, tunnel, supports, scenery, control system, and operating procedures are different parts of one experience. A crowd is one use of the motion ecosystem, just as a party, dance, starport crew, film set, or individual character is another. Population-scale work can expose common needs—batching, shared inputs, stable identity, scheduling, update-rate tiers, and compact output—that should become reusable capabilities where evidence shows they generalize.

The goal is therefore not merely to add a party or dance example. It is to identify and build the reusable system beneath them, then demonstrate that system through real applications.

A crowd response answers: **when should each participant begin responding to a shared event?** Efficiently evaluating a large population also asks **which work can be shared or batched, at what cadence, and how can each participant retain stable independent results?**

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

## Separate capabilities that cooperate

The architecture should be understood as cooperating layers, not one overloaded `Crowd` feature:

1. **Motion definitions and evaluation:** reusable procedures, keyframes/anchors, deterministic evaluation, and bounded stable variation.
2. **Coordination and scheduling:** shared clocks, events, roles, relationships, dependencies, interruption, and re-entry.
3. **Population execution:** batching compatible work, reusing shared inputs, separating per-entity data from cohort data, scheduling update cadence, and producing stable per-identity outputs. This is an ecosystem capability to investigate and measure—not a claim that a population batch engine already exists.
4. **Domain applications:** crowd response/flow, party, dance, starport crew, film production, and other consumers that compose the capabilities they need.
5. **Presentation and physical realization:** consumers resolve world anchors, physical outcomes, observer-relative fidelity, GPU presentation, and sound according to their own contracts.

Batching is an execution optimization, not a behavioral policy. A batch must not make participants interchangeable, change outcomes when input order changes, or force unrelated activities onto the same clock. Conversely, the domain application should not need to hand-roll a separate evaluator for every participant if the work can safely share a common path.

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

## Places, attention, and continuity across the world

The same ecosystem must work at several nested scales. A building is not merely a static container in which an unrelated crowd happens to run. It can host its own local activity, props, sound, attention sources, entrances/exits, and participant roles while remaining connected to the surrounding street and wider world. The world is composed from these places and transitions; the crowd is one population moving through them.

### A place is a local activity environment

A bar might contain a piano or music source, a counter, tables, a door, and several kinds of guest behavior. Some guests are playful and sociable; others may become angry quickly. These are not all “Gestures” themselves: the place/provider supplies the semantic affordances, guest profiles, activity rules, and environmental events. Gestures supplies reusable physical procedures for actions such as playing, sitting, turning toward a sound, greeting, arguing, standing, walking to a door, opening it, and carrying an object. A coordinator chooses and sequences actions; the host and Physics determine whether they can actually happen.

A building can be loaded, simulated, represented, and retired at a different fidelity from the world outside it. A player interacting with an exit may request a transition to an outside scene. That transition is not complete merely because the interaction fired: it may have an approach/arrival gate, a doorway crossing, and a handoff of the player's position to the sidewalk. The presentation can then scale the building back and reveal the exterior, but this visual transition must not change the authoritative position or identity of the player. The sidewalk connects to other places and is a natural boundary for grouping nearby agents into shared processing cohorts.

This yields a useful composition:

- **Place** — a building, shop, bar, park, airport terminal, street segment, or room, with identity and connections.
- **Affordance** — an available interaction or opportunity: seat, piano, exit, shop counter, puppy window, moving sidewalk, gate, or service desk.
- **Activity** — a specific local process, such as playing music, ordering food, entering, browsing, buying, waiting, or boarding.
- **Agent intent and state** — what an individual wants, knows, carries, can afford, and has actually completed.
- **Transition** — a validated handoff between places, preserving identity and relevant state.
- **Execution cohort** — a temporary set of compatible work items that can share scheduling, input data, or evaluator dispatch without becoming one behavior.

These are conceptual responsibilities, not frozen public API names.

### Attention is an opportunity, not a command

Attention points are authored sources or events that can attract notice: a piano performance, a sudden noise, a street performer, a sale, or puppies playing in a pet-store window. An attention-grabbing score can combine source salience, distance, visibility, novelty, current agent interests, competing stimuli, and the agent's present activity. It should be a score or ranked opportunity—not a universal rule that forces every observer to look.

A practical decision chain is:

1. The environment publishes a discoverable stimulus with position, extent, type, and a versioned salience profile.
2. An agent perceives or receives candidate stimuli according to its own location, senses, attention budget, and current commitments.
3. The agent's decision policy weighs those candidates against goals, time, resources, relationships, and willingness to interrupt.
4. If the agent chooses to respond, a local activity coordinator requests the relevant Gesture or activity and schedules its start.
5. The host reports observations: whether the agent could reach the window, entered the shop, completed a purchase, or continued walking.
6. Only authoritative completed outcomes update persistent state and downstream decisions.

This distinction makes the world legible. A high attention score can increase the chance that a passerby pauses, approaches the window, or enters the shop; it should not teleport the agent or guarantee a purchase. Different agents can ignore the same stimulus, notice it late, approach but be blocked, browse without buying, or choose to buy for different reasons. Deterministic seeded variation is useful for reproducible tests, while any genuinely stochastic policy must define its random stream and replay guarantees explicitly.

### A Gesture can become a spectacle

A Gesture does not stop mattering when the performer finishes evaluating its motion. A single person, a pair, or a coordinated group can become a **spectacle**: something other agents notice, interpret, and react to. A street performance may attract curiosity and imitation; a threatening display may provoke anger or make passersby avoid the area; a strange or confusing action may be ignored by some observers and draw others closer. These are different responses to one observed event, not different animation clips baked into the performers.

Keep the causal chain explicit:

1. **Performance intent:** an actor or activity requests a reusable Gesture or coordinated sequence.
2. **Evaluated procedure:** Gestures evaluates motion for the participating actors.
3. **Observed event:** the host publishes what is observable, with stable event identity and relevant context such as participants, location/extent, timing, and semantic event kind. It reports an event; it does not declare how every observer must interpret it.
4. **Observer interpretation:** each observer's perception, attention, goals, relationships, knowledge, and current commitments determine whether and how the event matters to them.
5. **Chosen reaction:** an observer may watch, approach, join, imitate, become curious, become angry, feel threatened, avoid the area, or continue without responding. If it chooses to act, its own activity policy requests appropriate Gestures.
6. **World outcome:** the host and Physics report actual movement, obstruction, contact, or other achieved state. A reaction request does not prove that the observer reached the spectacle or completed the response.

This makes a group Gesture more than the sum of its individual poses: it can have a shared event identity and a collective spatial/temporal footprint, while each observer remains an independent decision-maker. A spectator is not automatically a participant, and proximity is not consent to join. One observer may be captivated while another walks away.

The core now provides `GestureObservationEvent`, a small immutable record for this boundary. It carries a stable event ID, a consumer-defined semantic kind, the host-supplied occurrence time, stable participant IDs, and an optional causal parent event ID. The participant list is copied and validated so the event cannot be changed through the caller's original collection. It deliberately contains no world-space transform, observer interpretation, emotion, reaction schedule, or physical-success flag; those facts belong to the systems that can authoritatively supply them. This is an event data contract, not yet an event bus, perception engine, or social simulation. `CrowdResponseRequest` can now be constructed directly from this event and an observer ID, so the scheduler uses the event's stable identity and occurrence time without requiring the observer to be one of its performers. The constructor is only a bridge into deterministic intent timing: the host still decides whether the observer noticed the event and which response, if any, to schedule.

The reusable core should expose enough stable semantic information for a consumer to publish and query observed events, but it should not own universal emotion, personality, moral judgement, or social policy. Gesture providers describe physical procedures and their semantics; activity/scene systems decide what is happening; observer/agent systems interpret it; Physics and the host determine physical truth; Renderer presents the result. A performer does not need to know every possible audience reaction, and an observer does not need a bespoke branch in the performer's animation.

**Avoid reaction loops becoming accidental event storms.** An observer's reaction may itself become a new spectacle, but it should publish a distinct event with provenance and causal linkage to the event that prompted it. Policies can limit duplicate perception, repeated self-triggering, and reaction depth without suppressing legitimate chains such as one person stopping, a friend noticing, and both approaching. These limits belong to the observing activity/host policy, not to hidden global state in the Gesture evaluator.

### Persistent consequences make the world feel alive

The puppy example is a vertical slice across multiple systems, not a single animation:

- A Digiten notices puppies in a pet-store window and chooses to investigate.
- The agent enters the store, approaches an available animal, and completes a purchase through the store/economy rules.
- The purchase creates persistent ownership or care responsibility; it is not merely a prop attached for presentation.
- The Digiten exits to the sidewalk with the puppy, retaining identity and ownership across the place transition.
- Later shopping decisions account for dog food and other relevant needs.
- A visit to a dog park becomes a new activity opportunity, creating interactions with other owners and animals.
- The observing player can see the Digiten walking away with the puppy, then later recognize the same agent and its changed circumstances.

Gestures provides the physical procedures for relevant actions, not the entire economic or citizen simulation. The world/agent systems own needs, goals, affordability, inventory, ownership, memory, and decision policy. The store owns stock and purchase rules. Place/scene coordination owns entry, exit, and handoff. Physics owns reachability and physical outcomes. Renderer/Visualize presents the current view. Persistent state links these episodes so that an action in one place can alter behavior somewhere else later.

The important acceptance test is not “did the purchase animation play?” It is “did the same stable agent leave with the purchased animal, retain that ownership after a transition, and subsequently make consistent care-related choices?”

### One world, many execution cadences

A bar, pet store, airport, and dog park share the same underlying population concerns even though their activity policies differ. A nearby agent crossing a doorway may need frequent updates; a distant pedestrian may need only coarse route progress; an off-screen citizen may still need occasional decision/state updates without full pose evaluation. A moving sidewalk in an airport is a clear example: the local traversal policy differs from an ordinary walking area, but both are connected segments in the same route graph. The transport mode or area changes the movement request; it should not require a separate identity or world simulation.

Use independent cadence tiers for distinct kinds of work:

- **Decision cadence:** reconsider goals and stimuli when relevant events or thresholds occur, rather than rerunning every decision every frame.
- **Activity cadence:** advance local schedules, music/beat clocks, shop queues, and task gates at the cadence each domain requires.
- **Motion cadence:** evaluate poses or movement controls only as frequently as the required fidelity and physical stability demand.
- **Presentation cadence:** interpolate or select representations for the observer without making rendering frequency authoritative for agent decisions.
- **Persistence cadence:** commit durable changes at explicit transaction/outcome boundaries, not because a render frame happened.

Cohorts should be formed by compatible work and locality, not treated as permanent social groups. Members can share a street segment, route calculation, stimulus index, activity clock, or evaluator dispatch while retaining separate goals, possessions, personality/context parameters, motion seeds, and results. An agent moving from sidewalk to shop changes place and possibly cohort; it does not become a different agent. Cohorting must also tolerate entry, exit, interruption, and changing fidelity without order-dependent outcomes.

### A vertical slice that proves the ecosystem

A high-value demonstrator should show the complete causal chain rather than a crowd of decorative figures:

1. A small street connects a bar, a pet store, and a sidewalk/park route.
2. The bar has a music source and several distinct guest profiles; the music creates a measurable attention stimulus.
3. A puppy-window stimulus attracts a subset of passing agents according to per-agent policy, not a forced global reaction.
4. One agent enters the pet store, completes a valid purchase, and exits with the puppy through a gated place transition.
5. The same identity is visible later carrying the puppy, buys dog food on a subsequent shopping trip, and visits a dog park.
6. Distant and nearby agents use different measured update cadences; shared work is batched only where parity tests prove that per-agent decisions and outcomes are preserved.
7. A living SVG/timeline is generated from actual world, schedule, transaction, and transition records. Conceptual overlays are labelled; no artwork is presented as evidence of unimplemented simulation.

Keep the first proof deliberately small but end-to-end. A real causal chain with three places and one persistent agent teaches more about the architecture than a thousand anonymous agents moving in a loop. Once the chain is observable and replayable, scale the population and measure where execution cost actually resides.

## Relationship to the existing crowd scheduler

Keep `CrowdResponseScheduler` small and pure. It already answers a useful question: given one event, a policy, and one stable participant identity, what bounded intent time and variation parameters should that participant receive?

That implementation is a first reusable brick, not the end state for crowd support. The crowd workload should help drive shared population-execution capabilities: batch evaluation where inputs and semantics are compatible, shared event/policy data, stable per-ID variation, cadence tiers, and measurable CPU/allocation costs. Those capabilities should serve other workloads too, rather than being buried in a crowd-only application.

A future activity coordinator can call or compose the scheduler for independent participant responses. It must not silently retrofit pair relationships, music clocks, formations, or task dependencies into the crowd policy. Those are separate semantics and should have their own contracts and tests. Likewise, do not assume that batching means all agents share the same behavior or update rate.

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
- **Population execution (future capability):** measured batching, shared-input reuse, per-entity output preservation, cadence/update tiers, and population profiling. Keep it separate from the semantics of any one application.
- **Gestures:** reusable procedure definitions, semantic anchors, CPU reference evaluation, bounded stable motion variation, evaluated pose.
- **Domain provider:** party rules, dance vocabulary/choreography, starport task semantics, participant capability/experience and anchor data.
- **FSM/API or host:** optional lifecycle orchestration and authoritative session integration; not a dependency required by Gestures.
- **Physics:** movement feasibility, contact, collisions, forces, blocked paths, achieved physical outcomes and any physical acoustic model.
- **Renderer:** observer-relative representation/fidelity with phase continuity.
- **Visualize:** GPU-facing presentation.
- **Audio/scene presentation:** authored music/dialogue and mixing/propagation of sound events.

Do not put a universal social simulation, a complete choreography language, pathfinding, a physics solver, audio synthesis, or renderer-specific representations into the Gestures core.

## Implementation sequence

1. [ ] Establish representative workloads and a baseline: one procedure, a paired activity, a party-sized population, and staged large populations. Record elapsed CPU time, allocations, per-entity bytes, cadence, and output count; do not claim scale without measurements.
2. [ ] Specify the smallest versioned activity/session/participant/role/clock/output contract from concrete fixtures.
3. [ ] Add a two-person dance fixture with explicit beat times, pair identity, three phrases, interruption, and deterministic replay.
4. [ ] Add tests for input-order independence, stable resampling, missed cues, participant removal, pause/resume, and re-entry.
5. [ ] Add a small party fixture with overlapping activity groups and explicit waiting/blocked transitions.
6. [ ] Identify common evaluator work and design a batch path only where measurements and semantic equivalence justify it; compare scalar and batched outputs for parity and deterministic per-ID results.
7. [ ] Reuse the same activity vocabulary for a team task only where its dependency/evidence semantics match; do not force physical tasks into social activity rules.
8. [ ] Generate living SVGs from actual schedule and evaluator outputs, and label conceptual art separately.
9. [ ] Integrate in a consumer and profile staged population sizes before making optimization or scalability claims.

## Acceptance criteria before calling it general

- A participant can belong to a session, pair, or group without identity being derived from collection order.
- Two participants can coordinate while retaining independent motion and bounded variation.
- Shared timing is explicit and stable across render rates.
- The model distinguishes simultaneous, sequential, and responsive actions.
- Interruption, unavailability, and failed physical outcomes do not silently advance the plan.
- Replay from the same versioned inputs reproduces the same schedule; numerical pose tolerances are documented separately.
- Crowd response, social activity, partner choreography, and physical team tasks remain distinct policies over shared primitives, not one overloaded mode flag.
- Any batch path preserves per-identity semantics and matches the reference evaluator within documented tolerances; results do not depend on collection order.
- Optimization claims report workload, population size, hardware/runtime context, elapsed time, allocations/bytes, and correctness parity.
- Demonstrations and SVGs state whether they show specified, implemented, tested, measured, or merely conceptual behavior.

The goal is **coordinated individuality**: people participate in the same activity without becoming synchronized clones, and the system can explain both what it intended and what actually happened.
