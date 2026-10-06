using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Voice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>Proximity voice over in-process NGO, with a synthetic tone instead of a microphone.</summary>
    public class NetworkVoiceTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            VoiceBackends.CaptureOverride = () => new ToneVoiceCapture();
            VoiceSettings.Mode = VoiceMode.OpenMic;
            VoiceSettings.MicMuted = false;
            net = new NetTestHarness();
            net.BuildArena();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            VoiceBackends.CaptureOverride = null;
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            VoiceSettings.MicMuted = false;
            NetworkVoice.RadioHolder = null;
            yield return null;
        }

        private static NetworkVoice VoiceOf(NetworkBootstrap machine, ulong speaker) =>
            NetworkVoice.All.Single(v => v.NetworkManager == machine.Manager && v.OwnerClientId == speaker);

        [UnityTest]
        public IEnumerator EveryoneHearsEveryoneElseButNotThemselves()
        {
            yield return net.StartSession(clients: 2);
            ulong[] ids = net.Machines.Select(m => m.Manager.LocalClientId).ToArray();
            yield return WaitFor(() => net.Machines.All(listener => ids.Where(s => s != listener.Manager.LocalClientId)
                    .All(s => VoiceOf(listener, s).Playback.SamplesReceived > 4000)),
                "every machine to receive a quarter second of every other player's voice", 10f);

            foreach (NetworkBootstrap listener in net.Machines)
            {
                NetworkVoice own = VoiceOf(listener, listener.Manager.LocalClientId);
                Assert.IsFalse(own.Playback.enabled, $"{listener.name} doesn't play its own voice back");
                Assert.Greater(own.PacketsSent, 0);
                foreach (ulong speaker in ids.Where(s => s != listener.Manager.LocalClientId))
                {
                    NetworkVoice remote = VoiceOf(listener, speaker);
                    Assert.IsTrue(remote.IsSpeaking, $"{listener.name} shows p{speaker} talking");
                    Assert.Greater(remote.LastLevel, 0.1f, "the tone's level travels with it");
                    Assert.Greater(remote.Playback.Buffer.Count, 0);
                }
            }
            Assert.AreEqual(0, NetworkVoice.All.Sum(v => v.PacketsRejected), "nothing was refused");
        }

        [UnityTest]
        public IEnumerator PushToTalkSendsNothingUntilTheKeyIsHeld()
        {
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            yield return net.StartSession(clients: 1);
            yield return WaitSeconds(0.5f);
            Assert.AreEqual(0, NetworkVoice.All.Sum(v => v.PacketsSent), "no key held, nothing leaves the machine");
            Assert.IsTrue(NetworkVoice.All.All(v => !v.IsSpeaking));
        }

        [UnityTest]
        public IEnumerator MutingTheMicStopsSending()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            ulong speaker = client.Manager.LocalClientId;
            yield return WaitFor(() => VoiceOf(net.Host, speaker).PacketsReceived > 5, "the host to hear the client", 5f);
            VoiceSettings.MicMuted = true;
            yield return WaitSeconds(0.3f);
            int before = VoiceOf(net.Host, speaker).PacketsReceived;
            yield return WaitSeconds(0.5f);
            Assert.AreEqual(before, VoiceOf(net.Host, speaker).PacketsReceived, "muted: nothing more arrives");
            Assert.IsFalse(VoiceOf(net.Host, speaker).IsSpeaking);
        }

        // Sends half a second of tone from a machine's own player, marked as radio or not.
        private static IEnumerator Talk(NetworkBootstrap speaker, bool radio, float seconds = 0.5f, float level = 0.2f)
        {
            NetworkVoice voice = VoiceOf(speaker, speaker.Manager.LocalClientId);
            var tone = new ToneVoiceCapture { Recording = true };
            var packet = new byte[640];
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                int n = tone.ReadPacket(packet);
                if (n > 0) voice.Send(packet, n, tone.Codec, level, radio);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator TheRadioReachesEveryRadioHolderAndStillSpeaksInPerson()
        {
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            yield return net.StartSession(clients: 2);
            NetworkBootstrap speaker = net.Clients.First();
            ulong id = speaker.Manager.LocalClientId;
            yield return Talk(speaker, radio: true);
            yield return WaitSeconds(0.2f);
            foreach (NetworkBootstrap listener in net.Machines.Where(m => m != speaker))
            {
                NetworkVoice copy = VoiceOf(listener, id);
                Assert.Greater(copy.Radio.SamplesReceived, 4000, $"{listener.name} hears it on the radio");
                Assert.Greater(copy.Playback.SamplesReceived, 4000, $"{listener.name} also hears it in person (falloff applies)");
                Assert.IsTrue(copy.LastWasRadio);
            }
        }

        [UnityTest]
        public IEnumerator NoRadioNoTransmissionAndNoReception()
        {
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            yield return net.StartSession(clients: 2);
            NetworkBootstrap speaker = net.Clients.First(), deaf = net.Clients.Last();
            ulong speakerId = speaker.Manager.LocalClientId, deafId = deaf.Manager.LocalClientId;

            NetworkVoice.RadioHolder = client => client != deafId;
            yield return Talk(speaker, radio: true);
            yield return WaitSeconds(0.2f);
            Assert.Greater(VoiceOf(net.Host, speakerId).Radio.SamplesReceived, 0, "the host has a radio");
            Assert.AreEqual(0, VoiceOf(deaf, speakerId).Radio.SamplesReceived, "no radio in hand, nothing from it");

            NetworkVoice.RadioHolder = client => client != speakerId;
            int before = VoiceOf(net.Host, speakerId).Radio.SamplesReceived;
            yield return Talk(speaker, radio: true);
            yield return WaitSeconds(0.2f);
            Assert.AreEqual(before, VoiceOf(net.Host, speakerId).Radio.SamplesReceived, "the host strips the radio flag from someone without one");
            Assert.IsFalse(VoiceOf(net.Host, speakerId).LastWasRadio);
        }

        [UnityTest]
        public IEnumerator AWallBetweenUsCountsAsOcclusion()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap speaker = net.Clients.First();
            VoicePlayback playback = VoiceOf(net.Host, speaker.Manager.LocalClientId).Playback;
            Vector3 mouth = playback.transform.position;
            Vector3 ear = mouth + new Vector3(6f, 0f, 0f);
            Assert.AreEqual(0, playback.CountWalls(ear), "open line of sight");

            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            net.Track(wall);
            wall.transform.position = mouth + new Vector3(3f, 0f, 0f);
            wall.transform.localScale = new Vector3(0.2f, 4f, 4f);
            yield return new WaitForFixedUpdate();
            Assert.AreEqual(1, playback.CountWalls(ear), "one wall");
        }

        [UnityTest]
        public IEnumerator TheHostTurnsVoicesIntoNoiseAtTheSpeaker()
        {
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            yield return net.StartSession(clients: 1);
            NetworkBootstrap speaker = net.Clients.First();
            NetworkVoice hostCopy = VoiceOf(net.Host, speaker.Manager.LocalClientId);
            VoiceConfig c = hostCopy.Config;

            // The noise history is shared by every test in the run; only look at what this test made.
            float since = Time.time;
            System.Collections.Generic.List<Abandoned.Core.NoiseEvent> Voices() => Abandoned.Core.NoiseSystem.RecentEvents
                .Where(e => e.Source == Abandoned.Core.NoiseSource.Voice && e.Time >= since).ToList();

            yield return Talk(speaker, radio: false, seconds: 0.6f, level: c.WhisperLevel * 0.5f);
            yield return WaitSeconds(0.1f);
            Assert.IsEmpty(Voices(), "a whisper makes no noise");

            since = Time.time;
            yield return Talk(speaker, radio: false, seconds: 0.6f, level: c.ShoutLevel);
            yield return WaitSeconds(0.1f);
            var shouts = Voices();
            Assert.IsNotEmpty(shouts, "a shout is heard");
            Abandoned.Core.NoiseEvent shout = shouts[shouts.Count - 1];
            Assert.AreEqual(c.ShoutLoudness, shout.Loudness, 0.01f);
            Assert.Less(Vector3.Distance(shout.Position, hostCopy.Playback.transform.position), 0.5f, "from the speaker's mouth");
            Assert.LessOrEqual(shouts.Count, Mathf.CeilToInt(0.7f / c.NoiseInterval) + 1, "rate-limited, not one per packet");
        }
    }
}
