# CLAUDE.md — K-Mogi Project Guidelines

This repository hosts **K-Mogi**, a cross-platform XR simulation workspace for training Japanese language instructors. It targets both the **Meta Quest 3** (fully immersive VR) and the **Rokid Max 2 + Station 2** (spatial multi-window desktop).

## 1. Tech Stack & Software Architecture

- **Engine & Runtime**: Unity 2022.3 LTS or later, targeting Android (API Level 31+).
  - Note: this checkout is currently pinned to **Unity 6000.5.0f1** (`ProjectSettings/ProjectVersion.txt`); `AndroidMinSdkVersion` is 26. Match the editor version in the project file.
- **Graphics Pipeline**: Universal Render Pipeline (URP 17.5.0) — highly optimized for mobile XR.
- **XR Subsystems**: Unity XR Interaction Toolkit (XRI 3.5.1), OpenXR (1.17.1), Rokid UXR2.0 SDK, Meta XR SDK.
- **Core Systems**: Abstract Platform Adapters, local C# Japanese Morphological Parser (MeCab wrapper), async audio recorder.
- **Main scene**: `Assets/Scenes/Main VR Scene.unity` (the only scene in the build list).

### Mathematical Telemetry Constraints

- **Teacher Talk Time (TTT) Ratio**: Target is $TTT \le 40\%$ for Irodori lessons.
- **Processability Theory (PT) Syntax Limits**: Match avatar dialogue processing to Pienemann's developmental stages (Stages 1–5).

## 2. Command-Line & Automation Shortcuts

Since this is an Android-based Unity project, use these commands to compile, deploy, and debug without opening the GUI editor.

- **List ADB active devices**: `adb devices`
- **Run real-time mirror loop (scrcpy)**: `scrcpy --max-fps 60 --bit-rate 8M`
- **Install compiled target to Station 2 / Quest**: `adb install -r Build/KMogi_Target.apk`
- **Read runtime debug logs (Unity console filter)**: `adb logcat -s Unity`
- **Batch build Android target via Unity CLI** (macOS path shown; adjust the editor path per OS):

  ```bash
  "/Applications/Unity/Hub/Editor/<Version>/Unity.app/Contents/MacOS/Unity" \
    -batchmode -projectPath . -executeMethod KMogi.Editor.KMogiBuild.BuildAndroid \
    -logFile build_log.txt -quit
  ```

  The build method (`Assets/KMogi/Editor/KMogiBuild.cs`) writes `Build/KMogi_Target.apk`. The in-editor demo is set up via the **KMogi → Setup Demo Scene** menu (adds a `KMogiBootstrap`); press Play to run it with the XR Device Simulator — no headset required.

## 3. Code Style & C# Architectural Guidelines

To maintain cross-platform integrity and prevent spaghetti scripts, always adhere to these structural policies.

### A. The Separation Mandate (No Direct Hardware Coupling)

- Never bind platform-specific SDK references (e.g., `OVRCameraRig` or `RKCameraRig`) directly inside gameplay or evaluation logic scripts.
- Always query variables through the abstract interface: `IPlatformDeviceAdapter`.
- Decouple coordinate tracking systems so that camera UI layouts dynamically scale between the Quest ($110^\circ$ FOV) and the Rokid ($50^\circ$ FOV) modes.

### B. Unity C# Best Practices

- Use standard `PascalCase` for public fields, `camelCase` for private fields (with an underscore prefix: `_privateVariable`).
- Avoid placing resource-heavy operations (string splitting, regex parsing, local MeCab syntax assessments) inside the standard `MonoBehaviour.Update()` loop. Run them as asynchronous, time-sliced tasks (`async`/`await` or UniTask).
- Enforce absolute black background rendering (`#000000` solid clear flags) when targeting Rokid glass systems, as pure black is rendered as perfect optical transparency.

### C. SLA Rule Verification

- When generating avatar interactions, confirm that student responses strictly correspond to their active PT stage limits.
- Always evaluate error corrections via the Interactionist model:
  - Recasts should reduce student anxiety.
  - Explicit over-corrections should spike the student's affective filter and lower vocal output metrics.

## 4. Local Testing & Verification Loop

Before declaring a code task complete:

1. Run the **Unity XR Device Simulator** configuration profile to verify pointer hits and mouse-based 3DoF head rotation values inside the editor viewport.
2. Run the local parser unit test suites (if available, via Unity Test Framework `com.unity.test-framework@1.7.0`) to ensure Japanese sentence strings segment correctly.
3. Deploy the debug build to the Station 2 or Quest 3 over ADB, verify performance metrics, and ensure target frame rates (**90 fps on Quest 3, 60 fps on Station 2**) are maintained.

## 5. Repo Hygiene

- This is a Unity project — **do not use `dotnet build` / `dotnet test`.** Build and test through the Unity Editor or its CLI batch mode (see Sections 2 and 4).
- Keep `.meta` files in sync — never move or delete an asset without its `.meta`, and commit both.
- Generated/uncommitted dirs: `Library/`, `Temp/`, `Logs/`, `*_BurstDebugInformation_DoNotShip/`.
- Most `.cs` files under `Assets/Samples/` (XRI Starter Assets) and `Assets/TextMesh Pro/` are vendored samples, not project code.
- A `unity-mcp` server is available in this session for editor-aware operations (scene/GameObject/asset/script queries and edits).
- ECC language/workflow rules are installed globally (`~/.claude/`), so general C# and process guidance already applies here.
