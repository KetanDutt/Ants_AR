# Architecture

## Runtime flow

```text
SampleScene
├── AR Session + AR Session Origin
│   ├── ARPlaneManager / ARRaycastManager / ARAnchorManager
│   └── PlaceObject
├── GameManager + DataLoader
├── Canvas + EventSystem
└── TTS AudioSource + TextToSpeech + Google Cloud TTS plug-in

PlaceObject ──valid upward plane hit──> anchored SceneToSpawn
GameManager ──lesson config───────────> SceneScript + HUD
SceneScript ──three platform choice──> ant animation + score + next question
```

1. `GameManager` requests the bundled `StreamingAssets/config.json` through `UnityWebRequest`. This path is supported on Android/iOS where `StreamingAssets` may not be a normal filesystem directory.
2. A valid bundled lesson is applied immediately. The optional HTTPS refresh runs afterward and never blocks placement. A refresh received during a round is held until the next question boundary.
3. `PlaceObject` listens to the first Enhanced Touch finger, raycasts detected planes, ignores UI, and places on the nearest upward-facing horizontal plane. It uses an AR plane anchor when available and falls back to the hit pose when the provider cannot create an anchor.
4. `SceneScript` owns the ant and the three platform destinations. Physics hits are accepted only when they come from one of those platform hierarchies. The feedback animation updates `GameManager` once, then advances the question.
5. The HUD displays placement help, current question, progress, and score. Its reset button destroys the placed world/anchor and returns to placement mode. The HUD text does not block AR taps.
6. `TextToSpeech` is an optional adapter around the bundled Frostweep Games Google Cloud plug-in. It deduplicates pending speech, caches a bounded number of clips, handles synthesis failures, and uses the stable API overload. Text remains visible if speech is unavailable.

## Important code ownership

- `Assets/Scripts/`: project gameplay and content-loading code.
- `Assets/ExampleAssets/`: AR Foundation sample plane visualizer and a safe standalone anchor-marker example. The marker example is not attached to the main scene.
- `Assets/FrostweepGames/`: third-party TTS plug-in. A few project-specific safety changes remove a shipped key/log leak and disable the obsolete beta API configuration; preserve upstream notices and license terms.
- `Assets/Plugins/CW`, `Assets/Plugins/Demigiant`, `Assets/Joystick Pack`, `Assets/TextMesh Pro`, and the art folders are bundled dependencies/content. Review their individual licenses before redistribution.

## Scene/prefab contract

- `SampleScene` must keep an AR camera, AR Session, AR managers, `GameManager`, `DataLoader`, a TMP HUD, and an EventSystem.
- `PlaceObject.prefab` must point to `Assets/Prefab/SceneToSpawn.prefab`.
- `SceneToSpawn` must have `SceneScript`, one placeholder ant under the ant spawn point, three answer platform roots, three child TMP answer labels, three jump points, and the four answer-state materials.
- Each ant prefab in the GameManager's `Ants` array needs an `Animator` with `Happy Idle` and `JumpingLoop` states.
- The answer world currently has exactly three interactive choices. Content validation therefore checks the first three answers.

## Lifecycle and state

`GameManager.WorldPlaced` is true only after a valid world and ant are initialized. `PlaceObject` owns the optional `ARAnchor` and placed instance and clears both on reset. `GameManager` and `PlaceObject` are scene-scoped singletons; they are intentionally not kept alive across scene unloads because their AR camera/scene references are scene-local. The speech plug-in object is managed separately by its own singleton.
