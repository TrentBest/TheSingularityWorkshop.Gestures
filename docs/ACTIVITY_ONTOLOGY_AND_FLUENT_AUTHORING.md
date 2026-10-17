# Activity ontology and fluent runtime authoring

> **Status: initial activity-definition builder and optimistic definition-patch contract implemented; session coordination remains planned.** This repository uses *activity* for role/phase choreography inside a Workshop Experience. The Workshop's canonical Experience is an environment composed of MicroBundles, as defined in [WebPage Experience Theory](https://github.com/TrentBest/WebPage/blob/master/EXPERIENCE_THEORY.md).

## The core idea

A character should not need bespoke animation logic for every game or social event. An activity definition describes what can happen, how participants may take part, which reusable procedures are appropriate, what coordination is required, and which observations allow the activity to advance. Runtime systems compose that data with the current world and participants.

A flash mob, a pickup game of tag, hide-and-seek, a practical joke, a starport boarding procedure, and a coordinated film shoot are distinct activity models, not separate Workshop ontologies. They can reuse the same lower-level gesture definitions, role assignments, spatial constraints, timing, lifecycle, and observation contracts without pretending they are the same activity.

**Gesture = reusable physical procedure. Activity definition = an organized, versioned plan of roles and phases that uses procedures. Activity session = one live occurrence of that plan. Workshop Experience = the composed environment of MicroBundles in which the activity may run.** Keep these concepts distinct.

## Ontology layers

| Layer | Owns | Example |
|---|---|---|
| Gesture definition | Reusable motion/procedure data, keyframes, semantic anchors, constraints | Clap, turn, duck, board through a hatch |
| Gesture offer | Provider's procedure plus declared eligibility requirements | This provider offers a dance step to actors with balance capability |
| Activity definition | Versioned classification, phases, roles, action slots, rules, gates, timing, and participation policy | Flash mob; tag; hide-and-seek |
| Session | One runtime instance, stable identity, participant membership, live phase and clock | The flash mob that starts in the shopping district now |
| Assignment | Participant-to-role/action binding and optional group/pair relationship | Dancer, initiator, chaser, hider, seeker, ground crew |
| Runtime patch | Validated change to a live session or future session state | Add late joiners; move a formation; replace a blocked action |
| Observation | Facts supplied by authoritative owners | Participant entered a zone; collision blocked a step; player accepted a cue |
| Presentation | Guidance, avatar pose, observer-specific fidelity and rendering | Show the player's dance cue; replicate the evaluated avatar motion |

Activity classification belongs to the activity model, not a pile of character-specific conditionals. The current `ClassifyAs` values are plain activity-level classification terms; they are **not** the Workshop's nine-layer `OntologySignature` coordinates and do not implement ontology-token resolution. Ontology terms should have stable IDs, explicit versions, and documented relationships. Labels are for people; stable identifiers and declared semantics are for tools and runtime systems.

## Fluent authoring: edit the model, not the character

A fluent API should make composition readable in code and usable by an editor. The same underlying model should be inspectable, validated, serialized, diffed, and modified at runtime. Fluent calls are a convenience over structured data—not a separate source of truth.

Illustrative API only:

~~~csharp
var flashMob = ActivityDefinitionBuilder.Define("public-space.flash-mob")
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

The initial implemented builder currently supports a deliberately smaller slice:

~~~csharp
var definition = ActivityDefinitionBuilder.Define("public-space.flash-mob")
    .Version(1)
    .ClassifyAs("social", "performance")
    .Phase("gather")
        .AssignRole("initiator", min: 1, max: 3)
        .OfferGesture("notice-the-cue")
        .EndPhase()
    .Phase("perform")
        .AssignRole("participant", min: 1, max: 500)
        .OfferGesture("dance.phrase-a")
        .EndPhase()
    .Build();
~~~

The builder validates IDs, positive version, duplicate classifications/phases/roles/references, participant bounds, and nonempty phases. `ActivityDefinitionPatch` is also implemented: it represents a complete replacement definition, requires the same activity ID and a strictly newer version, and checks whether the currently observed definition still matches the expected version. It is an optimistic-concurrency proposal only; it does not mutate a session, apply atomically, or resolve active participant playback.

A good activity-authoring/runtime API should support:
- **Composition:** add/remove phases, role slots, actions, dependencies, timing cues, and spatial constraints.
- **Classification:** attach stable ontology terms and relationships; query activities by capability, setting, participant count, or interaction pattern.
- **Actor-relative filtering:** discover provider offers, then filter against actor capabilities and available semantic anchors.
- **Runtime changes:** submit a patch against a known session/version, validate it, and apply it at a declared safe boundary.
- **Inspection and undo:** show what changed, why it is valid, and what is affected; support undo or replay where the host can preserve state.
- **Determinism:** stable IDs, explicit ordering and timebases, reproducible assignment, and no dependence on dictionary/enumeration order.
- **Editor support:** expose the same graph to visual editing, validation messages, previews, and generated documentation.
- **Serialization:** persist definitions separately from session state and transient observations.

## Runtime patch contract

Runtime modification must not mean mutating arbitrary collections while a coordinator is iterating them. The implemented `ActivityDefinitionPatch` is the first narrow piece of this contract: a replacement definition plus target ID and expected version. A full session patch still needs a revisioned operation set, validation results, and a coordinator-owned apply boundary.

1. Build the proposed change without altering the live session.
2. Validate references, ontology terms, role bounds, action/provider compatibility, dependency cycles, timebase compatibility, and required anchors.
3. Report errors and warnings with paths to the offending node.
4. Apply atomically at a defined session boundary, or reject the patch with no partial mutation.
5. Record the accepted revision and change summary so tools can inspect or replay it.

The Workshop Experience host decides the safe application boundary. A live patch must define what happens to participants already executing an action: finish current phrase, transition at the next cue, cancel, or explicitly reassign. Never silently invalidate an active playback handle.

## Three example activities

### Flash mob in a shopping district

An activity session discovers eligible participants, offers participation, assigns spatial slots and dance phrases, and synchronizes to a shared clock. Each participant receives the procedure and target relevant to their role. A player receives local guidance; the player's evaluated avatar motion enters the normal multiplayer replication path; other observers see the replicated result through their own rendering/fidelity policy.

Joining is not mandatory merely because a participant is nearby. Capacity, participation policy, accessible space, interruption, late joining, and dispersal are explicit activity rules. A scheduled dance is not proof the participant completed it.

### Pickup tag and hide-and-seek

These should be separate activity definitions that reuse shared mechanics: participant roles, playable-area constraints, state transitions, action offers, and event/observation gates. Tag adds chaser/runner roles, valid tagging observations, safe zones if desired, and role transitions. Hide-and-seek adds seeker/hider roles, hiding-phase gates, discovery observations, and round completion rules.

Children can behave playfully, make mistakes, interrupt adults, or attempt pranks, but those are not just animation choices. The activity and character/world policy define intent and social context; Gestures offers reusable physical procedures; Physics and the host provide collisions, obstruction, and authoritative world state; the social simulation decides reactions and consequences. An adult being nearly knocked over is a physical observation, not a successful animation flag.

### Starport crew choreography

Ground crew and flight crew are role families within a task-oriented activity. The activity definition organizes procedures for doors, hoses, covers, landing gear, mass-driver covers, and boarding, while dependency gates express what must be ready before the next phase. Physics remains authoritative for contact and obstruction; the task coordinator requires evidence for critical transitions.

## Ownership boundaries

- **Gestures:** reusable physical procedure data, deterministic evaluation, offer eligibility, and motion-lifecycle primitives.
- **Activity coordinator:** phase graph, participant assignment, shared clocks, dependencies, interruption and re-entry, and session revisions.
- **Actor/gameplay system:** intent, permissions, personality, social policy, and authoritative game state.
- **Physics:** forces, contact, collision, reachability evidence, and achieved physical outcomes.
- **Renderer / Visualize:** observer-relative fidelity and presentation. They do not define the activity's ontology.
- **Editor/tooling:** authoring, graph inspection, validation, preview, classification, patch review, and documentation generation. It edits data; it should not become the only place the rules exist.

The activity coordinator should be optional. A one-person Gesture must remain inexpensive and useful without a crowd, social system, or activity graph.

## Editor and living-documentation proof

The first editor need not be a full visual node editor. A useful initial tool can list ontology categories and definitions, display the phase/role/dependency graph, validate references, preview provider offers for a selected actor context, and show a before/after runtime patch. Export the same graph to SVG so the picture is generated from the actual model and reports real validation status.

The proof sequence should be:
1. A serialized activity definition with stable IDs and version.
2. A validator with tests for dangling references, invalid role bounds, dependency cycles, and incompatible timebases.
3. A fluent builder that creates that same model and round-trips through serialization.
4. A session patch engine with atomic apply/reject tests and a defined boundary for active participants. The current definition patch is only a validated proposal with an optimistic version check.
5. A small executable tag or dance session whose visual output is generated from real assignment and schedule records.
6. An editor that reads and writes the same ontology model, rather than maintaining a separate visual-only representation.

Do not draw a populated shopping district and label it implemented before the session coordinator, participant assignment, runtime patches, and evidence path exist. Native SVG is not GUI.SVG usage; the technology label must reflect the actual generator.

## Suggested implementation order

1. **Implemented foundation:** immutable `ActivityDefinition`, phase and role-slot records, classification terms, and a fluent builder with structural validation.
2. Add serialization and round-trip tests for the same immutable model.
3. Add deterministic validation for cross-definition Gesture references, dependency cycles, and path-specific diagnostics.
4. Extend the implemented optimistic definition replacement proposal into a versioned session patch model and atomic application against a session revision.
5. Add deterministic role assignment and participant eligibility as independent policies.
6. Add a minimal prerequisite-aware session coordinator with waiting, blocked, interrupted, and completed states.
7. Build a small tag/hide-and-seek or flash-mob fixture and generate living documentation from its real output.
8. Build editor surfaces on top of the same model.

The first executable ontology slice now exists: ActivityDefinitionBuilder creates immutable, versioned definitions with classifications, ordered phases, role bounds, and Gesture references, with tests for structural invariants. It does not yet serialize the model, resolve Gesture references against providers, coordinate a live session, apply session patches atomically, assign participants, or implement the full fluent sketch above. `ActivityDefinitionPatch` validates replacement identity/version and detects stale expected versions, but deliberately does not apply changes. The GestureOfferSelector is the executable piece of actor-relative eligibility; fixed sequence playback and starport task dependencies remain narrower foundations, not a general activity coordinator.
