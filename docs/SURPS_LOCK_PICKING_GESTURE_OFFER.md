![The Singularity Workshop animated moniker](https://raw.githubusercontent.com/TrentBest/TheSingularityWorkshop.Renderer/development/docs/assets/moniker.svg)

# SURPS lock-picking Gesture offer

> **Status: executable provider-selection fixture.** The Gesture offer and its eligibility test are implemented. This is not a complete lock-picking animation, anchor-transform resolver, skill model, probability system, or authoritative lock simulation.

## Provider offer

The SURPS CORE Spying example uses the stable procedure identity:

`surps.core.spying.lock-picking.insert-and-turn.v1`

The provider offers the procedure only when the actor has both:

- `manual-dexterity`
- `lock-tool-use`

The environment must report the semantic anchor `lock-cylinder` as available. A richer environment can also provide `tool-grip`; that optional anchor is not currently required by the reference fixture.

The executable contract is in `ProviderToSurpsInteractionFixtureTests.SurpsLockPickingOfferRequiresDexterityToolUseAndLiveLockAnchors`. It composes the real `GestureOffer`, `GestureOfferSelector`, and `GesturePlayback` APIs. It proves that missing actor capability and missing environment anchor are reported independently, and that an eligible offer can be advanced by a host-supplied time step.

## Ownership boundaries

| Concern | Owner |
|---|---|
| Whether the character has a rated lock-picking capability | SURPS character model |
| Whether the environment exposes a lock and its interaction anchors | Environment / lock provider |
| Whether this physical procedure is eligible | Gestures offer selector |
| Which eligible action the player chooses | SURPS interaction policy |
| Pose progression and playback lifecycle | Gestures |
| Difficulty, chance, lock state, and success/failure | SURPS rules and authoritative lock domain |
| Character/environment presentation | Renderer / Visualize |

**An eligible Gesture is not a skill rating.** The selector checks declared requirements only. It does not calculate competence, chance, or character-point costs.

**A completed Gesture is not an unlocked lock.** Playback completion reports procedure progression. Only the lock domain may report whether the mechanism actually changed state. The preview must keep the probability/result display separate from the animation so that a successful-looking hand motion cannot silently grant success.

## Preview sequence expected by SURPS

1. Freeze the character capability/adversity snapshot and selected action.
2. Resolve the environment's lock difficulty and available semantic anchors.
3. Calculate and display the chance using the SURPS resolution policy.
4. Ask the player to confirm the action and spend any applicable character points before the attempt is committed.
5. Select and play the compatible Gesture.
6. Resolve the lock outcome from authoritative domain evidence.
7. Present the actual result and preserve an inspectable trace.

The probability calculation must not be recomputed from a changing character sheet halfway through the preview. Any point spend or situational change creates a new snapshot before commitment.

## Remaining integration

- Replace fixture-local anchor availability with a real environment provider response and resolved transforms.
- Add a lock-specific authored Gesture with meaningful hand/tool motion, rather than reusing the generic two-keyframe test helper.
- Connect the SURPS preview host to Gesture selection and playback.
- Calibrate the capability/difficulty probability model against the existing CORE Spying 2d6 procedure before adopting a percentile replacement.
- Connect the final result to an authoritative lock state transition and evidence trace.

![The Singularity Workshop](https://raw.githubusercontent.com/TrentBest/FSM_API/master/Documentation/Branding/TheSingularityWorkshop.png)
