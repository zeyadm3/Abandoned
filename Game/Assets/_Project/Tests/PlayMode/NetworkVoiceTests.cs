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
    }
}
