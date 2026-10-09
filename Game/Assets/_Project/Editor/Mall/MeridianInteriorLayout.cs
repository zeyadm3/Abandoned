using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Organized retail bays replace scattered fittings without altering the structural grid.</summary>
    public static class MeridianInteriorLayout
    {
        public const string GroupName = "MeridianInteriorLayout";

        public static void Build(Transform mall)
        {
            Remove(mall.Find(GroupName));
            // The old pass used one generic display per free tile; the new bays define deliberate aisles.
            Remove(mall.Find("MeridianDressing/FittedDensity/FittedStores"));
            Remove(mall.Find("MeridianDressing/FittedDensity/CinemaInterior"));
            Transform root = Group(GroupName, mall);
            var clearance = new MeridianDressingClearance(mall);
            var routes = new MallRouteClearance(mall);
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            {
                Transform level = Group($"RetailLevel_{floor}", root);
                for (int x = 0; x < MallLayout.TilesX; x++)
                for (int z = 0; z < MallLayout.TilesZ; z++)
                {
                    var cell = new Vector2Int(x, z);
                    var zone = MallLayout.ZoneAt(cell, floor);
                    if (!zone.HasValue || !MeridianDressingClearance.ClearCell(cell, floor)) continue;
                    string tag = zone.Value.Tag;
                    if (zone.Value.Kind != MallLayout.Kind.Store && tag != "food") continue;
                    if (tag == "gallery") continue;
                    bool cinema = tag == "cinema";
                    if (cinema && z < 4) continue;
                    bool perimeter = Wall(cell, floor, zone.Value, out Vector2Int side);
                    // Wide empty lanes connect perimeter departments, with paired islands in showrooms.
                    if (!cinema && !perimeter && (x + z + floor) % 3 != 0) continue;
                    Vector3 at = MallLayout.TileTopCenter(cell, floor);
                    Bounds footprint = new(at + Vector3.up * 1.35f, new Vector3(3.4f, 2.7f, 3.4f));
                    StructuralSection support = Support(mall, cell, floor);
                    if (support == null || routes.Conflict(footprint) != null || !clearance.Reserve(footprint, floor)) continue;
                    Transform bay = Group($"{tag}_Bay_{x}_{z}", level);
                    bay.localPosition = at;
                    bay.localRotation = Quaternion.LookRotation(perimeter && !cinema ? new Vector3(side.x, 0, side.y) : Vector3.forward);
                    int variant = Hash(cell, floor);
                    if (cinema) variant = z == 4 || z == 9 ? 0 : 1;
                    MeridianInteriorFixtures.Build(tag, bay, variant);
                    bay.gameObject.AddComponent<SectionProp>().EditorSetup(support);
                }
            }
            CinemaLobby(mall, clearance, Group("CinemaLobby", root));
        }

        private static bool Wall(Vector2Int cell, int floor, MallLayout.Zone zone, out Vector2Int side)
        {
            foreach (Vector2Int dir in new[] { Vector2Int.up, Vector2Int.left, Vector2Int.right, Vector2Int.down })
            {
                if (MallLayout.ZoneAt(cell + dir, floor)?.Name == zone.Name) continue;
                if (MallLayout.DoorBetween(cell, cell + dir, floor).HasValue || MallLayout.IsFlightOpening(cell, cell + dir, floor)) continue;
                side = dir;
                return true;
            }
            side = Vector2Int.zero;
            return false;
        }

        private static void CinemaLobby(Transform mall, MeridianDressingClearance clearance, Transform parent)
        {
            clearance.Place("TicketCounter", parent, 2, new Vector3(10f, 8f, 9.1f), Quaternion.identity);
            clearance.Place("FoodServiceStation", parent, 2, new Vector3(5.8f, 8f, 9.4f), Quaternion.identity);
            foreach (float z in new[] { 6f, 10f })
                clearance.Place("ConcourseBench", parent, 2, new Vector3(13.25f, 8f, z), Quaternion.Euler(0, -90, 0));
        }

        private static void Remove(Transform prior)
        {
            if (prior != null) Object.DestroyImmediate(prior.gameObject);
        }
    }
}
