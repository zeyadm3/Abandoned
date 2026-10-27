using System.Collections.Generic;
using System.IO;
using Abandoned.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What the built city costs and whether it holds together (Docs/CITY_PLAN.md step 8), plus the shots to judge it
    /// by. Reports only: no scene or asset is changed.
    ///
    /// It reads the city that exists — Scenes/World once the city is in it, else Scenes/CityDraft — so the same
    /// numbers come out of the draft and the real thing. The checks are the ones that catch a city built from a
    /// wrong trace: a map with blocks standing on its ground, or a map no road reaches.
    /// </summary>
    public static class CityAudit
    {
        private const string DraftPath = "Assets/_Project/Scenes/CityDraft.unity";
        private const string WorldPath = "Assets/_Project/Scenes/World.unity";
        private const int ShotWidth = 1600, ShotHeight = 900;

        // Where a site's road has to reach before the van is stranded: a spur is 420 m, so this is generous.
        private const float RoadReach = 80f;

        private static readonly (string name, Vector3 at, Vector3 look)[] Views =
        {
            ("01-city-from-the-south", new Vector3(0f, 2900f, -3600f), new Vector3(200f, 0f, 200f)),
            ("02-downtown-and-circus", new Vector3(300f, 760f, -260f), new Vector3(900f, 0f, 600f)),
            ("03-circus", new Vector3(754f, 240f, -40f), new Vector3(754f, 0f, 452f)),
            ("04-street-from-the-cab", new Vector3(140f, 2.2f, 150f), new Vector3(900f, 6f, 430f)),
            ("05-river-and-bridge", new Vector3(884f, 70f, -420f), new Vector3(884f, 0f, -68f)),
            ("06-harbour-and-hq", new Vector3(-2150f, 300f, -1700f), new Vector3(-2150f, 0f, -900f)),
            ("07-quay-and-gallows-rock", new Vector3(-2450f, 220f, -250f), new Vector3(-3850f, 0f, -250f)),
            ("08-retail-core", new Vector3(-300f, 320f, -420f), new Vector3(-300f, 0f, 200f)),
        };

        // The spots the budget test uses, so the two agree on where the city is heaviest.
        private static readonly Vector3[] Probes =
        {
            new(900f, 2f, 600f), new(-300f, 2f, 200f), new(-2150f, 2f, -900f), new(884f, 2f, -68f),
        };

        [MenuItem(ToolMenu.Checks + "Audit and Render City", priority = ToolMenu.ChecksP + 23)]
        public static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            string path = Open();
            if (path == null) return;

            var streamer = Object.FindFirstObjectByType<CityStreamer>();
            if (streamer == null)
            {
                Debug.LogError($"[CityAudit] {path} has no CityStreamer. Rebuild the city first.");
                return;
            }

            Counts(streamer);
            Budgets(streamer);
            Coplanar();
            Sites();
            string folder = Shots();
            Debug.Log($"[CityAudit] done on {path}. Shots in {folder}.");
        }

        private static string Open()
        {
            foreach (string path in new[] { WorldPath, DraftPath })
            {
                if (!File.Exists(path)) continue;
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                if (scene.IsValid() && Object.FindFirstObjectByType<CityStreamer>() != null) return path;
            }
            Debug.LogError("[CityAudit] No built city found. Run Rebuild World, or Rebuild City Draft.");
            return null;
        }

        private static void Counts(CityStreamer streamer)
        {
            int renderers = 0, triangles = 0, colliders = 0;
            foreach (GameObject near in streamer.NearCopies)
            {
                if (near == null) continue;
                colliders += near.GetComponentsInChildren<Collider>(true).Length;
                foreach (MeshFilter filter in near.GetComponentsInChildren<MeshFilter>(true))
                {
                    if (filter.sharedMesh == null) continue;
                    renderers++;
                    triangles += filter.sharedMesh.triangles.Length / 3;
                }
            }
            Debug.Log($"[CityAudit] {streamer.Centres.Count} cells, {renderers} renderers, " +
                      $"{triangles:n0} triangles, {colliders} colliders in the whole city. " +
                      $"Near {streamer.NearRange:0} m, far {streamer.FarRange:0} m.");
        }

        // What the streamer would have on at each probe. The budget test asserts these; here they are only reported,
        // with the limit beside them, so it is obvious how much a detail pass has left to spend.
        private static void Budgets(CityStreamer streamer)
        {
            foreach (Vector3 probe in Probes)
            {
                int renderers = 0, triangles = 0, objects = 0;
                for (int i = 0; i < streamer.Centres.Count; i++)
                {
                    float d = new Vector2(streamer.Centres[i].x - probe.x, streamer.Centres[i].z - probe.z).magnitude;
                    GameObject copy = d < streamer.NearRange ? streamer.NearCopies[i]
                        : d < streamer.FarRange && i < streamer.FarCopies.Count ? streamer.FarCopies[i] : null;
                    if (copy == null) continue;
                    objects += copy.GetComponentsInChildren<Transform>(true).Length;
                    foreach (MeshFilter filter in copy.GetComponentsInChildren<MeshFilter>(true))
                    {
                        if (filter.sharedMesh == null) continue;
                        renderers++;
                        triangles += filter.sharedMesh.triangles.Length / 3;
                    }
                }
                Debug.Log($"[CityAudit] at ({probe.x:0}, {probe.z:0}): {renderers}/700 renderers, " +
                          $"{triangles:n0}/1,500,000 triangles, {objects}/4000 objects.");
            }
        }

        // The 0.24.13 rule: no two drawn faces may share a plane and overlap. Keep this near zero.
        private static void Coplanar()
        {
            List<GraphicsAudit.Fight> fights = GraphicsAudit.FindFights(out int triangles);
            Debug.Log($"[CityAudit] coplanar faces close enough to flicker: {fights.Count} (of {triangles:n0} drawn).");
        }

        // The two ways a wrong trace shows up: something built on a map's ground, or a map no road reaches.
        private static void Sites()
        {
            var roads = new List<Bounds>();
            var blocks = new List<Bounds>();
            foreach (MeshRenderer renderer in Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,
                                                                                     FindObjectsSortMode.None))
            {
                if (renderer.name == "Surface" || renderer.name == "Deck") roads.Add(renderer.bounds);
                else if (System.Enum.TryParse(renderer.name, out CityKind _)) blocks.Add(renderer.bounds);
            }

            foreach (CityPlan.Site site in CityPlan.Sites)
            {
                Rect clear = CitySites.GroundOf(site);
                foreach (Bounds b in blocks)
                {
                    if (b.max.x < clear.xMin || b.min.x > clear.xMax || b.max.z < clear.yMin || b.min.z > clear.yMax)
                        continue;
                    Debug.LogWarning($"[CityAudit] {site.Scene}: blocks stand on its ground. " +
                                     "The site is not being kept clear.");
                    break;
                }
                if (site.Scene == "Prison") continue;   // reached by boat, never by road

                (Vector2 from, Vector2 _) = CitySites.Approach(site);
                float nearest = float.MaxValue;
                foreach (Bounds b in roads)
                    nearest = Mathf.Min(nearest, Vector2.Distance(from, new Vector2(
                        Mathf.Clamp(from.x, b.min.x, b.max.x), Mathf.Clamp(from.y, b.min.z, b.max.z))));
                if (nearest > RoadReach)
                    Debug.LogWarning($"[CityAudit] {site.Scene}: nearest road is {nearest:0} m from where the van " +
                                     "parks. Its spur is not reaching the grid.");
            }
        }

        // Review shots from the spots that say whether the drawing came out: the city from the air, the Circus, a
        // street at eye height, the river, the harbour and the island.
        private static string Shots()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../../Docs/renders/city"));
            Directory.CreateDirectory(folder);
            var go = new GameObject("CityAuditCamera");
            Camera camera = go.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.farClipPlane = 12000f;
            camera.clearFlags = CameraClearFlags.Skybox;
            var target = new RenderTexture(ShotWidth, ShotHeight, 24);
            camera.targetTexture = target;

            foreach ((string name, Vector3 at, Vector3 look) in Views)
            {
                camera.transform.SetPositionAndRotation(at, Quaternion.LookRotation((look - at).normalized, Vector3.up));
                camera.Render();
                RenderTexture.active = target;
                var shot = new Texture2D(ShotWidth, ShotHeight, TextureFormat.RGB24, false);
                shot.ReadPixels(new Rect(0, 0, ShotWidth, ShotHeight), 0, 0);
                shot.Apply();
                RenderTexture.active = null;
                File.WriteAllBytes(Path.Combine(folder, $"{name}.png"), shot.EncodeToPNG());
                Object.DestroyImmediate(shot);
            }

            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(go);
            return folder;
        }
    }
}
