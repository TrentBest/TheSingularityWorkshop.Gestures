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

The model and unit tests are executable, but the two-panel SVG remains a static concept image. The interactive/visual demonstration is **not complete** until a view consumes these generated samples and metrics, exposes the seed/spread inputs, and displays the distributions without implying physical validation.