using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.FireStationLayout;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The firehouse's lot (x -30..94, z -40..60, ground top at -0.02): the concrete apron in front of the four bay doors
    /// with the van's bay at the haul lane, the street beyond, lawns with the memorial flagpole, the training yard to the
    /// west (an extrication car, a smoke-training container, the hose drying rack), the crew car park to the east and the
    /// rear yard with the fuel pump. On the building: the bay doors' roller drums, the apron lamp, the roof siren and plant.
    /// No truck (the crew's van is the truck) and no trees, sky or grass (the world and Art > Upgrade New Maps add those).
    /// </summary>
    public static class FireStationExterior
    {
        public static void Build(StationContext ctx)
        {
            Transform root = ctx.Root;
            StationArt a = ctx.Art;
            Transform ground = Site.GroundSlabs(root);
            Site.Slab(ground, "Apron", new Rect(-6f, -14f, 44f, 14f), a.Sidewalk, SurfaceMaterial.Concrete);
            Site.Slab(ground, "LawnWest", new Rect(-30f, -10f, 24f, 10f), a.Grass, SurfaceMaterial.Dirt);
            Site.Slab(ground, "LawnEast", new Rect(38f, -10f, 56f, 10f), a.Grass, SurfaceMaterial.Dirt);
            Site.Slab(ground, "SidewalkWest", new Rect(-30f, -14f, 24f, 4f), a.Sidewalk, SurfaceMaterial.Concrete);
            Site.Slab(ground, "SidewalkEast", new Rect(38f, -14f, 56f, 4f), a.Sidewalk, SurfaceMaterial.Concrete);
            Site.Slab(ground, "TrainingYard", new Rect(-30f, 0f, 30f, 40f), a.Gravel, SurfaceMaterial.Dirt);
            Site.Slab(ground, "EastLot", new Rect(64f, 0f, 30f, 40f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "RearYard", new Rect(-30f, 40f, 124f, 20f), a.Asphalt, SurfaceMaterial.Asphalt);
            Transform ext = Site.Exterior(root);
            FireStationTown.Build(root, ext, a);
            Markings(ext, a);
            Yards(ext, a);
            BayDoors(ext, a);
            Roof(ext, a);
            Transform lamps = Group("StreetLamps", ext);
            foreach (float x in new[] { -20f, 50f, 80f })
                StationStreet.Lamp(lamps, a, new Vector3(x, -.02f, -12.6f), new Vector3(x, 0f, -19f));
            StationStreet.Lamp(lamps, a, new Vector3(-10f, -.02f, -2f), new Vector3(6f, 0f, -6f), 11f);
            StationStreet.Lamp(lamps, a, new Vector3(40f, -.02f, -2f), new Vector3(24f, 0f, -6f), 11f);
            StationStreet.Lamp(lamps, a, new Vector3(92f, -.02f, 38f), new Vector3(80f, 0f, 30f), 8f);
            StationStreet.Lamp(lamps, a, new Vector3(-28f, -.02f, 58f), new Vector3(-20f, 0f, 50f), 8f);
            MarkStatic(ext);
            Site.TrimOccluders(ext);
            Site.VanBay(root, a, L.VanRear, L.VanForward);
        }

        private static void Markings(Transform ext, StationArt a)
        {
            Transform m = Group("Markings", ext);
            foreach (float x in L.BayDoors)
            {
                Box("DoorLine", m, new Vector3(x, -.015f, -.6f), new Vector3(L.BayDoorWidth, .01f, .15f), a.Red, false);
                // The haul lane's apron carries the van bay's markings instead of the keep-clear hatching.
                if (Mathf.Abs(x - L.HaulDoor.x) < .1f) continue;
                for (int i = 0; i < 4; i++)
                {
                    GameObject hatch = Box("Hatch", m, new Vector3(x - 1.5f + i, -.015f, -4.5f), new Vector3(.15f, .01f, 5f), a.Safety, false);
                    hatch.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
                }
            }
            for (int i = 0; i <= 5; i++)
                Box("BayLine", m, new Vector3(72f + i * 4f, -.015f, 30f), new Vector3(.12f, .01f, 5.2f), a.Paint, false);
            // Kerbs along the station's own sidewalks; the apron is a driveway, so none across it.
            Site.Kerb(m, new Vector3(-30f, -.02f, -13.88f), new Vector3(-6f, -.02f, -13.88f), a);
            Site.Kerb(m, new Vector3(38f, -.02f, -13.88f), new Vector3(94f, -.02f, -13.88f), a);
            // The apron's tyre tracks and oil, the big dark patches where engines idled.
            foreach (float x in L.BayDoors)
                foreach (float s in new[] { -.9f, .9f })
                    Box("Track", m, new Vector3(x + s, -.013f, -6f), new Vector3(.35f, .01f, 11f), a.Grime, false);
            foreach (float x in new[] { 4f, 20f, 28f })
                Box("Oil", m, new Vector3(x, -.012f, -2.5f), new Vector3(2.4f, .01f, 1.8f), a.Oil, false);
        }

        private static void Yards(Transform ext, StationArt a)
        {
            Transform yard = Group("Yards", ext);
            StationVehicleArt v = StationVehicleArt.For(a);
            foreach (Vector3 at in new[] { new Vector3(-4f, -.02f, -12.8f), new Vector3(42f, -.02f, -12.8f), new Vector3(-8f, -.02f, 22f), new Vector3(66f, -.02f, 50f) })
                StationStreet.Hydrant(yard, a, at, 0f);
            // The rescue-drill yard: a car with its roof cut off, the smoke container, hose drying, pallets, burn barrels.
            StationVehicles.Car(yard, "ExtricationCar", new Vector3(-14f, -.02f, 12f), 25f, StationVehicleKind.Sedan, v.Paint(1), a,
                StationVehicleWear.RoofCut | StationVehicleWear.Smashed | StationVehicleWear.FlatFront, 3);
            // A car on its roof for the cutting drills: turned over about its length, lifted by its own height.
            Transform rolled = StationVehicles.Car(yard, "RolloverCar", new Vector3(-22f, -.02f, 18f), 140f, StationVehicleKind.Hatchback, v.Paint(6), a,
                StationVehicleWear.Smashed, 4);
            rolled.localRotation = Quaternion.Euler(0f, 140f, 178f);
            rolled.localPosition += Vector3.up * 1.47f;
            Transform box = Group("SmokeContainer", yard);
            box.SetPositionAndRotation(new Vector3(-18f, -.02f, 30f), Quaternion.Euler(0f, 90f, 0f));
            Material rust = StationTownArt.For(a).Container;
            Box("Body", box, new Vector3(0f, 1.3f, 0f), new Vector3(2.44f, 2.6f, 12.2f), rust);
            for (float z = -5.8f; z < 6f; z += .35f)
                foreach (float x in new[] { -1.23f, 1.23f }) Box("Rib", box, new Vector3(x, 1.3f, z), new Vector3(.03f, 2.5f, .12f), rust, false);
            Box("Soot", box, new Vector3(0f, 1.6f, 6.11f), new Vector3(2.2f, 1.8f, .02f), a.Soot, false);
            foreach (float s in new[] { -1f, 1f })
            {
                GameObject door = Box("Door", box, new Vector3(s * 1.9f, 1.3f, 6.6f), new Vector3(1.2f, 2.5f, .06f), rust, false);
                door.transform.localRotation = Quaternion.Euler(0f, s * 70f, 0f);
            }
            StationScans.Put(yard, "ladder_sectioned_01", new Vector3(-16.6f, -.02f, 30f), 90f, new Vector3(1f, 3.2f, 1f), solid: false);
            Transform dryer = Group("HoseDryingRack", yard);
            dryer.position = new Vector3(-20f, -.02f, 6f);
            foreach (float x in new[] { -3f, 3f }) Primitive(PrimitiveType.Cylinder, "Post", dryer, new Vector3(x, 1.5f, 0f), new Vector3(.12f, 1.5f, .12f), a.Steel);
            Box("Bar", dryer, new Vector3(0f, 2.95f, 0f), new Vector3(6.2f, .08f, .08f), a.Steel, false);
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            for (int i = 0; i < 5; i++) Box("Hose", dryer, new Vector3(-2.4f + i * 1.2f, 1.75f, 0f), new Vector3(.1f, 2.4f, .2f), canvas, false);
            foreach (Vector3 p in new[] { new Vector3(-26f, -.02f, 24f), new Vector3(-26.8f, -.02f, 25.4f) })
                StationScans.Put(yard, "barrel_stove", p, p.x * 20f, new Vector3(.7f, 1f, .7f), solid: true);
            for (int i = 0; i < 3; i++) StationScans.Put(yard, "wooden_crate_02", new Vector3(-8f, -.02f + i * .02f, 34f + i * 1.1f), i * 12f, new Vector3(.9f, .9f, .9f), solid: true);
            StationScans.Put(yard, "portable_searchlight", new Vector3(-6f, -.02f, 8f), 210f, new Vector3(1f, 2f, 1f), solid: true);
            for (int i = 0; i < 6; i++) StationStreet.Bollard(yard, a, new Vector3(-28f + i * 1.6f, -.02f, 1.2f));
            // The rear yard: the fuel island, a retired pumper under a tarp-grey coat of dust, the rescue van, the dumpster.
            Transform pump = Group("FuelPump", yard);
            pump.position = new Vector3(44f, -.02f, 48f);
            Box("Island", pump, new Vector3(0f, .1f, 0f), new Vector3(1.4f, .2f, 3f), a.Concrete);
            Box("Pump", pump, new Vector3(0f, 1f, 0f), new Vector3(.7f, 1.6f, .5f), a.Red);
            Box("Display", pump, new Vector3(0f, 1.4f, .26f), new Vector3(.5f, .4f, .01f), StationTownArt.For(a).Void, false);
            Box("Hose", pump, new Vector3(.36f, 1f, 0f), new Vector3(.04f, .9f, .04f), a.Rubber, false);
            foreach (float z in new[] { -1.7f, 1.7f }) StationStreet.Bollard(yard, a, new Vector3(44f, -.02f, 48f + z));
            Transform retired = Group("RetiredEngine", yard);
            retired.SetPositionAndRotation(new Vector3(20f, -.02f, 50f), Quaternion.Euler(0f, 90f, 0f));
            StationApparatus.Pumper(retired, a, a.Plain("FadedRed", new Color(.42f, .1f, .08f), .25f, .1f), 9);
            var engineSolid = retired.gameObject.AddComponent<BoxCollider>();
            engineSolid.center = new Vector3(0f, 1.6f, 0f);
            engineSolid.size = new Vector3(2.5f, 3.2f, 9.8f);
            Transform van = Group("RescueVan", yard);
            van.SetPositionAndRotation(new Vector3(6f, -.02f, 50f), Quaternion.Euler(0f, 90f, 0f));
            StationApparatus.BoxVan(van, a, a.White, a.Red, 2);
            var vanSolid = van.gameObject.AddComponent<BoxCollider>();
            vanSolid.center = new Vector3(0f, 1.4f, 0f);
            vanSolid.size = new Vector3(2.3f, 2.8f, 6.2f);
            StationStreet.Dumpster(yard, a, new Vector3(62f, -.02f, 48f), 0f, true);
            StationScans.Put(yard, "utility_box_02", new Vector3(58f, -.02f, 56f), 180f, new Vector3(1.4f, 1.8f, 1f), solid: true);
            StationScans.Put(yard, "small_lpg_tank", new Vector3(54f, -.02f, 56.5f), 0f, new Vector3(.7f, 1.3f, .7f), solid: true);
            Site.Fence(yard, "RearFence", new Vector3(-30f, -.02f, 59f), new Vector3(94f, -.02f, 59f), a);
            // The memorial by the flagpole: the old station bell on a brick plinth, a brass plaque, two benches.
            Transform flag = Group("Flagpole", yard);
            flag.position = new Vector3(70f, -.02f, -5f);
            Primitive(PrimitiveType.Cylinder, "Pole", flag, new Vector3(0f, 5f, 0f), new Vector3(.12f, 5f, .12f), a.Chrome);
            Box("Flag", flag, new Vector3(.75f, 9.2f, 0f), new Vector3(1.4f, .9f, .02f), a.Red, false);
            Box("Base", flag, new Vector3(0f, .2f, 0f), new Vector3(1.2f, .4f, 1.2f), a.Trim);
            Transform memorial = Group("Memorial", yard);
            memorial.position = new Vector3(62f, -.02f, -5f);
            Box("Plinth", memorial, new Vector3(0f, .6f, 0f), new Vector3(1.6f, 1.2f, 1.6f), a.Brick);
            Box("Cap", memorial, new Vector3(0f, 1.25f, 0f), new Vector3(1.8f, .1f, 1.8f), a.Trim, false);
            Box("Plaque", memorial, new Vector3(0f, .7f, -.81f), new Vector3(1f, .6f, .02f), a.Brass, false);
            Box("Yoke", memorial, new Vector3(0f, 2.3f, 0f), new Vector3(1.3f, .12f, .2f), a.DarkWood, false);
            foreach (float x in new[] { -.6f, .6f }) Box("Upright", memorial, new Vector3(x, 1.8f, 0f), new Vector3(.12f, 1f, .12f), a.DarkWood, false);
            StationMeshes.Draw(memorial, "Bell", StationMeshes.Lathe(a, "MemorialBell", new[]
            {
                new Vector2(.45f, 0f), new Vector2(.42f, .06f), new Vector2(.32f, .25f), new Vector2(.27f, .55f), new Vector2(.22f, .7f), new Vector2(0f, .74f),
            }, 24), new Vector3(0f, 1.48f, 0f), Vector3.zero, Vector3.one, a.Brass);
            StationStreet.Bench(yard, a, new Vector3(58f, -.02f, -7.5f), 180f);
            StationStreet.Bench(yard, a, new Vector3(66f, -.02f, -7.5f), 180f);
            // The crew's cars in the east lot, the chief's car by the watch office.
            StationVehicles.Car(yard, "CrewCar", new Vector3(74f, -.02f, 30f), 0f, StationVehicleKind.Pickup, v.Paint(2), a, StationVehicleWear.Dusty, 6);
            StationVehicles.Car(yard, "CrewCar", new Vector3(82f, -.02f, 30f), 0f, StationVehicleKind.Sedan, v.Paint(7), a, StationVehicleWear.Dusty | StationVehicleWear.FlatRear, 7);
            Site.CoveredCar(yard, new Vector3(86f, -.02f, 30f), 0f, a);
            StationVehicles.Car(yard, "CrewCar", new Vector3(90f, -.02f, 30f), 0f, StationVehicleKind.Suv, v.Paint(4), a, StationVehicleWear.Smashed, 8);
            Site.Car(yard, "ChiefCar", new Vector3(78f, -.02f, 9f), 180f, a.Red, a, true, StationVehicleWear.Dusty);
            StationScans.Put(yard, "security_light", new Vector3(64.2f, 4.6f, 20f), 90f, new Vector3(.6f, .6f, .6f));
            StationScans.Put(yard, "security_camera_01", new Vector3(64.2f, 5.6f, .2f), 135f, new Vector3(.5f, .4f, .5f));
        }

        // Each bay door's roller wound up into its drum, red-painted guides, and a lamp over the haul lane's door.
        private static void BayDoors(Transform ext, StationArt a)
        {
            Transform doors = Group("BayDoors", ext);
            foreach (float x in L.BayDoors)
            {
                Box("Drum", doors, new Vector3(x, L.BayDoorHeight + .35f, -.45f), new Vector3(L.BayDoorWidth + .4f, .6f, .55f), a.AccentDark, false);
                foreach (float s in new[] { -1f, 1f })
                    Box("Guide", doors, new Vector3(x + s * (L.BayDoorWidth * .5f + .08f), L.BayDoorHeight * .5f, -.25f), new Vector3(.12f, L.BayDoorHeight, .12f), a.Red, false);
            }
            Transform lamp = Group("ApronLamp", doors);
            lamp.position = new Vector3(L.HaulDoor.x, L.BayDoorHeight + 1.1f, -.5f);
            Box("Housing", lamp, Vector3.zero, new Vector3(.6f, .25f, .4f), a.DarkSteel, false);
            Transform aim = Group("Light", lamp);
            aim.rotation = Quaternion.LookRotation(new Vector3(0f, -1f, -.8f));
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 18f;
            l.spotAngle = 100f;
            l.intensity = 8f;
            l.color = new Color(1f, .8f, .55f);
            l.shadows = LightShadows.None;
        }

        // The quarters' roof: the siren on its pole, air handlers; skylights over the bay.
        private static void Roof(Transform ext, StationArt a)
        {
            Transform roof = Group("RoofPlant", ext);
            float y = L.Plan.RoofY + StationPlan.Slab;
            Transform siren = Group("Siren", roof);
            siren.position = new Vector3(50f, y, 20f);
            Primitive(PrimitiveType.Cylinder, "Pole", siren, new Vector3(0f, 2f, 0f), new Vector3(.15f, 2f, .15f), a.DarkSteel);
            Primitive(PrimitiveType.Cylinder, "Horn", siren, new Vector3(0f, 4.3f, 0f), new Vector3(.9f, .3f, .9f), a.Red, false);
            Primitive(PrimitiveType.Cylinder, "Rotor", siren, new Vector3(0f, 4.75f, 0f), new Vector3(.5f, .15f, .5f), a.Chrome, false);
            foreach (Vector3 at in new[] { new Vector3(42f, y, 8f), new Vector3(58f, y, 30f), new Vector3(20f, y, 22f) })
            {
                Transform unit = Group("AirHandler", roof);
                unit.position = at;
                Box("Body", unit, new Vector3(0f, .8f, 0f), new Vector3(2.4f, 1.6f, 1.6f), a.Steel);
                Primitive(PrimitiveType.Cylinder, "Fan", unit, new Vector3(0f, 1.62f, 0f), new Vector3(1f, .03f, 1f), a.DarkSteel, false);
            }
            for (int i = 0; i < 4; i++)
                Box("Skylight", roof, new Vector3(6f + i * 7f, y + .15f, 8f), new Vector3(2.4f, .3f, 4f), a.Glass, false);
        }
    }
}
