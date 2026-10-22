using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelAmenities
    {
        public static void Dress(Transform root, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Office(root, k, f, loot);
            Laundry(root, k, f, loot);
            Manager(root, k, f, loot);
            Alcoves(root, k, f, loot);
        }
        private static void Office(Transform p, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("FrontOffice", p);
            Transform counter = MotelFurniture.Prop(g, k, f, "ReceptionCounter", new Vector3(29.1f, 0f, 17.5f), new Vector3(1.7f, 1.1f, .7f));
            k.Box("Front", counter, new Vector3(0f, .55f, 0f), new Vector3(1.7f, 1.1f, .7f), k.Accent, false);
            k.Box("Top", counter, new Vector3(0f, 1.13f, 0f), new Vector3(1.8f, .06f, .8f), k.Wood, false);
            Transform back = MotelKit.Group("OfficeBackRoomWall", g);
            back.position = new Vector3(30f, 0f, 19.5f);
            MotelShell.DoorWall(back, k, 4f, 1.8f);
            k.Box("BackRoomSide", g, new Vector3(32f, 1.82f, 20.75f), new Vector3(.15f, 3.64f, 2.5f), k.Wall);
            Transform keys = MotelKit.Group("EmptyKeyBoard", g);
            keys.position = new Vector3(28.22f, 1.7f, 20.7f);
            k.Box("Board", keys, Vector3.zero, new Vector3(.1f, 1.1f, 1.5f), k.Wood, false);
            for (int i = 0; i <12; i++)
            {
                k.Box("EmptyHook", keys, new Vector3(.08f, - .35f + i / 4 * .35f, - .55f + i % 4 * .35f), new Vector3(.08f, .025f, .025f), k.Metal, false);
            }
            MotelHiding.Build(g, k, new Vector3(28.4f, 0f, 21.25f), 90f, 114, f.At(new Vector3(28.4f, 0f, 21.25f)), HideSpotKind.Cabinet);
            for (int i = 0; i <3; i++)
            {
                Loot(loot, "motel_office", new Vector3(34f, .05f, 17.5f + i * 1.3f));
            }
            Loot(loot, "motel_office", new Vector3(31f, .05f, 20.4f));
        }
        private static void Laundry(Transform p, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("Laundry", p);
            Transform bank = MotelFurniture.Prop(g, k, f, "BoltedLaundryBank", new Vector3(29.3f, 0f, 25.5f), new Vector3(1.2f, 1.2f, 3.6f));
            k.Box("Bank", bank, new Vector3(0f, .6f, 0f), new Vector3(1.2f, 1.2f, 3.6f), k.Wall, false);
            for (int i = 0; i <3; i++)
            {
                k.Box("WasherDoor", bank, new Vector3(.62f, .6f, - 1.2f + i * 1.2f), new Vector3(.04f, .7f, .7f), k.Glass, false);
                k.Box("CoinSlot", bank, new Vector3(.65f, 1.04f, - .9f + i * 1.2f), new Vector3(.04f, .12f, .05f), k.Dark, false);
            }
            for (int i = 0; i <4; i++)
            {
                Loot(loot, "motel_laundry", new Vector3(34f, .05f, 23.1f + i));
            }
            loot.Add(new LootPointSpec("Motel_WasherPrize", "motel_laundry_prize", new Vector3(32f, .05f, 26f), CarryClass.Huge, CarryClass.Huge, true));
            // The high cabinet stays beside the wall, outside both the washer bay and the rear exit.
            MotelHiding.Build(g, k, new Vector3(35.6f, 0f, 27f), 270f, 116, f.At(new Vector3(35.6f, 0f, 27f)), HideSpotKind.Locker);
        }
        private static void Manager(Transform p, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("ManagerFlat", p);
            Transform wall = MotelKit.Group("BedroomPartition", g);
            wall.position = new Vector3(32f, 4f, 24f);
            MotelShell.DoorWall(wall, k, 8f, 2.4f);
            Transform kitchen = MotelFurniture.Prop(g, k, f, "ManagerKitchen", new Vector3(29.2f, 4f, 20.5f), new Vector3(1.2f, 1f, 3f));
            k.Box("Cabinets", kitchen, new Vector3(0f, .5f, 0f), new Vector3(1.2f, 1f, 3f), k.Wood, false);
            k.Box("Worktop", kitchen, new Vector3(0f, 1.05f, 0f), new Vector3(1.25f, .1f, 3.05f), k.Metal, false);
            Transform bed = MotelFurniture.Prop(g, k, f, "ManagerBed", new Vector3(29.5f, 4f, 26f), new Vector3(1.4f, .8f, 2.1f));
            k.Box("BedFrame", bed, new Vector3(0f, .3f, 0f), new Vector3(1.4f, .6f, 2.1f), k.Wood, false);
            k.Box("Mattress", bed, new Vector3(0f, .7f, 0f), new Vector3(1.35f, .2f, 2f), k.Cloth, false);
            Transform sofa = MotelFurniture.Prop(g, k, f, "ManagerSofa", new Vector3(34.2f, 4f, 21.4f), new Vector3(2f, .85f, .9f));
            k.Box("Cushions", sofa, new Vector3(0f, .4f, 0f), new Vector3(2f, .7f, .9f), k.Cloth, false);
            k.Box("Back", sofa, new Vector3(0f, .65f, .4f), new Vector3(2f, .4f, .12f), k.Accent, false);
            MotelHiding.Build(g, k, new Vector3(35f, 4f, 26f), 270f, 115, f.At(new Vector3(35f, 4f, 26f)));
            Loot(loot, "motel_office", new Vector3(34f, 4.05f, 18.2f));
            Loot(loot, "motel_guest", new Vector3(31.1f, 4.05f, 26.6f));
            Loot(loot, "motel_guest", new Vector3(32.4f, 4.05f, 22.4f));
        }
        private static void Alcoves(Transform p, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("IceVendingAlcoves", p);
            foreach (float x in new []
            {
                6f, 58f
            })
            {
                MotelFurniture.Machine(g, k, f, "IceDispenser", new Vector3(x, 0f, 11.25f), false);
                MotelFurniture.Machine(g, k, f, "VendingMachine", new Vector3(x + 2f, 0f, 11.25f), true);
                k.Box("AlcoveHood", g, new Vector3(x + 1f, 2.4f, 11.2f), new Vector3(4f, .15f, 1.6f), k.Accent, false);
                Loot(loot, "motel_diner", new Vector3(x + 4f, .05f, 9.5f));
            }
            MotelHiding.Build(g, k, new Vector3(11f, 0f, 11.2f), 180f, 117, null, HideSpotKind.Cabinet);
            Loot(loot, "motel_pool", new Vector3(12f, .05f, 5f));
            Loot(loot, "motel_pool", new Vector3(24.5f, .05f, 7f));
        }
        public static void Loot(List<LootPointSpec> points, string tag, Vector3 at, CarryClass max = CarryClass.Heavy)
        {
            points.Add(new LootPointSpec("Motel_Amenity_" + points.Count, tag, at, CarryClass.Pocket, max));
        }
    }
}
