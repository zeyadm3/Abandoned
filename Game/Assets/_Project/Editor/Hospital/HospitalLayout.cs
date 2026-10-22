using Abandoned.EditorTools;
using UnityEngine;
using P = Abandoned.EditorTools.StationPlan;
using F = Abandoned.EditorTools.StationPlan.Finish;
namespace Abandoned.Hospital
{
    /// <summary>Departments, circulation and collapse risk share one four-metre grid.</summary>
    public static class HospitalLayout
    {
        public const string ScenePath = "Assets/_Project/Scenes/Hospital.unity";
        public const float Width = 56f;
        public const float Depth = 48f;
        public static readonly Bounds Bounds = new(new Vector3(28f, 2f, 24f), new Vector3(56f, 12f, 48f));
        public static readonly Vector3 [] Crew =
        {
            new(40f, 0f, - 18f),
            new(42f, 0f, - 18f),
            new(46f, 0f, - 18f),
            new(48f, 0f, - 18f)
        };
        // Kept for callers authored against the first hospital layout.
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
        public static string Tag(int floor, int side, int bay) => floor == 0 ?(side == 0 ?(bay == 0 ? "hospital_reception" : "hospital_pharmacy") : "hospital_clinical") :(side == 0 ? "hospital_ward" : bay == 0 ? "hospital_theatre" : "hospital_records");
        public static P Create()
        {
            var plan = new P
            {
                SceneName = "Hospital",
                ScenePath = ScenePath,
                DisplayName = "Saint Brigid Hospital",
                ArtFolder = HospitalKit.Art,
                LogTag = "Hospital",
                Width = 14,
                Depth = 12,
                Stability = .72f,
                Seed = 17023
            };
            plan.FillFloor(0, new RectInt(0, 0, 14, 12));
            plan.FillFloor(1, new RectInt(0, 0, 14, 12));
            plan.FillFloor(- 1, new RectInt(0, 6, 8, 6));
            plan.Hole(1, new Vector2Int(0, 4), new Vector2Int(0, 5), new Vector2Int(13, 6), new Vector2Int(13, 7));
            plan.Hole(0, new Vector2Int(0, 7), new Vector2Int(0, 8));
            Ground(plan);
            Upper(plan);
            Basement(plan);
            plan.Stairs.Add(new P.Flight("WestClinicalStair", 0, new Vector3(2f, 0f, 16f), Vector3.forward));
            plan.Stairs.Add(new P.Flight("EastClinicalStair", 0, new Vector3(54f, 0f, 24f), Vector3.forward));
            plan.Stairs.Add(new P.Flight("MorgueStair", - 1, new Vector3(2f, - 4f, 36f), Vector3.back));
            plan.Open(1, 0, 5, 0, 6);
            plan.Open(1, 13, 7, 13, 8);
            plan.Open(0, 0, 6, 0, 7);
            plan.Facade.Add(new P.Opening("AmbulanceBay", P.Side.South, 44f, 6f, 0f, 3.4f, false));
            plan.Facade.Add(new P.Opening("PublicEntry", P.Side.South, 10f, 3.2f, 0f, 3.1f, false));
            plan.Facade.Add(new P.Opening("RearEscape", P.Side.North, 30f, 3.2f, 0f, 3.1f, false));
            // Wards visibly fail over the lobby. The basement makes a ground-floor fall meaningful too.
            plan.Weak(1, new RectInt(0, 0, 4, 3), .65f, .68f);
            plan.Weak(1, new RectInt(8, 0, 3, 3), .75f, .7f);
            plan.UpperRoomsCollapse(1, .86f, .85f);
            plan.Weak(0, new RectInt(1, 6, 2, 2), .78f, .75f);
            foreach (int floor in new []
            {
                0, 1
            })
            {
                plan.Clear(floor, new RectInt(7, 0, 1, 12));
                plan.Clear(floor, new RectInt(0, 3, 14, 1));
                plan.Clear(floor, new RectInt(0, 8, 14, 1));
            }
            plan.Clear(1, new RectInt(0, 6, 14, 1));
            plan.Clear(0, new RectInt(8, 0, 6, 3));
            plan.Clear(- 1, new RectInt(0, 9, 8, 1));
            plan.Clear(- 1, new RectInt(4, 6, 1, 6));
            // KeepClear prevents rubble; removing weakness also makes the route itself permanent.
            foreach (Vector3Int key in plan.KeepClear)
            {
                plan.Collapsible.Remove(key);
            }
            return plan;
        }
        private static void Ground(P p)
        {
            p.AddRoom("Reception", 0, 0, 0, 4, 3, "hospital_reception");
            p.AddRoom("Triage", 0, 4, 0, 3, 3, "hospital_clinical", F.Tile);
            p.AddRoom("Ambulance", 0, 8, 0, 6, 3, "hospital_clinical", F.Concrete, false);
            p.AddRoom("Emergency", 0, 1, 4, 3, 4, "hospital_clinical", F.Tile);
            p.AddRoom("Radiology", 0, 4, 4, 3, 4, "hospital_clinical", F.Tile, false);
            p.AddRoom("Pharmacy", 0, 8, 4, 3, 4, "hospital_pharmacy", F.Tile, false);
            p.AddRoom("Supplies", 0, 11, 4, 2, 4, "hospital_pharmacy", F.Block);
            p.AddRoom("Chapel", 0, 0, 9, 4, 3, "hospital_chapel", F.Panel);
            p.AddRoom("Cafeteria", 0, 4, 9, 3, 3, "hospital_kitchen", F.Tile);
            p.AddRoom("Kitchen", 0, 8, 9, 3, 3, "hospital_kitchen", F.Tile);
            p.AddRoom("Changing", 0, 11, 9, 3, 3, "hospital_ward");
            p.Door(0, 2, 2, 2, 3, true);
            p.Door(0, 5, 2, 5, 3, true);
            p.Door(0, 7, 1, 8, 1, true);
            p.Door(0, 10, 2, 10, 3, true);
            p.Door(0, 2, 3, 2, 4, true);
            p.Door(0, 0, 6, 1, 6, true);
            p.Door(0, 5, 3, 5, 4, true);
            p.Door(0, 5, 7, 5, 8, true);
            p.Door(0, 9, 3, 9, 4, true);
            p.Door(0, 9, 7, 9, 8, true);
            p.Door(0, 12, 3, 12, 4, true);
            p.Door(0, 12, 7, 12, 8, true);
            foreach (int x in new []
            {
                2, 5, 9, 12
            })
            {
                p.Door(0, x, 8, x, 9, true);
            }
        }
        private static void Upper(P p)
        {
            p.AddRoom("WardA", 1, 0, 0, 4, 3, "hospital_ward");
            p.AddRoom("WardB", 1, 4, 0, 3, 3, "hospital_ward");
            p.AddRoom("TheatreA", 1, 8, 0, 3, 3, "hospital_theatre", F.Tile);
            p.AddRoom("TheatreB", 1, 11, 0, 3, 3, "hospital_theatre", F.Tile);
            p.AddRoom("IntensiveCare", 1, 1, 4, 3, 2, "hospital_ward", F.Tile);
            p.AddRoom("Nurses", 1, 4, 4, 3, 2, "hospital_records");
            p.AddRoom("Sterile", 1, 8, 4, 3, 2, "hospital_theatre", F.Tile);
            p.AddRoom("Recovery", 1, 11, 4, 3, 2, "hospital_ward");
            p.AddRoom("Laboratory", 1, 1, 7, 3, 1, "hospital_theatre", F.Tile);
            p.AddRoom("Records", 1, 4, 7, 3, 1, "hospital_records", F.Block);
            p.AddRoom("StaffRest", 1, 8, 7, 3, 1, "hospital_reception", F.Panel);
            p.AddRoom("Consultation", 1, 11, 7, 2, 1, "hospital_clinical");
            p.AddRoom("Isolation", 1, 0, 9, 4, 3, "hospital_ward");
            p.AddRoom("Paediatrics", 1, 4, 9, 3, 3, "hospital_ward");
            p.AddRoom("Administration", 1, 8, 9, 3, 3, "hospital_records", F.Panel);
            p.AddRoom("EquipmentStore", 1, 11, 9, 3, 3, "hospital_theatre", F.Block);
            foreach (int x in new []
            {
                2, 5, 9, 12
            })
            {
                p.Door(1, x, 2, x, 3, true);
                p.Door(1, x, 3, x, 4, true);
                p.Door(1, x, 5, x, 6, true);
                p.Door(1, x, 6, x, 7, true);
                p.Door(1, x, 7, x, 8, true);
                p.Door(1, x, 8, x, 9, true);
            }
        }
        private static void Basement(P p)
        {
            p.AddRoom("Morgue", - 1, 1, 6, 3, 3, "hospital_morgue", F.Tile, false);
            p.AddRoom("Boiler", - 1, 5, 6, 3, 3, "hospital_plant", F.Concrete, false);
            p.AddRoom("Autopsy", - 1, 1, 10, 3, 2, "hospital_morgue", F.Tile, false);
            p.AddRoom("BasementStores", - 1, 5, 10, 3, 2, "hospital_plant", F.Block, false);
            p.Door(- 1, 2, 8, 2, 9, true);
            p.Door(- 1, 6, 8, 6, 9, true);
            p.Door(- 1, 2, 9, 2, 10, true);
            p.Door(- 1, 6, 9, 6, 10, true);
        }
    }
}
