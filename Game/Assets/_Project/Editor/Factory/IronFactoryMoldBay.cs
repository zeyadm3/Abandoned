using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Ingot Teeming & Mold Stripping Bay on the east bank adjacent to the rail siding (x = 78.5, z = 36).
    /// In an authentic iron and steel works, molten metal from ladles is poured into massive tapered chill
    /// molds mounted on cast-iron stools. Once cooled into solid ingots, overhead stripping tongs clamp the molds
    /// to pull them free. Features heavy ingot stools, vertical mold clusters, one stripped glowing ingot,
    /// and a suspended overhead mechanical stripping tong.
    /// </summary>
    public static class IronFactoryMoldBay
    {
        private static readonly Vector3 Center = new(78.5f, 0f, 36f);

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform bay = Group("IngotMoldBay", root);
            bay.position = Center;

            Material moldIron = GetMaterial("IronFactory_MoldIron", new Color(.2f, .19f, .18f));
            Material coolingIngot = GetMaterial("IronFactory_CoolingIron", new Color(.25f, .08f, .03f));
            coolingIngot.SetColor("_EmissionColor", new Color(1.2f, .3f, .05f));
            coolingIngot.EnableKeyword("_EMISSION");

            // Heavy cast iron ingot stool base (ground plate)
            Box("StoolBase", bay, Vector3.up * .18f, new Vector3(2.4f, .36f, 5.2f), art.Steel, true);

            // Three vertical ingot molds standing on the stool
            float[] zPositions = { -1.6f, 0f, 1.6f };
            for (int i = 0; i < 2; i++)
            {
                float z = zPositions[i];
                Transform mold = Group($"IngotMold_{i}", bay);
                mold.localPosition = new Vector3(0f, .36f, z);

                // Tapered vertical mold: wider base, narrower top
                Box("MoldBase", mold, Vector3.up * .6f, new Vector3(.95f, 1.2f, .95f), moldIron, true);
                Box("MoldTop", mold, Vector3.up * 1.6f, new Vector3(.82f, 1f, .82f), moldIron, true);
                Box("MoldRim", mold, Vector3.up * 2.15f, new Vector3(.88f, .1f, .88f), art.Steel, false);

                // Lifting trunnion lugs on the sides for stripper crane tongs
                foreach (float side in new[] { -.48f, .48f })
                    Box("LiftingLug", mold, new Vector3(side, 1.8f, 0f), new Vector3(.18f, .16f, .16f), art.Steel, false);

                // Top cavity with dark molten slag crust
                Box("MoltenTop", mold, Vector3.up * 2.05f, new Vector3(.6f, .04f, .6f), art.Bed, false);
            }

            // Third position: The mold has been stripped off and set aside, revealing the glowing hot ingot!
            float zStripped = zPositions[2];
            Transform ingot = Group("SolidifiedIngot", bay);
            ingot.localPosition = new Vector3(0f, .36f, zStripped);

            // Tapered glowing ingot body
            Box("IngotCore", ingot, Vector3.up * .9f, new Vector3(.68f, 1.8f, .68f), coolingIngot, true);
            Box("HotTop", ingot, Vector3.up * 1.85f, new Vector3(.58f, .2f, .58f), art.Bed, false);

            // Warm radiant heat light from the glowing stripped ingot
            Transform heatLight = Group("IngotHeatGlow", ingot);
            heatLight.localPosition = Vector3.up * 1f;
            Light glow = heatLight.gameObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.range = 5f;
            glow.intensity = .9f;
            glow.color = new Color(1f, .35f, .1f);
            glow.shadows = LightShadows.None;

            // The stripped mold tilted on timber dunnage beside the stool
            Transform restingMold = Group("StrippedMold", bay);
            restingMold.localPosition = new Vector3(1.6f, .2f, zStripped);
            restingMold.localRotation = Quaternion.Euler(0f, 0f, 12f);
            Box("Dunnage_1", bay, new Vector3(1.4f, .08f, zStripped - .35f), new Vector3(.25f, .16f, .8f), art.Bed, false);
            Box("Dunnage_2", bay, new Vector3(1.9f, .08f, zStripped + .35f), new Vector3(.25f, .16f, .8f), art.Bed, false);
            Box("TiltedBody", restingMold, Vector3.up * 1.1f, new Vector3(.9f, 2.1f, .9f), moldIron, true);

            // Overhead mechanical mold stripper tong suspended above the bay at y = 6.8 m
            Transform stripper = Group("OverheadStripperTong", bay);
            stripper.localPosition = new Vector3(0f, 6.8f, 0f);

            // Cross spreader beam and hoist cables
            Box("SpreaderBeam", stripper, Vector3.zero, new Vector3(1.8f, .3f, .4f), art.Safety, false);
            foreach (float side in new[] { -.7f, .7f })
            {
                Box("HoistCable", stripper, new Vector3(side, 1.2f, 0f), new Vector3(.04f, 2.4f, .04f), art.Steel, false);

                // Scissor tong arms extending downward toward the molds
                Transform tongArm = Group($"TongArm_{side:0.0}", stripper);
                tongArm.localPosition = new Vector3(side, -.2f, 0f);
                Box("UpperArm", tongArm, new Vector3(0f, -.8f, 0f), new Vector3(.14f, 1.4f, .18f), art.Steel, false);
                Box("LowerBite", tongArm, new Vector3(side > 0 ? -.15f : .15f, -1.8f, 0f), new Vector3(.25f, .8f, .16f), art.Rust, false);
            }

            bay.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Metal);
            MarkStatic(bay);
        }
    }
}
