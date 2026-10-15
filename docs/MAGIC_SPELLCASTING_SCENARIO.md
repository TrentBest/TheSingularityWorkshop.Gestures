# VR Magic — Gesture Weaving Scenario

**Status: executable design target with a test-backed evaluator fixture; not a VR implementation.** This scenario uses the existing GestureDefinition and CPU evaluator to prove that a spell can be described as timed physical procedures. Input tracking, elemental visuals, spell rules, audio, authoritative effects, and VR presentation remain host/domain responsibilities.

## The interaction

The player pulls the **left trigger** to enter spell-weaving mode. The five elements are presented around a central weave:

- Earth
- Air
- Water
- Fire
- Spirit — with a selected alignment of **good, neutral, or evil**

Spirit is one element with an alignment, not three additional elements. The alignment is a spell/domain property; it does not imply that the player's body or Gesture evaluator changes moral state.

As the player moves an arm toward an elemental node, the host interprets tracked motion and requests the corresponding physical procedure. The visual flow should follow the tracked limb: energy races along the forearm toward the elbow, travels up the arm, then arcs into the casting sphere. At the sphere, the element becomes a colored thread in the weave.

The left trigger is then released to request a cast. Releasing is a request, not proof of a successful spell: the host validates the completed weave and either accepts the spell, explains the unmet requirement, or follows a defined failure/interruption policy.

## The weave is a typed input, not just a pretty ribbon

Each thread needs at least:

- **Element identity** — Earth, Air, Water, Fire, or Spirit.
- **Color** — presentation metadata selected by the element/skin policy.
- **Thickness** — measured or authored scalar with a documented unit and finite range.
- **Acquisition evidence** — which tracked movement/interaction selected the element, with the host responsible for confidence and tracking validity.
- **Weave position/order** — where and when the thread entered the spell pattern.
- **Optional modifiers** — bounded amplifications or transformations accepted by the spell definition.

Each spell declares its own weave grammar: required elements, ordering or placement constraints, acceptable thickness interval per thread, and allowed modifiers. A forgiving spell can accept a broad thickness interval. A precision spell can demand a narrower interval. Where the spell permits a thickness range, the chosen thickness may affect power or amplification; it must not silently change an element's identity or bypass required constraints.

Avoid treating a visually thick thread as proof of actual power. The host owns the accepted input value and spell calculation; Visualize merely presents the resulting state.

## Gesture procedure boundaries

The existing Gesture model is suitable for describing rig-independent motion procedures with named transforms and anchors. A consumer can author procedures such as:

1. magic.reach-element — move the hand toward a provider-defined elemental node.
2. magic.channel-to-elbow — follow the tracked arm path toward the elbow.
3. magic.arc-to-casting-sphere — carry the element from the elbow/hand path to the casting sphere.
4. magic.return-to-weave — finish the thread placement and return toward a ready pose.
5. magic.cast-release — a physical release/settle procedure after the host accepts a cast.

These are example identifiers, not new public APIs. Gesture keyframes express ideal local transforms; named anchors express spatial relationships. The world/VR host resolves tracking, world-space targets, reachability, obstruction, and actual hand contact. The existing CPU evaluator does not read controllers, solve arm IK, simulate energy, or determine spell validity.

## State and interruption

Spell-weaving mode is a host-owned state machine, naturally expressible through the Workshop's state technology, but the Gestures library must not depend on FSM_API to remain usable.

Suggested progression:

Ready → Weaving → Validating → CastAccepted | CastRejected

The host should also define interruption exits for trigger cancellation, tracking loss, unreachable nodes, a changed spell selection, invalid thread thickness, and player movement that invalidates the weave. An interrupted weave must not accidentally cast merely because a stale trigger-release event arrives.

Each acquired thread is retained as explicit weave data. Do not infer the complete spell from the final pose alone: two visually similar poses may encode different element order, alignment, or modifier choices.

## Suggested acceptance tests

- The same Gesture definition, elapsed time, and identity seed produce the same evaluated pose.
- Reaching and channeling are separate physical procedures with provider-defined anchors.
- A thread cannot be accepted when its thickness is non-finite or outside the spell's declared range.
- Spirit alignment accepts only good, neutral, or evil and is attached only to a Spirit thread.
- Releasing the trigger does not cast until the host validates all required threads and constraints.
- Missing elements, wrong order, invalid thickness, tracking loss, and interruption are distinguishable outcomes.
- Variation within a spell's declared tolerance is accepted; variation outside it is rejected.
- Visual thread color/thickness follows the accepted weave state, not an independent renderer guess.

## Ownership map

| Concern | Owner |
|---|---|
| Ideal physical procedure, keyframes, anchors, deterministic pose evaluation | Gestures |
| Controller input, tracked joints, IK target selection, tracking confidence | VR/input host and kinematics integration |
| Spell grammar, spirit alignment, thickness limits, modifiers, acceptance and effects | Magic/spell domain |
| Trigger mode and interruption lifecycle | Experience/behavior host |
| Element trail, colored thread, casting sphere, feedback visuals | Renderer/Visualize consumer |
| Sound, voice, elemental audio and spatial mix | Audio capability |
| Optional purchases, crafting materials, or vendor interactions | Optional Economy adapter; never a prerequisite for spellcasting |

## Implementation status

The accompanying test fixture evaluates actual reach/channel/arc Gesture definitions through the existing CPU evaluator and tests a small spell-weave acceptance model. It is an executable contract example, not a tracked-controller integration, IK solution, particle/effect system, production spell engine, or live VR demo.
