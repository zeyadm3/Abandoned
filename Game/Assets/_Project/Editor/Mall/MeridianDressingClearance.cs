using System.Collections.Generic;
using System.Linq;
using Abandoned.Interaction;
using Abandoned.Loot;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Fitted mall fixtures reserve real mesh footprints while leaving the carry network empty.</summary>
    public sealed class MeridianDressingClearance
    {
        private readonly Transform mall;
        private readonly LootSpawnPoint[] loot;
        private readonly List<Bounds> occupied = new();
        private readonly Dictionary<string, Bounds> models = new();
        private readonly Dictionary<string, int> counts = new();
        private readonly int[] floorCounts = new int[4];

        public MeridianDressingClearance(Transform mall)
        {
            this.mall = mall;
            loot = mall.GetComponentsInChildren<LootSpawnPoint>(true);
            foreach (SectionProp prop in mall.GetComponentsInChildren<SectionProp>(true))
            {
                if (prop.Section == null || Mathf.Abs(prop.transform.position.y - prop.Section.transform.position.y) > 0.12f) continue;
                Renderer[] renderers = prop.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) continue;
                Bounds bounds = renderers[0].bounds;
                foreach (Renderer renderer in renderers) bounds.Encapsulate(renderer.bounds);
                if (bounds.size.y > 0.18f) occupied.Add(bounds);
            }
        }

        public Bounds ModelBounds(string key)
        {
            if (models.TryGetValue(key, out Bounds cached)) return cached;
            GameObject source = MeridianArt.Model(key);
            if (source == null) throw new System.InvalidOperationException($"Missing Meridian dressing model {key}.");
            bool first = true;
            Bounds result = default;
            foreach (Renderer renderer in source.GetComponentsInChildren<Renderer>(true))
            {
                Bounds local = renderer.localBounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 p = local.center + Vector3.Scale(local.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    p = source.transform.InverseTransformPoint(renderer.transform.TransformPoint(p));
                    if (first) { result = new Bounds(p, Vector3.zero); first = false; }
                    else result.Encapsulate(p);
                }
            }
            if (first) throw new System.InvalidOperationException($"Meridian dressing model {key} has no renderer bounds.");
            models[key] = result;
            return result;
        }

        public bool Place(string key, Transform parent, int floor, Vector3 position, Quaternion rotation,
            Vector3? scale = null, bool ceiling = false)
        {
            Vector3 actualScale = scale ?? Vector3.one;
            Bounds bounds = TransformBounds(ModelBounds(key), Matrix4x4.TRS(position, rotation, actualScale));
            var cell = new Vector2Int(Mathf.FloorToInt(position.x / MallLayout.Tile), Mathf.FloorToInt(position.z / MallLayout.Tile));
            if (!ClearCell(cell, floor)) return false;
            float minX = cell.x * MallLayout.Tile + 0.04f, minZ = cell.y * MallLayout.Tile + 0.04f;
            if (bounds.min.x < minX || bounds.max.x > minX + MallLayout.Tile - 0.08f
                || bounds.min.z < minZ || bounds.max.z > minZ + MallLayout.Tile - 0.08f) return false;
            if (!ceiling)
            {
                foreach (LootSpawnPoint point in loot)
                {
                    if (Mathf.Abs(point.transform.position.y - MallLayout.FloorY(floor)) > 0.2f) continue;
                    Vector3 nearest = bounds.ClosestPoint(point.transform.position);
                    float clearance = point.MaxClass >= CarryClass.Heavy ? 1.35f : 0.80f;
                    if (new Vector2(nearest.x - point.transform.position.x, nearest.z - point.transform.position.z).sqrMagnitude
                        < clearance * clearance) return false;
                }
            }
            Bounds separation = bounds;
            separation.Expand(0.12f);
            if (occupied.Any(b => b.Intersects(separation))) return false;
            StructuralSection support = Support(mall, cell, ceiling ? floor + 1 : floor);
            if (support == null) return false;
            Prop(key, parent, position, rotation, support, actualScale);
            occupied.Add(bounds);
            counts[key] = counts.TryGetValue(key, out int count) ? count + 1 : 1;
            floorCounts[floor + 1]++;
            return true;
        }

        public Vector3 EdgePosition(string key, Vector2Int cell, int floor, Vector2Int side, Vector3? scale = null)
        {
            Vector3 normal = new(side.x, 0f, side.y);
            Quaternion rotation = Quaternion.LookRotation(-normal);
            Bounds bounds = TransformBounds(ModelBounds(key), Matrix4x4.TRS(Vector3.zero, rotation, scale ?? Vector3.one));
            float reach = Vector3.Dot(bounds.center, normal)
                + Mathf.Abs(normal.x) * bounds.extents.x + Mathf.Abs(normal.z) * bounds.extents.z;
            return MallLayout.TileTopCenter(cell, floor) + normal * (MallLayout.Tile * 0.5f - 0.12f - reach);
        }

        public static bool ClearCell(Vector2Int cell, int floor)
        {
            if (!Occupied(cell, floor) || MallLayout.IsProtected(cell, floor) || MallLootPoints.IsJackpotTile(cell, floor)) return false;
            // The balcony ring is exactly four metres wide; furniture belongs in its wider corner platforms.
            if (MallLayout.Ring.Contains(cell) && !MallLayout.Atrium.Contains(cell)) return false;
            if (MallLayout.Doors.Any(d => d.Floor == floor && (d.A == cell || d.B == cell))) return false;
            return !MallLayout.AllFlights.Any(f => (f.Floor == floor || f.Floor + 1 == floor)
                && (f.Tiles.Contains(cell) || cell == f.Start - f.Dir || cell == f.Start + f.Dir * 2));
        }

        public void Report()
        {
            string detail = string.Join(", ", counts.OrderBy(p => p.Key).Select(p => $"{p.Key}={p.Value}"));
            Debug.Log($"[MeridianDressingDensity] Added {floorCounts.Sum()} fitted modules "
                + $"(basement {floorCounts[0]}, ground {floorCounts[1]}, first {floorCounts[2]}, top {floorCounts[3]}). {detail}");
        }

        private static Bounds TransformBounds(Bounds source, Matrix4x4 matrix)
        {
            Bounds result = new(matrix.MultiplyPoint3x4(source.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = source.center + Vector3.Scale(source.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                result.Encapsulate(matrix.MultiplyPoint3x4(corner));
            }
            return result;
        }
    }
}
