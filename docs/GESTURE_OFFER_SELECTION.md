# Provider offers and actor-relative selection

## Contract

A provider may expose several reusable Gestures. The actor or consuming activity should not assume that every offered procedure is appropriate for every actor or environment. `GestureOffer` wraps a `GestureDefinition` with two explicit requirement sets:

- **Actor capabilities** — all listed capabilities must be present in the actor context.
- **Semantic anchors** — all listed anchor names must be defined by the Gesture and reported available by the consumer's environment context.

`GestureOfferSelector.Evaluate` applies those requirements and returns both eligible offers and rejected offers with the exact missing requirements. Both lists are ordered by stable Gesture ID using ordinal comparison; provider enumeration order is not a selection signal. Duplicate IDs are rejected rather than silently choosing one.

## Example

```csharp
var offer = new GestureOffer(
    StarportGestureCatalog.CreateBoardingRampGesture(),
    requiredActorCapabilities: ["climb", "balance"],
    requiredAnchorNames: ["hatch-threshold"]);

var selection = GestureOfferSelector.Evaluate(
    [offer],
    actorCapabilities: ["climb", "balance", "carry"],
    availableAnchorNames: ["hatch-threshold"]);

var eligible = selection.Eligible; // The consumer still chooses what to do.
```

The example uses an anchor name from the boarding-ramp catalog only if that exact name is available in the provider's definition; consumers should inspect the definition and use its actual semantic anchor identifiers.

## Ownership boundary

This is eligibility filtering, not autonomous behavior selection. The consumer owns intent, priorities, current task, actor state, and final choice. The provider owns the procedure and its declared requirements. The consumer/world integration resolves semantic anchors and reports which are available. The host and Physics remain authoritative for reachability, contact, obstruction, force, and achieved outcomes.

The selector does not infer competence from a name, mutate actor state, run a clock, start playback, or claim a gesture succeeded. Its deterministic output is a stable candidate set for a higher-level policy.

## Evidence and limitations

The unit tests cover missing capabilities, missing anchors, explicit rejection reasons, stable ordering under reversed provider enumeration, duplicate IDs, unknown required anchors, and invalid names. This primitive does not yet provide weighted ranking, contextual scoring, capability inheritance, version negotiation, anchor transform resolution, or automatic selection.

The provider-to-actor filtering step is now executable and testable. It is not a general choreography graph; dependency gates and authoritative outcome checks remain the responsibility of a task/activity coordinator. The starport departure scenario has a separate tested task graph and explicit departure evidence contract.
