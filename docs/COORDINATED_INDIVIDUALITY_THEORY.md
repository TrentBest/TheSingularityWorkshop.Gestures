# Coordinated individuality: a theory of procedural behavior at scale

> **Status: teaching and architecture theory.** The principles in this document are intended to guide implementation and experiments. Equations are reference models unless a section explicitly identifies an implemented API. A mathematical description is not evidence that the corresponding solver, contact model, or population backend exists.

## Abstract

How can hundreds of thousands of soldiers execute one marching order without appearing to be hundreds of thousands of mechanical copies? The answer is not to author hundreds of thousands of unrelated animations, nor to inject arbitrary noise into one animation. It is to separate a shared activity from each participant's individual procedure, and to separate requested movement from the physical outcome of that movement.

**Coordinated individuality** is the design principle that many entities may share an activity, timing reference, and recognizable movement vocabulary while retaining stable, bounded, context-sensitive differences in execution. The group remains legible as a group; each member remains an individual.

Gestures is the procedural-behavior layer in this model. It describes and evaluates an entity's intended physical procedure. Physics supplies or evaluates physical constraints and consequences. A host coordinates orders, identity, world state, and authoritative outcomes. Presentation systems choose what an observer needs to see.

The model is useful beyond soldiers: dancers, crowds, workers, animals, teams, performers, robots, and any population whose members coordinate without becoming identical.

## 1. The central problem: one order, many lives

Consider a regiment receiving the order *March at cadence*. The order is shared. The resulting motion should not be identical.

Even trained soldiers differ in stature, stride length, joint range, fatigue, equipment, skill, attention, terrain, and momentary circumstance. A convincing formation expresses a common purpose through many local executions.

A poor solution adds independent random offsets to every frame. That produces jitter, not individuality. Another poor solution authors a unique animation for every soldier. That does not scale and makes shared changes difficult to maintain.

A better solution has four properties:

1. **Shared intent:** the activity coordinator declares what the group is doing.
2. **Authoritative reference:** a clock or timing model supplies the ideal cadence and phase.
3. **Stable individual expression:** Gestures derives bounded, reproducible differences from participant identity and declared context.
4. **Physical consequence:** Physics and the host determine what movement is possible and what actually occurs.

The group is coordinated because its members relate to a common reference. The members are individual because their procedures and outcomes are not collapsed into one shared pose.

## 2. A vocabulary that prevents category errors

The following terms should remain distinct in both teaching and code.

| Term | Meaning | Primary owner |
|---|---|---|
| Order | A command or request, such as Fall In, March, or Halt | Gameplay / command domain |
| Activity | A versioned description of a coordinated undertaking | Activity coordinator |
| Ideal reference | A shared cadence, phase, trajectory, or timing target | Clock / timing owner; may be Physics |
| Gesture | A reusable description of an entity's physical procedure | Gestures |
| Expression | Stable, bounded individual differences in that procedure | Gestures + actor profile |
| Evaluation | The procedure's requested pose or motion sample at a logical time | Gestures |
| Physical state | The world-observed position, velocity, contacts, constraints, and other state | Physics / authoritative world |
| Consequence | The response to an actual physical event, such as impact or obstruction | Physics + actor behavior policy |
| Presentation | The observer-relative representation selected for display | Renderer / Visualize |

A scheduled Gesture is not proof that a participant moved. An evaluated pose is not proof that a limb reached its target. A collision candidate is not a confirmed contact. A confirmed contact is not yet the same thing as a committed physical response. Each transition requires evidence from the owner of that fact.

## 3. The layered model

The architecture can be represented as a causal pipeline:

```text
Player / activity intent
          |
          v
Activity and formation coordinator
          |
          +---- shared cadence / logical clock
          |
          v
Per-participant Gesture evaluation
          |
          +---- stable identity and bounded expression
          |
          v
Requested pose / motion contribution
          |
          v
Physics and authoritative world
          |
          +---- contact, obstruction, forces, constraints
          +---- achieved state and event evidence
          |
          v
Individual response / recovery procedure
          |
          v
Observer-relative presentation
```

The arrows represent contracts, not necessarily synchronous calls or direct package references. Gestures should remain usable without a Physics dependency. A host adapter can translate a Physics-owned clock or observation into the small data contract Gestures consumes.

### 3.1 Activity and command

The activity coordinator decides that a regiment is marching, which soldiers are participants, what their roles are, and which shared timing reference applies. It owns order acknowledgement, pause, cancellation, command hierarchy, formation membership, and transitions such as March to Halt.

A Gesture does not issue military orders or decide who belongs to a formation.

### 3.2 Ideal reference

An ideal reference expresses the common temporal or spatial structure of an activity. For marching, it may be a cadence (f) in steps per second and a phase (phi(t)).

For constant cadence:

[
phi(t)=phi_0+2pi f(t-t_0)
]

The phase is a logical activity quantity, not a render-frame counter. A display running at 60 Hz and another running at 144 Hz must not cause the regiment to march at different rates.

Physics may provide this timing as part of a broader physical model, but the clock need not be a network service or a Physics implementation. A music clock, external time source, or activity scheduler may also be authoritative for its domain.

### 3.3 Individual procedure

Gestures maps the shared reference and participant context into a requested procedure. The same marching definition can be evaluated for many stable participant identities.

A conceptual participant parameter vector is:

[
mathbf{p}_i =
(ell_i, s_i, a_i, r_i, q_i, ldots)
]

where components might represent preferred stride length, timing offset, arm-swing amplitude, posture range, or skill-related limits. These parameters are illustrative; each must have defined units, bounds, and a real consumer before becoming an API.

A useful decomposition is:

[
	ext{procedure}_i(t)=
	ext{ideal}(t;mathbf{c})
+
	ext{boundedExpression}(t;mathbf{p}_i,mathbf{e}_i)
]

Here (mathbf{c}) is shared activity configuration and (mathbf{e}_i) is declared participant context. The plus sign denotes conceptual composition, not necessarily direct addition of transforms. Rotations, constraints, and semantic channels require appropriate composition rules.

### 3.4 Physical realization

Physics and the authoritative world determine whether the requested movement is achievable. Terrain, obstacles, contact, mass, inertia, joint limits, friction, compliance, and other modeled properties affect the outcome.

The evaluated procedure is an input or target to the physical system. It must never overwrite the physical result merely because the animation clock advanced.

## 4. Individuality is structured variation, not noise

A useful variation model has four requirements.

### 4.1 Stable identity

Each participant has a stable identifier independent of collection order, rank position, memory address, or current rendering batch. Reordering a population must not change which variation belongs to a soldier.

### 4.2 Bounded parameters

Each varied quantity has a declared interval or distribution with documented support. If a stride multiplier is constrained to ([a,b]), every generated value must remain in that interval. Bounds should be chosen from domain evidence, not arbitrary visual preference.

### 4.3 Temporal coherence

Variation should persist long enough to read as a characteristic of the person. A participant should not receive a completely new stride length every frame. Changes to cohesion or skill policy should generally take effect at declared safe boundaries, such as a completed step, rather than abruptly changing a limb target mid-swing.

### 4.4 Reproducibility

For a fixed algorithm version, stable participant ID, seed, and declared context, the same parameters should be reproduced. This supports debugging, deterministic replay, distributed evaluation, and comparison between CPU and future accelerated implementations.

A conceptual deterministic sampler is:

[
mathbf{p}_i = F(v,,s,,	ext{id}_i,,	ext{purpose},,	ext{context}_i)
]

where (v) is the sampler version, (s) is a seed, and purpose separates independent channels. This is a specification pattern, not a license to invent a new random-number implementation when the Workshop already has an approved deterministic source.

## 5. Cohesion is a controllable range, not sameness

A formation needs enough timing agreement to read as a formation. It does not need zero individual difference.

Let the ideal phase for participant (i) be (phi(t)), and let its bounded phase offset be (delta_i). Then:

[
phi_i(t)=phi(t)+delta_i
]

A cohesion policy may narrow the allowed spread of (delta_i) over time or after completed activity boundaries. The policy must preserve nonzero individual expression where the activity calls for it.

For circular phase observations, the complex order parameter is:

[
Z=rac{1}{N}sum_{i=1}^{N}e^{mathrm{i}	heta_i},
qquad R=|Z|
]

(R) measures phase concentration at one observation instant. It does not prove persistent phase locking, entrainment, causal coupling, or successful physical movement. Those claims require time-series evidence and a declared model.

The distinction matters pedagogically: a group can look aligned at one instant while its members are drifting apart. Conversely, a group may intentionally preserve small phase offsets while remaining recognizably coordinated.

## 6. The fence experiment: physical truth creates individual stories

Imagine one marching soldier swings an arm and the hand meets a fence.

The result depends on the physical properties and geometry of the contacted object. Gestures must not choose the impact outcome by selecting a more dramatic animation.

### 6.1 Compliant chain-link fence

A chain-link fence can deform. Depending on its construction, tension, anchoring, and contact geometry, it may yield, deflect, vibrate, or transmit some of the impact back to the hand and arm.

The physical model determines the contact response. The soldier's resulting hand trajectory and body disturbance become observations available to the behavior layer.

### 6.2 Rigid reinforced wall

A rigid wall may permit negligible displacement relative to the hand's intended path. The hand is obstructed; the arm's actual motion differs from the requested motion. The impact may produce a larger abrupt deceleration or force response, depending on the modeled body and contact mechanics.

The soldier may then protect the hand, rub it, shake it, alter the arm swing, lose the next beat, or recover and rejoin the cadence. Which response occurs is a behavioral decision conditioned by the event, the actor, and the activity—not an unconditional animation attached to the word “wall.”

### 6.3 The causal chain

```text
March procedure requests arm trajectory
                 |
                 v
Physics detects candidate contact
                 |
                 v
Contact model resolves actual interaction
                 |
                 +---- fence deformation / wall obstruction
                 |
                 v
Authoritative event + achieved body state
                 |
                 v
Actor response policy selects recovery
                 |
                 v
Gestures evaluates hand protection / rubbing / recovery
                 |
                 v
Physics evaluates the new movement and any further contacts
```

The equal-and-opposite interaction principle is not a visual effect. Contact forces are an interaction between bodies; both sides receive physically consistent consequences within the assumptions and numerical limits of the chosen model. A deformable fence requires a model of its compliance and support. A rigid wall requires a different constraint/contact response. A rigid-body collision approximation cannot claim to simulate detailed chain-link deformation.

### 6.4 Why the player sees a unique event

The soldier who hit the fence is not a global animation state. The event is attached to that soldier's stable identity and local physical history. Nearby observers may perceive it; distant observers may not. Other soldiers continue their own procedures unless their own physical or decision context changes.

This is how a population becomes a world rather than a synchronized display: local causes create local consequences, while shared activity remains coherent.

## 7. Scaling to hundreds of thousands

The scale problem is not solved by pretending every entity needs identical work or by claiming that every entity receives a full physical simulation at every display frame.

Separate the workload into measurable categories:

- **Shared work:** activity definitions, cadence, formation orders, common Gesture data, and immutable configuration.
- **Per-entity work:** stable identity, individual parameters, current phase/state, local observations, and response decisions.
- **Local physical work:** contacts, constraints, nearby interactions, and the physical details demanded by the model.
- **Presentation work:** observer-specific detail, interpolation, instancing, and visibility.

A population backend may batch compatible evaluations, reuse shared inputs, and process entities in stable chunks. Batching is an execution optimization; it must preserve per-entity semantics. The CPU reference remains the correctness oracle for optimized paths.

Use fidelity tiers as an explicit resource policy. A soldier immediately in front of the player may receive detailed pose evaluation and contact response. A distant soldier may use a lower-cost representation. An off-screen soldier may advance activity state less frequently. These are not claims that all entities are equally simulated. They are declared policies with promotion/demotion rules and continuity tests.

Measure separately:

1. activity and command update cost;
2. identity/variation sampling cost;
3. pose evaluation cost;
4. physical integration and contact cost;
5. host synchronization and data movement;
6. presentation and rendering cost.

Report population size, hardware, runtime, build, update cadence, allocations, memory, frame-time distribution, determinism, and fidelity policy. A high rendering frame rate does not prove that every soldier was fully simulated.

## 8. Observer-relative fidelity without observer-relative truth

Different observers need different amounts of detail, but there must be one authoritative world state.

The nearby player may see the hand hit the wall and watch the soldier rub it. A distant observer may see only a brief arm interruption. A strategic map may show the soldier's formation slot as temporarily delayed. These are different representations of the same underlying event and state.

The observer must not create a second physical outcome. Level of detail changes how much is computed or shown, not what the world claims happened. If an entity is promoted to higher fidelity, its detailed state must reconcile with the authoritative lower-fidelity state without teleportation or fabricated history.

## 9. Design propositions

The following propositions are useful as review questions and test hypotheses.

1. **Identity invariance:** changing collection order does not change a participant's stable variation.
2. **Shared-reference invariance:** changing render frequency does not change activity cadence.
3. **Bounded expression:** every varied parameter remains within its declared bounds.
4. **Temporal coherence:** a fixed participant does not exhibit unbounded frame-to-frame parameter noise.
5. **Physical authority:** an evaluated target never overwrites achieved physical state.
6. **Local causality:** one participant's contact event does not automatically alter every participant's behavior.
7. **Replayability:** the same versioned inputs and event history reproduce the same deterministic decisions within declared numerical tolerances.
8. **Optimization parity:** a batch or accelerated evaluator preserves the CPU reference semantics.
9. **Fidelity continuity:** changing observer distance or representation does not silently change authoritative identity or state.
10. **Evidence honesty:** every demonstration distinguishes conceptual illustration, specified contract, implementation, test, benchmark, and cross-system validation.

These propositions can be turned into property tests, analytical fixtures, benchmarks, and classroom exercises.

## 10. Suggested laboratory sequence

A course or research group can reproduce the ideas in increasing complexity.

### Lab A — one procedure, two participants

Give two participants the same ideal march cadence and different stable identities. Measure whether their output is reproducible and bounded. Confirm that neither is a frame-by-frame random copy.

### Lab B — a cohort with a cohesion control

Increase the population through 1,000, 10,000, and 100,000 participants. Narrow variation at completed step boundaries. Record phase concentration, per-entity output, CPU time, allocations, and memory. Do not infer physical simulation from pose throughput.

### Lab C — one contact, one consequence

Run one soldier toward a compliant obstacle and a rigid obstacle. First use a documented analytical or simplified contact fixture. Compare requested and achieved hand trajectories. Then add a response policy and verify that only the affected actor receives the local recovery request.

### Lab D — observer-relative representation

Show the same event to a nearby and distant observer. Confirm that the presentation differs while event identity and authoritative outcome remain the same.

### Lab E — population parity

Compare scalar CPU evaluation with any future batch/GPU implementation for identical stable identities, seeds, definitions, and logical times. Report numerical tolerances and any unsupported channels.

## 11. What Gestures is—and is not

Gestures is not a replacement for Physics, a universal character AI, a military command system, a renderer, or a social simulator. It is the reusable procedural layer that translates declared activity intent and actor context into evaluated physical procedures, then consumes only explicitly supplied observations when planning subsequent behavior.

Physics is critical to the complete experience, but Gestures should not require a Physics assembly reference to define a march, evaluate a pose, or sample bounded individual expression. The integration boundary is a host-owned contract.

The first intended consumer is **raWWar**. The soldier population is not merely a stress test: it is the proving domain. A successful package must support a shared march procedure, stable per-soldier individuality, deterministic replay, local physical interruptions, recovery, and honest scaling evidence.

The long-term measure of success is not the largest number of identical figures on screen. It is the largest population for which the system can preserve meaningful identity, coherent shared activity, local physical consequence, and observer-appropriate detail.

---

## Glossary

- **Achieved state:** the state actually reported by the authoritative world/Physics owner.
- **Activity:** a coordinated undertaking with participants, phases, timing, and transition rules.
- **Cohesion:** a measurable degree of coordination among participants; not a synonym for identical motion.
- **Coordinated individuality:** shared activity with stable, bounded, context-sensitive individual expression.
- **Gesture:** a reusable procedural description of physical action.
- **Ideal reference:** shared timing, phase, or trajectory against which individual procedures are organized.
- **Local consequence:** an outcome caused by a participant's own interaction and physical history.
- **Observer-relative fidelity:** the amount and form of detail presented to a particular observer without changing authoritative world truth.
- **Requested pose:** a procedure's evaluated target or pose output; not proof of achieved movement.
- **Stable identity:** an identifier that persists independently of ordering, batching, and presentation.
