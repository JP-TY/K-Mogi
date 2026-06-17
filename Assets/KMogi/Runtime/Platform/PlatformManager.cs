using System;
using UnityEngine;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// Resolves the active <see cref="IPlatformDeviceAdapter"/> at startup and drives its per-frame
    /// sampling. Everything else in the scene queries the platform exclusively through
    /// <see cref="Adapter"/>, satisfying the Separation Mandate.
    /// </summary>
    public sealed class PlatformManager : MonoBehaviour
    {
        [Header("Scene references")]
        [SerializeField] private Camera headCamera;
        [SerializeField] private Transform headPivot;
        [SerializeField] private Transform pointerOrigin;

        [Header("Platform selection")]
        [Tooltip("Force a specific modality. 'Auto' resolves from the runtime device (editor = simulator).")]
        [SerializeField] private PlatformKind forcedPlatform = PlatformKind.Auto;

        [Tooltip("Which screen layout the editor simulator previews.")]
        [SerializeField] private ScreenLayoutMode editorSimulatedLayout = ScreenLayoutMode.Curved;

        public IPlatformDeviceAdapter Adapter { get; private set; }

        public Camera HeadCamera => headCamera;

        /// <summary>Assign scene references before <see cref="Awake"/> when building the rig in code.</summary>
        public void Configure(Camera camera, Transform pivot, Transform pointer)
        {
            headCamera = camera;
            headPivot = pivot;
            pointerOrigin = pointer;
        }

        public void SetForcedPlatform(PlatformKind kind) => forcedPlatform = kind;
        public void SetEditorLayout(ScreenLayoutMode layout) => editorSimulatedLayout = layout;

        private void Awake()
        {
            if (headCamera == null)
            {
                headCamera = Camera.main;
            }

            Adapter = CreateAdapter();
            Adapter.Initialize(new PlatformContext(headCamera, headPivot, pointerOrigin));
            Debug.Log("[KMogi] Platform resolved to " + Adapter.Kind + " (" + Adapter.PreferredLayout + " layout).");
        }

        private void Update()
        {
            Adapter?.Tick();
        }

        private void OnDestroy()
        {
            Adapter?.Shutdown();
        }

        private IPlatformDeviceAdapter CreateAdapter()
        {
            PlatformKind kind = forcedPlatform == PlatformKind.Auto ? DetectPlatform() : forcedPlatform;
            switch (kind)
            {
                case PlatformKind.MetaQuest:
                    return new QuestOpenXrAdapter();
                case PlatformKind.Rokid:
                    return new RokidAdapter();
                default:
                    return new EditorSimAdapter(editorSimulatedLayout);
            }
        }

        private static PlatformKind DetectPlatform()
        {
#if UNITY_EDITOR
            return PlatformKind.EditorSimulator;
#else
            string model = SystemInfo.deviceModel ?? string.Empty;
            if (model.IndexOf("Rokid", StringComparison.OrdinalIgnoreCase) >= 0 ||
                model.IndexOf("Station", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return PlatformKind.Rokid;
            }

            return PlatformKind.MetaQuest;
#endif
        }
    }
}
