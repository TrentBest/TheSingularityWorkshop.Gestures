# Integration Contract

## raWWar

raWWar owns game-specific behavior and authoritative world state. A soldier's behavior decides that boarding is needed; the vehicle capability provides the physical procedure and interaction anchors. Simulation/behavior code handles approach movement, alignment, collision authority, interruption, and the authoritative state transition when the procedure succeeds or fails.

Gestures supplies reusable procedure data and CPU reference evaluation. It does not require raWWar types.

## Renderer

Renderer selects the representation and fidelity appropriate for the observer/event horizon. It may request full local transforms nearby and a reduced pose/instance representation at distance. That policy should not be hard-coded into Gesture definitions.

## Visualize

Visualize consumes a presentation projection and executes GPU-facing work. It should not need to understand why a soldier is entering a cockpit; it needs evaluated data and the chosen representation.

## Suggested data path

```text
Authoritative world + FSM/API behavior
                 │
                 ├── interaction/physics owns validity and world movement
                 ▼
       Gesture Provider selects definition
                 ▼
       CPU or optimized Gesture evaluation
                 ▼
        evaluated pose / instance data
                 ▼
      Renderer selects event-horizon fidelity
                 ▼
          Visualize backend presents
```

The order may change as profiling reveals better batching. The responsibility boundaries must not.

## API change discipline

Any cross-repository contract should be discussed before introducing a hard package dependency. Prefer small adapters at package boundaries. The first Gestures release should remain usable independently of Renderer and Visualize, and raWWar should be able to test the CPU reference evaluator without a graphics device.
