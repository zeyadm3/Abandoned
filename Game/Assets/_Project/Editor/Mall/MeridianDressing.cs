using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MeridianDressingUtility;

namespace Abandoned.EditorTools
{
    /// <summary>Empty fitted displays distinguish fixed dressing from the crew's salvageable loot.</summary>
    public static class MeridianDressing
    {
        public static void Place(Transform mall)
        {
            Transform group = Group("MeridianDressing", mall);
            Landmarks(mall, Group("Landmarks", group));
            BackOfHouse(mall, Group("ServiceDetail", group));
            MeridianDressingLandmarks.Place(mall, Group("CinemaAndFoodCourt", group));
            MeridianDressingCeilings.Place(mall, Group("Ceilings", group));
            MeridianDressingDecay.Place(mall, Group("CausalDecay", group));
            MeridianDressingDensity.Place(mall, Group("FittedDensity", group));
            MeridianDressingSigns.Place(mall, Group("Signs", group));
        }

        private static void Landmarks(Transform mall, Transform group)
        {
            FloorProp("Fountain", new Vector3(28f, 0f, 26f), 0);
            FloorProp("MallDirectory", new Vector3(18f, 0f, 9.4f), 0, 20f);
            FloorProp("Kiosk", new Vector3(26f, 0f, 18f), 0);
            FloorProp("Planter", new Vector3(34.2f, 0f, 29.5f), 0);
            FloorProp("Planter", new Vector3(22f, 4f, 30f), 1);
            FloorProp("Planter", new Vector3(22f, 8f, 18f), 2);
            for (int floor = 0; floor < MallLayout.Floors; floor++)
            foreach (MallLayout.Zone zone in MallLayout.ZonesByFloor[floor])
            {
                if (zone.Tag != "food") continue;
                for (int x = zone.Area.xMin + 1; x < zone.Area.xMax - 1; x += 2)
                {
                    int row = zone.Area.yMin;
                    while (row < zone.Area.yMax && MallLayout.ZoneAt(new Vector2Int(x, row), floor)?.Name != zone.Name) row++;
                    if (row >= zone.Area.yMax) continue;
                    if (zone.Kind != MallLayout.Kind.Walkway)
                        FloorProp("FoodCourtCounter", new Vector3((x + 0.5f) * MallLayout.Tile,
                            MallLayout.FloorY(floor), zone.Area.yMax * MallLayout.Tile - 0.8f), floor);
                }
            }

            void FloorProp(string key, Vector3 at, int floor, float yaw = 0f)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(at.x / MallLayout.Tile), Mathf.FloorToInt(at.z / MallLayout.Tile));
                if (!Occupied(cell, floor) || MallLootPoints.IsJackpotTile(cell, floor)) return;
                if (MallLayout.AllFlights.Any(f => (f.Floor == floor || f.Floor + 1 == floor) && f.Tiles.Contains(cell))) return;
                StructuralSection support = Support(mall, cell, floor);
                if (support != null) Prop(key, group, at, Quaternion.Euler(0f, yaw, 0f), support);
            }
        }

        private static void BackOfHouse(Transform mall, Transform group)
        {
            for (int floor = -1; floor < MallLayout.Floors; floor++)
            {
                int start = floor < 0 ? MallLayout.Basement.yMin : 1;
                for (int z = start; z < MallLayout.TilesZ - 1; z += 2)
                {
                    Vector3 at = new(40.18f, MallLayout.FloorY(floor) + 2.8f, (z + 0.5f) * MallLayout.Tile);
                    var cell = new Vector2Int(10, z);
                    if (!Occupied(cell, floor)) continue;
                    Prop("WallConduit", group, at, Quaternion.Euler(0f, 90f, 0f));
                    if (floor < 0 || z >= 7)
                        Prop("WallPipe", group, at + Vector3.down * 0.35f, Quaternion.Euler(0f, 90f, 0f));
                }
            }
        }
    }
}
