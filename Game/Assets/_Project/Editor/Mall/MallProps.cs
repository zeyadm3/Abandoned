using System.Linq;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>Floor-supported fixtures with collisions measured in their authored, rotated model space.</summary>
    public static class MallProps
    {
        public static int Place(Transform root, Transform tiles)
        {
            Transform props = GreyboxFactory.Group("Props", root);
            int count = 0;
            for (int floor = 0; floor < Floors; floor++)
            foreach (Zone zone in ZonesByFloor[floor])
            {
                if (zone.Kind != Kind.Store || zone.Name == "ServicePassage") continue;
                for (int z = zone.Area.yMin + 1; z < zone.Area.yMax; z += 2)
                {
                    var cell = new Vector2Int(zone.Area.xMin, z);
                    if (IsVoid(cell, floor) || Flights.Any(f => f.Floor == floor && f.Tiles.Contains(cell))
                        || MallLootPoints.IsJackpotTile(cell, floor) || DoorBetween(cell, cell + Vector2Int.left, floor).HasValue) continue;
                    string model = zone.Tag switch
                    {
                        "clothing" => "Mannequin", "cinema" => "CinemaSeat", "office" => "SecurityDesk",
                        "furniture" => "Bench", _ => "RetailShelf",
                    };
                    float depth = model == "SecurityDesk" ? 1f : model == "Mannequin" ? .34f : .62f;
                    Vector3 centre = TileTopCenter(cell, floor);
                    Add(model, centre + new Vector3(-Tile * .5f + MallWalls.Thickness * .5f + depth * .5f + .05f, 0f, .3f), floor, 90f);
                }
            }
            Add("Fountain", new Vector3(28f, 0f, 26f), 0);
            Add("Kiosk", new Vector3(18f, 0f, 16f), 0);
            Add("Kiosk", new Vector3(38f, 0f, 28f), 0);
            for (int i = 0; i < 6; i++)
            {
                Add("FoodCourtTable", new Vector3(4f + i * 5f, 0f, 44f), 0, i * 13f);
                Add("Counter", new Vector3(3f + i * 6f, 0f, 47f), 0);
            }
            for (int i = 0; i < 3; i++) Add("Bench", new Vector3(18f + 10f * i, 0f, 8f), 0);
            for (int row = 0; row < 6; row++)
            for (int column = 0; column < 3; column++)
                Add("CinemaSeat", new Vector3(2f + column * 1.2f, 8f, 8f + row * 3f), 2, 180f);
            MallMysteryBuilder.Sign(props, new Vector3(7.5f, 10f, 1f), Vector3.forward, "THE LAST SCREENING", 7f, 1.8f);
            return count;

            void Add(string model, Vector3 floorPosition, int floor, float yaw = 0f)
            {
                var cell = new Vector2Int(Mathf.FloorToInt(floorPosition.x / Tile), Mathf.FloorToInt(floorPosition.z / Tile));
                if (!InGrid(cell) || IsVoid(cell, floor)) return;
                StructuralSection support = tiles.Find($"Floor_{floor}/{MallBuilder.TileName(cell, floor)}")?.GetComponent<StructuralSection>();
                GameObject prop = CustomMallArt.Place(model, props, floorPosition, Quaternion.Euler(0f, yaw, 0f));
                if (support != null) prop.AddComponent<SectionProp>().EditorSetup(support);
                // A generic box sized for a shelf becomes an invisible obstacle beside a small mannequin or rotated desk.
                Bounds bounds = ModelFit.LocalBounds(prop, prop.transform);
                var collider = prop.AddComponent<BoxCollider>();
                collider.center = bounds.center;
                collider.size = bounds.size;
                count++;
            }
        }

        public static void ParkVehicles(Transform exterior)
        {
            Vehicles.Park(exterior, Vehicles.Delivery, new Vector3(7f, 0f, -12f), 82f, new Vector3(2.2f, 2.6f, 4.6f));
            Vehicles.Park(exterior, Vehicles.Van, new Vector3(46f, 0f, -13f), -15f, new Vector3(2f, 2.1f, 4.4f));
        }
    }
}
