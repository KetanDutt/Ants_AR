# Testing and QA

## Automated checks available in this repository

The repository includes a dependency-free Python validator and unit tests for the bundled lesson data:

```sh
python3 Tools/validate_lesson.py
python3 -m unittest discover -s Tools -p 'test_*.py'
```

The validator checks root/array shape, prompt text, the three visible choices, and that the correct answer is one of those choices. It also covers the old `Answers` property and the legacy correct-answer rule. These checks do not compile or run Unity code.

## Unity Editor checks

In Unity 2021.3.19f1:

1. Open the project and wait for packages/shaders to import.
2. Verify there are no missing script references in `SampleScene`, `SceneToSpawn`, the ant prefabs, or `TTS.prefab`.
3. Check that `SampleScene` has exactly one active `PlaceObject` controller, one AR Session, one AR Session Origin, one GameManager, and an EventSystem.
4. Confirm the Canvas root scale is `(1, 1, 1)`, the help text has raycast disabled, and the reset button is generated only after a world is placed.
5. Inspect each ant variant for an Animator with `Happy Idle` and `JumpingLoop` states; verify all question platform references and materials.
6. Review build settings and make a Development Build before a release build.

The Editor scene may not receive real AR tracking. Use an AR Foundation simulation/device simulator only if installed and configured for this Unity version; do not treat a mouse-only pass as device QA.

## On-device acceptance test

- Install on an ARCore/ARKit-capable device, grant camera access, and verify tracking starts.
- Launch with airplane mode enabled. The bundled lesson should display without waiting for the remote endpoint or TTS.
- Scan a flat horizontal surface and place once. A single touch must not create duplicate worlds/anchors.
- Tap each of the three platforms; check correct/incorrect colors, ant movement, score, question progression, and interaction lock while the ant is moving.
- Pinch to resize at several scales. Confirm the world remains visible and does not invert or become unreasonably large/small.
- Use **REPOSITION WORLD** while the world is present. Confirm the previous world/anchor is removed and a new placement works.
- Tap the HUD/reset area and confirm it does not submit an answer.
- Restore network access and verify a valid remote lesson is accepted; make the endpoint unavailable and confirm bundled content still works.
- If TTS is enabled in a test environment, verify an empty/invalid key gives a useful failure and does not break text interaction or leak credentials to logs.
- Test camera permission denial, app background/resume, AR tracking loss, screen safe areas, different aspect ratios, and low-power/thermal behavior.

## Test gap

No Unity Editor executable/device runner is present in the development environment used for this change. The C# changes and Unity YAML are therefore not represented as Unity compile/play-mode test results; open and validate the project with the pinned Editor and target devices before publishing.
