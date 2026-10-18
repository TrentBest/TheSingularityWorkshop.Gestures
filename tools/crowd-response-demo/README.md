![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Crowd-response living SVG

This tool runs the library's real `CrowdResponseScheduler` and serializes its returned schedule records into `docs/assets/crowd-response-schedule.svg`.

From the repository root:

```bash
dotnet run --project tools/crowd-response-demo/crowd-response-demo.csproj --configuration Release -- --output docs/assets/crowd-response-schedule.svg
```

The event ID, event-origin time, participant IDs, stable seed, and policy bounds are explicit fixture inputs. Intent timestamps, variants, and intensities are returned by the scheduler. The SVG sorts records by intent time for readability; sorting does not affect the scheduler's output.

This is a living schedule visualization, not a crowd simulation. It does not demonstrate completed gestures, body motion, collisions, acoustic propagation, or population-scale performance. The speech-scene illustration remains conceptual.

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
