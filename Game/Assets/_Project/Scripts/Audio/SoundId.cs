namespace Abandoned.Audio
{
    /// <summary>Named sounds the game plays (impacts and footsteps are keyed by surface material instead).</summary>
    public enum SoundId
    {
        Creak,
        Groan,
        Snap,
        Crash,
        CrashDebris,
        BlindOneClick,
        CollectorJingle,
        Horn,
        Lever,
        NoiseMakerShriek,
        RadioStatic,
        FlashlightClick,
        LootPickup,
        LootPocket,
        Coins,
        DistantSettle,
        UiClick,
        UiConfirm,
        UiBack,
        UiError,
        UiOpen,
        UiClose,
        // Append only: the SoundBank stores these by number.
        HunterStep,
        HunterLanding,
        CrowbarHit,
        JackPlaced,
        // M10.4: shutters
        ShutterOpen,
        BoltCut,
        // UI overhaul: the loot scan's sonar ping
        ScanPing,
    }
}
