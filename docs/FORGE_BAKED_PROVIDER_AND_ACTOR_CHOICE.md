# Forge-Baked Gesture Providers and Actor Choice

> **Status: proposed architecture and tooling contract.** This document derives a candidate model from the existing engine-neutral Gesture evaluator and environment-defined vocabulary. It does not claim that provider discovery, Forge baking, actor decision-making, nested procedure execution, or a world simulation is implemented. The names below are design vocabulary, not frozen public API.

## The central separation

**The Forge defines what can happen. Providers describe how it can happen. Actors decide what they want to do. The domain/Physics layer decides what actually happened.**

A place, object, machine, animal, activity, or other entity may expose its own Gesture providers. The available capabilities are discovered from the relevant environment rather than hard-coded into a universal actor controller. A bar can offer sit, order, talk, dance, play piano, or leave; a piano offers its own instrument-specific procedures; a doorway offers threshold-crossing procedures; a pet-store window can expose attention stimuli and an invitation to investigate.

The provider's vocabulary is not an instruction queue. It is a set of possibilities with requirements, parameters, target anchors, costs, and expected outcomes. An actor filters that set from its own perspective, then chooses using its goals and current state. A procedure only starts after the choice is made and its preconditions are satisfied.

This preserves the critical distinction between *what an actor does* and *how the actor does it*.

## Four stages, four owners

```text
PLACE / OBJECT / ACTIVITY
  exposes provider capabilities, stimuli, anchors and preconditions
                 |
                 v
DISCOVER -> FILTER FROM ACTOR PERSPECTIVE -> CHOOSE
                 |                              |
                 | candidate set                | selected intent
                 v                              v
          Forge-baked index              ACTOR / DOMAIN POLICY
                                                |
                                                v
                                     RESOLVE PROCEDURE INSTANCE
                                                |
                                                v
                                      EXECUTE NESTED PROCEDURE
                                                |
                                +---------------+----------------+
                                v                                v
                       GestureEvaluator                    Physics / domain
                       pose/progress output                achieved outcome
                                |                                |
                                +---------------+----------------+
                                                v
                                  AUTHORITATIVE RESULT / EVENT
```

- **Provider / environment:** declares capabilities that are valid for this environment, their context requirements, semantic anchors, parameter domains, nested procedure references, and outcome expectations.
- **Forge:** validates references and composition, resolves inheritance/overrides, checks version compatibility and cycles, computes reusable indexes/graphs, and emits a compact runtime-ready representation. Forge is build-time work; runtime must not repeatedly interpret the full authoring graph.
- **Actor / decision policy:** owns intent, needs, interests, goals, memories, commitments, personality/context, and selection. The actor is not given a single forced behavior merely because a provider exists nearby.
- **Procedure runtime:** takes a selected action and concrete target/context, checks live preconditions, advances the nested procedure, handles interruption and blocked/failure outcomes, and asks the existing Gesture evaluator for pose output where appropriate.
- **Domain and Physics:** own permission, stock, ownership, reachability, contact, collisions, and authoritative results. An evaluated pose is not proof of a successful interaction.
- **Renderer / Visualize:** choose and display representation. Presentation cadence never becomes the authority for decisions or world-state changes.

## Candidate provider contract

Keep the first authoring format readable in the Forge and compile it into runtime data. The conceptual record below is intentionally illustrative, not a proposed public C# type declaration:

```yaml
provider:
  id: pet-store.window
  version: 1
  capabilities:
    - id: investigate-puppies
      intent: observe
      targets: [window.puppy-display]
      requires:
        - actor.can-perceive
        - target.is-available
      actor_filters:
        - interest.animals
        - current-goal-compatible
      procedure: pet-store.approach-and-observe
      outcomes:
        - attention.focused
        - opportunity.enter-store
```

The record says the environment makes an opportunity available and describes its requirements. It does **not** say every passing actor must investigate, nor does it assign every actor an animal interest. Those values belong to the actor and its decision policy.

A provider capability should ultimately have stable identity and version, an environment/domain scope, discoverability rules, preconditions, target/anchor requirements, typed parameters, nested procedure reference, interruption/failure behavior, and declared outcomes/events. Add fields only when concrete fixtures require them. Avoid freezing a large universal schema before a working vertical slice demonstrates the minimum.

## How an actor decides what it wants

A nearby provider should not answer "what does this Digiten want?" It should expose a bounded list of relevant opportunities. A separate actor policy evaluates those opportunities against actor-owned state.

A candidate selection pass can conceptually be:

1. **Perceive:** obtain stimuli/opportunities within the actor's relevant sensory and locality bounds.
2. **Filter:** reject impossible or invalid candidates (unreachable, unavailable, forbidden, incompatible with a hard commitment, missing required capability).
3. **Score:** estimate relevance using current needs, goals, interest, personality, memory, social context, expected cost, risk, and competing opportunities.
4. **Select:** choose according to a declared policy—highest score, weighted seeded choice, utility with hysteresis, or another explicit strategy.
5. **Commit:** record the chosen intent and any commitment duration so a tiny score fluctuation does not cause thrashing.
6. **Execute and observe:** follow the resolved procedure; accept authoritative domain/Physics outcomes; reconsider only at meaningful events, failure, interruption, or a decision checkpoint.

This is a decision-policy boundary, not a claim that one universal utility equation captures all intelligence. Different agents and domains can supply different policies while sharing provider discovery and compiled capability records.

### Attention is a signal, not a command

A stimulus can carry type, location, strength, range/falloff, temporal persistence, and a source identity. Attention score alone must not force behavior. Perception and response also depend on the actor's senses, interests, goals, current activity, distance, occlusion, competing stimuli, and willingness to interrupt its current task.

For example, the pet-store window may be visible to twenty passersby. Only some notice it; fewer consider stopping; fewer still enter and buy. The store's stock and purchase rules decide whether a valid transaction can occur. The same Digiten identity and inventory persist after leaving, enabling later dog-food and dog-park decisions.

## Nested procedures without runtime graph-walking

A provider may offer a simple gesture or a complex procedure composed from other procedures. The authoring graph should be expressive; the runtime plan should be compact.

The Forge should validate and bake, at minimum:

- stable provider/capability/procedure IDs and versioned references;
- dependency resolution and explicit parameter/anchor binding;
- cycle detection for composition, with recursion permitted only if a future contract defines safe bounded semantics;
- typed parameter ranges and defaults;
- precondition and outcome boundaries;
- interrupt, cancel, blocked, failure, and completion paths;
- shared subprocedure reuse without duplicating immutable definition data;
- actor-specific variation slots keyed from stable identity/seed, with independent procedure progress;
- lookup indexes for environment scope, capability tags, target type, and relevant stimuli;
- diagnostic traces that can explain why a capability was excluded, selected, blocked, or failed.

The baked artifact should separate immutable shared definitions from small per-actor execution state. Actor state might contain selected capability/procedure IDs, bound targets/parameters, program counter or node state, local clocks, seed-derived variation, and outcome evidence. It should not contain a copy of the full provider graph.

Avoid prematurely choosing a binary layout or GPU representation. First measure compiled graph size, lookup cost, allocation count, execution cost, and memory per active actor on representative workloads. The current CPU evaluator remains the semantic reference for its existing pose contract; nested procedure execution is a separate future layer.

## Efficient filtering and execution

Use staged filtering so an actor never scans every capability in the world:

1. **Locality index:** discover providers in the current place, nearby objects, current activity, and perceptible stimuli.
2. **Pre-baked capability index:** narrow by provider scope, capability tags, actor capability, and target kind.
3. **Cheap hard filters:** reject invalid, unavailable, unreachable, or disallowed candidates before expensive scoring.
4. **Actor policy:** score only the survivors, using cached actor features and relevant state.
5. **Procedure dispatch:** execute the chosen compiled plan, sharing immutable definitions and common environmental inputs.
6. **Cadence tiers:** decision updates occur at meaningful checkpoints; active movement/contact updates follow physical stability needs; distant presentation can update less frequently; persistence commits at authoritative transaction boundaries.

Batching is an execution strategy, not a social relationship. Actors may share an evaluator batch when they have compatible work and inputs while retaining individual identities, seeds, goals, progress, and outcomes. A cohort must be free to change as actors move between sidewalk, shop, bar, airport walkway, or park. Any batch optimization must preserve parity with the scalar reference and avoid order-dependent results.

## The Forge workflow should be diegetic

The authoring experience should let a creator inspect and compose the same concepts the world uses:

- browse places and objects, then inspect each one's offered providers;
- inspect a capability's target anchors, preconditions, procedure composition, outcomes, and failure paths;
- view a nested procedure as a graph/timeline, expand shared subprocedures, and see cycles or unresolved references flagged before baking;
- apply an actor profile to preview which opportunities survive filtering and why;
- step through a deterministic seed to see a choice, bound parameters, nested procedure progress, and authoritative result;
- compare two actor seeds side by side to show individualized motion from shared definitions;
- inspect generated runtime artifacts and diagnostics rather than treating the editor display as the implementation;
- generate SVGs/timelines from actual baked plans and execution traces, labelling conceptual views separately.

The interface should make the world legible *from inside the world*: a piano advertises instrument actions; a door advertises valid crossing procedures; a moving sidewalk changes traversal affordances; a pet-store display emits attention opportunities. A Forge author should be able to ask both "what does this object provide?" and "why would this actor choose it?"

## First end-to-end proof

Build one small deterministic reference fixture before a universal behavior framework:

1. A sidewalk and pet store expose distinct providers; the window emits a puppy attention stimulus.
2. Two actors pass the same window with different goals/interests. Show the actual candidate sets, filtering reasons, and selected decisions.
3. One actor chooses to investigate, enters through a valid threshold procedure, and completes a store-authorized purchase.
4. The actor exits carrying the puppy. The authoritative inventory/ownership record persists under the same actor identity.
5. At a later decision checkpoint, the actor chooses dog food and subsequently visits a dog park.
6. A second actor can ignore the window without being treated as incorrect.
7. A deterministic execution trace produces the Forge graph view, actor decision explanation, procedure timeline, and world-state event log. These are generated from the same records used by tests.

Acceptance means the same inputs and seed reproduce decisions; filtering has explainable reasons; unavailable/invalid actions cannot commit; interruption and failed outcomes do not advance state incorrectly; identity and ownership survive place transitions; and generated evidence matches actual execution. Only after this works should population size and batch performance be increased and measured.

## Relationship to existing Gestures contracts

The current `GestureDefinition`, `GestureEvaluator`, `GestureVariation`, and `EvaluatedPose` remain the narrow physical-motion reference. This proposal does not turn them into a world AI, decision engine, physics solver, or universal FSM. The provider/Forge/procedure layer can consume those contracts while remaining separately testable and host-neutral.

See also [Environment-Defined Gesture Vocabulary](ENVIRONMENT_DEFINED_GESTURE_VOCABULARY.md), [The Gestures Ecosystem](MULTI_PARTICIPANT_CHOREOGRAPHY.md), [Architecture](ARCHITECTURE.md), [Data Model](DATA_MODEL.md), and the [Roadmap](ROADMAP.md).
