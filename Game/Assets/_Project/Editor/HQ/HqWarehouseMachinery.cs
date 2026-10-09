using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>A parked warehouse forklift establishes industrial scale beside the receiving floor.</summary>
    public static class HqWarehouseMachinery
    {
        public static void Build(Transform root)
        {
            Transform lift = At("ParkedCompanyForklift", root, new Vector3(5.6f, 0f, 13.3f));
            Material paint = PolishAssets.Material("HQ_ForkliftPaint", new Color(.57f, .43f, .14f), .82f);
            Material steel = HqIndustrialMaterials.Steel, rubber = CustomMallArt.Material("Rubber");
            Box("TruckChassis", lift, new Vector3(0f, .57f, 0f), new Vector3(1.45f, .55f, 1.9f), steel);
            Box("Counterweight", lift, new Vector3(0f, .93f, -.60f), new Vector3(1.52f, .72f, .84f), paint);
            Box("OperatorFloor", lift, new Vector3(0f, .89f, .23f), new Vector3(1.32f, .08f, .87f), steel);
            foreach (float x in new[] { -.76f, .76f })
            foreach (float z in new[] { -.63f, .66f })
            {
                var wheel = Primitive(PrimitiveType.Cylinder, "ForkliftTyre", lift, new Vector3(x, .36f, z), new Vector3(.70f, .115f, .70f), rubber);
                wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                var hub = Primitive(PrimitiveType.Cylinder, "WheelHub", lift, new Vector3(x * 1.15f, .36f, z), new Vector3(.28f, .018f, .28f), paint, false);
                hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            }
            Box("SeatCushion", lift, new Vector3(0f, 1.14f, -.18f), new Vector3(.50f, .14f, .48f), rubber, false);
            Box("SeatBack", lift, new Vector3(0f, 1.44f, -.42f), new Vector3(.50f, .52f, .12f), rubber, false);
            foreach (float x in new[] { -.61f, .61f })
            foreach (float z in new[] { -.55f, .63f })
                Box("OverheadGuardPost", lift, new Vector3(x, 1.8f, z), new Vector3(.06f, 1.66f, .06f), steel);
            Box("OverheadSafetyGuard", lift, new Vector3(0f, 2.65f, .04f), new Vector3(1.5f, .11f, 1.48f), steel, false);
            for (int i = 0; i < 5; i++)
                Box("GuardSlot", lift, new Vector3(-.5f + i * .25f, 2.711f, .04f), new Vector3(.09f, .006f, 1.2f), rubber, false);
            foreach (float x in new[] { -.44f, .44f })
                Box("ForkMastRail", lift, new Vector3(x, 1.48f, 1.04f), new Vector3(.10f, 2.56f, .15f), steel);
            Box("MastHead", lift, new Vector3(0f, 2.77f, 1.04f), new Vector3(.99f, .13f, .15f), paint, false);
            Box("ForkCarriage", lift, new Vector3(0f, .40f, 1.16f), new Vector3(1.05f, .29f, .13f), steel);
            foreach (float x in new[] { -.43f, .43f })
            {
                Box("ForkHeel", lift, new Vector3(x, .37f, 1.26f), new Vector3(.13f, .41f, .13f), steel, false);
                Box("LoweredFork", lift, new Vector3(x, .19f, 1.86f), new Vector3(.13f, .08f, 1.29f), steel, false);
            }
            Rod("SteeringColumn", lift, new Vector3(0f, .91f, .64f), new Vector3(0f, 1.48f, .45f), .045f, steel);
            var steering = Primitive(PrimitiveType.Cylinder, "SteeringWheel", lift, new Vector3(0f, 1.50f, .44f), new Vector3(.34f, .014f, .34f), rubber, false);
            steering.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
            Label(lift, "ABANDONED SALVAGING\nHANDLING / 02", new Vector3(0f, 1.00f, -1.032f), .036f, new Color(.83f, .81f, .63f));
            MarkStatic(lift);
        }
    }
}
