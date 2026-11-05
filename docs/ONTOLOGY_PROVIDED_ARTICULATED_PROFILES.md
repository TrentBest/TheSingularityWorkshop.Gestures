![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Ontology-provided articulated profiles: armatures and gestures

> **Status: architecture direction and proposed data contract.** This document records the default discovery model. It does not claim that the ontology resolver, serialized armature schema, or profile loader is implemented in Gestures.

## The default should be ontology-first

A consumer should not begin by asking a global animation library for a generic clip named `walk`. It should begin with the ontology of the thing it wants to represent.

For example:

> “I have a cat. Resolve the cat's ontology profile.”

That profile should identify, or resolve references to:

1. **Entity concept** — what the entity is, with stable ontology identity and version.
2. **Armature definition** — the articulated structure appropriate to that entity/profile.
3. **Gesture catalog** — procedures meaningful for that structure and entity.
4. **Compatibility metadata** — the armature versions, transform roles, capabilities, and optional constraints each Gesture expects.
5. **Extensions and relationships** — inherited or composed capabilities that are explicitly declared by the ontology.

The ontology is the default semantic entry point. A caller may still explicitly supply a custom profile or override, but should not need to manually assemble a cat's skeleton and discover its gestures every time.

## Example: the cat profile

A cat is not a small humanoid. Its profile should describe a feline articulated body, not rename human bones.

An illustrative cat armature might contain:

- axial structure: pelvis, lumbar spine, thoracic spine, neck, head;
- forequarters: scapular structures, upper forelimb, lower forelimb, carpus, digits/paws;
- hindquarters: pelvis/hip articulation, femur, lower hindlimb, hock, digits/paws;
- tail: a chain of independently articulated segments;
- ears and jaw, where the chosen rig supports them.

The exact hierarchy, naming, rest transforms, coordinate conventions, and degrees of freedom belong to the versioned armature definition. These labels are examples for discussion, not canonical ontology tokens or a mandated rig schema.

The cat's associated Gesture catalog could include:

- locomotion: walk, trot, run, stalk, turn, back up;
- posture: stand, sit, crouch, loaf, stretch;
- transitions: crouch-to-leap, leap, land, recover;
- interaction: paw, bat, scratch, groom;
- expressive articulation: tail posture/signals, ear orientation, head tracking.

These are not all one universal animation set. Each offer declares what armature roles it uses, its parameters, and any required capabilities or anchors. A tail-signal Gesture can be evaluated without requiring a full-body locomotion procedure. A landing procedure can declare a richer set of requirements.

## The profile is a semantic package, not a baked animation clip

A useful conceptual graph is:

```text
Ontology entity concept: Felis catus
              |
              +---- resolves armature profile/version
              |          |
              |          +---- articulated topology
              |          +---- semantic transform roles
              |          +---- rest/reference pose
              |          +---- units and coordinate conventions
              |
              +---- resolves Gesture catalog/version
                         |
                         +---- walk / trot / stalk
                         +---- crouch / leap / land
                         +---- groom / scratch
                         +---- tail / ear / head procedures
                                      |
                                      v
                         compatibility declarations
                                      |
                                      v
                         concrete cat rig instance
                                      |
                                      v
                         validated adapter / mapping
                                      |
                                      v
                         Gesture evaluation -> Physics/world -> presentation
```

The ontology may contain the definitions directly or reference versioned resources owned by the armature and Gestures domains. Either way, the ontology resolution must lead to a coherent, versioned profile. It must not depend on searching a folder by a display name such as `cat`.

## Reuse without flattening species differences

Ontology relationships can express shared concepts and reusable capabilities. A cat and a dog may both be quadrupeds and share some high-level locomotion concepts, but that does not prove their armatures, gait timing, contact patterns, or gestures are interchangeable.

Use explicit relationships and compatibility declarations:

- A broad concept can contribute shared semantic vocabulary.
- A species profile can add or specialize armature structure and gesture offers.
- A breed, body-plan, age, or asset profile can select a compatible variant when the ontology actually defines one.
- A Gesture can declare compatible armature profile IDs/versions or required semantic transform roles.
- A consumer adapter validates the concrete rig and reports missing, ambiguous, or incompatible mappings.

Do not silently inherit every parent Gesture into every descendant. Inheritance, overrides, exclusions, and compatibility need deterministic rules. A query may discover candidates; it does not automatically authorize execution.

## Runtime responsibilities

| Concern | Owner |
|---|---|
| Entity meaning, ontology identity/version, relationships, profile resolution | Ontology system / authoring resolver |
| Armature topology, transform roles, reference pose, structural constraints | Versioned armature definition supplied or referenced by the ontology profile |
| Reusable movement procedures, keyframes, parameters, anchors, evaluation | Gestures |
| Concrete asset/rig instance and mapping to the declared armature | Consumer or host adapter |
| Intent, selection, personality, activity policy | Actor/gameplay/activity domain |
| Forces, contact, collision, balance, achieved physical state | Physics / authoritative world |
| Pixels, visual fidelity, textual observation | Visualize / Renderer |

Gestures should consume a narrow resolved-profile contract. It should not duplicate the Workshop's ontology signature, invent nine-layer integer coordinates, or require a direct dependency on the full ontology implementation merely to evaluate a procedure. The ontology resolver can live above the domain boundary and provide stable references plus validated profile data.

## Versioning and compatibility

Keep these identities distinct:

- ontology ID and version;
- entity concept ID;
- armature profile ID and version;
- Gesture definition ID and version;
- Gesture catalog/profile ID and version;
- concrete rig/asset instance ID;
- runtime mapping/registration revision.

A new armature version may preserve semantic identity while changing topology. A Gesture revision may add a requirement or correct a pose. Compatibility must be checked against the exact versions or an explicitly declared compatible range. Never infer compatibility from similar names or from both entities being classified as animals.

The resolved profile should be inspectable and serializable. It should explain which ontology relationship selected the armature and catalog, which versions were resolved, and why each Gesture was accepted or rejected for a target.

## How this fits the existing Gestures contract

Today, `GestureDefinition` contains normalized keyframes with named local transforms and optional semantic anchors. `GestureEvaluator` evaluates those transforms deterministically. It does not own a hierarchy, load an ontology, resolve a species, validate a concrete rig, or retarget arbitrary assets.

The existing articulated-motion proposal correctly keeps target hierarchy and transform mapping outside the base evaluator. Ontology-first profiles add the missing semantic discovery layer without changing that boundary: ontology resolves *which* armature and Gesture catalog are appropriate; the consumer adapter validates *this instance*; Gestures evaluates the selected procedure.

The existing ontology mapping also deliberately leaves concrete nine-layer coordinates unassigned until the canonical vocabulary owner supplies them. This proposal does not invent those values. The human-readable example `Felis catus` is explanatory only.

## Implementation sequence

1. Confirm the authoritative ontology resolver and the stable identity/version fields it returns.
2. Define or adopt the versioned armature resource schema in its owning domain; include hierarchy, transform roles, reference pose, units, and coordinate conventions only where required.
3. Define an ontology profile that links an entity concept to an armature profile and a Gesture catalog.
4. Specify deterministic relationship traversal, inheritance, override, exclusion, and compatibility rules.
5. Add a cat profile fixture with a distinctly feline armature and a small initial Gesture catalog.
6. Add dog and humanoid fixtures to prove that the same discovery pipeline does not impose the same armature or gesture vocabulary.
7. Validate concrete rig registration and report compatibility diagnostics before evaluation.
8. Only then expose an ontology-aware discovery API or serialized contract in Gestures.

The first proof should be data-driven: resolve a cat concept, obtain its armature and Gesture catalog, validate a concrete cat rig, and evaluate one feline Gesture. It should not be a hard-coded `if species == "cat"` branch inside the evaluator.

## Design rule

**Ontology tells us what the entity is and which articulated profile and procedures belong with it. The armature describes how that entity articulates. Gestures describe the procedures it can perform. Physics determines what actually happens.**

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
