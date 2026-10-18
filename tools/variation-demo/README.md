![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# Living variation scene

This generator turns the real `GestureExperienceProfile.Sample` and `GestureEvaluator.Evaluate` outputs into an animated SVG.

From the repository root:

```bash
dotnet run --project tools/variation-demo/variation-demo.csproj --configuration Release -- --output docs/assets/gesture-variation.svg
python -c "import xml.etree.ElementTree as ET; ET.parse('docs/assets/gesture-variation.svg')"
```

The scene samples two explicitly authored demonstration profiles with fixed identity seeds at 81 elapsed-time samples each. The marker position uses the evaluator's returned normalized progress and separate `RootOffset.X` output. Its animation serializes those samples; the browser does not execute the C# library.

## What it demonstrates

- Stable, seed-derived variation can change the progression speed while remaining repeatable for fixed inputs.
- The evaluator returns lateral root/presentation offset separately from named transforms.
- The profile ranges are caller-authored demonstration values, not empirical measurements or built-in skill rankings.

## What it does not demonstrate

This is not per-joint competence, a pose-quality score, a contact or balance model, or evidence that veterans are physically more capable. The profile does not change the ideal Gesture. The SVG is an illustration of the currently supported speed and lateral-offset channels only.

The build workflow generates this SVG, validates its XML, and uploads it with the other living scenes. Snapshot drift is currently reported as a warning rather than enforced as a failing gate; see [Living Motion Documentation](../../docs/LIVING_MOTION_DOCUMENTATION.md).

---

<p align="center">
  <a href="https://github.com/TrentBest/TheSingularityWorkshop" aria-label="The Singularity Workshop living avatar">
    <img src="https://raw.githubusercontent.com/TrentBest/WebPage/development/TheSingularityWorkshop/wwwroot/Images/TheSingularityWorkshopLivingAvatar.svg" alt="The Singularity Workshop living avatar — a digital vortex converging on its central eye" width="112" />
  </a><br />
  <strong>The Singularity Workshop</strong><br />
  <em>If a picture is worth a thousand words, what is a living picture worth?</em><br />
  <a href="https://github.com/TrentBest">GitHub</a> · <a href="https://www.patreon.com/TheSingularityWorkshop">Support the Workshop</a>
</p>
