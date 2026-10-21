using System;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// Head-related 3D sound (0.24.12) through Valve's Steam Audio spatializer: on headphones a monster behind you
    /// sounds behind you, above sounds above. Steam Audio only pans here; what walls do to a sound stays with
    /// <see cref="AudioOcclusion"/>, which keeps the game's rules (warnings never dulled, monsters never much quieter).
    /// Falls back to Unity's panning when the plugin isn't there or AudioConfig.SteamAudio is off.
    /// </summary>
    public static class SpatialAudio
    {
        private const string PluginName = "Steam Audio Spatializer";
        private static bool active;

        private static bool? started;

        public static bool Active => Ready();
        public static string Mode => Ready() ? "Steam Audio HRTF" : "built-in";

        // Steam Audio starts itself before the first scene. If its native plugin failed to load (a blocked bundle), its
        // manager has no simulator and every SteamAudioSource would throw, so Unity's panning stays instead.
        private static bool Ready()
        {
            if (!active) return false;
            if (started == null)
            {
                started = SteamAudio.SteamAudioManager.Singleton != null && SteamAudio.SteamAudioManager.Simulator != null;
                if (started == false) Debug.LogWarning("[Audio] Steam Audio did not start; using Unity's 3D panning.");
            }
            return started.Value;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            active = false;
            started = null;
            if (Application.isBatchMode || !AudioConfig.Current.SteamAudio) return;
            // The spatializer is chosen in Project Settings > Audio (EditorTools.SpatialAudioSetup); a player can only read it.
            string current = AudioSettings.GetSpatializerPluginName();
            if (string.IsNullOrEmpty(current) || current.IndexOf("Steam Audio", StringComparison.OrdinalIgnoreCase) < 0)
            {
                Debug.LogWarning($"[Audio] {PluginName} is not the project's spatializer; using Unity's 3D panning.");
                return;
            }
            active = true;
        }

        /// <summary>Called whenever a source is (re)used: spatial sounds go through the HRTF, 2D ones never do.</summary>
        public static void Apply(AudioSource source, bool spatial)
        {
            if (source == null) return;
            bool use = spatial && Ready();
            if (source.spatialize != use) source.spatialize = use;
            if (!use || source.TryGetComponent(out SteamAudio.SteamAudioSource _)) return;
            SteamAudio.SteamAudioSource steam = source.gameObject.AddComponent<SteamAudio.SteamAudioSource>();
            steam.directBinaural = true;
            steam.interpolation = SteamAudio.HRTFInterpolation.Bilinear;
            // Unity's own rolloff and our occlusion already shape the level; Steam Audio only places the sound.
            steam.distanceAttenuation = false;
            steam.airAbsorption = false;
            steam.occlusion = false;
            steam.transmission = false;
            steam.reflections = false;
        }
    }
}
