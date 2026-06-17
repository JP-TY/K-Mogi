using UnityEngine;
using UnityEngine.InputSystem;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// Meta Quest 3 adapter riding on the already-configured OpenXR + XR rig. Head pose is read
    /// from the XR-driven camera; the selection pointer originates at a controller transform; the
    /// trigger is read through a code-defined Input System action so no SDK-specific type leaks in.
    /// </summary>
    public sealed class QuestOpenXrAdapter : IPlatformDeviceAdapter
    {
        private Camera _camera;
        private Transform _pointerOrigin;
        private InputAction _selectAction;
        private float _referenceYaw;

        public PlatformKind Kind => PlatformKind.MetaQuest;

        public float FieldOfViewDegrees => 110f;

        public ScreenLayoutMode PreferredLayout => ScreenLayoutMode.Curved;

        public Pose HeadPose =>
            _camera != null ? new Pose(_camera.transform.position, _camera.transform.rotation) : Pose.identity;

        public float HeadYawDegrees =>
            _camera != null ? Mathf.DeltaAngle(_referenceYaw, _camera.transform.eulerAngles.y) : 0f;

        public bool SelectPressedThisFrame => _selectAction != null && _selectAction.WasPressedThisFrame();

        public void Initialize(PlatformContext context)
        {
            _camera = context.HeadCamera;
            _pointerOrigin = context.PointerOrigin;
            if (_camera != null)
            {
                _referenceYaw = _camera.transform.eulerAngles.y;
            }

            // Generic OpenXR controller trigger bindings; resolve on whatever Touch profile is active.
            _selectAction = new InputAction("KMogiSelect", InputActionType.Button);
            _selectAction.AddBinding("<XRController>{RightHand}/triggerPressed");
            _selectAction.AddBinding("<XRController>{LeftHand}/triggerPressed");
            _selectAction.Enable();
        }

        public void Tick()
        {
            // Pose and input are sampled lazily through properties; nothing per-frame to push here.
        }

        public bool TryGetPointerRay(out Ray ray)
        {
            if (_pointerOrigin != null)
            {
                ray = new Ray(_pointerOrigin.position, _pointerOrigin.forward);
                return true;
            }

            ray = default;
            return false;
        }

        public void Shutdown()
        {
            if (_selectAction != null)
            {
                _selectAction.Disable();
                _selectAction.Dispose();
                _selectAction = null;
            }
        }
    }
}
