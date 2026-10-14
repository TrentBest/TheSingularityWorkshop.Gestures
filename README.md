![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# ✳️ 00 The Singularity Workshop — Gestures

[![Build](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/actions/workflows/build.yml/badge.svg?branch=development)](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/actions/workflows/build.yml)
[![Coverage](https://img.shields.io/codecov/c/github/TrentBest/TheSingularityWorkshop.Gestures/development?style=flat-square&logo=codecov)](https://codecov.io/gh/TrentBest/TheSingularityWorkshop.Gestures/branch/development)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

![](docs/assets/dividers/star-gold.svg)

**Reusable animation principles, expressed as procedural motion data and evaluated deterministically.**

TheSingularityWorkshop.Gestures is a reusable motion model and CPU reference evaluator: keyframes, interpolation, progression, stable individual variation, and evaluated transform output. A gesture describes how targets move over time; it does not need to know whether a target is a soldier, tank, spaceship, tool, or something else. Target-specific behavior and physical simulation remain with their owning systems. Compatible evaluation work may later be batched for GPU execution; the current evaluator is CPU-only.

> **Status: early alpha foundation.** This first milestone establishes a small, testable CPU contract. It is not a physics solver, authoring tool, complete animation system, or GPU evaluator. This repository does not publish to NuGet.

## 🟦 01 What

![](docs/assets/dividers/blue.svg)

A Gesture is reusable motion data, not an object-specific animation catalogue. It describes movement over time and can be applied to different targets through their transform contracts. The target performs its own domain behavior; Gestures supplies the motion principles. Optional deterministic variation is bounded around ideal motion.

Examples include a soldier entering a vehicle cockpit using anchors supplied by the vehicle, climbing through a hatch, operating a mechanism, or coordinated marching where individuals share an ideal procedure without moving as perfect copies.

## 🟣 02 Why

![](docs/assets/dividers/violet.svg)

Baked animation clips can be useful, but they often encode a finished performance rather than a reusable procedure that can adapt to identity, context, and a desired level of variation. Gestures aims to make the procedure and its transition rules reusable data.

The ideal Gesture establishes the intended movement. Each target's stable seed and experience/context profile may influence bounded parameters of the motion itself. Green soldiers can have broader variation ranges; veterans can have tighter ranges while remaining individually distinct. The goal is coherent variation, not per-frame random noise.

## 🔺 03 How

![](docs/assets/dividers/triangle-red.svg)

```text
Capability / Gesture Provider
           | defines physical procedure
           v
    GestureDefinition -- anchors + keyframes
           |
           v
 CPU reference evaluator -- stable identity + bounded variation
           |
           v
     EvaluatedPose -------- disposable presentation input
           |
           v
 Renderer selects fidelity -> Visualize executes GPU presentation
```

Each part has a clear owner:

- **Gestures** owns Gesture data, anchors, deterministic reference evaluation, and evaluated pose output.
- **FSM_API** may govern progression, interruption, and completion in a consumer; Gestures does not require it as a dependency.
- **Gesture Provider / MicroBundle** supplies domain-specific physical procedure and interaction anchors.
- **Renderer** chooses observer-relative representation and fidelity.
- **Visualize** performs GPU-facing presentation.
- **raWWar** supplies domain capabilities and authoritative game state.

The package intentionally has no dependency on WPF, Unity, Renderer, a GPU API, or game-specific types. The initial evaluator linearly interpolates translation and scale and uses quaternion spherical interpolation for rotation. This is a reference baseline, not a final high-fidelity motion solver. GPU batching of compatible evaluation work is a future optimization; this package does not yet claim a GPU evaluator.

## 🟢 04 See it in a minute

![](docs/assets/dividers/green.svg)

![Three C#-generated named-transform trajectories from GestureEvaluator, with lifecycle coordinated by FSM_API](docs/assets/gesture-motion.svg)

![Animated position, rotation, and scale channels sampled from the same C# evaluator output](docs/assets/gesture-channels.svg)

![Shared root motion alongside an anchored, joint-limited articulated-tool illustration](docs/assets/gesture-target-agnostic.svg)

The third scene contrasts a shared root transform with an anchored articulated tool. The tool base stays fixed while two illustrative links rotate around joints within clamped angle ranges derived from evaluated rotation/scale channels. Those derived angles are a teaching illustration—not joint outputs from GestureEvaluator, a solved kinematic chain, or a physics simulation. Real mounted-tool motion must obey the owning system's joint geometry, limits, and constraints.

The scenes are generated by [the C# demonstration](tools/living-demo/Program.cs), not drawn as independent animations. **FSM_API** moves the demonstration through `Ready → Running → Complete`; `GestureEvaluator` evaluates one `GestureDefinition` at 49 elapsed-time samples. The motion and channel scenes serialize the returned `root`, `tool`, and `load` transforms, including position, quaternion rotation, and scale. The target-comparison scene reuses the root channels for the humanoid and tracked-vehicle glyphs; its anchored tool is a separate illustrative joint model derived from root rotation/scale, not actual joint output. The named transforms are independently interpolated and are not composed into a rig hierarchy. The browser plays serialized SVG values; it does not execute C# or run FSM_API at display time. [See how to regenerate the scenes](tools/living-demo/README.md). The channel scene plots the evaluated root transform as X, Y, rotation, and scale. The SVGs and their limits are described in [the living-motion guide](docs/LIVING_MOTION_DOCUMENTATION.md); CI validation for the current head is still pending.

You need the **.NET 8 SDK**. From the repository root:

```bash
dotnet restore TheSingularityWorkshop.Gestures.slnx
dotnet build TheSingularityWorkshop.Gestures.slnx --configuration Release --no-restore
dotnet test TheSingularityWorkshop.Gestures.slnx --configuration Release --no-build
dotnet pack src/TheSingularityWorkshop.Gestures/TheSingularityWorkshop.Gestures.csproj --configuration Release --no-build
```

This is a source-repository verification path, not a claim that the package has been published or that the early evaluator is a complete motion solver. Evaluation is deterministic for the same definition, elapsed time, and variation seed; the tests are the place to verify the guarantees of the current implementation.

## 🟪 05 Available documentation and theory

![](docs/assets/dividers/purple.svg)

- [Architecture](docs/ARCHITECTURE.md) — understand ownership boundaries and how evaluated poses reach the renderer.
- [Data Model](docs/DATA_MODEL.md) — learn the initial keyframe, transform, anchor, and evaluated-output contracts.
- [Determinism](docs/DETERMINISM.md) — understand what stable evaluation means and which inputs determine the result.
- [Motion Equation Exploration](docs/MOTION_EQUATION_EXPLORATION.md) — explore the candidate force, inertia, spring, damping, easing, and training-variance model. This is exploratory mathematics, not a frozen package contract.
- [Motion Model Experiment Plan](docs/MOTION_MODEL_EXPERIMENT_PLAN.md) — compare candidate models, see analytic damping-response curves, derive interpretable parameter ranges, and define measurable experiments before choosing an implementation.
- [Living Motion Documentation](docs/LIVING_MOTION_DOCUMENTATION.md) — explore the evidence standard, current generated scenes, and the next visual milestones.
- [Roadmap](docs/ROADMAP.md) — see what is implemented, proposed, and still open.

## 🩵 Design guarantees and boundaries

![](docs/assets/dividers/teal.svg)

- Keyframe times are normalized to [0, 1] and strictly increasing.
- Evaluation is deterministic for the same definition, elapsed time, and variation seed.
- Individualization is bounded and derived from stable identity; it is not per-frame random noise.
- Evaluated pose data is an output projection, not authoritative movement or collision state.
- The CPU evaluator is the reference implementation against which later GPU evaluators can be tested.
- The initial transform representation uses System.Numerics, not host-engine types.

## 🟣 Release discipline

![](docs/assets/dividers/violet.svg)

CI builds, tests, and packs the library as an artifact. NuGet publication is deliberately disabled. Publishing requires explicit maintainer approval, a reviewed version increment, API and dependency review, and successful validation of the actual package artifact.

---

<p align="center"><em>The Singularity Workshop — edify, not mystify.</em></p>
