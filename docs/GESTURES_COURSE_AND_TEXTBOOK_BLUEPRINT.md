# Gestures: Comprehensive Course and Textbook Blueprint

> **Status:** curriculum architecture. This is a proposed course sequence, not accreditation, a claim of completed lessons, or evidence that every topic has a production implementation. Each module must pair theory with an executable or explicitly conceptual laboratory.

## Program identity

**Working title:** Procedural Gesture: Time, Configuration, Interaction, and Coordinated Motion

**Central question:** How can we describe what an entity intends to do over time, evaluate that procedure for different targets and individuals, and distinguish the requested action from what the physical world actually permits?

The course uses Gestures as its procedural model and Physics as the authority for physical mathematics, constraints, environmental effects, and realized outcomes. Renderer and Visualize are presentation consumers, not sources of physical truth.

## Learning outcomes

A successful learner can:

1. distinguish posture, trajectory, procedure, activity, physical state, and presentation;
2. define coordinate frames, units, logical time, and temporal channels;
3. evaluate interpolated transforms and explain their limitations;
4. describe target-relative articulation without assuming a universal humanoid rig;
5. reason about reachability, constraints, and solver residuals;
6. parameterize procedures with stable, bounded, reproducible variation;
7. distinguish open-loop evaluation from observation-driven response;
8. explain how contact and environmental effects belong to Physics;
9. design coordinated multi-participant activities with shared timing and individual expression;
10. measure phase concentration without overclaiming synchronization;
11. design deterministic population workloads and validate scalar/batch parity;
12. formulate falsifiable hypotheses and report uncertainty honestly.

## Course architecture

### Foundation sequence

| Course | Subject | Signature laboratory |
|---|---|---|
| GES 101 | Seeing and describing movement | Annotate a posture-to-posture action |
| GES 110 | Space, frames, and transforms | Reconstruct a target transform from local frames |
| GES 120 | Time, rhythm, and procedure | Sample a Gesture at arbitrary logical times |
| GES 130 | Interpolation and motion channels | Compare linear translation and quaternion rotation interpolation |
| GES 140 | Target models and semantic anchors | Bind one procedure to two different target layouts |

### Intermediate sequence

| Course | Subject | Signature laboratory |
|---|---|---|
| GES 201 | Articulated configuration | Build a small non-humanoid articulated profile |
| GES 210 | Kinematics and reachability | Show reachable and unreachable targets with residuals |
| GES 220 | Procedure composition | Compose, interrupt, resume, and replay a multi-phase procedure |
| GES 230 | Individual expression | Generate stable seeded variation and test reorder invariance |
| GES 240 | Skill, adaptation, and uncertainty | Compare scalar skill with a multi-dimensional profile |
| GES 250 | Observation and response | Inject an obstruction observation and select a recovery procedure |

### Advanced sequence

| Course | Subject | Signature laboratory |
|---|---|---|
| GES 301 | Dynamics and the Physics boundary | Compare requested motion with achieved state under different environments |
| GES 310 | Contact and interaction | Contrast compliant and rigid contact using Physics-owned results |
| GES 320 | Coordination and synchronization | Measure phase concentration across a cohort |
| GES 330 | Choreography and activity systems | Model roles, dependencies, blocked states, and completion evidence |
| GES 340 | Population execution | Compare scalar and batched deterministic evaluation |
| GES 350 | Observer-relative fidelity | Promote/demote representation without altering authoritative truth |

### Research sequence

| Course | Subject | Signature laboratory |
|---|---|---|
| GES 401 | Experimental design and measurement | Pre-register a falsifiable motion hypothesis |
| GES 410 | Numerical methods and stability | Analyze timestep sensitivity and error envelopes |
| GES 420 | Empirical movement science | Compare model outputs against measured reference data |
| GES 430 | Reproducibility and verification | Produce golden vectors and an independent evaluator |
| GES 490 | Thesis / capstone | Defend a new model with code, data, limits, and literature review |

## Mathematics spine

Mathematics is taught as a connected sequence and coordinated with the Workshop Physics package rather than duplicated as a second, competing math implementation.

1. Arithmetic, ratios, units, and dimensional analysis.
2. Algebra, functions, sequences, and parameterization.
3. Geometry, vectors, coordinate frames, and transformations.
4. Trigonometry, periodic functions, phase, and angular measures.
5. Calculus, derivatives, integrals, velocity, and acceleration.
6. Linear algebra, matrices, bases, eigenstructure, and least squares.
7. Differential equations, state, stability, and response.
8. Probability and statistics, distributions, uncertainty, and estimation.
9. Numerical analysis, discretization, error, convergence, and reproducibility.
10. Optimization and constrained problems.
11. Graph theory for procedure dependencies and activity structures.
12. Signal processing for sampled motion and temporal patterns.

## Physics spine

The Physics package owns the authoritative physical theory and implementation. Gestures consumes well-defined results and contributes requested procedures.

- Measurement, units, reference frames, and modeling assumptions.
- Classical mechanics: position, velocity, acceleration, momentum, and energy.
- Rigid-body motion and multibody systems.
- Constraints, joints, and articulated mechanisms.
- Contact, collision, friction, compliance, and impact.
- Fluids and environmental resistance, including air, water, and dense gases.
- Oscillation, damping, and driven response.
- Numerical integration, stability, and error.
- Biological movement as a measured, variable physical system.
- Limits of idealization and model validity.

The same Gesture may produce different achieved outcomes in air, water, or dense gas because environmental conditions alter the physical realization. Gestures must not hard-code one outcome into the procedure.

## Laboratory design standard

Every lab should expose:

- the question and learning objective;
- prerequisites and vocabulary;
- the mathematical model and assumptions;
- inputs, units, coordinate frames, and logical time;
- an executable implementation or clearly labeled conceptual model;
- expected observations and known failure modes;
- a change-it challenge;
- automated tests or a reproducible measurement protocol;
- reflection questions;
- a statement of what the lab does **not** prove.

A lab image is evidence only for the process that generated it. A conceptual SVG must be labeled conceptual. A scheduler trace proves scheduling output, not completed movement. A pose sample proves evaluator output, not physical success.

## Assessment model

- **Observation notebooks:** learners describe what they can actually see.
- **Derivation exercises:** learners explain equations, units, and assumptions.
- **Implementation exercises:** learners build small evaluators and validate edge cases.
- **Experiment reports:** learners compare predicted and observed results.
- **Code review:** learners identify ownership violations and unsupported claims.
- **Capstone defense:** learners state what is new, what is borrowed, what is measured, and what remains unknown.

## Textbook volume plan

1. **Volume I — Foundations of Gesture:** posture, time, coordinates, channels, definitions, and semantics.
2. **Volume II — Mathematics of Motion:** vectors, transforms, calculus, differential equations, numerical methods, and statistics.
3. **Volume III — Articulation and Kinematics:** target models, topology, joints, reachability, constraints, and inverse problems.
4. **Volume IV — Procedure and Individuality:** temporal composition, stable identity, variation, skill, and adaptation.
5. **Volume V — Interaction and Physical Realization:** contact, environment, feedback, response, and Physics integration.
6. **Volume VI — Coordination and Populations:** shared references, choreography, synchronization, cohorts, scaling, and fidelity.
7. **Volume VII — Research Methods:** measurement, reproducibility, falsification, literature review, and scientific communication.

## Publication quality gates

A chapter is ready for textbook review only when it has:

1. precise definitions and consistent notation;
2. a clear distinction between established science and proposed synthesis;
3. primary references and a bibliography;
4. derivations with units and assumptions;
5. examples at more than one level of complexity;
6. exercises with answer criteria;
7. an executable or reproducible laboratory where appropriate;
8. explicit limitations and common misconceptions;
9. review by someone competent in the relevant discipline.

The goal is not to inflate a software README into a textbook. The goal is a coherent, cumulative educational program whose concepts are precise enough to implement, test, challenge, and teach.
