namespace Abandoned.Audio
{
    /// <summary>
    /// Which patch makes which <see cref="SoundId"/> (0.14.0). Called from <see cref="SynthLibrary"/>, on
    /// the worker thread or the main one, so nothing here or below may touch the Unity API. The last
    /// number on each line is the clip's peak level: how loud it is against the others at full volume.
    /// </summary>
    internal static class SoundPatches
    {
        public static SynthSamples Render(SoundId id, int take)
        {
            switch (id)
            {
                // The building.
                case SoundId.Creak: return Patch.Mono(id, take, CuePatches.Creak(take), 0.8f);
                case SoundId.Groan: return Patch.Mono(id, take, CuePatches.Groan(take), 0.85f);
                case SoundId.Snap: return Patch.Mono(id, take, CuePatches.Snap(take));
                case SoundId.Crash: return Patch.Mono(id, take, StructurePatches.Crash(take), 0.95f);
                case SoundId.CrashDebris: return Patch.Mono(id, take, StructurePatches.RubbleLand(take), 0.8f);
                case SoundId.RubbleLand: return Patch.Mono(id, take, StructurePatches.RubbleLand(take));
                case SoundId.DeepRumble: return Patch.Mono(id, take, StructurePatches.DeepRumble(take));
                case SoundId.FixtureFall: return Patch.Mono(id, take, StructurePatches.FixtureFall(take));
                case SoundId.PowerDown: return Patch.Mono(id, take, StructurePatches.PowerDown(), 0.85f);
                case SoundId.PowerUp: return Patch.Mono(id, take, StructurePatches.PowerUp(), 0.8f);
                case SoundId.LightBuzz: return Patch.Mono(id, take, StructurePatches.LightBuzz(take), 0.6f);
                case SoundId.DistantSettle: return Patch.Mono(id, take, StructurePatches.DistantSettle(take), 0.7f);

                // Monsters.
                case SoundId.BlindOneClick: return Patch.Mono(id, take, CuePatches.BlindOneClick(take));
                case SoundId.CollectorJingle: return Patch.Mono(id, take, ThreatPatches.CollectorJingle(take), 0.7f);
                case SoundId.HunterStep: return Patch.Mono(id, take, CuePatches.HunterStep(take), 0.95f);
                case SoundId.HunterLanding: return Patch.Mono(id, take, ThreatPatches.HunterLanding(), 0.95f);
                case SoundId.ThreatStrike: return Patch.Mono(id, take, ThreatPatches.Strike(take));
                case SoundId.Scream: return Patch.Mono(id, take, ThreatPatches.Scream(take));
                case SoundId.Knock: return Patch.Mono(id, take, ThreatPatches.Knock(take));
                case SoundId.Whisper: return Patch.Mono(id, take, ThreatPatches.Whisper(take), 0.7f);
                case SoundId.CeilingScuttle: return Patch.Mono(id, take, ThreatPatches.CeilingScuttle(take), 0.8f);
                case SoundId.Roar: return Patch.Mono(id, take, ThreatPatches.Roar(), 0.95f);
                case SoundId.StalkerBreath: return Patch.Mono(id, take, ThreatPatches.StalkerBreath(take), 0.6f);
                case SoundId.StalkerSignature: return Patch.Mono(id, take, CuePatches.StalkerSignature(take), 0.8f);
                case SoundId.CollectorSignature: return Patch.Mono(id, take, CuePatches.CollectorSignature(take), 0.8f);
                case SoundId.WeightSignature: return Patch.Mono(id, take, CuePatches.WeightSignature(take), 0.95f);
                case SoundId.ThingSignature: return Patch.Mono(id, take, CuePatches.ThingSignature(take), 0.8f);
                case SoundId.CrawlersSignature: return Patch.Mono(id, take, CuePatches.CrawlersSignature(take), 0.8f);
                case SoundId.LastHunterSignature: return Patch.Mono(id, take, CuePatches.LastHunterSignature(take), 0.95f);

                // The truck, the shutters, the siren.
                case SoundId.Horn: return Patch.Mono(id, take, TruckPatches.Horn());
                case SoundId.Lever: return Patch.Mono(id, take, TruckPatches.Lever(), 0.8f);
                case SoundId.EngineStop: return Patch.Mono(id, take, TruckPatches.EngineStop());
                case SoundId.EngineIdle: return Patch.Mono(id, take, TruckPatches.EngineIdle(), 0.7f);
                case SoundId.EngineEscape: return Patch.Mono(id, take, TruckPatches.EngineEscape());
                case SoundId.DoorSlam: return Patch.Mono(id, take, TruckPatches.DoorSlam(take));
                case SoundId.Siren: return Patch.Mono(id, take, TruckPatches.Siren(), 0.85f);
                case SoundId.ShutterOpen: return Patch.Mono(id, take, TruckPatches.ShutterOpen());
                case SoundId.ShutterSlam: return Patch.Mono(id, take, TruckPatches.ShutterSlam(), 0.95f);

                // Doors and lockers (0.16.0).
                case SoundId.DoorSlide: return Patch.Mono(id, take, DoorPatches.Slide(take, false), 0.8f);
                case SoundId.DoorShut: return Patch.Mono(id, take, DoorPatches.Slide(take, true), 0.9f);
                case SoundId.DoorBash: return Patch.Mono(id, take, DoorPatches.Bash(take), 0.95f);
                case SoundId.DoorBreak: return Patch.Mono(id, take, DoorPatches.Break(), 0.95f);
                case SoundId.LockerOpen: return Patch.Mono(id, take, DoorPatches.Locker(take, false), 0.7f);
                case SoundId.LockerShut: return Patch.Mono(id, take, DoorPatches.Locker(take, true), 0.8f);

                // Gear and loot.
                case SoundId.ScanPing: return Patch.Mono(id, take, GearPatches.ScanPing(), 0.8f);
                case SoundId.NoiseMakerShriek: return Patch.Mono(id, take, GearPatches.NoiseMakerShriek());
                case SoundId.RadioStatic: return Patch.Mono(id, take, GearPatches.RadioStatic(take), 0.8f);
                case SoundId.NightVisionOn: return Patch.Mono(id, take, GearPatches.NightVisionOn(), 0.6f);
                case SoundId.NightVisionOff: return Patch.Mono(id, take, GearPatches.NightVisionOff(), 0.6f);
                case SoundId.DetectorBlip: return Patch.Mono(id, take, GearPatches.DetectorBlip(), 0.7f);
                case SoundId.LootBreak: return Patch.Mono(id, take, GearPatches.LootBreak(take));
                case SoundId.VoicemailBeep: return Patch.Mono(id, take, GearPatches.VoicemailBeep(), 0.7f);
                case SoundId.FlashlightClick: return Patch.Mono(id, take, GearPatches.Click(2400f), 0.7f);
                case SoundId.LootPickup: return Patch.Mono(id, take, GearPatches.Rustle(0.18f, 3200f, 671), 0.6f);
                case SoundId.LootPocket: return Patch.Mono(id, take, GearPatches.Rustle(0.22f, 2200f, 672), 0.6f);
                case SoundId.Coins: return Patch.Mono(id, take, GearPatches.Coins(), 0.8f);
                case SoundId.CrowbarHit: return Patch.Mono(id, take, GearPatches.CrowbarHit());
                case SoundId.JackPlaced: return Patch.Mono(id, take, GearPatches.JackPlaced(), 0.8f);
                case SoundId.BoltCut: return Patch.Mono(id, take, GearPatches.BoltCut(), 0.8f);

                // The salvager's body.
                case SoundId.Heartbeat: return Patch.Mono(id, take, PlayerPatches.Heartbeat(), 0.8f);
                case SoundId.Breath: return Patch.Mono(id, take, PlayerPatches.Breath(), 0.3f);
                case SoundId.PlayerHurt: return Patch.Mono(id, take, PlayerPatches.Hurt(take));
                case SoundId.PlayerDeath: return Patch.Mono(id, take, PlayerPatches.Death(), 0.85f);
                case SoundId.PlayerLand: return Patch.Mono(id, take, PlayerPatches.Land(take), 0.8f);
                case SoundId.BodyFall: return Patch.Mono(id, take, PlayerPatches.BodyFall(take));

                // Beds.
                case SoundId.WindLoop: return AmbiencePatches.Wind();
                case SoundId.HumLoop: return new SynthSamples("Synth_HumLoop", Synth.Normalize(AmbiencePatches.Hum(), 0.7f), 1, Synth.LoopRate);
                case SoundId.RoomToneLoop: return AmbiencePatches.RoomTone();
                case SoundId.TravelEngineLoop: return new SynthSamples("Synth_TravelEngineLoop", Synth.Normalize(TruckPatches.TravelEngine(), 0.7f), 1, Synth.LoopRate);
                case SoundId.WaterDrip: return new SynthSamples("Synth_WaterDrip", Synth.Normalize(AmbiencePatches.WaterDrip(), 0.7f), 1, Synth.LoopRate);
                case SoundId.VentDrone: return new SynthSamples("Synth_VentDrone", Synth.Normalize(AmbiencePatches.VentDrone(), 0.7f), 1, Synth.LoopRate);
                case SoundId.Thunder: return Patch.Mono(id, take, AmbiencePatches.Thunder(take), 0.85f);

                // Interface.
                case SoundId.UiHover: return Patch.Mono(id, take, UiPatches.Hover(), 0.35f);
                case SoundId.UiToast: return Patch.Mono(id, take, UiPatches.Toast(), 0.6f);
                case SoundId.ChatBlip: return Patch.Mono(id, take, UiPatches.ChatBlip(), 0.5f);
                case SoundId.UiClick: return Patch.Mono(id, take, UiPatches.Click(), 0.6f);
                case SoundId.UiConfirm: return Patch.Mono(id, take, UiPatches.TwoNote(660f, 990f), 0.6f);
                case SoundId.UiBack: return Patch.Mono(id, take, UiPatches.TwoNote(660f, 495f), 0.6f);
                case SoundId.UiError: return Patch.Mono(id, take, UiPatches.Error(), 0.6f);
                case SoundId.UiOpen: return Patch.Mono(id, take, UiPatches.Whoosh(true), 0.5f);
                case SoundId.UiClose: return Patch.Mono(id, take, UiPatches.Whoosh(false), 0.5f);

                // Music.
                case SoundId.MusicHq: return MusicPatches.Hq();
                case SoundId.MusicDrive: return MusicPatches.Drive();
                case SoundId.MusicDeparture: return MusicPatches.Departure();
                case SoundId.StingPayday:
                case SoundId.StingQuotaMissed:
                case SoundId.StingDeath:
                case SoundId.StingLevelUp:
                    return MusicPatches.Sting(id);
                default:
                    if (id >= SoundId.ThemeBlindOne && id <= SoundId.ThemeLastHunter)
                        return ThreatThemePatches.Render(id - SoundId.ThemeBlindOne);
                    return ExtraPatches.Render(id, take);
            }
        }
    }
}
