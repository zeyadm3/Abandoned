using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Each store retains its merchandising pattern: fitted walls, till, then selected empty island displays.</summary>
    public static class MeridianDressingStoreDensity
    {
        private static readonly Vector2Int[] Sides = { Vector2Int.left, Vector2Int.up, Vector2Int.right, Vector2Int.down };

        public static void Place(MeridianDressingClearance clearance, Transform group)
        {
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Kind != MallLayout.Kind.Store || zone.Tag == "cinema") continue;
                Transform store = Group($"{floor}_{zone.Name}", group);
                bool till = false;
                for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
                for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
                {
                    var cell = new Vector2Int(x, z);
                    if (MallLayout.ZoneAt(cell, floor)?.Name != zone.Name || !MeridianDressingClearance.ClearCell(cell, floor)) continue;
                    foreach (Vector2Int side in Sides)
                    {
                        if (!Wall(cell, side, floor, zone)) continue;
                        string key = !till && zone.Tag != "gallery" ? "CheckoutCounter" : WallFixture(zone.Tag, cell);
                        Vector3 at = clearance.EdgePosition(key, cell, floor, side);
                        Quaternion rotation = Quaternion.LookRotation(new Vector3(-side.x, 0f, -side.y));
                        if (clearance.Place(key, store, floor, at, rotation)) till = true;
                        // Small custodial details occupy the unused end of alternate fixture bays.
                        if ((x + z) % 3 == 0)
                        {
                            string small = zone.Tag == "clothing" ? "Mannequin" : "MallBin";
                            Vector3 corner = MallLayout.TileTopCenter(cell, floor) + new Vector3(-1.4f, 0f, -1.4f);
                            clearance.Place(small, store, floor, corner, Quaternion.identity);
                        }
                    }
                    if (zone.Tag != "gallery" && (x + z) % 3 == 1)
                    {
                        string key = zone.Tag == "clothing" ? "ClothingRack"
                            : zone.Tag == "office" ? "SecurityDesk" : "RetailGondola";
                        clearance.Place(key, store, floor, MallLayout.TileTopCenter(cell, floor), Quaternion.identity);
                    }
                }
            }
            Basement(clearance, Group("WetStock", group));
        }

        private static string WallFixture(string tag, Vector2Int cell) => tag switch
        {
            "clothing" => "ClothingRack", "jewelry" => "EmptyDisplayCase", "office" => "SecurityDesk",
            "furniture" => (cell.x + cell.y) % 2 == 0 ? "FoodBooth" : "RetailShelf",
            "gallery" => "EmptyDisplayCase", "electronics" => (cell.x + cell.y) % 2 == 0 ? "RetailGondola" : "RetailShelf",
            _ => "RetailGondola",
        };

        private static bool Wall(Vector2Int cell, Vector2Int side, int floor, MallLayout.Zone zone)
        {
            Vector2Int neighbour = cell + side;
            MallLayout.Zone? other = MallLayout.ZoneAt(neighbour, floor);
            if (other?.Name == zone.Name) return false;
            if (MallLayout.DoorBetween(cell, neighbour, floor).HasValue || MallLayout.IsFlightOpening(cell, neighbour, floor)) return false;
            return !MallLayout.InGrid(neighbour) || !MallLayout.InFootprint(neighbour) || other.HasValue;
        }

        private static void Basement(MeridianDressingClearance clearance, Transform group)
        {
            RectInt area = MallLayout.Basement;
            for (int x = area.xMin + 1; x < area.xMax; x++)
            for (int z = area.yMin; z < 8; z++)
            {
                var cell = new Vector2Int(x, z);
                if (!MeridianDressingClearance.ClearCell(cell, -1)) continue;
                Vector2Int side = z == area.yMin ? Vector2Int.down : Vector2Int.right;
                Vector3 at = clearance.EdgePosition("StockCage", cell, -1, side);
                clearance.Place("StockCage", group, -1, at, Quaternion.LookRotation(new Vector3(-side.x, 0f, -side.y)));
                clearance.Place("ServiceCart", group, -1,
                    MallLayout.TileTopCenter(cell, -1) + new Vector3(-1.15f, 0f, 1.25f), Quaternion.identity);
            }
        }
    }
}
