using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// A client that connects after a collapse (joining at HQ between runs, or the test building
    /// already broken) gets the building as it is: the section is gone with its colliders off, but no
    /// break, crash or ragdoll is replayed out of nowhere.
    /// </summary>
    public class StructureNetworkJoinTests : StructureNetTestBase
    {
        protected override IEnumerator Connect()
        {
            net.StartHost(net.AddMachine("Host"));
            net.AddMachine("Client1");
            kit = NetStructureKit.Build(net, Tiles);
            yield return WaitFor(() => StructureNetSync.For(net.Host.Manager) != null, "the host's structure sync");
            client1 = net.Clients.Single();
        }

        [UnityTest]
        public IEnumerator AJoinerSeesEarlierCollapsesWithoutReplayingThem()
        {
            StructuralSection host = HostSection(A);
            host.Collapse();
            HostSection(B).ApplyImpact(1e6f);
            yield return WaitSeconds(0.2f);

            yield return net.ConnectClients();
            NetStructureSeparator.Create(net, kit);
            yield return kit.WaitForSync();
            yield return WaitSeconds(0.2f);

            StructuralSection copy = kit.Section(client1, A);
            Assert.IsTrue(copy.IsCollapsed, "already down on the joiner");
            Assert.IsTrue(copy.CollapsedQuietly, "applied as existing state");
            Assert.AreEqual(host.CollapseSeed, copy.CollapseSeed);
            Assert.IsFalse(copy.CollidersEnabled);
            Assert.IsNull(copy.GetComponent<SectionPresentation>().Debris, "no break replayed on joining");
            Assert.AreEqual(HostSection(B).Stage, kit.Section(client1, B).Stage, "damage done before joining is there too");
            Assert.AreEqual(HostSection(B).HealthFraction, kit.Section(client1, B).HealthFraction, 0.5f / 255f + 1e-4f);
        }
    }
}
