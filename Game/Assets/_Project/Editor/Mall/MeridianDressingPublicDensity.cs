using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Seating and service fittings cluster around real food and circulation landmarks.</summary>
    public static class MeridianDressingPublicDensity
    {
        public static void Place(MeridianDressingClearance clearance, Transform group)
        {
            FoodCourt(clearance, Group("FoodSeatingAndService", group));
            PublicPockets(clearance, Group("CourtFurniture", group));
            Ducts(clearance, Group("ShopMechanicalRuns", group));
        }

        private static void FoodCourt(MeridianDressingClearance clearance, Transform group)
        {
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Tag != "food") continue;
                for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
                {
                    int first = zone.Area.yMin, last = zone.Area.yMax - 1;
                    while (first <= last && MallLayout.ZoneAt(new Vector2Int(x, first), floor)?.Name != zone.Name) first++;
                    while (last >= first && MallLayout.ZoneAt(new Vector2Int(x, last), floor)?.Name != zone.Name) last--;
                    if (last < first) continue;
                    var back = new Vector2Int(x, last);
                    clearance.Place("FoodBooth", group, floor,
                        clearance.EdgePosition("FoodBooth", back, floor, Vector2Int.up), Quaternion.Euler(0f, 180f, 0f));
                    var front = new Vector2Int(x, first);
                    if (x % 3 == 0)
                        clearance.Place("FoodServiceStation", group, floor,
                            clearance.EdgePosition("FoodServiceStation", front, floor, Vector2Int.down), Quaternion.identity);
                    else
                        clearance.Place("FoodCourtTable", group, floor, MallLayout.TileTopCenter(front, floor), Quaternion.identity);
                    if (x % 2 == 0)
                        clearance.Place("MallBin", group, floor,
                            MallLayout.TileTopCenter(back, floor) + new Vector3(1.45f, 0f, -1.35f), Quaternion.identity);
                }
            }
        }

        private static void PublicPockets(MeridianDressingClearance clearance, Transform group)
        {
            Add("ConcourseBench", 0, 18f, 5.6f, 0f);
            Add("AdvertisingLightbox", 0, 16.4f, 9.8f, 90f);
            Add("MallBin", 0, 19.4f, 9.6f, 0f);
            Add("ConcourseBench", 0, 38f, 5.6f, 0f);
            Add("AdvertisingLightbox", 0, 39.6f, 9.8f, -90f);
            Add("MallBin", 0, 36.6f, 9.6f, 0f);
            Add("ConcourseBench", 0, 34f, 16.5f, 0f);
            Add("AdvertisingLightbox", 0, 32.4f, 18.4f, 90f);
            Add("ConcourseBench", 0, 22f, 31.5f, 180f);
            Add("ShutteredKiosk", 0, 26f, 30f, 180f);
            Add("MallBin", 0, 34.8f, 31.4f, 0f);
            Add("ConcourseBench", 1, 34f, 31.5f, 180f);
            Add("MallBin", 1, 32.55f, 28.55f, 0f);
            Add("ConcourseBench", 2, 22f, 31.5f, 180f);
            Add("MallBin", 2, 20.55f, 28.55f, 0f);
            Add("ConcourseBench", 2, 34f, 31.5f, 180f);
            Add("AdvertisingLightbox", 2, 32.4f, 29f, 90f);
            Add("ConcourseBench", 2, 22f, 16.5f, 0f);

            void Add(string key, int floor, float x, float z, float yaw)
                => clearance.Place(key, group, floor, new Vector3(x, MallLayout.FloorY(floor), z), Quaternion.Euler(0f, yaw, 0f));
        }

        private static void Ducts(MeridianDressingClearance clearance, Transform group)
        {
            Vector3 scale = new(0.94f, 1f, 1f);
            for (int floor = 0; floor < MallLayout.Floors - 1; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Kind != MallLayout.Kind.Store || zone.Tag == "cinema" || zone.Tag == "gallery") continue;
                for (int z = zone.Area.yMin + 1; z < zone.Area.yMax; z += 2)
                {
                    var cell = new Vector2Int(zone.Area.xMin == 0 ? 1 : zone.Area.xMin, z);
                    if (MallLayout.ZoneAt(cell, floor)?.Name != zone.Name || !MeridianDressingClearance.ClearCell(cell, floor)) continue;
                    Vector3 at = clearance.EdgePosition("CeilingDuct", cell, floor, Vector2Int.left, scale);
                    at.y = MallLayout.FloorY(floor + 1) - 0.57f;
                    clearance.Place("CeilingDuct", group, floor, at, Quaternion.Euler(0f, 90f, 0f), scale, ceiling: true);
                }
            }
        }
    }
}
