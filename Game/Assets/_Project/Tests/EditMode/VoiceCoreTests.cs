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
    }
}
