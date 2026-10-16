using System;
using System.Collections;
using System.Linq;
using Abandoned.Extraction;
using Abandoned.Loot;
using Abandoned.Networking;
using Abandoned.Player;
using Abandoned.Structure;
using Abandoned.Threats;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Abandoned.Tests
{
    /// <summary>
    /// A contract destination must be a complete salvage run, rather than a scene that merely opens in the
    /// editor. These checks deliberately cover the shared game loop instead of map geometry, which has its
    /// own focused map-builder checks: crew spawn, structural damage, loot, threats, power and extraction.
    /// </summary>
    public class SalvageMapTests
    {
        private static readonly string[] NewMaps = { "School", "Factory", "Prison" };

        [UnityTest]
        public IEnumerator EveryNewLocationStartsACompleteSalvageRun()
        {
            foreach (string scene in NewMaps)
            {
                yield return TestMapScene.Load(scene);
                yield return WaitUntil(() => RunState.Current != null && RunState.Current.IsSpawned, 5f,
                    $"{scene}: the host did not start a run");

                Assert.IsNotNull(Object.FindAnyObjectByType<StructureSimulation>(), $"{scene}: no structural simulation");
                Assert.IsNotEmpty(Object.FindAnyObjectByType<StructureSimulation>().Sections, $"{scene}: no structural sections");
                Assert.IsNotNull(Object.FindAnyObjectByType<StructureNetSpawner>(), $"{scene}: no structure sync spawner");

                PlayerSpawnPoint[] spawns = Object.FindObjectsByType<PlayerSpawnPoint>(FindObjectsSortMode.None);
                CollectionAssert.AreEquivalent(Enumerable.Range(0, 4), spawns.Select(s => s.Index),
                    $"{scene}: one distinct spawn for every crew slot");

                NetworkLootSpawner loot = Object.FindAnyObjectByType<NetworkLootSpawner>();
                Assert.IsNotNull(loot, $"{scene}: no network loot spawner");
                Assert.IsNotNull(loot.Catalog, $"{scene}: loot catalog is not wired");
                LootSpawnPoint[] lootPoints = loot.Points().ToArray();
                Assert.GreaterOrEqual(lootPoints.Length, 8, $"{scene}: too few authored salvage points");
                Assert.IsTrue(lootPoints.Any(point => point.IsJackpot), $"{scene}: no authored jackpot site");
                Assert.IsTrue(loot.Spawned.Any(item => item != null && item.Item.Definition.Jackpot), $"{scene}: the run did not spawn a jackpot");

                TruckCargo truck = Object.FindAnyObjectByType<TruckCargo>();
                Assert.IsNotNull(truck, $"{scene}: no extraction truck");
                Assert.IsNotNull(truck.Ignition, $"{scene}: truck ignition is not wired");
                Assert.IsNotNull(truck.Ignition.GetComponent<TruckIgnition>(), $"{scene}: truck cannot depart");

                Assert.IsNotNull(Object.FindAnyObjectByType<ThreatDirector>(), $"{scene}: no threat director");
                Assert.GreaterOrEqual(Object.FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None).Length, 4,
                    $"{scene}: too few threat spawn points");
                Assert.IsNotNull(Object.FindAnyObjectByType<DangerDirector>(), $"{scene}: no danger escalation");
                Assert.IsNotNull(Object.FindAnyObjectByType<PowerController>(), $"{scene}: no contract power control");

                Vector3 entry = new(24f, 0f, 4f);
                Assert.IsFalse(NavMesh.SamplePosition(new Vector3(24f, 0f, -6.5f), out _, 1f, NavMesh.AllAreas),
                    $"{scene}: exterior staging should not be a threat-navigation island");
                foreach (LootSpawnPoint point in lootPoints)
                    Assert.IsTrue(Reachable(entry, point.transform.position, out string route),
                        $"{scene}: no complete route to {point.name}; {route}");
                foreach (ThreatSpawnPoint point in Object.FindObjectsByType<ThreatSpawnPoint>(FindObjectsSortMode.None))
                    Assert.IsTrue(Reachable(entry, point.transform.position, out string route),
                        $"{scene}: no complete route to {point.name}; {route}");
            }
        }

        private static bool Reachable(Vector3 from, Vector3 to, out string diagnostic)
        {
            var path = new NavMeshPath();
            bool hasStart = NavMesh.SamplePosition(from, out NavMeshHit a, 2f, NavMesh.AllAreas);
            bool hasEnd = NavMesh.SamplePosition(to, out NavMeshHit b, 2f, NavMesh.AllAreas);
            bool hasNearby = NavMesh.SamplePosition(to, out NavMeshHit nearby, 100f, NavMesh.AllAreas);
            bool calculated = hasStart && hasEnd && NavMesh.CalculatePath(a.position, b.position, NavMesh.AllAreas, path);
            Vector3 terminal = calculated && path.corners.Length > 0 ? path.corners[path.corners.Length - 1] : Vector3.zero;
            diagnostic = $"start={hasStart}:{(hasStart ? a.position.ToString() : "none")}, " +
                         $"end={hasEnd}:{(hasEnd ? b.position.ToString() : "none")}, " +
                         $"nearest={hasNearby}:{(hasNearby ? nearby.position.ToString() : "none")}, " +
                         $"path={(calculated ? path.status.ToString() : "not calculated")}:{terminal}";
            bool complete = calculated && path.status == NavMeshPathStatus.PathComplete;
            return complete;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float seconds, string message)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.IsTrue(condition(), message);
        }
    }
}
