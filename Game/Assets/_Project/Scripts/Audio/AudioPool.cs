using UnityEngine;

namespace Abandoned.Audio
{
    /// <summary>
    /// A fixed set of reusable one-shot sources (no object per sound, unlike PlayClipAtPoint). When all
    /// are busy the one that started longest ago is cut: the newest sound matters most.
    /// </summary>
    public class AudioPool : MonoBehaviour
    {
        private const int Size = 32;

        private static AudioPool instance;
        private AudioSource[] sources;
        private float[] started;

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

        private void Awake()
        {
            sources = new AudioSource[Size];
            started = new float[Size];
            for (int i = 0; i < Size; i++)
            {
                var child = new GameObject($"Source{i}");
                child.transform.SetParent(transform, false);
                AudioSource s = child.AddComponent<AudioSource>();
                s.playOnAwake = false;
                s.rolloffMode = AudioRolloffMode.Linear;
                s.dopplerLevel = 0f;
                sources[i] = s;
            }
        }

        /// <summary>Plays a clip at a point (spatial) or flat in the listener's ears (UI, 2D).</summary>
        public static AudioSource Play(AudioClip clip, Vector3 position, float volume, float pitch, float minDistance, float maxDistance, bool spatial)
        {
            if (clip == null || volume <= 0f) return null;
            AudioSource s = Instance.Next();
            s.transform.position = position;
            s.clip = clip;
            s.volume = volume;
            s.pitch = pitch;
            s.spatialBlend = spatial ? 1f : 0f;
            s.minDistance = minDistance;
            s.maxDistance = Mathf.Max(minDistance + 0.1f, maxDistance);
            s.Play();
            return s;
        }

        private AudioSource Next()
        {
            int oldest = 0;
            for (int i = 0; i < Size; i++)
            {
                if (!sources[i].isPlaying) { oldest = i; break; }
                if (started[i] < started[oldest]) oldest = i;
            }
            started[oldest] = Time.unscaledTime;
            return sources[oldest];
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;
    }
}
