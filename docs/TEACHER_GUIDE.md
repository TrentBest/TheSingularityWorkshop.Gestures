# Teaching with the Gestures Learning Lab

This guide turns the [Learning Lab](LEARNING_LAB.md) into a class session. It is suitable for a computer-science, simulation, animation, or systems-design lesson. The goal is not to imply that every pictured capability is implemented; the goal is to teach learners to separate observation, model, implementation, and proof.

## A 45-minute session

### Before class (teacher)

- Open the [Learning Lab](LEARNING_LAB.md) and the [Gestures domain map](GESTURES_DOMAIN_MAP.md).
- Choose one visual: the evaluated motion path, transform channels, seeded variation, or crowd-response schedule.
- For a hands-on session, install the .NET 8 SDK and verify that the documented generator command runs. If setup is unavailable, use the checked-in SVG and perform the prediction and evidence exercises without changing code.
- Tell students which artifact is generated from implementation and which is conceptual before asking them to interpret it.

### Session plan

| Time | Activity | Teacher prompt | Evidence of learning |
|---|---|---|---|
| 0–5 min | Observe one image silently | “What can you state from the picture alone?” | Observations without assumptions |
| 5–12 min | Trace its provenance | “Which code produced these values? What is only presentation?” | Source link and evidence category |
| 12–20 min | Predict a change | “If this keyframe, channel, or seed changes, what should stay invariant?” | A written prediction |
| 20–32 min | Run or inspect the experiment | “Did the output match your prediction? What input did you use?” | Reproducible command/input or annotated image |
| 32–40 min | Turn a mismatch into a test | “What smallest example would distinguish the expected behavior from the observed behavior?” | Test idea with explicit expected result |
| 40–45 min | Explain the boundary | “What does this evidence *not* establish?” | One limitation stated precisely |

## Question prompts and expected reasoning

Use these as discussion guides, not as answers students must memorize.

### Sampled motion

**Prompt:** Are all points on the path authored keyframes?

**Expected reasoning:** No. The definition supplies keyframes; the evaluator returns samples between them. A generated path can show evaluated samples, but the rendered SVG does not mean the browser is executing the C# evaluator.

**Follow-up:** What would you change to test whether interpolation is linear? Choose a segment, predict an intermediate position, and compare it with an evaluator sample within the documented numerical tolerance.

### Transform channels

**Prompt:** If two actors travel along the same position path, must they have the same motion?

**Expected reasoning:** No. Rotation and scale can differ while translation is identical. A path-only visualization hides those differences.

**Follow-up:** Change one channel while keeping position keyframes fixed. Which chart or assertion would make that difference observable?

### Determinism and variation

**Prompt:** Should the same identity and seed produce a new lateral offset every frame?

**Expected reasoning:** No. The current experience-profile variation is a bounded, seeded parameter model, not per-frame noise. Stable inputs should reproduce stable outputs within the contract. Changing a seed may change the sample, but tests should assert only guarantees the implementation actually defines.

**Follow-up:** Which inputs must be held constant to make a fair comparison? Which output bounds can be asserted from the current profile?

### Intent versus outcome

**Prompt:** Does a scheduled crowd response prove that an actor arrived at the target?

**Expected reasoning:** No. A schedule is intent/timing data. Arrival requires achieved-state evidence from the owner that observes or simulates the actor and environment.

**Follow-up:** Define the minimum record needed to compare intended and achieved motion without silently converting units, coordinate frames, or timebases.

### Reachability and ownership

**Prompt:** If a target is outside a two-link arm's reachable region, should a diagram make the arm reach anyway?

**Expected reasoning:** No. A conceptual diagram can illustrate a question, but it is not a solver result. A real implementation needs an explicit success/failure contract and tests for unreachable and degenerate cases.

**Follow-up:** Decide which part owns target intent, which part solves a kinematic constraint, and which part determines physical contact consequences.

## A lightweight assessment rubric

Score each dimension from 0 to 2. A student can do well without writing production code.

| Dimension | 0 — Not yet | 1 — Developing | 2 — Demonstrated |
|---|---|---|---|
| Observation | Makes an unsupported claim | Separates some observation from inference | States what the artifact directly shows |
| Provenance | Cannot identify the source | Finds the source but overstates evidence | Names source, evidence class, and limitations |
| Prediction | No testable prediction | Prediction is partly measurable | Prediction names inputs and observable output |
| Reproduction | Steps cannot be repeated | Some inputs are missing | Records command, inputs/seed, and expected result |
| Correction | Suggests only a visual edit | Identifies a plausible change | Connects correction to a test and regenerated evidence |

Use the rubric to identify the next learning step, not to penalize a learner for discovering a real defect.

## Learner safety and scope

- A report can be valuable even when the learner cannot diagnose or fix the defect.
- Do not ask students to “correct” a generated SVG by editing the output alone. Fix the source or generator, regenerate, and inspect the diff.
- Do not present a research diagram as a production solver or a schedule as a physical outcome.
- Do not require a game engine. The conceptual exercises work from the checked-in artifacts; code exercises use the documented .NET 8 workflow.
- Keep package publication, secrets, and release operations out of classroom tasks.

## Student hand-in

Ask for one page or a short pull request containing:

1. The artifact and source they examined.
2. One observation and one limitation.
3. A prediction with fixed inputs.
4. The result and how it was reproduced.
5. A test, documentation correction, or issue report that would help the next learner.

If a student finds a defect but cannot fix it, submit it through the [learning or correctness issue template](../.github/ISSUE_TEMPLATE/learning-or-correctness-report.md).
