using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The main plant security gatehouse, vehicle inspection island and boom barrier gate
    /// at the south entrance of the factory yard (x = 60, z = -20).
    /// Gives the player a believable, structured entry point with a guard booth, overhead
    /// plant entrance gantry sign, yellow traffic bollards, and open heavy steel sliding gates.
    /// </summary>
    public static class IronFactoryGatehouse
    {
        private static readonly Vector3 Center = new(60f, 0f, -20.5f);

        public static void Build(Transform root, IronFactoryArt art)
        {
            Transform gatehouse = Group("SecurityGatehouse", root);
            gatehouse.position = Center;

            Material guardWindow = GetMaterial("IronFactory_GuardWindow", new Color(.2f, .25f, .26f));
            Material stripeRed = GetMaterial("IronFactory_BarrierRed", new Color(.8f, .15f, .12f));
            Material signBoard = GetMaterial("IronFactory_SignBoard", new Color(.15f, .17f, .16f));

            // Central concrete traffic splitter island
            Box("TrafficIsland", gatehouse, Vector3.up * .12f, new Vector3(2.4f, .24f, 8f), art.Concrete, true);

            // Guard Booth sitting on the island
            Transform booth = Group("GuardBooth", gatehouse);
            booth.localPosition = new Vector3(0f, .24f, 0f);

            // Booth walls and overhanging canopy roof
            Box("BaseWall", booth, Vector3.up * .55f, new Vector3(2.2f, 1.1f, 3.2f), art.Steel, true);
            Box("RoofCanopy", booth, Vector3.up * 2.7f, new Vector3(2.8f, .2f, 3.8f), art.Steel, true);

            // Corner mullions and viewing windows
            foreach (float sx in new[] { -.95f, .95f })
            foreach (float sz in new[] { -1.45f, 1.45f })
                Box("Mullion", booth, new Vector3(sx, 1.7f, sz), new Vector3(.15f, 1.8f, .15f), art.Steel, false);

            // Glass windows all around the booth
            Box("Window_N", booth, new Vector3(0f, 1.7f, 1.45f), new Vector3(1.8f, 1.2f, .05f), guardWindow, false);
            Box("Window_S", booth, new Vector3(0f, 1.7f, -1.45f), new Vector3(1.8f, 1.2f, .05f), guardWindow, false);
            Box("Window_E", booth, new Vector3(.95f, 1.7f, 0f), new Vector3(.05f, 1.2f, 2.6f), guardWindow, false);
            Box("Window_W", booth, new Vector3(-.95f, 1.7f, 0f), new Vector3(.05f, 1.2f, 2.6f), guardWindow, false);

            // Inside: guard desk, stool, retro communication radio
            Transform desk = Group("Desk", booth);
            desk.localPosition = new Vector3(0f, 1.1f, 0f);
            Box("Top", desk, Vector3.zero, new Vector3(1.2f, .08f, 1.8f), art.Bed, false);
            Box("Radio", desk, new Vector3(0f, .16f, .4f), new Vector3(.3f, .22f, .2f), art.Steel, false);
            Box("Antenna", desk, new Vector3(.1f, .42f, .4f), new Vector3(.02f, .35f, .02f), art.Safety, false);

            // Exterior spotlight illuminating the barrier lane
            Transform spot = Group("BoothSpotlight", booth);
            spot.localPosition = new Vector3(0f, 2.5f, 1.8f);
            Box("Casing", spot, Vector3.zero, new Vector3(.2f, .2f, .3f), art.Steel, false);
            Light spotLight = spot.gameObject.AddComponent<Light>();
            spotLight.type = LightType.Spot;
            spotLight.range = 14f;
            spotLight.spotAngle = 60f;
            spotLight.intensity = 4.5f;
            spotLight.color = new Color(1f, .92f, .8f);
            spotLight.shadows = LightShadows.None;

            // Counterweighted boom barrier gate on the east lane
            Transform boom = Group("BoomBarrier", gatehouse);
            boom.localPosition = new Vector3(1.4f, 0f, 2.2f);
            Box("PivotCabinet", boom, Vector3.up * .6f, new Vector3(.4f, 1.2f, .45f), art.Safety, true);
            Box("Counterweight", boom, new Vector3(0f, .95f, -.4f), new Vector3(.35f, .35f, .5f), art.Bed, false);

            // Striped arm tilted upwards in open/damaged position
            Transform arm = Group("BarrierArm", boom);
            arm.localPosition = new Vector3(0f, .95f, 0f);
            arm.localRotation = Quaternion.Euler(-32f, 0f, 0f);
            for (int seg = 0; seg < 6; seg++)
            {
                Material m = (seg % 2 == 0) ? stripeRed : art.Concrete;
                Box($"ArmSeg_{seg}", arm.transform, new Vector3(0f, 0f, .5f + seg * .7f), new Vector3(.08f, .16f, .7f), m, false);
            }

            // Overhead steel entrance gantry portal
            Transform gantry = Group("EntrancePortal", gatehouse);
            gantry.localPosition = new Vector3(0f, 0f, -3.5f);
            const float spanW = 14f, portalH = 6f;
            foreach (float side in new[] { -spanW * .5f, spanW * .5f })
                Box("Pillar", gantry, new Vector3(side, portalH * .5f, 0f), new Vector3(.4f, portalH, .4f), art.Steel, true);
            Box("CrossTruss", gantry, new Vector3(0f, portalH + .3f, 0f), new Vector3(spanW + .8f, .7f, .5f), art.Steel, false);

            // Large industrial entrance sign board
            Transform sign = Group("MainSign", gantry);
            sign.localPosition = new Vector3(0f, portalH - .6f, 0f);
            Box("Backing", sign, Vector3.zero, new Vector3(9f, 1.4f, .12f), signBoard, false);
            Box("Border", sign, Vector3.zero, new Vector3(9.2f, 1.55f, .06f), art.Safety, false);

            // Safety bollards lining the traffic island
            foreach (float z in new[] { -3.2f, -1.8f, 1.8f, 3.2f })
            {
                Primitive(PrimitiveType.Cylinder, "Bollard_W", gatehouse, new Vector3(-1.3f, .45f, z),
                    new Vector3(.22f, .9f, .22f), art.Safety, true);
                Primitive(PrimitiveType.Cylinder, "Bollard_E", gatehouse, new Vector3(1.3f, .45f, z),
                    new Vector3(.22f, .9f, .22f), art.Safety, true);
            }

            gatehouse.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);
            MarkStatic(gatehouse);
        }
    }
}
