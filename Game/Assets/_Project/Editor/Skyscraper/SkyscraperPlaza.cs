using Abandoned.Core;
using Abandoned.Extraction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the rich, detailed streetscape and forecourt plaza surrounding Cloudline Tower:
    /// two-lane asphalt avenue with painted markings, curbed sidewalks, street lamps, abandoned vehicles
    /// (executive towncars, yellow cab, courier van), drained architectural fountain with brass sculpture,
    /// concrete planters with withered trees, plaza benches, corporate monument sign, and parking ramp.
    /// </summary>
    public static class SkyscraperPlaza
    {
        public static TruckCargo Build(Transform exterior, SkyscraperArt art)
        {
            Transform plazaGroup = Group("Plaza", exterior);

            // 1. Broadway Avenue — Two-Lane City Street (z: -34 to -22)
            Transform street = Group("Street", plazaGroup);
            Box("StreetAsphalt", street, new Vector3(0f, -0.2f, -28f), new Vector3(48f, 0.4f, 13f), art.Asphalt);

            // Double Yellow Centerline
            for (float x = -23f; x <= 23f; x += 4.0f)
            {
                Box($"CenterYellowL_{x:0}", street, new Vector3(x, 0.005f, -28.1f), new Vector3(2.5f, 0.01f, 0.12f), art.RoadYellow, false);
                Box($"CenterYellowR_{x:0}", street, new Vector3(x, 0.005f, -27.9f), new Vector3(2.5f, 0.01f, 0.12f), art.RoadYellow, false);
            }

            // White Pedestrian Crosswalk (West: x = -14)
            for (float cz = -33.5f; cz <= -22.5f; cz += 1.2f)
                Box($"Crosswalk_{cz:0.0}", street, new Vector3(-14f, 0.005f, cz), new Vector3(2.8f, 0.01f, 0.6f), art.RoadWhite, false);

            // Drainage Manhole Covers & Storm Sewer Grates
            Box("StormGrateL", street, new Vector3(-18f, 0.005f, -22.2f), new Vector3(1.2f, 0.01f, 0.6f), art.Steel, false);
            Box("StormGrateR", street, new Vector3( 18f, 0.005f, -22.2f), new Vector3(1.2f, 0.01f, 0.6f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Manhole", street, new Vector3(-6f, 0.005f, -26f), new Vector3(0.9f, 0.02f, 0.9f), art.Steel, false);

            // 2. Sidewalk & Curbstone Apron
            Transform sidewalk = Group("Sidewalk", plazaGroup);
            Box("CurbSlab", sidewalk, new Vector3(0f, 0.1f, -21.2f), new Vector3(48f, 0.2f, 1.6f), art.Concrete);

            // 3. Modern Street Lighting (4 Tall City Lamp Posts)
            Transform streetLamps = Group("StreetLamps", plazaGroup);
            BuildStreetLamp(streetLamps, art, new Vector3(-20f, 0f, -21.6f), 0f);
            BuildStreetLamp(streetLamps, art, new Vector3(-7f,  0f, -21.6f), 0f);
            BuildStreetLamp(streetLamps, art, new Vector3( 7f,  0f, -21.6f), 0f);
            BuildStreetLamp(streetLamps, art, new Vector3( 20f, 0f, -21.6f), 0f);

            // 4. Abandoned Vehicles on the Street
            Transform cars = Group("AbandonedVehicles", plazaGroup);
            BuildCar(cars, "ExecutiveTowncar", new Vector3(-8.5f, 0f, -26.5f), 12f, art.CarPaintBlack, art);
            BuildCar(cars, "YellowTaxiCab", new Vector3(9.5f, 0f, -25.2f), -28f, art.CarPaintYellow, art, true);
            BuildCourierVan(cars, art, new Vector3(-19f, 0f, -25.5f));

            // Road Barricades and Dumpster blocking East traffic
            BuildRoadBarricades(plazaGroup, art, new Vector3(18f, 0f, -28f));

            // 5. Cloudline Forecourt Plaza (Paved Pedestrian Square, z: -20 to -10)
            Transform plaza = Group("PavedPlaza", plazaGroup);
            Box("PlazaPavement", plaza, new Vector3(0f, 0.08f, -15.5f), new Vector3(40f, 0.2f, 11f), art.PlazaPaver);

            // Drained Tiered Fountain Pool & Abstract Modern Sculpture
            BuildFountain(plaza, art, new Vector3(0f, 0.18f, -15.5f));

            // Architectural Concrete Planters with Withered Trees
            BuildPlanter(plaza, art, new Vector3(-12f, 0.18f, -16f));
            BuildPlanter(plaza, art, new Vector3( 12f, 0.18f, -16f));
            BuildPlanter(plaza, art, new Vector3(-12f, 0.18f, -12f));
            BuildPlanter(plaza, art, new Vector3( 12f, 0.18f, -12f));

            // Teak-and-Steel Plaza Benches
            BuildBench(plaza, art, new Vector3(-7f, 0.18f, -18f), 0f);
            BuildBench(plaza, art, new Vector3( 7f, 0.18f, -18f), 0f);

            // Corporate Monument Sign ("CLOUDLINE TOWER / 1200 BROADWAY")
            BuildMonumentSign(plaza, art, new Vector3(-14f, 0.18f, -19.5f));

            // Chrome Flagpoles
            Transform flagpoles = Group("Flagpoles", plaza);
            for (float fx = 12f; fx <= 16f; fx += 2.0f)
            {
                Transform flag = Group($"Flagpole_{fx:0}", flagpoles);
                flag.position = new Vector3(fx, 0.18f, -19.5f);
                Primitive(PrimitiveType.Cylinder, "Pole", flag, new Vector3(0f, 4f, 0f), new Vector3(0.1f, 4f, 0.1f), art.Brass, false);
                Box("Banner", flag, new Vector3(0.5f, 7.2f, 0f), new Vector3(1.0f, 0.6f, 0.02f), art.CarPaintBurgundy, false);
            }

            // Underground Parking Garage Ramp & Guard Booth (West Flank: x = -16, z = -14)
            BuildParkingRamp(plazaGroup, art, new Vector3(-16f, 0f, -11f));

            // 6. Van Arrival & Extraction Parking Bay
            return BuildVanBay(exterior, art);
        }

        private static TruckCargo BuildVanBay(Transform exterior, SkyscraperArt art)
        {
            Transform bay = Group("VanBay", exterior);
            bay.position = new Vector3(L.TruckRear.x, 0.005f, L.TruckRear.z);
            bay.rotation = Quaternion.LookRotation(L.TruckForward, Vector3.up);

            const float w = 3.6f, len = 9.0f;
            Box("BayLineL", bay, new Vector3(-w * 0.5f, 0.18f, len * 0.5f), new Vector3(0.12f, 0.01f, len), art.RoadWhite, false);
            Box("BayLineR", bay, new Vector3( w * 0.5f, 0.18f, len * 0.5f), new Vector3(0.12f, 0.01f, len), art.RoadWhite, false);
            Box("BayLineFront", bay, new Vector3(0f, 0.18f, len), new Vector3(w, 0.01f, 0.12f), art.RoadWhite, false);

            // Loading hazard box behind rear doors
            Box("LoadBoxEdge", bay, new Vector3(0f, 0.18f, -4.5f), new Vector3(w, 0.01f, 0.14f), art.RoadYellow, false);
            for (int i = 0; i < 6; i++)
            {
                GameObject hatch = Box($"Hatch_{i}", bay, new Vector3(-1.4f + i * 0.55f, 0.18f, -2.25f),
                    new Vector3(0.12f, 0.01f, 3.8f), art.RoadYellow, false);
                hatch.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            }

            // Standalone test truck (open world hides Exterior/Truck)
            Transform truckGroup = Group("Truck", exterior);
            TruckCargo cargo = TruckBuilder.Build(truckGroup, L.TruckRear, L.TruckForward);

            // Empty marker for open world survey
            Transform marker = Group("VanParking", exterior);
            marker.SetPositionAndRotation(L.TruckRear, Quaternion.LookRotation(L.TruckForward, Vector3.up));

            return cargo;
        }

        private static Transform BuildCar(Transform parent, string name, Vector3 at, float yaw, Material body, SkyscraperArt art, bool taxiSign = false)
        {
            Transform car = Group(name, parent);
            car.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("Body", car, new Vector3(0f, 0.62f, 0f), new Vector3(1.85f, 0.62f, 4.8f), body, false);
            Box("Cabin", car, new Vector3(0f, 1.18f, -0.2f), new Vector3(1.6f, 0.52f, 2.4f), body, false);
            Box("Windscreen", car, new Vector3(0f, 1.18f, 0.98f), new Vector3(1.5f, 0.46f, 0.04f), art.Glass, false);
            Box("RearScreen", car, new Vector3(0f, 1.18f, -1.38f), new Vector3(1.5f, 0.42f, 0.04f), art.Glass, false);
            Box("SideGlassL", car, new Vector3(-0.81f, 1.18f, -0.2f), new Vector3(0.04f, 0.44f, 2.2f), art.Glass, false);
            Box("SideGlassR", car, new Vector3( 0.81f, 1.18f, -0.2f), new Vector3(0.04f, 0.44f, 2.2f), art.Glass, false);
            Box("BumperF", car, new Vector3(0f, 0.35f, 2.42f), new Vector3(1.86f, 0.25f, 0.12f), art.Steel, false);
            Box("BumperR", car, new Vector3(0f, 0.35f, -2.42f), new Vector3(1.86f, 0.25f, 0.12f), art.Steel, false);

            if (taxiSign)
                Box("TaxiLight", car, new Vector3(0f, 1.50f, 0f), new Vector3(0.5f, 0.15f, 0.2f), art.LampEmissive, false);

            foreach (float side in new[] { -1f, 1f })
            foreach (float z in new[] { -1.4f, 1.4f })
            {
                Primitive(PrimitiveType.Cylinder, "Wheel", car, new Vector3(side * 0.92f, 0.34f, z),
                    new Vector3(0.68f, 0.18f, 0.68f), art.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }

            var col = car.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.85f, 0f);
            col.size = new Vector3(1.95f, 1.4f, 4.9f);
            return car;
        }

        private static void BuildCourierVan(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform van = Group("CourierVan", parent);
            van.SetPositionAndRotation(at, Quaternion.Euler(0f, 15f, 0f));
            Box("Body", van, new Vector3(0f, 1.35f, -0.3f), new Vector3(2.1f, 2.1f, 4.8f), art.ColdStoreWall, false);
            Box("Cab", van, new Vector3(0f, 1.1f, 2.5f), new Vector3(2.05f, 1.5f, 1.2f), art.ColdStoreWall, false);
            Box("Windscreen", van, new Vector3(0f, 1.55f, 3.11f), new Vector3(1.9f, 0.6f, 0.04f), art.Glass, false);
            foreach (float s in new[] { -1f, 1f })
            foreach (float z in new[] { -1.6f, 2.0f })
            {
                Primitive(PrimitiveType.Cylinder, "Tyre", van, new Vector3(s * 1.05f, 0.4f, z),
                    new Vector3(0.78f, 0.2f, 0.78f), art.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            var col = van.gameObject.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.25f, 0f);
            col.size = new Vector3(2.2f, 2.5f, 6.0f);
        }

        private static void BuildStreetLamp(Transform parent, SkyscraperArt art, Vector3 at, float yaw)
        {
            Transform lamp = Group("StreetLamp", parent);
            lamp.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Primitive(PrimitiveType.Cylinder, "Pole", lamp, new Vector3(0f, 3.2f, 0f), new Vector3(0.18f, 3.2f, 0.18f), art.Steel, false);
            Box("Arm", lamp, new Vector3(0f, 6.2f, 0.8f), new Vector3(0.12f, 0.12f, 1.8f), art.Steel, false);
            Box("Head", lamp, new Vector3(0f, 6.1f, 1.6f), new Vector3(0.4f, 0.15f, 0.7f), art.Steel, false);
            Box("Bulb", lamp, new Vector3(0f, 6.0f, 1.6f), new Vector3(0.3f, 0.05f, 0.5f), art.LampEmissive, false);

            Transform aim = Group("LightSource", lamp);
            aim.position = at + new Vector3(0f, 5.9f, 1.6f);
            aim.rotation = Quaternion.Euler(75f, yaw, 0f);
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 18f;
            l.spotAngle = 85f;
            l.intensity = 8.5f;
            l.color = new Color(1.0f, 0.82f, 0.55f);
            l.shadows = LightShadows.None;
        }

        private static void BuildFountain(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform f = Group("DrainedFountainPool", parent);
            f.position = at;
            Box("BasinBottom", f, new Vector3(0f, 0.05f, 0f), new Vector3(10f, 0.2f, 7f), art.Concrete);
            Box("BasinRimN", f, new Vector3(0f, 0.35f,  3.4f), new Vector3(10f, 0.5f, 0.4f), art.GraniteDark);
            Box("BasinRimS", f, new Vector3(0f, 0.35f, -3.4f), new Vector3(10f, 0.5f, 0.4f), art.GraniteDark);
            Box("BasinRimE", f, new Vector3( 4.8f, 0.35f, 0f), new Vector3(0.4f, 0.5f, 6.8f), art.GraniteDark);
            Box("BasinRimW", f, new Vector3(-4.8f, 0.35f, 0f), new Vector3(0.4f, 0.5f, 6.8f), art.GraniteDark);
            Box("MurkyPuddle", f, new Vector3(0f, 0.16f, 0f), new Vector3(9.2f, 0.02f, 6.2f), art.WaterMurky, false);

            // Center abstract sculpture: "The Ascendant Spiral"
            Transform sculpture = Group("ModernSculpture", f);
            sculpture.position = new Vector3(0f, 0.2f, 0f);
            Primitive(PrimitiveType.Cylinder, "Pedestal", sculpture, new Vector3(0f, 0.5f, 0f), new Vector3(1.6f, 0.5f, 1.6f), art.GraniteDark);
            for (int s = 0; s < 5; s++)
            {
                float sy = 1.0f + s * 0.7f;
                float angle = s * 45f;
                GameObject blade = Box($"Blade_{s}", sculpture, new Vector3(0f, sy, 0f), new Vector3(0.18f, 0.8f, 1.4f), art.Brass, false);
                blade.transform.localRotation = Quaternion.Euler(0f, angle, 20f);
            }
        }

        private static void BuildPlanter(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform p = Group("PlazaPlanter", parent);
            p.position = at;
            Box("ConcreteTrough", p, new Vector3(0f, 0.45f, 0f), new Vector3(3.0f, 0.9f, 1.2f), art.Concrete);
            Box("Soil", p, new Vector3(0f, 0.82f, 0f), new Vector3(2.8f, 0.1f, 1.0f), art.Asphalt, false);
            Primitive(PrimitiveType.Cylinder, "Trunk", p, new Vector3(0f, 1.6f, 0f), new Vector3(0.15f, 1.5f, 0.15f), art.Mahogany, false);
            Box("Branch1", p, new Vector3(0.3f, 2.6f, 0f), new Vector3(0.8f, 0.08f, 0.08f), art.Mahogany, false)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            Box("Branch2", p, new Vector3(-0.3f, 2.8f, 0.1f), new Vector3(0.7f, 0.08f, 0.08f), art.Mahogany, false)
                .transform.localRotation = Quaternion.Euler(0f, 20f, -40f);
        }

        private static void BuildBench(Transform parent, SkyscraperArt art, Vector3 at, float yaw)
        {
            Transform bench = Group("PlazaBench", parent);
            bench.SetPositionAndRotation(at, Quaternion.Euler(0f, yaw, 0f));
            Box("LegL", bench, new Vector3(-0.9f, 0.25f, 0f), new Vector3(0.1f, 0.5f, 0.55f), art.Steel, false);
            Box("LegR", bench, new Vector3( 0.9f, 0.25f, 0f), new Vector3(0.1f, 0.5f, 0.55f), art.Steel, false);
            Box("SeatWood", bench, new Vector3(0f, 0.45f, 0f), new Vector3(2.0f, 0.06f, 0.5f), art.Parquet);
            Box("BackWood", bench, new Vector3(0f, 0.75f, -0.22f), new Vector3(2.0f, 0.4f, 0.06f), art.Parquet, false);
        }

        private static void BuildMonumentSign(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform sign = Group("CorporateMonumentSign", parent);
            sign.position = at;
            Box("GranitePlinth", sign, new Vector3(0f, 0.75f, 0f), new Vector3(3.8f, 1.5f, 0.7f), art.GraniteDark);
            Box("BronzePlaque", sign, new Vector3(0f, 0.85f, -0.36f), new Vector3(3.4f, 1.1f, 0.04f), art.Brass, false);
        }

        private static void BuildParkingRamp(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform ramp = Group("UndergroundParkingRamp", parent);
            ramp.position = at;
            Box("RetainingWallL", ramp, new Vector3(-2.8f, 0.6f, -3f), new Vector3(0.35f, 2.0f, 8f), art.Concrete);
            Box("RetainingWallR", ramp, new Vector3( 2.8f, 0.6f, -3f), new Vector3(0.35f, 2.0f, 8f), art.Concrete);
            GameObject slope = Box("RampSlope", ramp, new Vector3(0f, -0.6f, -3f), new Vector3(5.2f, 0.3f, 8.2f), art.Asphalt);
            slope.transform.localRotation = Quaternion.Euler(15f, 0f, 0f);

            Transform gate = Group("BarrierGate", ramp);
            gate.position = new Vector3(-2.4f, 0f, 0.5f);
            Primitive(PrimitiveType.Cylinder, "GatePost", gate, new Vector3(0f, 0.6f, 0f), new Vector3(0.25f, 0.6f, 0.25f), art.Steel);
            Box("BarrierArm", gate, new Vector3(2.2f, 0.9f, 0f), new Vector3(4.4f, 0.1f, 0.08f), art.RoadYellow, false);

            Transform booth = Group("GuardBooth", ramp);
            booth.position = new Vector3(-4.2f, 0f, 0.5f);
            Box("BoothBody", booth, new Vector3(0f, 1.25f, 0f), new Vector3(1.8f, 2.5f, 1.8f), art.Steel);
            Box("BoothGlass", booth, new Vector3(0.91f, 1.4f, 0f), new Vector3(0.04f, 1.0f, 1.4f), art.Glass, false);
            Box("BoothRoof", booth, new Vector3(0f, 2.55f, 0f), new Vector3(2.1f, 0.12f, 2.1f), art.Steel, false);
        }

        private static void BuildRoadBarricades(Transform parent, SkyscraperArt art, Vector3 at)
        {
            Transform bar = Group("RoadBarricadeCluster", parent);
            bar.position = at;
            Box("JerseyBarrier1", bar, new Vector3(-1.8f, 0.45f, 0f), new Vector3(3.2f, 0.9f, 0.6f), art.Concrete);
            Box("JerseyBarrier2", bar, new Vector3( 1.8f, 0.45f, 0.4f), new Vector3(3.2f, 0.9f, 0.6f), art.Concrete)
                .transform.localRotation = Quaternion.Euler(0f, 15f, 0f);

            Transform dumpster = Group("Dumpster", bar);
            dumpster.localPosition = new Vector3(-4.2f, 0f, 1.2f);
            Box("DumpsterBody", dumpster, new Vector3(0f, 0.8f, 0f), new Vector3(2.2f, 1.5f, 1.4f), art.Rust);
            Box("DumpsterLid", dumpster, new Vector3(0f, 1.58f, 0f), new Vector3(2.24f, 0.08f, 1.44f), art.Steel, false);
        }
    }
}
