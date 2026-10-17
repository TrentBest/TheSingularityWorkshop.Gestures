# Extremus: Deterministic Extreme-Distance Crowd Representation

> **Status: architecture contract plus first CPU reference baker.** The first implementation now creates deterministic, engine-neutral population-instance records and a provenance manifest. It is not yet a pose/animation bake, billboard atlas, impostor renderer, GPU-instancing path, crowd-LOD transition system, or demonstrated population-scale performance result.

## The goal

At extreme distance, the world should still look populated and purposeful without requiring every visible person to run a full articulated performer simulation. Extremus is the strategy for composing and baking distant population representations from reusable source motion and deterministic placement data.

The key idea is not to stamp the same billboard hundreds of times. It is to **distribute a large source pose/activity vocabulary into a stable, varied population**, then bake that population into one or more renderer-consumable representations. Each bake should be reproducible from versioned inputs. Two different seeds should produce different crowds; the same inputs and seed should reproduce the same crowd.

## Implemented first slice: CPU population instances

The Gestures core now contains `ExtremusCrowdBaker` and immutable input/output records:

- `ExtremusBakeRequest` carries profile/catalogue versions, a region, seed, requested count, weighted semantic activities, optional exclusion envelopes, and a bounded placement-attempt limit.
- `ExtremusCrowdBaker.Bake` deterministically assigns a stable ID, position, heading, scale variation, activity, and phase to each generated instance.
- `ExtremusBakeManifest` records algorithm version, source/profile identifiers, region, seed, and requested/baked counts.
- Activity and exclusion inputs are canonicalized by stable ID; generated instances are emitted in stable ordinal order.
- Placement is rejected against domain-supplied axis-aligned exclusion envelopes. If a member cannot be placed within the configured deterministic attempt limit, the bake fails explicitly rather than silently reducing population count.
- The CPU test suite covers repeatability, input-order independence, seed diversity, region bounds, exclusion envelopes, empty populations, and invalid inputs.

This is a **population layout reference**, not yet a motion or geometry bake. It does not resolve gesture providers, evaluate pose sequences, solve paths or inter-person spacing, perform collision/clearance physics, create a render resource, or promise that an instance can be promoted back to a live actor. The algorithm version and canonical mapping must be pinned by golden vectors before the format is treated as a cross-language interchange contract. GitHub Actions is the verification source; consult the run linked from the relevant commit before claiming build/test success.


Extremus is not a second Gesture evaluator, not a world simulator, and not the authority for what a person is doing. It is a representation/baking path for detail that an observer cannot resolve individually.

## Ownership boundaries

| Owner | Responsibility |
|---|---|
| Gesture/provider catalogue | Reusable pose and procedure definitions, semantic tags, anchors, supported actions, and source motion data |
| Extremus bake/composition layer | Deterministic population composition, identity assignment, stable variation, spatial distribution, activity allocation, phase offsets, bake metadata, and validation |
| World/domain and Physics | Authoritative identity/state where required, movement, obstruction, collisions, contact, events, and any physical outcomes |
| Renderer | Observer-relative representation choice, distance/screen-size policy, refresh/freshness, promotion/demotion, and frame assembly |
| Visualize | Presentation of renderer-selected geometry, instances, or impostor resources through its backend contracts |
| Host/provider (WebPage, AnyApp, raWWar, studio, etc.) | Scene intent, content identities, population profile, fixture loading, and user-facing controls |

Do not add a Renderer or Visualize dependency to the Gestures core. Do not let a successful bake imply that a simulated action physically succeeded. Do not let an SVG illustration imply GPU execution.

## Separate the data concepts

These concepts must remain distinct, even if a first implementation stores them in a compact record:

1. **Pose** — one sampled articulated configuration or source frame.
2. **Gesture/procedure** — a meaningful progression through poses and phases.
3. **Activity** — a semantic role such as walking, waiting, boarding, carrying, servicing, looking toward an event, or dispersing.
4. **Population profile** — the distribution and spatial rules for activities in a context such as a starport departure, industrial yard, city street, stadium, or evacuation.
5. **Population instance** — a stable identity plus placement, heading, scale envelope, activity, and phase/variation parameters.
6. **Baked representation** — an output designed for a particular range of viewing conditions, with declared coverage and limitations.
7. **LOD decision** — the Renderer’s choice of which available representation to show to a particular observer now.

A profile should describe what kinds of activity are plausible and how they are distributed—not dictate one global synchronized animation. A pose atlas is source material; it is not itself a crowd. A baked atlas or impostor is a presentation asset; it is not authoritative world state.

## Proposed bake pipeline

```text
Versioned source poses / procedures
             |
             v
Semantic atlas + capability validation
             |
             v
Population profile + world-space region + stable seed
             |
             v
Deterministic placement / heading / scale / activity / phase
             |
             v
Representation bake + provenance manifest
             |
             v
Renderer chooses representation for observer
             |
             v
Visualize presents selected draw data
```

The first version should be an **offline CPU bake** with a transparent manifest. GPU-oriented layouts, texture arrays, multi-view impostors, and runtime streaming are later optimizations once the data contract and correctness fixtures are stable.

## Determinism and stable identity

The output must not depend on enumeration order, render-frame rate, wall-clock time, thread scheduling, or the order in which entities happen to be discovered.

For the same versioned source catalogue, profile, region, bake settings, and seed:

- The output identity set and all placement/activity parameters are identical.
- Reordering the source records does not change the output when stable IDs are unchanged.
- Re-running the bake produces byte-stable canonical metadata, except for fields explicitly excluded from canonical comparison (for example, a diagnostic timestamp; preferably omit timestamps from the manifest).
- A changed seed changes the composition while preserving declared bounds.
- Changing a catalogue/profile version invalidates the bake key and is visible in the manifest.
- No new random values are sampled per frame. Any animation phase is assigned once from stable identity and baked or evaluated as a deterministic function of time.

The existing `CrowdResponseScheduler` documents a versioned stable-identity approach using FNV-1a input hashing and SplitMix64-style finalization. Extremus should reuse a small, explicitly versioned deterministic primitive where the semantics fit rather than grow an incompatible second family of ad hoc hashes. Cross-language bit-for-bit parity must be tested before treating a bake manifest as portable between tools.

Never use array index as identity. Prefer stable source IDs plus a population seed and a stable generated identity. If the seed changes, generated identities may change by design; if only input ordering changes, they must not.

## Distribution should create relationships, not just noise

Independent random offsets are not enough to make a believable population. A profile should support a small number of explicit, testable distribution mechanisms:

- **Zones:** waiting, queueing, boarding, loading, walking, equipment service, safety boundaries, and low-activity buffers.
- **Flow fields or paths:** preferred direction, entry/exit regions, queue lanes, and travel corridors.
- **Clusters:** groups that share a destination or broad activity but retain individual offsets and phases.
- **Role/activity weights:** crew, passenger, security, maintenance, spectator, or other provider-owned semantic roles.
- **Exclusion constraints:** forbidden regions, keep-out envelopes, clearance around machinery, and no-placement regions supplied by the domain.
- **Density controls:** target count or density with an explicit deterministic sampling policy.
- **Decorrelated variation:** bounded pose/activity/phase/heading differences that do not destroy the larger flow pattern.

The bake may consume a domain-provided region and placement constraints; it must not invent collision clearance or claim that a crowd is physically navigable. If authoritative world state changes, the host must decide whether to rebuild, patch, transition, or temporarily promote the affected region to live simulation.

## Representation tiers and transitions

The following is a design vocabulary, not a frozen distance table:

| Tier | Candidate representation | What must remain stable |
|---|---|---|
| Near | Live performer with detailed procedure/pose evaluation | Authoritative identity, state, task phase, physical interactions |
| Middle | Reduced-cost actor or simplified articulated representation | Identity, approximate transform, activity, progression |
| Far | Small sets of view-dependent impostors or instanced simplified figures | Population density, silhouette variety, broad activity and flow |
| Extreme | Baked crowd field, multi-view impostor, or layered population representation | Spatial distribution, aggregate direction/activity, region identity, content version |
| Horizon | Density/flow/silhouette signal where individuals are no longer resolvable | Aggregate scene meaning and continuity with nearer tiers |

Do not pick fixed metre thresholds here. Renderer owns observer-relative policy and should use screen-space evidence, view, and refresh budget rather than a single distance alone.

Promotion and demotion must not restart an actor’s authoritative task or reaction. When an observer approaches a region, a host/renderer adapter needs a defined path from baked aggregate to live identities—or must clearly mark that the baked region has no individually recoverable identities. A billboard bake cannot honestly promise individual continuity unless its manifest retains a stable per-instance mapping sufficient to reconstruct the intended entities.

When world changes matter, static bake validity needs explicit inputs: region/content revision, profile version, source catalogue version, seed, and any world snapshot or constraint version used. Stale bakes must be detectable; they must not silently stand in for changed authoritative events.

## Bake manifest: required information

The first manifest should be plain, versioned data and should avoid renderer-specific binary layouts. At minimum, record:

- Schema version and bake algorithm version.
- Stable source-catalogue ID/version and the exact pose/procedure IDs referenced.
- Population-profile ID/version.
- Stable region ID and declared coordinate frame/units.
- Seed and deterministic algorithm ID/version.
- Requested population/density and the actual generated count.
- Representation kind, intended view coverage, and declared validity range.
- Region/content revision and versions of any placement constraints used.
- Stable generated identity and activity/phase mapping when individual recovery is supported.
- Output asset identifiers, content hashes, and generator version.
- Validation summary and explicit limitations.

The manifest must not claim that the bake was produced by GPU code unless the actual generator used that path. A content hash can prove asset identity; it does not prove visual quality or runtime performance.

## First vertical slice: two starport populations

Use the existing starport domain as the first concrete context, not a generic crowd of anonymous dots.

Create two separate population patches from the same source vocabulary and profile family, with different stable seeds:

- **Departure concourse:** passengers moving toward boarding zones, small waiting groups, and individuals pausing to check destinations.
- **Ground-service perimeter:** crew moving between task areas, small groups waiting near equipment, and clear buffers around vehicle/service paths.

The exact activities must come from capabilities actually represented in the source catalogue. If the current catalogue cannot express an activity, record that gap instead of drawing a fictional completed gesture.

The first slice should produce a canonical manifest and a living visualization that can show the source catalogue, profile, seeded placements, activity labels, phase distribution, and the two populations side by side. The SVG is a review surface for the deterministic CPU bake—not proof of impostor rendering, GPU throughput, physics correctness, or crowd scale in a running world.

### Acceptance gates

1. Same inputs and seed reproduce the canonical manifest and generated placement records.
2. Different seeds produce meaningfully different placement/activity composition within the same declared constraints.
3. Reordering source and profile records does not change results when stable IDs are unchanged.
4. Every generated position is finite and inside the declared allowed region; no point violates explicit exclusion constraints.
5. Generated identities are unique within the bake and do not derive from enumeration index.
6. All referenced source IDs and profile activity IDs resolve; unsupported activities fail validation or are explicitly omitted by policy.
7. Baked metadata declares what is and is not recoverable when promoting from far to near representation.
8. Renderer/Visualize integration consumes an explicit output contract; neither package is referenced by the bake core.
9. The generator output is snapshot-tested, XML-validated, and regenerated in CI with the source inputs visible.
10. Any performance claim includes a reproducible workload and measurements; a high entity count in a manifest or SVG alone is not a scale benchmark.

## What not to do yet

- Do not create hundreds of hand-drawn unique SVG silhouettes and call them a generated pose atlas.
- Do not generate per-frame randomness to make the crowd appear alive.
- Do not bake physical collisions, task success, or world changes into presentation-only data.
- Do not hard-code one distance threshold into Gestures.
- Do not assume every far billboard can reconstruct an individual actor later.
- Do not add GPU APIs, renderer resource handles, Unity types, WPF types, or host application types to the core package.
- Do not publish a package or deploy a consumer as part of this design step.

## Coordination handoff

- **Renderer:** propose and review the minimal representation descriptor needed for source kind, observer suitability, freshness/version, and promotion/demotion continuity. Renderer owns selection policy and frame assembly.
- **Visualize:** review which neutral instance/impostor resource descriptions it can actually consume today. Do not add a Visualize dependency to Gestures or promise a backend feature that has not been implemented.
- **WebPage / AnyApp:** use the same versioned fixture and expose the bake seed, profile, generated count, and provenance. Label SVG/GUI.SVG output as CPU-generated inspection output.
- **Physics:** supply authoritative region, exclusion, flow, and world-revision data where available. A bake consumes these constraints; it does not replace physics or imply collision avoidance.
- **raWWar / content providers:** own soldier/person identity, faction, equipment, and domain-specific activities. The generic bake contract remains content-neutral.

## Implementation sequence

1. Audit current source catalogue and stable ID rules; select the smallest reusable source set that is actually implemented.
2. Define immutable, validated core records for region, profile, generated population instance, and bake manifest.
3. Implement deterministic placement and activity assignment using the versioned stable-identity primitive.
4. Add order-independence, bounds, exclusion, uniqueness, replay, and seed-divergence tests.
5. Generate the first two starport population snapshots and manifest from executable C# code.
6. Add the generator to CI with XML and snapshot-parity checks; keep the current non-blocking drift warnings separate until existing living-motion snapshots are synchronized.
7. Review the neutral representation descriptor with Renderer and Visualize before adding asset baking or GPU-specific data.
8. Only then investigate view-dependent sprite/impostor generation, atlas packing, refresh policy, and measured LOD transitions.

Extremus succeeds when the distant world remains populated and legible, the same seed can rebuild the same result, and the renderer can spend its budget on what an observer can actually resolve—without confusing presentation with simulation.
