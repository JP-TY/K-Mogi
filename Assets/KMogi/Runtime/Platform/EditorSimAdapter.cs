using UnityEngine;
using UnityEngine.InputSystem;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// Mouse/keyboard simulation so the whole trainer is demoable in the editor without a headset.
    /// Per the PRD simulation spec: hold Right Mouse Button and drag to emulate 3DoF head rotation;
    /// the selection pointer follows the mouse; Left Click fires the selection trigger.
    /// </summary>
    public sealed class EditorSimAdapter : IPlatformDeviceAdapter
    {
        private const float LookSensitivity = 0.15f;
        private const float MaxPitchDegrees = 80f;

        private readonly ScreenLayoutMode _layout;
        private Camera _camera;
        private Transform _pivot;
        private float _yaw;
        private float _pitch;
        private bool _selectedThisFrame;

        public EditorSimAdapter(ScreenLayoutMode simulatedLayout)
        {
            _layout = simulatedLayout;
        }

        public PlatformKind Kind => PlatformKind.EditorSimulator;

        // Reflects whichever device the editor is previewing so UI scaling looks representative.
        public float FieldOfViewDegrees => _layout == ScreenLayoutMode.Flat ? 50f : 110f;

        public ScreenLayoutMode PreferredLayout => _layout;

        public Pose HeadPose =>
            _camera != null ? new Pose(_camera.transform.position, _camera.transform.rotation) : Pose.identity;

        public float HeadYawDegrees => Mathf.DeltaAngle(0f, _yaw);

        public bool SelectPressedThisFrame => _selectedThisFrame;

        public void Initialize(PlatformContext context)
        {
            _camera = context.HeadCamera;
            _pivot = context.HeadPivot != null ? context.HeadPivot : (_camera != null ? _camera.transform : null);
            if (_pivot != null)
            {
                Vector3 euler = _pivot.localEulerAngles;
                _yaw = euler.y;
                _pitch = euler.x;
            }
        }

        public void Tick()
        {
            _selectedThisFrame = false;

            Mouse mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                _yaw += delta.x * LookSensitivity;
                _pitch = Mathf.Clamp(_pitch - delta.y * LookSensitivity, -MaxPitchDegrees, MaxPitchDegrees);
                if (_pivot != null)
                {
                    _pivot.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
                }
            }

            _selectedThisFrame = mouse.leftButton.wasPressedThisFrame;
        }

        public bool TryGetPointerRay(out Ray ray)
        {
            Mouse mouse = Mouse.current;
            if (_camera == null || mouse == null)
            {
                ray = default;
                return false;
            }

            ray = _camera.ScreenPointToRay(mouse.position.ReadValue());
            return true;
        }

        public void Shutdown()
        {
        }
    }
}
