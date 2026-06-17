using KMogi.Core.Sla;
using KMogi.Runtime.Avatar;
using KMogi.Runtime.Parsing;
using KMogi.Runtime.Platform;
using KMogi.Runtime.UI;
using TMPro;
using UnityEngine;

namespace KMogi.Runtime.Bootstrap
{
    /// <summary>
    /// Single entry point for the showcase: builds the camera rig, platform manager, student avatar
    /// and the three spatial screens entirely in code, then wires them to a <see cref="SessionController"/>.
    /// Drop one of these into a scene (or use the KMogi → Setup Demo Scene menu) and press Play.
    ///
    /// The hierarchy is assembled under an inactive root and activated last, so every component's
    /// Awake sees its injected references and runs in the correct order.
    /// </summary>
    public sealed class KMogiBootstrap : MonoBehaviour
    {
        [Header("Platform")]
        [Tooltip("Force a modality, or Auto (editor → simulator).")]
        [SerializeField] private PlatformKind forcedPlatform = PlatformKind.Auto;

        [Tooltip("Which layout the editor simulator previews: Curved (Quest) or Flat (Rokid).")]
        [SerializeField] private ScreenLayoutMode editorLayout = ScreenLayoutMode.Curved;

        [Header("Pedagogy")]
        [SerializeField] private ProcessabilityStage studentStage = ProcessabilityStage.Stage2CanonicalOrder;

        [Header("Localization")]
        [Tooltip("Optional Japanese-capable TMP font. If empty, tries Resources/KMogiJP SDF, then the TMP default.")]
        [SerializeField] private TMP_FontAsset japaneseFont;

        private void Start()
        {
            TMP_FontAsset font = ResolveFont();

            var root = new GameObject("KMogi Runtime");
            root.transform.SetParent(transform, false);
            root.SetActive(false); // defer Awakes until fully wired

            Camera camera = BuildCameraRig(root.transform, out Transform headPivot);
            EnsureLighting();

            var platform = root.AddComponent<PlatformManager>();
            platform.Configure(camera, headPivot, null);
            platform.SetForcedPlatform(forcedPlatform);
            platform.SetEditorLayout(editorLayout);

            StudentAvatarController avatar = BuildAvatar(root.transform, font);
            KMogiUi ui = KMogiUiBuilder.Build(root.transform, font);

            var parser = new FixtureMorphologicalParser();
            var session = root.AddComponent<SessionController>();
            session.Configure(platform, ui.Left, ui.Center, ui.Right, avatar, parser, studentStage);

            root.SetActive(true);
        }

        // TMP font assets searched (by file name) under any Resources/ folder when no font is
        // assigned in the inspector. The bundled Noto Sans JP atlas covers ~7,130 Japanese glyphs.
        private static readonly string[] ResourceFontNames =
        {
            "NotoSansJP-Regular SDF",
            "KMogiJP SDF"
        };

        private TMP_FontAsset ResolveFont()
        {
            if (japaneseFont != null)
            {
                return japaneseFont;
            }

            foreach (string name in ResourceFontNames)
            {
                TMP_FontAsset font = Resources.Load<TMP_FontAsset>(name);
                if (font != null)
                {
                    return font;
                }
            }

            Debug.LogWarning("[KMogi] No Japanese TMP font found. Japanese glyphs may not render. " +
                             "Assign one to KMogiBootstrap, or place a TMP font asset in a Resources/ folder.");
            return null;
        }

        private Camera BuildCameraRig(Transform parent, out Transform headPivot)
        {
            // Take over rendering from any pre-existing camera so the editor demo is unambiguous.
            foreach (Camera other in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            {
                other.enabled = false;
                AudioListener listener = other.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }

            var pivot = new GameObject("Head Pivot");
            pivot.transform.SetParent(parent, false);
            pivot.transform.position = new Vector3(0f, 1.6f, 0f);
            headPivot = pivot.transform;

            var cameraGo = new GameObject("KMogi Head Camera", typeof(Camera), typeof(AudioListener));
            cameraGo.transform.SetParent(pivot.transform, false);
            cameraGo.tag = "MainCamera";

            var camera = cameraGo.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            // Pure black so Rokid optical systems render the backdrop as transparent (CLAUDE.md rule).
            camera.backgroundColor = Color.black;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 100f;
            return camera;
        }

        private StudentAvatarController BuildAvatar(Transform parent, TMP_FontAsset font)
        {
            var avatarRoot = new GameObject("Student Avatar");
            avatarRoot.transform.SetParent(parent, false);
            avatarRoot.transform.position = new Vector3(0f, 1.2f, 2.2f);

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(avatarRoot.transform, false);
            head.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            head.transform.localScale = Vector3.one * 0.45f;
            StripCollider(head);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(avatarRoot.transform, false);
            body.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            body.transform.localScale = new Vector3(0.45f, 0.5f, 0.45f);
            StripCollider(body);

            // Floating ASCII expression in front of the head, facing the viewer.
            var faceGo = new GameObject("Face", typeof(RectTransform));
            faceGo.transform.SetParent(avatarRoot.transform, false);
            faceGo.transform.localPosition = new Vector3(0f, 0.28f, -0.47f);
            faceGo.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            var faceText = faceGo.AddComponent<TextMeshPro>();
            faceText.alignment = TextAlignmentOptions.Center;
            faceText.fontSize = 4f;
            faceText.color = Color.black;
            faceText.text = ":)";
            if (font != null)
            {
                faceText.font = font;
            }

            var rect = faceText.rectTransform;
            rect.sizeDelta = new Vector2(1f, 0.6f);

            var controller = avatarRoot.AddComponent<StudentAvatarController>();
            controller.Configure(head.GetComponent<Renderer>(), avatarRoot.transform, faceText);
            return controller;
        }

        private static void StripCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }
        }

        private void EnsureLighting()
        {
            foreach (Light existing in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (existing.isActiveAndEnabled && existing.type == LightType.Directional)
                {
                    return;
                }
            }

            var lightGo = new GameObject("KMogi Directional Light");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
        }
    }
}
