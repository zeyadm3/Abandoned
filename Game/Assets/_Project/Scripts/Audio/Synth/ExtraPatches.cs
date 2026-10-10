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
                // @@cases
                default: return null;
            }
        }
    }
}
