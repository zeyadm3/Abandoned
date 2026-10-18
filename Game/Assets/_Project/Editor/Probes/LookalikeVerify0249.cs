using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Checks the 0.24.9 look-alike pass; changes nothing. (1) Every batch mesh it cut is compared with its
    /// committed copy: each removed triangle must belong to a look-alike, and none of a look-alike's may be left.
    /// (2) Every stand-in in the saved mall has a slab, renderers, meshes and materials, and the audit reads 0.
    /// (3) Close-up renders of each kind of stand-in. Needs <see cref="Temp"/>: the committed MallBefore.unity and
    /// the committed batch meshes under Old/, put there by the caller (git) and deleted afterwards.
    /// Batch: <c>VERIFY_OUT=dir Tools/unity.sh exec Abandoned.EditorTools.LookalikeVerify0249.VerifyAndExit</c>.
    /// </summary>
    public static class LookalikeVerify0249
    {
        private const string Temp = "Assets/_VerifyTemp0249";
        private static readonly string[] Fixtures = LootLookalikes.FixtureIds.ToArray();

        public static void VerifyAndExit()
        {
            int code = 0;
            try { if (!Verify()) code = 1; }
            catch (Exception exception) { Debug.LogException(exception); code = 1; }
            finally { EditorApplication.Exit(code); }
        }

        public static bool Verify()
        {
            string scenePath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", MallBuilder.ScenePath));
            string before = Hash(scenePath);
            bool cuts = Cuts();
            bool scene = SavedScene();
            Render(Environment.GetEnvironmentVariable("VERIFY_OUT") ?? Path.Combine(Application.dataPath, "..", "Temp", "Verify0249"));
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            bool untouched = Hash(scenePath) == before;
            Debug.Log($"[Verify0249] RESULT cuts={(cuts ? "ok" : "FAIL")} scene={(scene ? "ok" : "FAIL")} mall-file-unchanged={untouched}");
            return cuts && scene && untouched;
        }

        private static bool Cuts()
        {
            Scene scene = EditorSceneManager.OpenScene(Temp + "/MallBefore.unity", OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().First(root => root.name == "Mall").transform;
            int holders = 0, cells = 0, meshes = 0;
            long removed = 0, overcut = 0, undercut = 0, misaligned = 0;
            foreach (string name in new[] { SalvageMallDensity.GroupName, MallExpansion0156.GroupName })
            {
                Transform layer = mall.Find(name);
                if (layer == null) continue;
                var byCell = LootLookalikePass.Holders(layer)
                    .Select(holder => (holder, cell: LootLookalikePass.BatchCell(layer, holder)))
                    .Where(pair => pair.cell != null).GroupBy(pair => pair.cell);
                foreach (var group in byCell)
                {
                    var set = new BatchTriangleSet();
                    foreach ((SectionProp holder, Transform _) in group)
                    {
                        LootLookalikePass.Collect(holder, holder.Section.transform.position, set);
                        holders++;
                    }
                    cells++;
                    foreach (MeshFilter filter in group.Key.GetComponentsInChildren<MeshFilter>(true))
                    {
                        Mesh now = filter.sharedMesh;
                        string path = AssetDatabase.GetAssetPath(now);
                        Mesh old = AssetDatabase.LoadAssetAtPath<Mesh>(Temp + "/Old/" + path.Substring("Assets/".Length));
                        Compare(old != null ? old : now, now, set, ref removed, ref overcut, ref undercut, ref misaligned);
                        meshes++;
                    }
                }
            }
            Debug.Log($"[Verify0249] cuts: {holders} batched look-alikes in {cells} batches ({meshes} meshes); {removed} triangles " +
                $"removed, {overcut} of them NOT a look-alike's; {undercut} look-alike triangles left; {misaligned} meshes misaligned.");
            return holders > 0 && overcut == 0 && undercut == 0 && misaligned == 0;
        }

        private static void Compare(Mesh old, Mesh now, BatchTriangleSet set, ref long removed, ref long overcut,
            ref long undercut, ref long misaligned)
        {
            Vector3[] was = old.vertices, are = now.vertices;
            if (was.Length != are.Length || old.subMeshCount != now.subMeshCount) { misaligned++; return; }
            for (int submesh = 0; submesh < old.subMeshCount; submesh++)
            {
                int[] before = old.GetTriangles(submesh), after = now.GetTriangles(submesh);
                int j = 0;
                // The pass only drops triangles and keeps the order, so the new list is a subsequence of the old.
                for (int i = 0; i + 2 < before.Length; i += 3)
                {
                    if (j + 2 < after.Length && before[i] == after[j] && before[i + 1] == after[j + 1] && before[i + 2] == after[j + 2])
                    {
                        j += 3;
                        continue;
                    }
                    removed++;
                    if (!set.Matches(was[before[i]], was[before[i + 1]], was[before[i + 2]])) overcut++;
                }
                if (j != after.Length) misaligned++;
                for (int k = 0; k + 2 < after.Length; k += 3)
                    if (set.Matches(are[after[k]], are[after[k + 1]], are[after[k + 2]])) undercut++;
            }
        }

        private static bool SavedScene()
        {
            Scene scene = EditorSceneManager.OpenScene(MallBuilder.ScenePath, OpenSceneMode.Single);
            Transform mall = scene.GetRootGameObjects().First(root => root.name == "Mall").transform;
            int sheets = 0, packaging = 0, fixtures = 0, unbound = 0, broken = 0, solidPackaging = 0, bareSheets = 0;
            foreach (Transform node in mall.GetComponentsInChildren<Transform>(true))
            {
                bool sheet = node.name.EndsWith("_DustSheeted"), pack = node.name.EndsWith("_EmptyPackaging");
                bool fixture = Fixtures.Contains(node.name) && node.GetComponentInParent<SectionProp>() != null;
                if (!sheet && !pack && !fixture) continue;
                if (sheet) sheets++; else if (pack) packaging++; else fixtures++;
                SectionProp binding = node.GetComponentInParent<SectionProp>();
                if (binding == null || binding.Section == null) unbound++;
                Renderer[] renderers = node.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0 || renderers.Any(r => r.sharedMaterials.Any(m => m == null)) ||
                    node.GetComponentsInChildren<MeshFilter>(true).Any(f => f.sharedMesh == null)) broken++;
                bool solid = LootLookalikeStandIns.HasCollision(node.gameObject);
                if (pack && solid) solidPackaging++;
                if (sheet && !solid) bareSheets++;
            }
            int left = LootLookalikeAudit.Run(mall, "Mall");
            var report = mall.Find(LootLookalikeAudit.ReportName)?.GetComponent<LootLookalikeDebug>();
            int spots = report != null ? report.Spots.Length : -1;
            int named = mall.GetComponentsInChildren<Transform>(true).Count(t => LootLookalikes.Is(t.name));
            Debug.Log($"[Verify0249] scene: {sheets} sheets, {packaging} packaging, {fixtures} fixtures (= {sheets + packaging + fixtures}); " +
                $"{unbound} without a slab, {broken} with missing mesh/material, {solidPackaging} solid packaging, " +
                $"{bareSheets} sheets without collision; {left} look-alikes left; F1 report spots {spots}; {named} objects named as loot ids.");
            return sheets + packaging + fixtures == 595 && unbound == 0 && broken == 0 && solidPackaging == 0 &&
                left == 0 && spots == 595 && named == 0;
        }

        private static void Render(string folder)
        {
            Directory.CreateDirectory(folder);
            Transform mall = SceneManager.GetActiveScene().GetRootGameObjects().First(root => root.name == "Mall").transform;
            var picks = new List<(string, Transform)>();
            foreach (string fixture in Fixtures)
                picks.AddRange(mall.GetComponentsInChildren<Transform>(true).Where(t => t.name == fixture).Take(1).Select(t => (fixture, t)));
            foreach (string sheet in new[] { "showroom_sofa", "ergonomic_chair", "executive_chair", "office_printer", "floor_lamp", "pc_tower" })
                picks.AddRange(mall.GetComponentsInChildren<Transform>(true).Where(t => t.name == sheet + "_DustSheeted").Take(1).Select(t => (sheet + "-sheet", t)));
            picks.AddRange(mall.GetComponentsInChildren<Transform>(true).Where(t => t.name.EndsWith("_EmptyPackaging") &&
                ModelFit.RendererBounds(t.gameObject).min.y % 4f > .5f).Take(2).Select(t => (t.name, t)));

            RenderSettings.fog = false;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.45f, .45f, .45f);
            Physics.SyncTransforms();
            var cameraObject = new GameObject("VerifyCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 55f;
            camera.nearClipPlane = .05f;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            var lamp = new GameObject("VerifyLamp") { hideFlags = HideFlags.HideAndDontSave }.AddComponent<Light>();
            lamp.type = LightType.Point;
            lamp.intensity = 6f;
            var target = new RenderTexture(960, 600, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            try
            {
                foreach ((string name, Transform item) in picks)
                {
                    Bounds bounds = ModelFit.RendererBounds(item.gameObject);
                    float distance = Mathf.Max(1.8f, bounds.size.magnitude * 1.3f);
                    Vector3 view = BestView(item, bounds.center, distance);
                    camera.transform.position = bounds.center + view * distance + Vector3.up * Mathf.Max(.8f, bounds.extents.y);
                    camera.transform.LookAt(bounds.center);
                    lamp.transform.position = camera.transform.position + Vector3.up * .3f;
                    lamp.range = distance * 4f;
                    Capture(camera, target, Path.Combine(folder, name + ".png"));
                }
                Debug.Log($"[Verify0249] rendered {picks.Count} close-ups to {folder}");
            }
            finally
            {
                camera.targetTexture = null;
                target.Release();
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(lamp.gameObject);
            }
        }

        private static Vector3 BestView(Transform item, Vector3 focus, float distance)
        {
            Vector3 forward = Vector3.ProjectOnPlane(item.forward, Vector3.up).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward);
            // The first side with nothing solid between the item and the camera.
            foreach (Vector3 side in new[] { -forward, forward, right, -right })
                if (!Physics.Raycast(focus + side * .6f, (side * distance + Vector3.up).normalized, distance + 1f))
                    return side;
            return -forward;
        }

        private static void Capture(Camera camera, RenderTexture target, string file)
        {
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            bool supported = RenderPipeline.SupportsRenderRequest(camera, request);
            for (int frame = 0; frame < 2; frame++)
            {
                if (supported) RenderPipeline.SubmitRenderRequest(camera, request);
                else camera.Render();
            }
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(file, pixels.EncodeToPNG());
            }
            finally { RenderTexture.active = previous; Object.DestroyImmediate(pixels); }
        }

        private static string Hash(string path)
        {
            using SHA256 hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path)));
        }
    }
}
