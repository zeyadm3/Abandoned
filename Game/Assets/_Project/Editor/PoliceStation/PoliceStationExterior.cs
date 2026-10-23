using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.PoliceStationLayout;
using Site = Abandoned.EditorTools.StationSite;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The precinct's lot (x -30..86, z -30..60): the street and sidewalk to the south, the patrol car park to the west,
    /// the fenced impound yard behind, and the east yard where the crew's van backs up to the sally port. The lobby's
    /// canopy and its two blue lamps, street lamps, and on the roof the radio mast and the plant. No truck of its own and
    /// no trees, sky or grass (the world and Art > Upgrade New Maps add those).
    /// </summary>
    public static class PoliceStationExterior
    {
        public static void Build(StationContext ctx)
        {
            Transform root = ctx.Root;
            StationArt a = ctx.Art;
            Transform ground = Site.GroundSlabs(root);
            Site.Slab(ground, "Sidewalk", new Rect(-30f, -6f, 116f, 6f), a.Sidewalk, SurfaceMaterial.Concrete);
            Site.Slab(ground, "Street", new Rect(-30f, -16f, 116f, 10f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "Verge", new Rect(-30f, -30f, 116f, 14f), a.Grass, SurfaceMaterial.Dirt);
            Site.Slab(ground, "CarPark", new Rect(-30f, 0f, 30f, 40f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "EastYard", new Rect(56f, 0f, 30f, 40f), a.Asphalt, SurfaceMaterial.Asphalt);
            Site.Slab(ground, "ImpoundYard", new Rect(-30f, 40f, 116f, 20f), a.Gravel, SurfaceMaterial.Dirt);
            Transform ext = Site.Exterior(root);
            Markings(ext, a);
            Cars(ext, a);
            Impound(ext, a);
            Entrance(ext, a);
            Roof(ext, a);
            Transform lamps = Group("StreetLamps", ext);
            foreach (float x in new[] { -20f, 10f, 40f, 70f })
                StationLighting.StreetLamp(lamps, new Vector3(x, -.02f, -5.4f), new Vector3(x, 0f, -11f), a);
            StationLighting.StreetLamp(lamps, new Vector3(84f, -.02f, 22f), new Vector3(70f, 0f, 14f), a, 11f);
            StationLighting.StreetLamp(lamps, new Vector3(-28f, -.02f, 20f), new Vector3(-12f, 0f, 20f), a, 8f);
            MarkStatic(ext);
            Site.VanBay(root, a, L.VanRear, L.VanForward);
        }

        private static void Markings(Transform ext, StationArt a)
        {
            Transform m = Group("Markings", ext);
            for (float x = -28f; x < 86f; x += 6f)
                Box("CentreDash", m, new Vector3(x, -.015f, -11f), new Vector3(3f, .01f, .14f), a.Safety, false);
            Site.Kerb(m, new Vector3(-30f, -.02f, -6f), new Vector3(86f, -.02f, -6f), a);
            for (int i = 0; i <= 6; i++)
                Box("BayLine", m, new Vector3(-27f + i * 4f, -.015f, 9f), new Vector3(.12f, .01f, 5.2f), a.Paint, false);
            for (int i = 0; i <= 6; i++)
                Box("BayLine", m, new Vector3(-27f + i * 4f, -.015f, 31f), new Vector3(.12f, .01f, 5.2f), a.Paint, false);
            // The sally port's painted approach on the east yard, and the stop line.
            Box("StopLine", m, new Vector3(57.4f, -.015f, L.SallyDoor.z), new Vector3(.25f, .01f, L.SallyDoorWidth), a.Paint, false);
            foreach (float z in new[] { L.SallyDoor.z - 3.4f, L.SallyDoor.z + 3.4f })
                Box("LaneEdge", m, new Vector3(64f, -.015f, z), new Vector3(14f, .01f, .14f), a.Safety, false);
        }

        // The patrol car park: cruisers left where they were parked, one burnt out, one with its doors hanging open, two
        // that ran into each other on the way out, the evidence van; one more nosed into the kerb out in the street.
        private static void Cars(Transform ext, StationArt a)
        {
            Transform cars = Group("Cars", ext);
            Material patrol = a.Plain("PatrolBlack", new Color(.06f, .065f, .08f), .55f, .3f);
            Site.Car(cars, "PatrolCar", new Vector3(-25f, -.02f, 9f), 180f, patrol, a, true);
            Flat(Site.Car(cars, "BurntPatrolCar", new Vector3(-21f, -.02f, 9f), 180f, a.Soot, a, true), -3f);
            Transform open = Site.Car(cars, "PatrolCar", new Vector3(-13f, -.02f, 9f), 180f, patrol, a, true);
            foreach (float s in new[] { -1f, 1f })
            {
                GameObject door = Box("OpenDoor", open, new Vector3(s * 1.5f, .8f, .6f), new Vector3(.06f, .9f, 1.1f), patrol, false);
                door.transform.localRotation = Quaternion.Euler(0f, s * 55f, 0f);
            }
            Site.Car(cars, "PatrolCar", new Vector3(-17f, -.02f, 31f), 0f, patrol, a, true);
            Flat(Site.Car(cars, "PatrolCar", new Vector3(-9f, -.02f, 31f), 0f, patrol, a, true), 2.5f);
            Site.Car(cars, "Sedan", new Vector3(-5f, -.02f, 31f), 0f, a.Plain("SedanRust", new Color(.42f, .3f, .22f), .2f), a);
            Transform hit = Site.Car(cars, "PatrolCar", new Vector3(-12f, -.02f, 19.5f), 70f, patrol, a, true);
            hit.localRotation = Quaternion.Euler(0f, 70f, 1.5f);
            Site.Car(cars, "PatrolCar", new Vector3(-8.2f, -.02f, 22.4f), 160f, patrol, a, true);
            EvidenceVan(cars, a, new Vector3(-25f, -.02f, 31f));
            Transform crashed = Site.Car(cars, "PatrolCar", new Vector3(26f, -.02f, -9.5f), 72f, patrol, a, true);
            crashed.localRotation = Quaternion.Euler(0f, 72f, 2.5f);
        }

        // A flat tyre: the car sits down on one side.
        private static void Flat(Transform car, float roll) => car.localRotation = Quaternion.Euler(0f, car.localEulerAngles.y, roll);

        private static void EvidenceVan(Transform parent, StationArt a, Vector3 at)
        {
            Transform van = Group("EvidenceVan", parent);
            van.position = at;
            Box("Body", van, new Vector3(0f, 1.35f, -.3f), new Vector3(2.1f, 2.1f, 4.6f), a.White, false);
            Box("Cab", van, new Vector3(0f, 1.1f, 2.4f), new Vector3(2.05f, 1.5f, 1f), a.White, false);
            Box("Stripe", van, new Vector3(0f, 1.2f, 0f), new Vector3(2.12f, .25f, 5.6f), a.Accent, false);
            Box("Windscreen", van, new Vector3(0f, 1.55f, 2.91f), new Vector3(1.9f, .6f, .03f), a.Glass, false);
            foreach (float s in new[] { -1f, 1f })
            foreach (float z in new[] { -1.7f, 2f })
                Primitive(PrimitiveType.Cylinder, "Tyre", van, new Vector3(s * .95f, .38f, z), new Vector3(.76f, .14f, .76f), a.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var solid = van.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.25f, 0f);
            solid.size = new Vector3(2.15f, 2.5f, 5.8f);
        }

        // The impound yard behind the station: fenced, its gate in the east fence left open, the guard's booth, a tow truck
        // and rows of seized cars (one craned up onto another).
        private static void Impound(Transform ext, StationArt a)
        {
            Transform yard = Group("Impound", ext);
            Site.Fence(yard, "Fence_W", new Vector3(-2f, -.02f, 41f), new Vector3(-2f, -.02f, 58f), a);
            Site.Fence(yard, "Fence_N", new Vector3(-2f, -.02f, 58f), new Vector3(58f, -.02f, 58f), a);
            Site.Fence(yard, "Fence_E", new Vector3(58f, -.02f, 58f), new Vector3(58f, -.02f, 49f), a);
            Transform gate = Group("Gate", yard);
            gate.SetPositionAndRotation(new Vector3(58f, -.02f, 49f), Quaternion.Euler(0f, 120f, 0f));
            Box("Frame", gate, new Vector3(0f, 1.1f, 3.5f), new Vector3(.08f, 2.2f, 7f), a.Steel);
            Box("Mesh", gate, new Vector3(0f, 1.1f, 3.5f), new Vector3(.02f, 2f, 6.9f), a.Grating, false);
            Transform booth = Group("GuardBooth", yard);
            booth.position = new Vector3(61.5f, -.02f, 44f);
            Box("Body", booth, new Vector3(0f, 1.25f, 0f), new Vector3(2f, 2.5f, 2f), a.White);
            Box("Window", booth, new Vector3(-1.01f, 1.5f, 0f), new Vector3(.02f, .9f, 1.6f), a.Glass, false);
            Box("Roof", booth, new Vector3(0f, 2.6f, 0f), new Vector3(2.3f, .15f, 2.3f), a.DarkSteel, false);
            Box("Barrier", booth, new Vector3(-1.2f, 1f, -3f), new Vector3(.1f, .1f, 4f), a.Red, false);
            Material[] bodies =
            {
                a.Plain("WreckBlue", new Color(.15f, .2f, .3f), .15f), a.Plain("WreckGreen", new Color(.2f, .26f, .18f), .15f),
                a.Plain("WreckWhite", new Color(.6f, .6f, .56f), .2f), a.Plain("WreckRed", new Color(.4f, .1f, .08f), .2f),
            };
            for (int i = 0; i < 6; i++)
                Site.Car(yard, "Impounded", new Vector3(6f + i * 8f, -.02f, 50f + (i % 2) * 2.5f), 15f * (i % 3) - 15f, bodies[i % bodies.Length], a);
            for (int i = 0; i < 5; i++)
                Site.Car(yard, "Impounded", new Vector3(4f + i * 6f, -.02f, 44f), 90f + (i % 2) * 8f, bodies[(i + 2) % bodies.Length], a);
            Transform stacked = Site.Car(yard, "StackedWreck", new Vector3(6.2f, 1.48f, 50.1f), -10f, bodies[3], a);
            stacked.localRotation = Quaternion.Euler(4f, -10f, -3f);
            TowTruck(yard, a, new Vector3(36f, -.02f, 55.5f));
            for (int i = 0; i < 4; i++)
            {
                GameObject tyres = Primitive(PrimitiveType.Cylinder, "TyreStack", yard, new Vector3(52f + (i % 2) * 1.1f, .4f, 44f + i * 1.3f),
                    new Vector3(.8f, .4f, .8f), a.Rubber);
                tyres.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Plastic);
            }
        }

        private static void TowTruck(Transform parent, StationArt a, Vector3 at)
        {
            Transform t = Group("TowTruck", parent);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, 90f, 0f));
            Material yellow = a.Plain("TowYellow", new Color(.8f, .6f, .1f), .4f, .2f);
            Box("Cab", t, new Vector3(0f, 1.4f, 2.2f), new Vector3(2.3f, 2f, 2.2f), yellow, false);
            Box("Bed", t, new Vector3(0f, .9f, -1.2f), new Vector3(2.3f, .7f, 4.6f), a.DarkSteel, false);
            Box("Boom", t, new Vector3(0f, 2.1f, -2.4f), new Vector3(.25f, .25f, 2.8f), yellow, false).transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            Box("Hook", t, new Vector3(0f, 1.9f, -3.8f), new Vector3(.05f, 1.2f, .05f), a.Steel, false);
            foreach (float s in new[] { -1f, 1f })
            foreach (float z in new[] { 2.2f, -1.6f, -2.8f })
                Primitive(PrimitiveType.Cylinder, "Tyre", t, new Vector3(s * 1.05f, .5f, z), new Vector3(1f, .16f, 1f), a.Rubber, false)
                    .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            var solid = t.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0f, 1.2f, -.3f);
            solid.size = new Vector3(2.4f, 2.4f, 7.4f);
        }

        // The lobby's canopy over the street doors, its posts and the two blue lamps; the sally port's door drum and lamp.
        private static void Entrance(Transform ext, StationArt a)
        {
            Transform e = Group("Entrance", ext);
            Box("Canopy", e, new Vector3(L.LobbyDoorX, 3.55f, -1.4f), new Vector3(6.4f, .22f, 2.8f), a.Trim);
            Box("CanopyFascia", e, new Vector3(L.LobbyDoorX, 3.4f, -2.75f), new Vector3(6.4f, .4f, .1f), a.AccentDark, false);
            foreach (float dx in new[] { -2.9f, 2.9f })
            {
                Primitive(PrimitiveType.Cylinder, "Post", e, new Vector3(L.LobbyDoorX + dx, 1.72f, -2.5f), new Vector3(.18f, 1.72f, .18f), a.Trim);
                Primitive(PrimitiveType.Cylinder, "LampPost", e, new Vector3(L.LobbyDoorX + dx * 1.35f, 1f, -.6f), new Vector3(.1f, 1f, .1f), a.DarkSteel);
                Primitive(PrimitiveType.Sphere, "BlueLamp", e, new Vector3(L.LobbyDoorX + dx * 1.35f, 2.2f, -.6f), Vector3.one * .4f,
                    a.Plain("BlueLamp", new Color(.2f, .35f, .9f), .7f, 0f, new Color(.3f, .6f, 2.2f)), false);
            }
            Box("DoorMat", e, new Vector3(L.LobbyDoorX, -.012f, -.8f), new Vector3(3f, .01f, 1.2f), a.Rubber, false);
            Transform flag = Group("Flagpole", e);
            flag.position = new Vector3(6f, -.02f, -3.2f);
            Primitive(PrimitiveType.Cylinder, "Pole", flag, new Vector3(0f, 5f, 0f), new Vector3(.12f, 5f, .12f), a.Chrome);
            Box("Flag", flag, new Vector3(.75f, 9.2f, 0f), new Vector3(1.4f, .9f, .02f), a.Accent, false);
            Box("Base", flag, new Vector3(0f, .15f, 0f), new Vector3(.8f, .3f, .8f), a.Trim);
            // The sally port's roller door, wound up into its drum over the opening.
            Box("RollerDrum", e, new Vector3(L.SallyDoor.x + .45f, L.SallyDoorHeight + .35f, L.SallyDoor.z), new Vector3(.55f, .6f, L.SallyDoorWidth + .4f), a.DarkSteel, false);
            foreach (float dz in new[] { -1f, 1f })
                Box("Guide", e, new Vector3(L.SallyDoor.x + .25f, L.SallyDoorHeight * .5f, L.SallyDoor.z + dz * (L.SallyDoorWidth * .5f + .08f)),
                    new Vector3(.12f, L.SallyDoorHeight, .12f), a.Safety, false);
            Transform lamp = Group("SallyLamp", e);
            lamp.position = new Vector3(L.SallyDoor.x + .5f, 4.6f, L.SallyDoor.z);
            Box("Housing", lamp, Vector3.zero, new Vector3(.4f, .25f, .6f), a.DarkSteel, false);
            Transform aim = Group("Light", lamp);
            aim.rotation = Quaternion.LookRotation(new Vector3(.6f, -1f, 0f));
            Light l = aim.gameObject.AddComponent<Light>();
            l.type = LightType.Spot;
            l.range = 16f;
            l.spotAngle = 95f;
            l.intensity = 7f;
            l.color = new Color(1f, .78f, .5f);
            l.shadows = LightShadows.None;
        }

        // The roof: the radio mast, air handlers and a vent stack.
        private static void Roof(Transform ext, StationArt a)
        {
            Transform roof = Group("RoofPlant", ext);
            float y = L.Plan.RoofY + StationPlan.Slab;
            Transform mast = Group("RadioMast", roof);
            mast.position = new Vector3(50f, y, 34f);
            foreach (float x in new[] { -.6f, .6f })
            foreach (float z in new[] { -.6f, .6f })
                Box("Leg", mast, new Vector3(x * (1f - .25f), 6f, z * (1f - .25f)), new Vector3(.08f, 12f, .08f), a.Steel, false);
            for (int i = 1; i < 12; i++)
                Box("Brace", mast, new Vector3(0f, i, 0f), new Vector3(1f, .05f, 1f), a.Steel, false);
            Box("Antenna", mast, new Vector3(0f, 14f, 0f), new Vector3(.05f, 4f, .05f), a.Steel, false);
            Primitive(PrimitiveType.Sphere, "Beacon", mast, new Vector3(0f, 16.1f, 0f), Vector3.one * .25f, a.WarningRed, false);
            foreach (Vector3 at in new[] { new Vector3(20f, y, 20f), new Vector3(32f, y, 26f), new Vector3(12f, y, 8f) })
            {
                Transform unit = Group("AirHandler", roof);
                unit.position = at;
                Box("Body", unit, new Vector3(0f, .8f, 0f), new Vector3(2.6f, 1.6f, 1.6f), a.Steel);
                Primitive(PrimitiveType.Cylinder, "Fan", unit, new Vector3(0f, 1.62f, 0f), new Vector3(1.1f, .03f, 1.1f), a.DarkSteel, false);
            }
            Primitive(PrimitiveType.Cylinder, "VentStack", roof, new Vector3(4f, y + 1.2f, 30f), new Vector3(.5f, 1.2f, .5f), a.DarkSteel, false);
        }
    }
}
