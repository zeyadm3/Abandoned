using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Decay follows light, leaks, access and collapse history instead of uniform random dirt.</summary>
    public static class MeridianDressingDecay
    {
        private static readonly Vector2[] SkylightLeaks = { new(24f, 20f), new(31f, 28f), new(21f, 30f) };

        public static void Place(Transform mall, Transform group)
        {
            Leaks(mall, Group("SkylightRunoff", group));
            Edges(mall, Group("DustAndPeeling", group));
            Scavengers(mall, Group("EarlierVisitors", group));
            Gallery(mall, Group("GalleryWound", group));
            Basement(mall, Group("FloodAndWaterline", group));
        }

        private static void Leaks(Transform mall, Transform group)
        {
            foreach (Vector2 leak in SkylightLeaks)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(leak.x / MallLayout.Tile), Mathf.FloorToInt(leak.y / MallLayout.Tile));
                StructuralSection support = Support(mall, cell, 0);
                if (support == null) continue;
                Prop("Puddle", group, new Vector3(leak.x, 0.012f, leak.y), Quaternion.Euler(0f, leak.x * 3f, 0f), support,
                    new Vector3(1.35f, 1f, 1.9f));
                // Growth stays beside leaked planters and wall feet, outside the primary haul lane.
                Prop("Overgrowth", group, new Vector3(leak.x + 0.65f, 0.01f, leak.y + 1.3f), Quaternion.identity, support,
                    new Vector3(0.6f, 0.7f, 0.6f));
            }
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            {
                EdgePlant(new Vector3(21.7f, MallLayout.FloorY(floor), 30.6f), floor);
                if (floor == 0) EdgePlant(new Vector3(34.3f, 0f, 30.6f), floor);
            }
            void EdgePlant(Vector3 at, int floor)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                StructuralSection support = Support(mall, cell, floor);
                if (support != null && Occupied(cell, floor)) Prop("Overgrowth", group, at, Quaternion.identity, support);
            }
        }

        private static void Edges(Transform mall, Transform group)
        {
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
            for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
            {
                var cell = new Vector2Int(x, z);
                if (!Occupied(cell, floor)) continue;
                if (MallLayout.ZoneAt(cell, floor)?.Name != zone.Name) continue;
                StructuralSection support = Support(mall, cell, floor);
                if (support == null) continue;
                int hash = Hash(cell, floor);
                Vector3 centre = MallLayout.TileTopCenter(cell, floor);
                foreach (Vector2Int side in new[] { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down })
                {
                    Vector2Int neighbour = cell + side;
                    if (zone.Contains(neighbour) || MallLayout.DoorBetween(cell, neighbour, floor).HasValue
                        || MallLayout.IsFlightOpening(cell, neighbour, floor)) continue;
                    MallLayout.Zone? other = MallLayout.InGrid(neighbour) ? MallLayout.ZoneAt(neighbour, floor) : null;
                    if (other.HasValue && MallLayout.SameSpace(zone, other.Value)) continue;
                    // An open balcony edge is not a wall that can hold a stain or conduit.
                    if (MallLayout.InGrid(neighbour) && !other.HasValue) continue;
                    Vector3 normal = new(side.x, 0f, side.y);
                    Vector3 at = centre + normal * (MallLayout.Tile * 0.5f - 0.15f);
                    if (hash % 5 == 0)
                        Prop("DebrisPile", group, at - normal * 0.28f + Vector3.up * 0.01f,
                            Quaternion.Euler(0f, hash % 360, 0f), support, new Vector3(0.42f, 0.28f, 0.42f));
                    bool wet = NearLeak(new Vector2(at.x, at.z), 6f) || zone.Tag == "food" && z == zone.Area.yMax - 1;
                    if (wet && hash % 3 == 0)
                        Patch("Runoff", group, at + Vector3.up * 1.8f - normal * 0.015f,
                            Quaternion.LookRotation(normal), new Vector2(1.3f, 2.5f), MeridianArt.Material("Moss"));
                    if (zone.Tag == "stock" && hash % 7 == 0)
                        Patch("PipeRust", group, at + Vector3.up * 2.25f - normal * 0.02f,
                            Quaternion.LookRotation(normal), new Vector2(0.35f, 1.2f), MeridianArt.Material("Rust"));
                }
            }
        }

        private static void Scavengers(Transform mall, Transform group)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 at = new(23.3f + i * 3.3f, 0.01f, 5.2f + i * 0.25f);
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                StructuralSection support = Support(mall, cell, 0);
                if (support != null) Prop("LeafScatter", group, at, Quaternion.Euler(0f, i * 47f, 0f), support);
            }
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Tag != "clothing" && zone.Tag != "electronics" && zone.Tag != "jewelry") continue;
                if (!ScavengerCell(zone, floor, out Vector2Int cell)) continue;
                StructuralSection support = Support(mall, cell, floor);
                if (support == null || !Occupied(cell, floor)) continue;
                Vector3 at = MallLayout.TileTopCenter(cell, floor);
                Prop("BrokenGlass", group, at + new Vector3(-0.85f, 0.012f, 0.9f), Quaternion.identity, support);
                if (zone.Tag == "clothing" && floor == 0)
                {
                    Prop("CampMattress", group, at + new Vector3(-1f, 0f, 1.1f), Quaternion.Euler(0f, 12f, 0f), support);
                    Prop("CampScorch", group, at + new Vector3(0.4f, 0.014f, 0.9f), Quaternion.identity, support);
                    int wallColumn = zone.Area.xMin == 0 ? 1 : zone.Area.xMin;
                    Prop("GraffitiTag", group, new Vector3(wallColumn * MallLayout.Tile + 0.16f,
                        MallLayout.FloorY(floor) + 1.65f, at.z), Quaternion.Euler(0f, 0f, -90f));
                }
            }
        }

        private static bool ScavengerCell(MallLayout.Zone zone, int floor, out Vector2Int cell)
        {
            for (int z = zone.Area.yMin + 1; z < zone.Area.yMax; z++)
            for (int x = zone.Area.xMin + 1; x < zone.Area.xMax; x++)
            {
                cell = new Vector2Int(x, z);
                if (Occupied(cell, floor) && MallLayout.ZoneAt(cell, floor)?.Name == zone.Name
                    && !MallLayout.IsProtected(cell, floor) && !MallLootPoints.IsJackpotTile(cell, floor)) return true;
            }
            cell = default;
            return false;
        }

        private static void Gallery(Transform mall, Transform group)
        {
            for (int x = 7; x <= 9; x++)
            {
                StructuralSection edge = Support(mall, new Vector2Int(x, 9), 2);
                Prop("BrokenSlabEdge", group, new Vector3(x * MallLayout.Tile + 2f, 8f, 40f), Quaternion.identity, edge);
                Prop("ExposedRebar", group, new Vector3(x * MallLayout.Tile + 1f, 7.8f, 40.15f), Quaternion.Euler(0f, 12f * x, 0f), edge);
                var cell = new Vector2Int(x, 10);
                StructuralSection support = Support(mall, cell, 1);
                if (support != null)
                    Prop("DebrisPile", group, MallLayout.TileTopCenter(cell, 1) + Vector3.up * 0.01f,
                        Quaternion.Euler(0f, x * 23f, 0f), support);
            }
            MeridianDressingSigns.Sign(group, new Vector3(27.75f, 10.1f, 38f), Vector3.left,
                "NORTH GALLERY / LOAD REDUCED", 3.2f, 0.35f);
        }

        private static void Basement(Transform mall, Transform group)
        {
            RectInt area = MallLayout.Basement;
            for (int x = area.xMin; x < area.xMax; x++)
            for (int z = area.yMin; z < area.yMax; z++)
            {
                var cell = new Vector2Int(x, z);
                StructuralSection support = Support(mall, cell, -1);
                if (support == null) continue;
                if (x != area.xMin || z % 2 != 0) continue;
                Prop("Overgrowth", group, MallLayout.TileTopCenter(cell, -1) + new Vector3(-1.7f, 0f, 0.7f), Quaternion.identity,
                    support, new Vector3(0.6f, 0.4f, 0.6f));
                Patch("FloodWaterline", group, new Vector3(area.xMin * MallLayout.Tile + 0.18f, -3.4f,
                    (z + 0.5f) * MallLayout.Tile), Quaternion.Euler(0f, -90f, 0f), new Vector2(3.9f, 0.45f), MeridianArt.Material("Moss"));
            }
        }

        private static bool NearLeak(Vector2 at, float radius)
        {
            foreach (Vector2 leak in SkylightLeaks) if ((at - leak).sqrMagnitude < radius * radius) return true;
            return false;
        }
    }
}
