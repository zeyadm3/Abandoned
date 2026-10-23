using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Lot = Abandoned.EditorTools.StationTownPlan.Lot;
using Plan = Abandoned.EditorTools.StationTownPlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The neighbourhood round the firehouse (Docs/FIRE_STATION_PLAN.md "Outside"): Main Street in front of the apron,
    /// the avenues west and east, a back street behind the rear yard, and a row of buildings facing in from every side: a
    /// gas station, shops, a walk-up, row houses and a warehouse across Main Street; a garage at the west corner; a
    /// walk-up, a warehouse and row houses to the west; a church, two clapboard houses and a shop to the east; houses with
    /// their yards and row houses behind. Traffic lights, lamps, power lines, fire-alarm street boxes and parked cars,
    /// with the apron and the street in front of it kept clear for the van. Town ground x -70..134, z -50..100, top -0.02;
    /// two more rings round it (StationRings) take the whole ground to x -192..256, z -172..222.
    /// </summary>
    public static class FireStationTown
    {
        public const float West = -70f, East = 134f, South = -50f, North = 100f;

        public static void Build(Transform root, Transform ext, StationArt a)
        {
            StationTownArt t = StationTownArt.For(a);
            Transform marks = Group("StreetMarkings", ext);
            Transform town = Group("Neighbourhood", ext);
            Transform street = Group("StreetFurniture", town);
            Streets(root, marks, a);
            Lots(root, a);
            Buildings(town, a, t);
            Furniture(street, a);
            Rings(root, ext, a);
        }

        // Two more rings of town round it (StationRings): Main and Back Street run out west and east, the avenues south
        // and north, and a spur on the south, the side the open world's road comes in from, meets its middle.
        private static void Rings(Transform root, Transform ext, StationArt a)
        {
            var spec = new StationRingSpec
            {
                Inner = Rect.MinMaxRect(West, South, East, North), Road = 10f, Seed = 921, Vacancy = .1f, Lot = a.Gravel, Yard = a.Grass,
            };
            foreach (int side in new[] { StationRingSpec.South, StationRingSpec.North })
            {
                spec.Through(side, -40f, -30f);
                spec.Through(side, 98f, 108f);
            }
            foreach (int side in new[] { StationRingSpec.West, StationRingSpec.East })
            {
                spec.Through(side, -24f, -14f);
                spec.Through(side, 64f, 74f);
            }
            spec.Spur(StationRingSpec.South, 27f, 37f);
            var stock = new StationRingStock(a);
            StationRingPools.Town(stock, spec, 9200);
            StationRings.Build(root, ext, a, spec, stock);
        }

        private static void Streets(Transform root, Transform marks, StationArt a)
        {
            StationStreet.Road(root, marks, a, "MainStreet", Rect.MinMaxRect(West, -24f, East, -14f), true);
            StationStreet.Road(root, marks, a, "BackStreet", Rect.MinMaxRect(West, 64f, East, 74f), true);
            foreach ((float x, string n) in new[] { (-40f, "WestAvenue"), (98f, "EastAvenue") })
            {
                StationStreet.Road(root, marks, a, n + "_S", Rect.MinMaxRect(x, South, x + 10f, -24f), false);
                StationStreet.Road(root, marks, a, n + "_M", Rect.MinMaxRect(x, -14f, x + 10f, 64f), false);
                StationStreet.Road(root, marks, a, n + "_N", Rect.MinMaxRect(x, 74f, x + 10f, North), false);
            }
            foreach ((float z0, float z1, float kerb, string n) in new[] { (-30f, -24f, 1f, "MainS"), (60f, 64f, 1f, "BackS"), (74f, 78f, -1f, "BackN") })
                foreach ((float x0, float x1) in new[] { (West, -40f), (-30f, 98f), (108f, East) })
                    StationStreet.Sidewalk(root, marks, a, $"Sidewalk_{n}_{x0:0}", Rect.MinMaxRect(x0, z0, x1, z1), true, kerb);
            // Main Street's north side beyond the avenues (the station's own lot has its sidewalks and the apron).
            StationStreet.Sidewalk(root, marks, a, "Sidewalk_MainN_W", Rect.MinMaxRect(West, -14f, -40f, -10f), true, -1f);
            StationStreet.Sidewalk(root, marks, a, "Sidewalk_MainN_E", Rect.MinMaxRect(108f, -14f, East, -10f), true, -1f);
            foreach ((float x0, float x1, float kerb, string n, float mid) in new[]
                     { (-44f, -40f, 1f, "WestAveW", -10f), (94f, 98f, 1f, "EastAveW", -14f), (108f, 112f, -1f, "EastAveE", -10f) })
                foreach ((float z0, float z1) in new[] { (South, -30f), (mid, 60f), (78f, North) })
                    StationStreet.Sidewalk(root, marks, a, $"Sidewalk_{n}_{z0:0}", Rect.MinMaxRect(x0, z0, x1, z1), false, kerb);
            foreach ((float z0, float z1) in new[] { (South, -30f), (78f, North) })
                StationStreet.Sidewalk(root, marks, a, $"Sidewalk_WestAveE_{z0:0}", Rect.MinMaxRect(-30f, z0, -26f, z1), false, -1f);
            foreach (float x in new[] { -35f, 103f })
            foreach (float z in new[] { -19f, 69f })
            {
                StationStreet.Crosswalk(marks, a, new Vector3(x - 7.5f, 0f, z), true, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x + 7.5f, 0f, z), true, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x, 0f, z - 7.5f), false, 10f);
                StationStreet.Crosswalk(marks, a, new Vector3(x, 0f, z + 7.5f), false, 10f);
            }
        }

        private static void Lots(Transform root, StationArt a)
        {
            Plan.Ground(root, "Lot_SouthW", Rect.MinMaxRect(West, South, -44f, -30f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(root, "Lot_South", Rect.MinMaxRect(-26f, South, 94f, -30f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_SouthE", Rect.MinMaxRect(112f, South, East, -30f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_West", Rect.MinMaxRect(West, -10f, -44f, 60f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_East", Rect.MinMaxRect(112f, -10f, East, 60f), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_NorthW", Rect.MinMaxRect(West, 78f, -44f, North), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_North", Rect.MinMaxRect(-26f, 78f, 94f, North), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(root, "Lot_NorthE", Rect.MinMaxRect(112f, 78f, East, North), a.Gravel, SurfaceMaterial.Dirt);
        }

        private static void Buildings(Transform town, StationArt a, StationTownArt t)
        {
            // Across Main Street, facing the firehouse.
            Plan.Row(town, a, new Vector3(-25f, 0f, -30.3f), Vector3.right, 0f, new[]
            {
                new Lot(K.GasStation, 24f, 19f), new Lot(K.Shop, 10f, 18f, 2), new Lot(K.Shop, 10f, 18f, 3), new Lot(K.Shop, 8f, 17f, 2, 4f),
                new Lot(K.Apartment, 20f, 19f, 5), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3, 3f), new Lot(K.Warehouse, 22f, 19f),
            }, 410);
            Plan.Row(town, a, new Vector3(-69f, 0f, -30.3f), Vector3.right, 0f, new[] { new Lot(K.Garage, 22f, 18f) }, 420);
            Plan.Row(town, a, new Vector3(112.5f, 0f, -30.3f), Vector3.right, 0f, new[] { new Lot(K.Shop, 10f, 18f, 2), new Lot(K.Shop, 11f, 18f, 3) }, 430);
            // West of West Avenue, facing east.
            Plan.Row(town, a, new Vector3(-44.3f, 0f, -9f), Vector3.forward, 90f, new[]
            {
                new Lot(K.Apartment, 18f, 23f, 4), new Lot(K.Warehouse, 20f, 23f, 1, 1f), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3),
                new Lot(K.Rowhouse, 7f, 14f, 2),
            }, 440);
            // East of East Avenue, facing west: the church on the corner, two houses set back in their yards, a shop.
            Plan.Row(town, a, new Vector3(112.3f, 0f, -8f), Vector3.forward, -90f, new[] { new Lot(K.Church, 16f, 21f) }, 450);
            Plan.Row(town, a, new Vector3(115.5f, 0f, 11f), Vector3.forward, -90f, new[] { new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f) }, 460);
            Plan.Row(town, a, new Vector3(112.3f, 0f, 37f), Vector3.forward, -90f, new[] { new Lot(K.Shop, 12f, 18f, 2) }, 470);
            foreach (float z in new[] { 16f, 29f })
            {
                Plan.Picket(town, a, new Vector3(112.25f, 0f, z - 5.4f), new Vector3(112.25f, 0f, z - .7f), t.Frame);
                Plan.Picket(town, a, new Vector3(112.25f, 0f, z + .7f), new Vector3(112.25f, 0f, z + 5.2f), t.Frame);
                Plan.Paving(town, Rect.MinMaxRect(112.2f, z - .6f, 115.4f, z + .6f), a.Sidewalk);
                StationStreet.Mailbox(town, a, new Vector3(111.6f, 0f, z + 1.2f), -90f);
            }
            // Behind the rear yard, facing south: houses in their yards, then row houses.
            Plan.Row(town, a, new Vector3(-25f, 0f, 81.5f), Vector3.right, 180f, new[]
            {
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f),
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f),
            }, 480);
            Plan.Row(town, a, new Vector3(54f, 0f, 78.3f), Vector3.right, 180f, new[]
            {
                new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 2), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3),
            }, 490);
            for (int i = 0; i < 6; i++)
            {
                float cx = -20f + i * 13f;
                Plan.Picket(town, a, new Vector3(cx - 6.4f, 0f, 78.25f), new Vector3(cx - .7f, 0f, 78.25f), t.Frame);
                Plan.Picket(town, a, new Vector3(cx + .7f, 0f, 78.25f), new Vector3(cx + 5f, 0f, 78.25f), t.Frame);
                Plan.Paving(town, Rect.MinMaxRect(cx - .6f, 78.2f, cx + .6f, 81.4f), a.Sidewalk);
                Plan.Paving(town, Rect.MinMaxRect(cx + 5.2f, 78.2f, cx + 7.8f, 92f), a.Concrete);
                StationStreet.Mailbox(town, a, new Vector3(cx + 1.2f, 0f, 77.6f), 180f);
            }
            Plan.Row(town, a, new Vector3(-69f, 0f, 78.3f), Vector3.right, 180f, new[] { new Lot(K.Apartment, 24f, 20f, 4) }, 495);
            Plan.Row(town, a, new Vector3(112.5f, 0f, 78.3f), Vector3.right, 180f, new[] { new Lot(K.Warehouse, 21f, 20f) }, 497);
        }

        private static void Furniture(Transform s, StationArt a)
        {
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, -24.5f), Vector3.right, East - West - 6f, 32f, Vector3.forward * 5f);
            StationStreet.Lamp(s, a, new Vector3(-60f, -.02f, -10.5f), new Vector3(-60f, 0f, -16f));
            StationStreet.Lamp(s, a, new Vector3(122f, -.02f, -10.5f), new Vector3(122f, 0f, -16f));
            Plan.Lamps(s, a, new Vector3(-40.5f, 0f, South + 2f), Vector3.forward, North - South - 2f, 30f, Vector3.right * 5f);
            Plan.Lamps(s, a, new Vector3(108.5f, 0f, South + 2f), Vector3.forward, North - South - 2f, 30f, Vector3.left * 5f);
            Plan.Lamps(s, a, new Vector3(West + 8f, 0f, 74.5f), Vector3.right, East - West - 8f, 32f, Vector3.back * 5f);
            var back = new List<Vector3>();
            foreach (float x in new[] { -18f, 8f, 34f, 60f, 86f, 120f }) back.Add(new Vector3(x, -.02f, 61f));
            StationStreet.PowerLine(s, a, back);
            var westLine = new List<Vector3>();
            foreach (float z in new[] { -46f, -27f, 0f, 28f, 61f, 88f }) westLine.Add(new Vector3(-43f, -.02f, z));
            StationStreet.PowerLine(s, a, westLine);
            StationStreet.TrafficLight(s, a, new Vector3(-29.3f, -.02f, -13.4f), 180f);
            StationStreet.TrafficLight(s, a, new Vector3(-40.7f, -.02f, -24.6f), 0f);
            StationStreet.TrafficLight(s, a, new Vector3(97.3f, -.02f, -13.4f), 180f);
            StationStreet.TrafficLight(s, a, new Vector3(108.7f, -.02f, -24.6f), 0f);
            foreach (Vector3 p in new[] { new Vector3(-29.4f, -.02f, 63.4f), new Vector3(-40.6f, -.02f, 74.6f), new Vector3(97.4f, -.02f, 63.4f), new Vector3(108.6f, -.02f, 74.6f) })
                StationStreet.StopSign(s, a, p, p.z < 69f ? 180f : 0f);
            foreach (Vector3 p in new[] { new Vector3(-40.8f, -.02f, -13.2f), new Vector3(97.2f, -.02f, -24.8f), new Vector3(-29.2f, -.02f, 74.8f), new Vector3(108.8f, -.02f, 63.2f) })
                StationStreet.StreetSign(s, a, p, 0f);
            StationStreet.AlarmBox(s, a, new Vector3(39f, -.02f, -12.4f), 180f);
            StationStreet.AlarmBox(s, a, new Vector3(-24f, -.02f, -27.5f), 0f);
            StationStreet.AlarmBox(s, a, new Vector3(-42f, -.02f, 40f), 90f);
            StationStreet.BusShelter(s, a, new Vector3(60f, -.02f, -28.4f), 0f);
            StationStreet.Bench(s, a, new Vector3(70f, -.02f, -29.2f), 0f);
            StationStreet.NewsBoxes(s, a, new Vector3(8f, -.02f, -29.3f), 0f);
            StationStreet.Mailbox(s, a, new Vector3(14f, -.02f, -29.4f), 0f);
            foreach ((float x0, float x1) in new[] { (West, -40f), (-30f, 98f), (108f, East) })
                Plan.Kerbside(s, a, new Vector3(x0, -.02f, -25f), Vector3.right, x1 - x0, 0f, 5 + Mathf.RoundToInt(x0));
            Plan.Kerbside(s, a, new Vector3(-26f, -.02f, 77f), Vector3.right, 120f, 180f, 11);
            Plan.Kerbside(s, a, new Vector3(109f, -.02f, -10f), Vector3.forward, 70f, 270f, 13);
            Plan.Kerbside(s, a, new Vector3(-41f, -.02f, -10f), Vector3.forward, 70f, 90f, 17);
            // Cars along the kerbs: none on the apron's side of Main Street, where the van comes and goes.
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, -15.1f), Vector3.right, 26f, 71);
            Plan.ParkedCars(s, a, new Vector3(42f, 0f, -15.1f), Vector3.right, 54f, 73);
            Plan.ParkedCars(s, a, new Vector3(110f, 0f, -15.1f), Vector3.right, 22f, 75);
            Plan.ParkedCars(s, a, new Vector3(East - 2f, 0f, -22.9f), Vector3.left, East - West - 4f, 77, new Vector2(22f, 36f), new Vector2(66f, 78f),
                new Vector2(133f, 157f), new Vector2(160f, 174f));
            Plan.ParkedCars(s, a, new Vector3(-31.1f, 0f, North - 2f), Vector3.back, North - South - 4f, 79, new Vector2(22f, 36f), new Vector2(110f, 124f));
            Plan.ParkedCars(s, a, new Vector3(99.1f, 0f, South + 2f), Vector3.forward, North - South - 4f, 83, new Vector2(22f, 36f), new Vector2(110f, 124f));
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, 72.9f), Vector3.right, East - West - 4f, 89, new Vector2(28f, 40f), new Vector2(166f, 180f));
            StationStreet.Dumpster(s, a, new Vector3(29f, -.02f, -36f), 90f, true);
            StationStreet.Dumpster(s, a, new Vector3(66.5f, -.02f, -36f), 90f, false);
            StationStreet.Dumpster(s, a, new Vector3(-68.6f, -.02f, 15f), 90f, true);
            StationStreet.Dumpster(s, a, new Vector3(132f, -.02f, 43f), 90f, false);
            foreach (Vector3 p in new[] { new Vector3(29f, -.02f, -42f), new Vector3(66.5f, -.02f, -44f), new Vector3(-68.5f, -.02f, 40f),
                         new Vector3(-20f, -.02f, 95f), new Vector3(6f, -.02f, 96f), new Vector3(32f, -.02f, 95f) })
                StationStreet.Litter(s, a, p, Mathf.RoundToInt(p.x));
            // Road works someone walked away from on the back street.
            StationStreet.Sawhorse(s, a, new Vector3(20f, -.02f, 67.5f), 0f);
            StationStreet.Sawhorse(s, a, new Vector3(24f, -.02f, 67.5f), 10f);
            StationStreet.Barriers(s, new Vector3(18f, -.02f, 65.5f), new Vector3(26f, -.02f, 65.5f), 2);
        }
    }
}
