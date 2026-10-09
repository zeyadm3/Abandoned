using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>A recognisable reclaim workcell between receiving and high-bay stock, with a parked pallet jack.</summary>
    public static class HqSortingIsland
    {
        private static Material Steel => PolishAssets.Material("HQ_SalvageSteel", new Color(.22f, .25f, .24f), .64f);
        private static Material Rust => CustomMallArt.Material("Rust");
        private static Material Rubber => CustomMallArt.Material("Rubber");
        private static Material Yellow => PolishAssets.Material("HQ_PalletJackPaint", new Color(.66f, .44f, .08f), .78f);

        public static void Build(Transform parent)
        {
            Transform island = HqSalvageProps.At("ReclaimedPartsSortingIsland", parent, new Vector3(17f, 0f, 12f));
            Box("WornSortingTop", island, new Vector3(0f, 1.07f, 0f), new Vector3(4.6f, .12f, 1.8f), Steel);
            Box("SortingApron", island, new Vector3(0f, .87f, -.82f), new Vector3(4.47f, .3f, .10f), Rust);
            Box("UnderTableStockShelf", island, new Vector3(0f, .31f, 0f), new Vector3(4.38f, .085f, 1.58f), Steel);
            foreach (float x in new[] { -2.13f, 0f, 2.13f })
            foreach (float z in new[] { -.72f, .72f })
            {
                Box("TubularSupport", island, new Vector3(x, .52f, z), new Vector3(.12f, 1.04f, .12f), Steel);
                Box("BasePlate", island, new Vector3(x, .035f, z), new Vector3(.28f, .07f, .25f), Rust, false);
            }
            SortingBin(island, new Vector3(-1.2f, 1.14f, 0f), "COPPER / BRASS", 0);
            SortingBin(island, new Vector3(.15f, 1.14f, 0f), "ELECTRICAL PARTS", 1);
            Vice(island, new Vector3(1.62f, 1.14f, .4f));
            HqSalvageProps.Bin(island, new Vector3(1.5f, 1.14f, -.45f), "HAND TOOLS", 2);
            HqSalvageProps.Bin(island, new Vector3(-1.46f, .36f, -.08f), "CLEANED PARTS", 0);
            HqSalvageProps.Bin(island, new Vector3(-.67f, .36f, -.08f), "TO CHECK", 1);
            HqSalvageProps.Crate(island, new Vector3(.75f, .36f, .05f), new Vector3(1.05f, .58f, .88f), "RECLAIM\nREADY", 0f);
            foreach (float x in new[] { -2.16f, 2.16f })
                Box("WorkcellGantryPost", island, new Vector3(x, 1.63f, .83f), new Vector3(.09f, 3.26f, .09f), Steel);
            Box("WorkcellGantryBeam", island, new Vector3(0f, 3.17f, .83f), new Vector3(4.42f, .18f, .12f), Rust);
            Box("SortingCellHeader", island, new Vector3(0f, 2.84f, .78f), new Vector3(3.84f, .53f, .055f), Steel, false);
            HqSalvageProps.Label(island, "02 / PARTS SORTING", new Vector3(0f, 2.91f, .74f), .117f, new Color(.79f, .74f, .60f));
            HqSalvageProps.Label(island, "SORT   /   CLEAN   /   RECLAIM", new Vector3(0f, 2.71f, .74f), .055f, new Color(.70f, .65f, .49f));
            HqSalvageProps.Label(island, "COPPER     ELECTRICAL     SERVICEABLE PARTS", new Vector3(0f, .84f, -.882f), .066f,
                new Color(.76f, .70f, .54f));
            for (int i = 0; i < 8; i++)
                Box("EdgeWear", island, new Vector3(-2f + i * .54f, 1.134f, -.75f), new Vector3(.15f, .003f, .034f), Rust, false);
            PalletJack(parent);
        }

        private static void SortingBin(Transform parent, Vector3 floor, string label, int kind)
        {
            Transform bin = HqSalvageProps.At("OpenSortingBin_" + kind, parent, floor);
            Material paint = PolishAssets.Material("HQ_SortingBin_" + kind,
                kind == 0 ? new Color(.26f, .35f, .29f) : new Color(.29f, .34f, .38f), .91f);
            Box("BinFloor", bin, Vector3.up * .015f, new Vector3(1.25f, .03f, .84f), paint, false);
            foreach (float x in new[] { -.625f, .625f })
                Box("BinSide", bin, new Vector3(x, .17f, 0f), new Vector3(.045f, .31f, .84f), paint, false);
            Box("BinRear", bin, new Vector3(0f, .17f, .40f), new Vector3(1.25f, .31f, .045f), paint, false);
            Box("LowFrontRim", bin, new Vector3(0f, .07f, -.40f), new Vector3(1.25f, .11f, .045f), paint, false);
            Box("BinContentsLabel", bin, new Vector3(0f, .07f, -.43f), new Vector3(.74f, .08f, .012f), CustomMallArt.Material("Paper"), false);
            HqSalvageProps.Label(bin, label, new Vector3(0f, .07f, -.44f), .032f);
            if (kind == 0)
            {
                Material copper = PolishAssets.Material("HQ_ReclaimedCopper", new Color(.46f, .23f, .11f), .51f);
                for (int i = 0; i < 5; i++)
                {
                    float x = -.43f + i * .19f;
                    HqSalvageProps.Rod("CutCopperPipe", bin, new Vector3(x, .095f, -.24f),
                        new Vector3(x + .07f, .095f + i % 2 * .025f, .24f - i % 3 * .055f), .055f, copper);
                    Primitive(PrimitiveType.Sphere, "CopperFitting", bin, new Vector3(x, .075f, .22f), Vector3.one * .105f, copper, false);
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    float x = -.43f + i * .19f;
                    Box("SalvagedController", bin, new Vector3(x, .077f, -.05f + i % 2 * .14f),
                        new Vector3(.13f, .08f, .21f), Steel, false);
                    Box("CircuitContact", bin, new Vector3(x, .122f, -.05f + i % 2 * .14f),
                        new Vector3(.087f, .004f, .10f), CustomMallArt.Material("Brass"), false);
                }
            }
        }

        private static void Vice(Transform parent, Vector3 floor)
        {
            Transform vice = HqSalvageProps.At("PartsBenchVice", parent, floor, 90f);
            Box("ViceFoot", vice, Vector3.up * .05f, new Vector3(.42f, .10f, .33f), Steel, false);
            Box("CastViceBody", vice, new Vector3(0f, .18f, .01f), new Vector3(.31f, .23f, .32f), Rust, false);
            foreach (float z in new[] { -.17f, .13f })
                Box("ClampingJaw", vice, new Vector3(0f, .31f, z), new Vector3(.35f, .105f, .085f), Steel, false);
            HqSalvageProps.Rod("LeadScrew", vice, new Vector3(0f, .19f, -.16f), new Vector3(0f, .19f, -.40f), .041f, Steel);
            HqSalvageProps.Rod("ViceHandle", vice, new Vector3(-.19f, .19f, -.39f), new Vector3(.19f, .19f, -.39f), .031f, Steel);
            foreach (float x in new[] { -.19f, .19f })
                Primitive(PrimitiveType.Sphere, "HandleStop", vice, new Vector3(x, .19f, -.39f), Vector3.one * .046f, Rubber, false);
        }

        private static void PalletJack(Transform parent)
        {
            Transform jack = HqSalvageProps.At("WarehousePalletJack", parent, new Vector3(10.8f, 0f, 11.5f));
            Box("HydraulicBody", jack, new Vector3(0f, .18f, -.66f), new Vector3(.61f, .34f, .60f), Yellow);
            Box("ForkCrossmember", jack, new Vector3(0f, .17f, -.38f), new Vector3(.86f, .16f, .22f), Yellow, false);
            foreach (float x in new[] { -.31f, .31f })
            {
                Box("PalletFork", jack, new Vector3(x, .11f, .34f), new Vector3(.19f, .12f, 1.37f), Yellow, false);
                Box("ForkScuff", jack, new Vector3(x, .175f, .56f), new Vector3(.12f, .005f, .73f), Rust, false);
                Wheel(jack, new Vector3(x, .072f, .81f), .125f, .12f);
            }
            foreach (float x in new[] { -.18f, .18f }) Wheel(jack, new Vector3(x, .16f, -.79f), .31f, .11f);
            HqSalvageProps.Rod("HydraulicRam", jack, new Vector3(0f, .24f, -.73f), new Vector3(0f, .50f, -.75f), .066f, Steel);
            HqSalvageProps.Rod("SteeringMast", jack, new Vector3(0f, .40f, -.76f), new Vector3(0f, 1.22f, -.98f), .045f, Rubber);
            foreach (float x in new[] { -.21f, .21f })
                HqSalvageProps.Rod("HandleLoopSide", jack, new Vector3(x, 1.1f, -.95f), new Vector3(x, 1.42f, -1.04f), .04f, Rubber);
            HqSalvageProps.Rod("LowerHandleRail", jack, new Vector3(-.21f, 1.1f, -.95f), new Vector3(.21f, 1.1f, -.95f), .04f, Rubber);
            HqSalvageProps.Rod("HandleGrip", jack, new Vector3(-.21f, 1.42f, -1.04f), new Vector3(.21f, 1.42f, -1.04f), .044f, Rubber);
            Box("ReleaseLever", jack, new Vector3(.115f, 1.35f, -1.05f), new Vector3(.09f, .09f, .026f), Steel, false);
            HqSalvageProps.Label(jack, "DEPOT / 02", new Vector3(0f, .25f, -.969f), .035f);
        }

        private static void Wheel(Transform parent, Vector3 centre, float diameter, float width)
        {
            GameObject tyre = Primitive(PrimitiveType.Cylinder, "LoadWheel", parent, centre,
                new Vector3(diameter, width * .5f, diameter), Rubber, false);
            tyre.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            GameObject hub = Primitive(PrimitiveType.Cylinder, "WheelHub", parent, centre,
                new Vector3(diameter * .50f, width * .53f, diameter * .50f), Steel, false);
            hub.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        }
    }
}
