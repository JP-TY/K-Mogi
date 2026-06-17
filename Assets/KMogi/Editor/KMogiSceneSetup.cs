using System.Collections.Generic;
using System.IO;
using KMogi.Runtime.Bootstrap;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KMogi.Editor
{
    /// <summary>
    /// One-click demo setup: drops a single <see cref="KMogiBootstrap"/> into the open scene. The
    /// bootstrap builds the rig, avatar and spatial UI procedurally at Play time, so there is no
    /// fragile scene/prefab wiring to maintain.
    /// </summary>
    public static class KMogiSceneSetup
    {
        [MenuItem("KMogi/Setup Demo Scene")]
        public static void SetupDemoScene()
        {
            KMogiBootstrap existing = Object.FindAnyObjectByType<KMogiBootstrap>();
            if (existing != null)
            {
                Selection.activeObject = existing.gameObject;
                Debug.Log("[KMogi] KMogiBootstrap already present in the scene. Press Play to run the demo.");
                return;
            }

            var go = new GameObject("KMogi Bootstrap");
            go.AddComponent<KMogiBootstrap>();
            Undo.RegisterCreatedObjectUndo(go, "Create KMogi Bootstrap");
            Selection.activeObject = go;
            EditorSceneManager.MarkSceneDirty(go.scene);

            Debug.Log("[KMogi] Added KMogiBootstrap to the scene. Optionally assign a Japanese TMP font, " +
                      "then press Play to run the demo.");
        }
    }
}
