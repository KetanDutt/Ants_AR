# Third-party content and license notes

This repository contains assets beyond the code covered by the root `LICENSE`. Review the original license notices and terms before redistributing the project, models, fonts, shaders, plug-ins, or extracted build content.

| Folder/package | Use in this project | Notes |
|---|---|---|
| Unity AR Foundation / ARCore / ARKit packages | Plane detection, raycasts, anchors, session/camera | Unity package terms apply; package versions are pinned in `Packages/manifest.json` and `packages-lock.json`. |
| Universal Render Pipeline, UGUI, TextMesh Pro | Rendering and HUD | Unity package terms apply. TMP includes its own notices/resources. |
| Frostweep Games Google Cloud Text-to-Speech | Optional speech synthesis | Asset Store EULA applies; package notes are in `Assets/FrostweepGames/GCTextToSpeech/README.txt` and `Assets/FrostweepGames/LICENSE_INFO.txt`. Project changes remove an exposed sample key/log and select API v1. |
| Newtonsoft.Json | TTS plug-in serialization | The bundled `Assets/FrostweepGames/_Generic/3rdParty/JsonDotNet/license.txt` contains its MIT license notice. |
| Lean Touch / Lean Common (CW) | Bundled interaction utilities | Third-party licensing/attribution may apply; inspect package files before distribution. |
| DOTween (Demigiant) | Ant jump animation | Review the included plug-in license and DOTween distribution terms. |
| Joystick Pack | Bundled but inactive in the current gameplay scene | Inspect its asset/license terms if removing or redistributing it. |
| Ant models, animation FBXs, Simple Nature Pack models/textures, and fonts | Visual content | Verify each asset's original author/license and redistribution rights. The root project license does not automatically grant these rights. |

The project does not include a new open-source license grant for third-party packages. This table is an inventory aid, not legal advice or a replacement for the original licenses.
