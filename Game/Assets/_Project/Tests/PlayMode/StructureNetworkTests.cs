using System.Collections;
using System.Linq;
using Abandoned.Networking;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Structure state over in-process NGO: the host's simulation is the only one running, and clients
    /// see the same stages, health, load, Failing clock, stability and seed.
    /// </summary>
    public class StructureNetworkTests : StructureNetTestBase
    {
        [UnityTest]
        public IEnumerator OnlyTheHostSimulatesAndClientsStartFromTheHostsState()
        {
            Assert.IsTrue(kit.On(net.Host).HasAuthority, "the host runs the building");
            StructureSimulation host = kit.On(net.Host);
            foreach (NetworkBootstrap c in net.Clients)
            {
                StructureSimulation mirror = kit.On(c);
                Assert.IsTrue(mirror.IsMirror && !mirror.HasAuthority, $"{c.name} only mirrors");
                Assert.IsFalse(StructureNetSync.For(c.Manager).LayoutMismatch);
                Assert.AreEqual(host.Stability, mirror.Stability, 1e-4f);
                Assert.AreEqual(host.Seed, mirror.Seed);
                foreach (StructuralSection s in host.Sections)
                {
                    StructuralSection copy = mirror.Sections[s.Id];
                    Assert.AreEqual(s.name, copy.name, "same ids on every machine");
                    Assert.AreEqual(s.Stage, copy.Stage, $"{c.name} {s.name} stage");
                    Assert.AreEqual(s.HealthFraction, copy.HealthFraction, 0.5f / 255f + 1e-4f, $"{c.name} {s.name} health");
                }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClientsNeverApplyDamageThemselves()
        {
            StructuralSection mirror = kit.Section(client1, A);
            float before = mirror.HealthFraction;
            mirror.ApplyImpact(1e6f);
            mirror.Collapse();
            kit.On(client1).Step(5f);
            Assert.AreEqual(before, mirror.HealthFraction, 1e-6f, "an impact on a client copy changes nothing");
            Assert.IsFalse(mirror.IsCollapsed, "a client can't collapse its copy");
            Assert.IsTrue(mirror.CollidersEnabled);
            yield return WaitSeconds(0.3f);
            Assert.AreEqual(StructuralStage.Stable, HostSection(A).Stage, "and the host never heard of it");
        }

        [UnityTest]
        public IEnumerator StageAndHealthChangesReplicateToEveryClient()
        {
            StructuralSection host = HostSection(A);
            host.ApplyImpact(2500f);
            Assert.AreEqual(StructuralStage.Stressed, host.Stage, "a medium hit only stresses it");
            yield return WaitForClientsToMatch(A, "clients to see A stressed");
            yield return WaitFor(() => net.Clients.All(c => Mathf.Abs(kit.Section(c, A).HealthFraction - host.HealthFraction) < 0.004f),
                "clients to get A's health", 5f);

            host.ApplyImpact(1e6f);
            Assert.AreEqual(StructuralStage.Cracking, host.Stage);
            yield return WaitForClientsToMatch(A, "clients to see A cracking");
            foreach (NetworkBootstrap c in net.Clients)
            {
                StructuralSection copy = kit.Section(c, A);
                Assert.AreEqual(host.HealthFraction, copy.HealthFraction, 0.5f / 255f + 1e-4f, $"{c.name} health");
                Assert.Greater(copy.GetComponent<SectionPresentation>().CrackCount, 0, $"{c.name} shows the cracks");
                Assert.IsTrue(copy.CollidersEnabled);
            }
        }

        [UnityTest]
        public IEnumerator FailingClockAndSagFollowTheHost()
        {
            StructuralSection host = HostSection(B);
            Vector3 rest = kit.Section(client1, B).Visual.localPosition;
            BreakToFailing(host);
            yield return WaitForClientsToMatch(B, "clients to see B failing");
            yield return WaitSeconds(1f);
            Assert.IsFalse(host.IsCollapsed, "still inside its 2 s window");
            foreach (NetworkBootstrap c in net.Clients)
            {
                StructuralSection copy = kit.Section(c, B);
                Assert.AreEqual(StructuralStage.Failing, copy.Stage);
                Assert.AreEqual(host.FailingTime, copy.FailingTime, 0.25f, $"{c.name} counts the host's Failing time");
                Assert.Less(copy.Visual.localPosition.y, rest.y - 0.01f, $"{c.name} sees it sag");
            }
            yield return WaitFor(() => net.Machines.All(m => kit.Section(m, B).IsCollapsed), "B to collapse everywhere", 3f);
        }

        [UnityTest]
        public IEnumerator LoadFromAClientsPlayerAndCarriedItemIsComputedOnTheHost()
        {
            yield return StandOn(client1, TileA);
            float body = OwnPlayer(client1).Motor.Config.BodyWeight;
            yield return WaitFor(() => Mathf.Abs(HostSection(A).Load - body) < 0.5f, "the host to weigh client 1 on A", 5f);
            yield return WaitFor(() => Mathf.Abs(kit.Section(client2, A).Load - body) <= SectionNetState.LoadStep,
                "client 2 to see that load", 5f);

            Transform t = OwnPlayer(client1).transform;
            ulong id = 0;
            yield return NetLootKit.Spawn(net, "flatscreen_tv", t.position + t.forward * 1f + Vector3.up * 0.5f, i => id = i);
            yield return NetLootKit.Settle();
            Abandoned.Interaction.InteractionService.Handler.RequestPickup(OwnPlayer(client1).Carrier, NetLootKit.CopyOn(client1, id).Grabbable);
            yield return NetLootKit.WaitForHold(net, id, LootHoldState.For(LootHoldMode.Held, OwnPlayer(client1).NetworkObjectId),
                "everyone to see client 1 holding the TV");
            float tv = NetLootKit.CopyOn(net.Host, id).Item.Definition.GameplayWeight;
            yield return WaitFor(() => Mathf.Abs(HostSection(A).Load - (body + tv)) < 0.5f,
                $"the host to weigh client 1 + the TV it carries ({body + tv} kg) on A", 5f);
            Assert.AreEqual(0f, HostSection(B).Load, 0.01f, "nothing leaks onto the neighbour");
        }

        [UnityTest]
        public IEnumerator RerollAndStabilityChangeSyncAndRestoreEverything()
        {
            BreakToFailing(HostSection(B));
            yield return WaitFor(() => net.Machines.All(m => kit.Section(m, B).IsCollapsed), "B to collapse everywhere", 4f);
            Assert.IsTrue(net.Clients.All(c => kit.Section(c, B).GetComponent<SectionPresentation>().Debris != null));

            kit.On(net.Host).ApplyStability(0.3f, 4242);
            StructureSimulation host = kit.On(net.Host);
            yield return WaitFor(() => net.Clients.All(c => kit.On(c).Seed == 4242), "clients to get the new seed", 5f);
            yield return WaitSeconds(0.2f);
            foreach (NetworkBootstrap c in net.Clients)
            {
                StructureSimulation mirror = kit.On(c);
                Assert.AreEqual(0.3f, mirror.Stability, 1e-4f, $"{c.name} stability");
                foreach (StructuralSection s in host.Sections)
                {
                    StructuralSection copy = mirror.Sections[s.Id];
                    Assert.AreEqual(s.Stage, copy.Stage, $"{c.name} {s.name} stage after the re-roll");
                    Assert.AreEqual(s.HealthFraction, copy.HealthFraction, 0.5f / 255f + 1e-4f, $"{c.name} {s.name} pre-damage");
                    Assert.AreEqual(s.Capacity, copy.Capacity, 0.01f, $"{c.name} {s.name} capacity scaled for 30%");
                }
                StructuralSection b = mirror.Sections.Single(s => s.name == B);
                Assert.IsFalse(b.IsCollapsed, $"{c.name}: B is back");
                Assert.IsTrue(b.CollidersEnabled, $"{c.name}: B can be stood on again");
                Assert.IsNull(b.GetComponent<SectionPresentation>().Debris, $"{c.name}: B's debris cleared");
            }
        }
    }
}
