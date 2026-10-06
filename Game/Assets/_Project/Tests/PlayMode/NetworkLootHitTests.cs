using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// M3.4 review: hits that only happen against a kinematic copy another machine simulates.
    /// A host-simulated item must still knock a client's player down (only the owner judges hits on
    /// its player), and a client-carried item striking resting loot must damage and push that loot
    /// on the host (where neither the carrier's machine nor the host's physics would judge it).
    /// </summary>
    public class NetworkLootHitTests : NetworkLootTestBase
    {
        [UnityTest]
        public IEnumerator HostThrownSafeKnocksDownTheClientPlayerOnTheClient()
        {
            Transform target = OwnPlayer(client1).transform;
            PlayerRagdoll clientRagdoll = OwnPlayer(client1).Ragdoll;
            Vector3 start = target.position + target.forward * 3f + Vector3.up * 1.2f;
            ulong id = 0;
            yield return NetLootKit.Spawn(net, "safe", start, i => id = i);
            NetworkLoot hostCopy = NetLootKit.CopyOn(net.Host, id), clientCopy = NetLootKit.CopyOn(client1, id);
            Assert.IsTrue(clientCopy.Grabbable.Body.isKinematic, "on the client the host's safe is a kinematic follower");
            Assert.IsFalse(clientRagdoll.IsRagdolled);

            // Thrown by the host's physics straight at client 1's chest.
            Vector3 toPlayer = target.position + Vector3.up - start;
            hostCopy.Grabbable.Body.linearVelocity = toPlayer.normalized * 7f + Vector3.up * 1.5f;
            float fastestFollow = 0f;
            yield return WaitFor(() =>
            {
                fastestFollow = Mathf.Max(fastestFollow, clientCopy.Grabbable.Velocity.magnitude);
                return clientRagdoll.IsRagdolled;
            }, "client 1's own machine to knock its player down", 4f);
            Assert.Greater(fastestFollow, 3f, "the follower copy reported the network motion, not its zero Rigidbody velocity");
            yield return WaitFor(() => PlayerOf(net.Host, Id(client1)).Ragdoll.IsRagdolled, "the host to see client 1 fall", 3f);
        }

        [UnityTest]
        public IEnumerator RestingFollowerCopyDoesNotKnockAnyoneDown()
        {
            ulong id = 0;
            yield return SpawnInFront(client1, "safe", 0.9f, i => id = i);
            NetworkLoot clientCopy = NetLootKit.CopyOn(client1, id);
            Assert.IsTrue(clientCopy.Grabbable.Body.isKinematic);
            Assert.Less(clientCopy.Grabbable.Velocity.magnitude, 0.1f, "a settled follower isn't moving");
            yield return WaitSeconds(0.5f);
            Assert.IsFalse(OwnPlayer(client1).Ragdoll.IsRagdolled, "standing next to a safe is not being hit");
        }

        [UnityTest]
        public IEnumerator ClientCarriedItemStrikingRestingLootDamagesAndPushesItOnTheHost()
        {
            ulong tv = 0, laptop = 0;
            yield return SpawnInFront(client1, "flatscreen_tv", 1.9f, i => tv = i);
            yield return SpawnInFront(client1, "laptop", 0.8f, i => laptop = i, sideways: 0.6f);
            yield return ClientPicksUp(client1, laptop);
            yield return WaitSeconds(0.6f);

            NetworkLoot hostTv = NetLootKit.CopyOn(net.Host, tv), clientTv = NetLootKit.CopyOn(client1, tv);
            Rigidbody carried = NetLootKit.CopyOn(client1, laptop).Grabbable.Body;
            Assert.IsTrue(clientTv.Grabbable.Body.isKinematic, "the carrier only follows the TV: it's a wall there");
            Assert.IsFalse(carried.isKinematic, "the carrier simulates its laptop");
            int fullValue = hostTv.Item.FullValue;
            Vector3 tvStart = hostTv.transform.position;
            Quaternion tvStartRotation = hostTv.transform.rotation;

            // Swing the laptop into the TV on the carrier's machine (overriding the hold spring).
            float until = Time.time + 1.5f;
            while (hostTv.LastStruckSpeed < 0f && Time.time < until)
            {
                Vector3 aim = clientTv.Grabbable.GetBounds().center - carried.position;
                carried.linearVelocity = aim.normalized * 6f;
                yield return new WaitForFixedUpdate();
            }
            Assert.Greater(hostTv.LastStruckSpeed, 2.5f, "the host took the strike the carrier reported, at the carrier's speed");
            yield return WaitFor(() => NetLootKit.Copies(net, tv).All(c => c.Item.CurrentValue < fullValue),
                "every machine to see the struck TV lose value", 5f);

            yield return NetLootKit.Settle(1.5f);
            bool moved = Vector3.Distance(hostTv.transform.position, tvStart) > 0.03f
                         || Quaternion.Angle(hostTv.transform.rotation, tvStartRotation) > 3f;
            Assert.IsTrue(moved, "the host pushed the struck TV (it isn't an immovable wall)");
            Assert.Less(Vector3.Distance(clientTv.transform.position, hostTv.transform.position), 0.15f, "the carrier sees where the TV went");
        }

        [UnityTest]
        public IEnumerator StrikeReportsOnlyHitHostSimulatedLootNearTheContact()
        {
            ulong tv = 0, laptop = 0;
            yield return SpawnInFront(client2, "flatscreen_tv", 1f, i => tv = i);
            yield return SpawnInFront(client1, "laptop", 1f, i => laptop = i);
            yield return ClientPicksUp(client1, laptop);
            NetworkLoot reporter = NetLootKit.CopyOn(client1, laptop), hostTv = NetLootKit.CopyOn(net.Host, tv);
            int value = hostTv.Item.CurrentValue;

            // Beside the laptop but away from the TV: the laptop's own report is plausible, the strike isn't.
            Vector3 awayFromTv = reporter.transform.position + Vector3.left * 2f;
            float limit = reporter.Config.MaxImpactPointDistance;
            Assert.Greater(hostTv.Grabbable.GetBounds().SqrDistance(awayFromTv), limit * limit, "precondition: point far from the TV");
            reporter.ReportImpact(6f, awayFromTv, tv, Vector3.forward);
            yield return WaitSeconds(0.4f);
            Assert.Less(hostTv.LastStruckSpeed, 0f, "a strike reported metres away from the TV is ignored");

            // Held by someone else: its own carrier reports its hits.
            yield return ClientPicksUp(client2, tv);
            reporter.ReportImpact(6f, reporter.transform.position, tv, Vector3.forward);
            yield return WaitSeconds(0.4f);
            Assert.Less(hostTv.LastStruckSpeed, 0f, "a carried item is not struck through someone else's report");
            Assert.AreEqual(value, hostTv.Item.CurrentValue);
        }
    }
}
