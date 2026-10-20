using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using A = Abandoned.EditorTools.BurleySchoolArchitecture;

namespace Abandoned.EditorTools
{
    /// <summary>Traversable park bridge and fixed, recognisable winter vehicles.</summary>
    public static class BurleySchoolCampusProps
    {
        public static void Bridge(Transform parent, Vector3 pond, float length, BurleySchoolArt art)
        {
            Transform bridge = Group("WoodenPondBridge", parent);
            bridge.localPosition = new Vector3(pond.x, 0f, pond.z);
            const int segments = 32;
            float run = length / segments;
            for (int i = 0; i < segments; i++)
            {
                float a = i / (float)segments;
                float b = (i + 1) / (float)segments;
                float zA = (a - 0.5f) * length;
                float zB = (b - 0.5f) * length;
                float yA = BridgeHeight(a);
                float yB = BridgeHeight(b);
                float rise = yB - yA;
                float angle = -Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
                float slope = Mathf.Sqrt(run * run + rise * rise);
                Transform plank = Group($"DeckSegment_{i:00}", bridge);
                plank.localPosition = new Vector3(0f, (yA + yB) * 0.5f - 0.06f, (zA + zB) * 0.5f);
                plank.localRotation = Quaternion.Euler(angle, 0f, 0f);
                Box("Deck", plank, Vector3.zero, new Vector3(3.1f, 0.12f, slope + 0.015f), art.OakWood);
                Box("SnowDust", plank, new Vector3(-0.65f, 0.075f, 0f), new Vector3(1.35f, 0.025f, slope - 0.035f), art.Snow, false);
                foreach (float side in new[] { -1f, 1f })
                {
                    A.Rod(bridge, $"TopRail_{side}_{i}", new Vector3(side * 1.5f, yA + 1f, zA), new Vector3(side * 1.5f, yB + 1f, zB), 0.055f, art.OakWood, true);
                    A.Rod(bridge, $"MiddleRail_{side}_{i}", new Vector3(side * 1.5f, yA + 0.5f, zA), new Vector3(side * 1.5f, yB + 0.5f, zB), 0.04f, art.OakWood, true);
                    if (i % 2 == 0)
                        Box($"Post_{side}_{i}", bridge, new Vector3(side * 1.5f, yA + 0.51f, zA), new Vector3(0.12f, 1.12f, 0.12f), art.OakWood);
                    A.Rod(bridge, $"Baluster_{side}_{i}", new Vector3(side * 1.5f, (yA + yB) * 0.5f, (zA + zB) * 0.5f),
                        new Vector3(side * 1.5f, (yA + yB) * 0.5f + 1f, (zA + zB) * 0.5f), 0.025f, art.MetalSteel, true);
                }
            }
            foreach (float side in new[] { -1f, 1f })
            foreach (float end in new[] { -1f, 1f })
                Box($"EndPost_{side}_{end}", bridge, new Vector3(side * 1.5f, 0.55f, end * length * 0.5f), new Vector3(0.16f, 1.14f, 0.16f), art.OakWood);
            foreach (float z in new[] { -10f, 0f, 10f })
            foreach (float side in new[] { -1f, 1f })
            {
                float deck = BridgeHeight(z / length + 0.5f);
                Box($"Trestle_{side}_{z}", bridge, new Vector3(side * 1.28f, (deck - 0.3f) * 0.5f, z),
                    new Vector3(0.18f, deck + 0.3f, 0.2f), art.OakWood);
            }
        }

        private static float BridgeHeight(float t) => 0.04f + Mathf.Sin(t * Mathf.PI) * 0.82f;

        public static void Bus(Transform parent, string name, Vector3 at, float yaw, BurleySchoolArt art)
        {
            Transform bus = Group(name, parent);
            bus.localPosition = at;
            bus.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Material rubber = Rubber();
            Box("Chassis", bus, new Vector3(0f, 0.5f, 0.25f), new Vector3(2.38f, 0.25f, 10.5f), rubber);
            Box("LowerBody", bus, new Vector3(0f, 1.02f, 0f), new Vector3(2.6f, 1f, 9.5f), art.YellowStripe);
            Box("DarkCabin", bus, new Vector3(0f, 2.02f, -0.05f), new Vector3(2.42f, 1.15f, 9.25f), art.LabEpoxy);
            Box("Roof", bus, new Vector3(0f, 2.67f, 0f), new Vector3(2.65f, 0.22f, 9.6f), art.YellowStripe);
            Box("RoofSnow", bus, new Vector3(-0.12f, 2.83f, -0.1f), new Vector3(2.45f, 0.12f, 9.25f), art.Snow, false);
            Box("EngineHood", bus, new Vector3(0f, 1.11f, 5.55f), new Vector3(2.18f, 1.02f, 1.9f), art.YellowStripe);
            Box("HoodSnow", bus, new Vector3(-0.15f, 1.67f, 5.45f), new Vector3(1.8f, 0.1f, 1.65f), art.Snow, false);
            Box("FrontGrille", bus, new Vector3(0f, 1.03f, 6.52f), new Vector3(1.2f, 0.62f, 0.05f), art.MetalSteel, false);
            for (int slot = -4; slot <= 4; slot++)
                Box($"GrilleSlot_{slot}", bus, new Vector3(slot * 0.12f, 1.03f, 6.56f), new Vector3(0.045f, 0.5f, 0.02f), rubber, false);
            Box("FrontBumper", bus, new Vector3(0f, 0.6f, 6.61f), new Vector3(2.4f, 0.18f, 0.18f), art.MetalSteel);
            Box("RearBumper", bus, new Vector3(0f, 0.62f, -4.87f), new Vector3(2.65f, 0.18f, 0.2f), art.MetalSteel);
            foreach (float side in new[] { -1f, 1f })
            {
                Box($"Windshield_{side}", bus, new Vector3(side * 0.59f, 2.04f, 4.73f), new Vector3(1.1f, 0.91f, 0.06f), art.WindowGlass, false);
                Box($"WindshieldWiper_{side}", bus, new Vector3(side * 0.59f, 1.86f, 4.78f), new Vector3(0.55f, 0.04f, 0.025f), rubber, false);
                Box($"Headlight_{side}", bus, new Vector3(side * 0.84f, 1.2f, 6.56f), new Vector3(0.31f, 0.28f, 0.05f), art.WhiteStripe, false);
                Box($"RearLamp_{side}", bus, new Vector3(side * 1.06f, 1.34f, -4.78f), new Vector3(0.18f, 0.25f, 0.05f), art.TrackRubber, false);
                for (int pane = 0; pane < 9; pane++)
                {
                    float z = -4.08f + pane * 0.94f;
                    if (side > 0f && pane > 6) continue;
                    Box($"SideWindow_{side}_{pane}", bus, new Vector3(side * 1.316f, 2.04f, z), new Vector3(0.035f, 0.88f, 0.79f), art.WindowGlass, false);
                    Box($"WindowPillar_{side}_{pane}", bus, new Vector3(side * 1.31f, 2.04f, z - 0.445f), new Vector3(0.055f, 1.12f, 0.075f), art.YellowStripe, false);
                    Box($"WindowMeetingRail_{side}_{pane}", bus, new Vector3(side * 1.34f, 2.04f, z), new Vector3(0.025f, 0.035f, 0.79f), art.MetalSteel, false);
                }
                for (int rail = 0; rail < 3; rail++)
                    Box($"RubRail_{side}_{rail}", bus, new Vector3(side * 1.325f, 0.85f + rail * 0.22f, -0.1f), new Vector3(0.05f, 0.065f, 9.1f), rubber, false);
                Wheel(bus, $"FrontWheel_{side}", new Vector3(side * 1.27f, 0.45f, 4.1f), 0.56f, 0.34f, side, rubber, art);
                Wheel(bus, $"RearWheel_{side}", new Vector3(side * 1.27f, 0.45f, -2.75f), 0.56f, 0.42f, side, rubber, art);
                A.Rod(bus, $"MirrorArm_{side}", new Vector3(side * 1.3f, 2.2f, 4.55f), new Vector3(side * 1.67f, 2.22f, 4.86f), 0.022f, art.MetalSteel);
                Box($"Mirror_{side}", bus, new Vector3(side * 1.68f, 2.22f, 4.88f), new Vector3(0.16f, 0.28f, 0.06f), art.MetalSteel, false);
            }
            for (int leaf = 0; leaf < 2; leaf++)
            {
                float z = 3.05f + leaf * 0.65f;
                Box($"FoldingDoorFrame_{leaf}", bus, new Vector3(1.33f, 1.65f, z), new Vector3(0.07f, 1.9f, 0.6f), rubber, false);
                Box($"FoldingDoorGlass_{leaf}", bus, new Vector3(1.375f, 1.87f, z), new Vector3(0.025f, 1.12f, 0.45f), art.WindowGlass, false);
            }
            Box("EntranceStep", bus, new Vector3(1.37f, 0.57f, 3.38f), new Vector3(0.25f, 0.12f, 1.35f), art.MetalSteel);
            Box("SignBacking", bus, new Vector3(0f, 2.56f, 4.8f), new Vector3(2.2f, 0.24f, 0.03f), art.YellowStripe, false);
            Label(bus, "SchoolBusSign", "SCHOOL BUS", new Vector3(0f, 2.56f, 4.83f), Quaternion.Euler(0f, 180f, 0f), 0.11f, new Color(0.04f, 0.045f, 0.04f));
            Box("RearEmergencyDoor", bus, new Vector3(0f, 1.65f, -4.78f), new Vector3(1.14f, 1.72f, 0.04f), art.YellowStripe, false);
            Box("RearDoorGlass", bus, new Vector3(0f, 2.1f, -4.82f), new Vector3(0.9f, 0.66f, 0.025f), art.WindowGlass, false);
            Box("RearDoorLatch", bus, new Vector3(0.38f, 1.38f, -4.84f), new Vector3(0.12f, 0.05f, 0.025f), art.MetalSteel, false);
            Box("RustBelowDoor", bus, new Vector3(1.338f, 0.76f, 2.58f), new Vector3(0.015f, 0.22f, 0.6f), art.Rust, false);
        }

        public static void Sedan(Transform parent, string name, Vector3 at, float yaw, BurleySchoolArt art)
        {
            Transform car = Group(name, parent);
            car.localPosition = at;
            car.localRotation = Quaternion.Euler(0f, yaw, 0f);
            Material rubber = Rubber();
            Box("Underbody", car, new Vector3(0f, 0.35f, 0f), new Vector3(1.72f, 0.22f, 4.2f), rubber);
            Box("Body", car, new Vector3(0f, 0.65f, 0f), new Vector3(1.9f, 0.55f, 4.4f), art.LockerGreen);
            Box("DarkCabin", car, new Vector3(0f, 1.19f, -0.2f), new Vector3(1.58f, 0.58f, 2.4f), art.LabEpoxy);
            Box("Roof", car, new Vector3(0f, 1.52f, -0.2f), new Vector3(1.65f, 0.1f, 2.12f), art.LockerGreen);
            Box("RoofSnow", car, new Vector3(-0.1f, 1.62f, -0.24f), new Vector3(1.4f, 0.1f, 1.95f), art.Snow, false);
            Box("HoodSnow", car, new Vector3(-0.15f, 0.97f, 1.55f), new Vector3(1.48f, 0.1f, 0.94f), art.Snow, false);
            Box("TrunkSnow", car, new Vector3(0.1f, 0.96f, -1.76f), new Vector3(1.62f, 0.08f, 0.65f), art.Snow, false);
            GameObject front = Box("Windshield", car, new Vector3(0f, 1.22f, 1.025f), new Vector3(1.48f, 0.51f, 0.045f), art.WindowGlass, false);
            front.transform.localRotation = Quaternion.Euler(-22f, 0f, 0f);
            GameObject rear = Box("RearGlass", car, new Vector3(0f, 1.22f, -1.425f), new Vector3(1.48f, 0.51f, 0.045f), art.WindowGlass, false);
            rear.transform.localRotation = Quaternion.Euler(22f, 0f, 0f);
            foreach (float side in new[] { -1f, 1f })
            {
                for (int pane = 0; pane < 2; pane++)
                    Box($"SideGlass_{side}_{pane}", car, new Vector3(side * 0.805f, 1.23f, -0.76f + pane * 1.13f), new Vector3(0.035f, 0.42f, 1.02f), art.WindowGlass, false);
                Box($"DoorPillar_{side}", car, new Vector3(side * 0.82f, 1.23f, -0.2f), new Vector3(0.04f, 0.6f, 0.085f), art.LockerGreen, false);
                Box($"DoorHandle_{side}", car, new Vector3(side * 0.97f, 0.83f, 0.05f), new Vector3(0.04f, 0.05f, 0.18f), art.MetalSteel, false);
                Box($"Headlamp_{side}", car, new Vector3(side * 0.65f, 0.72f, 2.22f), new Vector3(0.44f, 0.2f, 0.04f), art.WhiteStripe, false);
                Box($"TailLamp_{side}", car, new Vector3(side * 0.68f, 0.72f, -2.22f), new Vector3(0.38f, 0.18f, 0.04f), art.TrackRubber, false);
                Wheel(car, $"FrontWheel_{side}", new Vector3(side * 0.93f, 0.32f, 1.42f), 0.35f, 0.22f, side, rubber, art);
                Wheel(car, $"RearWheel_{side}", new Vector3(side * 0.93f, 0.32f, -1.4f), 0.35f, 0.22f, side, rubber, art);
            }
            Box("FrontBumper", car, new Vector3(0f, 0.44f, 2.25f), new Vector3(1.84f, 0.12f, 0.12f), art.MetalSteel);
            Box("RearBumper", car, new Vector3(0f, 0.44f, -2.25f), new Vector3(1.84f, 0.12f, 0.12f), art.MetalSteel);
            Box("FrontGrille", car, new Vector3(0f, 0.7f, 2.23f), new Vector3(0.65f, 0.18f, 0.035f), rubber, false);
        }

        private static Material Rubber() => PolishAssets.Material("Burley_VehicleRubber", new Color(0.025f, 0.029f, 0.031f), roughness: 0.92f);

        private static void Wheel(Transform parent, string name, Vector3 at, float radius, float width, float side, Material rubber, BurleySchoolArt art)
        {
            Transform wheel = Group(name, parent);
            wheel.localPosition = at;
            GameObject tire = Primitive(PrimitiveType.Cylinder, "RoundTire", wheel, Vector3.zero, new Vector3(radius * 2f, width * 0.5f, radius * 2f), rubber);
            tire.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject hub = Primitive(PrimitiveType.Cylinder, "Hub", wheel, new Vector3(side * (width * 0.5f + 0.015f), 0f, 0f),
                new Vector3(radius * 1.02f, 0.025f, radius * 1.02f), art.MetalSteel, false);
            hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            for (int bolt = 0; bolt < 5; bolt++)
            {
                float angle = bolt * Mathf.PI * 0.4f;
                Box($"Lug_{bolt}", wheel, new Vector3(side * (width * 0.5f + 0.045f), Mathf.Sin(angle) * radius * 0.28f, Mathf.Cos(angle) * radius * 0.28f),
                    new Vector3(0.025f, 0.04f, 0.04f), art.BrickDark, false);
            }
        }
    }
}
