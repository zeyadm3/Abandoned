using Abandoned.Voice;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    public class VoiceCoreTests
    {
        [Test]
        public void MuLawRoundTripsSpeechLevelsClosely()
        {
            for (float x = -1f; x <= 1f; x += 0.01f)
            {
                float back = MuLaw.Decode(MuLaw.Encode(x));
                // mu-law error grows with amplitude (~3 % of the value) but stays tiny near silence.
                Assert.AreEqual(x, back, 0.002f + Mathf.Abs(x) * 0.04f, $"sample {x}");
            }
            Assert.AreEqual(0f, MuLaw.Decode(MuLaw.Encode(0f)), 0.0005f);
            Assert.Less(MuLaw.Decode(MuLaw.Encode(-0.5f)), 0f, "sign survives");
            Assert.AreEqual(1f, MuLaw.Decode(MuLaw.Encode(5f)), 0.05f, "clipped, not wrapped");
        }

        [Test]
        public void JitterBufferWaitsForItsTargetThenPlaysInOrder()
        {
            var buffer = new VoiceJitterBuffer(4, 16);
            var out4 = new float[4];
            buffer.Write(new[] { 1f, 2f, 3f }, 3);
            buffer.Read(out4, 4);
            CollectionAssert.AreEqual(new[] { 0f, 0f, 0f, 0f }, out4, "silence until the target is queued");
            Assert.AreEqual(3, buffer.Count, "nothing consumed while priming");

            buffer.Write(new[] { 4f, 5f }, 2);
            buffer.Read(out4, 4);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 3f, 4f }, out4);
        }

        [Test]
        public void JitterBufferRePrimesAfterRunningDry()
        {
            var buffer = new VoiceJitterBuffer(2, 16);
            var out3 = new float[3];
            buffer.Write(new[] { 1f, 2f }, 2);
            buffer.Read(out3, 3);
            CollectionAssert.AreEqual(new[] { 1f, 2f, 0f }, out3);
            Assert.AreEqual(1, buffer.Underruns);
            Assert.IsFalse(buffer.Playing);
            buffer.Write(new[] { 9f }, 1);
            buffer.Read(out3, 3);
            CollectionAssert.AreEqual(new[] { 0f, 0f, 0f }, out3, "one sample isn't enough to restart");
        }

        [Test]
        public void JitterBufferDropsTheOldestWhenFull()
        {
            var buffer = new VoiceJitterBuffer(1, 4);
            buffer.Write(new[] { 1f, 2f, 3f, 4f, 5f, 6f }, 6);
            Assert.AreEqual(4, buffer.Count);
            Assert.AreEqual(2, buffer.Dropped);
            var out4 = new float[4];
            buffer.Read(out4, 4);
            CollectionAssert.AreEqual(new[] { 3f, 4f, 5f, 6f }, out4, "the newest audio is kept, so delay can't grow");
        }

        [Test]
        public void DistanceGainIsFullUpCloseAndSilentPastTheMaximum()
        {
            Assert.AreEqual(1f, VoiceMath.DistanceGain(0.5f, 1.5f, 25f));
            Assert.AreEqual(1f, VoiceMath.DistanceGain(1.5f, 1.5f, 25f));
            Assert.AreEqual(0f, VoiceMath.DistanceGain(25f, 1.5f, 25f));
            Assert.AreEqual(0f, VoiceMath.DistanceGain(80f, 1.5f, 25f));
            float previous = 1f;
            for (float d = 1.6f; d < 25f; d += 0.5f)
            {
                float g = VoiceMath.DistanceGain(d, 1.5f, 25f);
                Assert.Less(g, previous, $"quieter at {d} m");
                previous = g;
            }
            Assert.Greater(VoiceMath.DistanceGain(5f, 1.5f, 25f), 0.2f, "a room away is still clearly audible");
        }

        [Test]
        public void LevelsSurviveTheWire()
        {
            Assert.AreEqual(0, VoiceMath.ToByte(-1f));
            Assert.AreEqual(255, VoiceMath.ToByte(2f));
            Assert.AreEqual(0.5f, VoiceMath.FromByte(VoiceMath.ToByte(0.5f)), 0.003f);
            var tone = new float[1000];
            for (int i = 0; i < tone.Length; i++) tone[i] = 0.5f * Mathf.Sin(i * 0.1f);
            Assert.AreEqual(0.5f / Mathf.Sqrt(2f), VoiceMath.Rms(tone, tone.Length), 0.01f);
            Assert.AreEqual(0f, VoiceMath.Rms(tone, 0));
        }

        [Test]
        public void MuLawCodecDecodesWhatTheEncoderMade()
        {
            var samples = new[] { 0f, 0.25f, -0.25f, 0.9f };
            var packet = new byte[8];
            int n = MuLawCodec.Encode(samples, samples.Length, packet);
            var pcm = new float[8];
            Assert.AreEqual(4, new MuLawCodec().Decode(packet, n, pcm));
            for (int i = 0; i < 4; i++) Assert.AreEqual(samples[i], pcm[i], 0.04f);
            Assert.AreSame(VoiceBackends.Codec(VoiceCodecId.MuLaw), VoiceBackends.Codec(VoiceCodecId.MuLaw));
            Assert.IsNull(VoiceBackends.Codec((VoiceCodecId)200), "unknown codecs are refused, not guessed");
        }

        [Test]
        public void BatchModeNeverOpensARealMicrophone()
        {
            VoiceBackends.CaptureOverride = null;
            using IVoiceCapture capture = VoiceBackends.CreateCapture(false);
            if (Application.isBatchMode)
            {
                Assert.IsInstanceOf<SilentVoiceCapture>(capture);
                Assert.IsNotEmpty(capture.Problem);
                Assert.AreEqual(0, capture.ReadPacket(new byte[64]));
            }
        }

        [Test]
        public void WallsMuffleRatherThanSilence()
        {
            Assert.AreEqual(1f, VoiceMath.OcclusionGain(0, 0.55f, 3));
            Assert.AreEqual(0.55f, VoiceMath.OcclusionGain(1, 0.55f, 3), 1e-5f);
            Assert.AreEqual(0.55f * 0.55f * 0.55f, VoiceMath.OcclusionGain(9, 0.55f, 3), 1e-5f, "capped at the max walls");
            Assert.AreEqual(VoiceMath.OpenCutoff, VoiceMath.OcclusionCutoff(0, 900f, 3));
            Assert.AreEqual(900f, VoiceMath.OcclusionCutoff(1, 900f, 3));
            Assert.Less(VoiceMath.OcclusionCutoff(2, 900f, 3), 900f);
            Assert.AreEqual(VoiceMath.OcclusionCutoff(3, 900f, 3), VoiceMath.OcclusionCutoff(5, 900f, 3));
        }

        [Test]
        public void WhispersAreSilentToMonstersShoutsAreLoudAndTheRadioSquawks()
        {
            var c = ScriptableObject.CreateInstance<VoiceConfig>();
            try
            {
                Assert.AreEqual(0f, VoiceMath.NoiseLoudness(c.WhisperLevel, false, c));
                Assert.AreEqual(c.ShoutLoudness, VoiceMath.NoiseLoudness(c.ShoutLevel, false, c), 1e-5f);
                Assert.AreEqual(c.ShoutLoudness, VoiceMath.NoiseLoudness(1f, false, c), 1e-5f, "louder than a shout doesn't grow further");
                float normal = VoiceMath.NoiseLoudness((c.WhisperLevel + c.ShoutLevel) / 2f, false, c);
                Assert.AreEqual(c.ShoutLoudness / 2f, normal, 1e-4f, "talking normally is half a shout");
                Assert.AreEqual(c.RadioLoudness, VoiceMath.NoiseLoudness(0f, true, c), 1e-5f, "even a whispered radio call clicks");
            }
            finally
            {
                Object.DestroyImmediate(c);
            }
        }

        [Test]
        public void VoiceNoiseIsImmediateThenRateLimitedToTheLoudestMoment()
        {
            var meter = new VoiceNoiseMeter(0.25f);
            Assert.IsTrue(meter.Add(0.2f, 10f, out float first), "the first word is heard at once");
            Assert.AreEqual(0.2f, first);
            Assert.IsFalse(meter.Add(0.5f, 10.1f, out _));
            Assert.IsFalse(meter.Add(0.1f, 10.2f, out _));
            Assert.IsTrue(meter.Add(0.1f, 10.3f, out float next));
            Assert.AreEqual(0.5f, next, "the shout in between isn't lost");
            Assert.IsFalse(meter.Add(0f, 11f, out _), "silence emits nothing");
        }

        [Test]
        public void ResamplerKeepsEverySampleAcrossChunksAtTheSameRate()
        {
            var r = new MicResampler(1.0);
            var output = new float[64];
            int total = 0;
            for (int chunk = 0; chunk < 5; chunk++)
            {
                var input = new float[10];
                for (int i = 0; i < 10; i++) input[i] = chunk * 10 + i;
                int n = r.Process(input, 10, output);
                for (int i = 0; i < n; i++) Assert.AreEqual(total + i, output[i], 1e-4f, "no sample dropped at a seam");
                total += n;
            }
            Assert.AreEqual(50, total);
        }

        [Test]
        public void ResamplerDecimatesSmoothlyAt48kHz()
        {
            var r = new MicResampler(3.0);
            var output = new float[4000];
            int total = 0;
            var input = new float[480];
            for (int chunk = 0; chunk < 10; chunk++)
            {
                for (int i = 0; i < input.Length; i++) input[i] = (i % 2 == 0) ? 1f : -1f; // pure 24 kHz hiss
                total += r.Process(input, input.Length, output);
            }
            Assert.AreEqual(1600, total, 1, "48 kHz -> 16 kHz");
            Assert.Less(Mathf.Abs(output[total - 1]), 0.4f, "hiss above the new Nyquist is filtered, not folded down at full strength");
        }

        [Test]
        public void JitterBufferCushionCoversTheAudioReadSize()
        {
            var buffer = new VoiceJitterBuffer(4, 64);
            buffer.EnsureTarget(10);
            Assert.AreEqual(10, buffer.TargetSamples);
            buffer.Write(new float[6], 6);
            Assert.IsFalse(buffer.Playing, "not a whole read queued yet");
            buffer.StartNow();
            Assert.IsTrue(buffer.Playing, "a short utterance plays once the speaker stops");
        }

        [Test]
        public void AStaleShoutIsntHeardAtTheNextWhisper()
        {
            var meter = new VoiceNoiseMeter(0.25f);
            Assert.IsTrue(meter.Add(0.3f, 10f, out _));
            Assert.IsFalse(meter.Add(0.6f, 10.1f, out _), "shout held back by the rate limit");
            // Key released; a minute later, a whisper elsewhere.
            Assert.IsFalse(meter.Add(0f, 70f, out float whisper), "the old shout doesn't leak into it");
            Assert.AreEqual(0f, whisper);
        }
    }
}
