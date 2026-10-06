using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Abandoned.Voice
{
    /// <summary>
    /// One speaker's audio on one AudioSource: decoded PCM goes into a jitter buffer that a streaming
    /// AudioClip pulls on the audio thread. The clip follows the codec's sample rate. An optional
    /// processor runs on the audio thread after each read (the radio's static).
    /// </summary>
    public sealed class VoiceStream
    {
        private readonly AudioSource source;
        private readonly VoiceConfig config;
        private readonly Action<float[], int> process;
        private volatile VoiceJitterBuffer buffer;
        private int sampleRate, baseTarget;
        private AudioClip clip;
        private float lastPush;

        public VoiceStream(AudioSource source, VoiceConfig config, Action<float[], int> process = null)
        {
            this.source = source;
            this.config = config;
            this.process = process;
            source.playOnAwake = false;
            source.loop = true;
        }

        public VoiceJitterBuffer Buffer => buffer;
        public int SamplesReceived { get; private set; }

        public void Push(float[] pcm, int count, int rate)
        {
            if (count <= 0 || rate <= 0) return;
            if (rate != sampleRate || buffer == null)
            {
                sampleRate = rate;
                baseTarget = rate * config.JitterMs / 1000;
                // Room for a 4096-sample read on top of the cushion (see EnsureTarget).
                buffer = new VoiceJitterBuffer(baseTarget, rate * config.MaxBufferMs / 1000 + 4096);
                source.Stop();
                if (clip != null) Object.Destroy(clip);
                clip = AudioClip.Create($"Voice {source.name}", rate, 1, rate, true, OnAudioRead);
                source.clip = clip;
            }
            // Stopped by a disable (or never started): start again when someone speaks.
            if (!source.isPlaying) source.Play();
            buffer.Write(pcm, count);
            SamplesReceived += count;
            lastPush = Time.time;
        }

        /// <summary>Main thread, every frame: a short "go!" shorter than the cushion still plays once the speaker stops.</summary>
        public void Tick()
        {
            VoiceJitterBuffer b = buffer;
            if (b != null && !b.Playing && b.Count > 0 && Time.time - lastPush > config.JitterMs / 1000f) b.StartNow();
        }

        public void Dispose()
        {
            Stop();
            if (clip != null) Object.Destroy(clip);
            clip = null;
        }

        public void Stop()
        {
            if (source != null) source.Stop();
            buffer?.Clear();
        }

        // Audio thread.
        private void OnAudioRead(float[] data)
        {
            VoiceJitterBuffer b = buffer;
            b?.EnsureTarget(data.Length + baseTarget);
            int played = b != null ? b.Read(data, data.Length) : 0;
            if (b == null) Array.Clear(data, 0, data.Length);
            process?.Invoke(data, played);
        }
    }
}
