![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Feline articulated Gesture fixture

> **Status: executable Gestures-domain fixture.** This proves species-specific pose evaluation using feline transform roles. It is not an ontology-resolved profile, serialized armature schema, production cat rig, or retargeting implementation.

## Why a cat is a useful first proof

A cat makes it obvious when an animation system is quietly assuming a humanoid. A feline profile needs a quadruped body plan, forepaws and hindpaws, a flexible axial structure, and a segmented tail. Its movement vocabulary also differs: stalking, crouching, loafing, grooming, pawing, tail signaling, and landing are not renamed human clips.

The test fixture `FelineGestureFixtureTests` contains a small `cat.stalk` procedure with these illustrative roles:

- `root`, `pelvis`, `spine`, `neck`, and `head`;
- `left_forepaw` and `right_forepaw`;
- `left_hindpaw` and `right_hindpaw`;
- `tail_01` and `tail_02`.

The test verifies that the evaluator preserves and deterministically evaluates those named transforms. It deliberately rejects humanoid-specific role assumptions such as `left_hand` and `right_foot`.

## What this proves — and what it does not

| Concern | Fixture status |
|---|---|
| Gestures can evaluate a quadruped-specific transform set | Demonstrated |
| Named local transforms remain deterministic | Tested |
| Cat locomotion can be represented without a humanoid rig | Demonstrated at evaluator-fixture level |
| Ontology resolves a feline entity to an armature profile | Not implemented |
| Ontology resolves a versioned feline Gesture catalog | Not implemented |
| Canonical armature hierarchy, rest pose, units, and coordinate conventions | Not defined here |
| Concrete asset/rig validation and retargeting | Consumer adapter responsibility; not implemented |
| Physics contacts, balance, footfall, and achieved movement | Physics/world responsibility; not implemented |

## Intended ontology-first resolution

The production direction remains:

```text
ontology entity: cat
        |
        v
resolved feline profile
        |
        +----> versioned feline armature
        |
        +----> versioned feline Gesture catalog
                       |
                       v
              compatibility validation
                       |
                       v
              concrete cat rig adapter
                       |
                       v
                Gesture evaluation
                       |
                       v
                Physics / world
```

The fixture's role strings are provisional examples, not canonical ontology tokens. The authoritative ontology should own semantic identity and relationship resolution. The armature resource owner should define structural topology and transform semantics. Gestures should remain a reusable evaluator and procedure domain, not become an ontology database or asset-rig loader.

## Next proof steps

1. Confirm the ontology's actual address and relationship contracts before defining a resolver adapter.
2. Agree on the versioned armature resource schema with its owning domain.
3. Resolve a feline concept to that profile and a small catalog containing `cat.stalk`.
4. Validate one concrete cat rig against the declared roles.
5. Add a second species fixture (for example, a dog) to prove that shared quadruped concepts do not erase species-specific structure or movement.
6. Add expressive feline procedures such as `cat.tail-signal` and `cat.groom` only after the profile contract is established.

---

**Design rule:** ontology selects what kind of entity this is and which articulated profile and procedures belong with it; the armature describes how it articulates; Gestures evaluates a procedure; Physics determines what actually happens.

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
