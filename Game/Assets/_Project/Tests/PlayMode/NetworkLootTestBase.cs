using System.Collections;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Host + two clients in one process with networked loot. Client 1 stands at spawn slot 1, client 2
    /// at slot 2, both facing +Z; items are spawned by the host and every machine gets its own copy.
    /// </summary>
    public abstract class NetworkLootTestBase
    {
        protected NetTestHarness net;
        protected NetworkBootstrap client1, client2;

        protected IInteractionHandler Handler => InteractionService.Handler;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return CleanWorld();
            net = new NetTestHarness();
            net.BuildArena();
            ArrangeArena();
            yield return net.StartSession(clients: 2);
            client1 = net.Clients.ElementAt(0);
            client2 = net.Clients.ElementAt(1);
            MachineSeparator.Create(net);
            Assert.IsInstanceOf<NetworkInteractionHandler>(Handler, "a session routes interactions through the network handler");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            net.Destroy();
            yield return null;
        }

        /// <summary>Extra level geometry or moved spawn points, before anyone joins.</summary>
        protected virtual void ArrangeArena() { }

        protected static ulong Id(NetworkBootstrap machine) => machine.Manager.LocalClientId;

        /// <summary><paramref name="owner"/>'s player as <paramref name="machine"/> sees it.</summary>
        protected static PlayerCarrier CarrierOf(NetworkBootstrap machine, NetworkBootstrap owner) =>
            PlayerOf(machine, Id(owner)).Carrier;

        protected static PlayerCarrier Own(NetworkBootstrap machine) => OwnPlayer(machine).Carrier;

        protected static LootHoldState HeldBy(NetworkBootstrap owner, LootHoldMode mode = LootHoldMode.Held) =>
            LootHoldState.For(mode, OwnPlayer(owner).NetworkObjectId);

        /// <summary>Spawns an item <paramref name="ahead"/> m in front of a machine's own player and lets it land.</summary>
        protected IEnumerator SpawnInFront(NetworkBootstrap of, string lootId, float ahead, System.Action<ulong> spawned, float sideways = 0f)
        {
            Transform t = OwnPlayer(of).transform;
            Vector3 at = t.position + t.forward * ahead + t.right * sideways + Vector3.up * 0.3f;
            ulong id = 0;
            yield return NetLootKit.Spawn(net, lootId, at, i => id = i);
            yield return NetLootKit.Settle();
            spawned(id);
        }

        /// <summary>The real request path: the client's handler asks, the host validates and answers.</summary>
        protected IEnumerator ClientPicksUp(NetworkBootstrap client, ulong id)
        {
            Handler.RequestPickup(Own(client), NetLootKit.CopyOn(client, id).Grabbable);
            yield return NetLootKit.WaitForHold(net, id, HeldBy(client), $"every machine to see {client.name} holding #{id}");
            yield return WaitFor(() => NetLootKit.Copies(net, id).All(c => c.OwnerClientId == Id(client)),
                $"#{id}'s physics to move to {client.name}", 5f);
        }

        protected static void AssertSimulatedOnlyBy(NetTestHarness net, ulong id, NetworkBootstrap simulator)
        {
            foreach (NetworkBootstrap m in net.Machines)
            {
                NetworkLoot copy = NetLootKit.CopyOn(m, id);
                bool here = m == simulator;
                Assert.AreEqual(here, copy.Grabbable.HasPhysicsAuthority, $"{m.name}'s copy: simulates here?");
                Assert.AreEqual(!here, copy.Grabbable.Body.isKinematic, $"{m.name}'s copy: kinematic follower?");
            }
        }
    }
}
