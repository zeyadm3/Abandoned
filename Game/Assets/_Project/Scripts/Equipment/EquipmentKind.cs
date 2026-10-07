namespace Abandoned.Equipment
{
    /// <summary>What a piece of equipment does when used (GDD 12). The definition carries the rest as data.</summary>
    public enum EquipmentKind : byte
    {
        Flashlight,
        Radio,
        Medkit,
        StressScanner,
        HandTrolley,
        Planks,
        NoiseMaker,
        // M9.4 (append only: definitions store these by number)
        Backpack,
        SupportJack,
        Crowbar,
        // M10 (append only)
        Flatbed,
        RopePulley,
        BoltCutters,
    }
}
