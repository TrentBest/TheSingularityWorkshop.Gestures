# Gesture ontology mapping: semantic address versus runtime composition

> **Status: design proposal.** This document maps Gestures into the Workshop's optional, versioned Ontology model. The nine semantic roles below are a candidate mapping for discussion, not a claim that the Ontology package already defines these names as a universal nine-layer schema.

## The question

Where does Gestures sit when a MicroBundle can be located within a nine-position semantic address?

**Gestures is a domain that contributes procedures and their relationships to an ontology; it is not itself one ontology layer.** A Gesture definition can have a semantic address, while the provider that supplies it, the actor who may perform it, the experience that uses it, and the runtime session that coordinates it remain distinct objects.

Keep three structures separate:

1. **Semantic address** — where a concept is classified in a selected ontology.
2. **Relationship graph** — how concepts are related across branches or even across coexisting ontologies.
3. **Runtime composition graph** — which providers, gesture definitions, actors, experiences, physics capabilities, and presentation capabilities are composed for a particular run.

An address is not a dependency list, a provider registry, an execution schedule, or a parent-child ownership tree.

## Ontology boundary and constraints

The Workshop Ontology package intentionally supports scalar or N-dimensional OntologyIndex values and does not impose five or nine dimensions on every ontology. See the canonical [Ontology README](https://github.com/TrentBest/TheSingularityWorkshop.Ontology/blob/master/README.md) and [Ontology Theory](https://github.com/TrentBest/TheSingularityWorkshop.Ontology/blob/master/docs/THEORY.md) for that boundary. A nine-position OntologySignature is a consumer's selected scheme, not a global rule for every MicroBundle.

This proposal preserves the known anchors used in Workshop discussions:

- **Position 0: Moniker** — the default/root experience context, when the chosen ontology uses this convention.
- **Position 8: TopicToken** — a possible leaf/topic coordinate, as used in the Physics ontology design example.

The intermediate names below are proposed semantic roles only. Before these are treated as canonical, compare them with the actual master ontology definition and agree on shared meanings. If a different ontology uses different dimensions, the same Gesture concept may have another address there.

## Candidate nine-position mapping for a Gesture

| Position | Proposed semantic role | Gesture example | What this position must not mean |
|---:|---|---|---|
| 0 | **Moniker / root experience** | moniker or a selected default world context | A mandatory parent object for every gesture runtime |
| 1 | **Domain** | gestures | A DLL, repository, or provider implementation |
| 2 | **Family** | locomotion, manipulation, interaction, expression | A closed enum that prevents new families |
| 3 | **Capability** | move-through-opening, operate-latch, signal-attention | A single baked animation clip |
| 4 | **Procedure** | board-through-hatch, close-hatch, step-to-beat | A live playback instance |
| 5 | **Intent / action class** | board, secure, dance, evade | Proof that the action succeeded |
| 6 | **Context / target kind** | vehicle-hatch, partner, ground-plane, shared-beat | A resolved world transform or current target instance |
| 7 | **Variant / profile** | trained, novice, low-clearance, accessible | Per-frame random variation or an actor's entire identity |
| 8 | **Topic token / stable leaf** | stable token for the precise concept | A mutable display label or a unique ID for every playback |

This is a candidate semantic path, not a requirement that every Gesture fill all nine positions or have exactly one path. A concept can be related to multiple families, contexts, or intents. Use explicit relationships or multiple addresses where the ontology supports them; do not force multiple meanings into one arbitrary slot. A concrete Gesture definition still needs its own stable ID and version independent of its semantic address.

### Example semantic paths

Illustrative paths only; token values and canonical names must come from the selected ontology, not from hard-coded strings in the Gestures runtime.

~~~text
moniker / gestures / locomotion / traverse-opening
        / board-through-hatch / board / vehicle-hatch
        / trained / <stable-topic-token>

moniker / gestures / manipulation / operate-mechanism
        / close-hatch / secure / hatch-handle
        / low-clearance / <stable-topic-token>

moniker / gestures / expression / rhythmic-movement
        / step-to-beat / dance / shared-beat
        / novice / <stable-topic-token>
~~~

The same board-through-hatch procedure might be supplied by multiple providers, adapted for different articulated mechanisms, or offered to different actors. Those facts should be expressed as provider metadata, capability requirements, relationships, and context—not by changing the meaning of its stable concept token.

## What belongs in the ontology, and what belongs elsewhere?

| Concept | Ontology contribution | Runtime or data owner |
|---|---|---|
| Gesture family and semantic purpose | Classification terms and relationships | Gesture definition/provider |
| Reusable physical procedure | Addressable concept and relations to capabilities or target kinds | GestureDefinition, keyframes, anchors, constraints |
| Provider offering a procedure | Relationship between provider, procedure, and eligibility concepts | Provider registry and GestureOfferSelector |
| Actor suitability | Capability terms and semantic relationships where useful | Actor/gameplay system supplies current capabilities and policy |
| Anchor such as vehicle-hatch | Stable anchor-kind concept and relationships | Actual anchor availability/transforms belong to the consumer/world |
| Flash mob, tag, hide-and-seek, starport departure | Activity/experience concepts and cross-links to reusable procedures | Versioned ExperienceDefinition, phase/role/action model |
| One live flash mob or tag round | Optional semantic links for inspection and discovery | Session coordinator and live session identity/state |
| Current playback progress | Generally not a taxonomy coordinate | GesturePlayback / GestureSequencePlayback |
| Collision, reachability, force, physical success | Concepts may be classified and linked | Physics/world authority |
| Evaluated pose and observer fidelity | Concepts may be linked to presentation affordances | Evaluator, Renderer, Visualize |
| Provider package and where to download it | Package/bundle identity may link to a concept | MicroBundleDomain and MicroBundleRepository |

Ontology can classify and connect these objects; it should not absorb their runtime ownership.

## Gesture provider and actor flow

The intended path from meaning to execution is:

~~~text
Ontology term / semantic address
          |
          v
Discover providers and Gesture offers
          |
          v
Filter against actor capabilities + available anchor kinds
          |
          v
Actor / experience policy chooses an offer and intent
          |
          v
Resolve actual anchors and evaluate the procedure
          |
          v
Physics / world observes the achieved outcome
          |
          v
Experience coordinator advances only from valid evidence
          |
          v
Renderer presents the evaluated result at observer-appropriate fidelity
~~~

The current GestureOfferSelector implements only the filtering step: declared capability names and available semantic-anchor names are matched, eligible/rejected offers are returned deterministically, and missing requirements are reported. It does not interpret ontology tokens, rank intent, resolve anchor transforms, or prove physical success. The broader flow is an architecture boundary, not a claim that the full pipeline already exists.

## Cross-domain examples

### Flash mob

The experience is classified as a social group performance. It references several gesture procedures and defines participant roles, phases, timing, and participation rules. A shared beat is a synchronization concept; it is not the motion itself. Each participant receives a role-appropriate procedure and target. The player is not automatically enrolled because they are nearby.

### Tag and hide-and-seek

These are separate experience definitions. They may reuse locomotion, turning, stopping, reaching, looking, and social signaling procedures. Their differences belong in roles, rules, state transitions, observation gates, and policy—not in duplicating every shared Gesture under a game-specific branch.

### Starport departure

The experience links ground-crew and flight-crew roles to procedures for hoses, covers, landing gear, doors, and boarding. A shared close-cover procedure can be classified once and linked to several compatible mechanisms. Task prerequisites and clearance evidence belong to the departure workflow; physical contact and obstruction belong to Physics/world authority.

### Flash mob, film shoot, and raWWar

A common turn, walk, or reach procedure can be reused by distinct experiences and domains. Domain-specific providers can offer specialized variants without claiming ownership of the generic procedure. raWWar, a studio simulation, and a city experience can consume the same semantic concept while applying their own intent, policy, physics, and presentation.

## Stable identity, versioning, and evolution

Do not overload one identifier for all of these:

- **Semantic concept ID/token:** identity of the meaning in a specific ontology version.
- **Gesture definition ID/version:** identity of the authored procedure and its revisions.
- **Provider ID/version:** identity of the capability implementation or bundle that offers it.
- **Experience ID/version:** identity of the organized activity definition.
- **Session ID/revision:** identity and revision of one live occurrence.
- **Playback/session handle:** identity of one execution lifecycle.

A definition can evolve while its broad concept remains the same; a provider can change without changing the concept; an experience can reuse a procedure without owning it. Breaking semantic changes should be versioned or represented by a new concept and explicit relationship rather than silently repurposing an existing token.

## Questions to settle before hardening the mapping

1. What are the **canonical names and definitions** for positions 1–7 in the master nine-position ontology? The candidate roles above must not become a competing schema.
2. Is Moniker a required coordinate for all concepts, or only a root/default convention in the ontology that uses it?
3. Is TopicToken a leaf vocabulary ID, a terminal classification, or a compact index into a separate term table?
4. How does a concept express multiple parents, facets, synonyms, compatibility, and replacement without forcing a single taxonomy path?
5. Which cross-ontology relationships are first-class, and how are ontology IDs/versions included in semantic addresses?
6. Are capability, intent, target-kind, and variant best represented as coordinate positions, relationships, or a mix? The coordinate should not become an inflexible substitute for graph edges.
7. Which fields are author-time ontology metadata versus runtime facts such as anchor availability, actor capabilities, and physical outcome?

## Recommended next step

First record the canonical nine-position contract from the master ontology as the shared reference. Then validate this Gesture mapping against three fixtures—board-through-hatch, step-to-beat, and close-cover—and demonstrate that each can be reused across providers and experiences without duplicating its semantic identity. Only after that should we add ontology-aware lookup or bake ontology tokens into runtime APIs.

The principle is simple: **ontology makes meaning addressable; Gesture providers make procedures available; actors and experiences choose what to do; Physics determines what actually happened.**
