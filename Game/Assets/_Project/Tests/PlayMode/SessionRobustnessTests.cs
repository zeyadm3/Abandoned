using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>Wrong builds are turned away with a reason; a host leaving tells every client why.</summary>
    public class SessionRobustnessTests
    {
        private NetTestHarness net;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator AnotherBuildIsTurnedAwayNamingBothVersions()
        {
            NetworkBootstrap host = net.AddMachine("Host");
            host.CompatibilityKey = "0.3.0+abc1234";
            net.StartHost(host);
            NetworkBootstrap stranger = net.AddMachine("Stranger");
            stranger.CompatibilityKey = "0.3.0+fffffff";
            Assert.IsTrue(stranger.StartClient("127.0.0.1", net.Port));
            yield return WaitFor(() => !stranger.IsRunning && !stranger.Manager.ShutdownInProgress, "the other build to be refused");
            StringAssert.Contains("0.3.0 (abc1234)", stranger.LastError);
            StringAssert.Contains("0.3.0 (fffffff)", stranger.LastError);
            Assert.AreEqual(1, host.Slots.Count, "a refused build takes no spawn point");

            NetworkBootstrap same = net.AddMachine("Same");
            same.CompatibilityKey = "0.3.0+abc1234";
            Assert.IsTrue(same.StartClient("127.0.0.1", net.Port));
            yield return WaitFor(() => same.Manager.IsConnectedClient, "the same build to get in");
        }

        [UnityTest]
        public IEnumerator TheHostLeavingTellsEveryClientWhy()
        {
            yield return net.StartSession(clients: 2);
            var ended = new Dictionary<NetworkBootstrap, (bool onPurpose, string reason)>();
            foreach (NetworkBootstrap m in net.Machines)
            {
                NetworkBootstrap machine = m;
                machine.SessionEnded += (onPurpose, reason) => ended[machine] = (onPurpose, reason);
            }

            net.Host.Disconnect();
            yield return WaitFor(() => net.Machines.All(m => !m.IsRunning && !m.Manager.ShutdownInProgress), "everyone to be out of the game");
            foreach (NetworkBootstrap client in net.Clients)
            {
                Assert.AreEqual(SessionMessages.HostLeft, client.LastError, $"{client.name} is told the host left");
                Assert.IsTrue(ended.ContainsKey(client), $"{client.name} reports its game ended");
                Assert.AreEqual((false, SessionMessages.HostLeft), ended[client]);
            }
            Assert.AreEqual((true, string.Empty), ended[net.Host], "the host left on purpose: no message for them");
        }

        [UnityTest]
        public IEnumerator AClientLeavingOnPurposeGetsNoMessage()
        {
            yield return net.StartSession(clients: 1);
            NetworkBootstrap client = net.Clients.First();
            (bool onPurpose, string reason)? ended = null;
            client.SessionEnded += (onPurpose, reason) => ended = (onPurpose, reason);
            client.Disconnect();
            yield return WaitFor(() => ended.HasValue, "the client's game to end");
            Assert.AreEqual((true, string.Empty), ended.Value);
            Assert.IsTrue(net.Host.IsRunning, "the host carries on");
        }
    }
}
