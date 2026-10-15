# Star Port — Crew Routing and Departure Scenario

**Status: scenario design, not an implemented simulation.** This document describes a concrete consumer scenario for Gestures and the wider Workshop. It must not be read as a claim that Star Port, its routing system, the spacecraft physics, or a live Renderer integration already exists.

## The central idea

A Star Port is a coordinated procedure, not a single animation. Every soldier first follows the same waypoint directive to the spacecraft-access door, regardless of role. At that shared waypoint, a router evaluates the soldier's assigned role and sends that individual into the relevant provider-owned procedure.

The movement directive is common. The subsequent work is not.

- **Waypoint directive:** tells an agent where to move.
- **Router:** chooses the next route/procedure from authoritative assignment and current conditions.
- **Role provider:** supplies the task-specific procedure and its Gesture definitions.
- **Gesture evaluator:** evaluates procedural pose data into an `EvaluatedPose`.
- **Host rig adapter and Renderer:** map named local transforms to a real rig and present the result.
- **World/domain owners:** decide whether movement, contact, interaction, equipment state, access, occupancy, and task completion actually succeeded.
- **FSM_COS or another host orchestrator:** coordinates the overall procedure and waits on explicit outcomes; Gestures does not require FSM_API or FSM_COS as a core dependency.

## Procedure outline

```text
STAR PORT / muster
        |
        v
COMMON DIRECTIVE: move to spacecraft-access door
        |
        v
ROUTER: classify by assigned role + current task plan
        |
        +------------------------------+
        |                              |
        v                              v
GROUND-CREW ROUTE                 FLIGHT-CREW ROUTE
        |                              |
        v                              v
Task/provider assignments        Move to boarding-ramp waypoint
        |                              |
        v                              v
Service stations / equipment     Ramp access + boarding procedure
        |                              |
        v                              v
Verify each assigned task        Board + move to assigned station
        |                              |
        v                              v
Return-to-door directive         Verify boarding + station occupancy
        |                              |
        v                              |
Return/access verification       |
        |                              |
        +---------------+--------------+
                        |
                        v
             DEPARTURE READINESS GATE
                        |
        all required evidence present?
                 /              \
               no                yes
               |                  |
          hold / resolve      request ATC clearance
                                  |
                         clearance granted?
                            /         \
                          no           yes
                          |             |
                       hold         authorized departure
```

This is a logical flow, not a timing chart. Ground tasks may run in parallel when their work areas, equipment, and safety rules allow it. Routing must not imply that every soldier can occupy the same physical waypoint or work envelope simultaneously.

## Shared ground-crew Gesture vocabulary

The first scenario should start with eight reusable physical procedure definitions. A role/task provider composes these with its named interaction anchors, parameters, and domain checks; the eight definitions are not hard-coded as eight universal job types.

1. **Approach and inspect** — move to a provider-supplied station anchor and adopt an inspection pose.
2. **Stabilize for work** — establish a suitable stance and position the hands for the task.
3. **Release a latch or retainer** — reach for and manipulate a named mechanism.
4. **Disconnect a coupling** — perform the intended release and separation movement.
5. **Cap or secure a connection** — place a protective cap or secure the exposed end.
6. **Stow a hose or service line** — move the line toward a provider-defined storage anchor.
7. **Remove a protective cover** — release, lift, and move a named cover.
8. **Stow and visually verify** — place the item at its designated anchor and request a station-clear observation.

These are a starting vocabulary, not a complete catalog of every possible procedure. Some tasks will need additional definitions or finer-grained steps once a real rig and interaction contract exist.

## Example role/task providers

### Landing-gear service provider

A soldier assigned to landing-gear service follows the route to the designated station. The provider supplies anchors and the task procedure for inspecting the retractable landing gear's pneumatic-pressure indication or connection, as appropriate to the spacecraft's authored equipment model.

The gesture can move a hand toward a gauge or service point. It cannot itself assert that pressure is in range. A domain-owned observation supplies the measured pressure and its validity; the task evaluates it against the configured acceptable range and reports success, failure, or indeterminate evidence.

### Forward mass-driver cover provider

A soldier assigned to the forward-facing mass-driver station follows the same initial directive, then receives this provider's task. The procedure releases and removes the dust cover and stows it at a specified anchor.

Cover removal is not proven by the arm pose reaching its endpoint. The equipment/world owner confirms the cover is detached, its resulting state is correct, and it is stowed clear of the required area. Any weapon-system safety/interlock state remains outside the Gesture evaluator.

### Other ground service providers

The same route-and-provider pattern can support umbilical disconnection, hose stowage, exterior inspection, landing-gear checks, cover removal, and other authored service jobs. These examples do not prescribe a fixed number of crew or assume all tasks are safe to perform concurrently. The assignment and task plan determines which agents do which jobs.

## Flight-crew route

At the shared access-door waypoint, the router directs assigned flight crew to the boarding-ramp waypoint. Each soldier then:

1. waits for route/ramp access to be available;
2. follows the boarding procedure with host-supplied ramp, hatch, and vehicle anchors;
3. receives a station assignment;
4. follows the route to that station and performs any required seating/restraint/control interaction;
5. reports readiness only after the world/domain owner confirms the agent is aboard and the assigned station state is satisfied.

Sixteen soldiers can reuse the same procedure definitions with separate identity, progress, assignment, and evidence. They must not share mutable per-agent playback state. Ramp capacity, collision avoidance, route blockage, and actual occupancy belong to the host simulation.

## Return, seal, and departure gate

Ground crew return to the spacecraft-access door when their assigned work is complete or when an explicit safe-abort/recall policy requires them to return. The return route is another waypoint directive, subject to route availability and world state.

The access door may be sealed only after the authoritative host confirms that all required ground crew have returned or have been safely accounted for, the doorway and seal envelope are clear, and all equipment/cover/hose conditions required for closure are satisfied. A Gesture reaching the door does not prove that the soldier passed through it or that the door can safely close.

The departure gate should require explicit evidence, including as applicable:

- all required ground tasks have terminal outcomes and none is unresolved;
- required service lines are disconnected, capped, stowed, or otherwise in the required state;
- required covers and equipment are in their specified state;
- all required ground crew are accounted for and clear of the closure envelope;
- the access door is confirmed sealed;
- all sixteen flight-crew assignments are satisfied, or the authored manifest explicitly specifies a different required complement;
- boarding, seat/station occupancy, restraints, and other required onboard checks are confirmed;
- spacecraft systems report ready through their authoritative owners.

Only when the gate's configured prerequisites are satisfied should the spacecraft request Air Traffic Control (ATC) clearance. **Readiness is not clearance.** ATC may deny, delay, or condition clearance; the departure procedure remains held until a valid clearance is received. The scenario must not equate an animation ending, a local checklist completing, or a readiness flag with authorization to depart.

## Per-agent and per-task state

Track each agent's assignment, current directive, route, provider task, Gesture progress, interruption/recovery state, and authoritative outcome separately. Track each task's owner, prerequisites, evidence, and terminal status. Keep at least these concepts distinct:

- **motion complete:** the Gesture evaluator reached the requested time endpoint;
- **interaction observed:** the host reported a relevant observation;
- **task complete:** the domain-specific success conditions were met;
- **crew ready:** the required agent/station assignments have been verified;
- **departure ready:** all required ship and ground conditions have been verified;
- **cleared:** ATC has issued the required clearance.

A failed or unknown observation should not silently advance the route. The host should hold, retry, request inspection, reroute, or escalate according to explicit policy.

## Implementation sequence

1. Model this scenario as a deterministic, engine-neutral routing/task graph with stable directive, role, task, station, and evidence identifiers.
2. Add unit-test fixtures for the shared waypoint, role-based routing, concurrent independent tasks, blocked routes, and return-to-door behavior.
3. Add negative tests: failed coupling release, invalid pneumatic-pressure observation, cover not actually stowed, blocked ramp, missing soldier, occupied closure envelope, door not sealed, incomplete station assignment, and ATC denial.
4. Keep task outcomes injected as explicit host/domain observations in early tests. Label synthetic fixtures as test data, not physics.
5. Connect real Gesture definitions and per-agent evaluation.
6. Define the rig/anchor mapping and transform conventions with the actual consumer; do not assume a universal humanoid rig.
7. Agree a real evaluated-pose interchange contract with Renderer and validate it end to end before describing this as a live visual simulation.

## Acceptance criteria

The scenario is not complete merely because sixteen characters move and the ground crew perform plausible motions. It is complete when deterministic tests show that routing and task state remain correct under success, failure, delay, interruption, missing evidence, blocked movement, and ATC denial—and a real host can present the evaluated poses without conflating presentation with authoritative state.
