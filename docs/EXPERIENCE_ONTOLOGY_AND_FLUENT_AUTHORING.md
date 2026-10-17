# Experience ontology and fluent runtime authoring

> **Status: architecture contract and implementation plan.** This document records the intended reusable model. The fluent API below is illustrative, not a claim that these types already exist.

## The core idea

A character should not need bespoke animation logic for every game or social event. An experience describes what can happen, how participants may take part, which reusable procedures are appropriate, what coordination is required, and which observations allow the experience to advance. Runtime systems compose that data with the current world and participants.

A flash mob, a pickup game of tag, hide-and-seek, a practical joke, a starport boarding procedure, and a coordinated film shoot are different experience ontologies. They can reuse the same lower-level gesture definitions, role assignments, spatial constraints, timing, lifecycle, and observation contracts without pretending they are the same activity.

**Gesture = reusable physical procedure. Experience = organized, versioned ontology of activities that use procedures. Session = one live occurrence of an experience.** Keep those concepts distinct.

## Ontology layers

| Layer | Owns | Example |
|---|---|---|
| Gesture definition | Reusable motion/procedure data, keyframes, semantic anchors, constraints | Clap, turn, duck, board through a hatch |
| Gesture offer | Provider's procedure plus declared eligibility requirements | This provider offers a dance step to actors with balance capability |
| Experience definition | Versioned classification, phases, roles, action slots, rules, gates, timing, and participation policy | Flash mob; tag; hide-and-seek |
| Session | One runtime instance, stable identity, participant membership, live phase and clock | The flash mob that starts in the shopping district now |
| Assignment | Participant-to-role/action binding and optional group/pair relationship | Dancer, initiator, chaser, hider, seeker, ground crew |
| Runtime patch | Validated change to a live session or future session state | Add late joiners; move a formation; replace a blocked action |
| Observation | Facts supplied by authoritative owners | Participant entered a zone; collision blocked a step; player accepted a cue |
| Presentation | Guidance, avatar pose, observer-specific fidelity and rendering | Show the player's dance cue; replicate the evaluated avatar motion |

Classification belongs to the experience ontology, not a pile of character-specific conditionals. Ontology terms should have stable IDs, explicit versions, and documented relationships. Labels are for people; stable identifiers and declared semantics are for tools and runtime systems.

## Fluent authoring: edit the model, not the character

A fluent API should make composition readable in code and usable by an editor. The same underlying model should be inspectable, validated, serialized, diffed, and modified at runtime. Fluent calls are a convenience over structured data—not a separate source of truth.

Illustrative API only:

~~~csharp
var flashMob = Experience.Define("public-space.flash-mob")
    .Version(1)
    .ClassifyAs("social", "performance", "group-activity")
    .Phase("gather")
        .AssignRole("initiator", min: 1, max: 3)
        .AssignRole("participant", min: 1, max: 500)
        .OfferGesture("notice-the-cue")
    .Phase("perform")
        .SynchronizeTo("music.beat")
        .OfferGesture("dance.phrase-a")
        .AllowLateJoiners()
    .Phase("disperse")
        .OfferGesture("return-to-local-activity")
    .Build();

session.Apply(patch =>
    patch.AddParticipants(newParticipants)
         .AssignAvailableRoles()
         .SetFormation(newFormation));
~~~

This sketch deliberately omits concrete type names, overloads, and implementation guarantees. The design target is a validated ontology graph with clear diagnostics, not a chain of methods that hides invalid state.

A good runtime API should support:
- **Composition:** add/remove phases, role slots, actions, dependencies, timing cues, and spatial constraints.
- **Classification:** attach stable ontology terms and relationships; query experiences by capability, setting, participant count, or interaction pattern.
- **Actor-relative filtering:** discover provider offers, then filter against actor capabilities and available semantic anchors.
- **Runtime changes:** submit a patch against a known session/version, validate it, and apply it at a declared safe boundary.
- **Inspection and undo:** show what changed, why it is valid, and what is affected; support undo or replay where the host can preserve state.
- **Determinism:** stable IDs, explicit ordering and timebases, reproducible assignment, and no dependence on dictionary/enumeration order.
- **Editor support:** expose the same graph to visual editing, validation messages, previews, and generated documentation.
- **Serialization:** persist definitions separately from session state and transient observations.

## Runtime patch contract

Runtime modification must not mean mutating arbitrary collections while a coordinator is iterating them. A patch should be a declarative change set with a target definition/session ID, expected version or revision, operations, and validation results.

1. Build the proposed change without altering the live session.
2. Validate references, ontology terms, role bounds, action/provider compatibility, dependency cycles, timebase compatibility, and required anchors.
3. Report errors and warnings with paths to the offending node.
4. Apply atomically at a defined session boundary, or reject the patch with no partial mutation.
5. Record the accepted revision and change summary so tools can inspect or replay it.

The host decides the safe application boundary. A live patch must define what happens to participants already executing an action: finish current phrase, transition at the next cue, cancel, or explicitly reassign. Never silently invalidate an active playback handle.

## Three example experiences

### Flash mob in a shopping district

An experience instance discovers eligible participants, offers participation, assigns spatial slots and dance phrases, and synchronizes to a shared clock. Each participant receives the procedure and target relevant to their role. A player receives local guidance; the player's evaluated avatar motion enters the normal multiplayer replication path; other observers see the replicated result through their own rendering/fidelity policy.

Joining is not mandatory merely because a participant is nearby. Capacity, participation policy, accessible space, interruption, late joining, and dispersal are explicit experience rules. A scheduled dance is not proof the participant completed it.

### Pickup tag and hide-and-seek

These should be separate experience definitions that reuse shared mechanics: participant roles, playable-area constraints, state transitions, action offers, and event/observation gates. Tag adds chaser/runner roles, valid tagging observations, safe zones if desired, and role transitions. Hide-and-seek adds seeker/hider roles, hiding-phase gates, discovery observations, and round completion rules.

Children can behave playfully, make mistakes, interrupt adults, or attempt pranks, but those are not just animation choices. The experience and character/world policy define intent and social context; Gestures offers reusable physical procedures; Physics and the host provide collisions, obstruction, and authoritative world state; the social simulation decides reactions and consequences. An adult being nearly knocked over is a physical observation, not a successful animation flag.

### Starport crew choreography

Ground crew and flight crew are role families within a task-oriented experience. The ontology organizes procedures for doors, hoses, covers, landing gear, mass-driver covers, and boarding, while dependency gates express what must be ready before the next phase. Physics remains authoritative for contact and obstruction; the task coordinator requires evidence for critical transitions.

## Ownership boundaries

- **Gestures:** reusable physical procedure data, deterministic evaluation, offer eligibility, and motion-lifecycle primitives.
- **Experience/activity coordinator:** phase graph, participant assignment, shared clocks, dependencies, interruption and re-entry, and session revisions.
- **Actor/gameplay system:** intent, permissions, personality, social policy, and authoritative game state.
- **Physics:** forces, contact, collision, reachability evidence, and achieved physical outcomes.
- **Renderer / Visualize:** observer-relative fidelity and presentation. They do not define the experience's ontology.
- **Editor/tooling:** authoring, graph inspection, validation, preview, classification, patch review, and documentation generation. It edits data; it should not become the only place the rules exist.

The coordinator should be optional. A one-person Gesture must remain inexpensive and useful without a crowd, social system, or experience graph.

## Editor and living-documentation proof

The first editor need not be a full visual node editor. A useful initial tool can list ontology categories and definitions, display the phase/role/dependency graph, validate references, preview provider offers for a selected actor context, and show a before/after runtime patch. Export the same graph to SVG so the picture is generated from the actual model and reports real validation status.

The proof sequence should be:
1. A serialized experience definition with stable IDs and version.
2. A validator with tests for dangling references, invalid role bounds, dependency cycles, and incompatible timebases.
3. A fluent builder that creates that same model and round-trips through serialization.
4. A patch engine with atomic apply/reject tests and a defined boundary for active participants.
5. A small executable tag or dance session whose visual output is generated from real assignment and schedule records.
6. An editor that reads and writes the same ontology model, rather than maintaining a separate visual-only representation.

Do not draw a populated shopping district and label it implemented before the session coordinator, participant assignment, runtime patches, and evidence path exist. Native SVG is not GUI.SVG usage; the technology label must reflect the actual generator.

## Suggested implementation order

1. Formalize immutable ExperienceDefinition, phase, role-slot, action-reference, ontology-term, and version records.
2. Add a fluent builder that produces the immutable model and gives path-specific validation diagnostics.
3. Add deterministic validation for graph references, role bounds, and dependency cycles.
4. Add a versioned runtime patch model and atomic patch application against a session revision.
5. Add deterministic role assignment and participant eligibility as independent policies.
6. Add a minimal prerequisite-aware session coordinator with waiting, blocked, interrupted, and completed states.
7. Build a small tag/hide-and-seek or flash-mob fixture and generate living documentation from its real output.
8. Build editor surfaces on top of the same model.

This is a proposed scope and order, not a statement that these items are already implemented. The current GestureOfferSelector is the first executable piece of actor-relative eligibility; the fixed sequence playback and starport task dependency primitives remain narrower foundations, not a general experience coordinator.
