using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Fixed industrial fittings for the basement: readable machine faces, a continuous steam service, shower
    /// drainage and boiler instrumentation. Small fittings sit on their solid parent machines and never acquire
    /// bounding-box collision of their own. Dressing stays off the east-west haul route through both doorways.
    /// </summary>
    public static class IslandBasementDetails
    {
        private const float Y = L.BasementY;

        public static void Build(Transform parent, IslandPrisonArt art)
        {
            Transform detail = Visuals("AttachedFittings", parent);
            Services(detail, art);
            RoomEdges(detail, art);
            Showers(detail, art);
            Boiler(detail, art);
            Laundry(detail, art);
        }

        private static Transform Visuals(string name, Transform parent)
        {
            Transform g = Group(name, parent);
            g.gameObject.AddComponent<WalkThroughObject>().EditorSetup("Small fixed fittings sit on solid machinery and walls; thin service pipes stay above the haul route.");
            return g;
        }

        // Cylinder's native long axis is Y; positioning between endpoints avoids floating pipe joints.
        private static GameObject Pipe(string name, Transform g, Vector3 a, Vector3 b, float diameter, Material material)
        {
            Vector3 delta = b - a;
            GameObject pipe = Primitive(PrimitiveType.Cylinder, name, g, (a + b) * .5f,
                new Vector3(diameter, delta.magnitude * .5f, diameter), material, false);
            pipe.transform.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
            return pipe;
        }

        private static void FaceDisc(string name, Transform g, Vector3 at, float diameter, float depth, Material material)
        {
            Primitive(PrimitiveType.Cylinder, name, g, at, new Vector3(diameter, depth * .5f, diameter), material, false)
                .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void Ring(string name, Transform g, Vector3 at, float radius, float width, Material material, int sides = 12)
        {
            float segment = 2f * radius * Mathf.Sin(Mathf.PI / sides) + width * .25f;
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                K.RotBox(name, g, at + new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * radius,
                    new Vector3(segment, width, width), Quaternion.Euler(0f, 0f, -i * 360f / sides), material, false);
            }
        }

        public static void Washer(Transform parent, IslandPrisonArt art, float x, float front, int index)
        {
            Transform g = Visuals($"WasherFittings_{index}", parent);
            Vector3 hatch = new(x, Y + .82f, front);
            Ring("HatchRim", g, hatch, .5f, .09f, art.Steel);
            FaceDisc("DrumWindow", g, hatch + Vector3.back * .014f, .79f, .018f, art.Glass);
            Box("HatchHinge", g, hatch + new Vector3(-.47f, 0f, -.04f), new Vector3(.13f, .22f, .13f), art.Bars, false);
            Box("HatchLatch", g, hatch + new Vector3(.47f, .01f, -.08f), new Vector3(.12f, .24f, .1f), art.Brass, false);
            Box("ControlPlate", g, new Vector3(x, Y + 1.42f, front + .025f), new Vector3(1.7f, .22f, .04f), art.Dado, false);
            FaceDisc("TimerDial", g, new Vector3(x - .55f, Y + 1.42f, front - .016f), .16f, .05f, art.Brass);
            Box("DialNeedle", g, new Vector3(x - .55f, Y + 1.45f, front - .047f), new Vector3(.016f, .06f, .012f), art.Bars, false);
            for (int i = 0; i < 3; i++)
                FaceDisc("Button", g, new Vector3(x + .27f + i * .2f, Y + 1.42f, front - .012f), .065f, .035f, i == 0 ? art.HullTrim : art.Bars);
            for (int i = 0; i < 5; i++)
                Box("VentSlot", g, new Vector3(x - .32f + i * .16f, Y + .16f, front + .025f), new Vector3(.075f, .065f, .02f), art.Bars, false);
            Pipe("SupplyDrop", g, new Vector3(x + .65f, Y + 1.65f, 91.08f), new Vector3(x + .65f, Y + 3.18f, 91.08f), .075f, art.Rust);
            FaceDisc("SupplyUnion", g, new Vector3(x + .65f, Y + 2.3f, 91.03f), .14f, .08f, art.Brass);
        }

        private static void Services(Transform g, IslandPrisonArt art)
        {
            // Steam, water and return are distinct in diameter and finish, with all supports against the north wall.
            foreach ((float y, float z, float d, Material material) in new[]
            {
                (3.18f, 91.08f, .15f, art.Rust), (3f, 90.86f, .11f, art.Steel), (2.81f, 91.18f, .07f, art.Brass),
            })
            {
                Pipe("ServiceMain", g, new Vector3(34.2f, Y + y, z), new Vector3(76.8f, Y + y, z), d, material);
                for (float x = 35f; x < 77f; x += 4f)
                {
                    Box("PipeAnchor", g, new Vector3(x, Y + y, 91.42f), new Vector3(.16f, .2f, .04f), art.Bars, false);
                    Box("PipeBracket", g, new Vector3(x, Y + y - d * .6f, (z + 91.42f) * .5f), new Vector3(.045f, .045f, 91.42f - z + .13f), art.Steel, false);
                    Pipe("MainCollar", g, new Vector3(x, Y + y, z), new Vector3(x + .09f, Y + y, z), d * 1.5f, art.Bars);
                }
            }
            foreach (float x in L.BasementPartitions)
            {
                // Services cross above the doorway lintel; the opening itself remains 2.4 m clear.
                Box("LintelTrim", g, new Vector3(x, Y + 2.65f, L.BasementDoorZ), new Vector3(.38f, .1f, 2.56f), art.Steel, false);
                foreach (float z in new[] { L.BasementDoorZ - 1.24f, L.BasementDoorZ + 1.24f })
                    Box("DoorJamb", g, new Vector3(x, Y + 1.3f, z), new Vector3(.32f, 2.6f, .065f), art.Railing, false);
            }
        }

        private static void RoomEdges(Transform g, IslandPrisonArt art)
        {
            float south = L.NorthBand.yMin + .23f, north = L.Cellhouse.yMax - .53f;
            foreach ((float x0, float x1) in new[] { (30.54f, 49.84f), (50.17f, 65.83f), (66.17f, 77.46f) })
            {
                foreach (float z in new[] { south, north })
                {
                    Box("Skirting", g, new Vector3((x0 + x1) * .5f, Y + .14f, z), new Vector3(x1 - x0, .26f, .045f), art.FacadeDark, false);
                    Box("DadoCap", g, new Vector3((x0 + x1) * .5f, Y + 2.42f, z), new Vector3(x1 - x0, .06f, .055f), art.Dado, false);
                }
            }
            foreach (float x in new[] { 50.17f, 65.83f })
            foreach ((float a, float b) in new[] { (south, L.BasementDoorZ - 1.24f), (L.BasementDoorZ + 1.24f, north) })
            {
                Box("TileEdgeCap", g, new Vector3(x, Y + 2.42f, (a + b) * .5f), new Vector3(.065f, .06f, b - a), art.Dado, false);
                Box("TileSkirting", g, new Vector3(x, Y + .13f, (a + b) * .5f), new Vector3(.04f, .18f, b - a), art.FacadeDark, false);
            }
        }

        private static void Showers(Transform g, IslandPrisonArt art)
        {
            for (float x = 51.35f; x < 64.75f; x += .22f)
                Box("DrainGrate", g, new Vector3(x, Y + .071f, 86.985f), new Vector3(.045f, .018f, .25f), art.Steel, false);
            for (float x = 52.65f; x < 63.85f; x += 1.4f)
            {
                Pipe("ShowerRiser", g, new Vector3(x, Y + .95f, 88.45f), new Vector3(x, Y + 2.63f, 88.45f), .055f, art.Steel);
                FaceDisc("ShowerValve", g, new Vector3(x, Y + 1.15f, 88.38f), .12f, .09f, art.Brass);
                Box("ValveCross", g, new Vector3(x, Y + 1.15f, 88.31f), new Vector3(.17f, .025f, .025f), art.Steel, false);
                Pipe("ShowerNeck", g, new Vector3(x, Y + 2.22f, 88.45f), new Vector3(x, Y + 2.22f, 88.27f), .045f, art.Steel);
                K.Patch("MineralDeposit", g, new Vector3(x, Y + .065f, 88.25f), new Vector2(.7f, 1.2f), x * 17f, art.Stain);
            }
            foreach (float x in new[] { 54.3f, 58.5f, 62.7f })
            {
                Box("PrivacyScreenFoot", g, new Vector3(x, Y + .11f, 90.22f), new Vector3(.12f, .18f, 2.35f), art.FacadeDark, false);
                Box("PrivacyScreen", g, new Vector3(x, Y + 1.13f, 90.22f), new Vector3(.065f, 1.92f, 2.35f), art.Dado, true);
                Box("ScreenCap", g, new Vector3(x, Y + 2.12f, 90.22f), new Vector3(.09f, .055f, 2.4f), art.Steel, false);
            }
            for (float x = 55.5f; x < 64f; x += 1.2f)
            {
                Box("HookBackplate", g, new Vector3(x, Y + 1.7f, 80.29f), new Vector3(.12f, .18f, .045f), art.Steel, false);
                Box("CoatHook", g, new Vector3(x, Y + 1.69f, 80.37f), new Vector3(.035f, .06f, .14f), art.Brass, false);
            }
        }

        private static void Boiler(Transform g, IslandPrisonArt art)
        {
            // Bands and end plate make the cylinder read as a pressure vessel, rather than a bare primitive.
            foreach (float x in new[] { 72.1f, 73.1f, 74.8f })
                Pipe("BoilerBand", g, new Vector3(x, Y + 1.7f, 88.2f), new Vector3(x + .09f, Y + 1.7f, 88.2f), 2.26f, art.Bars);
            for (int i = 0; i < 10; i++)
            {
                float a = i * Mathf.PI * .2f;
                Pipe("EndPlateRivet", g, new Vector3(71.57f, Y + 1.7f + Mathf.Sin(a) * .87f, 88.2f + Mathf.Cos(a) * .87f),
                    new Vector3(71.53f, Y + 1.7f + Mathf.Sin(a) * .87f, 88.2f + Mathf.Cos(a) * .87f), .065f, art.Brass);
            }
            Box("FireboxLip", g, new Vector3(71.2f, Y + .38f, 87.49f), new Vector3(.6f, .09f, .22f), art.Bars, false);
            foreach (float x in new[] { 72.6f, 74.3f })
            {
                Pipe("GaugeStem", g, new Vector3(x, Y + 2.66f, 88.1f), new Vector3(x, Y + 2.98f, 88.1f), .065f, art.Brass);
                FaceDisc("GaugeCase", g, new Vector3(x, Y + 3.02f, 88.05f), .28f, .08f, art.Brass);
                FaceDisc("GaugeFace", g, new Vector3(x, Y + 3.02f, 88f), .23f, .015f, art.Porcelain);
                K.RotBox("GaugeNeedle", g, new Vector3(x + .02f, Y + 3.04f, 87.986f), new Vector3(.016f, .14f, .008f), Quaternion.Euler(0f, 0f, -35f), art.Bars, false);
            }
            Pipe("SteamFeed", g, new Vector3(73.3f, Y + 2.7f, 88.4f), new Vector3(73.3f, Y + 3.18f, 88.4f), .13f, art.Rust);
            Pipe("SteamReturn", g, new Vector3(73.3f, Y + 3.18f, 88.4f), new Vector3(73.3f, Y + 3.18f, 91.08f), .13f, art.Rust);
            Ring("IsolationWheel", g, new Vector3(73.3f, Y + 3.02f, 88.17f), .22f, .035f, art.HullTrim);
            Box("WheelSpoke", g, new Vector3(73.3f, Y + 3.02f, 88.17f), new Vector3(.44f, .025f, .025f), art.HullTrim, false);
            foreach (float x in new[] { 68.5f, 70f })
            {
                foreach (float y in new[] { .5f, 1.9f })
                    Pipe("TankBand", g, new Vector3(x, Y + y - .035f, 90.4f), new Vector3(x, Y + y + .035f, 90.4f), 1.23f, art.Steel);
                Pipe("TankTakeoff", g, new Vector3(x, Y + 2.4f, 90.4f), new Vector3(x, Y + 3.18f, 90.4f), .09f, art.Rust);
                Pipe("TankElbow", g, new Vector3(x, Y + 3.18f, 90.4f), new Vector3(x, Y + 3.18f, 91.08f), .09f, art.Rust);
            }
            Box("BenchBackboard", g, new Vector3(77.43f, Y + 1.65f, 82.8f), new Vector3(.045f, 1.15f, 2.5f), art.Dado, false);
            for (int i = 0; i < 5; i++)
                Box("FixedPartsBin", g, new Vector3(76.98f, Y + .33f, 81.75f + i * .48f), new Vector3(.65f, .18f, .38f), art.Steel, false);
        }

        private static void Laundry(Transform g, IslandPrisonArt art)
        {
            // The failed machine and press are fixed scenery; separate loose tools would resemble collectible loot.
            Box("MachineRearPanel", g, new Vector3(48.28f, Y + 1.02f, 87.9f), new Vector3(.8f, 1.15f, .045f), art.Bars, false);
            Ring("MangleFlywheel", g, new Vector3(48.6f, Y + 1.15f, 80.92f), .34f, .055f, art.Rust);
            Box("MangleSpoke", g, new Vector3(48.6f, Y + 1.15f, 80.92f), new Vector3(.64f, .045f, .045f), art.Steel, false);
            Box("MangleGuard", g, new Vector3(48.6f, Y + 1.38f, 82.5f), new Vector3(.6f, .08f, 2.9f), art.Dado, false);
            foreach (float x in new[] { 41f, 44.5f })
            {
                Box("HangingSheet", g, new Vector3(x + .7f, Y + .66f, 81.67f), new Vector3(.65f, .5f, .025f), art.Canvas, false);
                K.Patch("TableWear", g, new Vector3(x - .7f, Y + .925f, 82.2f), new Vector2(.35f, .45f), 25f, art.Stain);
            }
            Box("LaundrySink", g, new Vector3(32.2f, Y + .85f, 90.85f), new Vector3(1.75f, .25f, .78f), art.Porcelain, true);
            Box("SinkRecess", g, new Vector3(32.2f, Y + .98f, 90.85f), new Vector3(1.44f, .025f, .57f), art.Bars, false);
            foreach (float x in new[] { 31.58f, 32.82f })
                Box("SinkBracket", g, new Vector3(x, Y + .55f, 91.07f), new Vector3(.12f, .6f, .4f), art.Steel, true);
            Pipe("SinkTap", g, new Vector3(32.2f, Y + .98f, 91.17f), new Vector3(32.2f, Y + 1.3f, 91.17f), .05f, art.Brass);
            Pipe("TapSpout", g, new Vector3(32.2f, Y + 1.3f, 91.17f), new Vector3(32.2f, Y + 1.3f, 90.94f), .05f, art.Brass);
        }
    }
}
