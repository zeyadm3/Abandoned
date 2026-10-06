using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Collapse over in-process NGO: the host's collapse reaches every client with the same seed, each
    /// client flips its own colliders off and plays the identical pre-fractured break locally, and a
    /// client's own player standing on the tile drops through it on its own machine.
    /// </summary>
    public class StructureNetworkCollapseTests : StructureNetTestBase
    {
        [UnityTest]
        public IEnumerator CollapseReachesEveryClientWithTheSameSeedCollidersAndBreak()
        {
            StructuralSection host = HostSection(A);
            host.Collapse();
            Assert.IsFalse(host.CollidersEnabled, "the host's colliders go at once");
            yield return WaitFor(() => kit.Copies(A).All(s => s.IsCollapsed), "A to collapse on every machine", 5f);

            DebrisLifetime hostBreak = host.GetComponent<SectionPresentation>().Debris.GetComponent<DebrisLifetime>();
            Assert.Greater(hostBreak.Launches.Count, 4, "a real break with several chunks");
            foreach (NetworkBootstrap c in net.Clients)
            {
                StructuralSection copy = kit.Section(c, A);
                Assert.AreEqual(host.CollapseSeed, copy.CollapseSeed, $"{c.name} got the host's seed");
                Assert.IsFalse(copy.CollidersEnabled, $"{c.name} flipped its colliders off");
                Assert.IsFalse(copy.CollapsedQuietly, $"{c.name} saw it happen");
                GameObject debris = copy.GetComponent<SectionPresentation>().Debris;
                Assert.IsNotNull(debris, $"{c.name} plays the break locally");
                Assert.AreEqual(GameLayers.DebrisLayer, debris.GetComponentsInChildren<Rigidbody>()[0].gameObject.layer, "cosmetic Debris layer");
                Assert.IsNull(debris.GetComponent<Unity.Netcode.NetworkObject>(), "debris is never networked");
                CollectionAssert.AreEqual(hostBreak.Launches, debris.GetComponent<DebrisLifetime>().Launches,
                    $"{c.name}'s chunks launch exactly like the host's");
                Assert.AreEqual(1, kit.On(c).CollapseCount);
            }
            foreach (NetworkBootstrap m in net.Machines)
                Assert.IsFalse(kit.Section(m, B).IsCollapsed || kit.Section(m, C).IsCollapsed, $"{m.name}: only A went");
            ScreenshotCapture.CaptureFrom(TileA + new Vector3(6f, 3f, -7f), TileA + Vector3.down * 2f, "M3_6_collapse_on_every_machine");
        }

        [UnityTest]
        public IEnumerator AClientsPlayerOnTheCollapsingTileFallsOnItsOwnMachineAndEveryoneSeesIt()
        {
            yield return StandOn(client1, TileA);
            NetworkPlayer own = OwnPlayer(client1);
            float top = TileA.y;
            HostSection(A).Collapse();
            float lowest = top, hostSeen = top;
            float end = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < end && (lowest > top - 2f || hostSeen > top - 2f))
            {
                lowest = Mathf.Min(lowest, HeightOf(own));
                hostSeen = Mathf.Min(hostSeen, HeightOf(PlayerOf(net.Host, Id(client1))));
                yield return null;
            }
            Assert.IsFalse(kit.Section(client1, A).CollidersEnabled, "client 1's own copy let go of its player");
            Assert.Less(lowest, top - 2f, "client 1's player fell through A on client 1's machine");
            Assert.Less(hostSeen, top - 2f, "and the host saw client 1 go down");
            Assert.IsTrue(own.Ragdoll.IsRagdolled, "standing on a collapse knocks you down");
        }

        [UnityTest]
        public IEnumerator AHeavyItemOnACollapsingTileFallsOnEveryMachine()
        {
            ulong id = 0;
            yield return NetLootKit.Spawn(net, "server_rack", TileB + Vector3.up * 1.2f, i => id = i);
            // The host's copy loads B (300 kg on 150 kg) and drains it: no forcing, the load model does it.
            yield return WaitFor(() => net.Machines.All(m => kit.Section(m, B).IsCollapsed), "the rack to break B on every machine", 15f);
            yield return WaitSeconds(2f);
            foreach (NetworkBootstrap m in net.Machines)
                Assert.Less(NetLootKit.CopyOn(m, id).transform.position.y, TileB.y - 1.5f, $"{m.name} sees the rack fall through");
        }

        private static ulong Id(NetworkBootstrap machine) => machine.Manager.LocalClientId;

        /// <summary>How low a (possibly remote) player is: its body while down, else its feet.</summary>
        private static float HeightOf(NetworkPlayer p) => p.Ragdoll.IsRagdolled ? p.Ragdoll.BodyPosition.y : p.transform.position.y;
    }
}
