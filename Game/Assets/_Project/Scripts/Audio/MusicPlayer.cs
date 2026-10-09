using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Audio
{
    /// <summary>
    /// All the game's music, on one object (0.14.0). Music belongs to the HQ and menus and the drive.
    /// Exploration is carried by the building and the crew, plus the song of
    /// whichever monster is coming for you: Threats.ThreatMusic decides which song and how loud, and this
    /// plays it. Payday and level stings wait until the return to HQ, and deaths stay diegetic.
    /// Music is flat in the listener's ears, so it has no room on it, but the low-health muffle reaches it.
    /// One per game, made on first scene load (never in batch mode).
    /// </summary>
    public class MusicPlayer : MonoBehaviour
    {
        // The HQ's scene name (Company.CompanyService.HomeLevel; Audio sits below Company).
        private const string HomeScene = "HQ";
        private const float ThemeLevel = 0.3f, StingLevel = 0.45f, ThreatLevel = 0.6f, ThreatCrossfade = 1.5f;

        private enum Mood { Quiet, Hq, Drive, Departure }

        private static MusicPlayer instance;
        private AudioSource theme, sting, threat, threatOld;
        private AudioLowPassFilter muffle;
        private float themeVolume;
        private Mood mood;
        private bool travelling;
        private MusicSting? pendingReward;
        // The approaching monster's song.
        private SoundId? threatSong;
        private AudioClip threatCustom;
        private float threatTarget;
        private bool threatWanted, threatStarted;

        /// <summary>How loud the monster's song is playing right now (0..1 of the source); for the F1 overlay.</summary>
        public static float ThreatVolume => instance != null ? instance.threat.volume : 0f;

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

        /// <summary>
        /// The song of the monster coming for this player, and how loud (0..1). Call every time the choice
        /// is made; a different song crossfades in. <paramref name="custom"/> is a clip set on the threat's
        /// definition, or null for the built-in theme.
        /// </summary>
        public static void SetThreat(SoundId song, AudioClip custom, float volume01)
        {
            if (instance == null) return;
            instance.threatWanted = true;
            instance.threatTarget = Mathf.Clamp01(volume01);
            if (instance.threatSong == song && instance.threatCustom == custom) return;
            instance.threatSong = song;
            instance.threatCustom = custom;
            instance.threatStarted = false;
        }

        /// <summary>Nothing is coming: the song fades out.</summary>
        public static void ClearThreat()
        {
            if (instance != null) instance.threatWanted = false;
        }

        private static bool AtHq => SceneManager.GetActiveScene().name == HomeScene;

        // A recorded track in the SoundBank replaces the built-in one; a built-in one may still be rendering.
        private static AudioClip Clip(SoundId id)
        {
            SoundBank bank = GameAudio.Bank;
            SoundCue cue = bank != null ? bank.Get(id) : null;
            if (cue != null && cue.HasClips && bank.Revision >= SoundCatalog.Revision) return cue.Pick();
            return SynthLibrary.TryGet(id, 0, out AudioClip clip) ? clip : null;
        }

        private void Sting(MusicSting cue)
        {
            AudioClip clip = Clip(cue switch
            {
                MusicSting.Payday => SoundId.StingPayday,
                MusicSting.QuotaMissed => SoundId.StingQuotaMissed,
                MusicSting.LevelUp => SoundId.StingLevelUp,
                _ => SoundId.StingDeath,
            });
            if (clip == null)
            {
                pendingReward = cue; // not rendered yet: try again next frame
                return;
            }
            sting.Stop();
            sting.clip = clip;
            sting.volume = AudioMix.Gain(AudioBus.Music) * StingLevel;
            sting.Play();
        }

        private void Awake()
        {
            theme = Source(loop: true, ignorePause: true, 192);
            sting = Source(loop: false, ignorePause: true, 192);
            threat = Source(loop: true, ignorePause: false, 64);
            threatOld = Source(loop: true, ignorePause: false, 64);
            muffle = gameObject.AddComponent<AudioLowPassFilter>();
            muffle.lowpassResonanceQ = 1f;
            muffle.enabled = false;
        }

        private AudioSource Source(bool loop, bool ignorePause, int priority)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.loop = loop;
            s.playOnAwake = false;
            s.spatialBlend = 0f;
            s.volume = 0f;
            s.ignoreListenerPause = ignorePause;
            s.bypassListenerEffects = true;
            s.bypassReverbZones = true;
            s.priority = priority;
            return s;
        }

        private void Update()
        {
            float bus = AudioMix.Gain(AudioBus.Music);
            // Music skips the listener's room, so it has to be given the low-health muffle itself.
            float cutoff = ListenerSpace.MuffleNow;
            muffle.enabled = cutoff < ListenerSpace.OpenCutoff - 250f;
            if (muffle.enabled) muffle.cutoffFrequency = cutoff;
            Places(bus);
            Threat(bus);
        }

        // HQ, drive and departure themes, and the stings.
        private void Places(float bus)
        {
            Mood wanted = Context();
            if (AtHq && !travelling && pendingReward is MusicSting reward)
            {
                pendingReward = null;
                Sting(reward);
            }
            if (wanted != Mood.Hq && sting.isPlaying) sting.Stop();
            sting.volume = bus * StingLevel;
            // Fade between places rather than cutting a chord across a scene load.
            float target = wanted == mood && mood != Mood.Quiet ? bus * ThemeLevel : 0f;
            if (mood == Mood.Departure) target *= 0.85f;
            // Duck under a sting so it reads.
            if (sting.isPlaying) target *= 0.35f;
            if (mood != Mood.Quiet && theme.clip == null)
            {
                theme.clip = Clip(mood switch
                {
                    Mood.Hq => SoundId.MusicHq,
                    Mood.Drive => SoundId.MusicDrive,
                    _ => SoundId.MusicDeparture,
                });
                if (theme.clip == null) target = 0f; // still rendering: it fades in when it arrives
            }
            themeVolume = Mathf.MoveTowards(themeVolume, target, Time.unscaledDeltaTime * 0.8f);
            theme.volume = themeVolume;
            if (themeVolume > 0f && !theme.isPlaying && theme.clip != null) theme.Play();
            else if (themeVolume <= 0f && theme.isPlaying) theme.Pause();
            if (wanted != mood && themeVolume <= 0f)
            {
                theme.Stop();
                theme.clip = null;
                mood = wanted;
            }
        }

        // The approaching monster's song: swells quickly as it closes, fades slower once it gives up.
        private void Threat(float bus)
        {
            if (threatWanted && !threatStarted && threatSong is SoundId song)
            {
                AudioClip clip = threatCustom != null ? threatCustom : Clip(song);
                if (clip != null)
                {
                    (threat, threatOld) = (threatOld, threat);
                    threat.clip = clip;
                    threat.volume = 0f;
                    threat.time = 0f;
                    threat.Play();
                    threatStarted = true;
                }
            }
            float step = Time.unscaledDeltaTime / ThreatCrossfade;
            float want = threatWanted && threatStarted ? threatTarget * ThreatLevel * bus : 0f;
            threat.volume = Mathf.MoveTowards(threat.volume, want, step * (want > threat.volume ? 1.5f : 0.6f));
            threatOld.volume = Mathf.MoveTowards(threatOld.volume, 0f, step);
            if (threatOld.isPlaying && threatOld.volume <= 0f) threatOld.Stop();
            if (!threatWanted && threat.volume <= 0f && threat.isPlaying)
            {
                threat.Stop();
                threatSong = null;
                threatStarted = false;
            }
        }

        private Mood Context()
        {
            if (travelling) return Mood.Drive;
            Extraction.RunState run = Extraction.RunState.Current;
            // A job has no music of its own, the departure included (off since the 0.13.0 horror redesign:
            // the truck's doors and engine carry that moment). The departure theme stays ready in the catalog.
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
