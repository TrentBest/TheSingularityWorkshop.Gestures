# Gesture phase synchronization

**Status: implemented observation-only reference measurement.** This is a choreography-facing measurement contract. It does not add an oscillator, clock, coupling model, motion solver, or physical synchronization claim to Gestures.

## Ownership boundary

A host supplies one unwrapped phase observation for each stable participant ID at a common logical observation time. The source may be a music clock, a timing service, or a Physics-owned timing model. Gestures does not create or advance that source.

The boundary is intentionally one-way:

- The timing/Physics owner defines phase evolution and supplies observations.
- The host ensures observations refer to the same logical instant and compatible phase convention.
- Gestures computes a deterministic phase-concentration measurement that a choreography coordinator may inspect.
- The choreography/host decides what cue or procedure to schedule.
- Physics and the world remain authoritative for achieved movement, contact, balance, and other physical outcomes.

The Gestures core has no Physics dependency.

## Measurement

For participant phases \(\theta_i\), the implementation first wraps each phase into \[0, 2\pi)\ and computes the complex order parameter:

\[
Z = \frac{1}{N}\sum_{i=1}^{N} e^{\mathrm{i}\theta_i},
\qquad
R = |Z|,
\qquad
\bar{\theta}=\arg(Z).
\]

- \(R\) is the phase-concentration magnitude, bounded to [0, 1] after floating-point normalization.
- \(R\) near 1 indicates that the observed phases are concentrated.
- \(R\) near 0 indicates a dispersed or balanced phase distribution.
- The mean phase is marked undefined when the order parameter is numerically indistinguishable from zero.

The result preserves every participant's unwrapped phase and wrapped phase, sorted by ordinal stable participant ID. Duplicate IDs are rejected. The input population is bounded by a caller-configurable maximum (100,000 by default).

## What this does not prove

This is a single-instant measurement. It does **not** prove:

- phase locking over time;
- entrainment or causal influence between participants;
- mechanical or physical coupling;
- that a participant perceived a beat or intended to follow it;
- that a scheduled Gesture was physically achieved.

A high order parameter can occur because independent clocks happen to align at one instant. To classify phase locking, a future analyzer must inspect a time series, declare a window and tolerance, and distinguish bounded relative-phase drift from a brief coincidence. Frequency ratios and beat frequency also require a declared observation model; they are not inferred from one snapshot.

## Reference API

- `GesturePhaseObservation` — stable participant ID plus finite, unwrapped phase in radians.
- `GesturePhaseSynchronization.Evaluate` — validates and orders observations, wraps phases, and computes the order parameter and mean phase.
- `GesturePhaseSynchronizationResult` — immutable participant observations and aggregate measurement, including whether the mean phase is defined.

The API is deliberately independent of any oscillator implementation. A host can adapt Physics phase outputs into these observations without adding a Physics reference to the Gestures package.

## Verification cases

The test suite covers in-phase participants, evenly spaced phases, equivalent whole-cycle phases, negative-phase wrapping, deterministic order independence, duplicate/default IDs, non-finite values, and population-budget enforcement.

The next choreography milestone remains the two-person dance reference fixture: shared beat timebase, partner relationship, bounded individual expression, interruption, and deterministic replay. The phase-concentration result is one measurement for that fixture, not the fixture itself.

---

*The Singularity Workshop — edify, not mystify.*
