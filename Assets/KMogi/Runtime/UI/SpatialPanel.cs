using TMPro;
using UnityEngine;

namespace KMogi.Runtime.UI
{
    /// <summary>
    /// A single spatial screen: a world-space canvas whose body is one rich-text field. Content
    /// (including ASCII meter bars for telemetry) is composed by the session controller and pushed
    /// in via <see cref="SetText"/>, keeping this component a dumb display surface.
    /// </summary>
    public sealed class SpatialPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text body;

        public void Bind(TMP_Text bodyText)
        {
            body = bodyText;
        }

        public void SetText(string content)
        {
            if (body != null)
            {
                body.text = content ?? string.Empty;
            }
        }
    }
}
