using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Networking;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Host-owned value over in-process NGO: rolled value and damage replicate, feedback (-$X, shatter)
    /// plays on every machine, a shatter despawns everywhere, pockets hide items on all machines, and a
    /// carrier's reported impact is applied once on the host.
    /// </summary>
    public class NetworkLootValueTests : NetworkLootTestBase
    {
        [UnityTest]
        public IEnumerator HostRolledValueAndDamageReplicateWithFeedbackOnEveryMachine()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            LootItem host = NetLootKit.CopyOn(net.Host, id).Item;
            Assert.Greater(host.FullValue, 0, "the host rolled a value");
            foreach (NetworkLoot copy in NetLootKit.Copies(net, id))
            {
                Assert.AreEqual(host.FullValue, copy.Item.FullValue, $"{copy.NetworkManager.name}: full value");
                Assert.AreEqual(host.CurrentValue, copy.Item.CurrentValue, $"{copy.NetworkManager.name}: current value");
                Assert.AreEqual(host.Condition, copy.Item.Condition, 1e-5f, $"{copy.NetworkManager.name}: condition");
                Assert.AreEqual(copy.IsServer, copy.Item.HasValueAuthority, "only the host owns value");
            }

            var losses = new Dictionary<NetworkBootstrap, List<int>>();
            foreach (NetworkBootstrap m in net.Machines)
            {
                var list = losses[m] = new List<int>();
                NetLootKit.CopyOn(m, id).Item.Damaged += (_, loss, _) => list.Add(loss);
            }
            host.ApplyImpact(9f, host.transform.position);
            int lost = host.FullValue - host.CurrentValue;
            Assert.Greater(lost, 0);

            yield return WaitFor(() => NetLootKit.Copies(net, id).All(c => c.Item.CurrentValue == host.CurrentValue),
                "every machine to see the damaged value", 5f);
            yield return WaitFor(() => losses.Values.All(l => l.Count > 0), "the -$X feedback on every machine", 5f);
            foreach (var pair in losses)
                CollectionAssert.AreEqual(new[] { lost }, pair.Value, $"{pair.Key.name}: one -${lost} popup");

            // A client's own copy never applies damage itself.
            LootItem onClient = NetLootKit.CopyOn(client1, id).Item;
            int before = host.CurrentValue;
            onClient.ApplyImpact(30f, onClient.transform.position);
            yield return WaitSeconds(0.3f);
            Assert.AreEqual(before, host.CurrentValue);
            Assert.AreEqual(before, onClient.CurrentValue);
        }

        [UnityTest]
        public IEnumerator ShatterWhileAClientCarriesItDespawnsItEverywhereAndShowsOnEveryMachine()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "antique_vase", 1f, i => id = i);
            yield return ClientPicksUp(client1, id);
            var shatteredOn = new List<string>();
            List<NetworkLoot> copies = NetLootKit.Copies(net, id).ToList();
            foreach (NetworkLoot copy in copies)
            {
                string who = copy.NetworkManager.name;
                copy.Item.Shattered += (_, _) => shatteredOn.Add(who);
            }

            LootItem host = NetLootKit.CopyOn(net.Host, id).Item;
            host.ApplyImpact(5f, host.transform.position);
            yield return WaitFor(() => copies.All(c => c == null), "every copy to be despawned and destroyed", 5f);

            CollectionAssert.AreEquivalent(net.Machines.Select(m => m.Manager.name), shatteredOn, "shards and '$X -> $0' on every machine");
            foreach (NetworkBootstrap m in net.Machines)
            {
                Assert.IsNull(NetLootKit.CopyOn(m, id), $"{m.name} no longer has it spawned");
                Assert.IsNull(CarrierOf(m, client1).Held, $"{m.name}: the carrier's hands are empty");
            }
        }

        [UnityTest]
        public IEnumerator PocketedItemIsHiddenOnEveryMachineAndComesBackOut()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "gold_watch", 1f, i => id = i);
            float weight = NetLootKit.CopyOn(net.Host, id).Item.GameplayWeight;

            Handler.RequestPickup(Own(client1), NetLootKit.CopyOn(client1, id).Grabbable);
            yield return NetLootKit.WaitForHold(net, id, HeldBy(client1, LootHoldMode.Pocketed), "every machine to see it pocketed");
            foreach (NetworkBootstrap m in net.Machines)
            {
                NetworkLoot copy = NetLootKit.CopyOn(m, id);
                Assert.IsTrue(copy.Grabbable.IsPocketed, $"{m.name}: pocketed");
                Assert.IsTrue(copy.GetComponentsInChildren<Renderer>().All(r => !r.enabled), $"{m.name}: hidden");
                Assert.IsFalse(copy.Grabbable.Body.detectCollisions, $"{m.name}: no collisions");
                Assert.AreEqual(NetworkManager.ServerClientId, copy.OwnerClientId, "pockets have no physics; the host keeps it");
                PlayerCarrier carrier = CarrierOf(m, client1);
                CollectionAssert.AreEqual(new[] { copy.Grabbable }, carrier.Inventory.Items, $"{m.name}: in client 1's pockets");
                Assert.AreEqual(weight, carrier.CarriedWeight, 0.001f, $"{m.name}: pocket weight counts");
            }

            Handler.RequestDropFromPocket(Own(client1));
            yield return NetLootKit.WaitForHold(net, id, LootHoldState.Free, "every machine to see it taken out");
            yield return WaitSeconds(0.3f);
            Vector3 eye = CarrierOf(net.Host, client1).EyePosition;
            foreach (NetworkBootstrap m in net.Machines)
            {
                NetworkLoot copy = NetLootKit.CopyOn(m, id);
                Assert.IsFalse(copy.Grabbable.IsPocketed, $"{m.name}: out of the pocket");
                Assert.IsTrue(copy.GetComponentsInChildren<Renderer>().All(r => r.enabled), $"{m.name}: visible again");
                Assert.IsEmpty(CarrierOf(m, client1).Inventory.Items, $"{m.name}: pockets empty");
                Assert.Less(Vector3.Distance(copy.transform.position, eye), 2f, $"{m.name}: reappears in front of client 1");
            }
        }

        [UnityTest]
        public IEnumerator CarrierReportedImpactIsAppliedOnceOnTheHost()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            yield return ClientPicksUp(client1, id);
            NetworkLoot carried = NetLootKit.CopyOn(client1, id), bystander = NetLootKit.CopyOn(client2, id);
            LootItem host = NetLootKit.CopyOn(net.Host, id).Item;
            yield return WaitSeconds(0.5f);
            int full = host.FullValue, before = host.CurrentValue;
            const float speed = 9f;
            int expected = LootMath.ImpactLoss(host.DamageConfig.Profile(host.Definition.Fragility), full, speed, out _);
            Assert.Greater(expected, 0);
            int impactNoises = 0;
            System.Action<NoiseEvent> onNoise = e => { if (e.Source == NoiseSource.LootImpact) impactNoises++; };
            NoiseSystem.Emitted += onNoise;

            // Same hit reported twice in one frame (a physics step with two contacts), plus a
            // non-carrier's and an implausibly placed report: only one may count.
            carried.ReportImpact(speed, carried.transform.position);
            carried.ReportImpact(speed, carried.transform.position);
            bystander.ReportImpact(speed, bystander.transform.position);
            carried.ReportImpact(speed, carried.transform.position + Vector3.up * 50f);
            yield return WaitFor(() => carried.Item.CurrentValue != before, "the damage to come back to the carrier", 5f);
            yield return WaitSeconds(0.4f);

            Assert.AreEqual(before - expected, host.CurrentValue, "applied exactly once on the host");
            Assert.IsTrue(NetLootKit.Copies(net, id).All(c => c.Item.CurrentValue == host.CurrentValue), "everyone agrees");
            NoiseSystem.Emitted -= onNoise;
            Assert.AreEqual(1, impactNoises, "threats (on the host) heard the one accepted hit, once");

            // Past the cooldown a new hit counts again: the filter isn't just blocking everything.
            carried.ReportImpact(speed, carried.transform.position);
            yield return WaitFor(() => host.CurrentValue < before - expected, "a later hit to count", 5f);
        }
    }
}
