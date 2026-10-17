# Physics and Gestures: MicroBundle Arbitration Without Package Coupling

**Status:** integration contract and proposed composition pattern. This document does not claim that an executable Physics↔Gestures MicroBundle adapter or FSM_COS arbitration is implemented.

## The design principle

Physics and Gestures should be interwoven at the experience level, not coupled by project references.

- **Physics** owns physical models, units, geometry, state evolution, contact evidence, forces/impulses where calculated, and the validity limits of those calculations.
- **Gestures** owns reusable procedures, deterministic pose evaluation, activity/role descriptions, and recommendations for the next motion intent.
- **An integration MicroBundle** translates between the capabilities those packages already expose. It is selected and composed by the Workshop's arbitration/composition boundary.
- **The host / FSM_COS** supplies world identity, logical clocks, lifecycle, and composition. FSM_COS is not a new dependency of either core package.
- **Renderer / Visualize** presents the resulting state but does not decide physical truth or gesture success.

Neither core package should reference the other NuGet package just to exchange information. Each stays usable by consumers that never install Workshop MicroBundles.

## Why the existing atomic constructs fit

Physics already has a bounded, immutable `Physics.Collision.ContactObservation` record. It can report stable contact/body IDs, world-space point in metres, unit contact normal, relative velocity in metres per second, signed separation, active state, logical step/time, and optional normal force or impulse when the producer actually calculated them. Its detector/resolver contracts explicitly do not imply a general friction solver or proof of support.

Gestures already has a separate, data-only `TheSingularityWorkshop.Gestures.ContactObservation` assessment. It expresses contact presence and an externally assessed load outcome (supported, slipping, unsupported, or unknown), plus optional force magnitudes. `ContactRecoveryPolicy` maps that assessment to a next-intent recommendation; it does not calculate forces, friction, balance, or coordinates.

These similarly named types are **not interchangeable and should not be unified by copying one into the other**. They sit at different levels:

1. Physics contact model reports physical evidence.
2. A compatible assessment capability interprets that evidence with the relevant body, material, load, and model context.
3. An adapter maps the assessment into Gestures' narrower recovery input.
4. Gestures recommends an intent; its provider/actor chooses a procedure and evaluates it.
5. Physics/host checks the resulting physical attempt and emits new evidence.

A contact normal and impulse alone cannot establish that a hand is gripping, a foot is supporting weight, a boot is slipping, or a body is balanced. Those conclusions require an appropriate model and context. Unknown evidence must remain unknown rather than being silently upgraded to success.

## MicroBundle arbitration seam

The intended composition is a set of independently acquired capabilities, not a direct dependency chain:

`Physics contact provider` → `contact/load assessment provider (when required)` → `Physics-to-Gestures adapter` → `Gesture procedure provider`

The arrows mean **declared capability contracts selected and connected by the host/arbitrator**, not C# project references. This is a conceptual flow, not an existing manifest schema or a claim that an arbitrator currently wires these exact providers.

At composition time, a MicroBundle may declare:
- the stable capability contract and version it provides;
- the input/output contract versions it consumes;
- required model assumptions, units, and optional fields;
- supported body/anchor identity conventions and logical-time semantics;
- capability limitations and required companion capabilities;
- optionality and an explicit unavailable/incompatible result.

The existing MicroBundle descriptor/artifact schemas must be inspected and reused before encoding these declarations in a real manifest. Do not invent a parallel bundle format or bake repository/package names into the semantic contract.

If no compatible assessment or adapter is available, composition should report the missing capability and allow a caller-selected fallback, such as `AwaitObservation`. It must not fabricate a supported-contact result.

## Keep event families distinct

There are at least two useful, related contracts, but they answer different questions:

- **Physics contact evidence:** what a physical model reported about two bodies at a logical step.
- **Gesture observation event:** what occurrence the host reports for independent observers to interpret, such as a performance or group action.

A physical contact may cause a domain event; a gesture spectacle may cause an observer to respond; a response may lead to a physical contact. An adapter can link these with stable IDs and causal provenance, but must not pretend all events share one universal schema or clock. Convert time only through an explicit host-provided mapping. Never use render-frame count as the physical clock.

## Worked example: a boot slips on wet stone

1. A locomotion Gesture evaluates a foot-placement procedure at the declared time.
2. Physics reports contact geometry and whatever loads its current model actually calculates.
3. A selected assessment MicroBundle combines the observation with the supported surface/load model and returns `Slipping` only if its declared criterion is satisfied.
4. The adapter creates Gestures' `ContactObservation`; it does not copy uncomputed quantities or infer support from contact alone.
5. `ContactRecoveryPolicy` recommends `SeekAlternativeContact`.
6. The actor/provider selects an available recovery procedure. Gesture evaluation describes the requested pose/motion; it does not prove that the foot moved successfully.
7. Physics evaluates the next attempt and supplies fresh evidence. The host records success, continued slip, loss of contact, or unknown state according to the available model.

The same architecture supports a soldier bracing a door, a crew member handling a hose, a dancer losing balance, a child colliding with another player, or a tool failing to grip—without putting those domain policies into Physics or Gestures core.

## Learning without requiring math first

The public Physics package deliberately places reusable mathematical and geometric constructs beside the physical domains that make them meaningful. That lets an author start from an atomic question—contact, a spring, a projectile, heat transfer, or a water surface—inspect the geometry/state and see the physical result, then follow the documented quantities, units, assumptions, and derivation when ready.

Gestures should consume those observable outcomes at the domain boundary, not duplicate their equations. For example, an author can ask for a `brace` or `recover footing` procedure without writing vector math; the selected Physics capability determines whether the intended support/contact was achieved. Living documentation should show both the motion intent and the measured/assessed physical outcome, including failed or unknown cases.

## Validation before claiming integration

The first executable adapter should have focused tests proving:
- stable identity and time mapping from the source observation;
- explicit handling of optional force/impulse values;
- no conversion of a raw contact into `Supported` or `Slipping` without an assessment rule;
- deterministic output for identical declared inputs;
- explicit rejection or fallback for incompatible/missing capabilities;
- clear distinction between scheduled/evaluated motion and achieved physical result.

A living example should be generated from actual Physics source records and Gestures policy outputs, with input IDs, units, timebase, model assumptions, and unmodeled effects visible. Until that exists, diagrams of the cross-package flow must be labelled conceptual.

## Ownership summary

| Question | Owner |
|---|---|
| What physical contact or state was measured? | Physics model |
| Does the evidence satisfy a support/slip criterion? | Selected physical/domain assessment capability |
| Which recovery intent is recommended? | Gestures policy |
| Which procedure is available and selected? | Gesture provider and actor/domain policy |
| What actually happened after the attempt? | Physics / authoritative host |
| Which compatible capabilities are composed? | MicroBundle arbitration / FSM_COS boundary |
| How is the result shown and explained? | Renderer / Visualize / learning experience |

The goal is not to hide the physics. It is to let people understand the physical world through its atomic constructs first, then learn the math because the explanation is attached to the thing they can see and test.
