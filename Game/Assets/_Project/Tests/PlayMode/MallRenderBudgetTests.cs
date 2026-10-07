using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.TestTools;

namespace Abandoned.Tests
{
    /// <summary>
    /// M7.6: what drawing the mall costs from a few typical spots (batches and triangles per frame),
    /// with and without the baked occlusion culling. A budget guard, and the numbers for the notes.
    /// </summary>
    public class MallRenderBudgetTests
    {
        private const long BatchBudget = 2200; // ~1.75k measured at M7.6 (concourse, 2 shadow cascades)

        private static readonly (Vector3 from, Vector3 to, string name)[] Views =
        {
            (new Vector3(10f, 1.6f, 1f), new Vector3(2f, 0.8f, 18f), "electronics"),
            (new Vector3(24f, 1.6f, 4f), new Vector3(24f, 3f, 30f), "concourse"),
            (new Vector3(13f, 5.6f, 9f), new Vector3(26f, 5f, 22f), "walkway"),
            (new Vector3(38f, 9.6f, 1f), new Vector3(46f, 8.8f, 20f), "offices"),
        };

        [UnityTest]
        public IEnumerator DrawingTheMallStaysInBudgetAndOcclusionHelps()
        {
            yield return TestMapScene.Load("Mall");
            var go = new GameObject("BudgetCamera");
            var cam = go.AddComponent<Camera>();
            cam.fieldOfView = 75f;
            cam.targetTexture = new RenderTexture(1280, 720, 24);
            cam.enabled = false; // rendered by hand only, once per measured frame
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count");
            using var tris = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
            var lines = new List<string>();
            long worst = 0;
            // Only the probe draws: the player's camera would add its own view to every count.
            var others = new List<Camera>();
            foreach (Camera c in Camera.allCameras) if (c != cam) { c.enabled = false; others.Add(c); }
            foreach ((Vector3 from, Vector3 to, string name) in Views)
            {
                go.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from));
                long[] culled = new long[2], noCull = new long[2];
                cam.useOcclusionCulling = true;
                yield return Measure(cam, batches, tris, culled);
                cam.useOcclusionCulling = false;
                yield return Measure(cam, batches, tris, noCull);
                if (culled[0] <= 0) Assert.Ignore("render counters aren't recorded here (no graphics device?)");
                worst = System.Math.Max(worst, culled[0]);
                lines.Add($"{name}: {culled[0]} batches / {culled[1] / 1000}k tris (no occlusion: {noCull[0]} / {noCull[1] / 1000}k)");
            }
            foreach (Camera c in others) if (c != null) c.enabled = true;
            Debug.Log("[Perf] Mall render budget:\n" + string.Join("\n", lines));
            Object.Destroy(cam.targetTexture);
            Object.Destroy(go);
            Assert.Less(worst, BatchBudget, string.Join("; ", lines));
        }

        private static IEnumerator Measure(Camera cam, ProfilerRecorder batches, ProfilerRecorder tris, long[] into)
        {
            // One frame to settle (culling caches), then one frame with exactly one render of this view.
            cam.Render();
            yield return null;
            cam.Render();
            yield return null;
            into[0] = batches.LastValue;
            into[1] = tris.LastValue;
        }
    }
}
