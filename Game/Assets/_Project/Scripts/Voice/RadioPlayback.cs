using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// A walkie-talkie transmission as this machine's radio plays it: not positional (it's in your
    /// hand), band-limited like a cheap radio and with static under the voice. Lives on the remote
    /// speaker's player, so several people talking over the radio mix naturally.
    /// </summary>
    [RequireComponent(typeof(AudioSource), typeof(AudioHighPassFilter), typeof(AudioLowPassFilter))]
    public class RadioPlayback : MonoBehaviour
    {
        [SerializeField] private VoiceConfig config;

        private AudioSource source;
        private VoiceStream stream;
        private System.Random noise;
        private float staticLevel;

        public VoiceJitterBuffer Buffer => stream?.Buffer;
        public int SamplesReceived => stream?.SamplesReceived ?? 0;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.spatialBlend = 0f;
            source.volume = 0f;
            GetComponent<AudioHighPassFilter>().cutoffFrequency = config.RadioLowCut;
            GetComponent<AudioLowPassFilter>().cutoffFrequency = config.RadioHighCut;
            noise = new System.Random(GetInstanceID());
            staticLevel = config.RadioStatic;
            stream = new VoiceStream(source, config, AddStatic);
        }

        public void Push(float[] pcm, int count, int rate) => stream.Push(pcm, count, rate);

        private void Update()
        {
            source.volume = config.RadioVolume * VoiceSettings.Volume;
            stream.Tick();
        }

        // Audio thread: hiss while the transmission plays, and a slight overdrive like a small speaker.
        private void AddStatic(float[] data, int played)
        {
            if (played <= 0) return;
            for (int i = 0; i < played; i++)
            {
                float hiss = (float)(noise.NextDouble() * 2.0 - 1.0) * staticLevel;
                data[i] = Mathf.Clamp(data[i] * 1.6f + hiss, -0.9f, 0.9f);
            }
        }

        private void OnDisable() => stream?.Stop();

        private void OnDestroy() => stream?.Dispose();
    }
}
