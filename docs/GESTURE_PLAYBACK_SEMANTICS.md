# Gesture playback semantics — consumer contract proposal

This document proposes playback rules for consumers of the current Gesture evaluator. It does not add a playback controller, take ownership of an FSM, or claim that the proposal is a frozen public API.

## What the evaluator currently guarantees

`GestureEvaluator.Evaluate` is a stateless evaluation of a definition at a supplied elapsed time and duration. Negative elapsed time is clamped to the beginning, progression is clamped to `[0, 1]`, and `EvaluatedPose.IsComplete` becomes true at normalized progression 1. A caller can evaluate any time point again; the evaluator does not retain a playback cursor.

That makes pause, resume, reversal, and interruption orchestration responsibilities of the caller today.

## Recommended consumer policy

- **Pause:** retain the playback clock/progression externally. While paused, evaluate at the retained elapsed time rather than advancing it.
- **Resume:** continue from the retained elapsed time with the same definition, duration, variation settings, and identity seed. This avoids an implicit jump caused by restarting at zero.
- **Reverse:** evaluate the same authored definition at decreasing elapsed time. This retraces the interpolation path; it is not a physically validated reverse action and does not guarantee safe reverse contact.
- **Interrupt:** stop advancing the interrupted procedure and let the owning state machine choose the next procedure or recovery. Do not infer that contacts, constraints, or physical actions were undone.
- **Re-enter:** make the restart policy explicit at the call site. A fresh entry may reset elapsed time to zero; a continuation may restore saved elapsed time. Do not silently mix the two.
- **Change target/provider:** re-resolve anchors and validate the next pose against current domain observations. A previously reachable target or contact must not be treated as still valid after the world changes.

## Determinism checklist

For repeatable evaluation, keep the definition, elapsed time, duration, optional variation parameters, and identity seed fixed. If a caller changes any of these during playback, that is an orchestration decision and should be visible in its own tests.

Golden-vector values and floating-point tolerance policy remain open work; this note does not invent cross-runtime bitwise parity.

## Ownership boundary

Gestures supplies authored transforms and deterministic interpolation. FSM_COS or another host may orchestrate procedure selection and lifecycle. The domain/physics owner decides whether contact, support, occupancy, collision, or other physical outcomes actually occurred. Renderer consumes evaluated poses for presentation and must not turn visual completion into proof of a domain event.

This proposal should be revised alongside a concrete consumer integration before being treated as a stable compatibility promise.
