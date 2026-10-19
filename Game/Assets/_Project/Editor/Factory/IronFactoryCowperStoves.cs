using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.IronFactoryLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The battery of 3 Cowper hot-blast stoves standing outside the west wall opposite the blast furnace.
    /// In an authentic iron works, these regenerative brick-filled towers preheat combustion blast air to 1100°C
    /// before feeding it into the bustle pipe. They tower over the west yard with domed caps, hot-blast mains
    /// bridging through the west wall, and cold-blast feeder manifolds. Scenery only (out of NavMesh).
    /// </summary>
    public static class IronFactoryCowperStoves
    {
        private const float StoveRadius = 2.2f;
        private const float StoveHeight = 22f;
        private const float StoveX = -7f;
        private static readonly float[] StoveZ = { 16f, 24f, 32f };

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform battery = Group("CowperStoves", root);
            Material stoveMetal = GetMaterial("IronFactory_StoveRust", new Color(.24f, .16f, .12f));

            foreach (float z in StoveZ)
            {
                Transform stove = Group($"Stove_{z:00}", battery);
                stove.position = new Vector3(StoveX, 0f, z);

                // Main vertical cylindrical shaft
                Primitive(PrimitiveType.Cylinder, "Shaft", stove, Vector3.up * (StoveHeight * .5f),
                    new Vector3(StoveRadius * 2f, StoveHeight * .5f, StoveRadius * 2f), stoveMetal, false);

                // Structural armor bands every 4 metres
                for (float y = 3f; y < StoveHeight; y += 4f)
                    Primitive(PrimitiveType.Cylinder, "Band", stove, Vector3.up * y,
                        new Vector3(StoveRadius * 2.1f, .2f, StoveRadius * 2.1f), art.Steel, false);

                // Domed cap (stepped hemispherical dome)
                float[] domeRadii = { 2.1f, 1.8f, 1.3f, .7f };
                float[] domeHeights = { .5f, .5f, .4f, .3f };
                float currY = StoveHeight;
                for (int d = 0; d < domeRadii.Length; d++)
                {
                    float r = domeRadii[d], dh = domeHeights[d];
                    Primitive(PrimitiveType.Cylinder, $"Dome_{d}", stove, Vector3.up * (currY + dh * .5f),
                        new Vector3(r * 2f, dh * .5f, r * 2f), art.Steel, false);
                    currY += dh;
                }

                // Top snort / chimney valve
                Box("ChimneyValve", stove, Vector3.up * (currY + .6f), new Vector3(.6f, 1.2f, .6f), art.Rust, false);

                // Combustion burner chamber at base
                Box("BurnerChamber", stove, new Vector3(StoveRadius + .5f, 1.8f, 0f), new Vector3(1.4f, 2.8f, 1.4f), art.Rust, false);
                Box("GasValve", stove, new Vector3(StoveRadius + 1.2f, 1.8f, 0f), new Vector3(.4f, .4f, .4f), art.Safety, false);
            }

            // Cold-blast air feeder manifold along the outer base (west side)
            Transform manifold = Group("ColdBlastManifold", battery);
            float minZ = StoveZ[0] - 3f, maxZ = StoveZ[2] + 3f;
            float span = maxZ - minZ;
            Box("MainPipe", manifold, new Vector3(StoveX - StoveRadius - .8f, 1.4f, (minZ + maxZ) * .5f),
                new Vector3(.9f, .9f, span), art.Steel, false);

            // Overhead Hot-Blast Main header pipe connecting all 3 stoves into the plant west wall
            Transform hotBlast = Group("HotBlastMain", battery);
            // Longitudinal collector duct between the 3 stoves at y = 8m
            Box("CollectorDuct", hotBlast, new Vector3(StoveX + StoveRadius + .8f, 8f, (minZ + maxZ) * .5f),
                new Vector3(1.1f, 1.1f, span), art.Rust, false);

            // Stub connectors from each stove into collector duct
            foreach (float z in StoveZ)
                Box($"StoveConnector_{z:00}", hotBlast, new Vector3(StoveX + StoveRadius * .5f + .4f, 8f, z),
                    new Vector3(StoveRadius, .8f, .8f), art.Rust, false);

            // Hot-blast trunk line running through west wall (x=0) to the blast furnace bustle pipe (x=5, y=7.5, z=24)
            Box("ThroughWallTrunk", hotBlast, new Vector3((StoveX + StoveRadius + .8f + 5f) * .5f, 7.8f, 24f),
                new Vector3(5f - (StoveX + StoveRadius + .8f), 1.1f, 1.1f), art.Rust, false);

            // Steel support trestles under the hot-blast main
            foreach (float z in new[] { 18f, 24f, 30f })
            {
                Box("TrestleLeg_L", hotBlast, new Vector3(StoveX + StoveRadius + .5f, 3.8f, z - .6f), new Vector3(.2f, 7.6f, .2f), art.Steel, false);
                Box("TrestleLeg_R", hotBlast, new Vector3(StoveX + StoveRadius + .5f, 3.8f, z + .6f), new Vector3(.2f, 7.6f, .2f), art.Steel, false);
                Box("TrestleBeam", hotBlast, new Vector3(StoveX + StoveRadius + .5f, 7.2f, z), new Vector3(.25f, .3f, 1.4f), art.Steel, false);
            }

            battery.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(battery);
        }
    }
}
