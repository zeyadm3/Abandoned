using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Pickup, contention, drop, throw and a leaving carrier over in-process NGO: the host validates
    /// every request with its own view, a single carrier simulates the item, and the host takes the
    /// physics back on release.
    /// </summary>
    public class NetworkLootPickupTests : NetworkLootTestBase
    {
        private const float LaptopWeight = 2.5f;

        [UnityTest]
        public IEnumerator ClientPickupIsValidatedByTheHostAndHandsTheCarrierThePhysics()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            AssertSimulatedOnlyBy(net, id, net.Host);
            Assert.IsTrue(NetLootKit.Copies(net, id).All(c => c.OwnerClientId == NetworkManager.ServerClientId), "resting loot is host-owned");

            yield return ClientPicksUp(client1, id);
            AssertSimulatedOnlyBy(net, id, client1);
            foreach (NetworkBootstrap m in net.Machines)
            {
                PlayerCarrier carrier = CarrierOf(m, client1);
                Assert.AreSame(NetLootKit.CopyOn(m, id).Grabbable, carrier.Held, $"{m.name} sees client 1 holding it");
                Assert.AreEqual(LaptopWeight, carrier.CarriedWeight, 0.001f, $"{m.name}: client 1's carried weight (structure load, speed)");
            }

            // The carrier's spring lifts it to the hold point; everyone else follows the carrier's copy.
            yield return WaitSeconds(0.75f);
            Vector3 carried = NetLootKit.CopyOn(client1, id).transform.position;
            Assert.Greater(carried.y, 0.8f, "lifted to hand height on the carrier's machine");
            foreach (NetworkLoot copy in NetLootKit.Copies(net, id))
                Assert.Less(Vector3.Distance(copy.transform.position, carried), 0.3f, $"{copy.NetworkManager.name}'s copy follows the carrier");

            Vector3 eye = PlayerOf(net.Host, Id(client1)).transform.position;
            ScreenshotCapture.CaptureFrom(eye + new Vector3(2.2f, 1.6f, 2.2f), carried, "M3_4_client_carrying_seen_from_host");
        }

        [UnityTest]
        public IEnumerator HostRefusesAPickupThatIsOutOfReachOnItsOwnView()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 6f, i => id = i);
            NetworkLoot copy = NetLootKit.CopyOn(client1, id);

            // A modified client skipping its own checks: only the host's validation stands in the way.
            Assert.IsTrue(copy.ClientRequestPickup());
            yield return WaitFor(() => copy.LastHint != null, "the host's refusal", 5f);
            Assert.AreEqual("Too far away", copy.LastHint);
            Assert.AreEqual("Too far away", Own(client1).Hint, "shown to the player");
            yield return WaitSeconds(0.2f);
            Assert.IsTrue(NetLootKit.Copies(net, id).All(c => c.Hold.Mode == LootHoldMode.Free && c.OwnerClientId == NetworkManager.ServerClientId));
            Assert.IsNull(Own(client1).Held);
        }

        [UnityTest]
        public IEnumerator TwoClientsGrabbingAtOnceNeverBothGetIt()
        {
            ulong id = 0;
            // Between the two clients, in reach of both.
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i, sideways: 1f);
            NetworkLoot a = NetLootKit.CopyOn(client1, id), b = NetLootKit.CopyOn(client2, id);
            Assert.IsTrue(PickupRules.CanPickUp(Own(client1), a.Grabbable, out _) && PickupRules.CanPickUp(Own(client2), b.Grabbable, out _),
                "both clients may try");

            Assert.IsTrue(a.ClientRequestPickup());
            Assert.IsTrue(b.ClientRequestPickup());
            yield return WaitFor(() => NetLootKit.CopyOn(net.Host, id).Hold.Mode == LootHoldMode.Held, "the host to grant one", 5f);
            yield return WaitFor(() => a.LastHint != null || b.LastHint != null, "the other to be refused", 5f);
            yield return WaitSeconds(0.3f);

            NetworkLoot hostCopy = NetLootKit.CopyOn(net.Host, id);
            NetworkBootstrap winner = hostCopy.Hold.HolderObjectId == OwnPlayer(client1).NetworkObjectId ? client1 : client2;
            NetworkBootstrap loser = winner == client1 ? client2 : client1;
            Assert.AreEqual(Id(winner), hostCopy.OwnerClientId, "the winner simulates it");
            Assert.AreEqual("Someone else has it", NetLootKit.CopyOn(loser, id).LastHint);
            foreach (NetworkBootstrap m in net.Machines)
            {
                Assert.IsNotNull(CarrierOf(m, winner).Held, $"{m.name}: winner holds it");
                Assert.IsNull(CarrierOf(m, loser).Held, $"{m.name}: loser holds nothing");
            }

            // Trying again through the normal path is refused on the loser's own machine already.
            Handler.RequestPickup(Own(loser), NetLootKit.CopyOn(loser, id).Grabbable);
            Assert.AreEqual("Someone else has it", Own(loser).Hint);
        }

        [UnityTest]
        public IEnumerator DropReturnsThePhysicsToTheHostAndEveryoneAgreesWhereItLands()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            yield return ClientPicksUp(client1, id);
            yield return WaitSeconds(0.5f);

            Handler.RequestDrop(Own(client1));
            yield return NetLootKit.WaitForHold(net, id, LootHoldState.Free, "every machine to see it dropped");
            yield return WaitFor(() => NetLootKit.Copies(net, id).All(c => c.OwnerClientId == NetworkManager.ServerClientId),
                "ownership back to the host", 5f);
            AssertSimulatedOnlyBy(net, id, net.Host);
            foreach (NetworkBootstrap m in net.Machines) Assert.IsNull(CarrierOf(m, client1).Held, $"{m.name}: hands empty");

            yield return NetLootKit.Settle(1.5f);
            Vector3 landed = NetLootKit.CopyOn(net.Host, id).transform.position;
            Assert.Less(landed.y, 0.2f, "fell to the floor");
            Assert.Less(Vector3.Distance(landed, OwnPlayer(client1).transform.position), 2f, "dropped where the carrier stood");
            foreach (NetworkLoot copy in NetLootKit.Copies(net, id))
                Assert.Less(Vector3.Distance(copy.transform.position, landed), 0.1f, $"{copy.NetworkManager.name} agrees where it landed");
        }

        [UnityTest]
        public IEnumerator ThrowVelocityIsClampedOnTheHost()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            yield return ClientPicksUp(client1, id);
            NetworkLoot hostCopy = NetLootKit.CopyOn(net.Host, id), clientCopy = NetLootKit.CopyOn(client1, id);
            float max = LootServerActions.MaxReleaseSpeed(hostCopy.Config, CarrierOf(net.Host, client1), hostCopy.Grabbable, isThrow: true, remote: true);

            Assert.IsTrue(clientCopy.ClientRequestRelease(Vector3.forward * 100f, isThrow: true), "a modified client asks for 100 m/s");
            yield return NetLootKit.WaitForHold(net, id, LootHoldState.Free, "the throw to be applied");
            Assert.Greater(hostCopy.LastReleaseSpeed, 1f, "still thrown");
            Assert.LessOrEqual(hostCopy.LastReleaseSpeed, max + 0.01f, "clamped to a full-charge throw plus movement");
            Assert.Less(max, 30f, "the cap is a real limit");

            // A non-finite velocity (another client, another laptop) must not poison the host's physics.
            ulong second = 0;
            yield return SpawnInFront(client2, "laptop", 1f, i => second = i);
            yield return ClientPicksUp(client2, second);
            NetworkLoot hostSecond = NetLootKit.CopyOn(net.Host, second);
            Assert.IsTrue(NetLootKit.CopyOn(client2, second).ClientRequestRelease(new Vector3(float.NaN, 0f, float.PositiveInfinity), isThrow: true));
            yield return NetLootKit.WaitForHold(net, second, LootHoldState.Free, "the bad throw to be applied");
            Assert.AreEqual(0f, hostSecond.LastReleaseSpeed, "a non-finite velocity becomes a plain drop");
            Assert.IsTrue(float.IsFinite(hostSecond.Grabbable.Body.linearVelocity.sqrMagnitude));
        }

        [UnityTest]
        public IEnumerator HostTakesBackAnItemWhoseCarrierLeaves()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "laptop", 1f, i => id = i);
            yield return ClientPicksUp(client1, id);
            NetworkLoot hostCopy = NetLootKit.CopyOn(net.Host, id);

            client1.Manager.Shutdown();
            yield return WaitFor(() => hostCopy.Hold.Mode == LootHoldMode.Free && hostCopy.OwnerClientId == NetworkManager.ServerClientId,
                "the host to free the leaver's item", 5f);
            Assert.IsTrue(hostCopy.Grabbable.HasPhysicsAuthority && !hostCopy.Grabbable.Body.isKinematic, "host simulates it again");
            Assert.IsTrue(hostCopy.IsSpawned, "the item outlives its carrier");
            NetworkLoot other = NetLootKit.CopyOn(client2, id);
            yield return WaitFor(() => other.Hold.Mode == LootHoldMode.Free, "client 2 to see it free", 5f);
        }

        [UnityTest]
        public IEnumerator APocketedItemDropsWhereItsCarrierLeft()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "gold_watch", 1f, i => id = i);
            Handler.RequestPickup(Own(client1), NetLootKit.CopyOn(client1, id).Grabbable);
            yield return NetLootKit.WaitForHold(net, id, HeldBy(client1, LootHoldMode.Pocketed), "every machine to see the watch pocketed");
            NetworkLoot hostCopy = NetLootKit.CopyOn(net.Host, id);
            Vector3 carrierAt = PlayerOf(net.Host, Id(client1)).transform.position;

            client1.Manager.Shutdown();
            yield return WaitFor(() => hostCopy.Hold.Mode == LootHoldMode.Free, "the host to free the leaver's pocketed item", 5f);
            Assert.IsFalse(hostCopy.Grabbable.IsPocketed, "back out in the world");
            Assert.IsTrue(hostCopy.IsSpawned && hostCopy.gameObject.activeInHierarchy);
            Assert.Less(Vector3.Distance(hostCopy.transform.position, carrierAt), 2.5f, "dropped where its carrier was");
            NetworkLoot other = NetLootKit.CopyOn(client2, id);
            yield return WaitFor(() => other.Hold.Mode == LootHoldMode.Free && !other.Grabbable.IsPocketed, "client 2 to see it back out", 5f);
        }
    }
}
