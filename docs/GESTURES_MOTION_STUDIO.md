# The Gestures Motion Studio: a living Workshop integration

> **Mission:** let a learner author a procedure, watch its evaluated motion, inspect the data, change the observer or environment, and distinguish what was requested from what was physically achieved.

This is the integration blueprint for turning Gestures documentation into an executable motion studio. It deliberately composes existing Workshop technologies instead of cloning their responsibilities inside Gestures.

## The experience we are building

A learner should be able to:

1. Choose an entity profile (humanoid, quadruped, bird, articulated tool, vehicle mechanism, or a neutral transform target).
2. Select a reusable procedure: reach, turn, step, salute, board, climb, operate, recover, or coordinate.
3. Scrub logical time, pause, resume, seek, and compare keyframes with evaluated samples.
4. Inspect position, orientation, scale, semantic channels, local frames, anchors, phase, and stable identity parameters.
5. Change identity/skill parameters and replay the same seed to verify repeatability.
6. Place the same requested procedure in different environments and compare requested pose with Physics-reported achieved state.
7. Change observer distance and attention to see Renderer select an appropriate representation and update cadence.
8. Scale a cohort from one actor to a population, with measured CPU/GPU costs and explicit fidelity transitions.
9. Export a reproducible experiment: inputs, versions, seed, samples, measurements, provenance, and limitations.

The studio must never imply that a presentation artifact is an authoritative simulation result.

## Technology ownership and composition

| Workshop technology | Use in the Motion Studio | Boundary |
|---|---|---|
| **Gestures** | Procedure definitions, time evaluation, deterministic individuality, playback, coordination, and reference output | Does not own rendering, collision authority, or physical success |
| **FSM_API** | Explicit lifecycle and experiment state progression; deterministic stepping where the host chooses to use it | Optional consumer integration; Gestures remains usable without it |
| **Physics** | Units, frames, constraints, contact, environmental resistance, integration, and achieved physical state | Physics is the authority for physical outcomes; Gestures supplies requested motion/procedure |
| **Renderer** | Observer context, Event Horizon selection, representation fidelity, visibility, and computational-detail policy | Chooses what work/representation is appropriate; does not own world truth or draw pixels |
| **Visualize** | Visual scene/frame contracts, direct SVG artifact output, and the exploratory WebGPU population lab | SVG backend is CPU/static/not-presented; WebGPU sample is a separate exploratory presentation path |
| **GUI.Core / adapters** | Semantic controls, inspectors, parameter panels, timeline controls, and accessible teaching UI | GUI is for interface semantics; it is not the general scene renderer |
| **GUI.SVG** | Optional manifestation when the input is a semantic GUI tree and SVG-specific GUI behavior is useful | Do not route ordinary scene geometry through GUI.SVG merely to claim its use |
| **Learning** | Course progression, lessons, exercises, teacher materials, assessment, and learner submissions | Curriculum and learning orchestration, not motion or physics authority |
| **Ontology** | Entity-specific semantic discovery of articulated profiles, Gesture catalogs, and capability vocabulary | Use canonical ontology contracts; do not invent layer names or integer coordinates |
| **FSM_COS / MicroBundles** | Compose the studio experience and domain-specific Gesture providers as capabilities | Keep composition above the individual domain libraries |

### The critical distinction: three different pictures

- **Gesture picture:** what the procedure evaluator returned at a logical time.
- **Physical picture:** what Physics says the entity actually achieved after constraints, contact, and environment.
- **Presentation picture:** what this observer is shown after Renderer selects a representation and Visualize manifests it.

These pictures may differ. The difference is a teaching opportunity, not something to conceal.

## Reference flow

```text
Learner / activity intent
        |
        v
GUI controls + Learning lesson
        |
        v
FSM_API experiment lifecycle (host-owned)
        |
        v
GestureDefinition + entity profile + stable identity + logical time
        |
        v
Gestures CPU reference evaluator
        |
        +---------------------> Gesture trace / channel inspector
        |
        v
Requested motion / trajectory intent
        |
        v
Physics constraints + environment + contact
        |
        v
Achieved state + observations + residuals
        |
        +---------------------> Physics comparison inspector
        |
        v
World / activity state
        |
        v
Renderer observer + Event Horizon policy
        |
        v
Representation work selection
        |
        v
Visualize scene/frame -> SVG proof or GPU presentation
```

The arrows describe intended contracts. A particular sample must label which segments are actually connected and which are mocked, conceptual, or supplied as fixture data.

## Build on proven Workshop capabilities

### 1. Living documentation now

The Gestures repository already generates SVGs from real C# output: evaluator samples, transform channels, seeded variation, provider/actor-choice records, crowd-response schedules, and Extremus population-bake records. Continue this approach:

- generate every evidence graphic from the implementation or a declared experiment fixture;
- put visible provenance inside each SVG;
- publish the exact generated files as CI artifacts;
- make snapshot parity blocking after a snapshot has been reviewed;
- provide a textual table/trace beside each image so the lesson remains accessible without animation.

Visualize owns a direct `SvgVisualizerBackend` for general scene/frame SVG artifacts. Prefer that path when a sample is genuinely a Visualizer scene. Use GUI.SVG only when a semantic GUI tree is the intended input. Do not relabel existing native SVG generators as GUI.SVG.

### 2. Interactive procedure workbench

Create a browser-first lab with a shared deterministic fixture schema:

- **Timeline:** normalized progress, elapsed seconds, keyframe markers, play/pause/step/seek.
- **Viewport:** target profile, local axes, anchors, path, orientation glyphs, and optional articulated overlay.
- **Inspector:** selected entity, stable seed, input channels, evaluated channels, interpolation mode, and output residuals.
- **Comparison:** two definitions or two parameter profiles side by side.
- **Evidence drawer:** exact fixture JSON, algorithm/version IDs, expected invariants, and download/export.

The first version should replay samples generated by the .NET CPU evaluator; the browser must not silently substitute a second implementation. A later live browser evaluator is acceptable only after parity tests compare it against the CPU reference over a shared golden-vector corpus.

### 3. Renderer observer and Event Horizon lab

Use Renderer’s real observer-distance and Event Horizon contracts to demonstrate that the same entity may receive different representational work for different observers. Show the selected horizon, representation, update cadence, and preserved semantic identity.

The existing Visualize RendererAdapter proof already demonstrates a CPU-side path from Renderer selection and FSM-backed representation state through a Visualize frame to GUI.SVG. Reuse that proven adapter boundary where appropriate; do not claim that the complete production bucket/scheduling pipeline is connected until it is.

### 4. Population and GPU lab

The Visualize Soldier Field WebGPU runner is a useful existing stress-test substrate: it has deterministic procedural geometry, semantic part IDs, instanced populations, multiple horizons, and performance reporting through 100,000 instances per horizon workload.

The next Gestures integration should replace or supplement the static soldier pose with real Gesture-derived instance data:

- start with a small fixed vocabulary of motion states and sampled transforms;
- compare one shared procedure with stable per-identity phase/variation;
- preserve stable semantic IDs across horizon/fidelity changes;
- report CPU evaluation, upload bytes, GPU submission, and frame-time percentiles separately;
- test repeatability and scalar/reference parity before increasing population;
- do not call the result a GPU Gesture evaluator unless motion evaluation itself actually runs on the GPU.

A population-rendering stress test is not proof of a physical crowd simulation, navigation solver, or completed action.

### 5. Physics comparison lab

For one procedure, create controlled environment fixtures (for example, unobstructed space, water, and a constrained contact scenario). Keep the procedure inputs fixed, then vary only the Physics-owned environmental or constraint inputs.

Display, as separate traces:

- requested pose/trajectory;
- achieved pose/state;
- contact/constraint observations;
- residual/error and reason for divergence;
- whether the host considers the task complete.

Start with the Physics↔Gestures handoff contract and the existing arbitration rules. Do not add a second force, collision, or integration implementation inside Gestures.

### 6. Course and textbook connection

Every interactive lab becomes a lesson module with:
- a learning objective and prerequisites;
- a mathematical model, units, frames, and assumptions;
- a reproducible fixture and a prediction prompt;
- an interactive experiment;
- a generated evidence artifact and machine-readable trace;
- boundary cases and failure injection;
- assessment questions and instructor notes;
- citations and a statement of what the experiment cannot prove.

The textbook should link to runnable labs by stable lesson IDs. Changes to a model should update the corresponding derivation, tests, and generated evidence together.

## Delivery sequence

| Stage | Deliverable | Acceptance evidence |
|---|---|---|
| A — Evidence index | One page linking every living asset to source, command, fixture, and evidence class | No orphan instructional image; provenance visible in artwork |
| B — Motion workbench | Timeline, viewport, channel inspector, deterministic replay | Browser replay matches .NET golden vectors |
| C — Profile studio | Entity-specific profiles, anchors, seeded variation | Stable identity under reorder; profile compatibility tests |
| D — Physics bridge | Requested-versus-achieved comparison | Fixed procedure, varied Physics fixtures, explicit outcome source |
| E — Observer studio | Renderer Event Horizon and representation inspector | Real Renderer selection; stable semantic identity across fidelity |
| F — Population studio | Visualize instanced cohort with Gesture-derived inputs | Measured 1/100/1K/10K/100K workload ladder and reproducible report |
| G — GPU evaluation research | GPU implementation compared with CPU reference | Golden-vector parity within declared tolerances; independent benchmarks |
| H — University curriculum | Lesson graph, instructor guides, assessment, research labs | Chapters meet the textbook publication quality gates |

## Scientific and engineering guardrails

1. A generated image proves only the code path and data that generated it.
2. A browser replay of saved samples is not a live C# or Physics simulation.
3. Renderer selection is not physical truth.
4. Visualize SVG output is not GPU presentation; the static backend reports that it was not presented.
5. A GPU population renderer does not prove GPU-side Gesture evaluation.
6. A deterministic fixture is not measured human biomechanics.
7. A conceptual topology is not a standardized armature schema.
8. A research prototype is not a production solver.
9. Established mathematics, established literature, proposed synthesis, and original hypotheses must be labeled separately.
10. Novelty and priority claims require a documented literature review, not confidence or implementation effort alone.

## Existing proof surfaces

- [Gestures living motion documentation](LIVING_MOTION_DOCUMENTATION.md)
- [Visual Learning Lab](LEARNING_LAB.md)
- [SVG provenance standard](SVG_PROVENANCE_STANDARD.md)
- [Gestures domain map](GESTURES_DOMAIN_MAP.md)
- [Physics motion handoff](PHYSICS_MOTION_HANDOFF.md)
- [Physics/MicroBundle arbitration](PHYSICS_MICROBUNDLE_ARBITRATION.md)
- [Visualize static SVG backend](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/docs/STATIC_SVG_BACKEND.md)
- [Renderer → Visualize → GUI.SVG proof](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/blob/development/integrations/RendererAdapter/Proof/README.md)
- [Visualize Soldier Field WebGPU lab](https://github.com/TrentBest/TheSingularityWorkshop.Visualize/tree/development/samples/SoldierField/WebGpuRunner)
- [Renderer theory and Event Horizon model](https://github.com/TrentBest/TheSingularityWorkshop.Renderer/blob/development/docs/RENDERING_THEORY.md)
- [Workshop GUI](https://github.com/TrentBest/TheSingularityWorkshop.GUI)
- [Workshop Learning](https://github.com/TrentBest/TheSingularityWorkshop.Learning)

---

The goal is a motion studio in which the reader can inspect every transformation from procedure to physical outcome to observer-facing representation—and can reproduce the evidence rather than simply watch a polished animation.
