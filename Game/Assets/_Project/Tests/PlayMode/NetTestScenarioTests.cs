using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// The multi-process nettest's scenario code, run in one process over loopback with four
    /// in-process machines: the channel delivers both ways and 'basic' passes end to end, so a
    /// failing Tools/nettest.sh points at the build/launch side rather than the scenario itself.
    /// </summary>
    public class NetTestScenarioTests
    {
        private NetTestHarness net;
        private TestCoroutineHost runner;
        private readonly List<NetTestChannel> channels = new();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
            runner = TestCoroutineHost.Create();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (NetTestChannel c in channels) c.Close();
            channels.Clear();
            if (runner != null) Object.Destroy(runner.gameObject);
            net.Destroy();
            yield return null;
        }

        private NetTestContext ContextFor(NetworkBootstrap machine, int expectedClients)
        {
            var channel = new NetTestChannel(machine.Manager);
            channel.Open();
            channels.Add(channel);
            return new NetTestContext(machine, channel, new NetTestResult(), expectedClients);
        }

        [UnityTest]
        public IEnumerator ChannelDeliversMessagesBetweenHostAndClients()
        {
            yield return net.StartSession(2);
            NetTestContext host = ContextFor(net.Host, 2);
            List<NetTestContext> clients = net.Clients.Select(c => ContextFor(c, 2)).ToList();

            host.Channel.SendToClients("ping", "hello");
            foreach (NetTestContext c in clients)
            {
                NetTestMessage got = default;
                yield return c.Receive("ping", 5f, m => got = m);
                Assert.IsFalse(c.Aborted, string.Join("; ", c.Result.errors));
                Assert.AreEqual("hello", got.Payload);
                Assert.AreEqual(Unity.Netcode.NetworkManager.ServerClientId, got.Sender);
                c.Channel.SendToHost("pong", c.Manager.LocalClientId.ToString());
            }

            var pongs = new List<NetTestMessage>();
            yield return host.Collect("pong", 2, pongs, 5f);
            Assert.IsFalse(host.Aborted, string.Join("; ", host.Result.errors));
            CollectionAssert.AreEquivalent(clients.Select(c => c.Manager.LocalClientId), pongs.Select(p => p.Sender));
            CollectionAssert.AreEquivalent(pongs.Select(p => p.Sender.ToString()), pongs.Select(p => p.Payload),
                "each payload came from the client that sent it");
        }

        [UnityTest]
        public IEnumerator BasicScenarioPassesWithHostAndThreeClients()
        {
            yield return net.StartSession(3);
            Assert.IsTrue(NetTestScenarios.TryCreate("basic", out INetTestScenario scenario));
            NetTestContext host = ContextFor(net.Host, 3);
            List<NetTestContext> clients = net.Clients.Select(c => ContextFor(c, 3)).ToList();

            int finished = 0;
            runner.StartCoroutine(Run(scenario.RunHost(host), () => finished++));
            foreach (NetTestContext c in clients) runner.StartCoroutine(Run(scenario.RunClient(c), () => finished++));
            yield return WaitFor(() => finished == 4, "all four machines to finish the scenario", 40f);

            foreach (NetTestContext ctx in clients.Prepend(host))
                Assert.IsEmpty(ctx.Result.errors, $"machine {ctx.Manager.LocalClientId}: {string.Join("; ", ctx.Result.errors)}");

            Assert.AreEqual(4, host.Result.views.Count, "host gathered a view from every machine");
            Assert.IsTrue(host.Result.views.All(v => v.players.Count == 4), "every machine saw all four players");
            Assert.AreEqual(3, host.Result.moves.Count, "every client moved");
            foreach (NetTestMove move in host.Result.moves)
            {
                Assert.GreaterOrEqual(move.HorizontalDistance, BasicNetTestScenario.MinDistance, $"player {move.owner} walked");
                // The host's own copy of a mover must be where the mover stopped, not at its spawn point.
                Vector3 onHost = PlayerOf(net.Host, move.owner).transform.position;
                Assert.Less(Vector3.Distance(onHost, move.end), BasicNetTestScenario.Tolerance,
                    $"host sees player {move.owner} where it stopped");
            }
        }

        [UnityTest]
        public IEnumerator WaitingPastTheTimeoutAbortsWithARecordedFailure()
        {
            yield return net.StartSession(0);
            NetTestContext host = ContextFor(net.Host, 0);
            float start = Time.realtimeSinceStartup;
            yield return host.WaitFor(() => false, "something that never happens", 0.3f);
            Assert.IsTrue(host.Aborted);
            Assert.Less(Time.realtimeSinceStartup - start, 2f, "gave up at the timeout, didn't hang");
            StringAssert.Contains("something that never happens", host.Result.errors.Single());
        }

        private static IEnumerator Run(IEnumerator body, System.Action done)
        {
            yield return body;
            done();
        }
    }
}
