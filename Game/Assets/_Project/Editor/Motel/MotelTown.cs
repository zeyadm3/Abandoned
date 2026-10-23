using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Lot = Abandoned.EditorTools.StationTownPlan.Lot;
using M = Abandoned.EditorTools.StationMeshes;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;

namespace Abandoned.Motel
{
    /// <summary>
    /// The roadside round the Last Light Motor Lodge (Docs/MOTEL_PLAN.md "The roadside round it"), built with the station
    /// town kit outside the motel's own ground (x -8..112, z -24..44, top 0), which it never touches: the highway along the
    /// front, an access road running south from the van apron's mouth at x 32 (the open world's road comes in along it),
    /// side roads and a back road. Across the highway: a gas station, a burger stand, a strip mall behind its car park, a
    /// tyre shop, warehouses and a liquor store; down the sides an auto salvage garage, a strip mall, shops and houses;
    /// behind, a row of houses in their yards, a church and a warehouse. Billboards over the highway, a water tower,
    /// power lines, wrecks left on the road. Exterior only, off the NavMesh. Whole ground x -80..190, z -110..90, top 0.
    /// </summary>
    public static class MotelTown
    {
        public const float West = -80f, East = 190f, South = -110f, North = 90f;

        public static void Build(Transform root)
        {
            var a = new StationArt(MotelKit.Art, "MotelTown", new Color(.7f, .48f, .38f), new Color(.62f, .32f, .12f), new Color(1f, .78f, .5f));
            Transform town = Group("Roadside", root);
            town.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            float previous = Site.GroundTop;
            Site.GroundTop = 0f;
            try
            {
                Transform ext = Site.Exterior(town);
                Transform marks = Group("RoadMarkings", ext);
                Transform blocks = Group("Blocks", ext);
                Transform street = Group("RoadFurniture", ext);
                StationTownArt t = StationTownArt.For(a);
                Roads(town, marks, a);
                Lots(town, a);
                Buildings(blocks, a, t);
                Yards(blocks, a, t);
                Furniture(street, marks, a);
                Landmarks(street, a, t);
                MarkStatic(ext);
                Site.TrimOccluders(ext);
                StationCoverMarks.Apply(ext);
                StationSurfaces.Apply(town, a);
            }
            finally
            {
                Site.GroundTop = previous;
            }
        }

        private static void Roads(Transform town, Transform marks, StationArt a)
        {
            Street.Road(town, marks, a, "Highway", Rect.MinMaxRect(West, -40f, East, -24f), true);
            Street.Road(town, marks, a, "BackRoad", Rect.MinMaxRect(West, 44f, East, 54f), true);
            foreach ((float x, string n) in new[] { (-20f, "WestRoad"), (112f, "EastRoad") })
            {
                Street.Road(town, marks, a, n + "_S", Rect.MinMaxRect(x, South, x + 12f, -40f), false);
                Street.Road(town, marks, a, n + "_M", Rect.MinMaxRect(x, -24f, x + 12f, 44f), false);
                Street.Road(town, marks, a, n + "_N", Rect.MinMaxRect(x, 54f, x + 12f, North), false);
            }
            Street.Road(town, marks, a, "AccessRoad", Rect.MinMaxRect(27f, South, 37f, -40f), false);
            foreach ((float x0, float x1) in new[] { (West, -20f), (-8f, 27f), (37f, 112f), (124f, East) })
                Street.Sidewalk(town, marks, a, $"Verge_HighwayS_{x0:0}", Rect.MinMaxRect(x0, -44f, x1, -40f), true, 1f);
            foreach ((float x0, float x1) in new[] { (West, -20f), (-8f, 112f), (124f, East) })
                Street.Sidewalk(town, marks, a, $"Sidewalk_BackN_{x0:0}", Rect.MinMaxRect(x0, 54f, x1, 58f), true, -1f);
            foreach ((float z0, float z1) in new[] { (South, -44f), (-24f, 44f), (58f, North) })
            {
                Street.Sidewalk(town, marks, a, $"Sidewalk_WestW_{z0:0}", Rect.MinMaxRect(-24f, z0, -20f, z1), false, 1f);
                Street.Sidewalk(town, marks, a, $"Sidewalk_EastE_{z0:0}", Rect.MinMaxRect(124f, z0, 128f, z1), false, -1f);
            }
            foreach ((float z0, float z1) in new[] { (South, -44f), (58f, North) })
            {
                Street.Sidewalk(town, marks, a, $"Sidewalk_WestE_{z0:0}", Rect.MinMaxRect(-8f, z0, -4f, z1), false, -1f);
                Street.Sidewalk(town, marks, a, $"Sidewalk_EastW_{z0:0}", Rect.MinMaxRect(108f, z0, 112f, z1), false, 1f);
            }
            Street.Sidewalk(town, marks, a, "Sidewalk_AccessW", Rect.MinMaxRect(23f, South, 27f, -44f), false, 1f);
            Street.Sidewalk(town, marks, a, "Sidewalk_AccessE", Rect.MinMaxRect(37f, South, 41f, -44f), false, -1f);
            // Kerbs where the motel's own ground meets the roads; its apron's mouth onto the highway stays open.
            Site.Kerb(marks, new Vector3(-8.12f, 0f, -24f), new Vector3(-8.12f, 0f, 44f), a);
            Site.Kerb(marks, new Vector3(112.12f, 0f, -24f), new Vector3(112.12f, 0f, 44f), a);
            Site.Kerb(marks, new Vector3(-8f, 0f, 44.12f), new Vector3(112f, 0f, 44.12f), a);
            Site.Kerb(marks, new Vector3(-8f, 0f, -24.12f), new Vector3(24f, 0f, -24.12f), a);
            Site.Kerb(marks, new Vector3(40f, 0f, -24.12f), new Vector3(112f, 0f, -24.12f), a);
            foreach (float x in new[] { -14f, 118f })
            {
                Street.Crosswalk(marks, a, new Vector3(x - 7.5f, 0f, -32f), true, 16f);
                Street.Crosswalk(marks, a, new Vector3(x + 7.5f, 0f, -32f), true, 16f);
                Street.Crosswalk(marks, a, new Vector3(x, 0f, -41.5f), false, 12f);
                Street.Crosswalk(marks, a, new Vector3(x, 0f, -22.5f), false, 12f);
                Street.Crosswalk(marks, a, new Vector3(x - 7.5f, 0f, 49f), true, 10f);
                Street.Crosswalk(marks, a, new Vector3(x + 7.5f, 0f, 49f), true, 10f);
            }
            // Rumble strips and a turn arrow where the access road meets the highway.
            for (int i = 0; i < 6; i++) Box("Rumble", marks, new Vector3(32f, .006f, -46f - i * .5f), new Vector3(9f, .01f, .15f), a.Paint, false);
            Box("TurnArrow", marks, new Vector3(30f, .006f, -52f), new Vector3(.3f, .01f, 3f), a.Paint, false);
        }

        private static void Lots(Transform town, StationArt a)
        {
            Plan.Ground(town, "Lot_SW", Rect.MinMaxRect(West, South, -24f, -44f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_S1", Rect.MinMaxRect(-4f, South, 23f, -44f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(town, "Lot_S2", Rect.MinMaxRect(41f, South, 108f, -44f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(town, "Lot_SE", Rect.MinMaxRect(128f, South, East, -44f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_W", Rect.MinMaxRect(West, -24f, -24f, 44f), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_E", Rect.MinMaxRect(128f, -24f, East, 44f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(town, "Lot_NW", Rect.MinMaxRect(West, 58f, -24f, North), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_N", Rect.MinMaxRect(-4f, 58f, 108f, North), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_NE", Rect.MinMaxRect(128f, 58f, East, North), a.Gravel, SurfaceMaterial.Dirt);
        }

        private static void Buildings(Transform b, StationArt a, StationTownArt t)
        {
            // Across the highway, facing the motel.
            Plan.Row(b, a, new Vector3(-3f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.GasStation, 26f, 24f) }, 710);
            Plan.Row(b, a, new Vector3(42f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.Shop, 12f, 16f, 1) }, 720);
            Plan.Row(b, a, new Vector3(58f, 0f, -62f), Vector3.right, 0f, new[] { new Lot(K.StripMall, 36f, 16f) }, 730);
            Plan.Row(b, a, new Vector3(95f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.Garage, 13f, 16f) }, 740);
            Plan.Row(b, a, new Vector3(-78f, 0f, -44.3f), Vector3.right, 0f, new[]
            {
                new Lot(K.Warehouse, 30f, 26f, 1, 2f), new Lot(K.Shop, 10f, 16f, 1, 2f), new Lot(K.Shop, 9f, 16f, 2),
            }, 750);
            Plan.Row(b, a, new Vector3(130f, 0f, -44.3f), Vector3.right, 0f, new[]
            {
                new Lot(K.Warehouse, 26f, 24f, 1, 1f), new Lot(K.Shop, 10f, 16f, 1, 2f), new Lot(K.Garage, 18f, 18f),
            }, 760);
            // West of the west road, facing east: the auto salvage garage, two houses set back, a shop.
            Plan.Row(b, a, new Vector3(-24.3f, 0f, -22f), Vector3.forward, 90f, new[] { new Lot(K.Garage, 20f, 22f) }, 770);
            Plan.Row(b, a, new Vector3(-27.5f, 0f, 4f), Vector3.forward, 90f, new[] { new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f) }, 775);
            Plan.Row(b, a, new Vector3(-24.3f, 0f, 32f), Vector3.forward, 90f, new[] { new Lot(K.Shop, 10f, 16f, 2) }, 780);
            // East of the east road, facing west: a strip mall, a shop, a house.
            Plan.Row(b, a, new Vector3(128.3f, 0f, -22f), Vector3.forward, -90f, new[] { new Lot(K.StripMall, 30f, 18f, 1, 2f), new Lot(K.Shop, 12f, 16f, 2) }, 790);
            Plan.Row(b, a, new Vector3(131.5f, 0f, 28f), Vector3.forward, -90f, new[] { new Lot(K.House, 10f, 11f) }, 795);
            // Behind the back road, facing south: houses in their yards, a warehouse; the church and two houses at the
            // west corner, a warehouse at the east.
            Plan.Row(b, a, new Vector3(-2f, 0f, 61.5f), Vector3.right, 180f, new[]
            {
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f),
                new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f),
            }, 800);
            Plan.Row(b, a, new Vector3(89f, 0f, 58.3f), Vector3.right, 180f, new[] { new Lot(K.Warehouse, 18f, 20f) }, 810);
            Plan.Row(b, a, new Vector3(-78f, 0f, 58.3f), Vector3.right, 180f, new[] { new Lot(K.Church, 16f, 22f) }, 820);
            Plan.Row(b, a, new Vector3(-58f, 0f, 61.5f), Vector3.right, 180f, new[] { new Lot(K.House, 10f, 11f, 2, 3f), new Lot(K.House, 10f, 11f) }, 825);
            Plan.Row(b, a, new Vector3(130f, 0f, 58.3f), Vector3.right, 180f, new[] { new Lot(K.Warehouse, 24f, 22f) }, 830);
        }

        // Fences, paths and driveways for the houses; the strip mall's car park.
        private static void Yards(Transform b, StationArt a, StationTownArt t)
        {
            for (int i = 0; i < 7; i++)
            {
                float cx = 3f + i * 13f;
                Plan.Picket(b, a, new Vector3(cx - 6.4f, 0f, 58.25f), new Vector3(cx - .7f, 0f, 58.25f), t.Frame);
                Plan.Picket(b, a, new Vector3(cx + .7f, 0f, 58.25f), new Vector3(cx + 5f, 0f, 58.25f), t.Frame);
                Plan.Paving(b, Rect.MinMaxRect(cx - .6f, 58.2f, cx + .6f, 61.4f), a.Sidewalk);
                if (i < 6) Plan.Paving(b, Rect.MinMaxRect(cx + 5.2f, 58.2f, cx + 7.8f, 72f), a.Concrete);
                Street.Mailbox(b, a, new Vector3(cx + 1.2f, 0f, 57.6f), 180f);
            }
            foreach (float cx in new[] { -53f, -40f })
            {
                Plan.Picket(b, a, new Vector3(cx - 5f, 0f, 58.25f), new Vector3(cx - .7f, 0f, 58.25f), t.Frame);
                Plan.Picket(b, a, new Vector3(cx + .7f, 0f, 58.25f), new Vector3(cx + 5f, 0f, 58.25f), t.Frame);
                Plan.Paving(b, Rect.MinMaxRect(cx - .6f, 58.2f, cx + .6f, 61.4f), a.Sidewalk);
            }
            foreach (float z in new[] { 9f, 22f })
            {
                Plan.Picket(b, a, new Vector3(-24.25f, 0f, z - 5f), new Vector3(-24.25f, 0f, z - .7f), t.Frame);
                Plan.Picket(b, a, new Vector3(-24.25f, 0f, z + .7f), new Vector3(-24.25f, 0f, z + 5f), t.Frame);
                Plan.Paving(b, Rect.MinMaxRect(-27.4f, z - .6f, -24.2f, z + .6f), a.Sidewalk);
            }
            Plan.Picket(b, a, new Vector3(128.25f, 0f, 28f), new Vector3(128.25f, 0f, 32.3f), t.Frame);
            Plan.Picket(b, a, new Vector3(128.25f, 0f, 33.7f), new Vector3(128.25f, 0f, 38f), t.Frame);
            // The strip mall's car park in front of it: bays, the few cars left, a cart corral.
            for (int i = 0; i <= 10; i++)
                Box("Bay", b, new Vector3(60f + i * 3.2f, .006f, -55f), new Vector3(.12f, .01f, 5f), a.Paint, false);
            StationVehicleArt v = StationVehicleArt.For(a);
            foreach ((float x, int seed) in new[] { (64.8f, 1), (71.2f, 4), (80.8f, 7), (87.2f, 9) })
                StationVehicles.Car(b, "MallCar", new Vector3(x, 0f, -55f), seed % 2 == 0 ? 0f : 180f, (StationVehicleKind)(new[] { 0, 2, 3, 4 }[seed % 4]), v.Paint(seed), a,
                    StationVehicleWear.Dusty | (seed % 3 == 0 ? StationVehicleWear.Smashed : StationVehicleWear.FlatFront), seed);
            Box("CartCorral", b, new Vector3(76f, .5f, -51f), new Vector3(1.2f, 1f, 4f), a.Steel);
        }

        private static void Furniture(Transform s, Transform marks, StationArt a)
        {
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, -40.6f), Vector3.right, East - West - 6f, 32f, Vector3.forward * 7f, 10f);
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, 54.6f), Vector3.right, East - West - 6f, 32f, Vector3.back * 5f);
            foreach (float z in new[] { -96f, -66f, -14f, 14f, 40f, 66f, 86f })
            {
                Street.Lamp(s, a, new Vector3(-20.6f, 0f, z), new Vector3(-14f, 0f, z));
                Street.Lamp(s, a, new Vector3(124.6f, 0f, z), new Vector3(118f, 0f, z));
            }
            Plan.Lamps(s, a, new Vector3(26.6f, 0f, South), Vector3.forward, 64f, 22f, Vector3.right * 5f);
            var front = new List<Vector3>();
            foreach (float x in new[] { -72f, -40f, -6f, 22f, 50f, 80f, 108f, 140f, 172f }) front.Add(new Vector3(x, 0f, -42f));
            Street.PowerLine(s, a, front);
            var back = new List<Vector3>();
            foreach (float x in new[] { -70f, -40f, -2f, 34f, 60f, 90f, 126f, 158f }) back.Add(new Vector3(x, 0f, 56f));
            Street.PowerLine(s, a, back);
            Street.TrafficLight(s, a, new Vector3(-20.6f, 0f, -23.4f), 180f, 7f);
            Street.TrafficLight(s, a, new Vector3(-7.4f, 0f, -40.6f), 0f, 7f);
            Street.TrafficLight(s, a, new Vector3(124.6f, 0f, -23.4f), 180f, 7f);
            Street.TrafficLight(s, a, new Vector3(111.4f, 0f, -40.6f), 0f, 7f);
            Street.TrafficLight(s, a, new Vector3(37.6f, 0f, -40.8f), 0f, 7f);
            foreach (Vector3 p in new[] { new Vector3(-20.6f, 0f, 43.4f), new Vector3(124.6f, 0f, 43.4f), new Vector3(-7.4f, 0f, 58.6f), new Vector3(111.4f, 0f, 58.6f) })
                Street.StopSign(s, a, p, p.z < 50f ? 180f : 0f);
            foreach (Vector3 p in new[] { new Vector3(-21f, 0f, -42.5f), new Vector3(125f, 0f, -42.5f), new Vector3(24f, 0f, -42.5f), new Vector3(-21f, 0f, 56f) })
                Street.StreetSign(s, a, p, 0f);
            Street.BusShelter(s, a, new Vector3(100f, 0f, -42.4f), 0f);
            Street.Bench(s, a, new Vector3(52f, 0f, -43.2f), 0f);
            Street.NewsBoxes(s, a, new Vector3(56.5f, 0f, -43.3f), 0f);
            foreach ((float x0, float x1) in new[] { (West, -20f), (-8f, 27f), (37f, 112f), (124f, East) })
                Plan.Kerbside(s, a, new Vector3(x0, 0f, -41f), Vector3.right, x1 - x0, 0f, 11 + Mathf.RoundToInt(x0));
            Plan.Kerbside(s, a, new Vector3(-4f, 0f, 57f), Vector3.right, 112f, 180f, 13);
            Plan.Kerbside(s, a, new Vector3(-23f, 0f, -24f), Vector3.forward, 68f, 90f, 17);
            // Cars along the side roads and the back road; the highway has its wrecks instead.
            Plan.ParkedCars(s, a, new Vector3(-9.1f, 0f, North - 2f), Vector3.back, North - South - 4f, 731, new Vector2(30f, 46f), new Vector2(110f, 130f));
            Plan.ParkedCars(s, a, new Vector3(122.9f, 0f, South + 2f), Vector3.forward, North - South - 4f, 733, new Vector2(66f, 86f), new Vector2(150f, 164f));
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, 52.9f), Vector3.right, East - West - 4f, 737, new Vector2(56f, 72f), new Vector2(188f, 204f));
            StationVehicleArt v = StationVehicleArt.For(a);
            StationVehicles.Car(s, "HighwayWreck", new Vector3(-48f, 0f, -27.5f), 86f, StationVehicleKind.Sedan, v.Paint(2), a, StationVehicleWear.Burnt, 1);
            StationVehicles.Car(s, "HighwayWreck", new Vector3(70f, 0f, -36.4f), 262f, StationVehicleKind.Pickup, v.Paint(5), a,
                StationVehicleWear.FlatFront | StationVehicleWear.DoorOpen | StationVehicleWear.Dusty, 2);
            StationVehicles.Car(s, "HighwayWreck", new Vector3(152f, 0f, -29f), 110f, StationVehicleKind.Suv, v.Paint(8), a, StationVehicleWear.Smashed | StationVehicleWear.FlatRear, 3);
            Transform tow = Group("TowTruck", s);
            tow.SetPositionAndRotation(new Vector3(160f, 0f, -36f), Quaternion.Euler(0f, 270f, 0f));
            StationApparatus.TowTruck(tow, a, a.Plain("TowOrange", new Color(.8f, .4f, .08f), .4f, .2f));
            var solid = tow.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.2f, -.3f);
            solid.size = new Vector3(2.4f, 2.4f, 8f);
            Street.Sawhorse(s, a, new Vector3(146f, 0f, -33f), 20f);
            Street.Dumpster(s, a, new Vector3(56f, 0f, -70f), 90f, true);
            Street.Dumpster(s, a, new Vector3(-40f, 0f, -66f), 0f, false);
            Street.Dumpster(s, a, new Vector3(186f, 0f, -4f), 90f, true);
            foreach (Vector3 p in new[] { new Vector3(100f, 0f, -70f), new Vector3(-60f, 0f, 20f), new Vector3(170f, 0f, 30f), new Vector3(20f, 0f, 84f) })
                Street.Litter(s, a, p, Mathf.RoundToInt(p.x - p.z));
        }

        private static void Landmarks(Transform s, StationArt a, StationTownArt t)
        {
            Billboard(s, a, t, new Vector3(-60f, 0f, -20f), 180f, 1);
            Billboard(s, a, t, new Vector3(165f, 0f, -20f), 180f, 2);
            // One facing the access road, for whoever drives in from the south.
            Billboard(s, a, t, new Vector3(8f, 0f, -95f), 90f, 3);
            WaterTower(s, a, new Vector3(172f, 0f, 74f));
        }

        // A highway billboard on two steel posts: a framed panel of faded sheets, a catwalk under it, lamps on arms.
        private static void Billboard(Transform s, StationArt a, StationTownArt t, Vector3 at, float yaw, int seed)
        {
            Transform bb = Group("Billboard", s);
            bb.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            foreach (float x in new[] { -3f, 3f })
            {
                Box("Post", bb, new Vector3(x, 4.5f, 0f), new Vector3(.45f, 9f, .45f), a.DarkSteel);
                Box("Footing", bb, new Vector3(x, .3f, 0f), new Vector3(1.2f, .6f, 1.2f), a.Concrete);
            }
            Box("Frame", bb, new Vector3(0f, 8.5f, -.1f), new Vector3(12.4f, 4.4f, .3f), a.DarkSteel, false);
            Material[] sheets = { t.Sign(seed), a.Paper, t.Sign(seed + 2), t.Awning(seed) };
            for (int i = 0; i < 4; i++)
            {
                GameObject sheet = Box("Sheet", bb, new Vector3(-4.5f + i * 3f, 8.5f, .07f), new Vector3(2.98f, 4f, .02f), sheets[i], false);
                if ((i + seed) % 3 == 0) sheet.transform.localRotation = Quaternion.Euler(0f, 0f, 2f);
            }
            Box("PeeledCorner", bb, new Vector3(4.6f, 9.9f, .12f), new Vector3(1.2f, .9f, .01f), a.Grime, false);
            Box("Catwalk", bb, new Vector3(0f, 6.2f, .6f), new Vector3(12f, .06f, .9f), a.Grating, false);
            Box("CatwalkRail", bb, new Vector3(0f, 7.2f, 1.05f), new Vector3(12f, .05f, .05f), a.Steel, false);
            for (int i = 0; i < 4; i++)
            {
                float x = -4.5f + i * 3f;
                M.Strut(bb, "LampArm", new Vector3(x, 6.3f, .6f), new Vector3(x, 6.9f, 1.8f), .05f, a.DarkSteel);
                Box("LampHead", bb, new Vector3(x, 6.95f, 1.85f), new Vector3(.4f, .14f, .25f), a.DarkSteel, false);
            }
            M.Strut(bb, "Ladder", new Vector3(3.4f, 0f, .3f), new Vector3(3.4f, 6.2f, .3f), .05f, a.Steel, .45f);
        }

        // The town's water tower: four raking legs with cross-bracing, a ladder to the balcony, the tank and its cone.
        private static void WaterTower(Transform s, StationArt a, Vector3 at)
        {
            Transform w = Group("WaterTower", s);
            w.position = at;
            Material steel = a.Plain("TowerSteel", new Color(.55f, .58f, .58f), .35f, .5f);
            const float top = 20f, spread = 4.5f, head = 2.2f;
            var feet = new List<Vector3>();
            var heads = new List<Vector3>();
            for (int i = 0; i < 4; i++)
            {
                float ang = (45f + i * 90f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang));
                feet.Add(dir * spread);
                heads.Add(dir * head + Vector3.up * top);
                M.Strut(w, "Leg", dir * spread, dir * head + Vector3.up * top, .35f, steel);
                Box("Footing", w, dir * spread + Vector3.up * .3f, new Vector3(1.2f, .6f, 1.2f), a.Concrete);
            }
            for (int level = 1; level < 4; level++)
            {
                float u = level / 4f;
                for (int i = 0; i < 4; i++)
                {
                    Vector3 p = Vector3.Lerp(feet[i], heads[i], u), q = Vector3.Lerp(feet[(i + 1) % 4], heads[(i + 1) % 4], u);
                    Vector3 p2 = Vector3.Lerp(feet[i], heads[i], u + .25f), q2 = Vector3.Lerp(feet[(i + 1) % 4], heads[(i + 1) % 4], u + .25f);
                    M.Strut(w, "Girt", p, q, .14f, steel);
                    if (level < 3) M.Strut(w, "Brace", p, q2, .06f, steel);
                    if (level < 3) M.Strut(w, "Brace", q, p2, .06f, steel);
                }
            }
            M.Draw(w, "Tank", M.Lathe(a, "WaterTowerTank", new[]
            {
                new Vector2(0f, 0f), new Vector2(1.6f, .3f), new Vector2(3f, 1.2f), new Vector2(3.6f, 2.4f), new Vector2(3.6f, 5.6f), new Vector2(3.6f, 5.6f),
                new Vector2(3.8f, 5.6f), new Vector2(3.8f, 5.6f), new Vector2(1.2f, 7f), new Vector2(.3f, 7.3f), new Vector2(0f, 7.35f),
            }, 32), new Vector3(0f, top - .4f, 0f), Vector3.zero, Vector3.one, steel);
            Primitive(PrimitiveType.Cylinder, "Balcony", w, new Vector3(0f, top + 2.4f, 0f), new Vector3(8.4f, .04f, 8.4f), a.Grating, false);
            Primitive(PrimitiveType.Cylinder, "BalconyRail", w, new Vector3(0f, top + 3.4f, 0f), new Vector3(8.5f, .03f, 8.5f), a.Steel, false);
            Box("Riser", w, new Vector3(0f, top * .5f, 0f), new Vector3(.7f, top, .7f), steel, false);
            M.Strut(w, "Ladder", new Vector3(spread * .7f, 0f, 0f), new Vector3(3.9f, top + 2.4f, 0f), .06f, a.DarkSteel, .45f);
            Box("Band", w, new Vector3(0f, top + 3.6f, 3.62f), new Vector3(3.4f, 1.2f, .02f), StationTownArt.For(a).Sign(4), false);
        }
    }
}
