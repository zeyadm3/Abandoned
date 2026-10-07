using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Audio
{
    /// <summary>
    /// Music (M10.8): the HQ theme plays at the HQ and on the main menu (which sits over it) and fades out
    /// on a job, where the building's own sounds carry the mood; stings mark payday, a missed quota, a death
    /// and a level-up. One per game, made on first scene load (never in batch mode: tests stay silent).
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        // The HQ's scene name (Company.CompanyService.HomeLevel; Audio sits below Company).
        private const string HomeScene = "HQ";
        private const float ThemeLevel = 0.35f, StingLevel = 0.6f;

        private static MusicPlayer instance;
        private readonly Dictionary<MusicSting, AudioClip> stings = new();
        private AudioSource theme, sting;
        private float themeVolume;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("Music");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
        }

        /// <summary>Play a sting over whatever is playing (nothing in batch mode).</summary>
        public static void Play(MusicSting cue)
        {
            if (instance == null) return;
            if (!instance.stings.TryGetValue(cue, out AudioClip clip)) instance.stings[cue] = clip = MusicSynth.Sting(cue);
            instance.sting.Stop();
            instance.sting.clip = clip;
            instance.sting.volume = AudioLevels.Music * StingLevel;
            instance.sting.Play();
        }

        private void Awake()
        {
            theme = Source(loop: true);
            sting = Source(loop: false);
        }

        private AudioSource Source(bool loop)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.loop = loop;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.ignoreListenerPause = true;
            return s;
        }

        private void Update()
        {
            bool home = SceneManager.GetActiveScene().name == HomeScene;
            if (home && theme.clip == null) theme.clip = MusicSynth.HqTheme();
            float target = home ? AudioLevels.Music * ThemeLevel : 0f;
            // Duck under a sting so it reads.
            if (sting.isPlaying) target *= 0.35f;
            themeVolume = Mathf.MoveTowards(themeVolume, target, Time.unscaledDeltaTime * 0.25f);
            theme.volume = themeVolume;
            if (themeVolume > 0f && !theme.isPlaying && theme.clip != null) theme.Play();
            else if (themeVolume <= 0f && theme.isPlaying) theme.Pause();
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
