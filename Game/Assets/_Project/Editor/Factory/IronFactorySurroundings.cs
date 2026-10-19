using Abandoned.Core;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// What lies around the Iron Factory (the user's choice: rail yard, slag heaps, smokestacks, cooling towers):
    /// a perimeter fence that keeps the crew on site, the open ground out to the mall's mountain horizon, a rail
    /// yard of dead rolling stock to the south, slag heaps, two cooling towers and the plant's smokestacks. All of
    /// it is scenery (static, out of the NavMesh); only the fence and the stacks inside it are solid.
    /// </summary>
    public static class IronFactorySurroundings
    {
        public const string GroupName = "Surroundings";
        private static readonly Rect Yard = new(-24f, -24f, L.Width * L.Tile + 48f, L.Depth * L.Tile + 48f);
        private static readonly Rect Fence = new(-20f, -23.5f, L.Width * L.Tile + 40f, L.Depth * L.Tile + 43.5f);

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform group = Group(GroupName, root);
            Material slag = GetMaterial("IronFactory_Slag", new Color(.11f, .1f, .1f));
            Material concrete = GetMaterial("IronFactory_TowerConcrete", new Color(.42f, .41f, .39f));
            Material brick = GetMaterial("IronFactory_StackBrick", new Color(.27f, .14f, .1f));
            Material ballast = GetMaterial("IronFactory_Ballast", new Color(.16f, .15f, .14f));
            Material boxcar = GetMaterial("IronFactory_Boxcar", new Color(.3f, .12f, .08f));

            Ground(Group("OuterGround", group), art);
            BuildFence(Group("Fence", group), art);
            RailYard(Group("RailYard", group), art, ballast, boxcar);
            foreach ((Vector3 at, Vector3 size) in new[]
            {
                (new Vector3(-80f, -3f, 60f), new Vector3(46f, 16f, 34f)), (new Vector3(-70f, -2f, 125f), new Vector3(34f, 11f, 28f)),
                (new Vector3(190f, -3f, 140f), new Vector3(40f, 14f, 30f)),
            })
                Far(PolishAssets.Shape("SlagHeap", group, at, size, slag, true));
            CoolingTower(group, new Vector3(215f, 0f, 20f), concrete);
            CoolingTower(group, new Vector3(250f, 0f, 75f), concrete);
            foreach (float x in new[] { 16f, 34f, 104f }) Smokestack(group, new Vector3(x, 0f, L.Depth * L.Tile + 9f), brick, art);
            WaterTower(group, new Vector3(-36f, 0f, -48f), art);

            // Exterior scenery compounds
            IronFactoryYardLights.Build(root, art);
            IronFactorySubstation.Build(root, art);
            IronFactoryGatehouse.Build(root, art);
            IronFactoryYardClutter.Build(root, art);

            MarkStatic(group);
        }

        // Four slabs from the yard out toward the mountains, never under the plant (one there would roof the lava).
        private static void Ground(Transform g, IronFactoryArt art)
        {
            const float far = 160f, y = -.3f, thick = .5f;
            float x0 = Yard.xMin - far, x1 = Yard.xMax + far, z0 = Yard.yMin - far, z1 = Yard.yMax + far;
            void Slab(string name, float ax, float az, float bx, float bz) =>
                Box(name, g, new Vector3((ax + bx) * .5f, y, (az + bz) * .5f), new Vector3(bx - ax, thick, bz - az), art.Dirt);
            Slab("South", x0, z0, x1, Yard.yMin);
            Slab("North", x0, Yard.yMax, x1, z1);
            Slab("West", x0, Yard.yMin, Yard.xMin, Yard.yMax);
            Slab("East", Yard.xMax, Yard.yMin, x1, Yard.yMax);
        }

        // Chain-link on posts: real woven wire mesh, tension rails, and 3-strand angled barbed wire outriggers.
        private static void BuildFence(Transform g, IronFactoryArt art)
        {
            const float height = 2.8f;
            Vector2[] corners = { new(Fence.xMin, Fence.yMin), new(Fence.xMax, Fence.yMin), new(Fence.xMax, Fence.yMax), new(Fence.xMin, Fence.yMax) };
            for (int side = 0; side < 4; side++)
            {
                Vector2 a = corners[side], b = corners[(side + 1) % 4];
                var from = new Vector3(a.x, 0f, a.y);
                var to = new Vector3(b.x, 0f, b.y);
                Vector3 mid = (from + to) * .5f;
                float length = Vector3.Distance(from, to);
                Transform line = Group($"FenceLine_{side}", g);
                line.SetPositionAndRotation(mid, Quaternion.LookRotation((to - from).normalized));
                var stop = line.gameObject.AddComponent<BoxCollider>();
                stop.center = Vector3.up * height * .5f;
                stop.size = new Vector3(.2f, height, length);

                // Real see-through woven chain-link mesh panel
                Box("ChainlinkMesh", line, Vector3.up * (height * .5f), new Vector3(.02f, height, length), art.Guard, false);

                // Top and bottom tubular steel tension rails
                Box("TopRail", line, Vector3.up * height, new Vector3(.06f, .06f, length), art.Steel, false);
                Box("BottomRail", line, Vector3.up * .1f, new Vector3(.06f, .06f, length), art.Steel, false);

                int posts = Mathf.CeilToInt(length / 4f);
                for (int i = 0; i <= posts; i++)
                {
                    float pz = -length * .5f + length * i / posts;
                    Box("Post", line, new Vector3(0f, height * .5f, pz), new Vector3(.1f, height, .1f), art.Rust, false);

                    // Angled barbed-wire outrigger arm facing outward
                    GameObject outrigger = Box("Outrigger", line, new Vector3(-.18f, height + .22f, pz),
                        new Vector3(.03f, .5f, .03f), art.Steel, false);
                    outrigger.transform.localRotation = Quaternion.Euler(0f, 0f, 40f);
                }

                // 3 strands of barbed wire running along the top
                foreach (float y in new[] { height + .1f, height + .25f, height + .4f })
                    Box("BarbedWire", line, new Vector3(-.18f, y, 0f), new Vector3(.02f, .02f, length), art.Steel, false);
            }
        }

        private static void RailYard(Transform g, IronFactoryArt art, Material ballast, Material boxcar)
        {
            const float x0 = -120f, x1 = 240f;
            float[] tracks = { -34f, -40f, -46f, -52f };
            foreach (float z in tracks)
            {
                Box("Ballast", g, new Vector3((x0 + x1) * .5f, -.02f, z), new Vector3(x1 - x0, .06f, 2.8f), ballast, false);
                foreach (float side in new[] { -.72f, .72f })
                    Box("Rail", g, new Vector3((x0 + x1) * .5f, .08f, z + side), new Vector3(x1 - x0, .14f, .07f), art.Steel, false);
            }
            // Dead rolling stock: detailed cuts of boxcars and ore hoppers
            for (int i = 0; i < 5; i++) Car(g, new Vector3(-10f + i * 16f, 0f, -40f), boxcar, art, false);
            for (int i = 0; i < 4; i++) Car(g, new Vector3(110f + i * 14f, 0f, -46f), art.Rust, art, true);
            for (int i = 0; i < 3; i++) Car(g, new Vector3(-80f + i * 16f, 0f, -52f), boxcar, art, false);
        }

        private static void Car(Transform g, Vector3 at, Material body, IronFactoryArt art, bool hopper)
        {
            Transform car = Group(hopper ? "OreHopperCar" : "Boxcar", g);
            car.position = at;
            float length = hopper ? 11.5f : 14.5f, height = hopper ? 2.8f : 3.5f;

            // Main Car Body
            Box("Body", car, new Vector3(0f, 1.1f + height * .5f, 0f), new Vector3(length, height, 3.1f), body, false);

            if (hopper)
            {
                // Tapered hopper slope chutes and discharge gates
                foreach (float x in new[] { -2.8f, 2.8f })
                {
                    Box("Chute", car, new Vector3(x, .85f, 0f), new Vector3(2.4f, .9f, 2.4f), art.Rust, false);
                    Box("GateWheel", car, new Vector3(x, .65f, 1.3f), new Vector3(.3f, .3f, .05f), art.Safety, false);
                }
                Box("TopRim", car, new Vector3(0f, 1.1f + height + .06f, 0f), new Vector3(length + .2f, .12f, 3.3f), art.Steel, false);
            }
            else
            {
                // Boxcar exterior structural side ribs
                for (float x = -length * .45f; x <= length * .45f; x += 1.4f)
                {
                    Box("SideRib_L", car, new Vector3(x, 1.1f + height * .5f, 1.58f), new Vector3(.08f, height, .06f), art.Rust, false);
                    Box("SideRib_R", car, new Vector3(x, 1.1f + height * .5f, -1.58f), new Vector3(.08f, height, .06f), art.Rust, false);
                }
                // Sliding cargo doors in center
                Box("Door_L", car, new Vector3(0f, 1.1f + height * .5f, 1.6f), new Vector3(2.2f, height - .4f, .1f), art.Steel, false);
                Box("Door_R", car, new Vector3(0f, 1.1f + height * .5f, -1.6f), new Vector3(2.2f, height - .4f, .1f), art.Steel, false);
                Box("DoorGuide", car, new Vector3(0f, 1.1f + height - .1f, 1.62f), new Vector3(4.8f, .08f, .06f), art.Rust, false);

                // Roof running board catwalk
                Box("RoofCatwalk", car, new Vector3(0f, 1.1f + height + .06f, 0f), new Vector3(length + .4f, .1f, .8f), art.Grating, false);

                // End ladder
                for (float y = 1.2f; y < 1.1f + height; y += .45f)
                    Box("LadderRung", car, new Vector3(length * .5f + .05f, y, .8f), new Vector3(.04f, .03f, .45f), art.Steel, false);
            }

            // Wheel bogies (two 2-axle trucks) with actual steel wheels
            foreach (float end in new[] { -1f, 1f })
            {
                Transform truck = Group("Bogie", car);
                truck.localPosition = new Vector3(end * (length * .5f - 2f), .5f, 0f);
                Box("BogieFrame", truck, Vector3.zero, new Vector3(2.6f, .4f, 2.2f), art.Bed, false);

                // 4 cylindrical wheels per truck (2 axles)
                foreach (float ax in new[] { -.75f, .75f })
                foreach (float az in new[] { -1.05f, 1.05f })
                {
                    Primitive(PrimitiveType.Cylinder, "Wheel", truck, new Vector3(ax, -.05f, az),
                        new Vector3(.68f, .14f, .68f), art.Steel, false).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                }
            }

            // Coupler draft gear on ends
            foreach (float end in new[] { -1f, 1f })
                Box("Coupler", car, new Vector3(end * (length * .5f + .35f), .6f, 0f), new Vector3(.7f, .25f, .25f), art.Rust, false);
        }

        // A hyperboloid of stacked rings: wide foot, narrow waist, a slight flare at the lip.
        private static void CoolingTower(Transform g, Vector3 at, Material concrete)
        {
            Transform tower = Group("CoolingTower", g);
            tower.position = at;
            float[] radius = { 24f, 20.5f, 17.5f, 15.5f, 15f, 16f };
            const float ring = 10f;
            for (int i = 0; i < radius.Length; i++)
                Far(Primitive(PrimitiveType.Cylinder, "Ring", tower, new Vector3(0f, ring * (i + .5f), 0f),
                    new Vector3(radius[i] * 2f, ring * .5f, radius[i] * 2f), concrete, false));
        }

        // Inside the fence, so the shaft is solid.
        private static void Smokestack(Transform g, Vector3 at, Material brick, IronFactoryArt art)
        {
            Transform stack = Group("Smokestack", g);
            stack.position = at;
            const float height = 72f, r = 2.4f;
            Far(Primitive(PrimitiveType.Cylinder, "Shaft", stack, new Vector3(0f, height * .5f, 0f), new Vector3(r * 2f, height * .5f, r * 2f), brick));
            foreach (float y in new[] { 24f, 48f, height - 1f })
                Primitive(PrimitiveType.Cylinder, "Band", stack, new Vector3(0f, y, 0f), new Vector3(r * 2.2f, .6f, r * 2.2f), art.Rust, false);
            // The flue duct from the plant's wall to the stack's foot.
            Box("Flue", stack, new Vector3(0f, 14f, -5f), new Vector3(2.2f, 2.2f, 8f), art.Steel, false);

            // Red aviation warning beacon on the top rim
            Transform beacon = Group("Beacon", stack);
            beacon.localPosition = Vector3.up * (height + .4f);
            Box("BeaconBase", beacon, Vector3.zero, new Vector3(.4f, .3f, .4f), art.Steel, false);
            Material redGlow = GetMaterial("IronFactory_RedBeacon", new Color(1f, .1f, .05f));
            redGlow.SetColor("_EmissionColor", new Color(3f, .2f, .1f));
            redGlow.EnableKeyword("_EMISSION");
            Primitive(PrimitiveType.Cylinder, "BeaconLamp", beacon, Vector3.up * .25f, new Vector3(.3f, .35f, .3f), redGlow, false);
        }

        private static void WaterTower(Transform g, Vector3 at, IronFactoryArt art)
        {
            Transform tower = Group("WaterTower", g);
            tower.position = at;
            foreach (float x in new[] { -3f, 3f })
            foreach (float z in new[] { -3f, 3f })
                Box("Leg", tower, new Vector3(x, 7f, z), new Vector3(.4f, 14f, .4f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Tank", tower, new Vector3(0f, 17f, 0f), new Vector3(9f, 3f, 9f), art.Rust, false);
            Primitive(PrimitiveType.Cylinder, "Roof", tower, new Vector3(0f, 20.3f, 0f), new Vector3(9.4f, .3f, 9.4f), art.Steel, false);
        }

        // Big distant shapes: no shadows (they would cost the most and show the least).
        private static void Far(GameObject go) => go.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
    }
}
