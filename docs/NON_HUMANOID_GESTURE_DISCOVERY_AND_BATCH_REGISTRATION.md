# Non-humanoid gesture discovery and batch registration

> **Status: design proposal, not a frozen public API.** This document captures a consumer-first workflow for dogs, birds, butterflies, people, vehicles, mechanisms, and other targets without imposing a humanoid rig or claiming automatic retargeting exists.

## The idea

Ask the thing that owns a capability, not a global list of animation clips:

> Consumer: “Do you have gestures for a dog?”
>
> Dog capability provider: “Yes — candidate handle/index `n`.”

The answer identifies a matching **provider offer or target profile**, not a selected motion and not a promise that the current dog rig is compatible. The consumer then asks for the profile's **registration pose and staging requirements**. It does not request a Gesture to animate the dog into registration.

This distinction is the key to population-scale work: resolve and validate each rig once, then group compatible registrations into batches. A dog, a bird, and a butterfly can use the same discovery and scheduling machinery while retaining different transform topology and motion semantics.

## Proposed lifecycle

```text
Actor / host asks capability providers
          |
          v
"dog" query -> matching offer(s) + stable identity/version
          |
          v
Request registration-pose contract (not an action Gesture)
          |
          v
Host stages actor at the requested point using authoritative movement
          |
          v
Actor/rig reaches and validates its registration pose
          |
          v
Rig adapter binds its actual transform IDs and topology
          |
          v
Validate compatibility -> register target instance
          |
          v
Build compatible work cohorts -> evaluate in batches
          |
          v
Per-instance output -> consumer adapter -> renderer/presentation
```

### 1. Query for capability

The query describes the target or desired domain in terms the provider understands: for example `species=dog`, an optional breed/size or rig profile, and requested capabilities such as `walk`, `run`, `sit`, or `shake-off`. A bird may offer perch, hop, flap, and land; a butterfly may offer wingbeat, turn, settle, and takeoff. These are illustrative vocabulary, not a claim that these providers already exist.

A query may return zero, one, or many offers. Do not silently pick the first offer just because it was enumerated first.

### 2. Return a handle safely

The response can include a compact result index for the caller's current query result, but the index is only a lookup slot in that result snapshot. It must not become a durable identity across provider reloads, reordered results, or catalog versions.

A durable reference should carry a stable provider/offer identifier and version. In shorthand:

- Query-local index: efficient to use immediately.
- Stable offer ID + version: safe to persist, cache, compare, or send to another process.
- Rig instance ID: identifies one registered actor/asset instance, not the provider's reusable offer.

The index is a handle to inspect/request the offer; it is not an index into a universal array of dog animation clips.

### 3. Request registration data, not a gesture

The selected offer should expose or reference a registration profile with the information the consumer needs to adapt the concrete rig, such as:

- a named registration-pose definition or reference;
- the target transform identifiers/topology expected by that profile;
- coordinate and unit conventions, if required by the eventual adapter contract;
- a staging point/anchor and tolerances supplied by the host/world;
- required capabilities and validation outcomes for a compatible registration.

The registration pose is a calibration/entry condition. It is not a movement procedure. A consumer can request the profile and pose data without starting a walk, sit, or other action.

Current `GestureDefinition` and `EvaluatedPose` can represent named local transforms, but the package does **not** yet define a complete versioned rig descriptor, a universal rest-pose schema, or a registration API. Treat this section as the target contract, not existing implementation.

### 4. Stage, assume, and validate

The host or simulation moves the actor to the required staging point. Gestures must not teleport the authoritative actor or treat an evaluated pose as proof that the actor arrived.

After the host determines that the actor can safely enter the registration pose, the rig adapter maps the supplied pose to the actual rig, applies it through the appropriate host-owned mechanism, and validates required transforms. The provider does not get to assume the target topology matches merely because the query said “dog.”

Registration should return explicit outcomes, such as compatible, compatible-with-mapping, missing-transform, ambiguous-mapping, unsupported-profile-version, or staging/pose-not-achieved. Exact enum/API names are deferred until a real rig adapter and consumer test justify them.

### 5. Register once, then batch

A successful registration produces a runtime record linking the rig instance to its validated mapping/profile. Subsequent work can be grouped by compatible execution requirements, for example:

- same evaluator/data layout and compatible transform mapping;
- same sampling time or cadence tier;
- same solver/backend capabilities;
- same output projection shape.

A cohort is temporary scheduling metadata, not a social group or a permanent species grouping. Dogs of different rig topology may need separate cohorts; a dog and a bird can still share higher-level scheduling infrastructure. Per-instance identity, seed, variation, state, and output remain distinct.

The scalar reference evaluator remains the correctness oracle. A future batch path must demonstrate per-instance deterministic parity with scalar evaluation before performance claims are made. Do not force every animal into one batch if the shared work assumptions do not hold.

## Ownership boundaries

| Concern | Owner |
| --- | --- |
| Discover which reusable procedures/profile are offered | Capability / Gesture provider |
| Stable offer identity and version | Provider catalog contract |
| Query-local result index | Caller/provider query response; never a durable identity |
| Registration-pose data and expected transform vocabulary | Profile/provider contract, subject to versioned schema |
| World staging, movement, contact, and whether the pose was physically achieved | Host / simulation / Physics |
| Map profile transform names to a concrete asset/rig | Consumer-owned rig adapter |
| Registration validation and instance-to-mapping record | Host/adapter boundary |
| Intent, choosing the next action, interruption, and achieved outcome | Actor / behavior / host |
| Deterministic pose evaluation | Gestures |
| Compatible-work grouping and execution cadence | Batch scheduler / evaluator backend |
| Observer-relative fidelity and GPU presentation | Renderer / Visualize |

Physics and Gestures remain independent packages. Gestures consumes assessed evidence through an explicit adapter where needed; it does not invent support, collision, or successful staging. This proposal adds no dependency on Unity, Renderer, Visualize, FSM_COS, or a game-specific actor.

## Minimal proof before freezing an API

1. Use the existing named-transform evaluator with one dog/quadruped fixture and one bird or articulated mechanism fixture.
2. Define a small fake provider query result that returns a query-local index plus a stable offer ID/version.
3. Request a registration-pose fixture separately from a locomotion Gesture.
4. Test a compatible mapping and explicit missing/ambiguous-transform failures.
5. Show that staging/pose-achieved evidence is supplied by the host, not inferred from the pose data.
6. Register many instances and compare a future batch evaluator against scalar reference results, including stable per-instance variation and mixed cohort membership.
7. Keep SVG/living documentation labelled as fixture output or conceptual design according to its actual provenance.

Until that evidence exists, do not advertise automatic dog/bird/butterfly rig support, a stable public index API, or batch-evaluator performance as implemented.

## Related contracts

- [Articulated Motion Contract Proposal](ARTICULATED_MOTION_CONTRACT_PROPOSAL.md)
- [Gesture Data Model](DATA_MODEL.md)
- [Provider Offers and Actor-Relative Selection](GESTURE_OFFER_SELECTION.md)
- [Renderer Pose Handoff Proposal](RENDERER_POSE_HANDOFF_PROPOSAL.md)
- [Roadmap](ROADMAP.md)
