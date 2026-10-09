using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Receiving stock, recovery work and tall stock aisles support a clear company workflow.</summary>
    public static class HqSalvageDressing
    {
        public static void Build(Transform parent)
        {
            Transform stock = Group("WarehouseStockAisles", parent);
            StockAisles(stock);
            ReceivingStock(stock);
            ElectronicRack(stock, new Vector3(1.15f, 0f, 13.65f), 270f, "ELECTRONICS 04\nNO MAINS POWER");
            ElectronicRack(stock, new Vector3(1.15f, 0f, 15.25f), 270f, "RECOVERED RACK\nSTRIP FOR PARTS");
            Drum(stock, new Vector3(1.0f, 0f, 9.9f), "USED OIL\nDO NOT MIX", -8f);
            Drum(stock, new Vector3(2.0f, 0f, 10.2f), "MACHINE\nLUBRICANT", 11f);
            Drum(stock, new Vector3(2.9f, 0f, 9.75f), "EMPTY\nRETURN", -15f);
            CopperSalvage(stock);
            HqWarehouseZones.Build(parent);
            HqMaintenanceDressing.Build(parent);
            HqLogisticsBoard.Build(parent);
            MarkStatic(stock);
        }

        private static void StockAisles(Transform stock)
        {
            Material metal = CustomMallArt.Material("Metal"), rust = CustomMallArt.Material("Rust");
            for (int row = 0; row < 2; row++)
            for (int bay = 0; bay < 2; bay++)
            {
                float x = row == 0 ? 4.5f : 11.8f;
                float yaw = row == 0 ? 270f : 90f;
                Transform shelf = Shelf(stock, new Vector3(x, 0f, 19.7f + bay * 3.5f), 3f, 4.05f, 1.1f, yaw);
                foreach (float y in new[] { 1.075f, 2.965f })
                {
                    Box("IntermediateStockDeck", shelf, new Vector3(0f, y, 0f), new Vector3(3f, .065f, 1.1f), metal);
                    Box("StockDeckLip", shelf, new Vector3(0f, y, -.55f), new Vector3(3f, .12f, .045f), rust, false);
                }
                string[] grades = row == 0 ? new[] { "MOTORS", "VALVES", "PUMPS", "BRASS", "FITTINGS" }
                    : new[] { "ELECTRICAL", "CERAMICS", "CONNECTORS", "RELAYS", "SPARES" };
                for (int level = 0; level < 5; level++)
                {
                    float floor = .17f + level * .945f;
                    if ((level + row + bay) % 2 == 0)
                    {
                        Crate(shelf, new Vector3(-.81f, floor, .06f), new Vector3(.9f, .59f, .83f), grades[level] + "\nLOT " + (21 + bay + row * 3), -2f);
                        Crate(shelf, new Vector3(.28f, floor, .05f), new Vector3(.93f, .62f, .82f), "RECOVERED\nCHECKED", 2f);
                        Bin(shelf, new Vector3(1.10f, floor, -.1f), "SMALL PARTS", level % 3);
                    }
                    else
                        for (int bin = 0; bin < 4; bin++)
                            Bin(shelf, new Vector3(-1.03f + bin * .69f, floor, -.06f), grades[level], (row + bin) % 3, bin % 2 == 0 ? -3f : 2f);
                }
                for (int label = 0; label < 3; label++)
                {
                    Vector3 at = new Vector3(-1f + label, 2.86f, -.588f);
                    Box("BayBarcode", shelf, at, new Vector3(.42f, .11f, .015f), CustomMallArt.Material("Paper"), false);
                    Label(shelf, "STK " + (row + 1) + "-" + (bay + 1) + "-" + label, at + Vector3.back * .012f, .026f);
                }
            }
            Pallet(stock, new Vector3(4.1f, 0f, 26.5f), new Vector2(3.3f, 1.38f));
            Crate(stock, new Vector3(3.1f, .16f, 26.5f), new Vector3(1.16f, .87f, .93f), "LARGE MOTORS\nTWO PERSON LIFT", -2f);
            Crate(stock, new Vector3(4.45f, .16f, 26.5f), new Vector3(1.16f, .87f, .93f), "PUMP ASSEMBLY", 2f);
            Crate(stock, new Vector3(4.45f, 1.06f, 26.5f), new Vector3(1.02f, .68f, .85f), "SPARE CASINGS", -3f);
        }

        private static void ReceivingStock(Transform stock)
        {
            Pallet(stock, new Vector3(12.7f, 0f, 7.5f), new Vector2(1.55f, 1.4f));
            Crate(stock, new Vector3(12.7f, .16f, 7.5f), new Vector3(1.12f, .76f, 1.0f), "INCOMING\nAWAITING WEIGHT", -4f);
            Bin(stock, new Vector3(12.71f, .95f, 7.48f), "UNSORTED", 2, 2f);
            Crate(stock, new Vector3(3.25f, 0f, 13.35f), new Vector3(1.1f, .68f, .93f), "E-WASTE\nRECOVERY", 3f);
            Bin(stock, new Vector3(3.25f, .71f, 13.35f), "CIRCUIT BOARDS", 0);
            Crate(stock, new Vector3(4.7f, 0f, 14.1f), new Vector3(1.05f, .72f, .9f), "CABLE / TEST FIRST", -6f);
            Bin(stock, new Vector3(4.69f, .76f, 14.1f), "LOW VOLTAGE", 1);

            Transform workshop = Shelf(stock, new Vector3(28.55f, 0f, 20.35f), 1.75f, 2.2f, .7f);
            Bin(workshop, new Vector3(-.45f, .17f, 0f), "BENCH SPARES", 1);
            Bin(workshop, new Vector3(.4f, .17f, 0f), "FASTENERS", 0);
            Crate(workshop, new Vector3(-.25f, 1.14f, .04f), new Vector3(1.03f, .58f, .58f), "REPAIR QUEUE");
            Coil(workshop, new Vector3(.42f, 2.14f, -.03f), .21f, CustomMallArt.Material("Rubber"));
        }

        private static void ElectronicRack(Transform stock, Vector3 floor, float yaw, string status)
        {
            Transform rack = At("DecommissionedElectronicsRack", stock, floor, yaw);
            Material metal = CustomMallArt.Material("Metal"), black = CustomMallArt.Material("Rubber"), rust = CustomMallArt.Material("Rust");
            Box("CabinetBase", rack, new Vector3(0f, .1f, 0f), new Vector3(.98f, .2f, .7f), rust);
            foreach (float x in new[] { -.45f, .45f })
            {
                Box("RackRail", rack, new Vector3(x, 1.31f, -.26f), new Vector3(.09f, 2.42f, .1f), metal);
                Box("CabinetSide", rack, new Vector3(x, 1.31f, .11f), new Vector3(.065f, 2.42f, .64f), rust, false);
            }
            Box("CabinetTop", rack, new Vector3(0f, 2.55f, .04f), new Vector3(.98f, .075f, .7f), metal, false);
            Box("CabinetBack", rack, new Vector3(0f, 1.34f, .38f), new Vector3(.92f, 2.47f, .055f), black, false);
            for (int i = 0; i < 5; i++)
            {
                float y = .35f + i * .38f;
                Box("SalvagedAmplifier", rack, new Vector3(0f, y, .03f), new Vector3(.79f, .25f, .58f), metal);
                Box("DeadFaceplate", rack, new Vector3(0f, y, -.279f), new Vector3(.84f, .23f, .035f), black, false);
                for (int vent = 0; vent < 7; vent++)
                    Box("VentSlot", rack, new Vector3(-.26f + vent * .043f, y, -.3f), new Vector3(.014f, .13f, .005f), rust, false);
                Rod("ControlKnob", rack, new Vector3(.25f, y, -.295f), new Vector3(.25f, y, -.323f), .066f, metal);
                Box("RemovedSerialPlate", rack, new Vector3(.12f, y + .065f, -.302f), new Vector3(.09f, .035f, .008f), CustomMallArt.Material("Paper"), false);
            }
            Box("LockoutTag", rack, new Vector3(0f, 2.18f, -.35f), new Vector3(.63f, .29f, .018f), CustomMallArt.Material("Paper"), false);
            Label(rack, status, new Vector3(0f, 2.18f, -.364f), .034f);
            for (int i = 0; i < 3; i++)
            {
                Vector3 start = new Vector3(.30f - i * .09f, .23f, -.29f);
                Vector3 loose = new Vector3(.35f - i * .13f, .10f, -.54f);
                Rod("UnpluggedCable", rack, start, loose, .019f, black);
                Rod("CableTail", rack, loose, loose + new Vector3(-.17f, -.025f, -.12f), .019f, black);
            }
        }

        private static void CopperSalvage(Transform stock)
        {
            Material copper = PolishAssets.Material("HQ_ReclaimedCopper", new Color(.47f, .28f, .17f), .62f);
            copper.SetFloat("_Metallic", .72f);
            Transform bundle = At("LooseCopperPipeBundle", stock, new Vector3(2.4f, .13f, 16.5f));
            for (int i = 0; i < 7; i++)
            {
                float z = (i % 4 - 1.5f) * .093f;
                float y = i / 4 * .08f;
                float length = 1.9f - (i % 3) * .17f;
                Rod("CopperPipe", bundle, new Vector3(-length * .5f, y, z), new Vector3(length * .5f, y, z), .071f, copper);
                Rod("OxidisedJoint", bundle, new Vector3(.48f, y, z), new Vector3(.55f, y, z), .092f, CustomMallArt.Material("Rust"));
            }
            foreach (float x in new[] { -.52f, .53f })
                Box("BundleTie", bundle, new Vector3(x, .027f, 0f), new Vector3(.03f, .19f, .42f), CustomMallArt.Material("Rubber"), false);
            Box("PipeSortTag", bundle, new Vector3(0f, .21f, -.2f), new Vector3(.55f, .19f, .01f), CustomMallArt.Material("Paper"), false);
            Label(bundle, "COPPER / NO STEEL", new Vector3(0f, .21f, -.211f), .029f);
            Coil(stock, new Vector3(4.7f, 1.25f, 14.1f), .21f, copper);
        }

        private static void Pallet(Transform parent, Vector3 floor, Vector2 size)
        {
            Transform pallet = At("RecoveryPallet", parent, floor);
            Material wood = CustomMallArt.Material("Wood");
            for (int i = 0; i < 6; i++)
                Box("DeckBoard", pallet, new Vector3(-size.x * .5f + size.x / 12f + i * size.x / 6f, .12f, 0f),
                    new Vector3(size.x / 6f - .04f, .05f, size.y), wood, false);
            foreach (float z in new[] { -size.y * .34f, 0f, size.y * .34f })
                Box("PalletBearer", pallet, new Vector3(0f, .05f, z), new Vector3(size.x, .1f, .16f), wood, false);
        }

        private static void Coil(Transform parent, Vector3 position, float radius, Material material)
        {
            Transform coil = At("SalvagedCableCoil", parent, position);
            for (int turn = 0; turn < 3; turn++)
            for (int segment = 0; segment < 12; segment++)
            {
                float a = segment * Mathf.PI / 6f, b = (segment + 1) * Mathf.PI / 6f;
                Vector3 from = new Vector3(Mathf.Cos(a) * radius, turn * .025f, Mathf.Sin(a) * radius);
                Vector3 to = new Vector3(Mathf.Cos(b) * radius, turn * .025f, Mathf.Sin(b) * radius);
                Rod("CableLoop", coil, from, to, .022f, material);
            }
            Rod("LooseCableEnd", coil, new Vector3(radius, .065f, 0f), new Vector3(radius + .16f, .035f, -.14f), .022f, material);
        }
    }
}
