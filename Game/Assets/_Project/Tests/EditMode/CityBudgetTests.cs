using System.IO;
using Abandoned.World;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.Tests
{
    /// <summary>
    /// The city's budgets (Docs/CITY_PLAN.md, "How it stays fast"), checked on the saved draft without rendering it:
    /// from any spot in the city, how many renderers and triangles the streamer would have on, and how many objects
    /// that comes to. A guard against the city quietly getting heavier as the detail passes land, not a frame-rate
    /// measurement — Tests/PlayMode/MallRenderBudgetTests is the one that counts real batches through a camera.
    ///
    /// Ignores itself until the draft has been built (Tools > Abandoned > Maps > Open World > Rebuild City Draft),
    /// so the suite stays green on a checkout that has never run it.
    /// </summary>
    public class CityBudgetTests
    {
        private const string ScenePath = "Assets/_Project/Scenes/CityDraft.unity";
        private const int RendererBudget = 700;
        private const int TriangleBudget = 1_500_000;
        private const int ObjectBudget = 4000;

        // Spots worth being heavy: the tower district, the retail core, the harbour and a bridge over the river.
        private static readonly Vector3[] Probes =
        {
            new(900f, 2f, 600f), new(-300f, 2f, 200f), new(-2150f, 2f, -900f), new(884f, 2f, -68f),
        };

        [Test]
        public void TheCityStaysInsideItsDrawBudgetFromEverySpot()
        {
            CityStreamer streamer = OpenDraft();
            var worst = (renderers: 0, triangles: 0, objects: 0, where: Vector3.zero);
            foreach (Vector3 probe in Probes)
            {
                (int renderers, int triangles, int objects) = Visible(streamer, probe);
                Debug.Log($"[CityBudget] at {probe}: {renderers} renderers, {triangles:n0} triangles, {objects} objects.");
                if (renderers > worst.renderers) worst = (renderers, triangles, objects, probe);
            }
            Assert.LessOrEqual(worst.renderers, RendererBudget,
                $"Too many renderers on at {worst.where}. Fewer materials per cell, or a shorter near range.");
            Assert.LessOrEqual(worst.triangles, TriangleBudget, $"Too many triangles on at {worst.where}.");
            Assert.LessOrEqual(worst.objects, ObjectBudget,
                $"Too many objects on at {worst.where}. Collider holders are shared per district angle; check that.");
        }

        [Test]
        public void EveryCellHasBothCopiesAndTheRangesFitTheCamera()
        {
            CityStreamer streamer = OpenDraft();
            Assert.Greater(streamer.Centres.Count, 0, "The draft has no cells.");
            Assert.AreEqual(streamer.Centres.Count, streamer.NearCopies.Count);
            for (int i = 0; i < streamer.NearCopies.Count; i++)
                Assert.NotNull(streamer.NearCopies[i], $"Cell {i} has no near copy.");
            Assert.Less(streamer.NearRange, streamer.FarRange, "The near range has to come before the far one.");

            Camera camera = Object.FindFirstObjectByType<Camera>();
            Assert.NotNull(camera, "The draft has no camera to judge the ranges against.");
            Assert.LessOrEqual(streamer.FarRange, camera.farClipPlane,
                "Cells are kept on past the camera's far plane, which only costs: shorten the far range.");
        }

        // What the streamer would have switched on at a point, counted off the saved objects rather than by running.
        private static (int renderers, int triangles, int objects) Visible(CityStreamer streamer, Vector3 at)
        {
            int renderers = 0, triangles = 0, objects = 0;
            for (int i = 0; i < streamer.Centres.Count; i++)
            {
                float distance = new Vector2(streamer.Centres[i].x - at.x, streamer.Centres[i].z - at.z).magnitude;
                GameObject copy = distance < streamer.NearRange ? streamer.NearCopies[i]
                    : distance < streamer.FarRange ? Far(streamer, i) : null;
                if (copy == null) continue;
                objects += copy.GetComponentsInChildren<Transform>(true).Length;
                foreach (MeshFilter filter in copy.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    renderers++;
                    triangles += filter.sharedMesh.triangles.Length / 3;
                }
            }
            return (renderers, triangles, objects);
        }

        private static GameObject Far(CityStreamer streamer, int i) =>
            i < streamer.FarCopies.Count ? streamer.FarCopies[i] : null;

        private static CityStreamer OpenDraft()
        {
            if (!File.Exists(ScenePath))
                Assert.Ignore("The city draft has not been built yet (Maps > Open World > Rebuild City Draft).");
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Assert.IsTrue(scene.IsValid(), $"Could not open {ScenePath}.");
            var streamer = Object.FindFirstObjectByType<CityStreamer>();
            Assert.NotNull(streamer, "The draft has no CityStreamer: rebuild it.");
            return streamer;
        }
    }
}
