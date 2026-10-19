using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Overhead industrial piping, ducting and electrical cable trays throughout the Iron Factory:
    /// high-pressure insulated steam mains with thermal expansion U-loops, vertical steam feeds to presses,
    /// a blast furnace dirty gas downcomer pipe and cyclone dust collector on the west wall, cable trays
    /// hung beneath the tier-1 catwalks, and cooling water manifolds over the rolling mill.
    /// Scenery only: static, well clear of player movement routes.
    /// </summary>
    public static class IronFactoryPiping
    {
        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform group = Group("Piping", root);
            SteamMains(Group("SteamMains", group), art);
            GasDowncomer(Group("GasDucts", group), art);
            CableTrays(Group("CableTrays", group), art);
            WaterManifolds(Group("WaterManifolds", group), art);
            MarkStatic(group);
        }

        // High-pressure steam mains suspended below the roof trusses with authentic thermal expansion U-loops.
        private static void SteamMains(Transform parent, IronFactoryArt art)
        {
            const float pipeY = 17.2f, pipeDia = .42f;
            float[] zLines = { 32f, 68f };

            foreach (float z in zLines)
            {
                Transform line = Group($"Main_Z_{z:0}", parent);
                // Straight run west to east, interrupted by expansion U-loops at x = 38 m and x = 82 m
                Box("SegmentA", line, new Vector3(18f, pipeY, z), new Vector3(32f, pipeDia, pipeDia), art.Steel, false);
                ExpansionLoop(line, new Vector3(38f, pipeY, z), art);
                Box("SegmentB", line, new Vector3(60f, pipeY, z), new Vector3(40f, pipeDia, pipeDia), art.Steel, false);
                ExpansionLoop(line, new Vector3(82f, pipeY, z), art);
                Box("SegmentC", line, new Vector3(102f, pipeY, z), new Vector3(36f, pipeDia, pipeDia), art.Steel, false);

                // Pipe hangers to the trusses above every 12 m
                for (float x = 12f; x < L.Width * L.Tile; x += 12f)
                {
                    float drop = L.WallHeight - .25f - pipeY;
                    Box("Hanger", line, new Vector3(x, pipeY + drop * .5f, z), new Vector3(.06f, drop, .06f), art.Steel, false);
                    Box("Clamp", line, new Vector3(x, pipeY, z), new Vector3(.14f, pipeDia + .08f, pipeDia + .08f), art.Rust, false);
                }
            }

            // Vertical steam downcomer pipes feeding the heavy machines
            Downcomer(parent, new Vector3(38f, 0f, 44f), pipeY, art); // Steam hammer feed
            Downcomer(parent, new Vector3(36f, 0f, 30f), pipeY, art); // Press 1 feed
            Downcomer(parent, new Vector3(36f, 0f, 56f), pipeY, art); // Press 2 feed
        }

        // Classic U-shaped expansion bend dropping down and looping out horizontally
        private static void ExpansionLoop(Transform parent, Vector3 at, IronFactoryArt art)
        {
            Transform loop = Group("ExpansionLoop", parent);
            loop.position = at;
            const float pipe = .38f, drop = 1.4f, width = 2.4f;

            // Down legs, bottom cross and return
            Box("Leg1", loop, new Vector3(-width * .5f, -drop * .5f, 0f), new Vector3(pipe, drop, pipe), art.Steel, false);
            Box("Leg2", loop, new Vector3(width * .5f, -drop * .5f, 0f), new Vector3(pipe, drop, pipe), art.Steel, false);
            Box("Bottom", loop, new Vector3(0f, -drop, 0f), new Vector3(width, pipe, pipe), art.Steel, false);

            // Flanged elbow rings
            foreach (float side in new[] { -1f, 1f })
            {
                Box("FlangeTop", loop, new Vector3(side * width * .5f, 0f, 0f), new Vector3(pipe + .1f, .08f, pipe + .1f), art.Rust, false);
                Box("FlangeBot", loop, new Vector3(side * width * .5f, -drop, 0f), new Vector3(pipe + .1f, .08f, pipe + .1f), art.Rust, false);
            }
        }

        private static void Downcomer(Transform parent, Vector3 baseAt, float topY, IronFactoryArt art)
        {
            Transform feed = Group("SteamDowncomer", parent);
            float h = topY - 3.8f;
            Box("Pipe", feed, baseAt + new Vector3(.8f, 3.8f + h * .5f, 0f), new Vector3(.22f, h, .22f), art.Steel, false);
            Box("Valve", feed, baseAt + new Vector3(.8f, 4.2f, 0f), new Vector3(.38f, .38f, .38f), art.Rust, false);
            Box("Handwheel", feed, baseAt + new Vector3(1.1f, 4.2f, 0f), new Vector3(.05f, .3f, .3f), art.Safety, false);
        }

        // Large 1.4 m blast furnace dirty gas pipe leading to a cyclone dust catcher on the west wall.
        private static void GasDowncomer(Transform parent, IronFactoryArt art)
        {
            Transform duct = Group("GasDuct", parent);
            Vector3 bfTop = new(5f, 17f, 24f);
            Vector3 cyclonePos = new(2.2f, 13.5f, 24f);

            // Diagonal riveted main duct from blast furnace to cyclone
            Vector3 delta = cyclonePos - bfTop;
            GameObject pipe = Primitive(PrimitiveType.Cylinder, "MainDuct", duct, (bfTop + cyclonePos) * .5f,
                new Vector3(1.4f, delta.magnitude * .5f, 1.4f), art.Rust, false);
            pipe.transform.rotation = Quaternion.LookRotation(Vector3.Cross(delta.normalized, Vector3.up), delta.normalized);

            // Cyclone dust catcher vessel on the west wall
            Transform cyclone = Group("DustCatcher", duct);
            cyclone.position = cyclonePos;
            Primitive(PrimitiveType.Cylinder, "Body", cyclone, Vector3.up * 1.5f, new Vector3(2.4f, 1.8f, 2.4f), art.Steel, false);
            Box("Cone", cyclone, Vector3.down * .8f, new Vector3(1.8f, 1.6f, 1.8f), art.Rust, false);
            Box("SupportLeg", cyclone, new Vector3(0f, -1.8f, .8f), new Vector3(.3f, 2.2f, .3f), art.Steel, false);
            Box("SupportLeg", cyclone, new Vector3(0f, -1.8f, -.8f), new Vector3(.3f, 2.2f, .3f), art.Steel, false);
            Box("DischargeChute", cyclone, Vector3.down * 1.8f, new Vector3(.5f, .8f, .5f), art.Steel, false);
        }

        // Heavy perforated electrical cable trays hung under tier-1 catwalks
        private static void CableTrays(Transform parent, IronFactoryArt art)
        {
            const float trayY = 4.45f;
            float[] xRoutes = { 16f, 32f, 80f, 104f };

            foreach (float x in xRoutes)
            {
                Transform tray = Group($"Tray_X_{x:0}", parent);
                // 40 m run along the aisles
                Box("Channel", tray, new Vector3(x, trayY, 50f), new Vector3(.55f, .12f, 44f), art.Steel, false);
                Box("Cables", tray, new Vector3(x, trayY + .03f, 50f), new Vector3(.42f, .08f, 44f), art.Bed, false);

                // Junction boxes along the run
                for (float z = 32f; z <= 68f; z += 12f)
                {
                    Box("JunctionBox", tray, new Vector3(x, trayY - .08f, z), new Vector3(.35f, .22f, .35f), art.Steel, false);
                    Box("VoltageLabel", tray, new Vector3(x, trayY - .08f, z - .18f), new Vector3(.2f, .12f, .02f), art.Safety, false);
                }
            }
        }

        // Water spray manifolds running over the hot rolling mill stands
        private static void WaterManifolds(Transform parent, IronFactoryArt art)
        {
            Transform water = Group("WaterManifold", parent);
            const float millX = 26f, pipeY = 4.15f;

            // Long water header pipe spanning the rolling mill line
            Box("Header", water, new Vector3(millX, pipeY, 49f), new Vector3(.24f, .24f, 48f), art.Steel, false);

            // Spray branches pointing down at each mill stand
            float[] standsZ = { 26f, 36f, 52f, 68f };
            foreach (float z in standsZ)
            {
                Transform spray = Group($"Spray_Z_{z:0}", water);
                Box("Branch", spray, new Vector3(millX, pipeY - .35f, z), new Vector3(.12f, .7f, .12f), art.Steel, false);
                Box("NozzleBar", spray, new Vector3(millX, pipeY - .75f, z), new Vector3(1.6f, .08f, .14f), art.Safety, false);
                for (float side = -.6f; side <= .6f; side += .4f)
                    Box("Nozzle", spray, new Vector3(millX + side, pipeY - .82f, z), new Vector3(.05f, .08f, .05f), art.Rust, false);
            }
        }
    }
}
