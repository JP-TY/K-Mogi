using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace KMogi.Editor
{
    /// <summary>
    /// Android build entry point. Named distinctly (not "BuildPipeline") to avoid colliding with
    /// <see cref="UnityEditor.BuildPipeline"/>. Invoke from the menu, or headless via:
    ///   Unity -batchmode -projectPath . -executeMethod KMogi.Editor.KMogiBuild.BuildAndroid -quit
    /// </summary>
    public static class KMogiBuild
    {
        private const string OutputPath = "Build/KMogi_Target.apk";

        [MenuItem("KMogi/Build Android APK")]
        public static void BuildAndroid()
        {
            var options = new BuildPlayerOptions
            {
                scenes = GetEnabledScenes(),
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("[KMogi] Build succeeded: " + summary.outputPath +
                          " (" + summary.totalSize + " bytes).");
            }
            else
            {
                Debug.LogError("[KMogi] Build failed: " + summary.result +
                               " (" + summary.totalErrors + " errors).");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }

        private static string[] GetEnabledScenes()
        {
            var scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            if (scenes.Count == 0)
            {
                scenes.Add("Assets/Scenes/Main VR Scene.unity");
            }

            return scenes.ToArray();
        }
    }
}
