# Crowd-response living SVG

This tool runs the library's real `CrowdResponseScheduler` and serializes its returned schedule records into `docs/assets/crowd-response-schedule.svg`.

From the repository root:

```bash
dotnet run --project tools/crowd-response-demo/crowd-response-demo.csproj --configuration Release -- --output docs/assets/crowd-response-schedule.svg
```

The event ID, event-origin time, participant IDs, stable seed, and policy bounds are explicit fixture inputs. Intent timestamps, variants, and intensities are returned by the scheduler. The SVG sorts records by intent time for readability; sorting does not affect the scheduler's output.

This is a living schedule visualization, not a crowd simulation. It does not demonstrate completed gestures, body motion, collisions, acoustic propagation, or population-scale performance. The speech-scene illustration remains conceptual.
