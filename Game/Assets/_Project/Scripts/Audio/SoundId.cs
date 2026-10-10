namespace Abandoned.Audio
{
    /// <summary>
    /// Every sound the game plays, in one list (impacts and footsteps are keyed by surface material instead).
    /// What each one is (bus, range, caption, how it's made) is in <see cref="SoundCatalog"/>.
    /// Append only: the SoundBank stores these by number and RPCs send them.
    /// </summary>
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
        HunterStep,
        HunterLanding,
        CrowbarHit,
        JackPlaced,
        // M10.4: shutters
        ShutterOpen,
        BoltCut,
        // UI overhaul: the loot scan's sonar ping
        ScanPing,
        // QA fixes: a shutter crashing down (not the open rattle)
        ShutterSlam,

        // ---- 0.14.0 sound rebuild: everything below used to be played outside the catalog, or not at all ----
        // The truck and the horror director's cues.
        EngineStop,
        EngineIdle,
        EngineEscape,
        Scream,
        Knock,
        Whisper,
        CeilingScuttle,
        Siren,
        Roar,
        DoorSlam,
        WaterDrip,
        VentDrone,
        StalkerBreath,
        // What each monster sounds like when it is near (Blind One and Hunter keep their click and tread).
        StalkerSignature,
        CollectorSignature,
        WeightSignature,
        ThingSignature,
        CrawlersSignature,
        LastHunterSignature,
        ThreatStrike,
        // The salvager's own body.
        Heartbeat,
        Breath,
        PlayerHurt,
        PlayerDeath,
        PlayerLand,
        BodyFall,
        // Loot and the building.
        LootBreak,
        RubbleLand,
        DeepRumble,
        FixtureFall,
        PowerDown,
        PowerUp,
        LightBuzz,
        // Gear.
        NightVisionOn,
        NightVisionOff,
        DetectorBlip,
        // Beds that loop under everything.
        WindLoop,
        HumLoop,
        RoomToneLoop,
        TravelEngineLoop,
        Thunder,
        // Interface.
        UiHover,
        UiToast,
        ChatBlip,
        VoicemailBeep,
        // Music. The eight themes stay in ThreatKind order: a theme is ThemeBlindOne + (int)kind.
        MusicHq,
        MusicDrive,
        MusicDeparture,
        StingPayday,
        StingQuotaMissed,
        StingDeath,
        StingLevelUp,
        ThemeBlindOne,
        ThemeStalker,
        ThemeCollector,
        ThemeHunter,
        ThemeWeight,
        ThemeThing,
        ThemeCrawlers,
        ThemeLastHunter,

        // 0.14.5: original spoken HQ dispatch lines; append after the existing music block.
        DispatchBriefingRoutine,
        DispatchBriefingPowerOff,
        DispatchBriefingHighThreat,
        DispatchBriefingSealed,

        // 0.16.0: sliding doors and hiding lockers.
        DoorSlide,
        DoorShut,
        DoorBash,
        DoorBreak,
        LockerOpen,
        LockerShut,
        // 0.16.1: breaker rooms (a heavy switch and the mains coming back, never a creak).
        BreakerThrow,
        // 0.16.3: the freight lift (motor, cable and brakes; never a creak).
        LiftStart,
        LiftArrive,
        LiftSnap,
        // 0.16.4: vents (thin sheet metal knocked about, never a creak).
        VentCrawl,
        // 0.16.5: store alarms (an electric bell; the cut is a snip).
        AlarmBell,
        AlarmCut,
        // @@ids
    }
}
