using System.Collections.Generic;
using System.Linq;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.MallLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Set dressing for the mall's stores (M7.1): fixtures from the Kenney Furniture Kit (shelves,
    /// counters, desks, racks, plants; never anything that looks like loot) standing against store
    /// walls. They keep to a strip along the wall, clear of the loot points (tile centre +-0.9 m),
    /// doorways, exterior openings, flights and jackpot tiles. Each hides when its floor collapses
    /// (SectionProp). Seeded by tile, so rebuilding gives the same mall.
    /// </summary>
    public static class MallProps
    {
        private const float WallStrip = 0.85f; // deepest a prop may reach into the room (m)
        private const int Chance = 34;         // Most retail bays feel emptied, with a few abandoned fixtures along the walls.

        private readonly struct Prop
        {
            public readonly string Model;
            public readonly Vector3 Box; // width along the wall, height, depth into the room

            public Prop(string model, float width, float height, float depth)
            {
                Model = model;
                Box = new Vector3(width, height, Mathf.Min(depth, WallStrip));
            }
        }

        private static readonly Dictionary<string, Prop[]> Themes = new()
        {
            ["electronics"] = new[] { P("cabinetTelevision", 1.4f, 0.6f, 0.5f), P("bookcaseOpenLow", 1.2f, 1f, 0.45f), P("kitchenBar", 1.6f, 1f, 0.7f), P("cardboardBoxClosed", 0.6f, 0.5f, 0.6f) },
            ["clothing"] = new[] { P("coatRackStanding", 0.6f, 1.8f, 0.6f), P("bookcaseOpen", 1.2f, 2f, 0.45f), P("pottedPlant", 0.6f, 1.2f, 0.6f) },
            ["jewelry"] = new[] { P("sideTableDrawers", 0.6f, 0.7f, 0.45f), P("cabinetBedDrawerTable", 0.6f, 0.7f, 0.5f), P("lampSquareFloor", 0.4f, 1.6f, 0.4f), P("pottedPlant", 0.6f, 1.2f, 0.6f) },
            ["office"] = new[] { P("desk", 1.4f, 0.8f, 0.7f), P("bookcaseClosed", 0.9f, 2f, 0.45f), P("trashcan", 0.4f, 0.6f, 0.4f), P("pottedPlant", 0.6f, 1.2f, 0.6f), P("chairDesk", 0.6f, 1f, 0.6f) },
            ["stock"] = new[] { P("cardboardBoxClosed", 0.7f, 0.6f, 0.7f), P("cardboardBoxOpen", 0.7f, 0.6f, 0.7f), P("bookcaseOpen", 1.2f, 2f, 0.45f) },
            ["furniture"] = new[] { P("loungeSofa", 2f, 0.85f, 0.8f), P("loungeChair", 0.9f, 0.85f, 0.8f), P("tableCoffee", 1.2f, 0.45f, 0.6f), P("bookcaseClosedWide", 1.6f, 2f, 0.5f), P("lampSquareFloor", 0.4f, 1.6f, 0.4f) },
            ["toys"] = new[] { P("bookcaseOpenLow", 1.2f, 1f, 0.45f), P("cardboardBoxOpen", 0.6f, 0.5f, 0.6f), P("bookcaseOpen", 1.2f, 2f, 0.45f) },
            ["cinema"] = new[] { P("loungeSofaLong", 2.4f, 0.85f, 0.8f), P("trashcan", 0.4f, 0.6f, 0.4f), P("speakerSmall", 0.4f, 0.8f, 0.4f) },
            ["gallery"] = new[] { P("benchCushion", 1.6f, 0.5f, 0.5f), P("lampSquareFloor", 0.4f, 1.6f, 0.4f), P("pottedPlant", 0.6f, 1.2f, 0.6f) },
        };

        private static readonly Vector2Int[] Sides = { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down };

        private static Prop P(string model, float width, float height, float depth) => new(model, width, height, depth);

        public static int Place(Transform root, Transform tiles)
        {
            Transform parent = GreyboxFactory.Group("Props", root);
            var models = new Dictionary<string, GameObject>();
            int count = 0;
            for (int f = 0; f < Floors; f++)
                foreach (Zone zone in ZonesByFloor[f])
                {
                    if (zone.Kind != Kind.Store || !Themes.TryGetValue(zone.Tag, out Prop[] theme)) continue;
                    for (int x = zone.Area.xMin; x < zone.Area.xMax; x++)
                    for (int z = zone.Area.yMin; z < zone.Area.yMax; z++)
                    {
                        var c = new Vector2Int(x, z);
                        if (IsVoid(c, f) || Flights.Any(fl => fl.Floor == f && fl.Tiles.Contains(c)) || MallLootPoints.IsJackpotTile(c, f)) continue;
                        var section = tiles.Find($"Floor_{f}/{MallBuilder.TileName(c, f)}")?.GetComponent<StructuralSection>();
                        foreach (Vector2Int side in Sides)
                        {
                            if (!AgainstWall(zone, c, side, f)) continue;
                            int hash = Hash(c, f, side);
                            if (hash % 100 >= Chance) continue;
                            Prop prop = theme[hash / 100 % theme.Length];
                            if (!models.TryGetValue(prop.Model, out GameObject model))
                                models[prop.Model] = model = AssetDatabase.LoadAssetAtPath<GameObject>($"{LootModelBuilder.Furniture}{prop.Model}.fbx");
                            if (model == null) continue;
                            Build(parent, model, prop, c, f, side, section);
                            count++;
                        }
                    }
                }
            return count;
        }

        /// <summary>Abandoned vehicles in the parking lot, away from the spawn and the truck's route.</summary>
        public static void ParkVehicles(Transform exterior)
        {
            Vehicles.Park(exterior, Vehicles.Delivery, new Vector3(7f, 0f, -10f), 80f, new Vector3(2.2f, 2.6f, 4.6f));
            Vehicles.Park(exterior, Vehicles.Van, new Vector3(40f, 0f, -11f), -15f, new Vector3(2f, 2.1f, 4.4f));
        }

        // A solid wall on this side of the tile: the zone ends there, and it isn't a doorway,
        // an exterior opening or the foot/head of a flight.
        private static bool AgainstWall(Zone zone, Vector2Int c, Vector2Int side, int floor)
        {
            Vector2Int n = c + side;
            if (zone.Contains(n)) return false;
            if (InGrid(n))
            {
                Zone? other = ZoneAt(n, floor);
                if (other.HasValue && SameSpace(zone, other.Value)) return false;
                if (DoorBetween(c, n, floor).HasValue || IsFlightOpening(c, n, floor)) return false;
                return true;
            }
            foreach ((int f, char s, int column, bool _, bool _) in Exterior)
            {
                if (f != floor) continue;
                bool onEdge = s switch
                {
                    'S' => side == Vector2Int.down && column == c.x,
                    'N' => side == Vector2Int.up && column == c.x,
                    'W' => side == Vector2Int.left && column == c.y,
                    _ => side == Vector2Int.right && column == c.y,
                };
                if (onEdge) return false;
            }
            return true;
        }

        private static void Build(Transform parent, GameObject model, Prop prop, Vector2Int c, int floor, Vector2Int side, StructuralSection section)
        {
            // Back against the wall's inner face, facing into the room (Kenney models face -Z).
            Vector3 normal = new(side.x, 0f, side.y);
            float fromCentre = Tile / 2f - MallWalls.Thickness / 2f - 0.03f - prop.Box.z / 2f;
            var holder = new GameObject($"Prop_{floor}_{c.x}_{c.y}_{prop.Model}");
            holder.transform.SetParent(parent, false);
            holder.transform.SetPositionAndRotation(TileTopCenter(c, floor) + normal * fromCentre, Quaternion.LookRotation(normal));
            ModelFit.Place(model, holder.transform, Vector3.up * prop.Box.y / 2f, prop.Box, 0f, standOnFloor: true);
            Bounds fitted = ModelFit.LocalBounds(holder.transform.GetChild(0).gameObject, holder.transform);
            var box = holder.AddComponent<BoxCollider>();
            box.center = fitted.center;
            box.size = fitted.size;
            holder.AddComponent<SectionProp>().EditorSetup(section);
            foreach (Transform t in holder.GetComponentsInChildren<Transform>())
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.ContributeGI);
        }

        private static int Hash(Vector2Int c, int floor, Vector2Int side) =>
            Mathf.Abs((c.x * 73856093) ^ (c.y * 19349663) ^ (floor * 83492791) ^ ((side.x + 2 * side.y + 3) * 2654435));
    }
}
