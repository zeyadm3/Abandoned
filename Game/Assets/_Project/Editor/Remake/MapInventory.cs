using System.Collections.Generic;
using System.IO;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Abandoned.EditorTools.Remake
{
    /// <summary>
    /// Phase 6's first step: what a saved map draws, grouped by mesh, so the map remake (KitSwap) knows which meshes
    /// carry the picture. Writes Game/Logs/remake/maps/&lt;Scene&gt;_inventory.json (not an asset folder): per mesh its
    /// asset, vertex count, how often it is drawn, its size, materials, a role guessed from object names, and whether
    /// it hangs under a StructuralSection (those keep their fracture stages).
    /// </summary>
    public static class MapInventory
    {
        public const string OutFolder = "Logs/remake/maps";

        [System.Serializable]
        public class Item
        {
            public string mesh, asset, role, example;
            public int vertices, uses, structural;
            public Vector3 size;
            public List<string> materials = new();
        }

        [System.Serializable]
        private class Report { public string scene; public int renderers, meshes; public List<Item> items = new(); }

        [MenuItem(ToolMenu.Art + "Remake: Inventory Open Map", priority = ToolMenu.ArtP + 102)]
        public static void InventoryOpen() => Write(EditorSceneManager.GetActiveScene());

        public static string Run(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return Write(scene);
        }

        public static string Write(UnityEngine.SceneManagement.Scene scene)
        {
            var byMesh = new Dictionary<Mesh, Item>();
            int renderers = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.enabled || !r.gameObject.activeInHierarchy) continue;
                var mf = r.GetComponent<MeshFilter>();
                Mesh mesh = mf != null ? mf.sharedMesh : null;
                if (mesh == null) continue;
                renderers++;
                if (!byMesh.TryGetValue(mesh, out Item item))
                {
                    item = new Item
                    {
                        mesh = mesh.name, asset = AssetDatabase.GetAssetPath(mesh), vertices = mesh.vertexCount,
                        size = mesh.bounds.size, role = Role(r.transform), example = PathOf(r.transform),
                        materials = r.sharedMaterials.Select(m => m != null ? m.name : "null").ToList()
                    };
                    byMesh[mesh] = item;
                }
                item.uses++;
                if (r.GetComponentInParent<StructuralSection>(true) != null) item.structural++;
            }
            var report = new Report { scene = scene.name, renderers = renderers, meshes = byMesh.Count };
            report.items = byMesh.Values.OrderByDescending(i => (long)i.uses * i.vertices).ToList();
            Directory.CreateDirectory(OutFolder);
            string path = Path.Combine(OutFolder, scene.name + "_inventory.json");
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
            string line = $"[Remake] {scene.name}: {renderers} renderers, {byMesh.Count} meshes -> {path}";
            Debug.Log(line);
            return line;
        }

        private static readonly string[] Roles =
        {
            "wall", "floor", "slab", "ceiling", "roof", "stair", "step", "ramp", "rail", "window", "glass", "door", "frame",
            "column", "pillar", "beam", "facade", "trim", "skirting", "light", "lamp", "fixture", "shelf", "shelv", "desk",
            "table", "chair", "bench", "counter", "cabinet", "locker", "bed", "sofa", "toilet", "sink", "pipe", "duct",
            "vent", "cable", "machine", "tree", "fence", "road", "kerb", "curb", "car", "truck", "sign", "rubble", "crate",
            "box", "barrel", "bin", "pallet", "plant", "ground", "terrain"
        };

        /// <summary>The first role word found in the object's own name, then its parents' (nearest first).</summary>
        private static string Role(Transform t)
        {
            for (Transform p = t; p != null; p = p.parent)
            {
                string low = p.name.ToLowerInvariant();
                foreach (string role in Roles)
                    if (low.Contains(role)) return role;
            }
            return "other";
        }

        private static string PathOf(Transform t)
        {
            var names = new List<string>();
            for (Transform p = t; p != null && names.Count < 4; p = p.parent) names.Add(p.name);
            names.Reverse();
            return string.Join("/", names);
        }
    }
}
