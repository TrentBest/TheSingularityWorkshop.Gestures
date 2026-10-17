# FSM_COS LLM — Manifest and MicroBundle Configuration Alignment

> **Request to the FSM_COS LLM:** please work with the Gestures owner on the real manifest-driven AnyApp proof. This is a cross-repository coordination request, not a claim that a runtime manifest or bundle configuration is already executable.

## Goal

Prove the user journey:

1. A user opens the WebPage experience and selects a named production/scene.
2. The browser offers a download/install/launch path for AnyApp; browser security means a normal web page cannot silently launch an arbitrary local executable. The supported handoff must be an explicit user action using an installed protocol handler, an opened/downloaded manifest file, or another intentionally installed local bridge.
3. AnyApp receives or opens a specific runtime manifest.
4. AnyApp supplies a catalog/resolver and a configuration source to FSM_COS.
5. FSM_COS resolves the manifest's requested MicroBundle roots and dependency closure, supplies per-bundle configuration bytes, loads bundles, arbitrates to convergence, and returns a RuntimeAssembly.
6. AnyApp reports exactly what was resolved, configured, loaded, and handed off; it must not imply that composition alone executes the experience.

## Verified FSM_COS development contract

The inspected `docs/RUNTIME_MANIFEST.md` and `docs/CONSUMING_MICROBUNDLES.md` on FSM_COS development establish:

- A `RuntimeManifest` requests root MicroBundle IDs and versions; it is not the configuration file.
- FSM_COS discovers dependencies from bundle declarations.
- Configuration is a separate `IMicroBundleConfigurationSource.TryGetConfiguration(runtimeId, bundleId, version, out bytes)` boundary.
- No external configuration means the bundle uses its own defaults.
- FSM_COS does not parse JSON or choose the storage format.
- The catalog/resolver locates artifacts; FSM_COS composes them and returns a `RuntimeAssembly`.
- The manifest's root version is checked against the resolved root. Dependency/version policy must not be invented by a host-side JSON convention.

References:
- [Runtime Manifest](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/RUNTIME_MANIFEST.md)
- [Consuming MicroBundles](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/blob/development/docs/CONSUMING_MICROBUNDLES.md)
- [Minimal Consumer sample](https://github.com/TrentBest/TheSingularityWorkshop.FSM_COS/tree/development/samples/FSM_COS.MinimalConsumer)

## Needed from the FSM_COS/AnyApp side

Please confirm with concrete source paths, tests, and commits:

1. **Exact runtime manifest import format.** The current core contract is a C# object; which serializer/transport envelope, if any, is actually implemented for loading a manifest from a local file or command line? Do not treat a proposed JSON representation as implemented until a real loader and round-trip test exist.
2. **Stable bundle identity and versions.** Provide the actual `ulong BundleId` and version for each MicroBundle that the first vertical slice will request as a root. Show the catalog's actual resolution source. Do not invent IDs or use package names as though they were bundle IDs.
3. **Configuration file convention.** Confirm the local configuration directory/layout and how `runtimeId`, `bundleId`, and resolved `version` map to bytes. Provide a tested `IMicroBundleConfigurationSource` implementation or identify the exact consumer that owns it.
4. **Per-bundle config contracts.** For each requested bundle, name the configuration schema and version, defaults, validation rules, and required/optional fields. The provider owns its schema; FSM_COS should remain format-neutral.
5. **One runnable end-to-end proof.** AnyApp opens a named manifest, resolves actual bundles, passes configuration bytes, calls FSM_COS, and reports the resulting assembly plus failures. Include a test for missing configuration/default behavior and a test for an unresolved root.
6. **Branch discipline.** Keep work on `development`; keep `master` untouched; do not publish packages.

## Gestures provider boundary

Gestures is currently an engine-neutral package with a CPU reference evaluator; it is **not yet confirmed as an independently loadable MicroBundle artifact**. The first manifest must not list Gestures as a root merely because it is a NuGet package. First identify the provider MicroBundle that owns the procedure definitions, stage/take fixture, and bindings, and confirm how it references or packages the Gestures evaluator.

The first useful studio proof should request only the real roots required to assemble a small take. Candidate capability concepts (not confirmed bundle IDs) are:

- studio stage/take data provider;
- performer/crew/equipment procedure provider, using Gestures where appropriate;
- any required host-neutral shared contracts;
- optional Renderer/Visualize presentation capabilities only if they truly participate in FSM_COS composition today.

Do not list AnyApp, WPF, Blazor, browser launch logic, or host UI as a MicroBundle unless the real domain contract and implementation prove that classification is correct.

## Acceptance checklist

- [ ] Actual manifest artifact is opened by AnyApp through a deliberate user action.
- [ ] Manifest roots and exact versions correspond to catalog-resolvable MicroBundles.
- [ ] Each root's external configuration has a provider-owned schema and validated example file, or intentionally uses defaults.
- [ ] Configuration source tests lookup by runtime ID, bundle ID, and version.
- [ ] Missing root, invalid configuration, and arbitration non-convergence produce actionable errors.
- [ ] Successful composition returns a RuntimeAssembly whose actual contents are reported.
- [ ] AnyApp distinguishes “composed/ready” from “executing/rendering.”
- [ ] Browser-to-desktop launch behavior is documented honestly and requires user consent.
- [ ] Build/tests pass on development; no package publication.

## Decision requested

Please respond with the actual first-proof bundle IDs/versions, catalog mechanism, manifest-file import path, and configuration-source implementation. Then the Gestures side can author the correct per-bundle configuration files instead of inventing a parallel manifest or a speculative JSON schema.
