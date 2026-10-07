using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Audio
{
    /// <summary>
    /// Music belongs to the HQ/menu, the drive and the truck's departure countdown. Exploration is carried
    /// by the building and crew; payday/level stings wait until the return to HQ, and deaths stay diegetic.
    /// One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        // The HQ's scene name (Company.CompanyService.HomeLevel; Audio sits below Company).
        private const string HomeScene = "HQ";
        private const float ThemeLevel = 0.3f, StingLevel = 0.45f;

        private enum Mood { Quiet, Hq, Drive, Departure }

        private static MusicPlayer instance;
        private readonly Dictionary<MusicSting, AudioClip> stings = new();
        private AudioSource theme, sting;
        private float themeVolume;
        private AudioClip hqClip, driveClip, departureClip;
        private Mood mood;
        private bool travelling;
        private MusicSting? pendingReward;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Application.isBatchMode || instance != null) return;
            var go = new GameObject("Music");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<MusicPlayer>();
        }

        /// <summary>Reward stings are held for HQ so they never interrupt the building's warning sounds.</summary>
        public static void Play(MusicSting cue)
        {
            if (instance == null || cue == MusicSting.Death) return;
            if (!AtHq || instance.travelling)
            {
                // One arrival cue reads better than a stack of payday and promotion fanfares.
                if (instance.pendingReward != MusicSting.LevelUp) instance.pendingReward = cue;
                return;
            }
            instance.Sting(cue);
        }

        public static void SetTravel(bool on)
        {
            if (instance != null) instance.travelling = on;
        }

        private static bool AtHq => SceneManager.GetActiveScene().name == HomeScene;

        private void Sting(MusicSting cue)
        {
            if (!stings.TryGetValue(cue, out AudioClip clip)) stings[cue] = clip = MusicSynth.Sting(cue);
            sting.Stop();
            sting.clip = clip;
            sting.volume = AudioLevels.Music * StingLevel * AudioLevels.BackgroundDuck;
            sting.Play();
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
            s.priority = 192;
            return s;
        }

        private void Update()
        {
            Mood wanted = Context();
            if (AtHq && !travelling && pendingReward is MusicSting reward)
            {
                pendingReward = null;
                Sting(reward);
            }
            if (wanted != Mood.Hq && sting.isPlaying) sting.Stop();
            sting.volume = AudioLevels.Music * StingLevel * AudioLevels.BackgroundDuck;
            // Fade between places rather than cutting a chord across a scene load.
            float target = wanted == mood && mood != Mood.Quiet ? AudioLevels.Music * ThemeLevel * AudioLevels.BackgroundDuck : 0f;
            if (mood == Mood.Departure) target *= 0.85f;
            // Duck under a sting so it reads.
            if (sting.isPlaying) target *= 0.35f;
            themeVolume = Mathf.MoveTowards(themeVolume, target, Time.unscaledDeltaTime * 0.8f);
            theme.volume = themeVolume;
            if (themeVolume > 0f && !theme.isPlaying && theme.clip != null) theme.Play();
            else if (themeVolume <= 0f && theme.isPlaying) theme.Pause();
            if (wanted != mood && themeVolume <= 0f)
            {
                theme.Stop();
                mood = wanted;
                theme.clip = mood switch
                {
                    Mood.Hq => hqClip ??= MusicSynth.HqTheme(),
                    Mood.Drive => driveClip ??= MusicSynth.TravelTheme(),
                    Mood.Departure => departureClip ??= MusicSynth.DepartureTheme(),
                    _ => null,
                };
            }
        }

        private Mood Context()
        {
            if (travelling) return Mood.Drive;
            Extraction.RunState run = Extraction.RunState.Current;
            if (run != null && run.IsSpawned)
                return Mood.Quiet;
            UI.MenuUi menu = UI.MenuUi.Current;
            bool offlineMenu = menu != null && (int)menu.Showing >= 0 && menu.Bootstrap != null &&
                               (menu.Bootstrap.Manager == null || !menu.Bootstrap.Manager.IsListening);
            return AtHq || offlineMenu ? Mood.Hq : Mood.Quiet;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }
    }
}
