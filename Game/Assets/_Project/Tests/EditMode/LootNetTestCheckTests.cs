using System.Collections.Generic;
using System.Linq;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;

namespace Abandoned.Tests
{
    /// <summary>The 'loot' nettest verdict catches each way machines can disagree about loot.</summary>
    public class LootNetTestCheckTests
    {
        private const int Players = 3, Clients = 2;
        private const float Tolerance = 0.25f, MinMove = 0.75f;
        private static readonly string Free = LootHoldState.Free.ToString();

        private static NetTestLootSnapshot Item(ulong id, Vector3 at, int current = 900, int full = 1200) =>
            new() { id = id, item = "laptop", fullValue = full, currentValue = current, position = at, owner = 0, hold = Free };

        private static List<NetTestLootView> Agreeing() => Enumerable.Range(0, Players).Select(o => new NetTestLootView
        {
            observer = (ulong)o,
            items = { Item(10, new Vector3(2f, 0f, 3f)), Item(11, new Vector3(4f, 0f, 3f)), Item(5, new Vector3(9f, 0f, 9f), 400, 400) },
        }).ToList();

        private static List<NetTestLootAction> Actions() => new()
        {
            new NetTestLootAction { client = 1, item = 10, gotOwnership = true, returnedOwnership = true, start = new Vector3(2f, 0f, 1f) },
            new NetTestLootAction { client = 2, item = 11, gotOwnership = true, returnedOwnership = true, start = new Vector3(4f, 0f, 1f) },
        };

        private static List<string> Verify(List<NetTestLootView> views, List<NetTestLootAction> actions) =>
            LootNetTestCheck.Verify(views, actions, Players, Clients, Tolerance, MinMove);

        [Test]
        public void AgreeingMachinesPass() => Assert.IsEmpty(Verify(Agreeing(), Actions()));

        [Test]
        public void DifferentValueOnAClientFails()
        {
            List<NetTestLootView> views = Agreeing();
            views[2].items[0].currentValue = 1200;
            StringAssert.Contains("machine 2 sees #10 laptop at $1200", Verify(views, Actions()).Single());
        }

        [Test]
        public void ClientCopyFarFromTheHostsFails()
        {
            List<NetTestLootView> views = Agreeing();
            views[1].items[1].position += Vector3.right * 0.5f;
            StringAssert.Contains("0.50 m from the host's", Verify(views, Actions()).Single());
        }

        [Test]
        public void MissingOrExtraCopiesFail()
        {
            List<NetTestLootView> views = Agreeing();
            views[1].items.RemoveAt(2);
            views[2].items.Add(Item(99, Vector3.zero));
            List<string> errors = Verify(views, Actions());
            Assert.IsTrue(errors.Any(e => e.Contains("machine 1 is missing item #5")));
            Assert.IsTrue(errors.Any(e => e.Contains("machine 2 has item #99")));
        }

        [Test]
        public void ItemStillHeldOrOwnedByAClientFails()
        {
            List<NetTestLootView> views = Agreeing();
            foreach (NetTestLootView v in views) { v.items[0].owner = 1; v.items[0].hold = "held by #3"; }
            StringAssert.Contains("should be free and host-simulated", Verify(views, Actions()).Single());
        }

        [Test]
        public void OwnershipThatNeverMovedOrItemThatDidntMoveOrWasntDamagedFails()
        {
            List<NetTestLootAction> actions = Actions();
            actions[0].gotOwnership = false;
            actions[1].returnedOwnership = false;
            List<NetTestLootView> views = Agreeing();
            foreach (NetTestLootView v in views) { v.items[0].position = actions[0].start; v.items[1].currentValue = 1200; }
            List<string> errors = Verify(views, actions);
            Assert.IsTrue(errors.Any(e => e.Contains("never got item #10's physics")));
            Assert.IsTrue(errors.Any(e => e.Contains("didn't go back to the host")));
            Assert.IsTrue(errors.Any(e => e.Contains("moved only 0.00 m")));
            Assert.IsTrue(errors.Any(e => e.Contains("shows no damage")));
        }

        [Test]
        public void MissingHostViewOrClientActionFails()
        {
            List<NetTestLootView> views = Agreeing();
            views.RemoveAt(0);
            StringAssert.Contains("no loot view from the host", string.Join("; ", Verify(views, Actions())));
            List<NetTestLootAction> actions = Actions();
            actions.RemoveAt(1);
            StringAssert.Contains("from each of 2 clients, got 1", string.Join("; ", Verify(Agreeing(), actions)));
        }

        [Test]
        public void ScenarioIsRegistered()
        {
            Assert.IsTrue(NetTestScenarios.TryCreate("loot", out INetTestScenario s));
            Assert.AreEqual(LootNetTestScenario.ScenarioName, s.Name);
        }
    }
}
