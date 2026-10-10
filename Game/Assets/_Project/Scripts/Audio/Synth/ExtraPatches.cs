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
                // @@cases
                default: return null;
            }
        }
    }
}
