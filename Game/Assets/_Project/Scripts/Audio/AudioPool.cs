using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// The one place sound comes out of (0.14.0): a fixed set of reusable one-shot voices (no object per
    /// sound, unlike PlayClipAtPoint) and every running loop. When all voices are busy the oldest sound of
    /// equal or lower importance is cut, so the building's warnings survive a pile of dropped loot. Each
    /// frame every voice and loop is re-levelled from its bus, so the volume settings act on what is playing.
    /// </summary>
    public class AudioPool : MonoBehaviour
    {
        private static AudioPool instance;
        private static readonly List<SoundLoop> loops = new();
        private AudioVoice[] voices;

        /// <summary>Voices sounding this frame; for the F1 overlay.</summary>
        public static int Playing { get; private set; }

        private static AudioPool Instance
        {
            get
            {
                if (instance != null) return instance;
                var go = new GameObject("AudioPool");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<AudioPool>();
                return instance;
            }
        }

        // Made at once outside batch mode, so the synth's finished renders become clips from the first frame.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!Application.isBatchMode) _ = Instance;
        }

        private void Awake()
        {
            voices = new AudioVoice[AudioConfig.Current.Voices];
            for (int i = 0; i < voices.Length; i++) voices[i] = new AudioVoice(transform, i);
        }

        /// <summary>Plays a clip at a point (spatial) or flat in the listener's ears (UI, 2D).</summary>
        public static AudioSource Play(AudioClip clip, in VoiceRequest request)
        {
            if (clip == null || request.Volume * request.OcclusionGain <= 0f) return null;
            AudioVoice voice = Instance.Next(request.Priority);
            if (voice == null) return null;
            voice.Play(clip, request);
            return voice.Source;
        }

        // Capture the voice's use, so a caller cannot stop a different sound after this source is reused.
        internal static SoundPlayback Capture(AudioSource source)
        {
            if (source == null || instance == null) return default;
            foreach (AudioVoice voice in instance.voices)
                if (voice.Source == source) return new SoundPlayback(voice);
            return default;
        }

        private AudioVoice Next(int priority)
        {
            int oldest = -1;
            for (int i = 0; i < voices.Length; i++)
            {
                AudioVoice v = voices[i];
                if (!v.Source.isPlaying) return v;
                if (v.Source.priority >= priority && (oldest < 0 || v.Started < voices[oldest].Started)) oldest = i;
            }
            return oldest < 0 ? null : voices[oldest];
        }

        internal static void Register(SoundLoop loop)
        {
            _ = Instance; // loops are levelled from this object's Update
            loops.Add(loop);
        }

        private void Update()
        {
            int playing = 0;
            foreach (AudioVoice v in voices)
            {
                if (!v.Source.isPlaying) continue;
                v.Refresh();
                playing++;
            }
            Playing = playing;
            SynthLibrary.Adopt();
            // A loop whose object went with its scene drops out here.
            for (int i = loops.Count - 1; i >= 0; i--)
                if (!loops[i].Tick()) loops.RemoveAt(i);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!DebugView.Visible) return;
            GUI.Label(new Rect(Screen.width - 360f, 118f, 350f, 22f),
                $"AUDIO voices {Playing}/{voices.Length}  loops {loops.Count}  synth {SynthLibrary.Rendered}/{SynthLibrary.Total}  last {GameAudio.LastSound}");
            GUI.Label(new Rect(Screen.width - 360f, 140f, 350f, 22f), ListenerSpace.Describe());
        }

        // Where the last sounds were played, coloured by bus (Scene view, with F1 on).
        private void OnDrawGizmos()
        {
            if (!DebugView.Visible) return;
            foreach (GameAudio.Mark mark in GameAudio.Recent)
            {
                float age = Time.unscaledTime - mark.Time;
                if (age > 4f) continue;
                Gizmos.color = mark.Bus switch
                {
                    AudioBus.Structure => new Color(1f, 0.6f, 0.1f, 1f - age / 4f),
                    AudioBus.Threats => new Color(1f, 0.15f, 0.15f, 1f - age / 4f),
                    AudioBus.Footsteps => new Color(0.4f, 1f, 0.4f, 1f - age / 4f),
                    _ => new Color(0.3f, 0.8f, 1f, 1f - age / 4f),
                };
                Gizmos.DrawWireSphere(mark.Position, 0.25f + age * 0.3f);
            }
        }
#endif

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            loops.Clear();
            Playing = 0;
        }
    }
}
