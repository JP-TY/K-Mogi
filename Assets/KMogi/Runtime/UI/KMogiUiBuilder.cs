using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KMogi.Runtime.UI
{
    /// <summary>References to the procedurally-built spatial UI.</summary>
    public sealed class KMogiUi
    {
        public SpatialPanel Left { get; }
        public SpatialPanel Center { get; }
        public SpatialPanel Right { get; }
        public ThreeScreenLayout Layout { get; }

        public KMogiUi(SpatialPanel left, SpatialPanel center, SpatialPanel right, ThreeScreenLayout layout)
        {
            Left = left;
            Center = center;
            Right = right;
            Layout = layout;
        }
    }

    /// <summary>
    /// Builds the three world-space spatial screens entirely in code, so no fragile scene/prefab
    /// authoring is required. An optional Japanese-capable <see cref="TMP_FontAsset"/> is applied to
    /// every text field; when null, TMP's default font is used (Latin/romaji still legible).
    /// </summary>
    public static class KMogiUiBuilder
    {
        private static readonly Vector2 PanelSize = new Vector2(720f, 480f);
        private const float PanelScale = 0.0028f;
        private const float Margin = 22f;

        public static KMogiUi Build(Transform parent, TMP_FontAsset japaneseFont)
        {
            var layoutRoot = new GameObject("KMogi UI");
            layoutRoot.transform.SetParent(parent, false);

            SpatialPanel left = CreatePanel("Panel_Left (EER)", layoutRoot.transform, japaneseFont, 20f);
            SpatialPanel center = CreatePanel("Panel_Center (Workspace)", layoutRoot.transform, japaneseFont, 22f);
            SpatialPanel right = CreatePanel("Panel_Right (Telemetry)", layoutRoot.transform, japaneseFont, 22f);

            var layout = layoutRoot.AddComponent<ThreeScreenLayout>();
            layout.Bind(left.transform, center.transform, right.transform);

            return new KMogiUi(left, center, right, layout);
        }

        private static SpatialPanel CreatePanel(string name, Transform parent, TMP_FontAsset font, float fontSize)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            go.AddComponent<CanvasScaler>();

            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = PanelSize;
            rect.localScale = Vector3.one * PanelScale;

            CreateBackground(canvas.transform);
            TMP_Text body = CreateBody(canvas.transform, font, fontSize);

            var panel = go.AddComponent<SpatialPanel>();
            panel.Bind(body);
            return panel;
        }

        private static void CreateBackground(Transform parent)
        {
            var go = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);

            var image = go.GetComponent<Image>();
            image.color = new Color(0.05f, 0.06f, 0.09f, 0.88f);

            Stretch(go.GetComponent<RectTransform>(), 0f);
        }

        private static TMP_Text CreateBody(Transform parent, TMP_FontAsset font, float fontSize)
        {
            var go = new GameObject("Body", typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.richText = true;
            if (font != null)
            {
                text.font = font;
            }

            Stretch(text.rectTransform, Margin);
            return text;
        }

        private static void Stretch(RectTransform rect, float margin)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, margin);
            rect.offsetMax = new Vector2(-margin, -margin);
        }
    }
}
