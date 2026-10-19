using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Batch checks for the island prison (Tools/unity.sh exec Abandoned.EditorTools.IslandPrisonBatch.BuildAndReport):
    /// rebuild, then the walk-through, invisible-collider and material audits, a coplanar-overlap (z-fighting) audit
    /// of every box in the level, and review renders from fixed viewpoints (each twice: as lit in the game, and with
    /// the night lifted and the fog off so the geometry can be judged) into Docs/renders/island/.
    /// </summary>
    public static class IslandPrisonBatch
    {
        private const int Width = 1600, Height = 900;
        private static string Folder => System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../../Docs/renders/island"));

        [MenuItem("Tools/Abandoned/Prison/Audit and Render Island Prison")]
        public static void Menu() => Report(false);

        public static void BuildAndReport()
        {
            int code = 0;
            try
            {
                IslandPrisonBuilder.Build();
                code = Report(true);
            }
            catch (Exception e) { Debug.LogException(e); code = 1; }
            finally { EditorApplication.Exit(code); }
        }

        public static void CheckAllLootReachable()
        {
            int code = 0;
            try
            {
                Scene scene = EditorSceneManager.OpenScene(PrisonBuilder.ScenePath, OpenSceneMode.Single);
                Vector3 entry = new Vector3(82f, 0f, 8f);
                var lootPoints = UnityEngine.Object.FindObjectsByType<Abandoned.Loot.LootSpawnPoint>(FindObjectsSortMode.None);
                int lootFailed = 0;
                foreach (var p in lootPoints)
                {
                    if (p.name.StartsWith("Tower_Cabin")) continue;
                    var path = new UnityEngine.AI.NavMeshPath();
                    bool hasStart = UnityEngine.AI.NavMesh.SamplePosition(entry, out var a, 2f, UnityEngine.AI.NavMesh.AllAreas);
                    bool hasEnd = UnityEngine.AI.NavMesh.SamplePosition(p.transform.position, out var b, 2f, UnityEngine.AI.NavMesh.AllAreas);
                    bool calculated = hasStart && hasEnd && UnityEngine.AI.NavMesh.CalculatePath(a.position, b.position, UnityEngine.AI.NavMesh.AllAreas, path);
                    bool complete = calculated && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
                    if (!complete)
                    {
                        lootFailed++;
                        Debug.LogWarning($"[LootReachability] Failed: {p.name} at {p.transform.position} (hasStart={hasStart}, hasEnd={hasEnd}, calculated={calculated}, status={(calculated ? path.status.ToString() : "none")})");
                    }
                }
                var threatPoints = UnityEngine.Object.FindObjectsByType<Abandoned.Threats.ThreatSpawnPoint>(FindObjectsSortMode.None);
                int threatFailed = 0;
                foreach (var t in threatPoints)
                {
                    var path = new UnityEngine.AI.NavMeshPath();
                    bool hasStart = UnityEngine.AI.NavMesh.SamplePosition(entry, out var a, 2f, UnityEngine.AI.NavMesh.AllAreas);
                    bool hasEnd = UnityEngine.AI.NavMesh.SamplePosition(t.transform.position, out var b, 2f, UnityEngine.AI.NavMesh.AllAreas);
                    bool calculated = hasStart && hasEnd && UnityEngine.AI.NavMesh.CalculatePath(a.position, b.position, UnityEngine.AI.NavMesh.AllAreas, path);
                    bool complete = calculated && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete;
                    if (!complete)
                    {
                        threatFailed++;
                        Debug.LogWarning($"[ThreatReachability] Failed: {t.name} at {t.transform.position} (hasStart={hasStart}, hasEnd={hasEnd}, calculated={calculated}, status={(calculated ? path.status.ToString() : "none")})");
                    }
                }
                Debug.Log($"[ReachabilityResult] Loot: {lootPoints.Length - lootFailed}/{lootPoints.Length} passed ({lootFailed} failed). Threats: {threatPoints.Length - threatFailed}/{threatPoints.Length} passed ({threatFailed} failed).");
                if (lootFailed > 0 || threatFailed > 0) code = 1;
            }
            catch (Exception e) { Debug.LogException(e); code = 1; }
            finally { EditorApplication.Exit(code); }
        }

        public static void ReportAndExit()
        {
            int code = 0;
            try { code = Report(true); }
            catch (Exception e) { Debug.LogException(e); code = 1; }
            finally { EditorApplication.Exit(code); }
        }

        private static int Report(bool batch)
        {
            Scene scene = EditorSceneManager.OpenScene(PrisonBuilder.ScenePath, OpenSceneMode.Single);
            Transform root = scene.GetRootGameObjects().First(g => g.name == L.SceneName).transform;
            Physics.SyncTransforms();
            int walk = SolidObjectsPass.Audit(root, L.SceneName);
            int invisible = InvisibleColliderAudit.Run(root, L.SceneName);
            int overlaps = Coplanar(root);
            Debug.Log($"[IslandAudit] walk-through objects: {walk}, invisible colliders: {invisible}, coplanar overlaps: {overlaps}.");
            Render(root);
            return 0;
        }

        // ---- Coplanar faces: two different-material boxes sharing a face plane over an area (they flicker) ----------

        private static int Coplanar(Transform root)
        {
            var boxes = new List<(Bounds b, Material m, string path)>();
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>())
            {
                if (!r.enabled || r.sharedMaterial == null) continue;
                if (r.GetComponentInParent<Abandoned.Core.WalkThroughObject>() is { } w && w.transform.name == IslandBackdrop.GroupName) continue;
                Bounds b = r.bounds;
                if (b.size.x > 60f || b.size.z > 60f) continue;
                // Only axis-aligned boxes have tight bounds worth comparing.
                Vector3 e = r.transform.rotation.eulerAngles;
                if (!Square(e.x) || !Square(e.y) || !Square(e.z)) continue;
                var filter = r.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null || filter.sharedMesh.vertexCount > 24) continue;
                boxes.Add((b, r.sharedMaterial, Path(r.transform)));
            }
            var grid = new Dictionary<Vector2Int, List<int>>();
            for (int i = 0; i < boxes.Count; i++)
            {
                Bounds b = boxes[i].b;
                for (int x = Mathf.FloorToInt(b.min.x / 4f); x <= Mathf.FloorToInt(b.max.x / 4f); x++)
                for (int z = Mathf.FloorToInt(b.min.z / 4f); z <= Mathf.FloorToInt(b.max.z / 4f); z++)
                {
                    var k = new Vector2Int(x, z);
                    if (!grid.TryGetValue(k, out List<int> list)) grid[k] = list = new List<int>();
                    list.Add(i);
                }
            }
            var seen = new HashSet<long>();
            int found = 0;
            foreach (List<int> cell in grid.Values)
            for (int a = 0; a < cell.Count; a++)
            for (int c = a + 1; c < cell.Count; c++)
            {
                int i = cell[a], j = cell[c];
                long key = (long)Mathf.Min(i, j) * 1000003L + Mathf.Max(i, j);
                if (!seen.Add(key)) continue;
                var A = boxes[i]; var B = boxes[j];
                if (A.m == B.m) continue;
                string axis = SharedFace(A.b, B.b);
                if (axis == null) continue;
                found++;
                if (found <= 120) Debug.LogWarning($"[IslandAudit] coplanar ({axis}): {A.path} [{A.m.name}] and {B.path} [{B.m.name}] at {A.b.center}");
            }
            return found;
        }

        private static bool Square(float deg) => Mathf.Abs(Mathf.DeltaAngle(deg, Mathf.Round(deg / 90f) * 90f)) < .5f;

        // Do two boxes share a face plane over more than a sliver (and both faces show there)?
        private static string SharedFace(Bounds a, Bounds b)
        {
            const float eps = .003f, sliver = .05f;
            for (int axis = 0; axis < 3; axis++)
            {
                int u = (axis + 1) % 3, v = (axis + 2) % 3;
                float ou = Mathf.Min(a.max[u], b.max[u]) - Mathf.Max(a.min[u], b.min[u]);
                float ov = Mathf.Min(a.max[v], b.max[v]) - Mathf.Max(a.min[v], b.min[v]);
                if (ou < sliver || ov < sliver) continue;
                // Same-facing faces in one plane: both visible from the same side.
                if (Mathf.Abs(a.max[axis] - b.max[axis]) < eps) return "xyz"[axis].ToString();
                // Bottoms resting on the same floor are hidden; any other shared min face shows.
                if (axis != 1 && Mathf.Abs(a.min[axis] - b.min[axis]) < eps) return "xyz"[axis].ToString();
            }
            return null;
        }

        private static string Path(Transform t)
        {
            string p = t.name;
            for (Transform q = t.parent; q != null; q = q.parent) p = q.name + "/" + p;
            return p;
        }

        // ---- Review renders ------------------------------------------------------------------------------------------

        private static readonly (string name, Vector3 eye, Vector3 look)[] Views =
        {
            ("01-from-the-boat", new Vector3(82f, 2.2f, -30f), new Vector3(70f, 14f, 60f)),
            ("02-aerial-south", new Vector3(70f, 75f, -95f), new Vector3(66f, 8f, 75f)),
            ("03-aerial-northwest", new Vector3(-70f, 70f, 200f), new Vector3(66f, 6f, 70f)),
            ("04-aerial-east", new Vector3(240f, 60f, 60f), new Vector3(66f, 6f, 70f)),
            ("05-main-path", new Vector3(54f, 13.7f, 24f), new Vector3(54f, 18f, 60f)),
            ("06-north-lawn", new Vector3(96f, 13.7f, 112f), new Vector3(60f, 18f, 106f)),
            ("07-yard", new Vector3(12f, 13.7f, 54f), new Vector3(30f, 14f, 82f)),
            ("08-broadway", new Vector3(54f, 13.7f, 47f), new Vector3(54f, 16f, 80f)),
            ("09-gallery-tier2", new Vector3(47.8f, 21.7f, 55f), new Vector3(47f, 20f, 78f)),
            ("10-dining", new Vector3(40f, 13.7f, 82f), new Vector3(66f, 13f, 88f)),
            ("11-laundry", new Vector3(33f, 9.7f, 82f), new Vector3(50f, 9f, 88f)),
            ("12-road", new Vector3(136f, 1.8f, 22f), new Vector3(134f, 6f, 64f)),
            ("13-north-shore", new Vector3(60f, -1f, 200f), new Vector3(60f, 14f, 100f)),
            ("14-bay-northeast", new Vector3(112f, 13.7f, 114f), new Vector3(400f, 40f, 700f)),
            ("15-warden-house", new Vector3(46f, 13.7f, 102f), new Vector3(31f, 13f, 106f)),
            ("16-quay", new Vector3(120f, 1.7f, 4f), new Vector3(70f, 6f, 12f)),
            ("17-southwest-rocks", new Vector3(-10f, 4f, -10f), new Vector3(30f, 10f, 40f)),
            ("18-lighthouse-top", new Vector3(62f, 29.7f, 109.5f), new Vector3(150f, 10f, 40f)),
        };

        private static void Render(Transform root)
        {
            Directory.CreateDirectory(Folder);
            var go = new GameObject("IslandReviewCamera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = go.AddComponent<Camera>();
            camera.enabled = false;
            camera.fieldOfView = 70f;
            camera.nearClipPlane = .07f;
            camera.farClipPlane = 5000f;
            camera.allowHDR = true;
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            camera.targetTexture = target;
            Color sky = RenderSettings.ambientSkyColor, eq = RenderSettings.ambientEquatorColor, gr = RenderSettings.ambientGroundColor;
            bool fog = RenderSettings.fog;
            try
            {
                foreach (bool lifted in new[] { false, true })
                {
                    RenderSettings.ambientSkyColor = lifted ? new Color(.55f, .58f, .62f) : sky;
                    RenderSettings.ambientEquatorColor = lifted ? new Color(.45f, .46f, .47f) : eq;
                    RenderSettings.ambientGroundColor = lifted ? new Color(.3f, .3f, .3f) : gr;
                    RenderSettings.fog = !lifted && fog;
                    foreach ((string name, Vector3 eye, Vector3 look) in Views)
                    {
                        camera.transform.SetPositionAndRotation(eye, Quaternion.LookRotation(look - eye));
                        Shot(camera, target, name + (lifted ? "-lit" : "-night"));
                    }
                }
            }
            finally
            {
                RenderSettings.ambientSkyColor = sky; RenderSettings.ambientEquatorColor = eq; RenderSettings.ambientGroundColor = gr;
                RenderSettings.fog = fog;
                camera.targetTexture = null;
                target.Release();
                UnityEngine.Object.DestroyImmediate(go);
            }
            Debug.Log($"[IslandAudit] renders written to {Folder}");
        }

        private static void Shot(Camera camera, RenderTexture target, string name)
        {
            var request = new UniversalRenderPipeline.SingleCameraRequest { destination = target };
            bool supported = RenderPipeline.SupportsRenderRequest(camera, request);
            for (int frame = 0; frame < 2; frame++)
            {
                if (supported) RenderPipeline.SubmitRenderRequest(camera, request);
                else camera.Render();
            }
            RenderTexture previous = RenderTexture.active;
            var pixels = new Texture2D(Width, Height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                pixels.Apply();
                File.WriteAllBytes(System.IO.Path.Combine(Folder, name + ".png"), pixels.EncodeToJPG(82));
            }
            finally { RenderTexture.active = previous; UnityEngine.Object.DestroyImmediate(pixels); }
        }
    }
}
