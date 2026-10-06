using System;
using UnityEngine;

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
        private int sampleRate;

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
                buffer = new VoiceJitterBuffer(rate * config.JitterMs / 1000, rate * config.MaxBufferMs / 1000);
                source.Stop();
                source.clip = AudioClip.Create($"Voice {source.name}", rate, 1, rate, true, OnAudioRead);
                source.Play();
            }
            buffer.Write(pcm, count);
            SamplesReceived += count;
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
            int played = b != null ? b.Read(data, data.Length) : 0;
            if (b == null) Array.Clear(data, 0, data.Length);
            process?.Invoke(data, played);
        }
    }
}
