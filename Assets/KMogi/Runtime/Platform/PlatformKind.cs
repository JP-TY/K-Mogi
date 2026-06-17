namespace KMogi.Runtime.Platform
{
    /// <summary>The hardware modality a session is running on (or is being simulated as).</summary>
    public enum PlatformKind
    {
        /// <summary>Resolve automatically at runtime from the device / editor context.</summary>
        Auto = 0,

        /// <summary>Meta Quest 3 — fully immersive VR via OpenXR.</summary>
        MetaQuest,

        /// <summary>Rokid Max 2 + Station 2 — spatial multi-window AR glasses.</summary>
        Rokid,

        /// <summary>In-editor mouse/keyboard simulation (no headset).</summary>
        EditorSimulator
    }
}
