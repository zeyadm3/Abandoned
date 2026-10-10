using System;

namespace Abandoned.Audio
{
    /// <summary>
    /// What every <see cref="SoundId"/> is (0.14.0): one row each. Read-only after start, so the synth's
    /// worker thread can use it. Tuning (clips, volume, pitch, range) belongs to the SoundBank; a row's
    /// numbers only stand in until the bank holds that cue.
    /// </summary>
    public static class SoundCatalog
    {
        /// <summary>
        /// Raised when sounds move from recordings to synthesis. A SoundBank asset older than this still
        /// holds the old recordings for those cues; they are ignored until the bank is rebuilt.
        /// </summary>
        public const int Revision = 2;

        public static readonly int Count = Enum.GetValues(typeof(SoundId)).Length;

        private static readonly SoundInfo[] Table = Build();

        public static SoundInfo Info(SoundId id)
        {
            int i = (int)id;
            return i >= 0 && i < Table.Length ? Table[i] : Table[0];
        }

        private static SoundInfo[] Build()
        {
            var t = new SoundInfo[Count];
            void Set(SoundId id, AudioBus bus, int priority, float min, float max, SoundMode mode = SoundMode.Synth,
                SoundOcclusion occlusion = SoundOcclusion.Full, int takes = 1, string caption = null, float volume = 1f,
                float pitch = 0.06f, float cooldown = 0f, bool flat = false, bool loop = false) =>
                t[(int)id] = new SoundInfo(bus, priority, volume, min, max, 1f - pitch, 1f + pitch, cooldown, mode,
                    occlusion, takes, flat, loop, caption);

            // Short local names: the bare words Structure, Threats and Synth mean other things in this namespace.
            const AudioBus Fx = AudioBus.Effects, Steps = AudioBus.Footsteps, Building = AudioBus.Structure,
                Monsters = AudioBus.Threats, Bed = AudioBus.Ambience, Song = AudioBus.Music, Menu = AudioBus.Ui,
                Body = AudioBus.Vitals;
            const SoundMode Lib = SoundMode.Library, Syn = SoundMode.Synth, Both = SoundMode.Layered;
            const SoundOcclusion Open = SoundOcclusion.None, Dull = SoundOcclusion.Soft, Wall = SoundOcclusion.Full;

            for (int i = 0; i < Count; i++) Set((SoundId)i, Fx, 128, 2f, 40f);

            // The building. A creak must only ever mean the floor (QA B-14), and walls never hide one.
            Set(SoundId.Creak, Building, 44, 3f, 35f, Both, Open, 3, "floor creaks", 0.9f);
            Set(SoundId.Groan, Building, 44, 4f, 45f, Both, Open, 3, "floor groans");
            Set(SoundId.Snap, Building, 40, 3f, 45f, Both, Open, 3, "something snaps");
            Set(SoundId.Crash, Building, 32, 5f, 70f, Syn, Open, 3, "FLOOR COLLAPSES");
            Set(SoundId.CrashDebris, Building, 64, 5f, 60f, Lib, Open);
            Set(SoundId.RubbleLand, Building, 60, 4f, 55f, Syn, Open, 2);
            Set(SoundId.DeepRumble, Building, 50, 10f, 140f, Syn, Open, 2);
            Set(SoundId.FixtureFall, Fx, 90, 3f, 40f, Syn, Wall, 2);
            Set(SoundId.PowerDown, Fx, 50, 2f, 30f, flat: true);
            Set(SoundId.PowerUp, Fx, 60, 2f, 30f, flat: true);
            Set(SoundId.LightBuzz, Bed, 190, 1f, 12f, Syn, Wall, 3, volume: 0.6f, cooldown: 0.25f);
            Set(SoundId.DistantSettle, Bed, 160, 6f, 40f, Syn, Wall, 6, volume: 0.6f);

            // Monsters: dulled by walls, never hidden by them.
            Set(SoundId.BlindOneClick, Monsters, 50, 3f, 30f, Syn, Dull, 4, "clicking");
            Set(SoundId.CollectorJingle, Monsters, 60, 2f, 25f, Both, Dull, 3, "jingling footsteps", 0.9f);
            Set(SoundId.HunterStep, Monsters, 46, 4f, 38f, Both, Dull, 3, "heavy footsteps");
            Set(SoundId.HunterLanding, Monsters, 36, 6f, 60f, Syn, Dull, 1, "something heavy crashes down");
            Set(SoundId.Scream, Monsters, 44, 3f, 50f, Syn, Dull, 2, "a scream");
            Set(SoundId.Knock, Monsters, 60, 2f, 35f, Syn, Dull, 2, "knocking");
            Set(SoundId.Whisper, Monsters, 70, 1f, 20f, Syn, Open, 2, "whispering");
            Set(SoundId.CeilingScuttle, Monsters, 70, 2f, 30f, Syn, Dull, 2, "scuttling");
            Set(SoundId.Roar, Monsters, 30, 6f, 90f, Syn, Dull, 1, "a roar");
            Set(SoundId.StalkerBreath, Monsters, 52, 1.5f, 18f, Syn, Dull, 3, "breathing");
            Set(SoundId.StalkerSignature, Monsters, 54, 2f, 38f, Syn, Dull, 2, "a dry rasp");
            Set(SoundId.CollectorSignature, Monsters, 54, 2f, 38f, Syn, Dull, 2, "clinking");
            Set(SoundId.WeightSignature, Monsters, 42, 4f, 55f, Syn, Dull, 2, "something enormous strains");
            Set(SoundId.ThingSignature, Monsters, 54, 2f, 38f, Syn, Dull, 2, "a glassy ringing");
            Set(SoundId.CrawlersSignature, Monsters, 54, 2f, 38f, Syn, Dull, 3, "skittering");
            Set(SoundId.LastHunterSignature, Monsters, 34, 6f, 90f, Syn, Dull, 2, "a low pounding");
            Set(SoundId.ThreatStrike, Monsters, 34, 2f, 30f, Syn, Open, 3);

            // The truck and the job's set pieces.
            Set(SoundId.Horn, Fx, 36, 8f, 120f, Syn, Open, 1, "truck horn", pitch: 0f);
            Set(SoundId.Lever, Fx, 100, 2f, 25f, Lib);
            Set(SoundId.EngineStop, Fx, 80, 3f, 45f, pitch: 0f);
            Set(SoundId.EngineIdle, Fx, 120, 2f, 22f, pitch: 0f, loop: true);
            Set(SoundId.EngineEscape, Fx, 60, 4f, 60f, pitch: 0f);
            Set(SoundId.DoorSlam, Fx, 70, 3f, 45f, Syn, Wall, 2, "a door slams");
            Set(SoundId.Siren, Fx, 40, 2f, 30f, caption: "siren", pitch: 0f, flat: true);
            Set(SoundId.ShutterOpen, Fx, 90, 3f, 35f);
            Set(SoundId.ShutterSlam, Fx, 70, 4f, 45f);

            // Gear and loot. Recordings where a real object in the hand sounds right.
            Set(SoundId.NoiseMakerShriek, Fx, 60, 5f, 60f, Syn, Dull, 1, "shrieking alarm", pitch: 0f);
            Set(SoundId.RadioStatic, Fx, 90, 1f, 15f, Syn, Open, 2, "radio static");
            Set(SoundId.FlashlightClick, Fx, 128, 1f, 10f, Lib, volume: 0.8f);
            Set(SoundId.LootPickup, Fx, 128, 1f, 12f, Lib, volume: 0.8f);
            Set(SoundId.LootPocket, Fx, 128, 1f, 10f, Lib, volume: 0.9f);
            Set(SoundId.Coins, Fx, 110, 1f, 10f, Lib);
            Set(SoundId.CrowbarHit, Fx, 100, 3f, 40f, Lib);
            Set(SoundId.JackPlaced, Fx, 100, 2f, 20f, Lib);
            Set(SoundId.BoltCut, Fx, 110, 1f, 12f, Lib, volume: 0.9f);
            Set(SoundId.LootBreak, Fx, 80, 2f, 35f, takes: 3);
            Set(SoundId.NightVisionOn, Fx, 110, 1f, 8f, pitch: 0f);
            Set(SoundId.NightVisionOff, Fx, 110, 1f, 8f, pitch: 0f);
            Set(SoundId.DetectorBlip, Menu, 110, 1f, 5f, pitch: 0f, flat: true);
            Set(SoundId.ScanPing, Menu, 100, 1f, 5f, pitch: 0f, flat: true);
            Set(SoundId.VoicemailBeep, Fx, 100, 1f, 12f, pitch: 0f);

            // The salvager's body.
            Set(SoundId.Heartbeat, Body, 72, 1f, 5f, pitch: 0f, flat: true);
            Set(SoundId.Breath, Body, 96, 1f, 5f, pitch: 0.04f, flat: true);
            Set(SoundId.PlayerHurt, Fx, 60, 1f, 18f, takes: 3, cooldown: 0.15f);
            Set(SoundId.PlayerDeath, Fx, 40, 2f, 30f, pitch: 0.02f);
            Set(SoundId.PlayerLand, Steps, 120, 1f, 20f, takes: 2);
            Set(SoundId.BodyFall, Fx, 100, 1.5f, 22f, takes: 2);

            // Beds.
            Set(SoundId.WindLoop, Bed, 200, 1f, 5f, pitch: 0f, flat: true, loop: true);
            Set(SoundId.HumLoop, Bed, 200, 1f, 5f, pitch: 0f, flat: true, loop: true);
            Set(SoundId.RoomToneLoop, Bed, 200, 1f, 5f, pitch: 0f, flat: true, loop: true);
            Set(SoundId.TravelEngineLoop, Bed, 150, 1f, 5f, pitch: 0f, flat: true, loop: true);
            Set(SoundId.WaterDrip, Bed, 170, 2f, 17f, pitch: 0f, loop: true);
            Set(SoundId.VentDrone, Bed, 170, 2f, 17f, pitch: 0f, loop: true);
            Set(SoundId.Thunder, Bed, 120, 1f, 5f, takes: 2, flat: true);

            // Interface: Kenney's recordings for the buttons, synthesis for what they have no sound for.
            Set(SoundId.UiClick, Menu, 100, 1f, 5f, Lib, Open, volume: 0.6f, flat: true);
            Set(SoundId.UiConfirm, Menu, 100, 1f, 5f, Lib, Open, volume: 0.7f, flat: true);
            Set(SoundId.UiBack, Menu, 100, 1f, 5f, Lib, Open, volume: 0.6f, flat: true);
            Set(SoundId.UiError, Menu, 100, 1f, 5f, Lib, Open, volume: 0.6f, flat: true);
            Set(SoundId.UiOpen, Menu, 100, 1f, 5f, Lib, Open, volume: 0.6f, flat: true);
            Set(SoundId.UiClose, Menu, 100, 1f, 5f, Lib, Open, volume: 0.6f, flat: true);
            Set(SoundId.UiHover, Menu, 150, 1f, 5f, volume: 0.5f, cooldown: 0.03f, flat: true);
            Set(SoundId.UiToast, Menu, 110, 1f, 5f, volume: 0.7f, pitch: 0f, flat: true);
            Set(SoundId.ChatBlip, Menu, 120, 1f, 5f, volume: 0.6f, pitch: 0f, flat: true);

            // Music: never pitched, never positioned.
            for (SoundId id = SoundId.MusicHq; id <= SoundId.ThemeLastHunter; id++)
            {
                bool sting = id >= SoundId.StingPayday && id <= SoundId.StingLevelUp;
                Set(id, Song, 10, 1f, 5f, pitch: 0f, flat: true, loop: !sting);
            }

            // The job sheet carries the changing terms; these recordings give dispatch's safety warning.
            Set(SoundId.DispatchBriefingRoutine, Fx, 72, 3f, 18f, Lib, Open,
                caption: "dispatch briefing", pitch: 0f);
            Set(SoundId.DispatchBriefingPowerOff, Fx, 72, 3f, 18f, Lib, Open,
                caption: "dispatch: bring charged flashlights", pitch: 0f);
            Set(SoundId.DispatchBriefingHighThreat, Fx, 72, 3f, 18f, Lib, Open,
                caption: "dispatch: high threat job", pitch: 0f);
            Set(SoundId.DispatchBriefingSealed, Fx, 72, 3f, 18f, Lib, Open,
                caption: "dispatch: bring bolt cutters or a crowbar", pitch: 0f);

            // 0.16.0: doors and lockers. A door is a roll and a thud, never a creak (a creak only ever means
            // the floor). The Hunter at a door is dulled by walls but never hidden: the crew is meant to hear it.
            Set(SoundId.DoorSlide, Fx, 96, 2f, 26f, Syn, Wall, 2, "a door slides open", 0.9f);
            Set(SoundId.DoorShut, Fx, 90, 2f, 30f, Syn, Wall, 2, "a door slides shut");
            Set(SoundId.DoorBash, Monsters, 38, 5f, 60f, Syn, Dull, 3, "something pounds on a door");
            Set(SoundId.DoorBreak, Monsters, 32, 6f, 75f, Syn, Dull, 1, "A DOOR IS BROKEN DOWN");
            Set(SoundId.LockerOpen, Fx, 110, 1f, 14f, Syn, Wall, 2, volume: 0.8f);
            Set(SoundId.LockerShut, Fx, 110, 1f, 16f, Syn, Wall, 2, volume: 0.85f);
            // 0.16.1: breaker rooms (a heavy switch and the mains coming back, never a creak).
            Set(SoundId.BreakerThrow, Fx, 40, 4f, 60f, Syn, Dull, 2, "a breaker slams on");
            // 0.16.3: the freight lift (motor, cable and brakes; never a creak).
            Set(SoundId.LiftStart, Fx, 60, 3f, 45f, Syn, Dull, 1, "a lift motor starts");
            Set(SoundId.LiftArrive, Fx, 70, 2f, 35f, Syn, Dull, 2, "the lift stops");
            Set(SoundId.LiftSnap, Fx, 30, 5f, 70f, Syn, Open, 1, "A LIFT CABLE SNAPS");
            // 0.16.4: vents (thin sheet metal knocked about, never a creak).
            Set(SoundId.VentCrawl, Fx, 100, 1.5f, 22f, Syn, Dull, 3, "something crawls in the vents", 0.8f);
            // 0.16.5: store alarms (an electric bell; the cut is a snip).
            Set(SoundId.AlarmBell, Fx, 34, 6f, 70f, Syn, Dull, 1, "AN ALARM BELL RINGS", 0.9f, 0f);
            Set(SoundId.AlarmCut, Fx, 110, 1f, 12f, Syn, Wall, 1);
            // 0.16.6: the vault drill (a core drill in steel; the door's bolts).
            Set(SoundId.VaultDrillStart, Fx, 50, 4f, 60f, Syn, Dull, 1, "a heavy drill starts");
            Set(SoundId.VaultDrill, Fx, 60, 4f, 60f, Syn, Dull, 3, "drilling", 0.85f);
            Set(SoundId.VaultDrillJam, Fx, 60, 3f, 45f, Syn, Dull, 1, "the drill jams");
            Set(SoundId.VaultOpen, Fx, 50, 3f, 40f, Syn, Dull, 1, "a safe door swings open");
            // 0.17.0: the sledgehammer.
            Set(SoundId.SledgeHit, Fx, 50, 4f, 55f, Syn, Dull, 3, "a hammer pounds the floor");
            Set(SoundId.SledgeSwing, Fx, 140, 1f, 10f, Syn, Wall, 2);
            // 0.17.2: floor types' own warnings. Building bus and never hidden by walls, like every floor warning.
            Set(SoundId.GlassFloorCrack, Building, 42, 3f, 40f, Syn, Open, 3, "the glass floor cracks");
            Set(SoundId.GlassFloorFail, Building, 38, 3f, 50f, Syn, Open, 2, "glass splinters underfoot");
            Set(SoundId.TimberFloorCrack, Building, 42, 3f, 40f, Syn, Open, 3, "floorboards split");
            Set(SoundId.TimberFloorFail, Building, 38, 3f, 50f, Syn, Open, 2, "floorboards give way");
            Set(SoundId.TileFloorCrack, Building, 42, 3f, 40f, Syn, Open, 3, "floor tiles pop");
            Set(SoundId.TileFloorFail, Building, 38, 3f, 50f, Syn, Open, 2, "the tiled floor buckles");
            // 0.17.3: noisy ground underfoot (footsteps bus).
            Set(SoundId.GlassCrunch, Steps, 120, 1f, 22f, Syn, Wall, 4, "glass crunches");
            Set(SoundId.TileClack, Steps, 125, 1f, 18f, Syn, Wall, 4);
            Set(SoundId.PuddleSplash, Steps, 125, 1f, 16f, Syn, Wall, 4);
            // 0.17.4: aftershocks (the ground itself; walls never hide it).
            Set(SoundId.Aftershock, Building, 30, 10f, 140f, Syn, Open, 2, "THE GROUND SHAKES");
            // 0.17.5: live water (mains hum and arcing; never a creak).
            Set(SoundId.WaterBuzz, Fx, 90, 1f, 14f, Syn, Wall, 3, "live water buzzes", 0.7f);
            Set(SoundId.WaterZap, Fx, 50, 1f, 25f, Syn, Dull, 3, "an electric crack");
            // 0.17.6: spray chalk.
            Set(SoundId.ChalkSpray, Fx, 150, 1f, 10f, Syn, Wall, 3, null, 0.6f);
            // 0.17.7: flares.
            Set(SoundId.FlareIgnite, Fx, 80, 2f, 30f, Syn, Wall, 1, "a flare is struck");
            Set(SoundId.FlareHiss, Fx, 160, 1f, 14f, Syn, Wall, 3, null, 0.5f);
            // 0.18.2: loot quirks.
            Set(SoundId.LootRattle, Fx, 120, 1f, 20f, Syn, Wall, 3, "rattling", 0.8f);
            Set(SoundId.LootDrip, Fx, 160, 1f, 8f, Syn, Wall, 3, null, 0.6f);
            Set(SoundId.LootRing, Fx, 60, 3f, 40f, Syn, Dull, 2, "something is ringing");
            // 0.19.0: ping marker (UI).
            Set(SoundId.PingBlip, Menu, 80, 1f, 10f, Syn, Open, 1, null, 0.7f, 0f);
            // 0.19.2: ghosts (not the Stalker's knock: a ghost raps softly, three times, on wood).
            Set(SoundId.GhostKnock, Fx, 70, 2f, 28f, Syn, Dull, 2, "a soft rap, rap, rap");
            Set(SoundId.GhostWhoosh, Fx, 120, 1f, 14f, Syn, Wall, 2, null, 0.7f);
            // 0.19.6: the HQ toys.
            Set(SoundId.ToyScore, Menu, 90, 1f, 10f, Syn, Open, 1, null, 0.7f, 0f);
            // 0.20.1: the lockpick (barely audible).
            Set(SoundId.LockpickWork, Fx, 140, 0.5f, 6f, Syn, Wall, 2, null, 0.6f);
            // 0.20.2: flashlight charging at the truck.
            Set(SoundId.ChargeTick, Menu, 150, 1f, 10f, Syn, Open, 1, null, 0.5f, 0f);
            // 0.20.3: the truck winch.
            Set(SoundId.WinchStart, Fx, 60, 3f, 45f, Syn, Dull, 1, "a winch clanks into gear");
            Set(SoundId.WinchRun, Fx, 80, 3f, 40f, Syn, Dull, 2, "a winch grinds", 0.8f);
            // 0.20.5: the drone.
            Set(SoundId.DroneBuzz, Fx, 90, 2f, 30f, Syn, Dull, 2, "a drone whines", 0.8f);
            // 0.21.2: a shove.
            Set(SoundId.Shove, Fx, 130, 1f, 12f, Syn, Wall, 2, null, 0.7f);
            // 0.21.3: a monster recoils.
            Set(SoundId.ThreatFlinch, Monsters, 60, 2f, 30f, Syn, Dull, 2, "something snarls");
            // @@catalog
            return t;
        }
    }
}
