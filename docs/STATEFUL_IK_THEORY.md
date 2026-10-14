# Stateful IK: a research direction for motion as a living process

## Why this is different

Conventional inverse kinematics (IK) is commonly introduced as a geometric question: given a target and an articulated chain, find joint values that place the end effector at that target. This is useful, but it is not the whole problem for a physically simulated system.

The Singularity Workshop's proposed **Stateful IK** starts from a different premise:

> A joint is not a disposable number produced by a solver. It is a stateful participant in a hierarchy, with a history, limits, motion intent, and physical consequences.

This document defines a research hypothesis and vocabulary, not a claim that the full system is already implemented or proven faster.

![Stateful IK hierarchy with local joint state and feedback from measured physical outcomes](assets/stateful-ik-hierarchy.svg)

*The animated diagram is a conceptual architecture sketch, not a runtime capture or benchmark result.*

## 1. Start with familiar animation

A conventional baked animation clip is often authored as a sequence of poses over time. At a given time, a player samples or blends pose data and presents the result. This can be efficient and predictable, but a baked pose does not by itself know whether the hand actually contacted the handle, whether the door was moved, whether the character's foot slipped, or whether a target became physically unreachable.

A procedural animation system can calculate poses from parameters and context instead of storing every final pose. IK is one such tool: it works backward from an endpoint goal to joint values.

Stateful IK asks what happens when that calculation becomes part of a continuing physical process. The requested goal is an intention or constraint, not a guarantee. The system retains the current joint condition, evaluates the next permitted action, observes the resulting pose/forces/contact outcomes when available, and continues from the new state.

This is not a claim that all conventional animation is baked, or that existing engines lack physics-aware tools. It is a contrast between a pose-sampling baseline and the stateful, physically accountable behavior we want to investigate.

![Comparison of sampled baked animation, procedural IK, and the proposed Stateful IK feedback loop](assets/animation-models.svg)

*The diagram explains conceptual differences; it is not a benchmark and does not imply that the three approaches are mutually exclusive.*

## 2. The joint as a small state machine

A useful first abstraction classifies a joint's movement direction into three motion states:

- **MovingPositive** — motion is proceeding in the joint's chosen positive direction.
- **MovingNegative** — motion is proceeding in the opposite direction.
- **Stationary** — no meaningful movement is occurring within a chosen tolerance.

These are *motion classifications*, not a complete physical state. The joint still needs whatever variables its model requires: angle or transform, angular velocity, allowed range, target or intent, torque/force limits, contact information, health/damage state where relevant, and the timestamp or simulation step of its last update.

The sign convention must be local and explicit. “Positive” means increasing the chosen joint coordinate, not universally “left,” “up,” or “forward.”

A three-state direction classification can make transitions compact and inspectable. It does not automatically make the entire simulation three-state, eliminate floating-point work, or prove better performance. Position, velocity, constraints, and physical integration still cost time.

## 3. A hierarchy of joint FSMs

An articulated body can be represented as a graph or hierarchy of joints and links. Each joint owns local state and constraints; parent transforms define frames in which child joints are interpreted. A hierarchy lets a change at a parent affect the world-space placement of descendants without requiring each descendant to independently rediscover the whole body's pose.

A possible evaluation flow is:

1. Read the requested action or target and current simulation state.
2. Determine which joints or subtrees may be affected.
3. Evaluate local intent, legal motion, and relevant constraints.
4. Update the affected joint state and propagate transform changes through descendants.
5. Evaluate contacts and physical consequences in the owning physics system.
6. Report whether the requested goal was achieved, approximated, blocked, deferred, or invalid.
7. Preserve enough state for the next step; do not silently reset history each frame.

The hierarchy is not necessarily a humanoid skeleton. A vehicle suspension, robotic mechanism, articulated tool, creature, or procedural assembly can all have different joint topologies and constraints.

“Thousands of joint FSMs” is a workload hypothesis to test. It raises important architecture questions: compact data layout, cache locality, grouping compatible joints, sparse versus dense updates, dependency ordering, parallel batches, and how to avoid synchronization overhead. The count alone does not establish that one FSM per joint is the best implementation.

## 4. Goals are requests; outcomes are measured

A target may be geometrically unreachable. It may be reachable geometrically but disallowed by joint limits. It may be blocked by collision. It may be reachable only by losing balance or exceeding an actuator's force budget. In a physical model, those are different failure reasons and should not be collapsed into “IK failed.”

A responsible result should be able to distinguish at least:

- **Reached** — measured endpoint error is within a defined tolerance and required constraints are satisfied.
- **Approximate** — a legal candidate exists, but the requested target was not reached.
- **Blocked** — a collision, contact, or other explicit constraint prevents the requested motion.
- **Limited** — joint range, speed, acceleration, torque, or another modeled limit prevents further progress.
- **Unsafe** — the model predicts violation of a defined safety envelope. This status only has meaning when that envelope is actually modeled.
- **Invalid** — input or model state is not valid for evaluation.
- **Pending** — evaluation is incomplete or deferred.

Not every implementation needs every status immediately. The key is not to report success just because an optimizer returned numbers.

## 5. Geometry, control, and physics are separate layers

Stateful IK should not conflate three different questions:

**Geometry:** What poses satisfy the kinematic relationships and target constraints?

**Control:** Given the current state and desired motion, what change should be requested next, while respecting local limits and continuity?

**Physics:** Given forces, mass/inertia, contacts, collisions, and the simulation's integration rules, what actually happens?

A geometrically valid pose may be dynamically infeasible. A controller may request a move that the physics simulation cannot realize. A physics engine may resolve a collision by moving a body away from the ideal pose. The next IK/control step should account for that discrepancy rather than assuming the intended pose was achieved.

Damage or injury is a domain-specific consequence model, not an automatic property of IK. The system can only predict injury, wear, or structural damage to the extent that the relevant thresholds and mechanics are represented and validated.

## 6. Contact is an outcome, not a pose guarantee

Rock climbing makes the distinction between a requested pose and a physical outcome easy to see.

A climber's hand or foot may be directed toward a hold, but the target does not guarantee that the limb can reach it, that contact occurs, or that the contact can support the load. A useful model must represent the entire range of outcomes—from a stable, successful hold to a near miss, a blocked reach, a brief contact, and a slip.

A candidate contact loop is:

1. **Approach:** Treat the hold as a desired contact region, with tolerances and approach constraints, rather than a guaranteed endpoint.
2. **Reach:** Evaluate the available joint range and legal motion. If the target is out of reach, retain the closest legal result and report the remaining error.
3. **Establish contact:** Let collision/contact evaluation determine whether the hand or foot actually touches the surface. Being visually close is not proof of contact.
4. **Evaluate support:** Use the physical model's contact forces, friction limits, load, and any modeled grip or adhesion mechanics to determine whether the contact can hold.
5. **Respond:** If the support is insufficient, permit sliding or loss of contact. Feed the measured outcome back into the next control step and the affected hierarchy.
6. **Recover or fail:** The character may adjust another limb, shift its center of mass, find another hold, or fall. Those are outcomes of the modeled body and environment—not success states to fabricate in the IK layer.

In ordinary solid-contact models, grip is not a vague scalar called “surface tension”: support depends on the contact model and its assumptions, including normal force and friction. If a domain models special adhesion, that mechanism must be specified separately. The model should only claim the behavior it actually computes.

This separates four related but different questions:

- **Kinematic feasibility:** Can the limb's geometry and joint limits reach the contact region?
- **Contact validity:** Is there a real contact under the collision/contact rules?
- **Load-bearing stability:** Can the contact forces support the current load without violating the model's friction, force, or stability limits?
- **Whole-body response:** What happens to the rest of the body when the contact holds, moves, or disappears?

A kinematically reachable pose can still be physically unsustainable. Conversely, an unreachable target does not mean the whole action is useless: a best-effort approach, partial reach, or alternate contact may still be meaningful. Report the actual result and its constraints instead of collapsing every outcome into a binary “IK succeeded/failed.”

The same principle applies beyond climbing: a foot can touch a moving vehicle without having a stable foothold; a robot gripper can reach an object without securing it; a mechanism can reach a position but stall under load. Stateful IK should preserve the difference between intent, pose, contact, and consequence.

*Scope boundary:* IK alone does not calculate friction, grip, balance, falling, injury, or damage. Those outcomes require a defined physical/domain model. The research goal is to connect the layers without pretending that one layer already implements the others.

## 7. Why history may help

A memoryless solver typically derives a pose from current inputs and a target. A stateful controller can additionally use previous joint state, direction, velocity, the prior solution, and the reason progress stopped.

Potential benefits to test include:

- **Continuity:** reduce sudden flips between equivalent elbow configurations.
- **Incremental work:** avoid reevaluating unchanged subtrees when dependencies prove they are unaffected.
- **Early stopping:** stop a joint's update when its state and error are within a stable tolerance, while still honoring required physics steps.
- **Explicit transitions:** treat start, reverse, stop, blocked, release, and resume as meaningful state changes.
- **Failure awareness:** carry an unmet-target condition forward rather than reasserting an impossible ideal pose every frame.
- **Locality:** process nearby or compatible joints in batches that fit the data layout and execution backend.

These are hypotheses, not free optimizations. Cached state can become stale; sparse updates can miss dependencies; early stopping can cause drift; continuity can preserve the wrong local minimum; and extra state can cost more memory and branching than it saves. Each proposed optimization needs an invariant, a correctness test, and a benchmark.

## 8. A candidate joint transition model

A first motion-direction state machine might look like this:

```text
                    requested positive motion
           +--------------------------------------+
           |                                      v
     +-----------+   positive velocity       +----------------+
     | Stationary| ------------------------> | MovingPositive |
     +-----------+                           +----------------+
           ^                                      |
           | velocity settles / stop              | intent reverses
           |                                      v
     +-----------+   negative velocity       +----------------+
     | Stationary| <------------------------ | MovingNegative |
     +-----------+                           +----------------+
```

This diagram is conceptual. Actual transitions need hysteresis or tolerances so tiny numerical fluctuations do not cause rapid state flicker. A real implementation must also define what happens at a limit, on collision, when intent changes, when the parent moves, and when physics produces motion contrary to the requested direction.

Do not confuse this three-state direction classifier with the entire joint FSM. Events and orthogonal conditions may be better represented separately than by exploding every combination into a giant enumerated state.

## 9. From an IK request to a physical result

A candidate loop for research is:

```text
Action intent / target
        |
        v
Reachability + constraint assessment
        |
        v
Select affected joint hierarchy
        |
        v
Local state transitions and bounded motion requests
        |
        v
Physics / collision / contact integration
        |
        v
Measure achieved pose, error, and physical outcomes
        |
        +----> preserve state for the next simulation step
        |
        +----> report reached / approximate / blocked / limited
```

The exact ownership boundaries matter. Gestures can define procedural action and rig-independent intent. A joint-state/IK layer can own its algorithm and state model if the package boundaries support that. The domain physics owner remains authoritative for forces, collision, and damage. Rendering consumes a representation of the result; it should not be mistaken for the source of physical truth.

No dependency on FSM_API should be added merely to make the name sound aligned. First establish whether the existing FSM_API can express the required local transitions and what its actual overhead and semantics are. Then compare reuse against a small purpose-built data-oriented state representation using real tests.

## 10. Performance: make a falsifiable claim

The hypothesis is not “three states are faster.” A useful claim would be:

> For a specified articulated workload and correctness tolerance, retaining joint state and updating only affected dependencies reduces total evaluation cost versus a specified baseline, without unacceptable pose error, instability, or missed physical constraints.

Measure at least:

- total time per simulation step, including hierarchy propagation and physics hand-off;
- throughput and tail latency at several joint counts;
- allocations and memory footprint;
- percentage of joints/subtrees actually updated;
- transition count and constraint checks;
- error versus a fully evaluated reference;
- behavior under reversals, collisions, unreachable targets, and rapidly changing parents;
- single-threaded CPU, batched CPU, and GPU candidates where applicable;
- cold start and steady state separately.

Compare at equal quality and identical scenarios. Include simple cases where a conventional full solve is expected to win. GPU transfer/dispatch and synchronization belong in the total cost. Thousands of joints may offer parallelism, but hierarchy dependencies can limit it.

## 11. Suggested research sequence

1. **Write the contract:** define coordinates, local axes, parent/child frames, units, tolerances, joint types, and result statuses.
2. **Keep the planar CPU solver as a baseline:** it tests geometry and unreachable-target reporting, but it is not the Stateful IK implementation.
3. **Prototype a single stateful hinge:** use deterministic inputs and explicit positive/negative/stationary transitions.
4. **Build a small hierarchy:** test parent movement, child limits, reversed intent, unreachable goals, and transform propagation.
5. **Introduce a physical boundary:** feed requests to a simple, testable physics adapter; keep actual contacts and forces authoritative there.
6. **Add correctness oracles:** compare incremental evaluation to full reevaluation after every generated event sequence.
7. **Scale the workload:** benchmark 1, 10, 100, 1,000, and 10,000 joints, plus representative branched hierarchies.
8. **Only then optimize:** data-oriented storage, subtree invalidation, batching, parallel execution, and GPU evaluation are candidates, not assumptions.
9. **Test adversarial cases:** unreachable targets, joint limits, collision, external impulses, reversals, near-zero velocities, and state invalidation.
10. **Document evidence:** publish results, methodology, hardware/runtime details, quality tolerances, and failed hypotheses alongside any speedup claim.

## 12. Relationship to the current implementation

The current PlanarKinematics implementation is a deliberately small, engine-neutral two-link CPU reference. It calculates FK, generates two IK candidates, constrains those candidates, and measures endpoint error. That solver itself has no temporal memory, hierarchical rig evaluator, collision model, physics integration, damage model, or GPU implementation.

A separate [Stateful IK Prototype](STATEFUL_IK_PROTOTYPE.md) now demonstrates a retained hinge, a speed-limited step toward intent, the three movement classifications, joint-range enforcement, explicit reporting when a requested target remains unmet, and a planar parent/child hierarchy. The hierarchy currently performs full reevaluation so it can serve as a correctness baseline; incremental invalidation and physical integration remain open work.

These small experiments establish testable building blocks, not proof of the complete Stateful IK architecture or a performance advantage.

## 13. Open questions, not hidden assumptions

- Are joint FSMs the actual storage model, or the conceptual model compiled into a compact array?
- Which state is persistent and authoritative: angle, transform, velocity, intent, motion class, constraint status, or some combination?
- How do parent changes invalidate descendants, and can that invalidation be sparse without missing dependencies?
- Which constraints are kinematic, which belong to collision, and which belong to the physics solver?
- How is stability measured? What constitutes an unsafe pose for each domain?
- Does a joint stop updating when stationary, or must it still be checked for parent movement, external force, and contact?
- Can the same state model serve characters, vehicles, and mechanisms without forcing a universal rig?
- What measured threshold justifies CPU batching, GPU dispatch, or lower fidelity?
- How should simulation authority and visual approximation differ at distance or under load?

These questions should be answered with executable examples and evidence, not by treating a proposed architecture as already proven.

---

## Further reading

- [Inverse Kinematics Theory](INVERSE_KINEMATICS_THEORY.md) — geometric foundation and the existing planar CPU reference.
- [Planar Kinematics Reference](PLANAR_KINEMATICS_REFERENCE.md) — exact current implementation boundary.
- [Modern Robotics — Inverse Kinematics](https://modernrobotics.northwestern.edu/chapters/chapter6/) — geometric and iterative IK foundations.
- [The Robotics Book](https://www.roboticsbook.org/) — open robotics educational text.

*The Singularity Workshop — model reality's constraints; do not hide them behind an ideal pose.*
