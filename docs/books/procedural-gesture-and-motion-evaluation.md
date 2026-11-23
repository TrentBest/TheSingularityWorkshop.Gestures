![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Visualize/development/docs/assets/moniker.svg)

# Procedural Gesture and Motion Evaluation

**Book ID:** gestures.domain.procedural-gesture-and-motion-evaluation  
**Edition:** 0.1.0 · **Category:** Domain · **LOD:** 1  
**Status:** Draft — ownership pending independent SURPS review

## 1. Purpose and scope

This book defines a reusable, engine-neutral representation of a physical procedure and the deterministic reference operation that evaluates it at a declared time. A Gesture is not merely a baked animation clip: it is a procedure whose data can be inspected, sampled, tested, and consumed by different experiences.

It covers pose transforms, keyframes, named channels, anchors, procedure duration, deterministic interpolation, stable identity-based bounded variation, evaluated pose output, and caller-driven playback progression. It does not define universal actor intent, tabletop initiative, physical laws, collision resolution, production inverse kinematics, rendering, or a general social simulator.

## 2. Tabletop-first procedure

A paper-and-pencil group can use the same procedure without software:

1. The provider describes the procedure, required starting position, named reference points, expected duration, and ordered pose/action phases.
2. The acting character records relevant capabilities, training, condition, and available equipment.
3. The table checks prerequisites and chooses a permitted attempt. SURPS/CORE and selected domain rules determine action timing and resolution; this book supplies the procedure representation, not a replacement for those rules.
4. At each agreed time boundary, the group determines the current phase and records the expected pose or transition.
5. The referee resolves contact, obstacles, force, injury, and success using authoritative world and Physics rules. An evaluated pose never guarantees that the character reached it.
6. The table records completion, interruption, failure, or recovery separately from intended motion.

Example: a character boards a vehicle using a ramp. The procedure lists approach, threshold crossing, ascent, and entry poses. If the ramp shifts or balance is lost, the referee resolves that physical event; the procedure does not teleport the character into the seat.

## 3. Data and evaluation model

The reference model includes GestureDefinition, GestureKeyframe, PoseTransform, GestureAnchor, GestureEvaluator, EvaluatedPose, GestureVariation, and GestureExperienceProfile.

The current evaluator linearly interpolates translation and scale and uses quaternion spherical interpolation for rotation. Reproducibility depends on the same definition, elapsed time, variation parameters, and identity seed. The current profile sampler bounds speed and root lateral offset; it does not model joint-level competence.

Implementation:
- [GestureDefinition](../../src/TheSingularityWorkshop.Gestures/GestureDefinition.cs)
- [GestureEvaluator](../../src/TheSingularityWorkshop.Gestures/GestureEvaluator.cs)
- [EvaluatedPose](../../src/TheSingularityWorkshop.Gestures/EvaluatedPose.cs)
- [GestureVariation](../../src/TheSingularityWorkshop.Gestures/GestureVariation.cs)
- [GestureExperienceProfile](../../src/TheSingularityWorkshop.Gestures/GestureExperienceProfile.cs)

Technical references: [Data Model](../DATA_MODEL.md), [Determinism](../DETERMINISM.md), [Living Motion Documentation](../LIVING_MOTION_DOCUMENTATION.md), [Golden Vectors](../GESTURE_GOLDEN_VECTORS.md).

## 4. Time and playback

GesturePlayback is a caller-driven lifecycle for one procedure. The host supplies finite, non-negative delta time. The session supports Ready, Playing, Paused, Completed, and Cancelled states. It does not own a clock, schedule initiative, resolve anchors, blend gestures, or infer physical success.

GestureSequencePlayback composes a fixed ordered list and transfers unused time across step boundaries. It is not a general branching choreography graph. For tabletop use, the same states can be represented on a turn/time ledger.

Implementation and contract: [GesturePlayback](../../src/TheSingularityWorkshop.Gestures/GesturePlayback.cs), [GestureSequencePlayback](../../src/TheSingularityWorkshop.Gestures/GestureSequencePlayback.cs), [Evaluation Lifecycle](../GESTURE_EVALUATION_LIFECYCLE.md), [Playback Semantics](../GESTURE_PLAYBACK_SEMANTICS.md).

## 5. Capability maturity and limitations

| Capability | Current status | Boundary |
|---|---|---|
| Keyframe transform evaluation | Implemented reference baseline | Linear translation/scale; quaternion spherical rotation |
| Stable deterministic variation | Partial | Speed and root lateral offset only |
| Single-gesture lifecycle | Implemented | Caller supplies time; no scheduler |
| Fixed ordered sequence | Implemented | No general branching, looping, reversal, or blending |
| Planar kinematics and articulated joints | Research prototypes | Not a production general IK solver |
| Contact and recovery | Research prototypes | Physics/host remains authoritative |
| GPU evaluation and presentation | Not owned here | Visualize/Renderer territory |

No benchmark, build, test, physical correctness, or release claim is made here. Repository CI and linked tests are the evidence authorities for the exact commit.

## 6. Verification exercises

1. Evaluate the same definition at the same time and seed twice; compare every output channel.
2. Change only the identity seed; verify stable bounded variation rather than per-frame noise.
3. Advance playback in two deltas and compare with one equivalent delta where the contract permits equality.
4. Pause and resume without losing elapsed time.
5. Interrupt a vehicle boarding attempt and record physical outcome separately from the last evaluated pose.

Tests: [Evaluator tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureEvaluatorTests.cs), [Golden-vector tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureEvaluatorGoldenVectorTests.cs), [Playback tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GesturePlaybackTests.cs), [Sequence tests](../../tests/TheSingularityWorkshop.Gestures.Tests/GestureSequencePlaybackTests.cs).

---

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
