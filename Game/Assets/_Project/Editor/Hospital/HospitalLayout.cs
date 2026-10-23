using System.Collections.Generic;
using Abandoned.EditorTools;
using UnityEngine;
using P = Abandoned.EditorTools.StationPlan;
using F = Abandoned.EditorTools.StationPlan.Finish;
namespace Abandoned.Hospital
{
    /// <summary>A medical campus in miniature: separate wings around a glazed public gallery.</summary>
    public static class HospitalLayout
    {
        public const string ScenePath = "Assets/_Project/Scenes/Hospital.unity";
        public const float Width = 56f,
        Depth = 48f;
        public static readonly Bounds Bounds = new(new Vector3(28f, 6f, 24f), new Vector3(56f, 20f, 48f));
        public static readonly Vector3 [] Crew =
        {
            new(40, 0, - 18),
            new(42, 0, - 18),
            new(46, 0, - 18),
            new(48, 0, - 18)
        };
        public static readonly string [,
        ] Rooms =
        {
            {
                "RECEPTION",
                "PHARMACY",
                "EMERGENCY",
                "IMAGING"
            },
            {
                "WARD A",
                "WARD B",
                "SURGERY",
                "LAB AND RECORDS"
            }
        };
        public static string Tag(int floor, int side, int bay) => floor == 0 ? "hospital_clinical" : "hospital_ward";
        public static P Create()
        {
            var p = new P
            {
                SceneName = "Hospital",
                ScenePath = ScenePath,
                DisplayName = "Saint Brigid Hospital",
                ArtFolder = HospitalKit.Art,
                LogTag = "Hospital",
                Width = 14,
                Depth = 12,
                RoofY = 15.75f,
                ParapetTop = 17f,
                Stability = .72f,
                Seed = 17023
            };
            for (int f = 0; f <2; f++)
            {
                p.FillFloor(f, new RectInt(0, 2, 5, 10));
                p.FillFloor(f, new RectInt(5, 0, 4, 12));
                p.FillFloor(f, new RectInt(9, 0, 5, 4));
                p.FillFloor(f, new RectInt(9, 7, 5, 5));
            }
            p.FillFloor(2, new RectInt(0, 6, 5, 6));
            p.FillFloor(2, new RectInt(10, 7, 4, 5));
            p.FillFloor(2, new RectInt(5, 9, 5, 1));
            p.FillFloor(3, new RectInt(1, 7, 4, 5));
            p.FillFloor(3, new RectInt(10, 8, 4, 4));
            p.FillFloor(3, new RectInt(5, 9, 5, 1));
            p.FillFloor(- 1, new RectInt(0, 6, 8, 6));
            // The gallery is an eight-metre void, with one risky bridge and two permanent side routes.
            for (int x = 6; x <8; x++)
            {
                for (int z = 0; z <9; z++)
                {
                    if (z != 2 && z != 5)
                    {
                        p.Hole(1, new Vector2Int(x, z));
                    }
                }
            }
            p.Hole(1, new Vector2Int(0, 4), new Vector2Int(0, 5), new Vector2Int(13, 1), new Vector2Int(13, 2), new Vector2Int(13, 8), new Vector2Int(13, 9));
            p.Hole(2, new Vector2Int(0, 6), new Vector2Int(0, 7), new Vector2Int(13, 8), new Vector2Int(13, 9));
            p.Hole(3, new Vector2Int(1, 7), new Vector2Int(1, 8), new Vector2Int(13, 8), new Vector2Int(13, 9));
            p.Hole(0, new Vector2Int(0, 7), new Vector2Int(0, 8));
            HospitalDepartments.Ground(p);
            HospitalDepartments.Upper(p);
            HospitalDepartments.Basement(p);
            Doors(p);
            Flight(p, "PublicWest", 0, new Vector3(2, 0, 16), Vector3.forward, 1, 0, 5, 0, 6);
            Flight(p, "EmergencyEast", 0, new Vector3(54, 0, 4), Vector3.forward, 1, 13, 2, 13, 3);
            Flight(p, "ClinicalEast", 0, new Vector3(54, 0, 32), Vector3.forward, 1, 13, 9, 13, 10);
            Flight(p, "WardWest", 1, new Vector3(2, 4, 24), Vector3.forward, 2, 0, 7, 0, 8);
            Flight(p, "WardEast", 1, new Vector3(54, 4, 40), Vector3.back, 2, 13, 7, 13, 8);
            Flight(p, "UpperWest", 2, new Vector3(6, 8, 28), Vector3.forward, 3, 1, 8, 1, 9);
            Flight(p, "UpperEast", 2, new Vector3(54, 8, 32), Vector3.forward, 3, 13, 9, 13, 10);
            Flight(p, "Morgue", - 1, new Vector3(2, - 4, 36), Vector3.back, 0, 0, 6, 0, 7);
            p.Facade.Add(new P.Opening("MainEntry", P.Side.South, 28, 6, 0, 5.5f, false));
            p.Facade.Add(new P.Opening("AmbulanceBay", P.Side.South, 44, 6, 0, 3.4f, false));
            p.Facade.Add(new P.Opening("RearEscape", P.Side.North, 34, 3.2f, 0, 3.1f, false));
            for (int f = 1; f <= 3; f++)
            {
                p.UpperRoomsCollapse(f, .87f, .86f);
            }
            p.Weak(1, new RectInt(6, 5, 2, 1), .65f, .66f);
            p.Weak(0, new RectInt(1, 6, 2, 2), .78f, .76f);
            for (int f = 0; f <2; f++)
            {
                p.Clear(f, new RectInt(5, 0, 1, 12));
                p.Clear(f, new RectInt(8, 0, 1, 12));
                p.Clear(f, new RectInt(0, 5, 5, 1));
                p.Clear(f, new RectInt(0, 8, 5, 1));
                p.Clear(f, new RectInt(9, 2, 5, 1));
                p.Clear(f, new RectInt(9, 7, 5, 1));
                p.Clear(f, new RectInt(9, 10, 5, 1));
                p.Clear(f, new RectInt(f == 0 ? 4 : 3, 6, 1, 2));
            }
            p.Clear(1, new RectInt(5, 2, 4, 1));
            p.Clear(1, new RectInt(5, 9, 4, 1));
            p.Clear(0, new RectInt(10, 0, 2, 2));
            for (int f = 2; f <= 3; f++)
            {
                p.Clear(f, new RectInt(0, 9, 14, 1));
                p.Clear(f, new RectInt(3, 6, 1, 6));
                p.Clear(f, new RectInt(12, 7, 2, 5));
                p.Clear(f, new RectInt(0, 8, 5, 1));
            }
            p.Clear(- 1, new RectInt(0, 9, 8, 1));
            p.Clear(- 1, new RectInt(4, 6, 1, 6));
            foreach (Vector3Int key in p.KeepClear)
            {
                p.Collapsible.Remove(key);
            }
            return p;
        }
        private static void Flight(P p, string name, int f, Vector3 foot, Vector3 direction, int upper, int ax, int az, int bx, int bz)
        {
            p.Stairs.Add(new P.Flight(name, f, foot, direction));
            p.Open(upper, ax, az, bx, bz);
        }
        private static void Doors(P p)
        {
            Vector2Int [] steps =
            {
                Vector2Int.left,
                Vector2Int.right,
                Vector2Int.up,
                Vector2Int.down
            };
            foreach (P.Room room in p.Rooms)
            {
                foreach (Vector2Int step in steps)
                {
                    float best = float.MaxValue;
                    Vector2Int chosen = default;
                    for (int x = room.Cells.xMin; x <room.Cells.xMax; x++)
                    {
                        for (int z = room.Cells.yMin; z <room.Cells.yMax; z++)
                        {
                            var a = new Vector2Int(x, z);
                            Vector2Int b = a + step;
                            if (! p.HasFloor(room.Floor, a) || ! p.HasFloor(room.Floor, b) || p.RoomAt(room.Floor, b) != null)
                            {
                                continue;
                            }
                            float distance =(P.CellCentre(a, room.Floor) - room.Centre).sqrMagnitude;
                            if (distance >= best)
                            {
                                continue;
                            }
                            best = distance;
                            chosen = a;
                        }
                    }
                    if (best <float.MaxValue)
                    {
                        p.Door(room.Floor, chosen.x, chosen.y, chosen.x + step.x, chosen.y + step.y, true);
                    }
                }
            }
        }
    }
}
