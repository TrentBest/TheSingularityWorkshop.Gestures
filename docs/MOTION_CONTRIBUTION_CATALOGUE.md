# Motion contribution catalogue (research contract)

![Living conceptual map showing how inertia, damping, stiffness, target intent, and individual parameters relate](assets/motion-equation-map.svg)


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

## 6. Gravity and environmental acceleration

Gravity belongs in the world/environment model as an acceleration field (or, in a force-based formulation, as the corresponding mass-dependent force). It must not be silently baked into the target pose or assumed to point toward a fixed global floor.

For a point mass in a declared inertial frame, a minimal model is:

```text
a_total = g(position, time) + sum(F_applied) / mass
```

This is a teaching-level translational form, not the complete equation for an articulated body. A multi-body solver must account for generalized coordinates, coupled inertia, joint forces, angular dynamics, and constraints consistently.

The environmental input should support, at minimum, a uniform field as a reference case and a zero vector as the zero-gravity limit. Low gravity scales the field; it does not scale away mass, momentum, inertia, or the effects of an impulse. If the frame is rotating or accelerating, apparent/inertial forces require an explicitly declared non-inertial-frame model rather than being smuggled into ordinary gravity.

**Required checks**
- With no other forces or constraints, a free body follows the declared constant-gravity solution within numerical tolerance.
- Changing gravity magnitude changes acceleration, not instantaneous velocity.
- Setting gravity to zero removes gravitational acceleration while preserving the same initial momentum and other contributions.
- The same environmental field applies consistently to all bodies in its domain.
- Units and coordinate-frame conventions are explicit; changing the up-axis must not silently change the physical field.

## 7. Disturbance, articulated response, locomotion, and recovery scenarios

These are target scenarios for the future method, not features implemented by the current evaluator. They deliberately span different output types and should not be collapsed into one additive “motion weight.”

### 7.1 Explosion or other sudden disturbance

Represent a blast as either a time-varying external force or a bounded impulse, with application point, direction, duration/profile, and affected bodies declared. An impulse changes momentum over an event; it is not just a very large force sample. An off-center impulse also changes angular momentum. The articulated body's resulting motion depends on mass distribution, joint constraints, contact, and any active joint control.

A convincing result needs two separately testable layers:
1. **Passive physical response:** the disturbance changes linear/angular momentum and the body reacts through articulated dynamics and constraints.
2. **Active response/recovery:** a controller uses the current estimated state and task objective to command joint torques or other actuator outputs. It must be allowed to fail when authority, time, contact, or energy is insufficient.

Do not prescribe a universal arm-and-leg “flail” animation. Limb motion should emerge from the chosen passive model and controller, with any procedural variation bounded and clearly identified as control policy rather than physical law.

### 7.2 Abrupt braking and direction changes

A locomotion target may request a new velocity or heading immediately; the body's physical velocity must not teleport to that target. The controller requests forces/torques, while contact and friction limits determine what the ground can actually transmit. If the requested tangential force exceeds the model's available traction, slip should occur. If the body cannot redirect its momentum quickly enough, the path curves or the body loses balance.

The experiment should distinguish:
- desired velocity/heading (a target or control objective);
- achieved acceleration (result of forces, torque, inertia, and constraints);
- available traction (contact/material/load dependent);
- balance/recovery (a controller objective with finite capability).

A hard 90-degree change can be a desired command, but not an instantaneous change in velocity under finite forces. Any arcade-style exception must be an explicit alternative mode, not a hidden violation in the physics path.

### 7.3 Zero-g recovery and exoskeleton thrusters

In zero-g there may be no floor to push against. Joint motion alone can reorient a free-floating body and its limbs relative to each other, but it cannot change the total linear momentum of the complete isolated system; internal actuation cannot propel the center of mass arbitrarily. To translate the soldier back toward a contact surface, the system needs an external interaction, such as a thruster expelling reaction mass, a tether, or contact with an object.

Define thruster authority explicitly: force/impulse, direction, moment arm, fuel or energy budget if modeled, activation delay, and control limits. Then test two distinct objectives:
- **attitude recovery:** orient the body and limbs toward a desired configuration;
- **translation/recontact:** change center-of-mass motion so the soldier can reach a specified surface, without assuming a surface is reachable or that the available thrust is sufficient.

When contact becomes available, contact constraints and friction provide a new control opportunity. The controller must transition between free flight and contact-aware behavior without a discontinuous pose snap.

### 7.4 Extreme speed and “Flash” cases

Keep requested motion, physically achieved motion, and presentation/visual effects separate. For game-scale high-speed movement, test acceleration, braking distance, turn radius, collision detection, numerical stability, and timestep sensitivity. A target speed is not a guarantee that the body can reach or change it instantly.

Declare the model's validity regime. Newtonian mechanics is a reasonable starting point for ordinary game speeds. If a scenario explicitly approaches relativistic speeds, classical equations are no longer sufficient; a relativistic model would be a separate physical regime, not a slider on the same unqualified equation. Likewise, any superhuman actuator strength, energy source, or injury immunity should be declared as a world/character assumption rather than disguised as ordinary physics.

### 7.5 Scenario-level acceptance matrix

| Scenario | Controlled input | Required observable | Failure modes to preserve |
|---|---|---|---|
| Normal / low / zero gravity | Same initial state, vary only `g` | Acceleration changes with field; zero-g preserves inertial motion | No gravity baked into pose or assumed floor direction |
| Off-center blast | Same body, vary impulse point/direction | Linear and angular momentum respond consistently | No canned pose reset; no force/impulse unit confusion |
| Articulated blast recovery | Same disturbance, compare passive and active control | Joint response and controller effort are separately observable | Controller may fail if torque, time, or contact is insufficient |
| Sudden braking | Same speed, vary traction and actuator limit | Stopping distance and slip change plausibly | No instantaneous velocity cancellation |
| Sharp turn | Same incoming momentum, vary available lateral traction | Finite turn radius or slip/loss of balance when limits are exceeded | No instantaneous heading-to-velocity snap |
| Zero-g thruster recovery | Same initial state, vary thrust budget and geometry | Attitude and center-of-mass translation are measured separately | No free translation from internal joint motion alone |
| Recontact / landing | Same approach, vary surface and contact conditions | Impact, joint response, contact stability, and recovery are measurable | No automatic snap-to-ground or guaranteed safe landing |
| Extreme-speed regime | Same target, vary speed, timestep, and collision resolution | Acceleration, turning, collision, and numerical error are measured | No undefined validity regime or silent arcade exceptions |

## 8. Research references

These references support the separation of articulated dynamics, contact/impact constraints, and active control. They are background material, not a claim that Gestures already implements these methods.

- [MIT Underactuated Robotics — Multi-Body Dynamics and Contact](https://underactuated.mit.edu/multibody.html): contact, constraints, collision events, and friction.
- [Practical Physics for Articulated Characters (GDC slides)](https://media.gdcvault.com/gdc04/slides/practical_physics_for.pdf): articulated contact, impact constraints, friction, joint limits, and motion control.
- [Interactive Physics-Based Control of Articulated Characters](https://publications.ri.cmu.edu/storage/publications/pub_files/pub4/kim_junggon_2008_1/kim_junggon_2008_1.pdf): why realistic articulated response depends on control as well as dynamics.
- [NVIDIA PhysX — Articulations](https://nvidia-omniverse.github.io/PhysX/physx/5.6.0/docs/Articulations.html): an implementation-oriented reference for articulation forces, gravity, external forces, constraints, and stability considerations.

## 9. Participant-owned equations and environmental interaction

The architectural target is not one equation that describes every object. It is a shared composition method in which each physical participant defines the constitutive law or equation appropriate to its own domain, while common mechanics provide the contracts for combining their effects.

Newton's second law is the deliberately small common foundation:

```text
F_net = m * a
```

For a point mass with a declared inertial frame, this can be rearranged to `a = F_net / m` when mass is nonzero. It does **not** tell us by itself which forces exist, how a ski grips snow, how a track deforms mud, how a joint actuates, or when dust becomes airborne. Those behaviors belong to separately defined models with explicit assumptions. For a rigid body or articulated system, angular/generalized dynamics and constraints must also be accounted for; do not misapply the point-mass equation as a complete solver.

### 9.1 Local ownership, shared contracts

Each participant/model should declare:
- its state and reference frame;
- the equation/law it owns and the conditions under which it applies;
- environmental and interaction inputs it requires;
- the type of output it contributes (force, torque, impulse, constraint, material/state update, or control command);
- its limits, units, and energy/stability expectations;
- what it reports so other models can respond without knowing its implementation.

Examples are a ski's ski–snow interaction law, a tank track's track–terrain interaction law, a thruster's force/impulse output, and a joint actuator's torque/control model. They can be composed because their interfaces are explicit—not because their equations are all identical. A model should not silently own another participant's behavior, and no central domain catalogue should have to enumerate every possible object type in advance.

Interaction is the bridge between participants. A contact pair can report contact geometry and relative motion, while the relevant surface/material models supply constitutive parameters and the contact solver enforces non-penetration and resolves normal/tangential response. Where an interaction transmits forces between two modeled bodies, its bookkeeping must preserve the chosen conservation and solver conventions; it must not apply the same interaction twice.

### 9.2 Environment as queried state, not object-specific scripting

A participant should receive environmental facts through a declared query/interface rather than contain bespoke rules such as `if tank then dust`. Depending on the model, useful facts may include:
- whether contact exists, plus contact point(s), surface normal(s), and the identities/material state of the contacted surface;
- relative velocity and tangential slip at the contact;
- normal load/contact impulse and local pressure where the contact model can provide them;
- surface type and physical state (for example, dry loose soil, wet cohesive mud, compacted dirt, grass, snow, or ice);
- moisture, temperature, particle/cohesion properties, and other state only when a chosen material model actually uses them.

A boolean `IsInContact` is a useful coarse gate, not a sufficient contact law. If it is false, a contact-dependent contribution should not invent traction or ground-generated particles. If it is true, the detailed contact state determines the response.

Environmental facts should describe the simulated world; the participant's own model decides how those facts affect it. A terrain/material model may change its parameters as water content or temperature changes. A track model may calculate slip and transmitted shear. A particle/debris model may emit particles only when its physical activation criteria are met. The tank does not need a special “make dust on dirt” instruction.

### 9.3 Effects as downstream consequences

Visual effects should consume physical interaction outputs rather than determine them. For example, dust emission could depend on a material's particle availability/erodibility, contact pressure or shear, relative slip speed, and a threshold or rate law. Wet cohesive mud may instead deform, adhere, and displace as clumps or spray; grass may bend or break without producing a dry-soil dust plume. These are hypotheses for material-specific models to validate, not universal rules implied by `F = ma`.

Keep the causal direction explicit:

```text
environment + participant state
             ↓
participant / interaction equations
             ↓
forces, impulses, constraints, material-state changes
             ↓
motion and contact outcomes
             ↓
secondary effects (dust, mud displacement, spray, debris)
             ↓
rendering
```

The simulation may use approximations and authored material parameters, but it must label them as such. A threshold or emission-rate heuristic is still a model decision that needs inputs and tests; it is not “physics did it” merely because it runs after the force calculation.

### 9.4 Acceptance tests for composability

- A new participant can register its own equation/model without adding a type-specific branch to a universal motion equation.
- With contact disabled, contact-dependent traction and contact-generated effects are absent.
- For identical contact geometry and relative state, changing only the surface's declared material properties changes the interaction according to that material law, not according to the name of the vehicle.
- Dry loose soil, wet cohesive mud, grass, and compacted dirt can produce different validated responses through material/interactions models; none is hard-coded as a tank-only visual behavior.
- Force/torque, impulse, constraint, material-state update, and visual-event outputs remain distinct types with explicit conversions only where physically justified.
- Secondary effects are reproducible from their declared physical inputs and policy; they cannot feed back into motion unless an explicit physical interaction (for example, particle drag or impact) is modeled.
- Adding a new participant or surface type does not require rewriting unrelated participants' equations.

## 10. Non-goals and current boundary

This document does not specify a complete rigid-body or articulated-body solver. It does not select a final public API, prescribe a universal friction law, or reconstruct the lost historical equation. The current Gestures evaluator remains a keyframe-oriented motion evaluator; it does not simulate temperature, phase transitions, contact friction, impact, or generalized multi-body dynamics.

The next evidence-driven step is to choose **one** contribution with a narrow domain, implement it in an isolated reference experiment, and test its contract. Contact friction is not a sensible first implementation until there is an explicit contact state/normal-response model; the scalar spring-damper experiment is a simpler initial numerical test bed.
