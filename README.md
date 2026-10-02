# Ants AR

Ants AR is a small Unity AR quiz for iOS and Android. It detects upward-facing horizontal planes, places an anchored miniature quiz world, and lets the player tap one of three answer platforms while an animated ant moves to the selected choice.

## Features

- AR plane scanning and single-tap world placement, with a pose fallback if anchors are unavailable.
- Three-choice questions with validated answer data and an explicit correct answer.
- Animated ant feedback, score/progress HUD, pinch-to-resize, and a reposition/reset control.
- Offline-first lesson loading from `Assets/StreamingAssets/config.json`; an optional HTTPS endpoint can refresh content.
- Optional Google Cloud Text-to-Speech. Gameplay remains usable without network access or a TTS credential.

## Requirements

- Unity **2021.3.19f1** (see `ProjectSettings/ProjectVersion.txt`).
- Android Build Support with SDK/NDK and OpenJDK, or iOS Build Support plus Xcode on macOS.
- An ARCore-capable Android device (Android API 24+) or an ARKit-capable iPhone/iPad.
- A camera-equipped device for AR testing. The Unity Editor does not provide a complete substitute for on-device AR validation.

## Open and run

1. Add this folder to Unity Hub using Unity 2021.3.19f1 and allow Package Manager to resolve packages.
2. Open `Assets/Scenes/SampleScene.unity` (it is the only enabled player scene).
3. Build and run on a supported AR device. On launch, move the camera slowly until a floor/table plane is detected, then tap the surface to place the world.
4. Tap one of the three platforms to answer. Pinch to resize the placed world; use **REPOSITION WORLD** to remove it and scan/place again.

The scene and models are committed as Unity assets. Unity-generated `Library`, `Temp`, build, and IDE files are intentionally excluded from Git.

## Lesson content

The bundled lesson is the source of truth for offline play. To change it, edit `Assets/StreamingAssets/config.json`. The project also keeps the previous remote lesson URL in the GameManager Inspector field; a valid HTTPS response refreshes the local lesson. If the endpoint is unavailable or invalid, the bundled lesson remains playable. Remote changes arriving during a round are applied at a question boundary.

Validate content before committing it:

```sh
python3 Tools/validate_lesson.py
python3 -m unittest discover -s Tools -p 'test_*.py'
```

See [docs/CONFIGURATION.md](docs/CONFIGURATION.md) for the JSON contract, backwards compatibility, and authoring guidance.

## Text-to-speech and credentials

Text-to-speech is optional. The project intentionally contains **no Google API key**. Client-side API keys are extractable from a mobile app; production speech should be routed through a secured backend that enforces quotas and protects credentials. See [docs/PRODUCTION_CHECKLIST.md](docs/PRODUCTION_CHECKLIST.md) before enabling speech in a release. If a key was previously used with a public build/repository, rotate and restrict it in Google Cloud.

## Build targets

- **Android:** ARCore, Internet permission (for optional remote config/TTS), minimum API level 24. Set a publisher-owned package ID and signing configuration before release.
- **iOS:** ARKit required; build on macOS with Xcode, then configure signing, privacy text, and a publisher-owned bundle ID.
- Only `Assets/Scenes/SampleScene.unity` is included in the player build. Platform configuration is under `ProjectSettings/`.

Detailed setup and release guidance: [docs/BUILDING.md](docs/BUILDING.md).

## Documentation

- [Architecture and runtime flow](docs/ARCHITECTURE.md)
- [Lesson configuration](docs/CONFIGURATION.md)
- [Build instructions](docs/BUILDING.md)
- [Testing and QA](docs/TESTING.md)
- [Production checklist](docs/PRODUCTION_CHECKLIST.md)
- [Suggested follow-up improvements](docs/ROADMAP.md)
- [Third-party assets and licensing notes](docs/THIRD_PARTY.md)

## Licensing

See the root [LICENSE](LICENSE) for this project. Bundled Asset Store packages, models, fonts, and plug-ins retain their own license terms; see [docs/THIRD_PARTY.md](docs/THIRD_PARTY.md) and the notices shipped with each asset. Do not assume that the root license grants redistribution rights to third-party content.
