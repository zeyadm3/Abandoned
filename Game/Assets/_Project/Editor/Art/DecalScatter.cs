using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.Universal;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// 0.24.12 wear: scatters grime at the foot of walls, water stains on ceilings, floor stains, rust runs and soot
    /// through the open scene, from points on its baked NavMesh (so only where people walk). Seeded by the scene's
    /// name, so a rerun puts every decal back where it was. Cosmetic only: no colliders, nothing networked. The
    /// group "_AV_Decals" is replaced on every run. Iron works get more soot and rust; the HQ gets little.
    /// </summary>
    public static class DecalScatter
    {
        private const string Root = "_AV_Decals";

        [MenuItem(ToolMenu.Art + "Scatter Wear Decals (Open Scene)", priority = ToolMenu.ArtP + 63)]
        public static void ScatterOpenScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            int n = Scatter(scene.name);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Decals] {scene.name}: {n} wear decals.");
        }

        public static int Scatter(string sceneName)
        {
            GameObject old = GameObject.Find(Root);
            if (old != null) Object.DestroyImmediate(old);
            NavMeshTriangulation nav = NavMesh.CalculateTriangulation();
            if (nav.indices.Length < 3) { Debug.LogWarning($"[Decals] {sceneName} has no NavMesh; no decals."); return 0; }

            string n = sceneName.ToLowerInvariant();
            bool hq = n.Contains("hq"), works = n.Contains("factory");
            var rng = new System.Random(StableHash(sceneName) ^ 0x24c12);
            var root = new GameObject(Root).transform;
            int mask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");

            // Triangles by area, so a big hall gets more than a cupboard.
            var area = new List<float>();
            float total = 0f;
            for (int i = 0; i < nav.indices.Length; i += 3)
            {
                Vector3 a = nav.vertices[nav.indices[i]], b = nav.vertices[nav.indices[i + 1]], c = nav.vertices[nav.indices[i + 2]];
                total += Vector3.Cross(b - a, c - a).magnitude * 0.5f;
                area.Add(total);
            }
            int count = Mathf.Clamp(Mathf.RoundToInt(total / (hq ? 60f : 22f)), 20, hq ? 120 : 700);
            int made = 0;
            for (int k = 0; k < count; k++)
            {
                Vector3 p = PointOn(nav, area, total, rng);
                float roll = (float)rng.NextDouble();
                if (roll < 0.35f) made += Floor(root, p, rng, mask) ? 1 : 0;
                else if (roll < 0.75f) made += Wall(root, p, rng, mask, works) ? 1 : 0;
                else made += Ceiling(root, p, rng, mask, works) ? 1 : 0;
            }
            return made;
        }

        // FNV-1a: string.GetHashCode may differ between runtimes, and the scatter must land in the same places every run.
        private static int StableHash(string s)
        {
            unchecked
            {
                uint h = 2166136261;
                foreach (char c in s) { h ^= c; h *= 16777619; }
                return (int)h;
            }
        }

        private static Vector3 PointOn(NavMeshTriangulation nav, List<float> area, float total, System.Random rng)
        {
            float pick = (float)rng.NextDouble() * total;
            int found = area.BinarySearch(pick);
            int tri = Mathf.Clamp(found < 0 ? ~found : found, 0, area.Count - 1) * 3;
            Vector3 a = nav.vertices[nav.indices[tri]], b = nav.vertices[nav.indices[tri + 1]], c = nav.vertices[nav.indices[tri + 2]];
            float r1 = Mathf.Sqrt((float)rng.NextDouble()), r2 = (float)rng.NextDouble();
            return (1f - r1) * a + r1 * (1f - r2) * b + r1 * r2 * c;
        }

        private static bool Floor(Transform root, Vector3 p, System.Random rng, int mask)
        {
            if (!Physics.Raycast(p + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 1.5f, mask, QueryTriggerInteraction.Ignore)) return false;
            float size = 0.8f + (float)rng.NextDouble() * 1.8f;
            // The box reaches 3 cm above the floor: a stain, not a stain on whatever stands on it.
            Place(root, "FloorStain", DecalAssets.FloorStain, hit.point + Vector3.up * 0.03f, Quaternion.LookRotation(Vector3.down, Rand(rng)),
                new Vector3(size, size, 0.4f), new Vector3(0f, 0f, 0.2f));
            return true;
        }

        private static bool Wall(Transform root, Vector3 p, System.Random rng, int mask, bool works)
        {
            Vector3 dir = Rand(rng);
            if (!Physics.Raycast(p + Vector3.up * 0.4f, dir, out RaycastHit hit, 4f, mask, QueryTriggerInteraction.Ignore)) return false;
            if (Mathf.Abs(hit.normal.y) > 0.3f) return false;
            Vector3 n = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
            float width = 1.4f + (float)rng.NextDouble() * 2f;
            bool rust = works && rng.NextDouble() < 0.4;
            if (rust)
            {
                // Boxes sit mostly inside the wall (5 cm out of it), so shelves and props against it stay clean.
                Vector3 top = hit.point + Vector3.up * (1.6f + (float)rng.NextDouble());
                Place(root, "RustRun", DecalAssets.RustRun, top - n * 0.15f, Quaternion.LookRotation(-n, Vector3.up), new Vector3(width * 0.5f, 1.8f, 0.4f), Vector3.zero);
            }
            else
            {
                // Grime rises from the floor: the decal's bottom edge sits at the floor.
                Vector3 at = new Vector3(hit.point.x, p.y + 0.6f, hit.point.z);
                Place(root, "WallGrime", DecalAssets.Grime, at - n * 0.15f, Quaternion.LookRotation(-n, Vector3.up), new Vector3(width, 1.2f, 0.4f), Vector3.zero);
            }
            return true;
        }

        private static bool Ceiling(Transform root, Vector3 p, System.Random rng, int mask, bool works)
        {
            if (!Physics.Raycast(p + Vector3.up * 0.5f, Vector3.up, out RaycastHit hit, 8f, mask, QueryTriggerInteraction.Ignore)) return false;
            if (hit.normal.y > -0.7f) return false;
            float size = 0.9f + (float)rng.NextDouble() * 1.8f;
            Material m = works && rng.NextDouble() < 0.6 ? DecalAssets.Soot : DecalAssets.WaterStain;
            // The box reaches 3 cm below the ceiling, clear of the fixtures hung from it.
            Place(root, m == DecalAssets.Soot ? "Soot" : "WaterStain", m, hit.point - Vector3.up * 0.03f, Quaternion.LookRotation(Vector3.up, Rand(rng)),
                new Vector3(size, size, 0.4f), new Vector3(0f, 0f, 0.2f));
            return true;
        }

        private static void Place(Transform root, string name, Material m, Vector3 at, Quaternion facing, Vector3 size, Vector3 pivot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.transform.SetPositionAndRotation(at, facing);
            var d = go.AddComponent<DecalProjector>();
            d.material = m;
            d.size = size;
            d.pivot = pivot;
            d.drawDistance = 30f;
            d.fadeScale = 0.85f;
        }

        private static Vector3 Rand(System.Random rng)
        {
            float a = (float)(rng.NextDouble() * Mathf.PI * 2f);
            return new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
        }
    }
}
