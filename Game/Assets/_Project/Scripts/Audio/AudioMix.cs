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
        private static float sidechain = 1f;

        /// <summary>Loudness of nearby structure and monster voices this frame (AudioPool), which ducks the ambience.</summary>
        public static float SidechainInput { get; set; }

        /// <summary>The ambience's sidechain gain right now; for the F1 overlay.</summary>
        public static float Sidechain => sidechain;

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
            // A creak or a monster close by pushes the room tone down so it reads (fast down, slow back up).
            float duckTarget = Mathf.Lerp(1f, config.SidechainFloor, Mathf.Clamp01(SidechainInput));
            float time = duckTarget < sidechain ? config.SidechainAttack : config.SidechainRelease;
            sidechain = Mathf.Lerp(sidechain, duckTarget, 1f - Mathf.Exp(-Time.unscaledDeltaTime / time));
            for (int i = 0; i < BusCount; i++)
            {
                var bus = (AudioBus)i;
                float gain = bus switch
                {
                    AudioBus.Music => AudioLevels.Music * background,
                    AudioBus.Ambience => AudioLevels.Ambience * sfx * background * sidechain,
                    AudioBus.Vitals => sfx * GameSettings.VitalsVolume,
                    _ => sfx,
                };
                gains[i] = gain * config.Trim(bus) * config.BaseLevel;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            frame = -1;
            sidechain = 1f;
            SidechainInput = 0f;
        }
    }
}
