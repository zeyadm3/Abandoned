using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Voice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>Death as a ghost (M6.7, GDD 11): watch living teammates, see threats, can't help (not heard, no noise).</summary>
    public class GhostTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            VoiceSettings.Mode = VoiceMode.PushToTalk;
            net = new NetTestHarness();
            net.BuildArena();
            yield return net.StartSession(clients: 1);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        private static NetworkVoice VoiceOf(NetworkBootstrap machine, ulong speaker) =>
            NetworkVoice.All.Single(v => v.NetworkManager == machine.Manager && v.OwnerClientId == speaker);

        private static IEnumerator Talk(NetworkBootstrap speaker, float seconds = 0.4f)
        {
            NetworkVoice voice = VoiceOf(speaker, speaker.Manager.LocalClientId);
            var tone = new ToneVoiceCapture { Recording = true };
            var packet = new byte[640];
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                int n = tone.ReadPacket(packet);
                if (n > 0) voice.Send(packet, n, tone.Codec, 0.3f, false);
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator TheDeadWatchTheLivingAndAreNotHeard()
        {
            NetworkBootstrap client = net.Clients.First();
            ulong clientId = client.Manager.LocalClientId;
            PlayerOf(net.Host, clientId).ServerKill();
            NetworkPlayer ghostPlayer = OwnPlayer(client);
            GhostSpectator ghost = ghostPlayer.GetComponent<GhostSpectator>();
            yield return WaitFor(() => ghost.ShowingDeathCard || ghost.Spectating, "the death card after the death camera", 5f);
            yield return WaitFor(() => ghost.Spectating, "a ghost after the death card", 6f);
            Assert.AreEqual(net.Host.Manager.LocalClientId, ghost.Following.OwnerClientId, "watching the living host");

            float since = Time.time;
            int heard = VoiceOf(net.Host, clientId).Playback.SamplesReceived;
            yield return Talk(client);
            yield return WaitSeconds(0.3f);
            Assert.AreEqual(heard, VoiceOf(net.Host, clientId).Playback.SamplesReceived, "the living don't hear ghosts");
            Assert.IsFalse(NoiseSystem.RecentEvents.Any(e => e.Source == NoiseSource.Voice && e.Time >= since), "ghosts make no noise");

            int ghostHears = VoiceOf(client, net.Host.Manager.LocalClientId).Playback.SamplesReceived;
            yield return Talk(net.Host);
            yield return WaitSeconds(0.3f);
            Assert.Greater(VoiceOf(client, net.Host.Manager.LocalClientId).Playback.SamplesReceived, ghostHears, "ghosts still hear the living");

            PlayerOf(net.Host, clientId).ServerRevive();
            yield return WaitFor(() => !ghost.Spectating, "back in the body when revived", 3f);
        }
    }
}
