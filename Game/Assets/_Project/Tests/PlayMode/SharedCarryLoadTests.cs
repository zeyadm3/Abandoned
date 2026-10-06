using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using static Abandoned.Tests.NetTestHarness;

namespace Abandoned.Tests
{
    /// <summary>
    /// Shared carrying and the rest of the game over in-process NGO: the logical load model splits a
    /// lifted item across each carrier's own section, solo Heavy dragging still works, a ragdolled
    /// carrier lets go, and disagreeing carriers make the load sway.
    /// </summary>
    public class SharedCarryLoadTests : SharedCarryTestBase
    {
        private StructureTestRig structure;

        [UnityTearDown]
        public IEnumerator DestroyStructure()
        {
            structure?.Destroy();
            structure = null;
            yield return null;
        }

        [UnityTest]
        public IEnumerator EachCarriersShareLoadsTheSectionUnderThem()
        {
            // Two floor tiles in the platform's surface: client 1 stands on A, client 2 on B. The host steps off both.
            structure = StructureTestRig.Create(ground: false);
            StructuralSection a = structure.AddTile("A", new Vector3(0.5f, Height, -1f), collapsible: false, presentation: false);
            StructuralSection b = structure.AddTile("B", new Vector3(4.5f, Height, -1f), collapsible: false, presentation: false);
            structure.StartSimulation();
            PlayerTestRig.ForExisting(OwnPlayer(net.Host).gameObject).Teleport(new Vector3(-3f, Height + 0.05f, -6f));
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            // One process holds every machine's copies; real clients never run the structure, so count only the host's.
            KeepOnlyHostLoads();
            float body = OwnPlayer(client1).Motor.Config.BodyWeight;
            yield return Grab(client1, id);
            yield return WaitForCrew(id, 1, lifted: false);
            structure.Simulation.SolveLoads();
            Assert.AreEqual(body, structure.Simulation.LoadOn(a), 0.5f, "dragging alone, client 1 carries none of it");

            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: true);
            yield return WaitSeconds(0.75f);
            structure.Simulation.SolveLoads();
            Assert.AreEqual(body + RackWeight / 2f, structure.Simulation.LoadOn(a), 0.5f, "client 1's half goes through client 1's feet");
            Assert.AreEqual(body + RackWeight / 2f, structure.Simulation.LoadOn(b), 0.5f, "client 2's half goes through client 2's feet");
        }

        [UnityTest]
        public IEnumerator OnePlayerCanStillDragAHeavyItem()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            yield return Grab(client1, id);
            yield return WaitForCrew(id, 1, lifted: false);
            foreach (NetworkBootstrap m in net.Machines)
            {
                Assert.IsTrue(CarrierOf(m, client1).IsDragging, $"{m.name} sees client 1 dragging (trolley stand-in)");
                Assert.AreEqual(RackWeight, ItemOn(m, id).LoadWeight, 0.01f, $"{m.name}: the dragged rack rests its own weight");
            }

            Vector3 start = NetLootKit.CopyOn(net.Host, id).transform.position;
            yield return Drive(1.5f, null, (client1, Vector2.down, false));
            yield return Stand(1f, client1);
            Vector3 host = NetLootKit.CopyOn(net.Host, id).transform.position;
            Assert.Greater(start.z - host.z, 0.75f, "the host moved the rack along behind client 1");
            Assert.Less(Vector3.Distance(NetLootKit.CopyOn(client1, id).transform.position, host), 0.3f, "and client 1 sees it there");
            AssertUpright(net.Host, id, 10f, "dragging doesn't tip a 2 m rack over");
        }

        [UnityTest]
        public IEnumerator ACarrierWhoGoesDownLetsGoAndTheItemDrops()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            float rest = Bottom(net.Host, id);
            yield return Grab(client1, id);
            yield return Grab(client2, id);
            yield return WaitFor(() => Bottom(net.Host, id) > rest + 0.2f, "the crew to lift the rack", 5f);

            OwnPlayer(client1).Ragdoll.Enter(Vector3.zero);
            yield return WaitForCrew(id, 1, lifted: false);
            Assert.IsNull(Own(client1).Held, "a ragdolled carrier holds nothing");
            yield return WaitFor(() => Bottom(net.Host, id) < rest + 0.05f, "the rack to drop back to the floor", 3f);
        }

        [UnityTest]
        public IEnumerator CarriersPullingApartMakeTheLoadSway()
        {
            ulong id = 0;
            yield return SpawnRack(i => id = i);
            yield return Grab(client1, id);
            yield return Grab(client2, id);
            yield return WaitForCrew(id, 2, lifted: true);
            yield return Stand(1.5f, client1, client2);
            float calm = Vector3.Angle(NetLootKit.CopyOn(net.Host, id).transform.up, Vector3.up);
            Assert.Less(calm, 2f, "standing still, the load hangs level");

            float swayed = 0f;
            Transform rack = NetLootKit.CopyOn(net.Host, id).transform;
            OwnPlayer(client1).Motor.enabled = false;
            OwnPlayer(client2).Motor.enabled = false;
            for (float t = 0f; t < 1.5f; t += Time.deltaTime)
            {
                OwnPlayer(client1).Motor.Simulate(PlayerTestRig.Frame(Vector2.left), Time.deltaTime);
                OwnPlayer(client2).Motor.Simulate(PlayerTestRig.Frame(Vector2.right), Time.deltaTime);
                swayed = Mathf.Max(swayed, Vector3.Angle(rack.up, Vector3.up));
                yield return null;
            }
            Assert.Greater(swayed, 3f, "pulling in different directions rocks the load");
            Assert.IsTrue(SharedOn(net.Host, id).IsLifted, "but they're still carrying it");
            foreach (NetworkBootstrap c in new[] { client1, client2 })
            {
                SharedCarryable shared = SharedOn(c, id);
                Vector3 off = Own(c).transform.position - shared.AnchorFor(shared.IndexOf(Own(c)));
                Assert.Less(new Vector2(off.x, off.z).magnitude, shared.Config.TetherPullStart + 0.3f,
                    $"{c.name}'s tether kept them by their handle instead of walking away from it");
            }
        }

        private void KeepOnlyHostLoads()
        {
            foreach (NetworkBootstrap m in net.Clients)
            foreach (NetworkObject no in m.Manager.SpawnManager.SpawnedObjectsList)
            foreach (ILoadSource source in no.GetComponentsInChildren<ILoadSource>(true))
                LoadSources.Unregister(source);
        }
    }
}
