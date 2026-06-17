using UnityEngine;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// Rokid Max 2 + Station 2 adapter. This is an honest, compiling STUB: it reports the Rokid's
    /// narrow FOV and flat layout and reads head pose from the camera, but the 3DoF gyro head
    /// tracking and air-mouse/remote pointer must be bound to the Rokid UXR2.0 SDK once that
    /// package is imported into the project. Keeping the seam real (rather than absent) means the
    /// rest of the app compiles and runs today and only this file changes when the SDK lands.
    /// </summary>
    public sealed class RokidAdapter : IPlatformDeviceAdapter
    {
        private Camera _camera;
        private float _referenceYaw;

        public PlatformKind Kind => PlatformKind.Rokid;

        public float FieldOfViewDegrees => 50f;

        public ScreenLayoutMode PreferredLayout => ScreenLayoutMode.Flat;

        public Pose HeadPose =>
            _camera != null ? new Pose(_camera.transform.position, _camera.transform.rotation) : Pose.identity;

        public float HeadYawDegrees =>
            _camera != null ? Mathf.DeltaAngle(_referenceYaw, _camera.transform.eulerAngles.y) : 0f;

        // TODO(rokid-sdk): drive from the Rokid remote/air-mouse selection event.
        public bool SelectPressedThisFrame => false;

        public void Initialize(PlatformContext context)
        {
            _camera = context.HeadCamera;
            if (_camera != null)
            {
                _referenceYaw = _camera.transform.eulerAngles.y;
            }

            // TODO(rokid-sdk): instantiate/bind RKCameraRig and the Rokid UXR head-tracking
            // subsystem here, and route the 3DoF pose into _camera. See the Rokid UXR2.0 docs.
            Debug.LogWarning(
                "[KMogi] RokidAdapter is a stub. Import the Rokid UXR2.0 SDK and bind RKCameraRig " +
                "to enable real 3DoF tracking and air-mouse pointer input.");
        }

        public void Tick()
        {
        }

        // TODO(rokid-sdk): project the air-mouse pointer into a world ray.
        public bool TryGetPointerRay(out Ray ray)
        {
            ray = default;
            return false;
        }

        public void Shutdown()
        {
        }
    }
}
