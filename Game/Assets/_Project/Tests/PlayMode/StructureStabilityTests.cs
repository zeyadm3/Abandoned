using System.Collections;
using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Player;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>Determinism, stability scaling, seeded pre-damage, re-roll and noise.</summary>
    public class StructureStabilityTests
    {
        private StructureTestRig rig;
        private PlayerTestRig players;

        // An upper-floor tile 4 m up, with the ground below.
        private static readonly Vector3 Upper = new(0f, 4f, 0f);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // A level left loaded by an earlier test (its structure, its load sources) would get in the way.
            yield return NetTestHarness.CleanWorld();
            rig = StructureTestRig.Create();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (LootItem item in Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None))
                Object.DestroyImmediate(item.gameObject);
            foreach (TestLoad load in Object.FindObjectsByType<TestLoad>(FindObjectsSortMode.None))
                Object.DestroyImmediate(load.gameObject);
            players?.Destroy();
            players = null;
            rig.Destroy();
            yield return null;
        }

        private static IEnumerator WaitFixed(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
        }

        // ---------- Determinism, stability, can-collapse flag ----------

        [Test]
        public void SameLoadSameDamageSameResult()
        {
            (List<float> health, int collapseStep) Run()
            {
                StructureTestRig r = StructureTestRig.Create();
                StructuralSection tile = r.AddTile("Tile", Upper, presentation: false);
                r.StartSimulation(0.7f, 42);
                TestLoad load = TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 1.6f);
                var trace = new List<float>();
                int collapsedAt = -1;
                for (int i = 0; i < 2000 && collapsedAt < 0; i++)
                {
                    r.Step();
                    trace.Add(tile.Health);
                    if (tile.IsCollapsed) collapsedAt = i;
                }
                Object.DestroyImmediate(load.gameObject);
                r.Destroy();
                return (trace, collapsedAt);
            }

            var a = Run();
            var b = Run();
            Assert.GreaterOrEqual(a.collapseStep, 0);
            Assert.AreEqual(a.collapseStep, b.collapseStep);
            CollectionAssert.AreEqual(a.health, b.health);
        }

        [Test]
        public void StabilityScalesCapacityAndPreDamageFollowsTheSeed()
        {
            var tiles = new List<StructuralSection>();
            for (int i = 0; i < 12; i++) tiles.Add(rig.AddTile($"Tile_{i:00}", new Vector3(i * 4f, 4f, 0f), presentation: false));
            StructuralSection ground = rig.AddTile("Tile_Ground", new Vector3(0f, 0.3f, 20f), collapsible: false, presentation: false);
            StructureSimulation sim = rig.StartSimulation(0.2f, 11);

            float expectedCapacity = rig.Config.Profile(SectionType.Floor).Capacity * rig.Config.CapacityScale(0.2f);
            Assert.AreEqual(expectedCapacity, tiles[0].Capacity, 0.01f);

            float[] Healths() => tiles.ConvertAll(t => t.Health).ToArray();
            float[] first = Healths();
            sim.ApplyStability(0.2f, 11);
            CollectionAssert.AreEqual(first, Healths(), "same seed, same pre-damage");
            Assert.IsTrue(tiles.Exists(t => t.Health < t.MaxHealth), "20% stability pre-damages something");
            Assert.AreEqual(ground.MaxHealth, ground.Health, "can't-collapse sections are never pre-damaged");

            sim.ApplyStability(0.2f, 12);
            CollectionAssert.AreNotEqual(first, Healths(), "new seed, new damage");

            sim.ApplyStability(1f, 12);
            Assert.IsTrue(tiles.TrueForAll(t => t.Health == t.MaxHealth), "100% stability: nothing pre-damaged");
        }

        [Test]
        public void PreDamageNeverStartsASectionFailing()
        {
            var tiles = new List<StructuralSection>();
            for (int i = 0; i < 10; i++)
                tiles.Add(rig.AddTile($"Weak_{i}", new Vector3(i * 4f, 4f, 0f), health: 0.1f + i * 0.05f, presentation: false));
            StructureSimulation sim = rig.StartSimulation(0f, 1);
            for (int seed = 1; seed < 60; seed++)
            {
                sim.ApplyStability(0f, seed);
                foreach (StructuralSection t in tiles)
                    Assert.AreNotEqual(StructuralStage.Failing, t.Stage, $"{t.name} would collapse unprovoked (seed {seed})");
            }
        }

        [Test]
        public void ReRollRestoresCollapsedSections()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            StructureSimulation sim = rig.StartSimulation();
            tile.Collapse();
            sim.ApplyStability(1f, 2);
            Assert.IsFalse(tile.IsCollapsed);
            foreach (Collider c in tile.GetComponentsInChildren<Collider>()) Assert.IsTrue(c.enabled);
        }

        // ---------- Noise ----------

        [Test]
        public void StressedSectionsCreakAndCollapsesAreLoud()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 1.5f);
            int before = NoiseSystem.TotalEmitted;
            rig.Step(Mathf.CeilToInt(rig.Config.CreakInterval / StructureTestRig.Dt) + 5);
            Assert.Greater(NoiseSystem.TotalEmitted, before);
            Assert.IsTrue(NoiseSystem.LastOf(NoiseSource.Creak).HasValue);

            tile.Collapse();
            NoiseEvent? collapse = NoiseSystem.LastOf(NoiseSource.Collapse);
            Assert.IsTrue(collapse.HasValue);
            Assert.AreEqual(rig.Config.CollapseLoudness, collapse.Value.Loudness, 1e-4f);
        }

        [UnityTest]
        public IEnumerator DroppedLootAndFootstepsMakeNoise()
        {
            rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("server_rack", Upper + Vector3.up * 2.5f);
            yield return WaitFixed(1f);
            Assert.IsTrue(NoiseSystem.LastOf(NoiseSource.LootImpact).HasValue);

            players = new PlayerTestRig();
            players.SpawnPlayer(new Vector3(0f, 0.05f, 8f), Quaternion.identity);
            players.Settle();
            var steps = players.Player.GetComponent<PlayerFootsteps>();
            for (int i = 0; i < 90; i++)
            {
                players.Motor.Simulate(PlayerTestRig.Frame(Vector2.up), PlayerTestRig.Dt);
                steps.Tick();
            }
            NoiseEvent? step = NoiseSystem.LastOf(NoiseSource.Footstep);
            Assert.IsTrue(step.HasValue);
            Assert.AreEqual(players.Motor.Config.WalkNoise, step.Value.Loudness, 1e-4f);
        }
    }
}
