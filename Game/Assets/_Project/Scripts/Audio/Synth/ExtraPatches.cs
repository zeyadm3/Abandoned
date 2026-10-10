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
                // @@cases
                default: return null;
            }
        }
    }
}
