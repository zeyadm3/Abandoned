using System.Collections.Generic;
using System.Linq;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Swaps every static loot look-alike in an already-saved level, in place. Prefab instances are swapped
    /// directly. The density layers merged their props into per-slab render batches, keeping only an empty
    /// holder (pose, collision, section) per prop: there the prop's exact triangles are cut out of the batch
    /// before the holder is swapped. Nothing else in the scene moves.
    /// </summary>
    public static class LootLookalikePass
    {
        public sealed class Result
        {
            public int Instances, Holders, TrianglesCut, Renamed;
            public readonly List<string> Missed = new();
        }

        private static readonly string[] BatchedLayers = { SalvageMallDensity.GroupName, MallExpansion0156.GroupName };

        public static Result Apply(Transform level)
        {
            var result = new Result();
            foreach (string name in BatchedLayers)
            {
                Transform layer = level.Find(name);
                if (layer != null) BatchedLayer(layer, result);
            }
            result.Renamed = RenameProxies(level);
            foreach ((GameObject item, string id) in Instances(level).ToList())
            {
                // Inside() placements (display contents, a tipped chair) have no section binding of their own.
                bool contents = item.GetComponent<SectionProp>() == null;
                LootLookalikeStandIns.Swap(item, id, contents);
                result.Instances++;
            }
            return result;
        }

        /// <summary>First 0.24.9 pass: copied root colliders kept the prop's name. Renames them; returns how many.</summary>
        private static int RenameProxies(Transform level)
        {
            int renamed = 0;
            foreach (Transform proxies in level.GetComponentsInChildren<Transform>(true))
            {
                if (proxies.name != "_CollisionProxies") continue;
                foreach (Transform part in proxies)
                {
                    string name = LootLookalikeStandIns.ProxyName(part.name);
                    if (name == part.name) continue;
                    part.name = name;
                    renamed++;
                }
            }
            return renamed;
        }

        /// <summary>Outermost scenery prefab instances made from a loot model.</summary>
        public static IEnumerable<(GameObject, string)> Instances(Transform level)
        {
            foreach (Transform node in level.GetComponentsInChildren<Transform>(true))
            {
                GameObject item = node.gameObject;
                if (!PrefabUtility.IsOutermostPrefabInstanceRoot(item)) continue;
                string id = LootLookalikes.IdOf(item);
                if (id != null) yield return (item, id);
            }
        }

        /// <summary>Unpacked density-layer holders still standing for a loot look-alike.</summary>
        public static IEnumerable<SectionProp> Holders(Transform layer)
        {
            Transform batches = layer.Find("SectionRenderBatches");
            return layer.GetComponentsInChildren<SectionProp>(true).Where(holder =>
                LootLookalikes.Is(holder.name) && (batches == null || !holder.transform.IsChildOf(batches)) &&
                holder.GetComponent<LootItem>() == null && holder.GetComponent<LootSpawnPoint>() == null &&
                !PrefabUtility.IsPartOfPrefabInstance(holder));
        }

        /// <summary>The render batch a batched holder's geometry went into, or null (the batcher's own key).</summary>
        public static Transform BatchCell(Transform layer, SectionProp holder)
        {
            Transform batches = layer.Find("SectionRenderBatches");
            if (batches == null || holder.Section == null || holder.GetComponentInChildren<Renderer>(true) != null) return null;
            Transform zone = holder.transform;
            while (zone.parent != null && zone.parent != layer) zone = zone.parent;
            return batches.Find(zone.name + "_" + holder.Section.name);
        }

        private static void BatchedLayer(Transform layer, Result result)
        {
            var byCell = new Dictionary<Transform, List<SectionProp>>();
            var plain = new List<SectionProp>();
            foreach (SectionProp holder in Holders(layer).ToList())
            {
                Transform cell = BatchCell(layer, holder);
                if (cell == null) { plain.Add(holder); continue; }
                if (!byCell.TryGetValue(cell, out List<SectionProp> list)) byCell[cell] = list = new List<SectionProp>();
                list.Add(holder);
            }

            foreach (KeyValuePair<Transform, List<SectionProp>> pair in byCell)
            {
                var cut = new BatchTriangleSet();
                var footprints = new Dictionary<SectionProp, (Bounds, int)>();
                foreach (SectionProp holder in pair.Value)
                {
                    Vector3 slab = holder.Section.transform.position;
                    footprints[holder] = Collect(holder, slab, cut);
                }
                int removed = 0;
                foreach (MeshFilter filter in pair.Key.GetComponentsInChildren<MeshFilter>(true))
                    removed += cut.Strip(filter.sharedMesh);
                result.TrianglesCut += removed;
                if (removed == 0)
                    result.Missed.Add($"{pair.Key.name}: none of {pair.Value.Count} prop(s) matched the batch");
                foreach (SectionProp holder in pair.Value)
                {
                    (Bounds footprint, int _) = footprints[holder];
                    // Surface stock lost its colliders when it was authored; anything solid stood on the floor.
                    bool contents = !LootLookalikeStandIns.HasCollision(holder.gameObject);
                    LootLookalikeStandIns.Swap(holder.gameObject, holder.name, contents, footprint);
                    result.Holders++;
                }
            }
            foreach (SectionProp holder in plain)
            {
                bool contents = !LootLookalikeStandIns.HasCollision(holder.gameObject);
                LootLookalikeStandIns.Swap(holder.gameObject, holder.name, contents);
                result.Holders++;
            }
        }

        /// <summary>
        /// Rebuilds the holder's original geometry exactly as the batcher saw it (same prefab, same parent and
        /// pose, same slab-relative matrix) and adds its triangles to <paramref name="cut"/>.
        /// </summary>
        public static (Bounds, int) Collect(SectionProp holder, Vector3 slab, BatchTriangleSet cut)
        {
            string id = holder.name;
            GameObject source = LootLookalikes.Source(id);
            if (source == null) return (new Bounds(holder.transform.position, Vector3.one * .5f), 0);
            var probe = (GameObject)PrefabUtility.InstantiatePrefab(source, holder.transform.parent);
            try
            {
                probe.transform.SetLocalPositionAndRotation(holder.transform.localPosition, holder.transform.localRotation);
                probe.transform.localScale = holder.transform.localScale;
                int added = 0;
                Matrix4x4 toCell = Matrix4x4.Translate(-slab);
                foreach (MeshFilter filter in probe.GetComponentsInChildren<MeshFilter>(true))
                    if (filter.sharedMesh != null) added += cut.Add(filter.sharedMesh, toCell * filter.transform.localToWorldMatrix);
                return (ModelFit.RendererBounds(probe), added);
            }
            finally { Object.DestroyImmediate(probe); }
        }
    }
}
