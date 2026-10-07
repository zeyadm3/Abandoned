using System.Collections;
using System.Linq;
using Abandoned.Loot;
using Abandoned.Networking;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>The mall fills itself with a seeded run of loot as soon as the solo host starts.</summary>
    public class MallLootTests
    {
        [UnitySetUp]
        public IEnumerator SetUp() => TestBuildingScene.Load("Mall", randomRun: true);

        [UnityTest]
        public IEnumerator TheHostSpawnsASeededRunOfLootWithOneOrTwoJackpots()
        {
            NetworkLootSpawner spawner = Object.FindAnyObjectByType<NetworkLootSpawner>();
            Assert.IsNotNull(spawner);
            for (int i = 0; i < 30 && spawner.Spawned.Count == 0; i++) yield return null;
            int points = spawner.Points().Count;
            Assert.Greater(points, 80, "spawn points across every store");
            Assert.Greater(spawner.Spawned.Count, points / 3, "most of the building has something in it");
            Assert.IsTrue(spawner.Spawned.All(l => l.IsSpawned && l.Item.IsInitialized), "network objects with rolled values");
            int jackpots = spawner.Spawned.Count(l => l.Item.Definition.Jackpot);
            Assert.That(jackpots, Is.InRange(1, 2));
            Assert.GreaterOrEqual(spawner.Spawned.Select(l => l.Item.Definition).Distinct().Count(), 15, "variety");
            Assert.AreNotEqual(1, spawner.Seed, "each session's first run rolls its own seed");

            // Every point can get something, and every item has somewhere to appear.
            var definitions = spawner.Points().Count > 0 ? Resources.FindObjectsOfTypeAll<LootDefinition>().Where(d => d.SpawnTags != null && d.SpawnTags.Length > 0).ToList() : null;
            foreach (LootSpawnPoint p in spawner.Points())
                Assert.IsTrue(definitions.Any(p.Fits), $"{p.name} ({p.Tag}, {p.MinClass}-{p.MaxClass}) can never be filled");
            foreach (LootDefinition d in definitions)
                Assert.IsTrue(spawner.Points().Any(p => p.Fits(d)), $"{d.Id} can never spawn in the mall");

            // Let everything settle: nothing falls through the floors or out of the building.
            yield return new WaitForSeconds(2f);
            foreach (NetworkLoot l in spawner.Spawned.Where(l => l != null))
                Assert.Greater(l.transform.position.y, -0.5f, $"{l.name} fell through the world");
        }
    }
}
