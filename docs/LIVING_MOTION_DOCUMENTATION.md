# Living motion documentation

Gestures teaches reusable animation principles. A gesture describes motion over time; it does not need to know whether its target is a soldier, tank, spaceship, tool, or abstract transform. The target's behavior and any physical simulation remain owned by their respective systems. Shared evaluation operations can later be batched across compatible targets, including on the GPU, without baking object-specific assumptions into Gestures. That batching is a future optimization, not a claim about the current CPU reference evaluator.

The documentation is part of the product experience: it should help readers *see* what the current implementation does, understand why the behavior matters, and follow the evidence back to real code.

## Current generated scenes

### 1. Named transform tracks

![Three evaluated named-transform tracks](assets/gesture-motion.svg)

The C# demonstration creates one `GestureDefinition` with three named transforms and samples `GestureEvaluator.Evaluate` 49 times. These are generic transform targets chosen to make the motion principles visible; their names do not make this a vehicle, character, or tool behavior. FSM_API coordinates the demo lifecycle. The path geometry and animated marker position, rotation, and scale are serialized from those evaluated outputs.

**What this demonstrates:** keyframe data, normalized progress, linear position interpolation, quaternion rotation interpolation, scale interpolation, and how evaluated data can feed presentation.

**What this does not demonstrate:** parent-child composition, an articulated rig, IK, collision response, physical force integration, or a vehicle/character physics model. The transform names are dictionary keys; the current evaluator does not infer a mechanical assembly from them.

### 2. Motion channels over time

![Evaluated root-transform channels](assets/gesture-channels.svg)

The second scene uses the same 49 samples to plot root X, root Y, rotation, and scale against normalized progress. The moving cursor connects the time axis to all four plots. This helps a reader see that a transform is a collection of channels, not only a dot moving along a path.

**What this demonstrates:** the relationship between the sampled evaluator output and the visual presentation of individual transform properties.

**What this does not demonstrate:** force, acceleration, momentum, damping, contact, or a physical solver. Those values are not outputs of the current evaluator and must not be invented for an attractive diagram.

## Scene contract

Every living scene added here should:

1. **Start from real data.** Prefer a C# executable using the package under demonstration. Use FSM_API only where it actually coordinates a lifecycle or process.
2. **Keep provenance visible.** State which implementation generated the output, how many samples were evaluated, and which assumptions shape the picture.
3. **Separate model from presentation.** SVG may animate serialized samples, but must not imply that the browser is executing C# or running the package.
4. **Teach one main idea at a time.** Pair motion with a graph, state timeline, comparison, or annotations only when that extra view helps explain the same behavior.
5. **Respect accessibility.** Provide a meaningful title and description, sufficient contrast, labels that do not rely on color alone where practical, and a static fallback for `prefers-reduced-motion`.
6. **Remain reproducible.** Generate the committed asset in CI, validate SVG XML, upload the artifact, check whitespace, and fail when committed output drifts from generator output.
7. **State limits, not aspirations.** Do not portray a future solver, hierarchy, interaction, or physics law as implemented.

## Next scenes to build

These are priorities for future iterations, not claims about current functionality.

| Priority | Scene | Evidence required before drawing |
|---|---|---|
| 1 | Keyframes → interpolated transform | Real keyframe definitions and evaluator samples; show keyframes separately from intermediate samples |
| 2 | Deterministic individual variation | Run the actual evaluator with fixed identity seeds and declared variation bounds; show repeatable differences, not random decoration |
| 3 | Progress and lifecycle | Real FSM_API transitions plus evaluator progress/completion values, including interruption only if the actual state model supports it |
| 4 | Multiple transforms with actual hierarchy | A documented parent/child composition contract and a tested evaluator/adapter that resolves it; named dictionary keys alone are insufficient |
| 5 | Dynamic response to forces/contact | A real, tested motion model producing state updates from declared forces, environment, and contact inputs; never substitute a hand-authored slip or impact animation |
| 6 | Baked motion-model data | A reproducible offline bake, metadata for its assumptions/domain, runtime interpolation, and explicit handling of out-of-domain inputs |

## Reuse and batching: operations, not object names

The long-term performance goal is to make detailed motion affordable at scale. Prefer grouping targets by compatible evaluation work, data layout, and fidelity—not by labels such as `tank` or `soldier`. Different kinds of targets may share interpolation or other motion kernels; targets of the same kind may still be running different behaviors. A GPU implementation should batch only work whose semantics and data contract are compatible, with the CPU reference evaluator remaining a correctness baseline.

This is an architectural direction, not current functionality: the present evaluator is a CPU reference implementation, and these SVGs do not demonstrate GPU execution or performance. A future performance scene should show measured work and output from an actual batch implementation, alongside correctness comparisons and the tested workload.

## Future physics scenes: causality, not labels

When a genuine motion-model implementation exists, a good demonstration should show one shared environmental event affecting several participants differently because their real model parameters and states differ. For example, a wave should not issue a “boat wobble” visual command. The wave/environment model should produce an interaction; each vessel model should compute its response; any collision or downstream effect should follow the implemented interaction contract.

Expensive calculations may be baked at development time when the assumptions and validity domain are declared. Runtime still needs to account for changing inputs such as speed, loading, orientation, damage, and disturbances. A precomputed table is evidence only inside the domain it was designed to cover.

## Regenerate and validate

From the repository root:

```bash
dotnet run --project tools/living-demo/living-demo.csproj --configuration Release -- --output docs/assets/gesture-motion.svg
python -c "import xml.etree.ElementTree as ET; ET.parse('docs/assets/gesture-motion.svg'); ET.parse('docs/assets/gesture-channels.svg')"
```

The C# generator writes both SVG assets. The workflow checks the committed assets against the generated output. The browser only plays the serialized results; it does not execute FSM_API or GestureEvaluator.
