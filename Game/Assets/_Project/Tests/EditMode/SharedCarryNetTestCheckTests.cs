using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The 'sharedcarry' nettest verdict catches each way a shared carry can go wrong across machines.</summary>
    public class SharedCarryNetTestCheckTests
    {
        private const int Players = 4, Carriers = 2;
        private const float Tolerance = 0.25f, MinMove = 0.8f, MinLift = 0.15f;
        private static readonly Vector3 End = new(8f, 1.05f, -3.6f);

        private static List<NetTestLootView> Views() => Enumerable.Range(0, Players).Select(o => new NetTestLootView
        {
            observer = (ulong)o,
            items = { new NetTestLootSnapshot { id = 20, item = "server_rack", position = End, owner = 0 } },
        }).ToList();

        private static List<NetTestSharedCarryAction> Actions() => new()
        {
            new NetTestSharedCarryAction { client = 1, item = 20, grabbed = true, sawLifted = true, letGo = true },
            new NetTestSharedCarryAction { client = 3, item = 20, grabbed = true, sawLifted = true, letGo = true },
        };

        private static NetTestSharedCarryHostView Host() => new()
        {
            item = 20, maxCarriers = 2, maxLift = 0.3f, targetsReceived = 80, endCarriers = 0,
            start = new Vector3(8f, 1.05f, -4.8f), end = End,
        };

        private static List<string> Verify(List<NetTestLootView> views, List<NetTestSharedCarryAction> actions, NetTestSharedCarryHostView host) =>
            SharedCarryNetTestCheck.Verify(views, actions, host, Players, Carriers, Tolerance, MinMove, MinLift);

        [Test]
        public void AGoodCarryPasses() => Assert.IsEmpty(Verify(Views(), Actions(), Host()));

        [Test]
        public void AClientThatNeverSawItLiftedFails()
        {
            List<NetTestSharedCarryAction> actions = Actions();
            actions[1].sawLifted = false;
            StringAssert.Contains("client 3 never saw #20 lifted", Verify(Views(), actions, Host()).Single());
        }

        [Test]
        public void NotLiftedOrNotMovedOnTheHostFails()
        {
            NetTestSharedCarryHostView host = Host();
            host.maxLift = 0.02f;
            host.end = host.start + Vector3.forward * 0.3f;
            List<NetTestLootView> views = Views();
            foreach (NetTestLootView v in views) v.items[0].position = host.end;
            List<string> errors = Verify(views, Actions(), host);
            Assert.IsTrue(errors.Any(e => e.Contains("rose only 0.02 m")), string.Join("\n", errors));
            Assert.IsTrue(errors.Any(e => e.Contains("moved only 0.30 m")), string.Join("\n", errors));
        }

        [Test]
        public void NoStreamedTargetsOrAStuckCarrierFails()
        {
            NetTestSharedCarryHostView host = Host();
            host.targetsReceived = 0;
            host.endCarriers = 1;
            host.maxCarriers = 1;
            List<string> errors = Verify(Views(), Actions(), host);
            Assert.AreEqual(3, errors.Count, string.Join("\n", errors));
        }

        [Test]
        public void AMachineDisagreeingOnWhereItIsFails()
        {
            List<NetTestLootView> views = Views();
            views[2].items[0].position += Vector3.right * 0.6f;
            views[3].items.Clear();
            List<string> errors = Verify(views, Actions(), Host());
            Assert.IsTrue(errors.Any(e => e.Contains("machine 2 sees #20") && e.Contains("0.60 m")), string.Join("\n", errors));
            Assert.IsTrue(errors.Any(e => e.Contains("machine 3 is missing #20")), string.Join("\n", errors));
        }

        [Test]
        public void HandingTheShareCarryToAClientFails()
        {
            List<NetTestLootView> views = Views();
            foreach (NetTestLootView v in views) v.items[0].owner = 1;
            StringAssert.Contains("stay host-simulated", Verify(views, Actions(), Host()).Single());
        }

        [Test]
        public void TheScenarioIsRegistered() => Assert.IsTrue(NetTestScenarios.TryCreate("sharedcarry", out _));
    }
}
