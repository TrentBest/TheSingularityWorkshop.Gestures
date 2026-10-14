# Motion Model Experiment Plan

> **Status: derivation and experiment plan. Not an implementation contract.** This document narrows the mathematical questions for the Gesture motion equation. It does not claim to recover the exact historical equation; that original derivation should be preserved if found.

## The question we are actually solving

A Gesture supplies an ideal procedure: a time-varying target pose path \(r(t)\), usually derived from keyframes and a normalized phase. The motion law determines how an individual responds to that path. The individual receives a stable parameter vector, sampled once from declared ranges using identity and context. Those parameters must not be re-randomized each frame.

This separates four things that should not be conflated:

1. **Procedure** — what action is intended and how its target changes over time.
2. **Dynamics** — how a moving part responds to the target, inertia, drive, and damping.
3. **Individual profile** — stable, bounded parameters for this performer and context.
4. **Evaluation policy** — how the state is evaluated over time and at a chosen fidelity.

The initial CPU evaluator implements keyframe interpolation and bounded deterministic speed/lateral variation. It does **not** currently implement a force solver, spring-damper integration, per-joint dynamics, or skill-based parameter ranges.

## Candidate A — direct kinematic easing

Given normalized phase \(u\in[0,1]\), choose an easing function \(E(u)\), then interpolate between poses:

\[
x(t)=x_0+(x_1-x_0)E(u)
\]

This is cheap, directly addressable at any time, and easy to test. It can express ease-in/ease-out, but the curve is authored rather than emerging from inertia and force. It is an important baseline, not a failed approach.

**Use when:** timing and shape matter more than dynamic response, or when deterministic absolute-time evaluation is the primary requirement.

## Candidate B — spring-damper tracking of a moving target

For one scalar coordinate, let \(x(t)\) be the actual coordinate and \(r(t)\) the desired coordinate from the Gesture path. Define the tracking error \(e=x-r\). A practical tracking force is proportional to position error and relative velocity:

\[
m\ddot{x}=k(r-x)+c(\dot r-\dot x)+F_{\mathrm{external}}
\]

Equivalently:

\[
m\ddot{x}= -ke-c\dot e+F_{\mathrm{external}}
\]

The second form describes the restoring and damping terms in terms of error. For a stationary target and no external force, this becomes the familiar free response:

\[
m\ddot e+c\dot e+ke=0
\]

The target can move, so the system may lag or overshoot while following a changing pose path. This is a feature to measure and tune, not an automatic guarantee of lifelike motion. A separate feed-forward term such as \(m\ddot r\) can compensate for target acceleration, but that changes the behavior and should be treated as another explicit model variant—not silently included.

**Use when:** the desired pose is a target the performer responds to, and lag, overshoot, and settling are useful characteristics.

## Make the parameters understandable

Sampling raw mass, damping, and stiffness independently can produce wildly different responses. Instead, explore a semantic parameterization using:

- \(m>0\): effective inertia;
- \(\omega_n>0\): undamped natural angular frequency, controlling response timescale;
- \(\zeta\ge0\): dimensionless damping ratio, controlling overshoot versus settling.

For the linear spring-damper model, derive stiffness and damping from those values:

\[
k=m\omega_n^2
\qquad
c=2\zeta m\omega_n
\]

This keeps the parameter ranges tied to recognizable behavior. For a constant target in the ideal linear model:

- \(0\le\zeta<1\): underdamped; oscillation/overshoot is possible;
- \(\zeta=1\): critically damped; the fastest non-oscillatory ideal response;
- \(\zeta>1\): overdamped; non-oscillatory but slower settling is possible.

These labels describe the ideal continuous-time linear system, not a guarantee about a discretely integrated, constrained, articulated body. Numerical step size, target discontinuities, joint limits, and coupling can change observed behavior.

A third useful semantic control may be **response scale** or **strength**, but it must have a defined mathematical meaning. Avoid exposing sliders named “springiness” or “weight” until their mapping to the model is documented and tested.

## Candidate C — driven dynamics

A more general Newtonian form is:

\[
m\ddot{x}=F_{\mathrm{drive}}(t,x,\dot{x})-c\dot{x}-k(x-r(t))
\]

This makes the intentional drive explicit. However, damping against absolute velocity and damping against target-relative velocity are different models. The implementation must choose deliberately:

- absolute-velocity damping can make the system settle relative to a world/rest frame;
- relative-velocity damping resists motion relative to the changing target;
- drive can represent active control, but its units and role must be specified.

For articulated movement, this is still only a one-coordinate teaching model. Rotations need angular quantities and a rotation-safe error representation; linked joints need coordination and constraints. Applying the scalar formula independently to every transform would not automatically produce a coherent body.

## How to define the individual's ranges

Let the ideal motion parameters be \(\theta_0\). A stable identity seed \(s\) and explicit profile/context \(p\) select a fixed parameter vector:

\[
\theta_i=\operatorname{Sample}(\theta_0,\text{bounds}(p),s)
\]

Sampling should happen at a declared boundary (for example, when an entity begins a Gesture), not on every frame. A replay record should be able to identify the definition version, seed, profile, sampled parameters, and time/evaluation policy.

**Prefer ranges on meaningful parameters.** For example, vary \(\omega_n\) and \(\zeta\), then derive \(k\) and \(c\), rather than independently varying all three. Use positive-preserving ranges for positive values. Related parameters may need correlated sampling so cadence, stride amplitude, and recovery remain plausible together.

**Skill is a policy, not a law of nature.** The design hypothesis is that inexperienced performers can have broader ranges and experienced performers tighter ranges around an ideal. That does not mean every parameter must shrink with skill. A profile should define which dimensions change and by how much; the hypothesis must be evaluated visually and quantitatively. “Veteran” should mean a narrower distribution where intended, not identical output.

## The minimum experiment

Before adding a public runtime API, implement a small offline/reference experiment for a single scalar coordinate. Use the same target trajectory and compare:

1. linear interpolation;
2. eased interpolation;
3. spring-damper tracking with low damping;
4. spring-damper tracking near critical damping;
5. spring-damper tracking with stronger damping;
6. optionally, target-acceleration feed-forward as a separately labelled variant.

For each run, record model variant, target curve, initial position and velocity, mass, \(\omega_n\), \(\zeta\), integration method, timestep, duration, and output samples. Then run a seeded population with narrow and wide parameter distributions.

Measure more than whether the curve “looks nice”:

- maximum tracking error;
- overshoot after a target change;
- settling time under a defined tolerance;
- peak acceleration and velocity;
- sensitivity to timestep;
- repeatability for the same recorded inputs;
- distribution width across identities;
- invalid or implausible parameter combinations.

Use the same seeds across profile comparisons to see how the distribution changes, while remembering that sampling the same seed under different bounds need not produce the same physical parameter values.

## Numerical evaluation is a first-class design decision

A stateful differential equation is not generally evaluated by asking for an arbitrary timestamp unless the solver reconstructs the state or uses a suitable closed-form solution. Incremental integration can depend on step size, ordering, and prior state. Therefore, do not promise identical results for arbitrary call order until the evaluation strategy supports it.

The experiment should compare fixed-step numerical integration against a reference solution for simple cases where one is available. Start with a known, stable test matrix before choosing an integrator. If CPU/GPU parity is a goal, define tolerances and compare outputs; matching source equations alone does not guarantee bitwise-identical results.

## Decisions deliberately left open

- Recover and preserve the original equation and derivation, if available, before calling a candidate the recreation.
- Decide whether a Gesture selects kinematic, dynamic, or hybrid motion.
- Decide whether the spring target is a pose path, a rest pose, or an intermediate trajectory.
- Define rotation and articulated-joint handling separately from the scalar derivation.
- Define how skill, load, fatigue, equipment, and terrain affect ranges.
- Choose a stateful, absolute-time, or hybrid evaluation contract only after the experiment.
- Keep engine, renderer, IK, and GPU integration out of the core mathematical experiment.

The success criterion is not “we have an equation.” It is: **we can explain each parameter, bound it safely, predict its qualitative effect, reproduce its output, and demonstrate that its variation creates coherent individual motion.**

---

*The Singularity Workshop — edify, not mystify.*
