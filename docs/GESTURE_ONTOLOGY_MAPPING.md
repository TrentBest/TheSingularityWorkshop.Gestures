# Gesture ontology mapping: canonical layers and runtime ownership

> **Status: design mapping, not a published Gesture ontology.** The canonical nine layer *names* are now confirmed against the Workshop's Experience Theory and the runtime `OntologySignature`. The concrete integer vocabularies and the correct classification of each Gesture still need to be agreed and resolved by the ontology authoring system.

## The answer: where do Gestures land?

**Gestures is a domain that contributes reusable physical procedures and their semantic relationships. It is not itself one of the nine layers.** A Gesture definition may receive an address in a selected ontology; the provider, actor, activity, live session, playback, and physical outcome remain separate identities and responsibilities.

The nine ordered layers in the current Workshop Experience Theory are:

| Zero-based index | Canonical layer name | What is established |
|---:|---|---|
| 0 | **Paradigm** | Layer name and position |
| 1 | **Domain** | Layer name and position |
| 2 | **Kingdom** | Layer name and position |
| 3 | **Phylum** | Layer name and position |
| 4 | **Class** | Layer name and position |
| 5 | **Order** | Layer name and position |
| 6 | **Family** | Layer name and position |
| 7 | **Genus** | Layer name and position |
| 8 | **Species** | Layer name and position |

Source of truth: [WebPage Experience Theory](https://github.com/TrentBest/WebPage/blob/master/EXPERIENCE_THEORY.md#rule-2--sensory-definition-and-ontology) defines the nine ordered names. The [runtime OntologySignature](https://github.com/TrentBest/The/blob/master/Architect/Workshop/Ontology/OntologySignature.cs) defines exactly nine integer slots and resolves those integers to author-facing strings through a resolver.

This corrects an earlier draft in this repository that treated **Moniker** and **TopicToken** as the names of positions 0 and 8. They are not the canonical layer names. Moniker is a Workshop concept/capability; TopicToken may be a vocabulary or leaf-identity concept. Either can be classified or related through the ontology, but neither replaces Paradigm or Species as a layer label.

## Important distinction: layer labels are known; Gesture coordinates are not

The canonical *shape* is established. The domain-specific value assigned at each position is not yet established by this mapping document. We should not make up integer tokens, assume the word “Gestures” is the correct value for every Domain coordinate, or infer that each Gesture must occupy a single linear taxonomy path.

The names resemble a taxonomy, but the ontology system also supports explicit relationships and multiple ontologies. Therefore:

- Keep the nine canonical layer labels stable.
- Obtain actual layer values/tokens from the master ontology or the approved authoring vocabulary.
- Keep synonyms, compatibility, aliases, multiple parents, and cross-domain links as explicit relationships where appropriate—not as invented layer meanings.
- A semantic address is not a provider ID, package location, runtime dependency list, execution schedule, or playback handle.
- Do not bake placeholder strings or guessed integer coordinates into the Gestures runtime API.

## Three structures that must stay separate

1. **Semantic coordinate** — the nine integer values classifying a concept in one ontology/version.
2. **Relationship graph** — explicit links among concepts, providers, procedures, actor capabilities, contexts, and manifestations, potentially across ontologies.
3. **Runtime composition graph** — the actual providers, offers, actors, activity definitions, physics capabilities, and presentation capabilities assembled for one run.

The [Ontology Theory](https://github.com/TrentBest/TheSingularityWorkshop.Ontology/blob/master/docs/THEORY.md) states that ontology is optional semantic structure, not a mandatory constraint on every MicroBundle. The separate [Ontology package](https://github.com/TrentBest/TheSingularityWorkshop.Ontology) supports variable-rank indexing; the fixed nine-slot signature is the Workshop Experience/Hub contract, not a universal requirement imposed on all consumers.

## Gesture concepts and the runtime owners

| Concept | Ontology contribution | Runtime/data owner |
|---|---|---|
| Paradigm/Domain/Kingdom/Phylum/Class/Order/Family/Genus/Species values | Authoritative classification vocabulary and coordinates | Master ontology and authoring resolver |
| Reusable physical procedure | Semantic identity and links to related concepts | `GestureDefinition`, keyframes, anchors, constraints |
| Provider offering a procedure | Links between provider, procedure, and eligibility concepts | Provider registry and `GestureOfferSelector` |
| Actor suitability | Capability concepts and semantic relationships where useful | Actor/gameplay system supplies current capabilities and policy |
| Semantic anchor kind, e.g. vehicle hatch | Stable concept and compatibility relationships | Consumer/world supplies current anchor instances and transforms |
| Flash mob, tag, hide-and-seek, starport departure | Activity concepts and links to reusable procedures | Versioned `ActivityDefinition`, roles and phases |
| One live instance of an activity | Optional semantic links for inspection/discovery | Session coordinator and live session identity/state |
| Current playback progress | Usually runtime state, not taxonomy | `GesturePlayback` / `GestureSequencePlayback` |
| Collision, reachability, force, physical success | Concepts may be classified and related | Physics/world authority |
| Evaluated pose and presentation fidelity | Concepts may link to manifestation capabilities | Evaluator, Renderer, Visualize |
| Package/bundle and download location | Optional link from semantic concept to implementation | MicroBundleDomain and repository |

## Example procedures without invented coordinates

These examples identify semantic concepts and reuse relationships. They intentionally do **not** assign unapproved integers or claim that the strings below are already canonical ontology tokens.

### Board through a hatch

- Procedure concept: `board-through-hatch`
- Related concepts: locomotion, traversable opening, vehicle hatch, boarding intent, actor capability, clearance requirement.
- Multiple providers may implement the same semantic procedure for different articulated mechanisms.
- Actual anchor transforms and physical feasibility belong to the world/Physics boundary.

### Step to a beat

- Procedure concept: `step-to-beat`
- Related concepts: locomotion, rhythm synchronization, shared beat, performance participation, skill/profile.
- A flash mob and a studio scene may reuse the procedure while applying different roles, policies, targets, and presentation.
- The beat/synchronization relation is not itself the motion procedure.

### Close a cover

- Procedure concept: `close-cover`
- Related concepts: manipulation, cover mechanism, contact, latch state, clearance and safety checks.
- A starport workflow can reuse the procedure for different compatible covers.
- Workflow prerequisites and departure clearance are not proof of physical closure; the world must supply outcome evidence.

These concepts should be assigned actual nine-layer coordinates only after the vocabulary owner provides the approved values and intended meaning for each level.

## From semantic meaning to execution

```text
Approved ontology term/address
          |
          v
Discover providers and Gesture offers
          |
          v
Filter actor capabilities + available anchor kinds
          |
          v
Actor / activity policy chooses intent and offer
          |
          v
Resolve actual anchors and evaluate the procedure
          |
          v
Physics / world observes the achieved outcome
          |
          v
Activity coordinator advances from valid evidence
          |
          v
Renderer presents the evaluated result at observer-appropriate fidelity
```

The current `GestureOfferSelector` implements only the filtering step using declared capability names and available semantic-anchor names. It does not interpret `OntologySignature`, resolve integer tokens, rank actor intent, resolve anchor transforms, or prove physical success. The broader flow is a design boundary, not a claim that the full pipeline exists.

## Identity and versioning

Keep these identities distinct:

- **Ontology ID/version + nine-layer coordinate:** semantic location within the selected schema.
- **Semantic concept ID/token:** identity of the concept in the vocabulary, if separately assigned.
- **Gesture definition ID/version:** authored physical procedure and its revisions.
- **Provider ID/version:** implementation or bundle offering the procedure.
- **Activity ID/version:** organized activity definition.
- **Session ID/revision:** one live occurrence.
- **Playback handle:** one execution lifecycle.

A concept can be reused by multiple providers and activities. A provider can change without changing the concept. An activity can reuse a procedure without owning it. A definition can evolve while its broader meaning remains stable. Semantic-breaking changes should be versioned or explicitly related to a replacement concept rather than silently repurposing a token.

## Questions for ontology coordination

1. Where is the authoritative vocabulary/resolver for the nine integer values maintained, and how are changes reviewed/versioned?
2. What values should the master ontology assign to Gesture procedures, and are those values already defined in another repository?
3. Which facts belong in coordinates versus relationship edges—especially intent, target kind, skill/profile, compatibility, and manifestation?
4. How should a concept with multiple parents/facets or cross-ontology links be represented without forcing a single path?
5. Is TopicToken an established term in the vocabulary layer? If so, is it a concept ID, a terminal classification, or a compact lookup token? It is not the canonical name of layer 8.
6. What author-time semantic facts are stable, and what must remain runtime state such as current actor capabilities, available anchors, and observed physical outcome?

## Recommended next step

Treat the nine layer names as canonical and the actual Gesture coordinates as unresolved. Ask the master-ontology owner to supply the approved token vocabulary and definitions, then validate `board-through-hatch`, `step-to-beat`, and `close-cover` against it. Only after that review should we add ontology-aware lookup or expose ontology tokens in runtime APIs.

**Ontology makes meaning addressable. Gesture providers make procedures available. Actor and activity policy choose. Physics/world determines what actually happened. Renderer presents the result.**