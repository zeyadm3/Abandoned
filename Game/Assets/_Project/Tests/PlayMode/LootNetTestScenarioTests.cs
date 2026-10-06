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
    /// The multi-process 'loot' scenario run in one process with a host and three clients, so a failing
    /// Tools/nettest.sh loot points at the build/launch side rather than the scenario or loot code.
    /// </summary>
    public class LootNetTestScenarioTests
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

        [UnityTest]
        public IEnumerator LootScenarioPassesWithHostAndThreeClients()
        {
            yield return net.StartSession(3);
            Assert.IsTrue(NetTestScenarios.TryCreate("loot", out INetTestScenario scenario));
            NetTestContext host = ContextFor(net.Host, 3);
            List<NetTestContext> clients = net.Clients.Select(c => ContextFor(c, 3)).ToList();

            int finished = 0;
            runner.StartCoroutine(Run(scenario.RunHost(host), () => finished++));
            foreach (NetTestContext c in clients) runner.StartCoroutine(Run(scenario.RunClient(c), () => finished++));
            // Items appear mid-scenario; real machines don't share physics, so keep the copies apart.
            MachineSeparator.Create(net);
            yield return WaitFor(() => finished == 4, "all four machines to finish the loot scenario", 60f);

            foreach (NetTestContext ctx in clients.Prepend(host))
                Assert.IsEmpty(ctx.Result.errors, $"machine {ctx.Manager.LocalClientId}: {string.Join("; ", ctx.Result.errors)}");
            Assert.AreEqual(3, host.Result.lootActions.Count, "every client picked up and threw");
            Assert.IsTrue(host.Result.lootActions.All(a => a.gotOwnership && a.returnedOwnership));
            Assert.AreEqual(4, host.Result.lootViews.Count, "a loot view from every machine");
            NetTestLootView hostView = host.Result.lootViews.Single(v => v.observer == 0);
            Assert.AreEqual(3, hostView.items.Count, "the three thrown laptops");
            Assert.IsTrue(hostView.items.All(i => i.currentValue < i.fullValue), "the host's damage is on every laptop");
        }

        private NetTestContext ContextFor(NetworkBootstrap machine, int expectedClients)
        {
            var channel = new NetTestChannel(machine.Manager);
            channel.Open();
            channels.Add(channel);
            return new NetTestContext(machine, channel, new NetTestResult(), expectedClients);
        }

        private static IEnumerator Run(IEnumerator body, System.Action done)
        {
            yield return body;
            done();
        }
    }
}
