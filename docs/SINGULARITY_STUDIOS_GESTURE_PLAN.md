# Singularity Studios: Gesture and Procedure Plan

> **Status: architectural proposal and WebPage handoff, not an implemented filming system.** This document defines how the reusable Gestures domain can help Singularity Studios describe a massive simulated digital production stage. It does not claim that camera rigs, crew simulation, recording, editing, sound mixing, or studio-scale execution exist in this repository.

## The central idea

Singularity Studios should not be a giant hand-authored scene. It should be a reusable production environment in which a production is assembled from data: a stage, sets, performers, crew roles, cameras, motion-control rigs, lighting and grip equipment, props, cues, safety constraints, and a shot plan. The same stage and procedures can be reset, re-blocked, filmed from new angles, and reused for a different production.

**Gestures supplies reusable physical procedures.** It should not become the film director, shot editor, scene graph, camera renderer, sound mixer, or studio simulator. A production provider composes Gesture definitions with studio-specific anchors and task data; the WebPage or AnyApp experience orchestrates them; Physics resolves physical consequences; Renderer/Visualize decide and present what is visible.

## Three different kinds of motion

Do not collapse every moving thing into a humanoid animation.

1. **Performer procedures** — walk to a mark, turn to a partner, sit, stand, point, salute, handle a prop, react, fall, recover, or repeat a stunt beat. Actor identity and performance style are inputs; the procedure is reusable.
2. **Crew procedures** — carry a light stand, push a dolly, connect a cable, secure a set panel, place a prop, adjust a flag, clear a stage, or reset a take. Crew tasks need role-specific anchors, load/clearance checks, and recovery paths.
3. **Equipment procedures** — crane arm travel, dolly movement, pan/tilt/roll, lens or focus mechanism movement, robotic camera paths, lighting-boom movement, curtain/door/set-piece motion, and rig folding or locking. These are articulated mechanisms, not human rigs; a rig-neutral Gesture vocabulary is essential.

A fourth related class is **crowd/background performance**: repeatable but decorrelated loops and event-triggered responses that can be represented at different levels of detail. Distant background performers must not require the same evaluation cost as a featured actor in close-up.

## What a studio Gesture should describe

A reusable procedure should identify:

- **Intent and phases:** approach, align, execute, hold, release, retreat/reset; phases can be interrupted or resumed by the host.
- **Semantic anchors:** actor marks, partner marks, prop grips, dolly rails, crane pivots, cable endpoints, stage boundaries, safe zones, camera targets, and equipment docking points. Anchors are supplied by the set/rig/provider, not hard-coded world coordinates.
- **Parameters:** target mark, approach side, pace, hand/attachment choice, repeat count, hold duration, movement envelope, and a stable variation seed where variation is desired.
- **Preconditions and observations:** mark available, prop present, path clear, rig unlocked, cable connected, contact observed, load within limit, or camera settled. The host supplies authoritative outcomes.
- **Completion and failure:** arrived, aligned, action presented, contact confirmed, blocked, interrupted, unsafe, or reset required. Gesture evaluation alone cannot claim that a physical contact or task succeeded.
- **Presentation fidelity:** a featured performer may need detailed pose output; a far-background performer or distant crane may need a compact phase/transform representation. Representation changes must not restart authoritative progress.

Keep definitions data-first and deterministic for the same definition, input parameters, elapsed time, and stable identity. Avoid per-frame randomness. Variation should be bounded and stable: two extras can share the same blocking but differ in timing and posture without drifting off their marks.

## The production graph is not the Gesture

A shot or production is a directed schedule of tasks and cues, not one enormous Gesture. The production controller coordinates the graph; each task may request a Gesture and wait for host-supplied evidence before unlocking dependent tasks.

Example: a camera dolly move must not begin until the track is clear; a performer must not cross into the active path until the dolly is parked; the next take must not begin until the camera is reset, props are checked, and the stage is cleared. These are orchestration and safety/readiness rules, not interpolation logic inside Gestures.

A provider-authored shot plan can reference reusable procedures by ID and bind them to a specific set's anchors. Keep the same generic procedure usable on different stages by changing the binding data rather than cloning its definition.

## A first vertical slice: film one repeatable 30-second scene

Start with a small scene that proves the reuse loop before building a stadium-sized stage.

1. **Stage data:** define a floor, a few set pieces, two performer marks, one prop table, a short dolly track, one static camera, and one moving camera rig.
2. **Blocking:** performer A enters and reaches a mark; performer B turns and exchanges a prop; both settle into a final pose. Use anchor-relative procedures.
3. **Camera choreography:** the dolly travels to a mark, stops, and performs a bounded pan/tilt; the static camera records a second angle. Treat camera motion as equipment procedures.
4. **Crew reset:** crew members retrieve the prop, clear the path, and reset the dolly and performers. Use task gates driven by explicit completion observations.
5. **Take replay:** replay the same production inputs and stable seeds; compare phase times, transforms, and camera paths. A take is reproducible data, not a one-off manually animated scene.
6. **Change one variable:** move a set piece or actor mark and rebind anchors. The same procedure should still work when constraints remain valid; if a path is blocked, the host must report it instead of Gestures pretending success.
7. **Living documentation:** generate a timeline/overhead SVG from actual procedure outputs and task schedule data. Mark conceptual overlays separately from runtime-derived evidence.

Only after this works should the studio expand into multiple stages, motion-control cranes, tracked virtual cameras, large crowd scenes, stunts, practical effects, and huge indoor battle sets.

## Scale without simulating every detail

The dream is a studio large enough to film enormous battles and environments indoors. Scale should come from composition and fidelity policy, not from making every extra a full-detail actor every frame.

- Store production definitions, set/rig definitions, anchors, task graphs, and procedure parameters as reusable data.
- Give every actor, rig, prop, and crew entity a stable identity and authoritative task/phase.
- Evaluate detailed motion for the current shot's featured subjects; reduce distant/background performers to shared phases, compact transforms, or instanced representations where acceptable.
- Preserve the same semantic progress while changing representation. A camera cut must not reset an actor's procedure.
- Let Physics own collision, contact, forces, obstruction, breakage, and physical sound-source events.
- Let Renderer choose observer-relative detail and Visualize present it.
- Profile stage population, active procedure count, CPU evaluation, upload bytes, GPU time, and transition artifacts. Do not claim scale based on a large SVG or a large count alone.

## Sound and takes

Gestures describes motion, not an attached sound-effect list. A real footstep, prop impact, rig latch, rolling dolly, or falling set piece should produce physical sound from the actual interaction and material/physical state through the Physics-owned sound model and audio presentation path. Intentional dialogue, performance, score, and designed cinematic sound are separate authored production tracks.

A take should be able to reference synchronized event times and source events, but the Gesture must not fabricate contact, impact, or acoustic arrival. For each take, keep distinct:
- performer and equipment intent;
- observed physical outcome;
- dialogue/music/designed performance cues;
- physical sound-source events and their listener-relative presentation;
- camera capture and display/monitor latency.

## Ownership boundaries

- **Gestures:** reusable engine-neutral motion procedures, anchors, deterministic CPU reference evaluation, stable bounded variation, and evaluated pose/transform output.
- **Singularity Studios provider / MicroBundle:** studio vocabulary, set/rig anchor bindings, reusable blocking procedures, shot/task definitions, and production-specific configuration.
- **WebPage / AnyApp:** interactive studio control surface, stage selection, shot setup, playback controls, and visual proof of production data.
- **FSM_API:** optional lifecycle/task orchestration in the consumer; not a Gestures dependency.
- **Physics:** collision, contact, forces, constraints, obstruction, physical outcomes, and physical sound-source generation.
- **Renderer / Visualize:** observer-relative fidelity selection and efficient presentation.
- **Audio / production timeline:** dialogue, score, designed cues, mixing, and listener-relative presentation of sound events.
- **raWWar or other content providers:** their own characters, identities, factions, and scene-specific performance vocabulary; the generic Gestures package must not absorb them.

## Suggested initial Gesture vocabulary

These are proposed names for a provider catalogue, not existing public API symbols:

- `Performer.ApproachMark`, `Performer.TurnToPartner`, `Performer.PresentProp`, `Performer.HoldForShot`, `Performer.ExitMark`
- `Crew.CarryEquipment`, `Crew.PlaceProp`, `Crew.ConnectCable`, `Crew.ClearTrack`, `Crew.ResetSet`
- `Camera.DollyToMark`, `Camera.PanTiltToTarget`, `Camera.CraneMove`, `Camera.FocusPull`, `Camera.ReturnToHome`
- `Rig.Unlock`, `Rig.MoveWithinEnvelope`, `Rig.Lock`, `SetPiece.Open`, `SetPiece.Close`
- `Crowd.BackgroundLoop`, `Crowd.ReactToCue`, `Crowd.ReturnToHold`

The names are illustrative. The data contract and tests should come before a large catalogue of animation names.

## First acceptance criteria

- Same procedure inputs and stable identity reproduce the same reference output.
- Rebinding valid anchors changes placement without editing the procedure definition.
- Invalid/missing anchors and blocked/unsafe preconditions are surfaced by the host; no false completion is inferred from a pose reaching its endpoint.
- Performer, crew, and equipment procedures do not assume a single humanoid skeleton.
- A camera cut or LOD change does not restart authoritative procedure progress.
- The production controller waits for explicit task/physical observations at critical gates.
- The generated timeline SVG is sourced from evaluator/scheduler output and labels any conceptual elements.
- CI verifies the test and generated artifact; no performance or production-readiness claim is made without measurements.

## Handoff for WebPageLLM

Build the Singularity Studios experience as a **data-driven production workbench**, not a static showcase image and not a pretend completed simulator. The first visible interaction should let a user choose a small stage, inspect reusable performer/crew/camera procedures, bind them to marks and rig anchors, arrange a short shot sequence, run or scrub the reference timeline, and inspect the resulting transforms and task states. Show which values are authored data, which outputs come from actual Gestures evaluation, and which systems are future integration points. Keep the studio's enormous indoor battle-stage vision as the long-term composition target while making the first 30-second take concrete, testable, and reusable.

Before implementing, inspect the current WebPage and AnyApp repositories and their actual renderer/Visualize integration. Do not invent APIs or claim a runtime link that has not been built. Keep the Gestures core engine-neutral, preserve the master/development-only branch policy, and do not publish a package without explicit approval.
