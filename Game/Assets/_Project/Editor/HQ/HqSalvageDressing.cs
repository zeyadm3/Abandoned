using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.HqSalvageProps;

namespace Abandoned.EditorTools
{
    /// <summary>Salvage stock lives in perimeter pockets, with clear approaches to every working station.</summary>
    public static class HqSalvageDressing
    {
        public static void Build(Transform parent)
        {
            Transform stock = Group("SalvageWarehouseStock", parent);
            CrateStacks(stock);
            SortingShelves(stock);
            ElectronicRack(stock, new Vector3(10.7f, 0f, 8.7f), 90f, "DECOMMISSIONED\nNO MAINS POWER");
            ElectronicRack(stock, new Vector3(13.2f, 0f, 15.1f), 0f, "RECOVERED RACK 04\nSTRIP FOR PARTS");
            Drum(stock, new Vector3(9.48f, 0f, 14.65f), "USED OIL\nDO NOT MIX", -8f);
            Drum(stock, new Vector3(10.25f, 0f, 15.0f), "MACHINE\nLUBRICANT", 11f);
            Drum(stock, new Vector3(10.25f, 0f, 14.1f), "EMPTY\nRETURN", -15f);
            CopperSalvage(stock);
            HqMaintenanceDressing.Build(parent);
            HqLogisticsBoard.Build(parent);
            MarkStatic(stock);
        }

        private static void CrateStacks(Transform stock)
        {
            Pallet(stock, new Vector3(2.15f, 0f, 15.13f), new Vector2(2.75f, 1.28f));
            Crate(stock, new Vector3(1.48f, .16f, 15.2f), new Vector3(1.05f, .82f, .86f), "MOTOR PARTS\nLOT 017", -3f);
            Crate(stock, new Vector3(1.48f, .995f, 15.2f), new Vector3(1.02f, .75f, .82f), "FRAGILE\nSALVAGE", 4f);
            Crate(stock, new Vector3(1.48f, 1.78f, 15.2f), new Vector3(.93f, .55f, .75f), "COPPER\nKEEP DRY", -5f);
            Crate(stock, new Vector3(2.68f, .16f, 15.15f), new Vector3(1.13f, .74f, .9f), "ELECTRICAL\nUNSORTED", 2f);
            Bin(stock, new Vector3(2.64f, .93f, 15.13f), "FUSES / RELAYS", 1, -4f);
            Bin(stock, new Vector3(2.64f, 1.42f, 15.13f), "FASTENERS", 0, 2f);
            Crate(stock, new Vector3(2.45f, 0f, 13.91f), new Vector3(1.12f, .68f, .84f), "TO BE SORTED\nCREW RETURN", -9f);

            Crate(stock, new Vector3(13.24f, 0f, 1.08f), new Vector3(1.1f, .76f, .92f), "ARCHIVE\nJOB LOGS", 7f);
            Bin(stock, new Vector3(13.24f, .79f, 1.08f), "PAPERWORK", 2, -4f);
            Crate(stock, new Vector3(22.79f, 0f, 8.87f), new Vector3(1.22f, .84f, .91f), "RECOVERED\nDO NOT DISCARD", -4f);
            Crate(stock, new Vector3(22.78f, .87f, 8.87f), new Vector3(1.03f, .68f, .85f), "SPARE PUMPS", 3f);
            Bin(stock, new Vector3(21.65f, 0f, 9.0f), "CABLE OFFCUTS", 1, -8f);
            Crate(stock, new Vector3(.85f, 0f, .98f), new Vector3(.94f, .72f, .93f), "RETURNS\nHOLD", 6f);
        }

        private static void SortingShelves(Transform stock)
        {
            Transform shelf = Shelf(stock, new Vector3(21.5f, 0f, 15.38f), 3.4f, 2.42f, .76f);
            string[] names = { "BRASS", "SWITCHES", "CABLE", "TOOLS", "SCRAP IRON", "CERAMICS", "SMALL MOTORS", "SPARES" };
            for (int i = 0; i < names.Length; i++)
            {
                int level = i / 3;
                Bin(shelf, new Vector3(-1.04f + i % 3 * 1.0f, .17f + level * 1.075f, 0f), names[i], i % 3, i % 2 == 0 ? -3f : 2f);
            }
            Box("ShelfHeader", shelf, new Vector3(0f, 2.63f, .34f), new Vector3(2.65f, .24f, .035f), CustomMallArt.Material("Paper"), false);
            Label(shelf, "COMPANY STOCK / SORT BEFORE STOWING", new Vector3(0f, 2.63f, .313f), .049f);

            Transform shelf2 = Shelf(stock, new Vector3(14.25f, 0f, 8.98f), 2.55f, 1.88f, .7f);
            Bin(shelf2, new Vector3(-.79f, .17f, 0f), "RECLAIMED WIRE", 0);
            Bin(shelf2, new Vector3(0f, .17f, 0f), "CONNECTORS", 1);
            Crate(shelf2, new Vector3(.65f, .17f, 0f), new Vector3(.66f, .5f, .57f), "VALVES", 0f);
            Bin(shelf2, new Vector3(-.7f, .98f, 0f), "WEIGHED / LOGGED", 2);
            Bin(shelf2, new Vector3(.05f, .98f, 0f), "CHECK AGAIN", 0);
            Coil(shelf2, new Vector3(.81f, 1.03f, -.04f), .19f, CustomMallArt.Material("Rubber"));
            Label(shelf2, "INCOMING / AWAITING SORT", new Vector3(0f, 2.1f, -.14f), .052f, new Color(.63f, .59f, .43f));
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
            Transform bundle = At("LooseCopperPipeBundle", stock, new Vector3(10.15f, .13f, 15.64f));
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

            Transform offcuts = At("CopperOffcuts", stock, new Vector3(.36f, .065f, 1.6f), 4f);
            for (int i = 0; i < 4; i++)
            {
                Vector3 from = new Vector3(i * .12f, i % 2 * .06f, -.5f);
                Vector3 to = from + new Vector3(0f, .012f, .82f + i * .16f);
                Rod("LoosePipe", offcuts, from, to, .068f, copper);
                Rod("BentElbow", offcuts, to, to + new Vector3(.15f, 0f, .03f), .068f, copper);
            }
            Coil(stock, new Vector3(13.25f, 1.29f, 1.11f), .21f, copper);
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
