namespace Abandoned.Audio
{
    /// <summary>
    /// Which patch makes the sounds added from 0.16.1 on (SoundPatches' default case lands here). Worker
    /// thread: nothing here or below may touch the Unity API.
    /// </summary>
    internal static class ExtraPatches
    {
        public static SynthSamples Render(SoundId id, int take)
        {
            switch (id)
            {
                // 0.16.1: breaker rooms (a heavy switch and the mains coming back, never a creak).
                case SoundId.BreakerThrow: return Patch.Mono(id, take, RoomPatches.Breaker(take), 0.95f);
                // 0.16.3: the freight lift (motor, cable and brakes; never a creak).
                case SoundId.LiftStart: return Patch.Mono(id, take, RoomPatches.LiftStart(), 0.9f);
                case SoundId.LiftArrive: return Patch.Mono(id, take, RoomPatches.LiftArrive(take), 0.85f);
                case SoundId.LiftSnap: return Patch.Mono(id, take, RoomPatches.LiftSnap(), 0.95f);
                // 0.16.4: vents (thin sheet metal knocked about, never a creak).
                case SoundId.VentCrawl: return Patch.Mono(id, take, RoomPatches.VentCrawl(take), 0.75f);
                // 0.16.5: store alarms (an electric bell; the cut is a snip).
                case SoundId.AlarmBell: return Patch.Mono(id, take, RoomPatches.AlarmBell(), 0.85f);
                case SoundId.AlarmCut: return Patch.Mono(id, take, RoomPatches.AlarmCut(), 0.7f);
                // 0.16.6: the vault drill (a core drill in steel; the door's bolts).
                case SoundId.VaultDrillStart: return Patch.Mono(id, take, RoomPatches.DrillStart(), 0.9f);
                case SoundId.VaultDrill: return Patch.Mono(id, take, RoomPatches.Drill(take), 0.8f);
                case SoundId.VaultDrillJam: return Patch.Mono(id, take, RoomPatches.DrillJam(), 0.9f);
                case SoundId.VaultOpen: return Patch.Mono(id, take, RoomPatches.SafeOpen(), 0.9f);
                // 0.17.0: the sledgehammer.
                case SoundId.SledgeHit: return Patch.Mono(id, take, HazardPatches.SledgeHit(take), 0.95f);
                case SoundId.SledgeSwing: return Patch.Mono(id, take, HazardPatches.SledgeSwing(take), 0.6f);
                // 0.17.2: floor types' own warnings. Building bus and never hidden by walls, like every floor warning.
                case SoundId.GlassFloorCrack: return Patch.Mono(id, take, HazardPatches.GlassCrack(take), 0.85f);
                case SoundId.GlassFloorFail: return Patch.Mono(id, take, HazardPatches.GlassFail(take), 0.95f);
                case SoundId.TimberFloorCrack: return Patch.Mono(id, take, HazardPatches.TimberCrack(take), 0.85f);
                case SoundId.TimberFloorFail: return Patch.Mono(id, take, HazardPatches.TimberFail(take), 0.95f);
                case SoundId.TileFloorCrack: return Patch.Mono(id, take, HazardPatches.TileCrack(take), 0.85f);
                case SoundId.TileFloorFail: return Patch.Mono(id, take, HazardPatches.TileFail(take), 0.95f);
                // 0.17.3: noisy ground underfoot (footsteps bus).
                case SoundId.GlassCrunch: return Patch.Mono(id, take, HazardPatches.GlassCrunch(take), 0.8f);
                case SoundId.TileClack: return Patch.Mono(id, take, HazardPatches.TileClack(take), 0.75f);
                case SoundId.PuddleSplash: return Patch.Mono(id, take, HazardPatches.Splash(take), 0.7f);
                // 0.17.4: aftershocks (the ground itself; walls never hide it).
                case SoundId.Aftershock: return Patch.Mono(id, take, HazardPatches.Aftershock(take), 0.95f);
                // 0.17.5: live water (mains hum and arcing; never a creak).
                case SoundId.WaterBuzz: return Patch.Mono(id, take, HazardPatches.WaterBuzz(take), 0.6f);
                case SoundId.WaterZap: return Patch.Mono(id, take, HazardPatches.WaterZap(take), 0.9f);
                // @@cases
                default: return null;
            }
        }
    }
}
