# The Singularity Workshop — Gestures

[![Build](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/actions/workflows/build.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**Procedural physical action, represented as data and evaluated deterministically.**

TheSingularityWorkshop.Gestures is the reusable model and CPU reference evaluator for physical procedures: pose keyframes, interpolation, progression, stable individual variation, and evaluated pose output. It lets a capability describe how an interaction physically happens without requiring every consumer to author a conventional baked animation clip.

> **Status: early alpha foundation.** This first milestone establishes a small, testable CPU contract. It is not a physics solver, authoring tool, complete animation system, or GPU evaluator. This repository does not publish to NuGet.

## Why Gestures?

A Gesture is more than a list of poses. It describes a physical procedure whose progress can be evaluated at a given time, with optional deterministic variation bounded around ideal motion.

Examples include a soldier entering a vehicle cockpit using anchors supplied by the vehicle, climbing through a hatch, operating a mechanism, or coordinated marching where individuals share an ideal procedure without moving as perfect copies.

The capability that understands an interaction provides the procedure. Entity behavior and FSM determine whether and when to perform it. Renderer decides which fidelity is useful to an observer. Visualize consumes evaluated data and performs GPU-facing presentation; it does not become authoritative over world state.

## Architecture

```text
Capability / Gesture Provider
           │ defines physical procedure
           ▼
    GestureDefinition ── anchors + keyframes
           │
           ▼
 CPU reference evaluator ── stable identity + bounded variation
           │
           ▼
     EvaluatedPose ───────── disposable presentation input
           │
           ▼
 Renderer selects fidelity → Visualize executes GPU presentation
```

- **Gestures** owns the reusable physical-procedure model and reference evaluation.
- **FSM_API** may govern progression, interruption, and completion in a consumer; Gestures does not require it as a dependency.
- **Renderer** chooses observer-relative representation and fidelity.
- **Visualize** performs GPU-facing presentation.
- **raWWar** supplies domain capabilities and authoritative game state.

The package intentionally has no dependency on WPF, Unity, a renderer, a GPU API, or game-specific types.

## Build and test

Requires the .NET 8 SDK.

```bash
dotnet restore TheSingularityWorkshop.Gestures.slnx
dotnet build TheSingularityWorkshop.Gestures.slnx --configuration Release --no-restore
dotnet test TheSingularityWorkshop.Gestures.slnx --configuration Release --no-build
dotnet pack src/TheSingularityWorkshop.Gestures/TheSingularityWorkshop.Gestures.csproj --configuration Release --no-build
```

## Design guarantees and boundaries

- Keyframe times are normalized to [0, 1] and strictly increasing.
- Evaluation is deterministic for the same definition, elapsed time, and variation seed.
- Individualization is bounded and derived from stable identity; it is not per-frame random noise.
- Evaluated pose data is an output projection, not authoritative movement or collision state.
- The CPU evaluator is the reference implementation against which later GPU evaluators can be tested.
- The initial transform representation uses System.Numerics, not host-engine types.

See [Architecture](docs/ARCHITECTURE.md), [Data Model](docs/DATA_MODEL.md), [Determinism](docs/DETERMINISM.md), and [Roadmap](docs/ROADMAP.md).

## Release discipline

CI builds, tests, and packs the library as an artifact. NuGet publication is deliberately disabled. Publishing requires explicit maintainer approval, a reviewed version increment, API and dependency review, and successful validation of the actual package artifact.

---

*The Singularity Workshop — edify, not mystify.*
