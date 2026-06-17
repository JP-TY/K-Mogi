using UnityEngine;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// Scene references handed to a <see cref="IPlatformDeviceAdapter"/> at initialization.
    /// Keeping these out of the adapters themselves lets the same adapter types be reused
    /// across scenes and unit-style harnesses.
    /// </summary>
    public sealed class PlatformContext
    {
        /// <summary>The camera representing the user's head/eye viewpoint.</summary>
        public Camera HeadCamera { get; }

        /// <summary>
        /// A transform the editor simulator rotates to emulate head movement. On real devices
        /// the head pose comes from tracking and this pivot stays neutral.
        /// </summary>
        public Transform HeadPivot { get; }

        /// <summary>
        /// Origin of the selection pointer ray (e.g. a controller or air-mouse). May be null in
        /// the editor, where the pointer is derived from the mouse instead.
        /// </summary>
        public Transform PointerOrigin { get; }

        public PlatformContext(Camera headCamera, Transform headPivot, Transform pointerOrigin)
        {
            HeadCamera = headCamera;
            HeadPivot = headPivot;
            PointerOrigin = pointerOrigin;
        }
    }
}
