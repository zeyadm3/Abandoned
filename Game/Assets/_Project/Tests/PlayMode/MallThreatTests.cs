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
            Vector3 inside = new(28f, 0f, 3f); // just inside the entrance
            Assert.IsFalse(NavMesh.SamplePosition(new Vector3(28f, 0f, -8f), out _, 1f, NavMesh.AllAreas), "the front court isn't its territory");
            Assert.IsFalse(NavMesh.SamplePosition(new Vector3(28f, 8.3f, 24f), out _, 1f, NavMesh.AllAreas), "nor the roof");
            Assert.IsTrue(Reachable(inside, new Vector3(8f, 0f, 24f)), "ground floor clothing store");
            Assert.IsTrue(Reachable(inside, new Vector3(8f, 4f, 24f)), "floor 1 offices");
            Assert.IsTrue(Reachable(inside, new Vector3(22f, 4f, 34f)), "floor 1 rear gallery");
            Assert.IsTrue(Reachable(inside, new Vector3(46f, -4f, 30f)), "the basement");
        }

        [UnityTest]
        public IEnumerator ACollapsedFloorCarvesAHoleInItsPaths()
        {
            StructureSimulation sim = Object.FindAnyObjectByType<StructureSimulation>();
            // An upper tile that can fall with clear floor (NavMesh) at its centre.
            StructuralSection tile = sim.Sections.Where(s => s.CanCollapse && s.Type == SectionType.Floor && s.transform.position.y > 3f
                    && NavMesh.SamplePosition(s.transform.position, out _, 0.3f, NavMesh.AllAreas)).OrderBy(s => s.name).First();
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
            yield return MallTestSpots.StepInside();
            BlindOne monster = ThreatDirector.Current.Spawn();
            Assert.IsNotNull(monster);
            MallTestSpots.SkipWarning(monster);
            // Somewhere open: the crash it hears is 4 m away in a straight, walkable line.
            (Vector3 at, Vector3 crash) = MallTestSpots.OpenLine(4f);
            monster.GetComponent<NavMeshAgent>().Warp(at);
            yield return null;
            // The mall is never silent (a route change dropping a floor, a splash): it may be investigating, never hunting.
            Assert.AreNotEqual(BlindOneState.Hunt, monster.State);

            // A quiet step across the building: nothing. A crash next to it: a hunt.
            NoiseSystem.Emit(monster.transform.position + new Vector3(30f, 0f, 30f), 0.05f, NoiseSource.Footstep);
            yield return null;
            Assert.AreNotEqual(BlindOneState.Hunt, monster.State, "a quiet step far off doesn't start a hunt");
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
            // It is blind: it strikes what it hears, and walking into it makes steps. Since the rebuilt threats it is
            // no longer a one-touch kill (70 a strike): keep blundering into it.
            float health = me.Health, until = Time.time + 10f;
            bool hurt = false;
            while (!me.IsDead && Time.time < until)
            {
                me.OwnerTeleport(monster.transform.position + monster.transform.forward * 0.6f);
                yield return null;
                NoiseSystem.Emit(me.transform.position, 0.6f, NoiseSource.Footstep);
                yield return new WaitForSeconds(0.3f);
                hurt |= me.Health < health;
            }
            Assert.IsTrue(hurt, "contact hurts");
            Assert.IsTrue(me.IsDead, "and keeps hurting until you're down");
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
