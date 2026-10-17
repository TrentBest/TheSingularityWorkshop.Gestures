# Speech scene: fight for every soldier

> **Status: scene design and system contract, not a claim of a finished playable scene.** This document turns the speech-scene concept into a sequence, ownership boundaries, scale strategy, and testable milestones. The companion SVG is an animated design map; until it is generated from a running scene, it is explanatory artwork—not runtime evidence.

![Animated speech-scene storyboard and event-horizon population strategy](assets/speech-scene-population.svg)

## The experience we are trying to create

The scene begins with the Workshop moniker, then resolves into a first-person view inside a fairly well-lit, richly decorated room. An advisor-soldier in the same exoskeleton family as every raWWar citizen stands directly ahead. Behind the player is an opening to a balcony. Beyond it is a city filled with soldiers: on rooftops, balconies, open windows, streets, plazas, and wherever the architecture plausibly supports a body or a viewing surface.

The advisor salutes over the already audible roar of the audience:

> “Sire, everything is ready. I just need to know if you want to use the prepared speech or if you wish to go off script again?”

The player chooses a response. The advisor places one hand on the player's shoulder and guides the player toward the opening behind them. The player steps onto the balcony. At first, the scale is discovered in layers: nearby formations, dense architectural surfaces, distant silhouettes, and public screens. Some screens show the player's own viewpoint with a small enough delay and a clear enough view that the player can raise a hand and recognize the same gesture on a screen. Then the crowd recognizes the arrival and the cheer blooms across the city—not as one synchronized sound, but as a spatially and temporally distributed event.

The emotional target is not simply “render a lot of soldiers.” It is **let the player discover that a city-sized population is present, responsive, and addressing them**. This scenario complements [Singularity Arena](SINGULARITY_ARENA_PERFORMANCE_SCENARIO.md), which already documents event-driven audience intention and performance cues; reuse that conceptual vocabulary where it fits, but do not assume the arena's current fixtures implement this city-scale scene.

## Sequence as an explicit state machine

These are scene-level phases. A consumer may orchestrate them with FSM_API; Gestures itself must remain a reusable, engine-neutral motion package.

| Phase | Player experience | Required cues / exit condition |
|---|---|---|
| 00 — Moniker | Workshop identity resolves into the scene | Transition completes; audio and world clocks have a defined origin |
| 01 — Private room | First-person view, decorated room, advisor directly ahead | Room is legible; advisor is framed and visible |
| 02 — Salute and question | Advisor salutes and speaks over the distant audience bed | Speech reaches its authored end; player can choose prepared / off-script |
| 03 — Choice and invitation | Advisor acknowledges the answer, places a hand on the shoulder, guides toward the rear opening | Player and advisor agree on the transition; contact presentation remains plausible |
| 04 — Threshold | Player crosses the opening and steps onto the balcony | The city reveal becomes visible without a single-frame crowd pop |
| 05 — Scale discovery | Soldiers occupy roofs, balconies, windows, streets, plazas, and distant surfaces; screens establish a shared viewpoint | Player can identify near, middle, and far populations and find at least one screen echo |
| 06 — Recognition wave | The population recognizes the player; cheering reaches the balcony with distance-aware timing | A spatially distributed response has propagated; nearby and far groups do not begin as one perfectly synchronized ring |
| 07 — Speech ready | Advisor yields the stage; player owns the next speaking action | Speech selection and crowd response remain available to the scene controller |

Player input must not be hidden inside a Gesture. The scene controller owns choices and phase transitions; Gestures describes the motion procedure and exposes progress/completion for the controller.

## Ownership: who is responsible for what?

- **raWWar** owns soldier identity, authoritative state, faction/citizenship, experience, equipment, and scene-specific crowd intent.
- **Gestures** owns reusable physical procedures, anchors, keyframes, deterministic reference evaluation, bounded identity variation, and evaluated pose output.
- **FSM_API** can coordinate scene phases, per-group lifecycles, interruption, and completion in the consuming experience. It is not a Gestures dependency.
- **Renderer** decides what the observer needs to perceive at each distance and which representation/fidelity tier is appropriate.
- **Visualize** presents selected geometry, instances, transforms, and effects efficiently on the GPU.
- **Physics** owns physical outcomes: contact, collisions, blocked movement, body/environment interaction, and any actual acoustic propagation model if that belongs to the simulation. Gestures can request an action; it must not fabricate the resulting physics.
- **Audio / scene presentation** schedules speech, crowd beds, localized reactions, propagation delay, and screen/audio mix. Do not pretend a simple time offset is a full acoustic simulation.

No Unity, WPF, Blazor, renderer, GPU, or game-specific types belong in the Gestures core. Keep domain-specific scene choreography in a consumer or provider bundle.

## The event horizon: spend detail where it changes the experience

The “event horizon” is an observer-relative representation policy, not a magic distance constant. The actual thresholds must be profiled and tuned against the target hardware and city density. Keep representation transitions stable with hysteresis, temporal continuity, and deterministic identity so soldiers do not pop, flicker, or change personality as the camera moves.

| Tier | What the player must perceive | Candidate representation |
|---|---|---|
| Interaction | Advisor, shoulder contact, hands, face/helmet direction, readable salute | Full authored gesture, high-fidelity pose, close contact presentation; Physics may resolve contact |
| Near crowd | Individual soldiers and recognizable reactions | Shared gesture definitions with stable per-soldier seeds; detailed transforms for visible bodies |
| Middle distance | Formation, wave direction, density, coherent non-identical silhouettes | Instanced archetypes and batched evaluated parameters; reduced pose/joint detail |
| Far city | A population on nearly every plausible surface and a response traveling across the city | Cluster/formation instances, occupancy masks, aggregated gesture phases and response fields |
| Beyond direct detail | Evidence that the city continues beyond individual readability | Skyline/architecture layers and density/response summaries, never thousands of useless fully articulated CPU agents |

These are **design tiers**, not claims that the current repository already has GPU batching, crowd simulation, articulated soldier rigs, or event-horizon selection. Start with measurable reference behavior, then optimize the presentation path while preserving the same semantic intent.

### Fight for every soldier—without simulating every vertex on the CPU

A soldier should remain an identity-bearing entity in authoritative data even when its visible form is aggregated. Rendering one hundred thousand silhouettes does not require one hundred thousand independent high-detail skeletons. The design target is:

1. Keep authoritative identity, location/cluster membership, experience profile, and current intent compact.
2. Represent crowd response as a shared event with a stable start time and a local response delay/phase.
3. Use a stable per-soldier seed to decorrelate timing, speed, amplitude, and selected gesture variants within bounded profiles.
4. Evaluate only the channels needed at each representation tier. Close soldiers get richer pose output; distant tiers consume compact parameters or precomputed/instanced forms.
5. Update population summaries and distant clusters at rates appropriate to perceptual change, while keeping close interaction and phase transitions responsive.
6. Make LOD changes observer-relative and reversible. Changing tier must not reset identity, restart the gesture, or alter authoritative state.
7. Measure CPU work, upload bytes, GPU time, visible count, and transition artifacts at explicit population sizes before making performance claims.

Do not add per-frame random jitter. Determinism means the same identity and event inputs reproduce the same behavior; controlled variation means a crowd is not a synchronized army of clones.

## Crowd recognition and sound delay

The cheering response should be modeled as an event with a defined origin time, not a global “play cheer now” toggle.

For a first-order sound-arrival approximation, a listener at distance (d) hears the event after

\[
t_{arrival}=t_{event}+\frac{d}{c}
\]

where (c) is the chosen speed of sound for the scene's medium and conditions. This is a useful timing approximation, **not** a complete model of urban reflections, occlusion, atmospheric variation, or directional filtering. If the simulation later owns a real acoustic propagation model, the scene should consume its results instead of duplicating them.

At city scale, the visual recognition wave and the audible cheer are related but not identical. A distant formation can begin reacting at a locally scheduled time; its sound reaches the player later. Local loudness, occlusion, and reverberation belong to the audio/presentation model. Avoid a single city-wide cheer sample that makes every block sound simultaneous.

A practical staged version can approximate the arrival time per cluster using distance and a documented speed-of-sound constant. It must use the cluster's actual location and the listener position, not just a hand-authored “far away” delay. Test monotonicity: for otherwise equivalent source events, a farther source must not arrive earlier than a nearer one.

## Screens are a perception contract

Public screens help the player understand that the whole city is watching the same event. A screen that shows the player raising a hand should use the same authoritative event/timebase as the player's gesture, with a declared capture/stream/display delay. It must not independently invent a second hand animation.

For the first proof, use a small number of screens with distinct orientations and viewpoints. At least one should clearly show the player-facing action, with enough latency to communicate a broadcast rather than a magical mirror. The scene can scale screen coverage later using shared feeds and instanced displays.

## Gesture inventory for the first vertical slice

Do not start by authoring every soldier animation. Establish a small reusable vocabulary that can make the scene readable:

- Advisor.SaluteAndAddress
- Advisor.AcknowledgeChoice
- Advisor.ShoulderContact
- Advisor.GuideToOpening
- Player.CrossThreshold
- Player.AcknowledgeCrowd
- Crowd.AttentionShift
- Crowd.RaiseHands
- Crowd.CheerResponse
- Crowd.ReturnToListening

These are proposed capability names, not necessarily existing code symbols. Each procedure should document required anchors, authored phases, interruption policy, completion signal, and which results are merely presentation versus physically resolved. A group can share a procedure while each soldier retains independent progress and stable bounded variation.

## Build order: prove the feeling, then prove the scale

1. **Contract and story:** preserve this sequence and ownership map in docs. Mark every illustration as conceptual until driven from runtime output.
2. **Single advisor:** create a test-backed salute/address progression and a separate shoulder-guide procedure with explicit anchors and completion states. Keep spoken dialogue in the scene/audio layer.
3. **First-person blocking:** prove room framing, rear opening, hand/shoulder staging, and threshold crossing with simple geometry before detailed art.
4. **Small responsive crowd:** demonstrate a few dozen independent identities reacting to a shared event without identical timing.
5. **Distance-aware sound:** test source/listener distances and arrival order with a controllable sound design placeholder.
6. **Screen echo:** show the player's raised hand on a small number of screens using the same event timebase.
7. **Event-horizon prototype:** render a measured progression from full bodies to instanced silhouettes to cluster density; preserve response phase across tier changes.
8. **Scale and profiling:** increase population by measured steps and record CPU/GPU/upload budgets. Do not call the scene “massive-scale capable” until those measurements exist.
9. **Living evidence:** where possible, generate SVG plots from real evaluator outputs, crowd-event schedules, and benchmark data. Keep a clear visual distinction between current implementation evidence and future-scene concept art.

## Acceptance checks

- Replaying the same seed, scene inputs, and event times reproduces the same gesture parameters and crowd response schedule.
- Different identities vary within declared bounds without changing the intended action.
- A soldier changing representation tier does not restart its gesture or lose identity.
- The advisor's contact gesture has explicit anchors and a documented failure/interruption path; a diagram alone does not prove collision handling.
- The crowd response can be traced to one event origin plus per-cluster/per-agent scheduling data.
- Sound arrival time is based on source/listener distance and a declared approximation or actual audio propagation output.
- Screen echoes use the same authoritative player action and an explicit display delay.
- The build records measured population counts and frame/CPU/GPU metrics before performance claims are made.
- Core package tests remain independent of Unity, Renderer, Visualize, Physics, and raWWar assemblies.

## Handoff notes for future LLMs

Before editing, inspect this document, the README, docs/LIVING_MOTION_DOCUMENTATION.md, docs/MOTION_EQUATION_EXPLORATION.md, the current evaluator/tests, and the consumer's scene controller. Separate **implemented behavior**, **test-backed research**, **conceptual design**, and **future optimization** in every change. Do not implement the whole scene by stuffing raWWar-specific choreography into Gestures. Keep changes on development; leave master untouched, do not merge or publish packages without explicit approval.
