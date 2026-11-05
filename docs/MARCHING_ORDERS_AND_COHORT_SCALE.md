![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Marching orders and player-led cohort scale

> **Status: architecture preparation, not an implemented army system.** Gestures currently provides deterministic individual pose evaluation, caller-supplied stable variation, and an immutable marching-cohesion progression primitive. It does not implement command issuance, formation hierarchy, command propagation, soldier AI, pathfinding, physical crowd simulation, or a player-led army.

## The experience we are preparing for

The player should be able to start with an individual soldier and build toward commanding a historically extraordinary number of soldiers. This must be visible and interactive, not merely a counter:

1. The player issues Fall In.
2. Soldiers assemble, resolve assigned formation positions, and report readiness.
3. The player gives a formation order: form ranks, form a column, dress the line, or take interval.
4. The player issues March, Halt, Turn, Wheel, or another supported order.
5. Soldiers visibly execute the order while retaining individual identity and responding to local conditions.
6. The player can inspect the formation, see which units acknowledged or are delayed, and issue a correction.
7. As command grows, subordinate leaders propagate orders through nested units instead of requiring a direct player command to every soldier.

The ambition is not just to display the largest number. It is to make the player feel in command of an enormous, coherent organization and make the represented scale measurable and honest.

## Keep the domains separate

| Responsibility | Owner |
|---|---|
| Player intent, command selection, authority, command history | Player/gameplay command domain |
| Unit hierarchy, membership, roles, formation slots, order propagation, acknowledgements | Formation / command-coordination domain |
| Versioned activity phases, reusable procedure references, safe transition boundaries | Activity/session coordination |
| Individual marching procedure, pose evaluation, bounded variation, step-phase progression | Gestures |
| Navigation, ground contact, collision, slope, balance, achieved movement | Physics / authoritative world |
| Scheduling compatible work and composing runtime providers | FSM_COS / Foundry as appropriate |
| LOD, instancing, pixels, animation presentation, observer fidelity | Visualize / Renderer |

A command is not a Gesture. March is an order that requests an organized activity; soldier.march is a reusable physical procedure used by each assigned soldier. The order coordinator decides who should do what and when. Gestures evaluates the procedure for an individual. Physics and the world decide what movement was actually achieved.

Do not add command authority, unit membership, or pathfinding to the Gestures evaluator.


## Formation assembly and fall-out: the first visible proof

Before a player can command a marching army, the player must be able to watch an army become an organized formation. The first scene should make that process legible at a glance.

### The drill-field composition

Use a broad, flat, clearly bounded field and an isometric/orthographic camera as the initial presentation. The camera is a presentation choice, not a coordinate-system assumption in the formation domain.

- The formation has an explicit facing direction. "Front", "rear", "left", and "right" are defined relative to that facing, never inferred from screen coordinates.
- A staging/arrival lane sits behind the planned formation area.
- The formation template defines rank count, file count, interval, distance, facing, and stable slot identities.
- The field shows the intended footprint subtly before soldiers arrive: rank guides, slot markers, or a temporary ghost formation.
- A small number of visible landmarks and restrained ground markings help the player read scale without turning the field into a spreadsheet.
- Camera zoom changes presentation density, not formation membership or soldier identity.

For the first proof, keep terrain flat and remove combat, weapons, routefinding complexity, and environmental obstacles. The purpose is to prove assembly choreography and spatial correctness.

### Row-by-row Fall In choreography

The initial line-forming demonstration should be a controlled queue, not a teleport or a simultaneous snap-to-grid:

1. The assembly coordinator creates a versioned formation plan and assigns every soldier a stable slot ID.
2. Soldiers enter through the rear staging lane in a deterministic arrival order.
3. Fill the rear-most rank first, progressing across its slots from one flank to the other; then fill the next rank forward. This makes the growth of the formation visible and avoids sending later arrivals through already occupied ranks.
4. Each soldier follows an approach path to its own slot, decelerates, aligns to the formation facing, and settles into the prescribed interval and distance.
5. The soldier reports **in position** only after the authoritative position/orientation tolerance is satisfied. A command being received or an animation finishing is not enough.
6. The rank and then the whole formation report readiness when all required slots are occupied or explicitly marked vacant/blocked.
7. If a soldier is delayed, blocked, or unavailable, retain the slot identity and show the exception; do not silently shift every later soldier and make the formation unstable.

A simplified top-down plan (the camera may render this isometrically):

~~~text
                         FORMATION FRONT
             [ ][ ][ ][ ][ ][ ][ ][ ]   Rank 3
             [ ][ ][ ][ ][ ][ ][ ][ ]   Rank 2
             [ ][ ][ ][ ][ ][ ][ ][ ]   Rank 1  <- fill first
                         |
                         | arrival / staging lane
                         |
                    incoming queue
                         REAR
~~~

The diagram labels ranks in fill order for this first choreography; it does not establish universal military terminology. The formation template owns its naming and geometry.

### Fall Out is a movement procedure too

Fall Out must not delete soldiers, clear the formation array, or make the unit vanish. It is a commanded transition from assigned formation slots to an explicit released/rally/staging state.

For the first demonstration, release soldiers in a controlled sequence through designated egress lanes:
- mark the formation as releasing and stop accepting new slot assignments for that revision;
- select a release order that keeps paths clear (initially, rear-most rank first, with flank egress lanes and per-soldier reservations);
- each soldier acknowledges the release order, leaves the slot, moves to the egress lane, and reaches the rally/staging area;
- free a slot only when the soldier has physically cleared its occupancy tolerance;
- report each rank and the whole formation as released only when all required soldiers have reached their release state;
- support cancellation/re-forming at explicit safe boundaries, preserving soldier and unit identity.

The egress policy is formation-specific. A column, square, or constrained field may need a different release sequence. Do not assume that simply reversing the Fall In list is always collision-free.

### Formation data versus presentation

The formation coordinator owns the authoritative plan: stable formation ID/revision, facing, geometry, rank/file dimensions, spacing, slot IDs, soldier-to-slot assignments, occupancy, and readiness/release state.

Visualize presents the field, slot guides, soldier instances, approach/egress motion, and scale-aware detail. Renderer may explain state and diagnostics. Gestures supplies each soldier's individual procedural pose while approaching, dressing, standing, stepping off, or falling out. Physics/world integration validates movement, clearance, collision, and actual slot occupancy. None of those presentation layers should become the owner of formation membership.

The visual should distinguish at least:
- **planned** slot (ghost/guide);
- **assigned** slot (a soldier has responsibility for it);
- **approaching** (soldier moving toward it);
- **occupied** (soldier is within position and facing tolerance);
- **blocked/vacant** (explicit exception);
- **releasing** (soldier is leaving the slot);
- **clear/released** (slot is physically free and the soldier reached the release state).

This lets the player understand progress without reading a wall of text.

### Grow the same mechanism into squads and platoons

Do not build a separate formation algorithm for every military echelon. A formation is a composition of subordinate formations and/or individual slots:

~~~text
Army / cohort
  +-- Platoon A
  |     +-- Squad A1
  |     |     +-- soldier slots
  |     +-- Squad A2
  |           +-- soldier slots
  +-- Platoon B
        +-- Squad B1
              +-- soldier slots
~~~

At squad scale, the player can watch individual soldiers fill a squad template. At platoon scale, squads become the arriving elements and take subordinate formation slots; soldiers need not all stream from the rear as one enormous queue. The same lifecycle applies recursively: plan, assign, approach, occupy, report ready, release.

Use explicit formation templates and stable slot identities at every level. A parent slot can be occupied by a child formation with its own local slots. Parent readiness rolls up from child readiness plus parent-level spacing/orientation checks. A delayed squad should be visible as a delayed element, without freezing unrelated squads unless the order's policy requires synchronized assembly.

### Presentation and scale progression

Build the visual proof in this order:
1. one soldier approaches one visible slot, aligns, and falls out;
2. a small squad fills one rank, then a second rank;
3. a complete squad forms and releases without path crossings;
4. multiple squads take platoon-level slots as subordinate units;
5. zoomed-out presentation shows many platoons as readable blocks while preserving selection and drill-down to individuals;
6. measured crowd tiers increase represented scale without implying every distant soldier receives full skeletal animation and physics every frame.

The first scene should be attractive but deliberately simple: flat field, readable formation footprint, clear staging lane, smooth arrival/settling, and a visible release path. A successful first proof is not a huge number of tiny soldiers; it is a small formation whose assembly and disassembly are correct, understandable, deterministic, and repeatable.

## Command lifecycle

A first command model should be inspectable and versioned. These are conceptual fields, not public API commitments.

| Concept | Meaning |
|---|---|
| Command ID | Stable identity for one issued order |
| Issuer / authority | Player or authorized subordinate that issued it |
| Target scope | Individual, squad, platoon, company, battalion, or selected cohort |
| Order kind | Fall in, form up, dress ranks, march, halt, turn, wheel, disperse |
| Parameters | Formation type, direction, cadence, destination, spacing, constraints |
| Issued revision / sequence | Orders apply in an explicit, reproducible order |
| Effective boundary | Immediate, next safe activity boundary, or declared future tick |
| Acknowledgement | Received, accepted, executing, completed, blocked, rejected, superseded |
| Observation/evidence | What the authoritative world actually observed |
| Parent command | Links a delegated order to its originating order |

A command is not completed merely because it was sent. A unit may acknowledge receipt, begin execution, become blocked, or complete only after the appropriate observations arrive.

### Initial order progression

~~~text
Player
  |
  | FALL IN
  v
Unit hierarchy / assembly coordinator
  |
  +--> Squad leaders receive assignment
  |       |
  |       +--> Soldiers resolve stable formation slots
  |       +--> Readiness is observed and reported
  |
  v
FORMATION READY (or partial readiness with diagnostics)
  |
  | FORM RANKS / FORM COLUMN / DRESS
  v
Formation coordinator validates spatial arrangement
  |
  | MARCH
  v
March activity session
  |
  +--> cohort-level shared timing and route intent
  +--> unit-level cadence / phase coordination
  +--> individual stable variation and Gesture evaluation
  +--> Physics/world observations
  |
  v
Visible achieved movement + acknowledgements
  |
  | HALT / TURN / WHEEL / NEW ORDER
  v
Safe transition boundary and next command revision
~~~

The first vertical slice should support Fall In → Form a simple line → March → Halt. Add column, turns, wheels, route following, terrain response, and complex formations only after the first sequence can be replayed and inspected.

## Hierarchy: the player commands formations, not an input loop

The organization needs nested command scope. Exact military naming and echelon size should be configurable content, not hard-coded assumptions.

~~~text
Player commander
    |
    +-- Cohort / army command
          |
          +-- Formation A
          |     +-- Unit A1
          |     |     +-- Squad leader
          |     |           +-- individual soldiers
          |     +-- Unit A2
          |
          +-- Formation B
                +-- Unit B1
                +-- Unit B2
~~~

The player issues one order to a selected scope. The coordinator resolves target membership and delegates relevant orders to subordinate units. Subordinate leaders acknowledge, report readiness, and surface exceptions. Soldiers retain stable individual identities and evaluate their own procedures.

The command hierarchy should support stable unit and soldier IDs independent of array/render order; explicit membership revisions and deterministic assignment; command scope expansion and exclusions; delegation and acknowledgement roll-up; local exceptions; interruption, replacement, cancellation, and safe re-entry; and replayable command history with a declared logical timebase.

## Cohort scale: count, simulation, and presentation are different budgets

Measure several distinct scales rather than use one vague maximum-soldiers number.

| Scale | What it measures |
|---|---|
| Commanded population | Unique soldiers covered by an accepted player order |
| Materialized population | Soldiers with individual persistent state currently resident |
| Actively simulated population | Soldiers receiving individual decision/physics updates at a given cadence |
| Gesture-evaluated population | Soldiers whose procedure is evaluated during the measurement window |
| Presented population | Soldiers actually visible to the player or another observer |
| Fully detailed population | Soldiers receiving high-fidelity individual animation and physical interaction |

A system may represent a very large army without running every soldier through full physics and full skeletal animation on every render frame. That is acceptable only if the fidelity tiers are explicit, observable, and preserve required behavior.

Potential tiers:
- **Near/player tier:** full individual state, command response, Gesture evaluation, local collision/contact, detailed presentation.
- **Formation tier:** individual identities and slots retained; shared route/clock/formation inputs; reduced-rate individual updates where safe.
- **Distant cohort tier:** compact cohort state, aggregate progress, deterministic individual identity/variation data, formation-level presentation.
- **Off-screen/background tier:** event- or time-advanced state with explicit catch-up rules; no unnecessary per-frame work.

Tier transitions must not change command semantics or erase identity. Promotion to higher fidelity reconstructs valid individual state from authoritative cohort/unit state.

## Performance and correctness principles

1. **Stable identity first.** Never derive soldier identity from current array index, visible row, or scheduling order.
2. **Keyed deterministic variation.** Use stable entity ID, logical tick/step, and purpose ID. Gestures accepts caller-supplied samples; it should not own a mutable random stream.
3. **Share work where inputs are shared.** Route, command, cadence, formation geometry, and phase may be shared at cohort/unit level. Per-soldier parameters and output remain individually addressable.
4. **Order-independent evaluation.** Reordering or parallelizing soldier evaluation must not change the result for a stable soldier and logical step.
5. **Explicit timebases.** Command time, march-step phase, physics time, and render time are not interchangeable.
6. **Safe transition boundaries.** A new command should not mutate the middle of an individual step unless the order explicitly defines an interruption policy.
7. **Intent is not outcome.** A scheduled step is not proof of movement; world observations decide whether the soldier advanced, collided, stumbled, or remained blocked.
8. **Measure each tier.** Record memory per represented soldier, active-update cost, Gesture-evaluation cost, scheduling overhead, output size, and presentation cost separately.

## How the current marching primitive fits

Gestures currently provides:
- a deterministic evaluator that accepts caller-supplied GestureVariationSample values;
- MarchingCohesionProgression and immutable MarchingCohesionState;
- explicit tightening of individual variation after every second completed march step, with a non-zero floor;
- state-level Evaluate so partial evaluations reuse the same variation state until the host reports a completed step;
- a crowd-scale BenchmarkDotNet profile with 1k, 10k, 100k, and 1M sample counts.

These are individual procedure/evaluation building blocks. The benchmark profile has not yet been run for measured results. The fixture's generated samples are test data, not a substitute for the Physics keyed Squirrel3 source. No command hierarchy, army simulation, or population batch engine is implied by these primitives.

## Evidence ladder

These are milestones, not claims that the current package supports those counts:

1. **One soldier:** Fall In, form position, march, halt; inspect command and Gesture state.
2. **One squad:** one order to the squad; stable assignments; acknowledgement and blocked-state reporting.
3. **One formation:** nested units, line/column, turns, and replay.
4. **1,000 soldiers:** benchmark command propagation, state memory, scheduling, evaluation, and presentation separately.
5. **10,000 soldiers:** demonstrate shared work and reduced-rate tiers without losing identity or order independence.
6. **100,000 soldiers:** verify cohort representation, local high-fidelity interaction, deterministic replay, and visible command propagation.
7. **1,000,000 and beyond:** scale only after measurements show which state is materialized, simulated, evaluated, and presented.

The ambition is to push the player-commanded population as far as architecture and measured hardware allow, while being clear about what each soldier is doing at each fidelity tier.

## First implementation sequence

1. Keep the feline articulated fixture moving independently; do not entangle animal profiles with army command.
2. Finish the individual marching procedure and its deterministic step-boundary contract.
3. Define the command/formation owner and its stable unit, membership, order, acknowledgement, and logical-time contracts.
4. Implement the one-soldier command lifecycle and replayable Fall In → March → Halt slice.
5. Add squad delegation and formation slots.
6. Connect shared cohort cadence and route intent to per-soldier Gesture samples.
7. Connect authoritative Physics/world observations and blocked/recovery handling.
8. Add population tiers and benchmarks, then expand represented cohort size from measured evidence.

**North star:** the player issues a clear order to an organized force; the order travels through a visible hierarchy; soldiers respond as individuals within a coherent formation; and the system can explain exactly how many soldiers are commanded, simulated, evaluated, and presented.

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
