# Renderer pose handoff — proposal for review

**Status: proposal only.** This is a boundary sketch based on the current Gestures and Renderer source, not an agreed or implemented integration contract.

## Why keep a boundary?

Gestures currently evaluates a definition at a caller-supplied time and returns an `EvaluatedPose`. Renderer currently owns observer-relative representation and render-frame command construction. Neither project should need to become the other's implementation detail just to pass an evaluated pose toward presentation.

The intended direction is:

```text
GestureDefinition + time + variation inputs
                  |
                  v
           GestureEvaluator
                  |
                  v
             EvaluatedPose
                  |
                  v
        consumer/rig adapter
                  |
                  v
    resolved representation / RenderFrame
```

## Current data available

The current `EvaluatedPose` exposes:

- `NormalizedProgress` — clamped progress in [0, 1].
- `IsComplete` — whether the evaluator reached the requested duration.
- `LocalTransforms` — a name-keyed map of `PoseTransform` values.
- `RootOffset` — a deterministic presentation/root-projection offset.

The pose is presentation data, not authoritative world state. In particular, `IsComplete` means the time-based Gesture evaluation reached its end; it does not prove that a hand contacted a handhold, a foot landed, a vehicle was boarded, or a physical action succeeded.


## What the current Renderer API actually accepts

A source inspection of `TheSingularityWorkshop.Renderer` on its current `master` branch establishes an important limit on the proposed first slice:

- `RenderFrame` contains a `RenderView` and a snapshot of `RenderDrawCommand` values.
- Each `RenderDrawCommand` requires geometry and material resource IDs, an already-resolved `Matrix4x4 WorldTransform`, index parameters, and a `RendererRepresentation` classification.
- The current public frame contract does **not** expose a rig, named skeleton nodes, a skeletal pose buffer, or an input slot for `EvaluatedPose`.

Therefore, a Gesture pose cannot currently be handed directly to `RenderFrame`. A consumer must first map the name-keyed local transforms onto an actual rig/entity representation, compose them according to that rig's hierarchy and coordinate convention, and decide which resolved transforms are used to construct draw commands. A single `RenderDrawCommand.WorldTransform` is a draw's world transform; it is not, by itself, a multi-node animated pose contract.

This is not a reason to add a dependency from Renderer to Gestures. It means the first integration slice needs a concrete host/rig fixture and a small explicit adapter contract. If no current Renderer input represents the resolved animated geometry, that gap should be recorded as a Renderer-side design decision rather than disguised by converting a pose into arbitrary draw commands.

## Proposed responsibilities

### Gestures

- Evaluate authored normalized keyframes and return the current pose values.
- Preserve deterministic behavior for identical definitions and inputs within the documented numeric contract.
- Keep the core free of renderer, graphics API, and engine-specific types.

### Consumer / rig adapter

- Resolve transform names against the rig or entity representation actually supplied by the host.
- Apply any established parent hierarchy and coordinate-space conversion.
- Decide how `RootOffset` participates in root presentation; do not silently reinterpret it as authoritative world displacement.
- Handle missing or unmapped transform names explicitly rather than inventing a universal humanoid rig.
- Pass resolved representation data to Renderer using the Renderer contract, not by making Renderer depend on Gesture definitions.

### Renderer

- Consume the representation data needed for observation, detail selection, and render-frame construction.
- Own observer-relative representation policy and presentation preparation.
- Not infer physical contact, support, collision outcomes, or domain success from a rendered/evaluated pose.

### Physics and domain owner

- Determine actual collision, contact, load-bearing, occupancy, failure, and other physical/domain outcomes.
- Provide evidence to systems that need those outcomes; neither pose interpolation nor rendered appearance substitutes for that evidence.

## Contract questions to settle before implementation

1. **Identity:** Is the adapter keyed by Gesture transform names, stable rig-node IDs, or a mapping from one to the other?
2. **Space:** Which coordinate convention and parent-composition rules are authoritative? The current pose is local-transform data; this proposal does not claim it already contains resolved world transforms.
3. **Root offset:** Is it applied before or after root transform composition, and how is it separated from root motion?
4. **Missing nodes:** Should the adapter reject the pose, skip unmapped entries with diagnostics, or use a caller-specified fallback?
5. **Sampling:** Who chooses the evaluation clock and sampling cadence as Event Horizon/update policy changes?
6. **Numerics:** What tolerances should be used for transform comparison after coordinate conversion? The current translation golden vectors are only an initial CPU fixture, not CPU/GPU parity evidence.
7. **Batching:** What compact representation is justified by measured population workloads? Do not choose texture packing, buffers, or another layout before measuring conversion, transfer, and dispatch costs.

## Suggested first integration slice

Start with one test-only adapter fixture, not a package dependency:

1. Evaluate a tiny two-keyframe Gesture at fixed progress values.
2. Resolve its named local transforms through an explicit test rig map.
3. Verify the adapter's coordinate conversion and parent composition with golden outputs.
4. Confirm that unknown names and invalid values follow the chosen policy.
5. Produce or populate the smallest Renderer-side representation input already supported by its current API.
6. Keep physical outcomes outside the fixture and record CPU-side conversion cost before proposing a GPU path.

Only after the fixture demonstrates a stable seam should the two repositories decide whether they need a shared DTO, an adapter package, or no new package at all.
