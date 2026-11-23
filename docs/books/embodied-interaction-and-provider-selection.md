![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Embodied Interaction and Provider Selection

**Book ID:** gestures.domain.embodied-interaction-and-provider-selection  
Edition: 0.1.0  
Category: Domain · LOD: 2  
**Status:** Draft — ownership pending independent SURPS review

## 1. Purpose and scope

An embodied interaction is often offered by the thing being interacted with: a ladder offers climbing procedures, a hatch offers opening/entry procedures, a sword offers use procedures, and a vehicle offers boarding procedures. The actor does not need to hard-code every possible object. The provider describes the procedure and requirements; the actor and consuming experience supply identity, capabilities, training, condition, intent, and available time.

Gestures owns the representation and evaluation of embodied procedures and provider-facing filtering. SURPS owns roleplaying semantics, intent, CORE action rules, initiative/time policy, and composition. The provider/experience owns its offer and environmental anchors. Physics owns physical laws and authoritative consequences.

## 2. Tabletop-first provider procedure

1. The referee or item description declares what the object offers: approach location/orientation, initial pose, required contact points, and available procedure variants.
2. The actor sheet supplies relevant anatomy/capabilities, training, condition, equipment, and current state.
3. The group compares requirements. Missing capability does not automatically mean no attempt is possible; the provider may publish a clumsy, risky, or failure-prone alternative.
4. The player declares intent. SURPS/CORE determines decision timing, action cost, initiative, and uncertainty resolution.
5. The procedure is played through preparation, commitment, execution, and recovery phases where defined. Interruptions, deadlines, injury, and late/no response are recorded explicitly.
6. The referee resolves physical contact, leverage, collision, injury, and success using Physics and world rules.
7. The table records observations/events and updates actor condition/context. Those changes affect the next offer-filtering pass.

The digital equivalent is typed provider data, actor context, deterministic filtering, an explicitly selected procedure, host-supplied time progression, and returned observations/outcomes.

## 3. Provider offers and actor-relative filtering

The implemented GestureOfferSelector compares required actor capability names and semantic anchor names against caller-supplied context. It returns eligible offers and rejected offers with exact missing requirements. Results are ordered by stable Gesture ID; provider enumeration order is not a selection signal. Duplicate IDs are rejected.

It does not infer intent, choose on the actor's behalf, rank by desirability, resolve anchor transforms, validate world geometry, calculate forces/collisions/injury, or guarantee physical success.

Implementation: [GestureOfferSelector](../../src/TheSingularityWorkshop.Gestures/GestureOfferSelector.cs), [GestureTargetDiscovery](../../src/TheSingularityWorkshop.Gestures/GestureTargetDiscovery.cs), [Offer Selection contract](../GESTURE_OFFER_SELECTION.md), [Target Discovery](../GESTURE_TARGET_DISCOVERY.md).

## 4. Concrete examples

### Ladder

A ladder provider can offer a trained climb with controlled approach/grip, an untrained attempt with less controlled timing, a constrained alternative for limited mobility, or an explicit rejection when required capabilities/anchors are absent and no alternate is offered. The ladder supplies anchors and procedure variants. The actor supplies capabilities, training, condition, and intent. The host binds actual rung positions. Physics resolves grip, load, slip, fall, and injury. SURPS resolves action meaning, time, risk, and consequences.

### Sword

A sword provider may offer trained and untrained handling procedures. Training can select controlled guards, parries, cuts, thrusts, recoveries, or disarms only where explicitly defined. An untrained actor may still attempt a clumsy action if offered. A duel is not scripted: reach, timing, pose, geometry, condition, and physical outcomes remain authoritative inputs.

### Vehicle boarding

A vehicle can provide a ramp/hatch procedure with anchors and starting orientation. Gestures evaluates the procedure; the vehicle/host resolves whether anchors remain valid; Physics resolves contact and physical feasibility. A generated pose is not proof the actor entered the vehicle.

## 5. Temporal contract and event return

Provider and host should make temporal phases explicit: decision window, preparation, commitment, execution, recovery, reaction, deadline, interruption, and late/no response. Current playback advances by caller-supplied delta time; it is not an initiative or scheduler engine.

GestureObservationEvent provides a typed record for an observed Gesture event. It is not a general event bus, perception engine, or social/emotion model. A host can translate verified observations and authoritative outcomes into SURPS state/context updates. Injury or condition changes must be explicit state evidence, not inferred from a pose sample.

References: [GestureObservationEvent](../../src/TheSingularityWorkshop.Gestures/GestureObservationEvent.cs), [Observation tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureObservationEventTests.cs), [Evaluation Lifecycle](../GESTURE_EVALUATION_LIFECYCLE.md), [General Theory](../GENERAL_THEORY_OF_PROCEDURAL_GESTURE.md), [Physics Motion Handoff](../PHYSICS_MOTION_HANDOFF.md).

## 6. Current versus proposed capability

| Capability | Status |
|---|---|
| Provider offer with actor-capability and semantic-anchor requirements | Implemented and test-backed |
| Deterministic eligible/rejected result with missing-requirement reasons | Implemented and test-backed |
| Actor intent and final choice policy | Consumer/SURPS responsibility; not implemented by selector |
| Provider-defined trained/untrained alternatives | Separate offers can be authored; no universal training system |
| Universal anatomy, injury, and competence model | Not implemented; ontology/host/Physics contracts unresolved |
| Time windows, initiative, and deadline arbitration | SURPS/host responsibility; no universal scheduler here |
| Host-supplied delta-time playback | Implemented |
| General interruption/recovery policy and event bus | Not implemented as universal orchestration |
| Contact/force/injury resolution | Physics/host responsibility |
| Gesture observation record | Implemented; no universal perception/social engine |

## 7. Verification exercises

1. Compare an actor with required ladder capabilities against one missing a requirement; inspect rejection details.
2. Supply trained and untrained sword offers and verify filtering does not choose intent or guarantee success.
3. Change actor context after an authoritative injury event and rerun filtering; verify injury is not inferred from a pose.
4. Interrupt playback in the host and retain observed state and physical outcome separately.
5. Remove an environment-provided anchor and verify explicit rejection.

Tests: [Offer selector tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureOfferSelectorTests.cs), [Target discovery tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureTargetDiscoveryTests.cs), [Observation tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureObservationEventTests.cs), [Playback tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GesturePlaybackTests.cs).

---

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
