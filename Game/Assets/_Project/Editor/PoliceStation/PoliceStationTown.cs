using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Lot = Abandoned.EditorTools.StationTownPlan.Lot;
using Plan = Abandoned.EditorTools.StationTownPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood round the precinct (Docs/POLICE_STATION_PLAN.md "Outside"): four streets boxing the station's
    /// block (Main Street in front, the avenues west and east, a back street behind the impound), with a row of buildings
    /// facing in from every side: shops, a diner, row houses, a walk-up, a church and a warehouse across Main Street; a
    /// walk-up, a garage and a warehouse to the west; shops, flats and row houses to the east; clapboard houses with
    /// their yards behind. Traffic lights at the corners, lamps, power lines, parked cars, and an abandoned police
    /// roadblock on Main Street. Town ground x -70..128, z -48..100, top -0.02; two more rings round it (StationRings) take
    /// the whole ground to x -192..250, z -170..222. Exterior only: no building has an inside.
    /// </summary>
    public static class PoliceStationTown
    {
        public const float West = -70f, East = 128f, South = -48f, North = 100f;

        public static void Build(Transform root, Transform ext, StationArt a)
        {
            StationTownArt t = StationTownArt.For(a);
            Transform marks = Group("StreetMarkings", ext);
            Transform town = Group("Neighbourhood", ext);
            Transform street = Group("StreetFurniture", town);
            Streets(root, marks, a);
            Lots(root, a, t);
            Buildings(town, a, t);
            Furniture(street, marks, a);
            Rings(root, ext, a);
        }

        // Two more rings of town round it (StationRings): Main and Back Street run out west and east, the avenues south
        // and north, and a spur on the east, the side the open world's road comes in from, meets its middle.
        private static void Rings(Transform root, Transform ext, StationArt a)
        {
            var spec = new StationRingSpec
            {
                Inner = Rect.MinMaxRect(West, South, East, North), Road = 10f, Seed = 911, Vacancy = .1f, Lot = a.Gravel, Yard = a.Grass,
            };
            foreach (int side in new[] { StationRingSpec.South, StationRingSpec.North })
            {
                spec.Through(side, -40f, -30f);
                spec.Through(side, 90f, 100f);
            }
            foreach (int side in new[] { StationRingSpec.West, StationRingSpec.East })
            {
                spec.Through(side, -16f, -6f);
                spec.Through(side, 64f, 74f);
            }
            spec.Spur(StationRingSpec.East, 21f, 31f);
            var stock = new StationRingStock(a);
            StationRingPools.Town(stock, spec, 9100);
            StationRings.Build(root, ext, a, spec, stock);
        }

        // Roads first (the east-west ones run the full width; the avenues stop at them), then sidewalks round them.
        private static void Streets(Transform root, Transform marks, StationArt a)
        {
            StationStreet.Road(root, marks, a, "MainStreet", Rect.MinMaxRect(West, -16f, East, -6f), true);
            StationStreet.Road(root, marks, a, "BackStreet", Rect.MinMaxRect(West, 64f, East, 74f), true);
            foreach ((float x, string n) in new[] { (-40f, "WestAvenue"), (90f, "EastAvenue") })
            {
                StationStreet.Road(root, marks, a, n + "_S", Rect.MinMaxRect(x, South, x + 10f, -16f), false);
                StationStreet.Road(root, marks, a, n + "_M", Rect.MinMaxRect(x, -6f, x + 10f, 64f), false);
                StationStreet.Road(root, marks, a, n + "_N", Rect.MinMaxRect(x, 74f, x + 10f, North), false);
            }
            // East-west sidewalks, broken at the avenues.
            foreach ((float z0, float z1, float kerb, string n) in new[] { (-6f, 0f, -1f, "MainN"), (-22f, -16f, 1f, "MainS"), (60f, 64f, 1f, "BackS"), (74f, 78f, -1f, "BackN") })
                foreach ((float x0, float x1) in new[] { (West, -40f), (-30f, 90f), (100f, East) })
                    StationStreet.Sidewalk(root, marks, a, $"Sidewalk_{n}_{x0:0}", Rect.MinMaxRect(x0, z0, x1, z1), true, kerb);
            // North-south sidewalks between them.
            foreach ((float x0, float x1, float kerb, string n) in new[] { (-44f, -40f, 1f, "WestAveW"), (86f, 90f, 1f, "EastAveW"), (100f, 104f, -1f, "EastAveE") })
                foreach ((float z0, float z1) in new[] { (South, -22f), (0f, 60f), (78f, North) })
                    StationStreet.Sidewalk(root, marks, a, $"Sidewalk_{n}_{z0:0}", Rect.MinMaxRect(x0, z0, x1, z1), false, kerb);
            foreach ((float z0, float z1) in new[] { (South, -22f), (78f, North) })
                StationStreet.Sidewalk(root, marks, a, $"Sidewalk_WestAveE_{z0:0}", Rect.MinMaxRect(-30f, z0, -26f, z1), false, -1f);
            // Crosswalks on every arm of the four corners.
            foreach (float x in new[] { -35f, 95f })
            foreach (float z in new[] { -11f, 69f })
            {
                StationStreet.Crosswalk(marks, a, new Vector3(x - 7.5f, 0f, z), true, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x + 7.5f, 0f, z), true, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x, 0f, z - 7.5f), false, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x, 0f, z + 7.5f), false, 10f);
            }
        }

        // The ground between the streets that the station's own lot doesn't cover.
        private static void Lots(Transform root, StationArt a, StationTownArt t)
        {
            Plan.Ground(root, "Lot_SouthW", Rect.MinMaxRect(West, South, -44f, -22f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_South", Rect.MinMaxRect(-26f, South, 86f, -22f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_SouthE", Rect.MinMaxRect(104f, South, East, -22f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "Lot_West", Rect.MinMaxRect(West, 0f, -44f, 60f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_East", Rect.MinMaxRect(104f, 0f, East, 60f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_NorthW", Rect.MinMaxRect(West, 78f, -44f, North), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_North", Rect.MinMaxRect(-26f, 78f, 86f, North), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_NorthE", Rect.MinMaxRect(104f, 78f, East, North), a.Gravel, SurfaceMaterial.Dirt);
        }

        private static void Buildings(Transform town, StationArt a, StationTownArt t)
        {
            // Across Main Street, facing the precinct.
            Plan.Row(town, a, new Vector3(-25f, 0f, -22.3f), Vector3.right, 0f, new[]
            {
                new Lot(K.Shop, 10f, 18f, 2), new Lot(K.Shop, 8f, 18f, 3), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3),
                new Lot(K.Rowhouse, 7f, 14f, 3, 4f), new Lot(K.Apartment, 18f, 22f, 5), new Lot(K.Shop, 12f, 18f, 1, 3f),
                new Lot(K.Church, 16f, 25f), new Lot(K.Shop, 9f, 16f, 2),
            }, 310);
            Plan.Row(town, a, new Vector3(-69f, 0f, -22.3f), Vector3.right, 0f, new[] { new Lot(K.Warehouse, 24f, 24f) }, 320);
            Plan.Row(town, a, new Vector3(105f, 0f, -22.3f), Vector3.right, 0f, new[] { new Lot(K.GasStation, 22f, 24f) }, 330);
            // West of West Avenue, facing east.
            Plan.Row(town, a, new Vector3(-44.3f, 0f, 1f), Vector3.forward, 90f, new[]
            {
                new Lot(K.Apartment, 20f, 23f, 4), new Lot(K.Garage, 16f, 20f, 1, 2f), new Lot(K.Warehouse, 18f, 23f),
            }, 340);
            // East of East Avenue, facing west.
            Plan.Row(town, a, new Vector3(104.3f, 0f, 2f), Vector3.forward, -90f, new[]
            {
                new Lot(K.Shop, 12f, 20f, 2), new Lot(K.Shop, 9f, 18f, 3, 1f), new Lot(K.Apartment, 18f, 22f, 5),
                new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3),
            }, 350);
            // Behind the impound, facing south: clapboard houses in their yards, then row houses and a corner shop.
            Plan.Row(town, a, new Vector3(-25f, 0f, 81.5f), Vector3.right, 180f, new[]
            {
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f),
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f),
            }, 360);
            Plan.Row(town, a, new Vector3(40f, 0f, 78.3f), Vector3.right, 180f, new[]
            {
                new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 2), new Lot(K.Rowhouse, 7f, 14f, 3),
                new Lot(K.Shop, 12f, 16f, 2),
            }, 370);
            Plan.Row(town, a, new Vector3(-69f, 0f, 78.3f), Vector3.right, 180f, new[] { new Lot(K.Apartment, 24f, 20f, 4) }, 380);
            Plan.Row(town, a, new Vector3(105f, 0f, 78.3f), Vector3.right, 180f, new[] { new Lot(K.Warehouse, 22f, 20f) }, 390);
            // The houses' yards: picket fences on the sidewalk line with a gap at each path, the path and the driveway.
            for (int i = 0; i < 5; i++)
            {
                float cx = -20f + i * 13f;
                Plan.Picket(town, a, new Vector3(cx - 6.4f, 0f, 78.25f), new Vector3(cx - .7f, 0f, 78.25f), t.Frame);
                Plan.Picket(town, a, new Vector3(cx + .7f, 0f, 78.25f), new Vector3(cx + 5f, 0f, 78.25f), t.Frame);
                Plan.Paving(town, Rect.MinMaxRect(cx - .6f, 78.2f, cx + .6f, 81.4f), a.Sidewalk);
                Plan.Paving(town, Rect.MinMaxRect(cx + 5.2f, 78.2f, cx + 7.8f, 92f), a.Concrete);
                StationStreet.Mailbox(town, a, new Vector3(cx + 1.2f, 0f, 77.6f), 180f);
            }
        }

        private static void Furniture(Transform s, Transform marks, StationArt a)
        {
            // Lamps down both sides of Main Street, one side of the others.
            Plan.Lamps(s, a, new Vector3(West, 0f, -5.5f), Vector3.right, East - West, 32f, Vector3.back * 5f);
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, -16.5f), Vector3.right, East - West - 6f, 32f, Vector3.forward * 5f);
            // Down the avenues at 30 m from -31: every lamp lands on a sidewalk, none in a crossing street.
            Plan.Lamps(s, a, new Vector3(-40.5f, 0f, South + 2f), Vector3.forward, North - South - 2f, 30f, Vector3.right * 5f);
            Plan.Lamps(s, a, new Vector3(100.5f, 0f, South + 2f), Vector3.forward, North - South - 2f, 30f, Vector3.left * 5f);
            Plan.Lamps(s, a, new Vector3(West + 8f, 0f, 74.5f), Vector3.right, East - West - 8f, 32f, Vector3.back * 5f);
            // Power lines: along the back street and down West Avenue.
            // Every pole on a sidewalk, clear of the crossings.
            var back = new System.Collections.Generic.List<Vector3>();
            foreach (float x in new[] { -18f, 8f, 34f, 60f, 86f, 112f }) back.Add(new Vector3(x, -.02f, 61f));
            StationStreet.PowerLine(s, a, back);
            var westLine = new System.Collections.Generic.List<Vector3>();
            foreach (float z in new[] { -44f, -19f, 8f, 36f, 61f, 86f }) westLine.Add(new Vector3(-43f, -.02f, z));
            StationStreet.PowerLine(s, a, westLine);
            // The corners: signals on Main Street, stop signs at the back street, blank street-name blades.
            StationStreet.TrafficLight(s, a, new Vector3(-29.3f, -.02f, -5.4f), 180f);
            StationStreet.TrafficLight(s, a, new Vector3(-40.7f, -.02f, -16.6f), 0f);
            StationStreet.TrafficLight(s, a, new Vector3(89.3f, -.02f, -5.4f), 180f);
            StationStreet.TrafficLight(s, a, new Vector3(100.7f, -.02f, -16.6f), 0f);
            foreach (Vector3 p in new[] { new Vector3(-29.4f, -.02f, 63.4f), new Vector3(-40.6f, -.02f, 74.6f), new Vector3(89.4f, -.02f, 63.4f), new Vector3(100.6f, -.02f, 74.6f) })
                StationStreet.StopSign(s, a, p, p.z < 69f ? 180f : 0f);
            foreach (Vector3 p in new[] { new Vector3(-40.8f, -.02f, -5.2f), new Vector3(89.2f, -.02f, -16.8f), new Vector3(-29.2f, -.02f, 74.8f), new Vector3(100.8f, -.02f, 63.2f) })
                StationStreet.StreetSign(s, a, p, 0f);
            // Main Street's pavement life, mostly on the shop side.
            StationStreet.BusShelter(s, a, new Vector3(20f, -.02f, -20.4f), 0f);
            StationStreet.Bench(s, a, new Vector3(30f, -.02f, -21.2f), 0f);
            StationStreet.NewsBoxes(s, a, new Vector3(-12f, -.02f, -21.3f), 0f);
            StationStreet.Mailbox(s, a, new Vector3(-6f, -.02f, -21.4f), 0f);
            StationStreet.Bench(s, a, new Vector3(28f, -.02f, -.9f), 180f);
            foreach ((float x0, float x1) in new[] { (West, -40f), (-30f, 90f), (100f, East) })
                Plan.Kerbside(s, a, new Vector3(x0, -.02f, -17f), Vector3.right, x1 - x0, 0f, 3 + Mathf.RoundToInt(x0));
            Plan.Kerbside(s, a, new Vector3(-30f, -.02f, -5.3f), Vector3.right, 120f, 180f, 5);
            Plan.Kerbside(s, a, new Vector3(101f, -.02f, South), Vector3.forward, North - South, 270f, 7);
            Plan.Kerbside(s, a, new Vector3(-26f, -.02f, 77f), Vector3.right, 112f, 180f, 9);
            // Cars along the kerbs, none across the station's front or the way into the east yard.
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, -7.1f), Vector3.right, 26f, 41);
            Plan.ParkedCars(s, a, new Vector3(-29f, 0f, -7.1f), Vector3.right, 118f, 43, new Vector2(20f, 120f));
            Plan.ParkedCars(s, a, new Vector3(East - 2f, 0f, -14.9f), Vector3.left, East - West - 4f, 47, new Vector2(0f, 22f), new Vector2(30f, 40f),
                new Vector2(100f, 114f), new Vector2(160f, 172f));
            Plan.ParkedCars(s, a, new Vector3(-31.1f, 0f, North - 2f), Vector3.back, North - South - 4f, 53, new Vector2(25f, 37f), new Vector2(105f, 117f));
            Plan.ParkedCars(s, a, new Vector3(98.9f, 0f, South + 2f), Vector3.forward, North - South - 4f, 59, new Vector2(26f, 38f), new Vector2(40f, 66f), new Vector2(106f, 118f));
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, 72.9f), Vector3.right, East - West - 4f, 61, new Vector2(30f, 40f), new Vector2(160f, 172f));
            // Dumpsters down the alleys and behind the shops.
            StationStreet.Dumpster(s, a, new Vector3(1.5f, -.02f, -38.6f), 90f, true);
            StationStreet.Dumpster(s, a, new Vector3(49.5f, -.02f, -30f), 90f, false);
            StationStreet.Dumpster(s, a, new Vector3(-50f, -.02f, 38f), 0f, true);
            StationStreet.Dumpster(s, a, new Vector3(126f, -.02f, 6f), 90f, false);
            float[] behind = { -20f, -1f, 17f, 40f, 72f };
            for (int i = 0; i < behind.Length; i++) StationStreet.Litter(s, a, new Vector3(behind[i], -.02f, i == 2 ? -45.5f : -42f), 70 + i);
            // The roadblock the department set up and never took down: barriers across Main Street east of the avenue,
            // a gap left at the north kerb, two cruisers nosed in, sawhorses and cones.
            StationStreet.Barriers(s, new Vector3(110f, -.02f, -15f), new Vector3(110f, -.02f, -9.5f), 2);
            StationStreet.Sawhorse(s, a, new Vector3(113f, -.02f, -12f), 90f);
            Material patrol = a.Plain("PatrolBlack", new Color(.06f, .065f, .08f), .55f, .3f);
            StationVehicles.Car(s, "RoadblockCruiser", new Vector3(116f, -.02f, -13.2f), 60f, StationVehicleKind.Cruiser, patrol, a, StationVehicleWear.Dusty | StationVehicleWear.FlatRear, 11);
            StationVehicles.Car(s, "RoadblockCruiser", new Vector3(121f, -.02f, -9f), 125f, StationVehicleKind.Cruiser, patrol, a, StationVehicleWear.DoorOpen | StationVehicleWear.Smashed, 12);
            for (int i = 0; i < 5; i++) Cone(s, a, new Vector3(108f, -.02f, -15.4f + i * 1.2f), i == 3);
        }

        // A traffic cone (a lathe), maybe knocked over.
        private static void Cone(Transform s, StationArt a, Vector3 at, bool over)
        {
            Material orange = a.Plain("ConeOrange", new Color(.9f, .35f, .05f), .45f);
            GameObject cone = StationMeshes.Draw(s, "Cone", StationMeshes.Lathe(a, "Cone", new[]
            {
                new Vector2(0f, 0f), new Vector2(.2f, 0f), new Vector2(.2f, 0f), new Vector2(.2f, .04f), new Vector2(.2f, .04f), new Vector2(.12f, .05f),
                new Vector2(.03f, .7f), new Vector2(0f, .71f),
            }, 16), at, Vector3.zero, Vector3.one, orange);
            if (!over) return;
            cone.transform.localRotation = Quaternion.Euler(0f, 40f, 90f);
            cone.transform.localPosition = at + Vector3.up * .2f;
        }
    }
}
