using System.Collections;
using System.Linq;
using Abandoned.Core;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Player;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static Abandoned.Tests.PlayerTestRig;

namespace Abandoned.Tests
{
    /// <summary>The M2 exit test, automated: TestBuilding's weak spots and dragging a rack through a rotten floor.</summary>
    public class TestBuildingStructureTests
    {
        private static StructuralSection Section(string name) =>
            Object.FindObjectsByType<StructuralSection>(FindObjectsSortMode.None).First(s => s.name == name);

        private static IEnumerator WaitFixed(float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
        }

        [UnityTest]
        public IEnumerator BuildingIsStableAtRestButTheWeakSpotsWarn()
        {
            yield return SceneManager.LoadSceneAsync("TestBuilding", LoadSceneMode.Single);
            var sim = Object.FindFirstObjectByType<StructureSimulation>();
            Assert.AreEqual(14 + 20 + 2, sim.Sections.Count, "every tile and stair segment is a section");
            Assert.IsTrue(sim.Sections.Where(s => s.name.StartsWith("Tile_G")).All(s => !s.CanCollapse), "ground floor can't collapse");
            Assert.IsTrue(sim.Sections.Where(s => s.name.StartsWith("StairSegment")).All(s => !s.CanCollapse),
                "the stairs are the only way down, so they can't fall (GDD 6.4)");
            Assert.IsTrue(sim.Sections.Where(s => s.name.StartsWith("Tile_U") || s.name.StartsWith("Balcony")).All(s => s.CanCollapse));

            yield return WaitFixed(5f);
            Assert.AreEqual(0, sim.CollapseCount, "nothing falls on its own");
            Assert.GreaterOrEqual(Section("Balcony_U_3_2").Stage, StructuralStage.Stressed, "statue balcony creaks");
            Assert.GreaterOrEqual(Section("Balcony_U_0_1").Stage, StructuralStage.Stressed, "piano balcony creaks");
            Assert.Greater(Section("Balcony_U_3_2").Load, 1900f, "the statue rests on it");

            ScreenshotCapture.CaptureFrom(new Vector3(10f, 9f, 2f), new Vector3(10f, 3f, 10f), "M2_testbuilding_warnings");
        }

        [UnityTest]
        public IEnumerator DraggingTheServerRackBreaksThroughTheRottenFloor()
        {
            yield return SceneManager.LoadSceneAsync("TestBuilding", LoadSceneMode.Single);
            var rig = ForExisting(Object.FindFirstObjectByType<PlayerMotor>().gameObject);
            var carrier = rig.Player.GetComponent<PlayerCarrier>();
            LootItem rack = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None)
                .First(l => l.Definition.Id == "server_rack" && l.transform.position.y > 3f);
            StructuralSection rotten = Section("Tile_U_3_3");

            // Stand east of the rack on the stair-arrival tile, facing west along the north wall.
            rig.Teleport(new Vector3(19.4f, 4.05f, rack.transform.position.z), 270f);
            yield return WaitFixed(0.5f);
            rig.Motor.Simulate(Frame(), Time.fixedDeltaTime);
            InteractionService.Handler.RequestPickup(carrier, rack.GetComponent<Grabbable>());
            Assert.IsTrue(carrier.IsDragging, $"couldn't drag the rack: {carrier.Hint}");

            bool screenshot = false;
            for (float t = 0f; t < 30f && !rotten.IsCollapsed; t += Time.fixedDeltaTime)
            {
                // Drag until the rack is over the rotten tile, then stand there and listen to it go.
                bool keepDragging = rack.transform.position.x > rotten.SurfaceBounds.center.x;
                if (!rig.Player.GetComponent<PlayerRagdoll>().IsRagdolled)
                    rig.Motor.Simulate(Frame(keepDragging ? Vector2.up : Vector2.zero), Time.fixedDeltaTime);
                if (!screenshot && rotten.Stage >= StructuralStage.Cracking)
                {
                    screenshot = true;
                    ScreenshotCapture.CaptureFrom(rig.Player.transform.position + new Vector3(3f, 3f, -4f),
                        rotten.SurfaceBounds.center, "M2_rack_cracking_floor");
                }
                yield return new WaitForFixedUpdate();
            }
            Assert.IsTrue(rotten.IsCollapsed, $"rotten tile ended at {rotten.Stage}, load {rotten.Load:0}/{rotten.Capacity:0}");
            yield return WaitFixed(1.5f);
            Assert.Less(rack.transform.position.y, 2.5f, "the rack fell through to the ground floor");
            ScreenshotCapture.CaptureFrom(new Vector3(12f, 8f, 9f), new Vector3(14f, 1f, 14f), "M2_rack_fell_through");
        }

        [UnityTest]
        public IEnumerator LoweringStabilityBringsTheStatueDown()
        {
            yield return SceneManager.LoadSceneAsync("TestBuilding", LoadSceneMode.Single);
            var sim = Object.FindFirstObjectByType<StructureSimulation>();
            StructuralSection balcony = Section("Balcony_U_3_2");
            yield return WaitFixed(1f);
            sim.ApplyStability(0.3f, sim.Seed);
            for (float t = 0f; t < 40f && !balcony.IsCollapsed; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            Assert.IsTrue(balcony.IsCollapsed, "a 2-ton statue on a 30%-stability balcony should not hold");
            LootItem statue = Object.FindObjectsByType<LootItem>(FindObjectsSortMode.None).First(l => l.Definition.Id == "marble_statue");
            yield return WaitFixed(2f);
            Assert.Less(statue.transform.position.y, 2.5f, "into the atrium");
            Assert.Greater(statue.CurrentValue, 0, "the statue survives the fall, damaged");
            ScreenshotCapture.CaptureFrom(new Vector3(6f, 7f, 2f), new Vector3(12f, 1f, 9f), "M2_statue_in_atrium");
        }
    }
}
