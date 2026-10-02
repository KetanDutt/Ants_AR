# Build and device setup

## Unity setup

- Install **Unity 2021.3.19f1** with the platform support module you need.
- Open the project from Unity Hub and let Package Manager resolve `Packages/manifest.json` and `Packages/packages-lock.json`.
- Open `Assets/Scenes/SampleScene.unity`. It is the only enabled scene in `ProjectSettings/EditorBuildSettings.asset`.
- Confirm the scene still references the AR plane prefab, `SceneToSpawn`, ant variants, answer materials, and TMP font resources before making prefab changes.

## XR loader configuration

`Assets/XR/XRGeneralSettings.asset` maps Android to the ARCore loader and iOS to the ARKit loader, with automatic loader initialization and subsystem startup enabled on both targets. After opening the project and resolving its pinned XR packages, verify both mappings in **Edit → Project Settings → XR Plug-in Management**. Do not disable automatic initialization unless you also implement and test the full manual XR loader lifecycle.

## Android / ARCore

1. Install Android Build Support, Android SDK & NDK Tools, and OpenJDK through Unity Hub.
2. Set **File → Build Settings → Android** and switch platform if needed.
3. Use a physical, ARCore-supported device. Minimum API level is set to 24; check current ARCore device support and Unity's ARCore plug-in requirements before lowering it.
4. Keep Internet permission enabled only if using remote lesson refresh or cloud TTS. The project currently forces `android.permission.INTERNET` because both integrations use HTTPS.
5. Configure a unique package ID, application signing, target SDK, icons, and store metadata for the publishing account. The checked-in identifier is a project placeholder, not a claim of store ownership.

## iOS / ARKit

1. Install iOS Build Support and build on macOS with a supported Xcode installation.
2. `iOSRequireARKit` is enabled. Test on ARKit-capable hardware; the Editor cannot validate camera tracking.
3. Configure a publisher-owned bundle ID, team/signing profile, version/build number, app icons, privacy manifest requirements, and deployment target.
4. Review the camera usage description (`Required For AR.`) and replace it with concise, user-facing wording before release.
5. Validate suspend/resume, permission denial, tracking interruption, and background audio behavior on-device.

## Optional remote content and speech

- Content is local-first; configure `GameManager.configUrl` only to an HTTPS endpoint your team owns and can operate.
- TTS is disabled until a valid credential exists. Never store a production key in a prefab, scene, source file, CI artifact, or app bundle. Prefer a server-side proxy with quotas, app attestation/restrictions, and request validation.
- The bundled Google TTS integration uses API v1 rather than its beta endpoint. If you change the provider or plug-in version, test error handling and audio decoding on both Android and iOS.

## Pre-release sanity check

- Run the Python validation commands in [TESTING.md](TESTING.md).
- Build both target platforms from a clean Library/cache and inspect Unity Console/build logs for missing scripts, serialization warnings, shader errors, and null references.
- Exercise the full AR flow on devices, including reset/reposition and offline launch.
- Complete [PRODUCTION_CHECKLIST.md](PRODUCTION_CHECKLIST.md); this repository does not contain store signing assets, a managed TTS backend, or device-specific performance measurements.
