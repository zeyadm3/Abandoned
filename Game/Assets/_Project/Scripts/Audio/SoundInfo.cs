namespace Abandoned.Audio
{
    /// <summary>
    /// What a sound is, as opposed to how it is tuned: its bus, importance, behaviour behind walls and
    /// caption. The numbers here are also the defaults for a cue the SoundBank doesn't hold yet.
    /// </summary>
    public readonly struct SoundInfo
    {
        public readonly AudioBus Bus;
        /// <summary>0 is never cut off, 255 goes first when every voice is busy.</summary>
        public readonly int Priority;
        public readonly float Volume, MinDistance, MaxDistance, PitchLow, PitchHigh;
        /// <summary>Seconds before the same sound may start again (0 = no limit).</summary>
        public readonly float Cooldown;
        public readonly SoundMode Mode;
        public readonly SoundOcclusion Occlusion;
        /// <summary>How many different synthesised takes exist.</summary>
        public readonly int Variations;
        /// <summary>Always in the listener's ears, never at a point in the world.</summary>
        public readonly bool Flat;
        public readonly bool Loop;
        /// <summary>Subtitle text, or null for sounds that aren't captioned.</summary>
        public readonly string Caption;

        public SoundInfo(AudioBus bus, int priority, float volume, float minDistance, float maxDistance, float pitchLow,
            float pitchHigh, float cooldown, SoundMode mode, SoundOcclusion occlusion, int variations, bool flat, bool loop,
            string caption)
        {
            Bus = bus;
            Priority = priority;
            Volume = volume;
            MinDistance = minDistance;
            MaxDistance = maxDistance;
            PitchLow = pitchLow;
            PitchHigh = pitchHigh;
            Cooldown = cooldown;
            Mode = mode;
            Occlusion = occlusion;
            Variations = variations;
            Flat = flat;
            Loop = loop;
            Caption = caption;
        }
    }
}
