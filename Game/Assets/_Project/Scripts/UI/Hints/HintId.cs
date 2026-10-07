namespace Abandoned.UI
{
    /// <summary>First-time tips (M8.2). Each is shown once per player.</summary>
    public enum HintId
    {
        HqBoard,
        HqVan,
        RunGoal,
        PickUp,
        Pockets,
        Heavy,
        Throw,
        FloorCracking,
        PowerOff,
        BlindOne,
        Stalker,
        Collector,
        Ghost,
        TruckLeaving,
        Hunter,
        // M10 (append only: seen flags are stored by name, but keep the order anyway)
        LockedShutter,
        Voicemail,
    }
}
