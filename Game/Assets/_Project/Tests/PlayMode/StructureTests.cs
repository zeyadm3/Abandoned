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
    /// <summary>Logical load, stages, collapse, cascades, can-collapse, determinism and stability.</summary>
    public class StructureTests
    {
        private StructureTestRig rig;
        private PlayerTestRig players;

        // An upper-floor tile 4 m up, with the ground below.
        private static readonly Vector3 Upper = new(0f, 4f, 0f);

        [SetUp]
        public void SetUp() => rig = StructureTestRig.Create();

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

        // ---------- Load model ----------

        [Test]
        public void AWeightOnTheTileIsItsLoad()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            TestLoad.Create(Upper + Vector3.up * 0.05f, 500f);
            rig.Step();
            Assert.AreEqual(500f, tile.Load, 1e-3f);
        }

        [UnityTest]
        public IEnumerator RestingLootLoadsTheTileWithItsGameplayWeight()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper, capacity: 1f);
            rig.StartSimulation();
            LootItem statue = LootDamageTests.SpawnLoot("marble_statue", Upper + Vector3.up * 1.15f);
            yield return WaitFixed(1f);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(statue.Definition.GameplayWeight, rig.Simulation.LoadOn(tile), 1f,
                "2000 kg gameplay weight, not the 80 kg clamped Rigidbody mass");
        }

        [UnityTest]
        public IEnumerator FallingLootDoesNotLoadUntilItRests()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("laptop", Upper + Vector3.up * 3f);
            yield return WaitFixed(0.15f);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(0f, rig.Simulation.LoadOn(tile), "still in the air");
            yield return WaitFixed(1.5f);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(2.5f, rig.Simulation.LoadOn(tile), 0.01f);
        }

        [UnityTest]
        public IEnumerator AnItemAcrossTwoTilesLoadsBoth()
        {
            StructuralSection west = rig.AddTile("West", new Vector3(-2f, 4f, 0f));
            StructuralSection east = rig.AddTile("East", new Vector3(2f, 4f, 0f));
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("grand_piano", new Vector3(0.1f, 4.55f, 0f));
            yield return WaitFixed(1f);
            rig.Simulation.SolveLoads();
            float w = rig.Simulation.LoadOn(west), e = rig.Simulation.LoadOn(east);
            Assert.Greater(w, 0f);
            Assert.Greater(e, 0f);
            Assert.AreEqual(500f, w + e, 0.5f, "weight is split, not duplicated");
        }

        [UnityTest]
        public IEnumerator APlayerLoadsTheirBodyPlusWhatTheyCarry()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            players = new PlayerTestRig();
            players.SpawnPlayer(Upper + Vector3.up * 0.05f, Quaternion.identity);
            players.Settle();
            yield return null;

            rig.Simulation.SolveLoads();
            float body = players.Motor.Config.BodyWeight;
            Assert.AreEqual(body, rig.Simulation.LoadOn(tile), 0.01f);

            var carrier = players.Player.GetComponent<PlayerCarrier>();
            Grabbable tv = TestCarryable.Create(carrier.HoldPoint, CarryClass.TwoHand, 15f);
            players.Track(tv.gameObject);
            yield return null;
            InteractionService.Handler.RequestPickup(carrier, tv);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(body + 15f, rig.Simulation.LoadOn(tile), 0.01f, "held weight goes through the feet, once");
        }

        // ---------- Stages, failing and collapse ----------

        [Test]
        public void OverloadGoesThroughEveryStageInOrderThenCollapsesAfterTheFailingTime()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            var stages = new List<StructuralStage>();
            tile.StageChanged += (s, _) => stages.Add(s.Stage);
            TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 3f);

            int toFailing = rig.StepUntil(() => tile.Stage == StructuralStage.Failing, 60f);
            Assert.GreaterOrEqual(toFailing, 0, "never started failing");
            int failingSteps = rig.StepUntil(() => tile.IsCollapsed, 10f);

            CollectionAssert.AreEqual(new[] { StructuralStage.Stressed, StructuralStage.Cracking, StructuralStage.Failing, StructuralStage.Collapsed }, stages);
            Assert.AreEqual(rig.Config.FailingDuration, failingSteps * StructureTestRig.Dt, StructureTestRig.Dt * 1.5f,
                "~2 s warning between Failing and Collapsed");
        }

        [Test]
        public void UnderCapacityNeverDegrades()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 0.8f);
            rig.Step(3000); // 60 s
            Assert.AreEqual(tile.MaxHealth, tile.Health, 1e-3f);
            Assert.AreEqual(StructuralStage.Stable, tile.Stage);
        }

        [Test]
        public void RemovingTheLoadStopsTheDamageButDoesNotHeal()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            TestLoad load = TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 2f);
            rig.Step(100);
            float damaged = tile.Health;
            Assert.Less(damaged, tile.MaxHealth);
            load.weight = 0f;
            rig.Step(500);
            Assert.AreEqual(damaged, tile.Health, 1e-3f);
        }

        [UnityTest]
        public IEnumerator CollapseSwitchesCollidersOffAndWhatWasOnItFalls()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            LootItem laptop = LootDamageTests.SpawnLoot("laptop", Upper + Vector3.up * 0.1f);
            yield return WaitFixed(0.5f);
            tile.Collapse();
            Assert.IsTrue(tile.IsCollapsed);
            foreach (Collider c in tile.GetComponentsInChildren<Collider>()) Assert.IsFalse(c.enabled);
            yield return WaitFixed(1.5f);
            Assert.Less(laptop.transform.position.y, 1f, "the laptop should have fallen to the floor below");
        }

        [UnityTest]
        public IEnumerator APlayerStandingOnACollapseGoesDownWithIt()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            players = new PlayerTestRig();
            players.SpawnPlayer(Upper + Vector3.up * 0.05f, Quaternion.identity);
            players.Settle();
            yield return null;
            tile.Collapse();
            Assert.IsTrue(players.Player.GetComponent<PlayerRagdoll>().IsRagdolled);
            yield return WaitFixed(1.5f);
            Assert.Less(players.Player.GetComponent<PlayerRagdoll>().Pelvis.position.y, 1.5f);
        }

        [Test]
        public void ANonCollapsibleSectionCracksButNeverFalls()
        {
            StructuralSection ground = rig.AddTile("GroundTile", new Vector3(0f, 0.3f, 0f), collapsible: false);
            rig.StartSimulation();
            TestLoad.Create(new Vector3(0f, 0.35f, 0f), ground.Capacity * 10f);
            for (int i = 0; i < 20; i++) ground.ApplyImpact(100000f);
            rig.Step(3000);
            Assert.AreNotEqual(StructuralStage.Failing, ground.Stage);
            Assert.IsFalse(ground.IsCollapsed);
            Assert.Greater(ground.Health, 0f);
            Assert.AreEqual(StructuralStage.Cracking, ground.Stage);
        }

        // ---------- Impacts and cascades ----------

        [UnityTest]
        public IEnumerator DroppingASafeDamagesTheFloorButALaptopDoesNot()
        {
            StructuralSection left = rig.AddTile("Left", new Vector3(-2f, 4f, 0f));
            StructuralSection right = rig.AddTile("Right", new Vector3(2f, 4f, 0f));
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("safe", new Vector3(-2f, 6.5f, 0f));
            LootDamageTests.SpawnLoot("laptop", new Vector3(2f, 6.5f, 0f));
            yield return WaitFixed(1.5f);
            Assert.Less(left.Health, left.MaxHealth, "400 kg from 2 m is a real hit");
            Assert.AreEqual(right.MaxHealth, right.Health, 1e-3f, "2.5 kg is below the impact threshold");
        }

        [Test]
        public void ACollapsingSectionDamagesTheOneBelowByTheRules()
        {
            StructuralSection lower = rig.AddTile("Lower", new Vector3(0f, 0.3f, 0f));
            StructuralSection upper = rig.AddTile("Upper", Upper);
            rig.StartSimulation();
            float before = lower.Health;
            upper.Collapse();

            // From just under the upper slab (4 - 0.3 - 0.05) down to the lower surface (0.3).
            float drop = 4f - 0.3f - 0.05f - 0.3f;
            float momentum = rig.Config.Profile(SectionType.Floor).SelfMass * rig.Config.CascadeFactor * StructureMath.FallSpeed(drop);
            float expected = StructureMath.ImpactDamage(momentum, rig.Config.ImpactThreshold, rig.Config.ImpactDamagePerMomentum);
            Assert.Greater(expected, 0f);
            Assert.AreEqual(Mathf.Min(before, expected), before - lower.Health, 0.5f);
        }

        [Test]
        public void HeavyCascadesChainDownwards()
        {
            StructuralSection bottom = rig.AddTile("Bottom", new Vector3(0f, 0.3f, 0f), health: 0.2f);
            StructuralSection middle = rig.AddTile("Middle", new Vector3(0f, 4f, 0f), health: 0.05f);
            StructuralSection top = rig.AddTile("Top", new Vector3(0f, 8f, 0f));
            rig.StartSimulation();
            top.Collapse();
            Assert.AreEqual(StructuralStage.Failing, middle.Stage, "the weak middle floor is knocked into failing");
            rig.StepUntil(() => middle.IsCollapsed, 5f);
            Assert.IsTrue(middle.IsCollapsed);
            Assert.Less(bottom.Health, bottom.MaxHealth * 0.2f, "and it lands on the bottom floor");
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
            Assert.IsTrue(System.Array.Exists(first, h => h < 100f), "20% stability pre-damages something");
            Assert.AreEqual(ground.MaxHealth, ground.Health, "can't-collapse sections are never pre-damaged");

            sim.ApplyStability(0.2f, 12);
            CollectionAssert.AreNotEqual(first, Healths(), "new seed, new damage");

            sim.ApplyStability(1f, 12);
            Assert.IsTrue(tiles.TrueForAll(t => t.Health == t.MaxHealth), "100% stability: nothing pre-damaged");
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
