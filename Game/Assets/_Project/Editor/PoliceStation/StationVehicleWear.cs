namespace Abandoned.EditorTools
{
    /// <summary>What the years have done to a parked car.</summary>
    [System.Flags]
    public enum StationVehicleWear
    {
        None = 0,
        FlatFront = 1, FlatRear = 2,
        /// <summary>Crazed, dusty glass.</summary>
        Smashed = 4,
        /// <summary>Burnt out: a black shell, no glass, the tyres gone, sitting on its rims.</summary>
        Burnt = 8,
        /// <summary>The driver's door hanging open.</summary>
        DoorOpen = 16,
        /// <summary>Dust and grime dulling the paint and glass.</summary>
        Dusty = 32,
        /// <summary>Roof cut off (a rescue-drill wreck).</summary>
        RoofCut = 64,
    }
}
