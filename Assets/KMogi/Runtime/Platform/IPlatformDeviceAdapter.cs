using UnityEngine;

namespace KMogi.Runtime.Platform
{
    /// <summary>
    /// The single seam through which gameplay and evaluation logic touch hardware. Per the
    /// Separation Mandate, no scene logic references Quest/Rokid SDK types directly — everything
    /// queries head pose, pointer ray, selection and layout preference through this interface.
    /// </summary>
    public interface IPlatformDeviceAdapter
    {
        /// <summary>The resolved hardware modality this adapter represents.</summary>
        PlatformKind Kind { get; }

        /// <summary>Nominal horizontal field of view in degrees (drives UI scaling).</summary>
        float FieldOfViewDegrees { get; }

        /// <summary>Whether spatial screens should be arranged curved or flat on this device.</summary>
        ScreenLayoutMode PreferredLayout { get; }

        /// <summary>Current world-space head pose.</summary>
        Pose HeadPose { get; }

        /// <summary>
        /// Signed head yaw in degrees relative to the forward direction captured at
        /// <see cref="Initialize"/> time. Negative = looking left, positive = looking right.
        /// </summary>
        float HeadYawDegrees { get; }

        /// <summary>True only on the frame a selection was triggered.</summary>
        bool SelectPressedThisFrame { get; }

        /// <summary>Bind the adapter to scene references. Called once before the first <see cref="Tick"/>.</summary>
        void Initialize(PlatformContext context);

        /// <summary>Sample input/pose for the current frame. Called once per frame by the manager.</summary>
        void Tick();

        /// <summary>Try to obtain the current selection pointer ray. Returns false when unavailable.</summary>
        bool TryGetPointerRay(out Ray ray);

        /// <summary>Release any input resources. Called when the manager is destroyed.</summary>
        void Shutdown();
    }
}
