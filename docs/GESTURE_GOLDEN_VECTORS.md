# Deterministic golden vectors and numeric tolerance

This note defines a small reference policy for the current CPU evaluator. It is intended to make regressions visible without claiming cross-platform bitwise identity or physical validity.

## Golden fixture

`GestureEvaluatorGoldenVectorTests` evaluates an unvaried, one-second translation from root position `(0, 0, 0)` to `(10, 0, 0)`. At normalized progress 0, 0.25, 0.5, 0.75, and 1, the expected X coordinates are 0, 2.5, 5, 7.5, and 10. Negative elapsed time clamps to the start. With no variation configured, root offset remains zero.

Run the focused fixture:

```sh
dotnet test TheSingularityWorkshop.Gestures.slnx --configuration Release --filter FullyQualifiedName~GestureEvaluatorGoldenVectorTests
```

## Numeric comparison policy

- **Same build and same inputs:** repeatability tests should compare the returned values exactly when the test exercises deterministic arithmetic without relying on platform-sensitive transcendental behavior.
- **Golden scalar/vector fixtures:** use an absolute tolerance of `1e-5` for the tested float position components, and eight decimal places for the normalized progress double in this simple fixture.
- **Rotations:** compare orientation, not raw quaternion components, in future fixtures; quaternions `q` and `-q` encode the same orientation. Establish the angular tolerance alongside representative rotation cases before treating it as a compatibility promise.
- **CPU/GPU or cross-runtime parity:** no parity guarantee is established by this fixture. Define backend-specific tolerances only after a GPU/reference comparison exists and records its input domain, error metric, and failure cases.

Golden vectors describe evaluator outputs, not successful contact, support, collision-free motion, or any other physical outcome. Keep them small and explicit; add more only where a stable contract has been chosen.
