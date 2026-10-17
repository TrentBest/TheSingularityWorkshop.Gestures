# Deterministic Crowd-Response Scheduling

> **Status: CPU reference implementation and generated living schedule verified in CI.** `CrowdResponseScheduler` schedules deterministic per-participant intent with bounded delay, variant, and intensity. The generated [crowd-response schedule SVG](assets/crowd-response-schedule.svg) is produced by running the actual scheduler and serializing its returned records. Acoustic arrival, crowd solving, and LOD continuity remain unimplemented.

## The observable result

One scene event—such as the player stepping onto the balcony—causes many independently identified soldiers and clusters to respond. Their behavior should look related without looking synchronized. Replaying the same authoritative inputs must reproduce the same schedule.

This is an event-scheduling problem before it is a rendering problem. Do not start by animating thousands of entities or by emitting a global `cheer now` flag.

## Keep three clocks distinct

1. **Intent time:** when a cluster or agent decides its response should begin, based on the shared event and the response policy.
2. **Presentation time:** when the response gesture becomes visible in the current representation. Renderer may change fidelity, but must not reset the underlying response phase.
3. **Acoustic arrival time:** when a source event reaches a listener. This is derived from a physical/acoustic result when available, or from an explicitly labeled approximation. It is not the same as intent time.

The scene controller owns the triggering event and scene phase. raWWar owns soldier identity and authoritative crowd intent. Gestures can evaluate the selected reusable motion procedure. Renderer selects observer-relative fidelity; Visualize presents it. Physics owns physical outcomes and any acoustic model assigned to it. Scene/audio presentation mixes intentional vocal events. Do not make Gestures a sound-effects system.

## Proposed schedule inputs

The smallest useful deterministic input record contains:

- Stable scene/event identifier and event-origin time.
- Stable agent identifier, or stable cluster identifier for aggregated distant populations.
- Authoritative source and listener positions when distance is relevant.
- Response policy identifier and version.
- Declared bounds for response delay, gesture variant, and intensity.
- Stable identity/context seed, derived from identity and event identity—not iteration order or a fresh random generator.

Use explicit units: seconds for time, metres for position, and a declared coordinate frame. Reject non-finite values and invalid bounds at the boundary. A missing or invalid position must not silently turn into a fabricated distance.

## Reference timing model

A first reference policy may express response intent as

\[
t_{intent}=t_{event}+\Delta_{policy}(agent,event)
\]

where \(\Delta_{policy}\) is a deterministic, bounded offset computed from declared policy inputs and a stable identity/event seed. It may include an authored cluster-wave delay or a bounded identity-specific decorrelation term. It must not sample new randomness each frame. The offset is policy data, not an invented claim about human reaction time.

For the simple acoustic-arrival approximation only:

\[
t_{arrival}=t_{source}+\frac{\lVert x_{source}-x_{listener}\rVert}{c}
\]

Here \(c\) is an explicitly declared speed-of-sound approximation for the scene medium. If Physics or an acoustic system supplies authoritative arrival data, consume that result instead of calculating a duplicate. This equation does not model occlusion, reflections, directional filtering, or reverberation.

**Do not conflate the equations.** The crowd may begin reacting at its local intent time; the resulting cheer has a separate source time and reaches the listener later.

## Determinism and identity

- A schedule is a pure function of versioned inputs and stable identifiers wherever practical.
- The same event, policy version, identities, positions, and timebase produce the same schedule.
- Input enumeration order must not affect results. Never derive identity from a list index.
- Distinct identities may receive distinct bounded delays or gesture variants, but all values must remain inside declared limits.
- Re-evaluating a schedule at a later time must not reroll earlier choices.
- Replaying a saved event with its original inputs must not depend on wall-clock time or render frame rate.
- If positions or intent change authoritatively, represent that as a new input/versioned observation; do not hide mutable state inside the random seed.
- Cluster aggregation must preserve a stable representative identity and phase. Changing representation tier must not restart or globally resynchronize the event.

The v1 implementation uses UTF-8 FNV-1a over the policy ID/version, event ID, participant ID, and caller-supplied stable seed; separate fixed salts feed a SplitMix64-style finalizer for delay, variant, and intensity. Integer arithmetic is 64-bit unsigned with wraparound, and the upper 53 bits map to a reproducible unit interval. The test suite pins one reference vector. Changing any of these details requires an explicit algorithm version or policy compatibility decision. Cross-language bit-for-bit parity remains a separate acceptance gate.

## Output contract

For each scheduled participant or cluster, expose enough data for inspection and replay:

- Event ID and policy version.
- Stable participant/cluster ID.
- Intent start time and scheduled response kind.
- Stable variation parameters selected within declared bounds.
- Source-event time and acoustic arrival estimate/result, when applicable.
- Provenance indicating whether an acoustic time is authoritative or approximate.

The scheduler does not decide whether an entity is visible, solve collisions, move bodies through geometry, synthesize vocal audio, or choose GPU buffers. A schedule is intent/timing data—not proof that a physical action completed.

## Tests required before scale claims

1. **Replay:** identical inputs produce identical complete output records.
2. **Order independence:** permuting input entities produces the same per-ID results.
3. **Stable resampling:** querying the same event later does not reroll its parameters.
4. **Bounded variation:** every selected delay/parameter remains inside its declared range, including edge values.
5. **Identity distinction:** a fixed sample of identities is not accidentally assigned one synchronized parameter set; do not assert that every possible pair must differ.
6. **Policy isolation:** changing the policy version changes output only according to the documented versioned contract.
7. **Timebase:** scheduled times are relative to the supplied event clock, never wall-clock time.
8. **Acoustic monotonicity:** for equivalent source events and the same listener, a farther source does not arrive earlier under the simple distance-over-speed approximation.
9. **Invalid data:** non-finite times, positions, speeds, and invalid bounds are rejected or explicitly represented as unavailable.
10. **Representation continuity:** a future LOD adapter preserves event identity and response phase across tier transitions.
11. **Separation:** crowd intent scheduling tests run without requiring a renderer, GPU, Unity, WPF, or raWWar assembly.

Tests should assert semantics and tolerances, not a visually attractive outcome.

## Living visualization: evidence before illustration

The [crowd-response demo](../tools/crowd-response-demo/Program.cs) now runs the actual scheduler and serializes its returned output records into the checked-in [living SVG](assets/crowd-response-schedule.svg). CI regenerates it and uploads the generated SVG artifact. The first living scene shows:

- One event-origin marker and a shared time axis.
- Several clusters with deterministic but non-identical intent starts.
- Source-event time separated visually from listener acoustic-arrival time.
- Stable IDs and bounds, so replay can be compared across runs.
- A small table or annotation stating sample count, policy version, generator command, and test coverage.

The generator calls the real scheduler and serializes its output records. Participant IDs, event time, stable seed, and policy bounds are explicit fixture inputs; the plotted intent times, variants, and intensities come from scheduler results. The SVG is evidence for scheduling only. The speech-scene SVG remains conceptual, and this is not evidence of crowd movement or acoustic arrival.

## Implementation order

1. [x] Freeze a minimal input/output record and validation rules.
2. [x] Add the first pure CPU scheduler with no rendering or audio dependency.
3. [x] Author fixed reference-vector, replay, order-independence, bounds, invalid-input, and overflow tests; verify them in CI before treating the reference as accepted.
4. [x] Generate a living SVG directly from scheduler outputs after verification; CI regenerates and validates it. CI fails if the checked-in crowd-response SVG drifts from the actual generated scheduler output; other legacy SVG snapshots still report drift as a warning.
5. Integrate one small crowd in a consumer; distinguish scheduled intent from completed gesture and from audible arrival.
6. Profile staged population sizes and only then decide whether cluster aggregation, lower update rates, or packed/GPU representations are warranted.

The goal is not a crowd that merely looks large. It is a crowd whose behavior has identity, causality, replayable timing, and honest evidence.
