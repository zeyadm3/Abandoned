using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.EditorTools;
using Unity.AI.Navigation;
using UnityEngine;
using K = Abandoned.EditorTools.StationTownBuildings.Kind;
using Lot = Abandoned.EditorTools.StationTownPlan.Lot;
using Plan = Abandoned.EditorTools.StationTownPlan;
using Site = Abandoned.EditorTools.StationSite;
using Street = Abandoned.EditorTools.StationStreet;

namespace Abandoned.Hospital
{
    /// <summary>
    /// The district round Saint Brigid's (Docs/HOSPITAL_PLAN.md "The district round it"), built with the station town kit
    /// outside the hospital's own ground (x -12..68, z -28..60, top 0), which it never touches: Hospital Boulevard along
    /// the front, Hospital Drive running south from the ambulance approach (the open world's road comes in along it to
    /// the unchanged van dock), West and East Streets and a back street round the other sides. Medical office towers,
    /// two parking decks, a pharmacy, cafes, a gas station, flats, row houses, a chapel and a supply warehouse face in;
    /// signals, lamps, power lines, bus shelters, a taxi rank in the boulevard's kerb lane, abandoned ambulances and a
    /// quarantine checkpoint left open on the drive. Exterior only, off the NavMesh. District ground x -64..124, z -100..112,
    /// top 0; two more rings round it (StationRings) take the whole ground to x -190..250, z -226..238.
    /// </summary>
    public static class HospitalTown
    {
        public const float West = -64f, East = 124f, South = -100f, North = 112f;

        public static void Build(Transform root)
        {
            var a = new StationArt(HospitalKit.Art, "HospitalTown", new Color(.68f, .45f, .36f), new Color(.16f, .34f, .29f), new Color(1f, .8f, .55f));
            Transform town = HospitalKit.Group("Neighbourhood", root);
            town.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            float previous = Site.GroundTop;
            Site.GroundTop = 0f;
            try
            {
                Transform ext = Site.Exterior(town);
                Transform marks = HospitalKit.Group("StreetMarkings", ext);
                Transform blocks = HospitalKit.Group("Blocks", ext);
                Transform street = HospitalKit.Group("StreetFurniture", ext);
                Streets(town, marks, a);
                Lots(town, a);
                Buildings(blocks, a, StationTownArt.For(a));
                Furniture(street, marks, a);
                Rings(town, ext, a);
                GreyboxFactory.MarkStatic(ext);
                GreyboxFactory.MarkStatic(Site.GroundSlabs(town));
                Site.TrimOccluders(ext);
                StationRings.Settle(ext);
                StationCoverMarks.Apply(ext);
                StationSurfaces.Apply(town, a);
            }
            finally
            {
                Site.GroundTop = previous;
            }
        }

        // Two more rings of the district round it (StationRings): the boulevard and the back street run out west and east,
        // West and East Streets south and north, and Hospital Drive on south to the new edge.
        private static void Rings(Transform town, Transform ext, StationArt a)
        {
            var spec = new StationRingSpec
            {
                Inner = Rect.MinMaxRect(West, South, East, North), Road = 12f, Seed = 931, Vacancy = .08f, Lot = a.Gravel, Yard = a.Grass,
                LampIntensity = 9f,
            };
            foreach (int side in new[] { StationRingSpec.South, StationRingSpec.North })
            {
                spec.Through(side, -24f, -12f);
                spec.Through(side, 68f, 80f);
            }
            spec.Through(StationRingSpec.South, 39f, 49f);
            foreach (int side in new[] { StationRingSpec.West, StationRingSpec.East })
            {
                spec.Through(side, -40f, -28f);
                spec.Through(side, 60f, 72f);
            }
            var stock = new StationRingStock(a);
            StationRingPools.District(stock, spec, 9300);
            StationRings.Build(town, ext, a, spec, stock);
        }

        private static void Streets(Transform town, Transform marks, StationArt a)
        {
            Street.Road(town, marks, a, "HospitalBoulevard", Rect.MinMaxRect(West, -40f, East, -28f), true);
            Street.Road(town, marks, a, "BackStreet", Rect.MinMaxRect(West, 60f, East, 72f), true);
            foreach ((float x, string n) in new[] { (-24f, "WestStreet"), (68f, "EastStreet") })
            {
                Street.Road(town, marks, a, n + "_S", Rect.MinMaxRect(x, South, x + 12f, -40f), false);
                Street.Road(town, marks, a, n + "_M", Rect.MinMaxRect(x, -28f, x + 12f, 60f), false);
                Street.Road(town, marks, a, n + "_N", Rect.MinMaxRect(x, 72f, x + 12f, North), false);
            }
            Street.Road(town, marks, a, "HospitalDrive", Rect.MinMaxRect(39f, South, 49f, -40f), false);
            foreach ((float x0, float x1) in new[] { (West, -24f), (-12f, 39f), (49f, 68f), (80f, East) })
                Street.Sidewalk(town, marks, a, $"Sidewalk_BlvdS_{x0:0}", Rect.MinMaxRect(x0, -44f, x1, -40f), true, 1f);
            foreach ((float x0, float x1) in new[] { (West, -24f), (-12f, 68f), (80f, East) })
                Street.Sidewalk(town, marks, a, $"Sidewalk_BackN_{x0:0}", Rect.MinMaxRect(x0, 72f, x1, 76f), true, -1f);
            foreach ((float z0, float z1) in new[] { (South, -44f), (-28f, 60f), (76f, North) })
            {
                Street.Sidewalk(town, marks, a, $"Sidewalk_WestW_{z0:0}", Rect.MinMaxRect(-28f, z0, -24f, z1), false, 1f);
                Street.Sidewalk(town, marks, a, $"Sidewalk_EastE_{z0:0}", Rect.MinMaxRect(80f, z0, 84f, z1), false, -1f);
            }
            foreach ((float z0, float z1) in new[] { (South, -44f), (76f, North) })
            {
                Street.Sidewalk(town, marks, a, $"Sidewalk_WestE_{z0:0}", Rect.MinMaxRect(-12f, z0, -8f, z1), false, -1f);
                Street.Sidewalk(town, marks, a, $"Sidewalk_EastW_{z0:0}", Rect.MinMaxRect(64f, z0, 68f, z1), false, 1f);
            }
            Street.Sidewalk(town, marks, a, "Sidewalk_DriveW", Rect.MinMaxRect(35f, South, 39f, -44f), false, 1f);
            Street.Sidewalk(town, marks, a, "Sidewalk_DriveE", Rect.MinMaxRect(49f, South, 53f, -44f), false, -1f);
            // Kerbs where the hospital's own ground meets the streets; the ambulance approach is a driveway.
            Site.Kerb(marks, new Vector3(-12.12f, 0f, -28f), new Vector3(-12.12f, 0f, 60f), a);
            Site.Kerb(marks, new Vector3(68.12f, 0f, -28f), new Vector3(68.12f, 0f, 60f), a);
            Site.Kerb(marks, new Vector3(-12f, 0f, 60.12f), new Vector3(68f, 0f, 60.12f), a);
            Site.Kerb(marks, new Vector3(-12f, 0f, -28.12f), new Vector3(37f, 0f, -28.12f), a);
            Site.Kerb(marks, new Vector3(51f, 0f, -28.12f), new Vector3(68f, 0f, -28.12f), a);
            foreach (float x in new[] { -18f, 74f })
            foreach (float z in new[] { -34f, 66f })
            {
                Street.Crosswalk(marks, a, new Vector3(x - 8.5f, 0f, z), true, 12f);
                Street.Crosswalk(marks, a, new Vector3(x + 8.5f, 0f, z), true, 12f);
                Street.Crosswalk(marks, a, new Vector3(x, 0f, z - 8.5f), false, 12f);
                Street.Crosswalk(marks, a, new Vector3(x, 0f, z + 8.5f), false, 12f);
            }
            Street.Crosswalk(marks, a, new Vector3(44f, 0f, -47.5f), false, 10f);
        }

        private static void Lots(Transform town, StationArt a)
        {
            Plan.Ground(town, "Lot_SW", Rect.MinMaxRect(West, South, -28f, -44f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(town, "Lot_SMidW", Rect.MinMaxRect(-8f, South, 35f, -44f), a.Asphalt, SurfaceMaterial.Asphalt);
            Plan.Ground(town, "Lot_SMidE", Rect.MinMaxRect(53f, South, 64f, -44f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_SE", Rect.MinMaxRect(84f, South, East, -44f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_W", Rect.MinMaxRect(West, -28f, -28f, 60f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_E", Rect.MinMaxRect(84f, -28f, East, 60f), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_NW", Rect.MinMaxRect(West, 76f, -28f, North), a.Gravel, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_N", Rect.MinMaxRect(-8f, 76f, 64f, North), a.Dirt, SurfaceMaterial.Dirt);
            Plan.Ground(town, "Lot_NE", Rect.MinMaxRect(84f, 76f, East, North), a.Gravel, SurfaceMaterial.Dirt);
        }

        private static void Buildings(Transform b, StationArt a, StationTownArt t)
        {
            // South of the boulevard, facing the hospital: a parking deck and the pharmacy, a cafe by the drive, an office
            // tower, a gas station and a corner shop at the west.
            Plan.Row(b, a, new Vector3(-2f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.ParkingDeck, 26f, 34f, 3, 1f), new Lot(K.Shop, 9f, 16f, 2) }, 610);
            Plan.Row(b, a, new Vector3(53.5f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.Shop, 10f, 16f, 1) }, 620);
            Plan.Row(b, a, new Vector3(86f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.Office, 26f, 24f, 6, 2f), new Lot(K.Shop, 10f, 16f, 3) }, 630);
            Plan.Row(b, a, new Vector3(-62f, 0f, -44.3f), Vector3.right, 0f, new[] { new Lot(K.GasStation, 24f, 22f, 1, 2f), new Lot(K.Shop, 8f, 16f, 2) }, 640);
            // West of West Street, facing east: a medical office block, flats, the chapel, row houses.
            Plan.Row(b, a, new Vector3(-28.3f, 0f, -26f), Vector3.forward, 90f, new[]
            {
                new Lot(K.Office, 22f, 26f, 5, 2f), new Lot(K.Apartment, 18f, 22f, 5, 2f), new Lot(K.Church, 16f, 20f, 1, 2f),
                new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 2),
            }, 650);
            // East of East Street, facing west: the tallest office tower, a second parking deck, flats and a shop.
            Plan.Row(b, a, new Vector3(84.3f, 0f, -26f), Vector3.forward, -90f, new[]
            {
                new Lot(K.Office, 24f, 26f, 7, 6f), new Lot(K.ParkingDeck, 26f, 30f, 4, 2f), new Lot(K.Apartment, 16f, 22f, 4, 1f), new Lot(K.Shop, 10f, 16f, 2),
            }, 660);
            // North of the back street, facing south: flats, row houses and shops; a supply warehouse at the east corner.
            Plan.Row(b, a, new Vector3(-62f, 0f, 76.3f), Vector3.right, 180f, new[] { new Lot(K.Apartment, 30f, 24f, 4) }, 670);
            Plan.Row(b, a, new Vector3(-6f, 0f, 76.3f), Vector3.right, 180f, new[]
            {
                new Lot(K.Apartment, 20f, 20f, 5, 1f), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3), new Lot(K.Rowhouse, 7f, 14f, 3),
                new Lot(K.Rowhouse, 7f, 14f, 2, 1f), new Lot(K.Shop, 10f, 16f, 2), new Lot(K.Shop, 9f, 16f, 2),
            }, 680);
            Plan.Row(b, a, new Vector3(86f, 0f, 76.3f), Vector3.right, 180f, new[] { new Lot(K.Warehouse, 26f, 24f) }, 690);
        }

        private static void Furniture(Transform s, Transform marks, StationArt a)
        {
            // Lamps on the town's sides of every street (the hospital's ground keeps its own yard lights), and the drive.
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, -40.6f), Vector3.right, East - West - 6f, 32f, Vector3.forward * 6f, 9f);
            Plan.Lamps(s, a, new Vector3(-24.6f, 0f, South + 4f), Vector3.forward, North - South - 4f, 34f, Vector3.right * 6f);
            Plan.Lamps(s, a, new Vector3(80.6f, 0f, South + 4f), Vector3.forward, North - South - 4f, 34f, Vector3.left * 6f);
            Plan.Lamps(s, a, new Vector3(West + 6f, 0f, 72.6f), Vector3.right, East - West - 6f, 32f, Vector3.back * 6f);
            Plan.Lamps(s, a, new Vector3(38.6f, 0f, South), Vector3.forward, 54f, 22f, Vector3.right * 5f);
            var line = new List<Vector3>();
            foreach (float x in new[] { -54f, -34f, -4f, 26f, 58f, 92f, 114f }) line.Add(new Vector3(x, 0f, 74f));
            Street.PowerLine(s, a, line);
            var eastLine = new List<Vector3>();
            foreach (float z in new[] { -92f, -60f, -20f, 12f, 40f, 82f, 106f }) eastLine.Add(new Vector3(83f, 0f, z));
            Street.PowerLine(s, a, eastLine);
            // Signals at the boulevard's corners and where the drive meets it.
            Street.TrafficLight(s, a, new Vector3(-24.6f, 0f, -27.4f), 180f, 6f);
            Street.TrafficLight(s, a, new Vector3(-11.4f, 0f, -40.6f), 0f, 6f);
            Street.TrafficLight(s, a, new Vector3(80.6f, 0f, -27.4f), 180f, 6f);
            Street.TrafficLight(s, a, new Vector3(67.4f, 0f, -40.6f), 0f, 6f);
            Street.TrafficLight(s, a, new Vector3(49.6f, 0f, -40.8f), 0f, 6f);
            foreach (Vector3 p in new[] { new Vector3(-24.6f, 0f, 72.6f), new Vector3(80.6f, 0f, 72.6f), new Vector3(-11.4f, 0f, 76.6f), new Vector3(67.4f, 0f, 76.6f) })
                Street.StopSign(s, a, p, p.z < 74f ? 180f : 0f);
            foreach (Vector3 p in new[] { new Vector3(-25f, 0f, -42.5f), new Vector3(81f, 0f, -42.5f), new Vector3(36f, 0f, -42.5f), new Vector3(-25f, 0f, 74f) })
                Street.StreetSign(s, a, p, 0f);
            // The boulevard's south side: two bus shelters, the taxi rank, benches, news boxes, a mailbox.
            Street.BusShelter(s, a, new Vector3(4f, 0f, -42.4f), 0f);
            Street.BusShelter(s, a, new Vector3(100f, 0f, -42.4f), 0f);
            foreach (float x in new[] { 12f, 26f, 58f, 106f }) Street.Bench(s, a, new Vector3(x, 0f, -43.2f), 0f);
            Street.NewsBoxes(s, a, new Vector3(30f, 0f, -43.3f), 0f);
            Street.Mailbox(s, a, new Vector3(56f, 0f, -43.4f), 0f);
            foreach ((float x0, float x1) in new[] { (West, -24f), (-12f, 35f), (53f, 68f), (80f, East) })
                Plan.Kerbside(s, a, new Vector3(x0, 0f, -41f), Vector3.right, x1 - x0, 0f, 3 + Mathf.RoundToInt(x0));
            Plan.Kerbside(s, a, new Vector3(-27f, 0f, -28f), Vector3.forward, 88f, 90f, 5);
            Plan.Kerbside(s, a, new Vector3(81f, 0f, -28f), Vector3.forward, 88f, 270f, 7);
            Plan.Kerbside(s, a, new Vector3(-12f, 0f, 75f), Vector3.right, 80f, 180f, 9);
            // The taxi rank on the boulevard's hospital side: painted bays and the two cabs still in them.
            Material taxi = a.Plain("TaxiYellow", new Color(.85f, .62f, .08f), .55f, .3f);
            for (int i = 0; i < 4; i++)
                GreyboxFactory.Box("TaxiBay", marks, new Vector3(4f + i * 6f, .006f, -29.1f), new Vector3(.12f, .01f, 2.2f), a.Safety, false);
            StationVehicles.Car(s, "Taxi", new Vector3(7f, 0f, -29.6f), 90f, StationVehicleKind.Sedan, taxi, a, StationVehicleWear.Dusty, 3);
            StationVehicles.Car(s, "Taxi", new Vector3(13f, 0f, -29.6f), 90f, StationVehicleKind.Sedan, taxi, a, StationVehicleWear.Dusty | StationVehicleWear.FlatRear, 4);
            // Parked cars, never on the drive or in the hospital's approach.
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, -38.9f), Vector3.right, East - West - 4f, 211, new Vector2(36f, 50f), new Vector2(60f, 72f),
                new Vector2(99f, 113f), new Vector2(128f, 144f), new Vector2(156f, 168f));
            Plan.ParkedCars(s, a, new Vector3(West + 2f, 0f, 70.9f), Vector3.right, East - West - 4f, 213, new Vector2(36f, 50f), new Vector2(128f, 144f));
            Plan.ParkedCars(s, a, new Vector3(-13.1f, 0f, North - 2f), Vector3.back, North - South - 4f, 217, new Vector2(38f, 52f), new Vector2(138f, 152f));
            Plan.ParkedCars(s, a, new Vector3(78.9f, 0f, South + 2f), Vector3.forward, North - South - 4f, 219, new Vector2(56f, 72f), new Vector2(156f, 170f));
            // Ambulances left where they were abandoned: one in the back street, one nosed into the East Street kerb.
            Ambulance(s, a, new Vector3(30f, 0f, 66.5f), 90f);
            Ambulance(s, a, new Vector3(71.4f, 0f, 20f), 8f);
            // The quarantine checkpoint on the drive, its barriers dragged aside so the lane is open.
            Street.Barriers(s, new Vector3(36.6f, 0f, -52f), new Vector3(36.6f, 0f, -56f), 2);
            Street.Barriers(s, new Vector3(51.4f, 0f, -52f), new Vector3(51.4f, 0f, -56f), 2);
            Street.Sawhorse(s, a, new Vector3(50.2f, 0f, -58f), 70f);
            Transform booth = HospitalKit.Group("CheckpointBooth", s);
            booth.position = new Vector3(51.6f, 0f, -61f);
            GreyboxFactory.Box("Body", booth, new Vector3(0f, 1.25f, 0f), new Vector3(1.8f, 2.5f, 2.2f), a.White);
            GreyboxFactory.Box("Window", booth, new Vector3(-.91f, 1.5f, 0f), new Vector3(.02f, .9f, 1.6f), StationTownArt.For(a).Window, false);
            GreyboxFactory.Box("Roof", booth, new Vector3(0f, 2.58f, 0f), new Vector3(2.1f, .14f, 2.5f), a.DarkSteel, false);
            GreyboxFactory.Box("Cross", booth, new Vector3(0f, 1.9f, 1.11f), new Vector3(.5f, .5f, .02f), a.Red, false);
            Street.Dumpster(s, a, new Vector3(-30.5f, 0f, -70f), 90f, true);
            Street.Dumpster(s, a, new Vector3(120f, 0f, 30f), 90f, false);
            Street.Dumpster(s, a, new Vector3(30f, 0f, -96f), 0f, true);
            foreach (Vector3 p in new[] { new Vector3(28f, 0f, -90f), new Vector3(-60f, 0f, 30f), new Vector3(118f, 0f, -10f), new Vector3(40f, 0f, 108f) })
                Street.Litter(s, a, p, Mathf.RoundToInt(p.x + p.z));
        }

        // An ambulance: the box van in white with a red band, a light bar, solid.
        private static void Ambulance(Transform s, StationArt a, Vector3 at, float yaw)
        {
            Transform t = HospitalKit.Group("Ambulance", s);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            StationApparatus.BoxVan(t, a, a.White, a.Red, 5);
            GreyboxFactory.Box("LightBar", t, new Vector3(0f, 2.8f, 1.2f), new Vector3(1.4f, .1f, .3f), a.Rubber, false);
            for (int i = 0; i < 4; i++) GreyboxFactory.Box("Lens", t, new Vector3(-.5f + i * .33f, 2.88f, 1.2f), new Vector3(.3f, .08f, .26f), i % 2 == 0 ? a.Red : a.White, false);
            var solid = t.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.4f, 0f);
            solid.size = new Vector3(2.3f, 2.8f, 6.2f);
        }
    }
}
