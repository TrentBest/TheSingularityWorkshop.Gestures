# Gesture ontology coordination fixtures

> **Status: vocabulary-pending design fixtures.** These examples make the semantic questions explicit without inventing nine-layer coordinates, canonical token IDs, or claiming that the ontology adapter exists.

This worksheet is the handoff between the Gestures domain and the owner of the canonical Workshop ontology vocabulary. It separates the reusable procedure from the actor's intent, the activity plan, the live environment, and observed physical outcomes.

## Contract for every fixture

| Field | Meaning | Current status |
|---|---|---|
| Procedure ID | Stable identifier for a reusable Gesture procedure | Illustrative domain ID |
| Intent / activity | Why an actor or activity wants the procedure | Context supplied by actor/activity policy |
| Semantic relations | Concepts this procedure relates to; not an implied taxonomy path | Proposed relations; vocabulary owner must resolve canonical terms |
| Actor capabilities | Explicit capabilities required for this provider offer | Supported by `GestureOfferSelector` as exact string names |
| Semantic anchors | Named anchor kinds required by the procedure | Supported as exact string names; actual instances/transforms are world-owned |
| Preconditions | Conditions that must hold before an attempt is meaningful or safe | Design contract; no generic precondition engine claimed |
| Outcome evidence | Observable fact required before an activity advances | Physics/world authority; not inferred from animation completion |
| Nine-layer address | Approved ontology/version and values for Paradigm → Species | **Pending vocabulary owner; deliberately unassigned** |
| Runtime owner | Component responsible for execution or evidence | Identified per fixture below |

Do not copy the illustrative IDs and relation labels into a canonical ontology as approved terms without review. In particular, a human-readable string is not automatically a TopicToken, a layer value, or a resolved integer.

## Fixture A — `board-through-hatch`

**Use cases:** a crew member boarding a spacecraft; a player entering a vehicle; an actor passing through another compatible opening.

- **Reusable procedure:** traverse a designated opening while respecting the actor's locomotion capabilities and the opening's geometry.
- **Intent supplied by caller:** board a vehicle, enter a room, or pass through a portal. The procedure does not decide which goal the actor should pursue.
- **Candidate semantic relations:** locomotion; traversable opening; vehicle hatch; boarding; clearance; actor capability. These are relation candidates, not canonical token spellings.
- **Offer requirements (illustrative):** actor capability `locomotion`; semantic anchor `hatch`.
- **Preconditions to evaluate:** a compatible opening instance is resolved; the passage is clear enough for the actor's current shape; the actor and destination are in a valid state.
- **Outcome evidence:** the world observes the actor crossing the opening and reaching the intended side. Finishing a keyframe sequence alone is insufficient.
- **Runtime owners:** provider offers the procedure; actor policy selects it; world resolves the hatch instance; Physics/world evaluates clearance and crossing; activity coordinator consumes the resulting evidence.
- **Nine-layer address:** **unassigned** pending approved vocabulary, ontology ID/version, and authoring-resolver semantics.

## Fixture B — `step-to-beat`

**Use cases:** a flash mob, a dance lesson, a parade, or an actor synchronizing movement to a shared event.

- **Reusable procedure:** perform a step phrase relative to a supplied beat or timing signal.
- **Intent supplied by caller:** participate in a flash mob, follow a lesson, or keep time with a group. These contexts may reuse the same procedure.
- **Candidate semantic relations:** locomotion; rhythm; synchronization; shared beat; performance participation; skill/profile. Rhythm alignment is not the motion procedure itself.
- **Offer requirements (illustrative):** actor capability `locomotion`; semantic anchor `beat-source`.
- **Preconditions to evaluate:** the timing source is valid and its time domain is known; actor policy permits the selected phrase and tempo.
- **Outcome evidence:** measured phase/timing error and evaluated motion are available to the activity policy. Whether the result is acceptable is a policy decision, not a universal Gesture success flag.
- **Runtime owners:** activity provides context and participation roles; actor policy chooses an offer; playback/evaluator generates motion; timing source provides beat data; renderer presents the result.
- **Nine-layer address:** **unassigned** pending approved vocabulary, ontology ID/version, and authoring-resolver semantics.

## Fixture C — `close-cover`

**Use cases:** closing a service panel, securing a starport vehicle cover, or completing a maintenance procedure.

- **Reusable procedure:** move a compatible cover toward its closed state while respecting contact and mechanism constraints.
- **Intent supplied by caller:** secure a particular cover as one step of a larger activity.
- **Candidate semantic relations:** manipulation; cover mechanism; contact; latch state; clearance; safety check. A task dependency is not itself proof of closure.
- **Offer requirements (illustrative):** actor capability `manipulation`; semantic anchor `cover`.
- **Preconditions to evaluate:** correct cover instance resolved; mechanism is operable; swept path is clear; required safety checks have passed.
- **Outcome evidence:** the world observes the cover at the required closed state and, where applicable, confirms latch engagement. Playback completion and departure-clearance workflow state are not substitutes.
- **Runtime owners:** activity/starport workflow establishes prerequisites; provider offers the procedure; world resolves the cover; Physics/world observes contact and state; workflow advances only on its required evidence.
- **Nine-layer address:** **unassigned** pending approved vocabulary, ontology ID/version, and authoring-resolver semantics.

## Questions for the ontology vocabulary owner

1. What are the approved ontology ID and version for these concepts, and where is the authoring resolver's source of truth?
2. What are the accepted values and semantics at each of the nine layers: Paradigm, Domain, Kingdom, Phylum, Class, Order, Family, Genus, Species?
3. Are these procedure concepts expected to receive one address, multiple addresses in different ontologies, or a primary address plus explicit relationships?
4. What is the canonical representation for synonyms, capability compatibility, semantic-anchor kinds, and relations such as `uses-anchor`, `requires-capability`, or `achieves-state`?
5. How are integer values allocated, versioned, deprecated, and resolved to author-facing terms? Can an address remain stable when the vocabulary evolves?
6. Which component owns validation and lookup, and what adapter contract should Gestures consume without taking a dependency on Workshop composition or duplicating `OntologySignature`?

## Implementation boundary

The current runtime can filter provider offers using explicitly supplied capability names and semantic-anchor names. It does not yet resolve ontology coordinates, traverse ontology relations, rank intent, resolve transforms, validate the listed physical preconditions, or verify outcome evidence.

The next implementation step should be an adapter against the approved vocabulary/resolver—not a new nine-layer type or a table of guessed integers inside Gestures. Until the vocabulary owner answers the questions above, these fixtures are useful review artifacts but are not ontology data.
