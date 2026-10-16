# Stateful IK hierarchy benchmarks

## Purpose

Measure the current CPU hierarchy prototype before changing its evaluation strategy. The benchmark compares:

- **Full reevaluation** — recalculates every world transform and acts as the baseline.
- **Incremental no-op** — evaluates an unchanged hierarchy after it has already been evaluated.
- **Incremental leaf change** — changes one leaf hinge and evaluates the resulting dirty path.

BenchmarkDotNet reports elapsed-time statistics and, through `MemoryDiagnoser`, managed allocation data. Each benchmark returns a value so the operation remains observable. The leaf-change case alternates the requested angle so each invocation changes retained state.

## Workloads

The harness covers chain hierarchies at 10, 100, and 1,000 joints, and balanced binary branching hierarchies at 10, 100, 1,000, and 10,000 joints. The deep-chain workload intentionally stops at 1,000 for this first pass: evaluation is currently recursive, so very deep chains need a separate stack-safety investigation rather than being treated as a routine benchmark size.

Existing unit tests compare incremental poses against full reevaluation across deterministic angle changes and branching structures. Run those tests before interpreting benchmark numbers; the benchmark itself measures cost, not correctness or physical realism.

## Run

From the repository root:

```sh
dotnet run --project benchmarks/TheSingularityWorkshop.Gestures.Benchmarks -c Release
```

BenchmarkDotNet writes machine-readable reports under `BenchmarkDotNet.Artifacts/results`. Keep the generated reports with the machine/runtime details when comparing revisions. For meaningful comparisons, use the same machine, power profile, .NET runtime, build configuration, and benchmark filters; avoid comparing CI timings against local desktop timings.

## Interpretation boundaries

- No-op incremental evaluation still traverses the hierarchy to discover unchanged nodes; zero recalculated transforms does not mean zero work.
- A leaf-only change should recalculate one transform, but traversal cost still depends on hierarchy shape and size.
- The benchmark does not measure a physics simulation, collision/contact resolution, whole-body balance, GPU execution, or end-to-end application frame time.
- Allocation and elapsed time must be read from actual benchmark output. No performance advantage is claimed in advance.
- The current recursive evaluator is a correctness reference. If traversal dominates, the next experiment should compare explicit iterative traversal and dirty-node scheduling while preserving full-evaluation pose parity.
