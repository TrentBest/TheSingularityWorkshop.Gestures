# Skill distribution sample model

`SkillDistributionDemo` is the first executable data-model step toward the skill-distribution illustration. It generates deterministic 2D sample data and summary statistics; it does not render a live chart or SVG, simulate a body, model a weapon, or predict real-world accuracy.

## Reproducible inputs

`Generate(seed, sampleCount, movementSpread, outcomeNoise)` takes:
- **Seed:** initializes a small fixed Xorshift32 generator so identical inputs produce identical samples independently of runtime `Random` implementation details.
- **Sample count:** number of generated movement/outcome pairs; must be positive.
- **Movement spread:** half-width of the uniform movement-offset range on each axis, around an ideal origin.
- **Outcome noise:** half-width of a separate uniform offset range on each outcome axis, added after movement is sampled.

All coordinates are arbitrary, dimensionless illustration units. This deliberately simple relationship is an explanatory model, not a calibrated mapping from a human movement to a physical outcome.

## Metrics remain separate

- **Mean pose error:** average Euclidean distance from each movement sample to the ideal origin. This describes deviation from the ideal, not sample-to-sample consistency.
- **Movement variance:** mean squared 2D distance of movement samples from their sample mean. This describes dispersion in the movement samples.
- **Outcome spread:** mean squared 2D distance of outcome samples from their sample mean. This describes dispersion after the separate outcome-noise stage.

Variance and spread are reported in squared illustration units. Mean pose error is reported in illustration units. The means are included so a future view can distinguish a consistently biased cluster from a centered one; a tight distribution can still be systematically offset from the ideal.

These statistics are intentionally not collapsed into one “skill score.” Consistency is only one axis of competence, and real outcome quality would require domain-owned observations and a validated model.

## Current boundary

The model and unit tests are executable. The generated SVG and standalone interactive HTML both consume the sample model's equivalent deterministic algorithm; neither is a physical simulation or a validated human-performance predictor. The HTML exposes seed, sample count, movement spread, and outcome noise. Both panels rescale together so the largest supported outcome envelope remains visible, and the broad spread is kept at least as wide as the narrow spread.
## SVG comparison renderer

`SkillDistributionSvgRenderer.RenderComparison(seed, sampleCount, narrowMovementSpread, broadMovementSpread, outcomeNoise)` returns a standalone SVG string. Both panels use the same seed and sample count; only the movement-spread parameter differs, while outcome noise is held constant. The view includes the ideal ghost marker and aim line, target bullseye, movement sample points, outcome points, and all three metrics.

The SVG is generated from the executable sample model rather than hand-authored point positions. Its shared plot scale reserves room for movement spread plus outcome noise, keeping the full supported distribution within the panels. It does not animate movement or establish physical validity. Consumers can save the returned string as an `.svg` file or embed it in a host view.

## Interactive HTML demo

Open [`skill-distribution-demo.html`](skill-distribution-demo.html) in a browser. It has no external dependencies and regenerates both plots when the controls change. The comparison is deterministic for a given seed and input set. The plots share a scale based on the broad movement spread plus outcome noise, so the widest allowed outcome envelope stays visible rather than being silently clipped.

The browser page mirrors the C# Xorshift32 sample-generation order. Small floating-point differences in derived metrics may still occur across runtimes; the visualization is illustrative, not a cross-platform numerical certification.