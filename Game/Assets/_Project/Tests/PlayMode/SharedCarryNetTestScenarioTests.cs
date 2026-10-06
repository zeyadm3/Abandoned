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
    /// The multi-process 'sharedcarry' scenario run in one process with a host and three clients, so a
    /// failing Tools/nettest.sh sharedcarry points at the build/launch side rather than the scenario or carry code.
    /// </summary>
    public class SharedCarryNetTestScenarioTests
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
        public IEnumerator SharedCarryScenarioPassesWithHostAndThreeClients()
        {
            yield return net.StartSession(3);
            Assert.IsTrue(NetTestScenarios.TryCreate("sharedcarry", out INetTestScenario scenario));
            NetTestContext host = ContextFor(net.Host, 3);
            List<NetTestContext> clients = net.Clients.Select(c => ContextFor(c, 3)).ToList();

            int finished = 0;
            runner.StartCoroutine(Run(scenario.RunHost(host), () => finished++));
            foreach (NetTestContext c in clients) runner.StartCoroutine(Run(scenario.RunClient(c), () => finished++));
            MachineSeparator.Create(net);
            yield return WaitFor(() => finished == 4, "all four machines to finish the sharedcarry scenario", 60f);

            foreach (NetTestContext ctx in clients.Prepend(host))
                Assert.IsEmpty(ctx.Result.errors, $"machine {ctx.Manager.LocalClientId}: {string.Join("; ", ctx.Result.errors)}");
            Assert.AreEqual(2, host.Result.sharedActions.Count, "two carriers");
            Assert.IsTrue(host.Result.sharedActions.All(a => a.grabbed && a.sawLifted && a.letGo));
            NetTestSharedCarryHostView carry = host.Result.sharedHost;
            Assert.AreEqual(2, carry.maxCarriers);
            Assert.Greater(carry.maxLift, SharedCarryNetTestScenario.MinLift);
            Vector3 moved = carry.end - carry.start;
            Assert.Greater(new Vector2(moved.x, moved.z).magnitude, SharedCarryNetTestScenario.MinMove, "carried forward");
            Assert.AreEqual(4, host.Result.lootViews.Count, "a view from every machine");
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
