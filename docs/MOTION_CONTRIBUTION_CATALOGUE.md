# Motion contribution catalogue (research contract)

This catalogue is a working contract for investigating composable motion. It is **not** a runtime API, a validated universal equation, or a claim that Gestures currently solves physical contact. Keep each contribution independently defined and testable before composing them.

Related research: [Motion model experiment plan](MOTION_MODEL_EXPERIMENT_PLAN.md) and [Motion equation exploration](MOTION_EQUATION_EXPLORATION.md).

## 1. Contract every contribution

Each proposed contribution must declare:

1. **Domain:** the coordinate(s), body, joint, or contact to which it applies.
2. **Input state:** every required state variable and its units.
3. **Output type:** force, torque, constraint, impulse, kinematic target, or control command. These types are not interchangeable.
4. **Reference frame:** world, body-local, joint-local, or contact frame; define transformations explicitly.
5. **Activation:** always active, state-selected, or event-triggered; identify the state source.
6. **Gate semantics:** whether it can be excluded, scaled, or only selected as a discrete mode.
7. **Validity range:** assumptions, limits, and undefined/invalid states.
8. **Energy and stability expectations:** whether the term stores, dissipates, or supplies energy, and under what assumptions.
9. **Numerical contract:** update order, timestep assumptions, and whether the term depends on solver iteration.
10. **Tests:** unit/dimensional checks, limiting cases, deterministic tests where applicable, and interaction tests.

A contribution that cannot meet this contract remains a named hypothesis, not a production-ready physics term.

## 2. Candidate catalogue

| Contribution | Illustrative model | Output and units | Gate policy | Main caveat |
|---|---|---|---|---|
| Inertial response | \\(\\mathbf M(\\mathbf q)\\ddot{\\mathbf q}\\) | Inertial term; formulation depends on generalized coordinates | Not a user-facing force blend | Configuration-dependent inertia and coupled coordinates require a consistent dynamics formulation |
| Linear restoring response | \\(F=-k(x-r)\\) | Force (N) | May scale only when an effective stiffness interpretation remains valid | Local linear approximation; define the target and valid displacement range |
| Viscous damping | \\(F=-c v_{rel}\\) | Force (N) | Scaling changes effective dissipation | State whether velocity is absolute or relative to target/contact |
| External drive | \\(F_{drive}(t)\\) | Force (N), or explicitly typed control output | May be scaled if the actuator/control model defines what scale means | Do not silently treat a procedural command as a physical force |
| Contact normal response | Solver-specific unilateral contact constraint | Constraint force or impulse | Usually mode/constraint-selected, not a generic continuous blend | Must prevent invalid penetration according to the solver's declared tolerance/model |
| Tangential contact friction | Static/kinetic friction model | Tangential force or impulse | Select regime from relative slip and contact state; avoid arbitrary universal blending | Coefficients and transition behavior depend on model assumptions |
| Impact | Impulse/contact event model | Impulse (N·s) | Event/mode selected | A finite impulse is not an ordinary force sample; event timing and restitution require explicit definitions |
| Environmental/material state | Thermal/phase/material transition rule | State update, not a force | State-transition policy | Requires explicit assumptions about heat, pressure, composition, and transition hysteresis where modeled |
| Kinematic target/easing | Prescribed (x(t)) or pose path | Position/pose target | Selects a motion mode; not an applied force by itself | Does not by itself conserve momentum or respond physically to contact |

The table is deliberately heterogeneous: it makes clear that a useful motion architecture contains more than additive force terms. Some entries produce forces; others provide constraints, impulses, state changes, or targets. A future solver must connect these through declared contracts rather than sum unlike quantities.

## 3. Gate and mode vocabulary

Use precise vocabulary in APIs and documentation:

- **Disabled:** contribution is not evaluated or applied.
- **Enabled:** contribution is evaluated according to its contract.
- **Scaled:** a dimensionless factor modifies a contribution only where the model defines this operation.
- **Mode-selected:** one of several mutually exclusive or state-dependent models is chosen.
- **Constrained:** the solver enforces a condition, potentially through coupled equations or impulses.
- **Event-triggered:** a transition such as impact occurs at a defined event or solver-detected crossing.

Do not call every one of these a “weight.” A weight can suggest arbitrary interpolation even where the correct behavior is a discrete regime change or constraint solve. A zero scale is meaningful for a defined force contribution, but disabling a required constraint can invalidate the simulation.

## 4. Worked causal chain: freezing surface

This is a future integration scenario, not a current Gestures feature.

```text
Environment / thermal assumptions
          |
          v
Material phase state (liquid / transition / solid)
          |
          v
Contact material parameters (e.g. friction model inputs)
          |
          v
Contact solver (normal + tangential response)
          |
          v
Body acceleration / velocity / pose
```

The test should verify the causal chain rather than merely check that a character plays a slip animation:

1. Under documented initial conditions, changing thermal state changes phase/material state according to the chosen simplified or full model.
2. The phase/material state selects contact parameters; no character-specific animation flag is required.
3. With geometry, normal load, and initial velocity held constant, changing contact parameters changes the predicted tangential response in the expected direction.
4. Different bodies interacting with the same surface follow the same material/contact rules, while their different masses, inertias, constraints, and initial states can yield different motion.
5. Repeated runs with identical inputs, solver configuration, and initial state meet the declared repeatability tolerance.

Avoid baking in a universal rule that “below 32°F means instant ice.” Pure water's freezing point is near 0°C (32°F) at ordinary atmospheric pressure, but a physically meaningful phase simulation may also require heat transfer, latent heat, pressure, dissolved substances, nucleation/supercooling, and time. The model must state which of these it represents and which it deliberately omits.

Likewise, do not hard-code friction as zero on ice. Use explicit contact assumptions and material parameters; static friction, kinetic friction, lubrication, surface roughness, temperature, speed, and solver behavior can all matter. A simple Coulomb approximation can be a useful first experiment, not a universal description of ice.

## 5. Minimum validation matrix

| Test | Setup | Expected evidence |
|---|---|---|
| Dimensional consistency | Inspect every equation and parameter | Both sides of each equation have compatible units |
| Zero contribution | Disable an optional applied-force term | Output matches the baseline within declared tolerance |
| Gate semantics | Compare disabled, enabled, and supported scaled cases | Results match the documented interpretation of each state |
| Static friction limit | Apply tangential load below/above the declared static limit | Contact remains stuck below the limit and transitions according to the selected model above it |
| Kinetic friction direction | Start with nonzero tangential relative velocity | Friction opposes relative slip under the model's sign convention |
| Contact loss | Separate bodies after contact | No stale contact force persists beyond the declared solver policy |
| Phase transition | Drive a documented thermal scenario across the chosen threshold/model | Material state changes according to the specified transition assumptions |
| Causal integration | Change phase state while keeping other inputs controlled | Contact response changes through material/contact inputs, not an animation special case |
| Energy accounting | Run isolated spring/damping/contact cases | Stored, dissipated, and supplied energy behave consistently with each term's assumptions |
| Timestep sensitivity | Repeat with multiple declared timesteps | Error/stability trend is measured and documented |
| Determinism | Repeat exact initial state and inputs | Output satisfies the chosen exact or tolerance-based repeatability contract |

## 6. Non-goals and current boundary

This document does not specify a complete rigid-body or articulated-body solver. It does not select a final public API, prescribe a universal friction law, or reconstruct the lost historical equation. The current Gestures evaluator remains a keyframe-oriented motion evaluator; it does not simulate temperature, phase transitions, contact friction, impact, or generalized multi-body dynamics.

The next evidence-driven step is to choose **one** contribution with a narrow domain, implement it in an isolated reference experiment, and test its contract. Contact friction is not a sensible first implementation until there is an explicit contact state/normal-response model; the scalar spring-damper experiment is a simpler initial numerical test bed.
