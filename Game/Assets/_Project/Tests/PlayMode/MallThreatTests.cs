using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using Abandoned.Networking;
using Abandoned.Structure;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>The mall's NavMesh and the Blind One: it hears, hunts and kills; collapses cut its paths.</summary>
    public class MallThreatTests
    {
        private PlayerTestRig rig;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return TestMapScene.Load("Mall");
            rig = ForExisting(TestMapScene.Player);
            for (int i = 0; i < 60 && (RunState.Current == null || ThreatDirector.Current == null); i++) yield return null;
        }

        private static bool Reachable(Vector3 from, Vector3 to)
        {
            var path = new NavMeshPath();
            return NavMesh.SamplePosition(from, out NavMeshHit a, 2f, NavMesh.AllAreas) && NavMesh.SamplePosition(to, out NavMeshHit b, 2f, NavMesh.AllAreas)
                   && NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path) && path.status == NavMeshPathStatus.PathComplete;
        }

        [Test]
        public void TheNavMeshReachesEveryFloorButNotOutside()
        {
            Vector3 parking = new(24f, 0f, 3f); // just inside the entrance
            Assert.IsFalse(NavMesh.SamplePosition(new Vector3(24f, 0f, -8f), out _, 1f, NavMesh.AllAreas), "the parking lot isn't its territory");
            Assert.IsFalse(NavMesh.SamplePosition(new Vector3(6f, 12.3f, 6f), out _, 1f, NavMesh.AllAreas), "nor the roof");
            Assert.IsTrue(Reachable(parking, new Vector3(6f, 0f, 30f)), "ground floor clothing store");
            Assert.IsTrue(Reachable(parking, new Vector3(6f, 4f, 20f)), "floor 1 furniture store");
            Assert.IsTrue(Reachable(parking, new Vector3(22f, 8f, 37f)), "floor 2 gallery");
            Assert.IsTrue(Reachable(parking, new Vector3(42f, 0f, 30f)), "loading bay");
        }

        [UnityTest]
        public IEnumerator ACollapsedFloorCarvesAHoleInItsPaths()
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            StructuralSection tile = sim.Sections.First(s => s.name == "Floor_1_1_5");
            Vector3 top = tile.transform.position;
            Assert.IsTrue(NavMesh.SamplePosition(top, out _, 0.3f, NavMesh.AllAreas), "walkable before");
            tile.Collapse();
            yield return null;
            yield return null;
            Assert.IsFalse(NavMesh.SamplePosition(top, out _, 0.3f, NavMesh.AllAreas), "a hole after");
            sim.ApplyStability(sim.Stability, sim.Seed + 1);
            yield return null;
            yield return null;
            Assert.IsTrue(NavMesh.SamplePosition(top, out _, 0.3f, NavMesh.AllAreas), "walkable again next run");
        }

        [UnityTest]
        public IEnumerator TheBlindOneHuntsANoiseAndKillsOnContact()
        {
            BlindOne monster = ThreatDirector.Current.Spawn();
            Assert.IsNotNull(monster);
            yield return null;
            Assert.AreEqual(BlindOneState.Wander, monster.State);

            // A quiet step across the building: nothing. A crash next to it: a hunt.
            NoiseSystem.Emit(monster.transform.position + new Vector3(30f, 0f, 30f), 0.05f, NoiseSource.Footstep);
            yield return null;
            Assert.AreEqual(BlindOneState.Wander, monster.State);
            Vector3 crash = monster.transform.position + Vector3.right * 4f;
            NoiseSystem.Emit(crash, 1f, NoiseSource.LootImpact);
            yield return new WaitForSeconds(0.6f);
            Assert.AreEqual(BlindOneState.Hunt, monster.State);
            float before = Vector3.Distance(monster.transform.position, crash);
            yield return new WaitForSeconds(0.6f);
            Assert.Less(Vector3.Distance(monster.transform.position, crash), before, "it closes in");

            // The player carries a pocket item into it.
            NetworkLoot pocket = Object.FindAnyObjectByType<NetworkLootSpawner>().Spawned.First(l => l != null && l.Item.Definition.CarryClass == CarryClass.Pocket);
            PlayerCarrier carrier = rig.Player.GetComponent<PlayerCarrier>();
            rig.Teleport(pocket.transform.position + Vector3.back * 0.8f);
            rig.Settle();
            pocket.Grabbable.Body.position = carrier.EyePosition + carrier.EyeForward * 0.8f;
            pocket.transform.position = pocket.Grabbable.Body.position;
            yield return null;
            Assert.IsTrue(LootServerActions.TryPickup(pocket, carrier, out string reason), reason);

            NetworkPlayer me = NetworkPlayer.Local;
            me.OwnerTeleport(monster.transform.position + Vector3.forward * 0.6f);
            yield return new WaitForSeconds(0.3f);
            Assert.IsTrue(me.IsDead, "contact kills");
            Assert.IsTrue(me.Ragdoll.IsRagdolled);
            Assert.IsFalse(pocket.Grabbable.IsPocketed, "the pocket loot dropped where they died");
            Assert.AreEqual(LootHoldMode.Free, pocket.Hold.Mode);
            Assert.AreEqual(1, monster.Kills);

            // Everyone (solo: just us) is dead: the run ends; the next run brings us back.
            float end = Time.time + 2f;
            while (RunState.Current.State.Phase != RunPhase.Departed && Time.time < end) yield return null;
            Assert.AreEqual(RunPhase.Departed, RunState.Current.State.Phase, "no one left alive: the run is over");
            Object.FindAnyObjectByType<RunDirector>().StartNextRun();
            yield return new WaitForSeconds(0.5f);
            Assert.IsFalse(me.IsDead, "alive again");
            Assert.IsFalse(me.Ragdoll.IsRagdolled, "standing");
            Assert.AreEqual(0, BlindOne.All.Count, "the old run's threats are gone");
        }
    }
}
