using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// One gain per bus, worked out once a frame from the player's volume settings, the config's base level
    /// and trims, and the ducking under crew speech. Every voice, loop and music source multiplies by its bus here,
    /// so a slider moves sounds that are already playing.
    /// </summary>
    public static class AudioMix
    {
        private static readonly int BusCount = System.Enum.GetValues(typeof(AudioBus)).Length;
        private static readonly float[] gains = new float[BusCount];
        private static int frame = -1;

        public static float Gain(AudioBus bus)
        {
            if (frame != Time.frameCount) Refresh();
            return gains[(int)bus];
        }

        private static void Refresh()
        {
            frame = Time.frameCount;
            AudioConfig config = AudioConfig.Current;
            float sfx = AudioLevels.Sfx;
            // Speech eases the background down; the building's warnings and the monsters stay as they are.
            float background = AudioLevels.BackgroundDuck;
            for (int i = 0; i < BusCount; i++)
            {
                var bus = (AudioBus)i;
                float gain = bus switch
                {
                    AudioBus.Music => AudioLevels.Music * background,
                    AudioBus.Ambience => AudioLevels.Ambience * sfx * background,
                    AudioBus.Vitals => sfx * GameSettings.VitalsVolume,
                    _ => sfx,
                };
                gains[i] = gain * config.Trim(bus) * config.BaseLevel;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => frame = -1;
    }
}
