using UnityEngine;
using KMogi.Runtime.Platform;

namespace KMogi.Runtime.UI
{
    /// <summary>
    /// Arranges the three spatial screens for the active device. On the Quest's wide FOV the panels
    /// wrap the user on a curved arc; on the Rokid's narrow FOV they sit on a flat coplanar plane the
    /// user pans across with head movement (which also exercises the gaze-distribution metric).
    /// Assumes the head initially faces +Z, matching the demo rig.
    /// </summary>
    public sealed class ThreeScreenLayout : MonoBehaviour
    {
        [SerializeField] private Transform left;
        [SerializeField] private Transform center;
        [SerializeField] private Transform right;

        [Header("Curved (Quest) tuning")]
        [SerializeField] private float radius = 3.2f;
        [SerializeField] private float sideAngleDegrees = 40f;

        [Header("Flat (Rokid) tuning")]
        [SerializeField] private float flatDepth = 3.2f;
        [SerializeField] private float flatSpacing = 2.4f;

        [Header("Shared")]
        [SerializeField] private float heightOffset = 0.1f;

        public void Bind(Transform leftPanel, Transform centerPanel, Transform rightPanel)
        {
            left = leftPanel;
            center = centerPanel;
            right = rightPanel;
        }

        public void Apply(ScreenLayoutMode mode, Vector3 headPosition)
        {
            if (mode == ScreenLayoutMode.Curved)
            {
                PlaceOnArc(center, headPosition, 0f);
                PlaceOnArc(left, headPosition, -sideAngleDegrees);
                PlaceOnArc(right, headPosition, sideAngleDegrees);
            }
            else
            {
                PlaceFlat(center, headPosition, 0f);
                PlaceFlat(left, headPosition, -flatSpacing);
                PlaceFlat(right, headPosition, flatSpacing);
            }
        }

        private void PlaceOnArc(Transform panel, Vector3 head, float yawDegrees)
        {
            if (panel == null)
            {
                return;
            }

            float rad = yawDegrees * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(
                head.x + Mathf.Sin(rad) * radius,
                head.y + heightOffset,
                head.z + Mathf.Cos(rad) * radius);

            panel.position = pos;
            // Canvas forward points along the view direction (head -> panel) so its face is readable.
            panel.rotation = Quaternion.LookRotation(pos - head, Vector3.up);
        }

        private void PlaceFlat(Transform panel, Vector3 head, float xOffset)
        {
            if (panel == null)
            {
                return;
            }

            panel.position = new Vector3(head.x + xOffset, head.y + heightOffset, head.z + flatDepth);
            panel.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up); // coplanar, parallel
        }
    }
}
