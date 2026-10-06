using UnityEngine;

namespace Abandoned.Voice
{
    /// <summary>
    /// Plays one remote player's voice from their head: decodes packets into a jitter buffer that a
    /// streaming AudioClip pulls from on the audio thread. Unity only pans it (flat rolloff curve); the
    /// volume is our own proximity falloff times the player's voice volume, set every frame.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class VoicePlayback : MonoBehaviour
    {
        [SerializeField] private VoiceConfig config;

        private AudioSource source;
        private volatile VoiceJitterBuffer buffer;
        private int sampleRate;
        private float[] decoded = new float[8192];

        public VoiceJitterBuffer Buffer => buffer;
        public int SamplesReceived { get; private set; }
        public float Gain { get; private set; }

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 1f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.SetCustomCurve(AudioSourceCurveType.CustomRolloff, AnimationCurve.Constant(0f, 1f, 1f));
            source.maxDistance = config.MaxDistance;
            source.volume = 0f;
        }

        public void Push(byte[] data, VoiceCodecId codec, bool radio)
        {
            IVoiceCodec decoder = VoiceBackends.Codec(codec);
            if (decoder == null || decoder.SampleRate <= 0) return;
            EnsureStream(decoder.SampleRate);
            int n = decoder.Decode(data, data.Length, decoded);
            buffer.Write(decoded, n);
            SamplesReceived += n;
        }

        // Steam voice and the microphone backend run at different rates; the stream follows the codec.
        private void EnsureStream(int rate)
        {
            if (rate == sampleRate && buffer != null) return;
            sampleRate = rate;
            if (decoded.Length < rate / 2) decoded = new float[rate / 2];
            buffer = new VoiceJitterBuffer(rate * config.JitterMs / 1000, rate * config.MaxBufferMs / 1000);
            source.Stop();
            source.clip = AudioClip.Create($"Voice {name}", rate, 1, rate, true, OnAudioRead);
            source.Play();
        }

        // Audio thread.
        private void OnAudioRead(float[] data) => buffer?.Read(data, data.Length);

        private void Update()
        {
            AudioListener listener = VoiceListener.Current;
            float distance = listener != null ? Vector3.Distance(listener.transform.position, transform.position) : 0f;
            Gain = VoiceMath.DistanceGain(distance, config.MinDistance, config.MaxDistance);
            source.volume = Gain * VoiceSettings.Volume;
        }

        private void OnDisable()
        {
            if (source != null) source.Stop();
            buffer?.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, config.MinDistance);
            Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, config.MaxDistance);
        }
    }
}
