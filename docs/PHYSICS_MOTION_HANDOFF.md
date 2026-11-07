# Physics motion handoff

**Status:** proposed integration boundary. This is not a new runtime API and does not claim that Gestures currently owns authoritative world motion or collision.

## 1. What Gestures currently provides

A Gesture is a reusable description of a procedure. The current reference evaluator evaluates named local transforms from normalized keyframes, interpolating translation and scale and using quaternion spherical interpolation for rotation. The output is an evaluated local pose.

That output is useful motion intent, but it is not by itself:

- a world-space trajectory for an identified physical body;
- a guarantee that the target can reach the requested pose;
- an applied force or torque;
- a collision/contact result;
- authoritative movement or a committed physical state.

The current evaluator's pose interpolation must not be described as a general (f(t)) physics solver. The spring-damper equation and motion-contribution catalogue remain exploratory research, not a frozen runtime contract.

## 2. Evaluator time semantics that the adapter must preserve

The current `GestureEvaluator.EvaluateWithSample` accepts elapsed seconds and a positive duration, but its interpolation domain is normalized progress, not world time.

- Deterministic speed variation scales elapsed time before division by duration.
- Negative elapsed time is clamped to the start; progress is clamped to [0, 1].
- Keyframe times are normalized progress values.
- Translation and scale are interpolated between neighboring local transforms; rotation uses the pose interpolation contract.
- The evaluator returns named local transforms and a lateral offset. It does not identify a physical body, compose a world transform, or report achieved motion.
- Variation samples are caller-supplied and validated; the host owns their stable identity/keying policy.

Therefore a host experiment must pin the exact definition/version, duration, variation sample, target mapping, frame/unit conversion, and logical-time-to-elapsed-time mapping. Re-evaluating the same elapsed time with different speed variation is not the same trajectory. A fixed elapsed-time sample sequence is deterministic only when those inputs are fixed.

The evaluator's current interpolation is piecewise over normalized keyframe intervals. The one-sided finite-difference slopes can differ at a keyframe; the new regression test verifies that distinction for a fixture. It does not add derivative outputs. A host adapter may estimate segment slopes for a declared linear-translation fixture, but must mark the keyframe boundary as derivative-discontinuous rather than expose an invented acceleration there.

## 3. Proposed adapter boundary

Keep Gestures and Physics independently usable. A host-side adapter should map a versioned Gesture definition and target mapping into an explicit motion request for Physics. The Physics core must not reference Gestures types.

The adapter contract should identify:

- Gesture definition ID/version and deterministic variation inputs;
- target body identity and transform mapping;
- logical time origin, duration, and progress/evaluation policy;
- coordinate frame and units;
- whether the result is a prescribed kinematic path, a controller target, or a typed physical contribution;
- pose and any derivatives only when their semantics are defined;
- continuity/validity boundaries and conservative motion bounds;
- the policy for unsupported targets, missing transforms, interruption, and invalid evaluation.

A target path and a force contribution are not interchangeable. A prescribed actuator path can move a mechanism kinematically while Physics accounts for contact reactions where supported. A controller target can be resisted or displaced by Physics. A force/torque contribution must instead declare units, frame, application point, activation, and validity.

## 4. Derivatives and time evaluation

The adapter must not promise derivatives that the source evaluator does not provide.

- Piecewise-linear translation has a defined segment velocity but a velocity discontinuity at a keyframe where segment slopes differ.
- Classical acceleration is not defined at such a discontinuity; report the boundary or use a documented bounded approximation.
- Quaternion interpolation needs an explicit angular-velocity convention before angular derivatives can be exposed.
- A stateful spring-damper response requires state initialization, integration method, timestep, and evaluation order. It cannot be treated as a pure arbitrary-time function without a separate reconstruction/closed-form strategy.

Physics uses these declarations to choose a safe prediction interval and conservative swept-volume bounds. If the path cannot be bounded safely, the adapter must expose that limitation rather than manufacture certainty.

## 5. Physical authority and observations

Physics owns the achieved physical state and confirms interactions. Gestures may supply intent and stable semantic anchors, but it does not decide whether contact occurred, whether a joint broke, whether a structure deformed, or whether a body completed its task.

The interaction result should return through stable body/feature identities and versioned observations. Gestures or the host may then use those observations to interrupt, adapt, or select a subsequent procedure. Visualize presents the physical evidence; it does not replace it.

## 6. Existing offline motion research

The repository already contains a dependency-free Python [motion-lab harness](../tools/motion-lab/README.md). It compares linear/eased kinematic baselines with a one-dimensional, fixed-step spring-damper tracker and records stable per-identity parameter samples as CSV. Its tests establish deterministic sampling and repeatability for that scalar experiment.

That harness is useful evidence for the motion-law research, but it does **not** provide a world-space (f(t)) contract in the .NET evaluator, finite-body trajectory bounds, angular derivatives, or collision prediction. Do not duplicate its scalar spring-damper experiment as if it solved the motion-to-contact problem. The next experiment is a separate geometric/trajectory study, with the existing harness retained as the scalar response reference.

## 7. First integration experiment

Before adding a public API:

1. Choose one simple translation path with a stable target identity.
2. Define its logical time mapping and motion mode.
3. Evaluate position at deterministic sample times and compare it with an analytic reference.
4. Declare the velocity discontinuity at a piecewise-linear keyframe.
5. Map the finite body shape through the path and calculate conservative swept bounds.
6. Compare predicted sphere contact with the existing Physics constant-velocity swept-sphere baseline.
7. Add a second case with constant acceleration and a third with a curved path; record when the source motion contract cannot provide sufficient bounds.
8. Keep detection observational and do not mutate authoritative body state.

The experiment should report position/time error, derivative validity, broadphase candidates, confirmed contacts, prediction subdivisions, and determinism. Only after this evidence should the packages freeze a shared adapter contract.

---

*The Singularity Workshop — edify, not mystify.*

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
