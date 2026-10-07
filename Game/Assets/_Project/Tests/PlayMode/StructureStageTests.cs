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
    /// <summary>Stages, failing, collapse, impacts and cascades.</summary>
    public class StructureStageTests
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
            rig.Step(); // impacts reported during physics are applied (shared) at the next step
            Assert.Less(left.Health, left.MaxHealth, "400 kg from 2 m is a real hit");
            Assert.AreEqual(right.MaxHealth, right.Health, 1e-3f, "2.5 kg is below the impact threshold");
        }

        [UnityTest]
        public IEnumerator StairSectionsWithChildCollidersTakeImpacts()
        {
            StructuralSection stair = rig.AddTile("Stair", Upper, SectionType.Stair, colliderOnChild: true);
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("safe", Upper + Vector3.up * 2.5f);
            yield return WaitFixed(1.5f);
            rig.Step();
            Assert.Less(stair.Health, stair.MaxHealth, "the ramp's collider is on a child; the relay must forward the hit");
        }

        [UnityTest]
        public IEnumerator ALandingOnASeamIsSharedNotDoubled()
        {
            StructuralSection solo = rig.AddTile("Solo", new Vector3(-12f, 4f, 0f));
            StructuralSection west = rig.AddTile("West", new Vector3(10f, 4f, 0f));
            StructuralSection east = rig.AddTile("East", new Vector3(14f, 4f, 0f));
            rig.StartSimulation();
            LootDamageTests.SpawnLoot("safe", new Vector3(-12f, 6.5f, 0f));
            LootDamageTests.SpawnLoot("safe", new Vector3(12f, 6.5f, 0f));
            yield return WaitFixed(1.5f);
            rig.Step();
            float single = solo.MaxHealth - solo.Health;
            float split = (west.MaxHealth - west.Health) + (east.MaxHealth - east.Health);
            Assert.Greater(single, 0f);
            Assert.AreEqual(single, split, single * 0.35f, "one landing on a seam does about one landing's damage in total");
        }

        [UnityTest]
        public IEnumerator LandingPlayersDamageTheFloor()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            players = new PlayerTestRig();
            players.SpawnPlayer(Upper + Vector3.up * 3.5f, Quaternion.identity);
            players.Motor.Simulate(PlayerTestRig.Frame(), PlayerTestRig.Dt);
            players.RunUntil(PlayerTestRig.Frame(), () => players.Motor.IsGrounded, 3f);
            yield return null;
            Assert.Less(tile.Health, tile.MaxHealth, "80 kg landing from 3.5 m is an impact (GDD 6.1)");
        }

        [Test]
        public void OneHitCannotSkipTheWarnings()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            tile.ApplyImpact(1_000_000f);
            Assert.AreEqual(StructuralStage.Cracking, tile.Stage, "a healthy floor shows cracks before it can fail");
            tile.ApplyImpact(1_000_000f);
            Assert.AreEqual(StructuralStage.Failing, tile.Stage, "the next big hit finishes it");
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
            // A Stable section keeps a sliver above Failing from one hit (warnings first).
            float cap = before - lower.MaxHealth * rig.Config.WarningFloor;
            Assert.AreEqual(Mathf.Min(cap, expected), before - lower.Health, 0.5f);
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

    }
}
