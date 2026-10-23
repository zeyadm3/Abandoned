using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.World;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Reads what the world needs from each built place's saved scene without changing it: its sky, ambient light and
    /// fog (<see cref="PlaceLook"/>, applied while it is loaded in the world), and for places marked to be measured,
    /// its ground (the largest flat collider: the world's terrain is cut away over it), what to hide in the world (its
    /// own collection truck, and scenery reaching far past its ground: horizons, distant hills) and where the van
    /// parks (where its truck stood). Opens each additively and closes it.
    /// </summary>
    public static class WorldSurvey
    {
        public static void Capture(List<WorldPoi> places)
        {
            foreach (WorldPoi p in places)
            {
                if (!p.built) continue;
                string path = ScenePath(p.scene);
                if (path == null)
                {
                    Debug.LogWarning($"[WorldBuild] No saved scene for {p.scene}; it stays a stand-in.");
                    p.built = false;
                    continue;
                }
                Scene before = SceneManager.GetActiveScene();
                Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                SceneManager.SetActiveScene(scene);
                p.look = PlaceLook.Capture();
                if (p.measure) Measure(scene, p);
                WorldProxies.Bake(scene, p);
                if (before.IsValid()) SceneManager.SetActiveScene(before);
                EditorSceneManager.CloseScene(scene, true);
                Debug.Log($"[WorldBuild] {p.scene}: sky {(p.look.skybox != null ? p.look.skybox.name : "none")}, fog {(p.look.fog ? p.look.fogDensity.ToString("0.0000") : "off")}.");
            }
        }

        private static void Measure(Scene scene, WorldPoi p)
        {
            Collider ground = null;
            float area = 400f;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
            {
                if (c.isTrigger) continue;
                Bounds b = c.bounds;
                float a = b.size.x * b.size.z;
                if (b.size.y < 2.5f && a > area && b.size.x < 2000f && b.size.z < 2000f && Mathf.Abs(b.max.y) < 6f) { area = a; ground = c; }
            }
            if (ground == null)
            {
                Debug.LogWarning($"[WorldBuild] {p.scene}: no flat ground collider found; keeping its planned site.");
                return;
            }
            // A district's ground is often several slabs round the building: all the large flat ones at the same level. A
            // small site (a yard, an apron and a verge round one building) counts every slab bigger than a parking bay.
            Bounds g = ground.bounds;
            float top = g.max.y, slab = Mathf.Min(2000f, area * .05f);
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Collider c in root.GetComponentsInChildren<Collider>(true))
            {
                Bounds b = c.bounds;
                if (!c.isTrigger && b.size.y < 2.5f && b.size.x * b.size.z > slab && b.size.x < 2000f && b.size.z < 2000f
                    && Mathf.Abs(b.max.y - top) < 1.5f) g.Encapsulate(b);
            }
            p.ground = Rect.MinMaxRect(g.min.x, g.min.z, g.max.x, g.max.z);
            p.groundHeight = top;
            var hide = new List<string>(p.hideInWorld);
            TruckCargo truck = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (TruckCargo t in root.GetComponentsInChildren<TruckCargo>(true))
                {
                    truck = t;
                    hide.Add(PathOf(t.transform));
                }
                foreach (Transform child in root.transform)
                {
                    // The summit's weather belongs to the loaded location, even though its volume extends past the site.
                    if (child.GetComponentInChildren<Abandoned.Skyscraper.SkyscraperCloudLayer>(true) != null) continue;
                    // Far scenery only: nothing anyone can touch or that the game drives.
                    if (child.GetComponentInChildren<Collider>(true) != null || child.GetComponentInChildren<NetworkObject>(true) != null) continue;
                    var renderers = child.GetComponentsInChildren<Renderer>(true);
                    if (renderers.Length == 0) continue;
                    Bounds b = renderers[0].bounds;
                    foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
                    if (b.min.x < g.min.x - 60f || b.max.x > g.max.x + 60f || b.min.z < g.min.z - 60f || b.max.z > g.max.z + 60f)
                        hide.Add(PathOf(child));
                }
            }
            p.hideInWorld = hide.ToArray();
            Transform park = truck != null ? truck.transform : Marker(scene, "VanParking") ?? Marker(scene, "VanDock");
            if (park != null) p.parking = new Vector3(park.position.x, park.eulerAngles.y, park.position.z);
            Debug.Log($"[WorldBuild] {p.scene}: ground {ground.name} {p.ground} at {p.groundHeight:0.00}; hides {string.Join(", ", p.hideInWorld)}; " +
                      $"parking {p.parking}.");
        }

        // A map without a truck of its own names where the crew's van stops (AGENT_REVIEW "No truck of your own").
        private static Transform Marker(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == name) return t;
            return null;
        }

        private static string PathOf(Transform t)
        {
            string path = t.name;
            for (Transform up = t.parent; up != null; up = up.parent) path = up.name + "/" + path;
            return path;
        }

        public static string ScenePath(string scene)
        {
            foreach (string guid in AssetDatabase.FindAssets($"{scene} t:Scene", new[] { "Assets/_Project/Scenes" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == scene) return path;
            }
            return null;
        }
    }
}
