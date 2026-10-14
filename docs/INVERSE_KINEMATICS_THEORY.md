# Inverse kinematics, from intuition to equations

Inverse kinematics (IK) answers a deceptively simple question:

> **Given a desired position for the end of a connected structure, what joint angles could put it there?**

If you reach for a cup, you usually think about the cup—not the exact angle of your shoulder, elbow, and wrist. A motion system faces the reverse problem: it is given a goal and must find joint values that make the structure reach toward it.

This page builds that idea from geometry before introducing the implementation. You do not need a robotics background to follow it.

![Animated two-link arm: joint angles shape the endpoint while the goal marks the requested position](assets/ik-intuition.svg)

*The animation is a geometry illustration. It is not a capture from the Gestures solver; the SVG animates its own demonstration pose.*

## 1. Forward and inverse are different questions

Imagine two rigid links joined by a hinge.

- **Forward kinematics (FK):** you know the link lengths and joint angles. Calculate where the endpoint is.
- **Inverse kinematics (IK):** you know the link lengths and desired endpoint. Find joint angles that place the endpoint there.

FK is a direct calculation: the angles determine the pose. IK works backward from a goal, and that reverse question can have no answer, one answer, or multiple answers.

| Given | Find | Question |
|---|---|---|
| Link lengths + joint angles | Endpoint position | “Where is the hand?” |
| Link lengths + target position | Joint angles | “How could the hand get there?” |

## 2. Start with a two-link arm

For a deliberately small reference model, we use:

- A fixed anchor at the origin \\((0,0)\\).
- A first link of length \\(L_1\\).
- A second link of length \\(L_2\\).
- A shoulder angle \\(\\theta_1\\), measured from the positive X axis.
- An elbow angle \\(\\theta_2\\), measured relative to the first link.
- A target point \\(T=(x,y)\\).

The elbow is the end of the first link. The end effector—the “hand” in this example—is the end of the second link.

![Forward geometry of a two-link chain and its reachable region](assets/ik-reachability.svg)

## 3. Forward kinematics: angles become a position

The elbow position is

\\[
E_x=L_1\\cos(\\theta_1),\\qquad
E_y=L_1\\sin(\\theta_1)
\\]

The second link uses the combined orientation \\(\\theta_1+\\theta_2\\), so the endpoint is

\\[
x=L_1\\cos(\\theta_1)+L_2\\cos(\\theta_1+\\theta_2)
\\]

\\[
y=L_1\\sin(\\theta_1)+L_2\\sin(\\theta_1+\\theta_2)
\\]

That is the FK calculation implemented by `PlanarKinematics.ForwardKinematics`. It gives us a trustworthy way to check any candidate joint angles: calculate where they actually put the endpoint.

## 4. Inverse kinematics: a target becomes possible angles

Let the target distance from the anchor be

\\[
r=\\sqrt{x^2+y^2}
\\]

The links and the anchor form a triangle. The law of cosines gives the elbow angle:

\\[
\\cos(\\theta_2)=
\\frac{x^2+y^2-L_1^2-L_2^2}{2L_1L_2}
\\]

For an unconstrained, reachable target, the two possible elbow configurations are

\\[
\\theta_2=\\pm\\arccos\\left(
\\frac{x^2+y^2-L_1^2-L_2^2}{2L_1L_2}
\\right)
\\]

These are often described as the two “elbow-up / elbow-down” solutions. The shoulder angle can then be found from the target direction and the triangle's internal angle:

\\[
\\theta_1=\\operatorname{atan2}(y,x)-
\\operatorname{atan2}(L_2\\sin\\theta_2,
L_1+L_2\\cos\\theta_2)
\\]

The signs and visual names depend on the coordinate convention. The important point is that **a target may correspond to more than one valid pose**.

The cosine expression can drift slightly outside \\([-1,1]\\) due to floating-point rounding, or because a target is outside the arm's reach. Clamping it keeps `acos` numerically defined, but does **not** make an unreachable target reachable. The resulting endpoint must still be calculated and checked against the requested target.

## 5. Reach is a real limit

With no joint restrictions, a two-link arm cannot extend farther than \\(L_1+L_2\\) from its anchor. Its nearest possible distance is \\(\\lvert L_1-L_2\\rvert\\). These bounds describe an annulus when both links can rotate freely.

Real mechanisms add more limits: a hinge may only rotate through a particular range, a shoulder may be blocked by the body, or a linkage may be forbidden from folding through itself. The target can be inside the simple reach envelope and still be impossible under those joint constraints.

A responsible solver must therefore distinguish:

- **Reached:** the computed endpoint is within the stated numerical tolerance of the target.
- **Not reached:** the returned pose is a valid candidate, but its endpoint remains some measurable distance from the goal.
- **Invalid input:** the geometry or limits do not define a meaningful calculation.

Never label a clamped, approximate, or best-effort pose as a successful exact solve without checking the resulting endpoint.

## 6. Joint limits and candidate selection

This repository's initial reference is intentionally modest. It evaluates both unconstrained elbow configurations, maps equivalent angles into the caller's angle ranges where possible, clamps the candidate angles, and then runs FK again to measure the actual endpoint error. It returns the candidate with the smaller measured error.

This is a deterministic **two-candidate reference**, not a globally optimal constrained IK algorithm. Independent clamping can miss a better pose elsewhere in the legal joint ranges. More advanced methods can search feasible ranges, minimize a cost function, preserve continuity from a previous pose, or solve a hierarchy of joints.

That distinction matters: a small honest baseline is more useful than calling an incomplete algorithm “general IK.”

## 7. What about more joints and three dimensions?

The same question scales up, but the mathematics and trade-offs change.

- A multi-joint chain has more degrees of freedom and may have many solutions.
- A target may specify position only, or position plus orientation.
- A 3D rig needs a coordinate-frame and hierarchy contract, not just a 2D point.
- Collision avoidance and self-intersection are separate constraints.
- Dynamics, inertia, contact forces, and physical balance are not provided by geometric IK alone.

Methods such as Jacobian-based solvers, cyclic coordinate descent (CCD), and FABRIK offer different ways to handle larger chains. They should not be treated as interchangeable magic: each has assumptions, convergence behavior, and costs. The current Gestures reference does **not** implement these methods.

## 8. Why begin on the CPU if the goal is GPU-first?

A CPU reference provides a readable mathematical contract and an oracle for tests. A later batched implementation can be compared against it using the same inputs, tolerance rules, limits, and failure cases.

GPU work becomes worthwhile when the workload has enough parallel work to offset dispatch, synchronization, memory traffic, and data transfer. A single small arm may be cheaper on the CPU; thousands of independent chains may present a different profile. Measure the complete workload instead of assuming that the GPU label guarantees a speedup.

The intended progression is:

1. Define the geometry and angle conventions.
2. Test FK and IK, including impossible targets and invalid inputs.
3. Establish numeric tolerances and representative test vectors.
4. Batch real workloads and measure CPU cost.
5. Prototype GPU evaluation and compare both results and end-to-end cost.
6. Add fidelity tiers only when their approximations and transitions can be verified.

## 9. Try the reference

The implementation is in [`PlanarKinematics.cs`](../src/TheSingularityWorkshop.Gestures/PlanarKinematics.cs), with executable examples in [`PlanarKinematicsTests.cs`](../tests/TheSingularityWorkshop.Gestures.Tests/PlanarKinematicsTests.cs).

The related [Planar Kinematics Reference](PLANAR_KINEMATICS_REFERENCE.md) documents the implemented API and its boundary. The tests are the executable contract; this theory page is the guided explanation. If either disagrees with the other, resolve that discrepancy rather than asking readers to guess.

## Further reading

These are starting points for deeper study, not dependencies of this package:

- [Modern Robotics — Chapter 6: Inverse Kinematics](https://modernrobotics.northwestern.edu/chapters/chapter6/) — a rigorous introduction to robot kinematics and iterative methods.
- [The Robotics Book](https://www.roboticsbook.org/) — open educational material covering robotics foundations.
- [FABRIK: A fast, iterative solver for the Inverse Kinematics problem](https://doi.org/10.1016/j.gmod.2011.05.003) — a published approach for articulated chains.

---

*The Singularity Workshop — edify, not mystify. Explain the boundary as carefully as the capability.*
