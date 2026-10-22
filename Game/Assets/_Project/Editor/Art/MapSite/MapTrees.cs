using System.Collections.Generic;
using System.Text.RegularExpressions;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Swaps a map's code-built trees (cones and balls on sticks) for the scanned trees the TestMap uses (Poly Haven,
    /// baked to LODs and an impostor by Tools/Art/bake_tree.py). A tree is anything named like one that is shaped like
    /// one: taller than 2.5 m, no wider than it is tall, standing on its own (a merged belt of far trees stays as it is).
    /// The scanned tree stands where the old one stood, scaled to its height; the old one is switched off and recorded,
    /// so the pass can be run again or undone. Winter maps get evergreens and dead snags only.
    /// </summary>
    public static class MapTrees
    {
        private static readonly Regex TreeName = new(@"(?i)(tree|pine|spruce|conifer|cypress|birch|oak|maple|poplar|evergreen|(^|_)fir($|_))");
        // Parts of a tree, and far scenery: never swapped on their own.
        private static readonly Regex NotATree = new(@"(?i)(street|treeline|belt|horizon|distant|christmas|mountain|bark|foliage|crown|trunk|canopy|leaves|leaf|needle|island|planter|scrub|shrub|bush|hedge)");

        private readonly struct Kind
        {
            public readonly GameObject Prefab;
            public readonly float Height;

            public Kind(GameObject prefab, float height)
            {
                Prefab = prefab;
                Height = height;
            }
        }

        public static int Swap(Scene scene, MapSiteProfile profile, Bounds level, Transform parent, MapSiteRecord record)
        {
            List<Kind> kinds = Kinds(profile.Winter);
            if (kinds.Count == 0) return 0;
            Transform trees = GreyboxFactory.Group("Trees", parent);
            var random = new System.Random(scene.name.GetHashCode());
            int swapped = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.transform == parent) continue;
                Visit(root.transform);
            }
            return swapped;

            void Visit(Transform t)
            {
                if (!t.gameObject.activeSelf) return;
                if (TreeName.IsMatch(t.name) && !NotATree.IsMatch(t.name) && Unrelated(t))
                {
                    if (TreeShaped(t, out Bounds b))
                    {
                        Plant(t, b);
                        return;
                    }
                    // A merged patch of trees near the map (a verge, a boundary belt): each of its trees found and replaced.
                    if (b.size.y is > 2.5f and < 45f && b.Intersects(Grown(level, 40f)) && PlantPatch(t))
                        return;
                }
                for (int i = 0; i < t.childCount; i++) Visit(t.GetChild(i));
            }

            void Plant(Transform original, Bounds b)
            {
                Stand(new Vector3(b.center.x, b.min.y, b.center.z), b.size.y);
                original.gameObject.SetActive(false);
                record.EditorAdd(original.gameObject);
            }

            bool PlantPatch(Transform original)
            {
                List<(Vector3 foot, float height)> found = Cluster(original);
                if (found.Count == 0) return false;
                foreach ((Vector3 foot, float height) in found) Stand(foot, height);
                original.gameObject.SetActive(false);
                record.EditorAdd(original.gameObject);
                return true;
            }

            void Stand(Vector3 foot, float height)
            {
                Kind kind = Pick(kinds, height, random);
                var tree = (GameObject)PrefabUtility.InstantiatePrefab(kind.Prefab, trees);
                tree.transform.SetPositionAndRotation(foot, Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f));
                tree.transform.localScale = Vector3.one * Mathf.Clamp(height / kind.Height, .45f, 1.8f);
                GameObjectUtility.SetStaticEditorFlags(tree, StaticEditorFlags.OccludeeStatic);
                swapped++;
            }
        }

        private static Bounds Grown(Bounds b, float by)
        {
            b.Expand(by * 2f);
            return b;
        }

        // Nothing to do with the structure system or the network.
        private static bool Unrelated(Transform t) =>
            t.GetComponentInChildren<SectionProp>(true) == null && t.GetComponentInChildren<StructuralSection>(true) == null &&
            t.GetComponentInChildren<Unity.Netcode.NetworkObject>(true) == null;

        // The trees inside a merged mesh: its vertices binned on a 2 m grid, the densest bins taken as trunks (no two
        // closer than 3.5 m), each tree as tall as its bin's highest vertex above its lowest.
        private static List<(Vector3 foot, float height)> Cluster(Transform original)
        {
            var bins = new Dictionary<Vector2Int, (int count, float low, float high, Vector3 sum)>();
            foreach (MeshFilter f in original.GetComponentsInChildren<MeshFilter>())
            {
                if (f.sharedMesh == null || !f.GetComponent<Renderer>() || !f.GetComponent<Renderer>().enabled) continue;
                Matrix4x4 m = f.transform.localToWorldMatrix;
                foreach (Vector3 v in f.sharedMesh.vertices)
                {
                    Vector3 w = m.MultiplyPoint3x4(v);
                    var key = new Vector2Int(Mathf.FloorToInt(w.x / 2f), Mathf.FloorToInt(w.z / 2f));
                    bins.TryGetValue(key, out var bin);
                    bins[key] = bin.count == 0 ? (1, w.y, w.y, w) : (bin.count + 1, Mathf.Min(bin.low, w.y), Mathf.Max(bin.high, w.y), bin.sum + w);
                }
            }
            var found = new List<(Vector3, float)>();
            var sorted = new List<KeyValuePair<Vector2Int, (int count, float low, float high, Vector3 sum)>>(bins);
            sorted.Sort((a, b) => b.Value.count.CompareTo(a.Value.count));
            int floor = sorted.Count > 0 ? Mathf.Max(6, sorted[sorted.Count / 3].Value.count) : 0;
            foreach (var pair in sorted)
            {
                var bin = pair.Value;
                if (bin.count < floor || bin.high - bin.low < 2.5f) continue;
                Vector3 c = bin.sum / bin.count;
                var foot = new Vector3(c.x, bin.low, c.z);
                if (found.Exists(f => (new Vector2(f.Item1.x, f.Item1.z) - new Vector2(foot.x, foot.z)).sqrMagnitude < 3.5f * 3.5f)) continue;
                found.Add((foot, bin.high - bin.low));
                if (found.Count >= 120) break;
            }
            return found;
        }

        // Tall, narrow and alone (the bounds are returned either way).
        private static bool TreeShaped(Transform t, out Bounds bounds)
        {
            bounds = default;
            bool any = false;
            foreach (Renderer r in t.GetComponentsInChildren<Renderer>())
            {
                if (!r.enabled) continue;
                if (!any) { bounds = r.bounds; any = true; }
                else bounds.Encapsulate(r.bounds);
            }
            if (!any) return false;
            float h = bounds.size.y, w = Mathf.Max(bounds.size.x, bounds.size.z);
            return h is > 2.5f and < 40f && w < 18f && w < h * 1.4f;
        }

        // A kind whose own height is within reach of the one replaced (so it isn't stretched), at random among them.
        private static Kind Pick(List<Kind> kinds, float height, System.Random random)
        {
            var near = kinds.FindAll(k => height / k.Height is > .6f and < 1.6f);
            if (near.Count > 0) return near[random.Next(near.Count)];
            Kind best = kinds[0];
            foreach (Kind k in kinds)
                if (Mathf.Abs(Mathf.Log(height / k.Height)) < Mathf.Abs(Mathf.Log(height / best.Height))) best = k;
            return best;
        }

        private static List<Kind> Kinds(bool winter)
        {
            var kinds = new List<Kind>();
            void Add(string id, string variant)
            {
                if (variant.Contains("_LOD")) return; // a scan's own lower LODs, not kinds of tree
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{ShowcaseNature.Prefabs}/{variant}.prefab") ??
                                    ShowcaseNature.TreePrefab(id, variant, .25f, new Vector3(.9f, .3f, .004f));
                if (prefab == null) return;
                float h = 0f;
                foreach (MeshFilter f in prefab.GetComponentsInChildren<MeshFilter>())
                    if (f.sharedMesh != null) h = Mathf.Max(h, f.sharedMesh.bounds.max.y * f.transform.lossyScale.y);
                if (h > .5f) kinds.Add(new Kind(prefab, h));
            }
            foreach (string v in new[] { "fir_tree_01_a", "fir_tree_01_b", "fir_tree_01_c" }) Add("fir_tree_01", v);
            foreach (string v in new[] { "fir_sapling_medium_a", "fir_sapling_medium_b", "fir_sapling_medium_c" }) Add("fir_sapling_medium", v);
            foreach (string v in ShowcaseNature.Parts("pine_sapling_small")) Add("pine_sapling_small", v);
            if (!winter) foreach (string v in ShowcaseNature.Parts("tree_small_02")) Add("tree_small_02", v);
            if (winter) foreach (string v in ShowcaseNature.Parts("dead_tree_trunk_02")) Add("dead_tree_trunk_02", v);
            return kinds;
        }

        // Kinds and their heights are also what the ring's forest plants (MapRing).
        public static (GameObject prefab, float height)[] Forest(bool winter)
        {
            List<Kind> kinds = Kinds(winter);
            var list = new (GameObject, float)[kinds.Count];
            for (int i = 0; i < kinds.Count; i++) list[i] = (kinds[i].Prefab, kinds[i].Height);
            return list;
        }
    }
}
