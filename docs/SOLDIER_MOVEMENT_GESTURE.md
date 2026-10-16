# Soldier Movement Gesture — rig-independent example

This example turns the existing GestureDefinition and CPU evaluator into a concrete, test-backed movement procedure without introducing a universal humanoid skeleton.

## What the fixture represents

`soldier.tactical-advance` has three authored normalized poses: start, mid-stride, and end. It names only the transforms this example needs:

- `root` and `pelvis`
- `left_foot` and `right_foot`
- `left_hand` and `right_hand`

The example includes `weapon-support` and `weapon-control` semantic anchors attached to the hands. These names are sample vocabulary, not mandatory package-wide bone or equipment standards. A provider can define a different rig and resolve the named transforms and anchors through its own adapter.

The fixture lives in `tests/TheSingularityWorkshop.Gestures.Tests/SoldierMovementGestureTests.cs` so the example is executable and its naming, anchors, and deterministic evaluation remain covered by the normal test suite.

## What evaluation means here

The reference evaluator interpolates the authored local transforms at normalized progress. It does not integrate root locomotion, enforce foot planting, solve balance, detect obstacles, resolve weapon collisions, or validate contact. The sample's coordinates are illustrative procedure data, not motion-capture output or a validated tactical movement model.

A consuming simulation owns the next steps:

1. Map the sample transform identifiers onto the consumer's rig.
2. Evaluate the procedure using its own clock/progression policy.
3. Treat evaluated pose as desired presentation/kinematic input, not authoritative movement.
4. Use the simulation's own locomotion, collision, contact, and gameplay rules to determine what actually happened.
5. If an interaction depends on an anchor, resolve it against the provider-owned object and let the owning simulation report whether contact or support was achieved.

## Try the fixture

From the repository root with the .NET 8 SDK:

```bash
dotnet test TheSingularityWorkshop.Gestures.slnx --configuration Release --filter FullyQualifiedName~SoldierMovementGestureTests
```

This is a small reference fixture intended to make the API tangible. It is not a complete soldier controller, a tactical AI, or a physics simulation.
