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

The current `MicroBundleDescriptor` contract has a stable bundle ID, a bundle version, dependencies expressed as bundle IDs, and provider IDs. Arbitration happens after the host has loaded the participating capabilities; the host owns the arbitration loop, and a capability reports whether it actually changed something that requires another round.

That is a useful composition seam, but the current descriptor does **not** itself encode a rich typed semantic contract, a provider contract version range, units, optional measurement semantics, or timebase compatibility. Do not imply those are already enforced by the descriptor.

For the first integration, use stable bundle/provider identities backed by versioned contract documentation and focused adapter validation. If machine-readable semantic compatibility later proves necessary, propose the smallest compatible extension to the existing MicroBundle descriptor/provider model and its tests. Do not invent a parallel manifest format or bake repository/package names into the semantic contract.

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

## Manifests, bundle configuration, and compatibility arbitration

The manifest is the assembly declaration for an experience: it names the MicroBundles to include and pins the requested versions. Each bundle's own configuration supplies the values that configure that bundle. Arbitration is the higher-level process that resolves and composes those declared capabilities and configurations for the current experience. These are related artifacts, but they are not interchangeable:

- **Manifest:** which bundles and versions are requested for this composition.
- **MicroBundle configuration:** how an individual selected bundle is configured.
- **Provider/capability contracts:** what each selected bundle actually offers, including requirements and limits that are explicitly declared.
- **Arbitration/composition:** whether the selected offers can be connected together for this use case, and what to do when they cannot.
- **Runtime/host:** whether a chosen operation can be attempted now, and what its authoritative outcome is.

A manifest that lists two bundles is not proof that their operations are mutually compatible. Nor should a bundle silently broaden its claims merely because another bundle uses a similarly named capability. Presence, semantic compatibility, current feasibility, and successful outcome are four different checks.

### Example: two bundles both advertise docking

Imagine a flight-simulation experience with a tanker provider and a space-station provider. Both may advertise a general "docking" capability, but the higher-level experience must not infer that a tanker can dock with that station simply because the names match.

The arbitration path should evaluate the contracts the providers actually declare: compatible operation/procedure identifiers and versions, target/interface class, required anchors and geometry, scale or unit conventions where relevant, environmental constraints, and any required supporting capabilities. This list is an illustrative compatibility checklist, not a claim that the current MicroBundleDescriptor already encodes these fields. Missing or ambiguous compatibility evidence should yield an explicit incompatibility or unresolved result—not a guessed connection.

The experience may then choose another station, load an adapter or capability MicroBundle, change configuration, or decline the operation. If the operation is selected, the simulation/Physics and host still determine approach feasibility, contact, and whether docking actually succeeded. Gestures can describe and evaluate the vehicle's intended procedure or a representation of the ideal operation versus the player's current control; it does not grant permission, force a connection, or certify the physical result.

### General rule

This is not specific to vehicles. The same rule applies to a toy set in which some pieces interlock and others do not, a tool and a workpiece, a character and a vehicle, or two distributed services. **The higher-level experience owns the intended composition; arbitration decides which declared capabilities/configurations can be composed; each bundle remains authoritative about its own contract; the relevant runtime remains authoritative about execution and outcome.**

Keep this boundary explicit while the contracts evolve. Do not add an independent manifest schema to Gestures, duplicate the Workshop's composition model, or move arbitration into GestureEvaluator. If the current descriptor/provider model cannot express a compatibility requirement that a real vertical slice needs, document that gap and propose a minimal extension at the MicroBundle contract boundary first.

For the current composition contract, see [FSM_COS Runtime Manifest](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/RUNTIME_MANIFEST.md) and [FSM_COS Arbitration and Convergence](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/ARBITRATION.md). Those documents are authoritative for what the kernel currently guarantees; this Gestures document describes the domain integration requirements, not a second implementation of that contract.

