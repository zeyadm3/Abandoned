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
    /// <summary>Logical load model: what weighs on which section.</summary>
    public class StructureLoadTests
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
            // A real LootItem is itself a load source: while held it must not count twice.
            LootItem tv = LootDamageTests.SpawnLoot("flatscreen_tv", carrier.HoldPoint);
            yield return null;
            InteractionService.Handler.RequestPickup(carrier, tv.GetComponent<Grabbable>());
            rig.Simulation.SolveLoads();
            Assert.AreEqual(body + tv.GameplayWeight, rig.Simulation.LoadOn(tile), 0.01f, "held weight goes through the feet, once");
        }

        [UnityTest]
        public IEnumerator ADraggedItemRestsItsOwnWeightAndTheCarrierOnlyTheirs()
        {
            StructuralSection near = rig.AddTile("Near", new Vector3(0f, 4f, 0f));
            StructuralSection far = rig.AddTile("Far", new Vector3(0f, 4f, 4f));
            rig.StartSimulation();
            players = new PlayerTestRig();
            players.SpawnPlayer(new Vector3(0f, 4.05f, 0.5f), Quaternion.identity);
            players.Settle();
            LootItem rack = LootDamageTests.SpawnLoot("server_rack", new Vector3(0f, 5.05f, 3.2f));
            yield return WaitFixed(0.8f);
            var carrier = players.Player.GetComponent<PlayerCarrier>();
            InteractionService.Handler.RequestPickup(carrier, rack.GetComponent<Grabbable>());
            Assert.IsTrue(carrier.IsDragging);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(players.Motor.Config.BodyWeight, rig.Simulation.LoadOn(near), 0.01f);
            Assert.AreEqual(rack.GameplayWeight, rig.Simulation.LoadOn(far), 0.01f);
        }

        [UnityTest]
        public IEnumerator ARagdolledPlayerStillWeighsOnTheFloor()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            rig.StartSimulation();
            players = new PlayerTestRig();
            players.SpawnPlayer(Upper + Vector3.up * 0.05f, Quaternion.identity);
            players.Settle();
            players.Player.GetComponent<PlayerRagdoll>().Enter(Vector3.zero);
            yield return WaitFixed(1.2f);
            rig.Simulation.SolveLoads();
            Assert.AreEqual(players.Motor.Config.BodyWeight, rig.Simulation.LoadOn(tile), 0.01f);
        }

        [UnityTest]
        public IEnumerator ALongItemOnARampLoadsTheRampNotTheFloorBelow()
        {
            StructuralSection below = rig.AddTile("Below", new Vector3(0f, 0.3f, 0f), collapsible: false, presentation: false);
            StructuralSection ramp = rig.AddTile("Ramp", new Vector3(0f, 2.3f, 0f), SectionType.Stair, tiltDegrees: -25f,
                presentation: false, colliderOnChild: true);
            rig.StartSimulation();
            LootItem piano = LootDamageTests.SpawnLoot("grand_piano", new Vector3(0f, 3.8f, 0f));
            piano.GetComponent<Rigidbody>().isKinematic = true;
            piano.transform.rotation = Quaternion.Euler(-25f, 0f, 0f);
            Physics.SyncTransforms();
            yield return null;
            piano.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            rig.Simulation.SolveLoads();
            Assert.AreEqual(piano.GameplayWeight, rig.Simulation.LoadOn(ramp), 0.5f);
            Assert.AreEqual(0f, rig.Simulation.LoadOn(below), 0.5f);
        }

    }
}
