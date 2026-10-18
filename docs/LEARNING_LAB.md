# Learn Gestures by seeing, changing, and testing motion

> **Teaching goal:** let a learner move from *watching a motion* to *explaining its data*, then to *changing a rule*, and finally to *proving whether the change works*.

Gestures is an engine-neutral domain for describing and evaluating reusable physical procedures. It is not yet a complete animation suite or physics solver. This lab is designed for students, teachers, hobbyists, and contributors who want to explore the implemented contract while helping identify its gaps.

## Start here: follow the evidence

### 1. Watch a procedure become a path

![Evaluator-backed motion: the path and marker are serialized from real GestureEvaluator samples](assets/gesture-motion.svg)

**Try to notice:** the authored keyframes, the in-between samples, the straight path segments, and the marker's changing orientation and scale.

- [Open the generator source](../tools/living-demo/Program.cs).
- [Regenerate the living scene](../tools/living-demo/README.md).
- **Ask:** Which points were authored, and which were evaluated between them?
- **Change-it challenge:** change one keyframe position, regenerate the SVG, and explain which parts of the image changed. Keep the generated snapshot synchronized with the source output.

**Evidence boundary:** the SVG serializes actual CPU evaluator output, but its browser animation is presentation of saved samples. It does not execute the evaluator in the browser or demonstrate physical forces, articulated joints, or collision response.

### 2. Read the motion as channels, not just a picture

![Evaluated root transform channels plotted over normalized progress](assets/gesture-channels.svg)

A transform has several observable channels. A path alone can hide changes in orientation or scale.

- **Ask:** Can two motions follow a similar path but have different rotations or scales?
- **Change-it challenge:** alter a rotation or scale keyframe and predict the graph before regenerating it.
- **Explain:** distinguish normalized gesture progress from seconds of wall-clock time.

See [Gesture playback semantics](GESTURE_PLAYBACK_SEMANTICS.md) for the difference between evaluating a time sample and coordinating a running procedure.

### 3. Compare consistent individuality with random noise

![Seeded experience-profile variation concept](assets/skill-distribution-concept.svg)

The variation generator also produces a seeded SVG artifact in CI; its downloadable workflow artifact is the generated output, rather than a checked-in snapshot. For a browser-side interactive companion, try the [skill distribution demo](skill-distribution-demo.html), which is illustrative data rather than physical validation.

The implemented experience-profile sampler varies a limited set of channels (speed and root lateral offset) using stable seeded inputs. It does not yet represent all the ways skill changes joint-level performance.

- [Open the variation generator](../tools/variation-demo/README.md).
- **Ask:** If the same participant is evaluated again with the same inputs, should their variation change? What should happen when the seed changes?
- **Change-it challenge:** run two seeds and compare the output. Record what is bounded, what is repeatable, and what the current model does not represent.

### 4. See the difference between intent and outcome

![Deterministic crowd-response intent schedule generated from CrowdResponseScheduler](assets/crowd-response-schedule.svg)

The schedule is generated from the CPU scheduler's returned intent records. A scheduled reaction is not proof that a person moved, a sound arrived, or the environment was physically traversable.

- **Ask:** What evidence would be needed to prove arrival rather than intent?
- **Change-it challenge:** alter a schedule input in the demo, regenerate the artifact, and add a test for the behavior you expected.
- Read [crowd-response scheduling](CROWD_RESPONSE_SCHEDULING.md) and [motion response comparison](MOTION_RESPONSE_COMPARISON.md).

### 5. Explore the edges of a reachable motion

![Inverse-kinematics intuition: target, links, and possible reach](assets/ik-intuition.svg)

![Inverse-kinematics reachability concept](assets/ik-reachability.svg)

These are teaching diagrams, not proof that the production evaluator includes a general IK solver. The repository contains research prototypes and explicit limits; learners should keep the distinction visible.

- **Ask:** What makes a target unreachable for a two-link mechanism?
- **Change-it challenge:** propose a reachable, unreachable, and degenerate test case. Before implementing anything, read [Inverse Kinematics Theory](INVERSE_KINEMATICS_THEORY.md) and [Stateful IK Theory](STATEFUL_IK_THEORY.md).

## A short lesson sequence

| Lesson | Learner should be able to… | Starting evidence | A useful student artifact |
|---|---|---|---|
| 1. Sampled motion | Explain keyframes and in-between samples | Motion SVG + C# generator | Annotated path or prediction |
| 2. Transform channels | Compare translation, rotation, and scale | Channel plots | Before/after graph |
| 3. Determinism | Explain why a stable seed matters | Variation demo + tests | Repeatability test |
| 4. Intent vs. achievement | Identify what an artifact proves and does not prove | Crowd schedule and comparison contract | Evidence checklist |
| 5. Reachability | Separate target intent from solver feasibility | IK theory and diagrams | Boundary-case test proposal |
| 6. Coordination | Describe roles, prerequisites, delays, and recovery | [Starport crew routing](STARPORT_CREW_ROUTING_SCENARIO.md) | Failure-injection scenario |
| 7. Domain boundaries | Assign responsibility to the right system | [Domain map](GESTURES_DOMAIN_MAP.md) | Ownership diagram |

Teachers can use one lesson in a class, or use the sequence as a small project. No game engine is required to read the diagrams or build the core solution; the documented generator path requires the .NET 8 SDK.

## What does “living documentation” mean here?

A living image should have a traceable relationship to code or an explicitly declared conceptual purpose.

- **Generated evidence:** the C# generator calls a real implementation and serializes its outputs. Examples include the motion path, seeded variation, provider/actor choice, and crowd-response schedule.
- **Conceptual illustration:** an authored SVG explains a theory, proposed architecture, or not-yet-implemented behavior.
- **Research visualization:** an experiment illustrates a model whose stability, scope, or integration is still under evaluation.

Every instructional SVG must carry its own visible provenance label, including whether GUI.SVG was used. See the [SVG provenance standard](SVG_PROVENANCE_STANDARD.md). A pretty picture is not evidence of an implemented feature; the caption and source should say what the image actually establishes.

## Let learners improve the project

The goal is not to pretend that the domain is finished. A careful student can make the repository better by finding a missing explanation, a misleading image, an edge case, or a mismatch between theory and executable behavior.

### A safe contribution loop

1. **Observe.** Pick one diagram, behavior, or contract and describe what you believe it means.
2. **Predict.** Write down the output or invariant you expect before changing code.
3. **Reproduce.** Use the existing generator or test project and capture the exact command and inputs.
4. **Test.** Add a small regression test for a behavior claim. Prefer a failing test that demonstrates the gap before the fix.
5. **Correct.** Make the smallest code or documentation change that addresses the evidence.
6. **Regenerate.** If a living SVG changed, regenerate it from the actual source; do not hand-edit generated output to hide a mismatch.
7. **Explain.** Update the relevant theory and image caption, including limitations and provenance.
8. **Verify.** Run build and tests, inspect SVG diffs, and report what was and was not verified.

### Good first contributions

- A confusing term with a clearer definition and a concrete example.
- A test for a boundary condition (zero duration, a missing anchor, a non-finite value, an unreachable target, or a changed seed).
- A generated-image mismatch between code and the checked-in snapshot.
- A diagram whose visual implication exceeds the behavior actually implemented.
- An accessibility improvement: meaningful alt text, clearer contrast, labels that do not depend on color alone, or a textual description of the diagram.
- A small reproducible experiment that measures a claim instead of repeating it as fact.

### Keep the ownership boundaries intact

- **Gestures:** reusable procedure data, deterministic evaluation, bounded variation, and explicit motion/coordination contracts.
- **Physics / the world owner:** authoritative forces, collision, contact consequences, material behavior, and whether a physical action succeeds.
- **Renderer:** observer-relative representation and fidelity decisions.
- **Visualize:** GPU-facing drawing and presentation.
- **The consuming experience:** intent, gameplay or activity rules, and authoritative actor state.

A report that says “the image looks wrong” is a useful start. A report that includes the expected behavior, actual behavior, reproduction steps, and a minimal test makes it actionable.

## Report a learning or correctness issue

Open an issue in [TheSingularityWorkshop.Gestures](https://github.com/TrentBest/TheSingularityWorkshop.Gestures/issues) with:

- **What I expected**
- **What I observed**
- **Steps to reproduce** (commands, inputs, seed, and runtime when relevant)
- **Why it matters** for the lesson or domain contract
- **Evidence** (test output, screenshot, SVG diff, or a small example)
- **Suggested correction** (optional; diagnosis is welcome even without a fix)

For code contributions, work on `development`, add or update tests, and let CI verify the result. Do not claim that a conceptual diagram proves implementation, and do not publish a package as part of a learning exercise.

## Keep the scope honest

The [domain completion criteria](DOMAIN_COMPLETION_CRITERIA.md) is the checklist for what remains to be implemented, verified, demonstrated, and measured. The [roadmap](ROADMAP.md) records direction; this lab is an invitation to inspect and improve the evidence. Gestures is engine-neutral and intentionally does not own a renderer, a game loop, or authoritative physics.

> The goal is not merely to teach a reader what motion means. It is to give them a visible experiment they can question, reproduce, and improve.
