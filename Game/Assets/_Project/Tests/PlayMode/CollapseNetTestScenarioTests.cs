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
    /// The multi-process 'collapse' scenario run in one process with a host and three clients, each with
    /// its own copy of a TestBuilding-like rotten tile (same name, place and weakness), so a failing
    /// Tools/nettest.sh collapse points at the build/launch side rather than the scenario or sync code.
    /// </summary>
    public class CollapseNetTestScenarioTests
    {
        private NetTestHarness net;
        private NetStructureKit kit;
        private TestCoroutineHost runner;
        private readonly List<NetTestChannel> channels = new();

        private static readonly NetStructureKit.Tile[] Building =
        {
            new(CollapseNetTestScenario.TargetName, new Vector3(14f, 4f, 14f), health: 0.7f, capacity: 0.08f),
            new("Tile_U_2_3", new Vector3(10f, 4f, 14f)),
        };

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
            kit?.Destroy();
            net.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator CollapseScenarioPassesWithHostAndThreeClients()
        {
            yield return net.StartSession(3);
            kit = NetStructureKit.Build(net, Building);
            NetStructureSeparator.Create(net, kit);
            yield return kit.WaitForSync();
            Assert.IsTrue(NetTestScenarios.TryCreate("collapse", out INetTestScenario scenario));
            NetTestContext host = ContextFor(net.Host, 3);
            List<NetTestContext> clients = net.Clients.Select(c => ContextFor(c, 3)).ToList();

            int finished = 0;
            runner.StartCoroutine(Run(scenario.RunHost(host), () => finished++));
            foreach (NetTestContext c in clients) runner.StartCoroutine(Run(scenario.RunClient(c), () => finished++));
            yield return WaitFor(() => finished == 4, "all four machines to finish the collapse scenario", 60f);

            foreach (NetTestContext ctx in clients.Prepend(host))
                Assert.IsEmpty(ctx.Result.errors, $"machine {ctx.Manager.LocalClientId}: {string.Join("; ", ctx.Result.errors)}");
            Assert.AreEqual(4, host.Result.collapseViews.Count, "a view from every machine");
            int target = host.Result.collapseTarget;
            Assert.AreEqual(kit.Section(net.Host, CollapseNetTestScenario.TargetName).Id, target);
            int seed = host.Result.collapseViews.Single(v => v.observer == 0).collapsed.Single(c => c.id == target).seed;
            foreach (NetTestCollapseView v in host.Result.collapseViews.Where(v => v.observer != 0))
            {
                Assert.AreEqual(seed, v.collapsed.Single(c => c.id == target).seed, $"client {v.observer} seed");
                Assert.Greater(v.Fall, CollapseNetTestScenario.MinFall, $"client {v.observer} fell");
            }
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
