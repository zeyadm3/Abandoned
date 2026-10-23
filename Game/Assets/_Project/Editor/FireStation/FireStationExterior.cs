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
            Site.Slab(ground, "Street", new Rect(-30f, -24f, 124f, 10f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "Verge", new Rect(-30f, -40f, 124f, 16f), a.Grass, SurfaceMaterial.Dirt);
            Site.Slab(ground, "TrainingYard", new Rect(-30f, 0f, 30f, 40f), a.Gravel, SurfaceMaterial.Dirt);
            Site.Slab(ground, "EastLot", new Rect(64f, 0f, 30f, 40f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "RearYard", new Rect(-30f, 40f, 124f, 20f), a.Asphalt, SurfaceMaterial.Asphalt);
            Transform ext = Site.Exterior(root);
            Markings(ext, a);
            Yards(ext, a);
            BayDoors(ext, a);
            Roof(ext, a);
            Transform lamps = Group("StreetLamps", ext);
            foreach (float x in new[] { -20f, 50f, 80f })
                StationLighting.StreetLamp(lamps, new Vector3(x, -.02f, -12.6f), new Vector3(x, 0f, -19f), a);
            StationLighting.StreetLamp(lamps, new Vector3(-10f, -.02f, -2f), new Vector3(6f, 0f, -6f), a, 11f);
            StationLighting.StreetLamp(lamps, new Vector3(40f, -.02f, -2f), new Vector3(24f, 0f, -6f), a, 11f);
            MarkStatic(ext);
            Site.VanBay(root, a, L.VanRear, L.VanForward);
        }

        private static void Markings(Transform ext, StationArt a)
        {
            Transform m = Group("Markings", ext);
            for (float x = -28f; x < 94f; x += 6f)
                Box("CentreDash", m, new Vector3(x, -.015f, -19f), new Vector3(3f, .01f, .14f), a.Safety, false);
            Site.Kerb(m, new Vector3(-30f, -.02f, -14f), new Vector3(94f, -.02f, -14f), a);
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
        }

        private static void Yards(Transform ext, StationArt a)
        {
            Transform yard = Group("Yards", ext);
            Material hydrant = a.Plain("Hydrant", new Color(.75f, .55f, .1f), .4f, .2f);
            foreach (Vector3 at in new[] { new Vector3(-4f, -.02f, -12.8f), new Vector3(42f, -.02f, -12.8f), new Vector3(-8f, -.02f, 22f) })
            {
                Transform h = Group("Hydrant", yard);
                h.position = at;
                Primitive(PrimitiveType.Cylinder, "Barrel", h, new Vector3(0f, .4f, 0f), new Vector3(.3f, .4f, .3f), hydrant);
                Primitive(PrimitiveType.Sphere, "Cap", h, new Vector3(0f, .82f, 0f), new Vector3(.32f, .18f, .32f), hydrant, false);
                Primitive(PrimitiveType.Cylinder, "Nozzle", h, new Vector3(.18f, .5f, 0f), new Vector3(.12f, .08f, .12f), hydrant, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            // The extrication car: roof cut off for the drill.
            Transform wreck = Site.Car(yard, "ExtricationCar", new Vector3(-14f, -.02f, 12f), 25f, a.Plain("WreckBlue", new Color(.15f, .2f, .3f), .15f), a);
            Transform cabin = wreck.Find("Cabin");
            if (cabin != null) Object.DestroyImmediate(cabin.gameObject);
            // The smoke-training container: a shipping box, its doors open, soot round them.
            Transform box = Group("SmokeContainer", yard);
            box.SetPositionAndRotation(new Vector3(-18f, -.02f, 30f), Quaternion.Euler(0f, 90f, 0f));
            Box("Body", box, new Vector3(0f, 1.3f, 0f), new Vector3(2.44f, 2.6f, 12.2f), a.Plain("ContainerRust", new Color(.42f, .2f, .12f), .2f, .3f));
            Box("Soot", box, new Vector3(0f, 1.6f, 6.11f), new Vector3(2.2f, 1.8f, .02f), a.Soot, false);
            foreach (float s in new[] { -1f, 1f })
            {
                GameObject door = Box("Door", box, new Vector3(s * 1.9f, 1.3f, 6.6f), new Vector3(1.2f, 2.5f, .06f), a.Plain("ContainerRust", new Color(.42f, .2f, .12f), .2f, .3f), false);
                door.transform.localRotation = Quaternion.Euler(0f, s * 70f, 0f);
            }
            Transform dryer = Group("HoseDryingRack", yard);
            dryer.position = new Vector3(-20f, -.02f, 6f);
            foreach (float x in new[] { -3f, 3f }) Primitive(PrimitiveType.Cylinder, "Post", dryer, new Vector3(x, 1.5f, 0f), new Vector3(.12f, 1.5f, .12f), a.Steel);
            Box("Bar", dryer, new Vector3(0f, 2.95f, 0f), new Vector3(6.2f, .08f, .08f), a.Steel, false);
            Material canvas = a.Plain("HoseCanvas", new Color(.62f, .56f, .4f), .1f);
            for (int i = 0; i < 5; i++) Box("Hose", dryer, new Vector3(-2.4f + i * 1.2f, 1.75f, 0f), new Vector3(.1f, 2.4f, .2f), canvas, false);
            Transform pump = Group("FuelPump", yard);
            pump.position = new Vector3(44f, -.02f, 48f);
            Box("Island", pump, new Vector3(0f, .1f, 0f), new Vector3(1.4f, .2f, 3f), a.Concrete);
            Box("Pump", pump, new Vector3(0f, 1f, 0f), new Vector3(.7f, 1.6f, .5f), a.Red);
            Box("Dumpster", yard, new Vector3(62f, .7f, 48f), new Vector3(3.6f, 1.4f, 1.8f), a.Plain("DumpsterGreen", new Color(.12f, .25f, .14f), .3f));
            Site.Fence(yard, "RearFence", new Vector3(-30f, -.02f, 59f), new Vector3(94f, -.02f, 59f), a);
            Transform flag = Group("Flagpole", yard);
            flag.position = new Vector3(70f, -.02f, -5f);
            Primitive(PrimitiveType.Cylinder, "Pole", flag, new Vector3(0f, 5f, 0f), new Vector3(.12f, 5f, .12f), a.Chrome);
            Box("Flag", flag, new Vector3(.75f, 9.2f, 0f), new Vector3(1.4f, .9f, .02f), a.Red, false);
            Box("Base", flag, new Vector3(0f, .2f, 0f), new Vector3(1.2f, .4f, 1.2f), a.Trim);
            Site.Car(yard, "CrewCar", new Vector3(74f, -.02f, 30f), 0f, a.Plain("SedanGrey", new Color(.35f, .36f, .37f), .3f), a);
            Site.Car(yard, "CrewCar", new Vector3(82f, -.02f, 30f), 0f, a.Plain("SedanRust", new Color(.42f, .3f, .22f), .2f), a);
            Site.Car(yard, "ChiefCar", new Vector3(78f, -.02f, 9f), 180f, a.Red, a, true);
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
