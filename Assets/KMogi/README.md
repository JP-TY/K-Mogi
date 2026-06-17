# K-Mogi — Showcase MVP

A working prototype of the K-Mogi instructor trainer that demonstrates the core concepts from
the PRD, runnable entirely in the Unity Editor with no headset. The SLA "brain" (parsing → PT
evaluation → cognitive metrics → telemetry) is real and unit-tested; the device/ML dependencies
are clean seams with mocks.

## Run the demo (editor, no headset)

1. Open `Assets/Scenes/Main VR Scene.unity`.
2. Menu: **KMogi → Setup Demo Scene** (adds a `KMogiBootstrap` GameObject).
3. (Optional) Select the GameObject and assign a Japanese TMP font (see below).
4. Press **Play**.

### Controls (editor simulator)

- **Space** or controller trigger — deliver the next lesson line
- **R** — correct a learner error with a *recast* (implicit; lowers anxiety)
- **E** — correct with an *explicit* over-correction (raises anxiety)
- **Right Mouse + drag** — look around the three spatial screens (drives the gaze metric)

You'll see live morphological analysis, the student avatar reacting (color/tilt/face) when input
is beyond its Processability stage or delivered too fast, and the telemetry panel tracking Teacher
Talk Time and gaze distribution.

## Architecture

- `Core/` — `KMogi.Core` assembly, **no UnityEngine dependency** (pure C#, unit-tested):
  - `Parsing/` — `IMorphologicalParser`, `Token`, `SovSyntaxValidator`
  - `Sla/` — Processability stages, `PtFeatureDetector`, `PtEvaluator`, `MoraCounter`,
    `PacingMonitor`, `CognitiveState`
  - `Telemetry/` — `TttCalculator`, `GazeQuadrantAccumulator`
- `Runtime/` — `KMogi.Runtime` (Unity glue): platform adapters, parser/speech wrappers, avatar,
  dialogue, spatial UI, and the `SessionController` / `KMogiBootstrap` demo loop.
- `Editor/` — `KMogiBuild` (Android build) and the demo-scene menu item.
- `Tests/` — EditMode tests for the Core.

The Separation Mandate is enforced: gameplay/evaluation code only touches hardware through
`IPlatformDeviceAdapter` (Quest / Rokid / Editor implementations).

## Run the tests

EditMode tests cover the Core brain. In the editor: **Window → General → Test Runner → EditMode → Run All**.
Headless:

```bash
Unity -runTests -batchmode -projectPath . -testPlatform EditMode -testResults results.xml
```

## Build the APK

Menu **KMogi → Build Android APK**, or headless:

```bash
Unity -batchmode -projectPath . -executeMethod KMogi.Editor.KMogiBuild.BuildAndroid -quit
```

Output: `Build/KMogi_Target.apk`.

## Swapping in the real dependencies (documented seams)

| Concern | Default (now) | To enable the real thing |
|---|---|---|
| **Japanese font** | **Bundled** — Noto Sans JP SDF (`Resources/NotoSansJP-Regular SDF.asset`, ~7,130 glyphs) is auto-loaded; full Japanese renders | Assign a different CJK `TMP_FontAsset` on `KMogiBootstrap` to override |
| **Morphological parser** | `FixtureMorphologicalParser` (curated lexicon) | Add NMeCab + an IPADIC dictionary under `Assets/StreamingAssets/ipadic/`, define `KMOGI_NMECAB`, construct `NMeCabParser` |
| **Speech-to-text** | `MockTranscriber` (text supplied directly) | Implement `SentisWhisperTranscriber` with a Whisper model via Unity Sentis (`com.unity.ai.inference`) |
| **Rokid runtime** | `RokidAdapter` stub (flat layout, no tracking) | Import Rokid UXR2.0 SDK, bind `RKCameraRig` + air-mouse pointer in `RokidAdapter` |

> **Note on the bundled font:** `NotoSansJP-Regular SDF.asset` is a ~137 MB static SDF atlas. Because it lives under `Resources/`, it is always included in the build. For a leaner APK later, move it out of `Resources/` and assign it to the `KMogiBootstrap` font field instead, subset it to the glyphs you use, or switch to a dynamic atlas. Its source-TTF reference is absent (the atlas is pre-baked), which is fine for rendering the included glyphs; TMP may log a harmless warning if it tries to add a glyph outside the 7,130-character set.

## Known follow-ups (out of MVP scope)

- **Device VR head tracking**: `KMogiBootstrap` builds a plain camera for the editor showcase. For
  on-device Quest, parent the camera under an XR Origin with a Tracked Pose Driver (the
  `QuestOpenXrAdapter` already reads the XR-driven camera pose).
- **Input actions**: the editor demo reads input directly via the Input System
  (`Mouse`/`Keyboard`/code-defined `InputAction`s) rather than a `.inputactions` asset, to keep the
  prototype self-contained. A semantic `.inputactions` map is a natural next step.
- **Authoring**: the lesson is authored in code (`DefaultLesson`); promote to ScriptableObjects
  (`PtStageProfile`, `DialogueTree`) for non-engineer tuning.
- **Pedagogy tuning**: the PT stage→feature mapping in `PtRuleSet.CreateDefault()` is a reasonable
  default — review with SLA experts and adjust.
