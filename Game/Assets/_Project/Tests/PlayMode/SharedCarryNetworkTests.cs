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
    /// Shared carrying over in-process NGO: the crew rule (Heavy needs 2, Huge 3), the host keeping the
    /// physics, letting go dropping the item with normal impact damage, and the slowest carrier setting
    /// everyone's speed.
    /// </summary>
    public class SharedCarryNetworkTests : SharedCarryTestBase
    {
        [UnityTest]
        public IEnumerator TwoClientsLiftARackAndItFallsAndBreaksWhenOneLetsGoOverTheEdge()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            float rest = Bottom(net.Host, id);

            yield return Grab(client1, id);
            yield return WaitForCrew(id, 1, lifted: false);
            yield return WaitSeconds(0.5f);
            Assert.AreEqual(rest, Bottom(net.Host, id), 0.05f, "one person can't lift a Heavy item; it stays down");
            Assert.AreEqual("Carrying 1/2 - needs 1 more person (dragging)", SharedCarryText.Of(SharedOn(client1, id)));

            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: true);
            yield return WaitFor(() => Bottom(net.Host, id) > rest + 0.2f, "the crew to lift the rack", 3f);
            AssertSimulatedOnlyBy(net, id, net.Host);
            Assert.IsTrue(NetLootKit.Copies(net, id).All(c => c.OwnerClientId == NetworkManager.ServerClientId),
                "a shared carry stays host-owned (host simulates from the carriers' input)");
            foreach (NetworkBootstrap m in net.Machines)
            {
                Assert.AreEqual(RackWeight / 2f, CarrierOf(m, client1).CarriedWeight, 0.01f, $"{m.name}: client 1 carries half");
                Assert.AreEqual(RackWeight / 2f, CarrierOf(m, client2).CarriedWeight, 0.01f, $"{m.name}: client 2 carries half");
            }
            Assert.AreEqual(0f, ItemOn(net.Host, id).LoadWeight, "a lifted item loads nothing itself");
            Assert.Greater(NetLootKit.CopyOn(net.Host, id).GetComponent<NetworkSharedCarry>().TargetsReceived, 0,
                "clients stream their hold targets to the host");
            Vector3 lifted = NetLootKit.CopyOn(net.Host, id).transform.position;
            ScreenshotCapture.CaptureFrom(lifted + new Vector3(1.8f, 0.1f, 3.6f), lifted + Vector3.down * 0.6f, "M3_5_two_clients_lift_rack");

            // Walk it out over the edge, each carrier stopping just short of the drop.
            const float stopZ = Edge - 0.55f;
            OwnPlayer(client1).Motor.enabled = false;
            OwnPlayer(client2).Motor.enabled = false;
            float end = Time.time + 8f;
            while (Time.time < end && (Own(client1).transform.position.z < stopZ || Own(client2).transform.position.z < stopZ))
            {
                foreach (NetworkBootstrap c in new[] { client1, client2 })
                {
                    Vector2 move = Own(c).transform.position.z < stopZ ? Vector2.up : Vector2.zero;
                    OwnPlayer(c).Motor.Simulate(PlayerTestRig.Frame(move), Time.deltaTime);
                }
                yield return null;
            }
            yield return Stand(1.5f, client1, client2);
            float rackZ = NetLootKit.CopyOn(net.Host, id).transform.position.z;
            Assert.Greater(rackZ, Edge + 0.25f, "the rack is held out beyond the edge");
            foreach (NetworkLoot copy in NetLootKit.Copies(net, id))
                Assert.Less(Vector3.Distance(copy.transform.position, NetLootKit.CopyOn(net.Host, id).transform.position), 0.3f,
                    $"{copy.NetworkManager.name} agrees where the carried rack is");

            int before = ItemOn(net.Host, id).CurrentValue;
            yield return LetGo(client1, id);
            Assert.IsFalse(SharedOn(net.Host, id).IsLifted, "one carrier can't hold a Heavy item up");
            yield return WaitFor(() => ItemOn(net.Host, id).CurrentValue < before, "the host to apply the landing's damage", 5f);
            yield return WaitFor(() => net.Machines.All(m => ItemOn(m, id).CurrentValue == ItemOn(net.Host, id).CurrentValue),
                "the damage to reach every machine", 3f);
            Assert.Less(Bottom(net.Host, id), 1f, "it fell to the ground");
            Assert.Greater(Own(client2).transform.position.y, Height - 0.2f, "the other carrier wasn't dragged off the ledge");
            yield return WaitFor(() => net.Machines.All(m => SharedOn(m, id).CarrierCount == 0),
                "client 2 to lose their grip on a rack 5 m below", 3f);
        }

        [UnityTest]
        public IEnumerator HugeItemsNeedThreeCarriers()
        {
            ulong id = 0;
            yield return SpawnPiano(i => id = i);
            float rest = Bottom(net.Host, id);
            Assert.AreEqual(3, SharedOn(net.Host, id).RequiredCarriers);
            Assert.AreEqual(4, SharedOn(net.Host, id).PointCount, "GDD: 3-4 people, so four handles");

            yield return Grab(client1, id);
            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: false);
            yield return Stand(1f, client1, client2);
            Assert.AreEqual(rest, Bottom(net.Host, id), 0.05f, "two people can't lift a Huge item");
            Assert.AreEqual("Carrying 2/3 - needs 1 more person (barely budges)", SharedCarryText.Of(SharedOn(client2, id)));
            Assert.AreEqual(0f, Own(client1).CarriedWeight, 0.01f, "an item still on the floor rests its own weight");

            yield return Grab(net.Host, id);
            yield return WaitForCrew(id, 3, lifted: true);
            yield return WaitFor(() => Bottom(net.Host, id) > rest + 0.2f, "three carriers to lift the piano", 3f);
            foreach (NetworkBootstrap m in net.Machines)
            foreach (NetworkBootstrap carrier in net.Machines)
                Assert.AreEqual(PianoWeight / 3f, CarrierOf(m, carrier).CarriedWeight, 0.01f, $"{m.name}: {carrier.name} carries a third");
            AssertUpright(net.Host, id, 10f, "three carriers standing still hold it level");
        }

        [UnityTest]
        public IEnumerator EveryoneMovesAtTheSlowestCarriersSpeed()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            yield return Grab(client1, id);
            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: true);
            yield return Stand(0.5f, client1, client2);

            // Client 1 crouch-walks; client 2 walks. Both back away from the edge, pulling the rack along.
            yield return Drive(0.4f, null, (client1, Vector2.down, true), (client2, Vector2.down, false));
            yield return WaitFor(() => CarrierOf(client2, client1).GetComponent<Abandoned.Player.PlayerMotor>().IsCrouching,
                "client 2 to see client 1 crouching", 2f);
            CarryConfig carry = Own(client1).Config;
            var movement = OwnPlayer(client1).Motor.Config;
            float crouchCap = movement.CrouchSpeed * carry.SpeedMultiplierFor(RackWeight / 2f);
            float walkCap = movement.WalkSpeed * carry.SpeedMultiplierFor(RackWeight / 2f);
            Assert.AreEqual(crouchCap, SharedOn(client2, id).GroupMaxSpeed, 0.01f, "client 2's machine holds it to client 1's crouch speed");
            Assert.AreEqual(crouchCap, SharedOn(net.Host, id).GroupMaxSpeed, 0.01f, "the host caps the rack at the same speed");

            Vector3 fastStart = Own(client2).transform.position, rackStart = NetLootKit.CopyOn(net.Host, id).transform.position;
            const float window = 1.5f;
            float start = Time.time;
            yield return Drive(window, null, (client1, Vector2.down, true), (client2, Vector2.down, false));
            float elapsed = Time.time - start;
            float fastSpeed = Flat(Own(client2).transform.position - fastStart) / elapsed;
            float rackSpeed = Flat(NetLootKit.CopyOn(net.Host, id).transform.position - rackStart) / elapsed;
            Assert.Greater(fastSpeed, crouchCap * 0.5f, "the crew still moves");
            Assert.LessOrEqual(fastSpeed, crouchCap * 1.1f, $"the walker is held to the croucher's {crouchCap:0.00} m/s, not {walkCap:0.00}");
            Assert.LessOrEqual(rackSpeed, crouchCap * SpeedSlack(id) + 0.1f, "the rack moves no faster than the slowest carrier");
        }

        private float SpeedSlack(ulong id) => SharedOn(net.Host, id).Config.SpeedSlack;

        private static float Flat(Vector3 v) => new Vector2(v.x, v.z).magnitude;
    }
}
