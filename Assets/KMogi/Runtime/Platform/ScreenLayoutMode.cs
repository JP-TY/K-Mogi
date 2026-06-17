namespace KMogi.Runtime.Platform
{
    /// <summary>How the three spatial screens are arranged for a given device's field of view.</summary>
    public enum ScreenLayoutMode
    {
        /// <summary>Curved arc wrapping the user — suits the Quest's wide (~110°) FOV.</summary>
        Curved,

        /// <summary>Coplanar flat panels — suits the Rokid's narrow (~50°) FOV.</summary>
        Flat
    }
}
