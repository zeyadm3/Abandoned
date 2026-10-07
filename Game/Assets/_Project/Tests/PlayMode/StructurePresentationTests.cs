using System.Collections;
using System.Linq;
using Abandoned.Audio;
using Abandoned.Core;
using Abandoned.Structure;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>What each stage looks and sounds like, and the pre-fractured collapse.</summary>
    public class StructurePresentationTests
    {
        private StructureTestRig rig;
        private static readonly Vector3 Upper = new(0f, 4f, 0f);

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            // A level loaded by an earlier test (the mall's sections, its run) would sound and break here too.
            yield return NetTestHarness.CleanWorld();
            rig = StructureTestRig.Create();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (TestLoad load in Object.FindObjectsByType<TestLoad>(FindObjectsSortMode.None))
                Object.DestroyImmediate(load.gameObject);
            rig.Destroy();
            yield return null;
        }

        [UnityTest]
        public IEnumerator EachStageChangesTintCracksDustAndSound()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            var view = tile.GetComponent<SectionPresentation>();
            Renderer visual = tile.Visual.GetComponent<Renderer>();
            rig.StartSimulation();
            yield return null;
            Assert.AreEqual(0, view.CrackCount);
            Assert.AreEqual(0f, view.DustRate);

            TestLoad.Create(Upper + Vector3.up * 0.05f, tile.Capacity * 3f);
            int sounds = GameAudio.StructureSoundCount;
            rig.StepUntil(() => tile.Stage == StructuralStage.Cracking, 30f);
            yield return null;
            Assert.AreEqual(rig.Visuals.CracksWhenCracking, view.CrackCount);
            Assert.AreEqual(rig.Visuals.CrackingDust, view.DustRate, 1e-3f);
            Assert.Greater(GameAudio.StructureSoundCount, sounds, "getting worse is announced");
            var block = new MaterialPropertyBlock();
            visual.GetPropertyBlock(block);
            Assert.AreNotEqual(Color.white, block.GetColor("_BaseColor"), "tinted");

            rig.StepUntil(() => tile.Stage == StructuralStage.Failing, 30f);
            rig.Step(Mathf.RoundToInt(rig.Config.FailingDuration * 0.8f / StructureTestRig.Dt));
            yield return null;
            Assert.AreEqual(rig.Visuals.CracksWhenFailing, view.CrackCount);
            Assert.Less(tile.Visual.localPosition.y, -0.15f - rig.Visuals.SagDepth * 0.5f, "sagging before it goes");
            Assert.AreEqual(StructureSound.Snap, GameAudio.LastStructureSound);
        }

        [UnityTest]
        public IEnumerator CracksAreRebuiltAfterAReRoll()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper, health: 0.3f);
            var view = tile.GetComponent<SectionPresentation>();
            StructureSimulation sim = rig.StartSimulation();
            yield return null;
            Assert.AreEqual(rig.Visuals.CracksWhenCracking, view.CrackCount);
            sim.ApplyStability(1f, 2);
            Assert.AreEqual(rig.Visuals.CracksWhenCracking, view.CrackCount, "re-roll back to Cracking keeps its cracks");
        }

        [UnityTest]
        public IEnumerator CollapseSwapsToCosmeticDebrisThatCleansItselfUp()
        {
            StructuralSection tile = rig.AddTile("Tile", Upper);
            var view = tile.GetComponent<SectionPresentation>();
            rig.StartSimulation();
            yield return null;
            int before = DebrisSpawner.LiveChunks;
            tile.Collapse();

            Assert.IsFalse(tile.Visual.GetComponent<Renderer>().enabled, "intact mesh hidden");
            Assert.IsNotNull(view.Debris);
            Rigidbody[] chunks = view.Debris.GetComponentsInChildren<Rigidbody>();
            Assert.Greater(chunks.Length, 10, "pre-fractured into many chunks");
            int debrisLayer = GameLayers.DebrisLayer;
            Assert.IsTrue(chunks.All(c => c.gameObject.layer == debrisLayer), "debris is on its own layer");
            Assert.AreEqual(before + chunks.Length, DebrisSpawner.LiveChunks);
            Assert.AreEqual(StructureSound.Crash, GameAudio.LastStructureSound);

            for (float t = 0f; t < 1.5f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            float meanY = chunks.Average(c => c.position.y);
            Assert.Less(meanY, Upper.y - 2f, $"chunks should have fallen (mean y {meanY:F2})");
            ScreenshotCapture.CaptureFrom(new Vector3(6f, 5f, -6f), new Vector3(0f, 1f, 0f), "M2_collapse_debris");

            float wait = rig.Config.DebrisLifetime - 1.5f + 0.5f;
            for (float t = 0f; t < wait; t += Time.deltaTime) yield return null;
            Assert.IsTrue(view.Debris == null, "debris removed after its lifetime");
            Assert.AreEqual(before, DebrisSpawner.LiveChunks);
        }

        [Test]
        public void SameCollapseSeedSameDebrisStart()
        {
            StructuralSection a = rig.AddTile("A", Upper);
            StructuralSection b = rig.AddTile("B", Upper + Vector3.right * 10f);
            rig.StartSimulation();
            GameObject da = DebrisSpawner.Spawn(a, rig.FracturedTile, null, rig.Visuals, 1234);
            GameObject db = DebrisSpawner.Spawn(b, rig.FracturedTile, null, rig.Visuals, 1234);
            Vector3[] va = da.GetComponentsInChildren<Rigidbody>().Select(r => r.linearVelocity).ToArray();
            Vector3[] vb = db.GetComponentsInChildren<Rigidbody>().Select(r => r.linearVelocity).ToArray();
            Assert.AreEqual(va.Length, vb.Length);
            for (int i = 0; i < va.Length; i++)
                Assert.Less((va[i] - vb[i]).magnitude, 1e-4f, "every machine plays the same break from the same seed");
            Object.DestroyImmediate(da);
            Object.DestroyImmediate(db);
        }

        [UnityTest]
        public IEnumerator DebrisNeverBlocksPlayersOrLoot()
        {
            int debris = GameLayers.DebrisLayer;
            yield return null;
            Assert.IsTrue(Physics.GetIgnoreLayerCollision(debris, GameLayers.PlayerLayer));
            Assert.IsTrue(Physics.GetIgnoreLayerCollision(debris, GameLayers.LootLayer));
            Assert.IsFalse(Physics.GetIgnoreLayerCollision(debris, GameLayers.StructureLayer), "but it does land on floors");
        }

        [UnityTest]
        public IEnumerator DebrisDoesNotDamageSections()
        {
            StructuralSection target = rig.AddTile("Target", new Vector3(0f, 0.3f, 0f));
            rig.StartSimulation();
            float before = target.Health;

            // A heavy debris chunk and an identical non-debris block, both dropped from 4 m.
            GameObject Drop(string name, Vector3 at, int layer)
            {
                GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
                block.name = name;
                block.layer = layer;
                block.transform.position = at;
                block.AddComponent<Rigidbody>().mass = 80f;
                return block;
            }
            GameObject chunk = Drop("Chunk", new Vector3(-1f, 4.5f, 0f), GameLayers.DebrisLayer);
            for (float t = 0f; t < 1.5f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            Assert.AreEqual(0.3f + 0.5f, chunk.transform.position.y, 0.15f, "the chunk is resting on the section, so it really hit it");
            rig.Step(); // apply anything reported during physics
            Assert.AreEqual(before, target.Health, "debris never damages structure");

            GameObject solid = Drop("Solid", new Vector3(1f, 4.5f, 0f), 0);
            for (float t = 0f; t < 1.5f; t += Time.fixedDeltaTime) yield return new WaitForFixedUpdate();
            rig.Step();
            Assert.Less(target.Health, before, "control: the same hit from a non-debris object does damage");
            Object.DestroyImmediate(chunk);
            Object.DestroyImmediate(solid);
        }
    }
}
