# Provider and actor-choice executable reference fixture

> **Status: executable architecture prototype, not a public API contract or world simulator.**

Run from the repository root:

```sh
dotnet run --project tools/provider-actor-choice-demo/provider-actor-choice-demo.csproj --configuration Release -- --output docs/assets/provider-actor-choice.svg
```

The tool emits an SVG and a neighboring JSON trace from the same in-memory records. The SVG is a view of the trace, not an independently authored animation. CI builds and executes this fixture, validates the generated SVG as XML, and uploads the outputs as workflow artifacts.

## What the fixture demonstrates

- One versioned environment provider offers capabilities at a pet-store window.
- Each actor evaluates those offers against its own goals, cash, store access, and ownership facts.
- Hard filters produce human-readable rejection reasons before scoring.
- Stable seed-derived tie-breaking avoids process-randomized string hashes.
- The companion-seeking actor selects the puppy purchase; the work-focused actor does not.
- A small nested procedure trace separates attempted steps from the declared domain outcome.

## Deliberate limits

The provider and actor records are private to this tool. They are not the proposed public Gestures schema, a Forge compiler, an AI planner, a physics engine, or a persistent world model. The nested procedure/outcome is a deterministic fixture, not proof that a real store, payment system, collision model, or ownership database executed. The next step is to replace fixture-local rules with validated Forge-baked artifacts and connect outcomes to authoritative domain records—without moving actor goals into provider definitions.

The generated JSON is intended to make each candidate, exclusion reason, score, selected capability, and outcome inspectable by a future diegetic Forge UI.

## Package-level interaction boundary test

`tests/TheSingularityWorkshop.Gestures.Tests/ProviderToSurpsInteractionFixtureTests.cs` composes the actual `GestureOfferSelector` and `GesturePlayback` contracts in one deterministic test. A provider offers ladder and sword procedures; actor capabilities and available semantic anchors filter the candidates; the consumer explicitly supplies the selected intent; and the host advances only that procedure with a supplied one-second delta. The test then keeps a fixture-local observation record separate from a fixture-local physical-outcome record. The outcome is deliberately marked non-authoritative: only the host/domain/Physics integration can supply authoritative success. These local records demonstrate the boundary and are not new public package types.

This is a package-level contract fixture, not yet a live SURPS runtime integration, anchor-transform resolver, or Physics-backed contact test. It proves that the existing selector and playback primitives compose without assigning intent, timekeeping, or physical truth to Gestures.
