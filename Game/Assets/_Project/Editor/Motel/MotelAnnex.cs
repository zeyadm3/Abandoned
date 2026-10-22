using System.Collections.Generic;
using Abandoned.EditorTools;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
namespace Abandoned.Motel
{
    public static class MotelAnnex
    {
        public static void Build(Transform root, MotelKit k, MotelStructure f)
        {
            f.AddArea(new Rect(80f, 0f, 24f, 20f));
            f.AddArea(new Rect(76f, 28f, 12f, 12f));
            Transform shell = MotelKit.Group("AnnexShell", root);
            Transform front = MotelKit.Group("DinerFront", shell);
            front.position = new Vector3(92f, 0f, 0f);
            MotelShell.DoorWall(front, k, 24f, 4f);
            foreach (float x in new []
            {
                80f, 104f
            })
            {
                k.Box("DinerSideWall", shell, new Vector3(x, 1.82f, 10f), new Vector3(.2f, 3.64f, 20f), k.Wall);
            }
            k.Box("DinerRearWall", shell, new Vector3(92f, 1.82f, 20f), new Vector3(24f, 3.64f, .2f), k.Wall);
            Transform kitchen = MotelKit.Group("DinerKitchenPartition", shell);
            kitchen.position = new Vector3(92f, 0f, 12f);
            MotelShell.DoorWall(kitchen, k, 24f, 3.2f);
            // Much of the dining-room roof is gone: this detached building is open-air cover, outside lodge collapse.
            Transform roof = MotelKit.Group("DinerRoof", root.Find("Roof"));
            k.Box("KitchenRoof", roof, new Vector3(92f, 3.8f, 16f), new Vector3(24f, .2f, 8f), k.Dark);
            for (int i = 0; i <6; i++)
            {
                k.Box("ExposedRoofRib", roof, new Vector3(82f + i * 4f, 3.7f, 6f), new Vector3(.12f, .15f, 12f), k.Metal, false);
            }
            k.Box("ExposedCrossBeam", roof, new Vector3(92f, 3.7f, 6f), new Vector3(24f, .15f, .12f), k.Metal, false);
            Transform shed = MotelKit.Group("MaintenanceShed", shell);
            Transform door = MotelKit.Group("ShedFront", shed);
            door.position = new Vector3(82f, 0f, 28f);
            MotelShell.DoorWall(door, k, 12f, 3.2f);
            foreach (float x in new []
            {
                76f, 88f
            })
            {
                k.Box("ShedSide", shed, new Vector3(x, 1.82f, 34f), new Vector3(.2f, 3.64f, 12f), k.Wood);
            }
            k.Box("ShedRear", shed, new Vector3(82f, 1.82f, 40f), new Vector3(12f, 3.64f, .2f), k.Wood);
            k.Box("ShedRoof", roof, new Vector3(82f, 3.8f, 34f), new Vector3(12f, .2f, 12f), k.Dark);
            GreyboxFactory.MarkStatic(shell);
        }
        public static void Dress(Transform root, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("Diner", root);
            foreach (float z in new []
            {
                3f, 7f, 10f
            })
            {
                MotelFurniture.Table(g, k, f, new Vector3(84f, 0f, z), 1.5f, 1f);
                foreach (float x in new []
                {
                    82f, 86f
                })
                {
                    Transform booth = MotelFurniture.Prop(g, k, f, "DinerBooth", new Vector3(x, 0f, z), new Vector3(.8f, 1.1f, 2f));
                    k.Box("Seat", booth, new Vector3(0f, .35f, 0f), new Vector3(.8f, .6f, 2f), k.Accent, false);
                    k.Box("Back", booth, new Vector3(x <84f ? - .3f : .3f, .75f, 0f), new Vector3(.15f, .7f, 2f), k.Cloth, false);
                }
            }
            Transform counter = MotelFurniture.Prop(g, k, f, "DinerServiceCounter", new Vector3(100f, 0f, 6f), new Vector3(1.2f, 1.1f, 9f));
            k.Box("Front", counter, new Vector3(0f, .55f, 0f), new Vector3(1.2f, 1.1f, 9f), k.Accent, false);
            k.Box("Top", counter, new Vector3(0f, 1.15f, 0f), new Vector3(1.3f, .1f, 9.1f), k.Metal, false);
            foreach (float z in new []
            {
                3f, 6f, 9f
            })
            {
                MotelFurniture.Chair(g, k, f, new Vector3(98f, 0f, z), 90f);
            }
            MotelFurniture.Shelf(g, k, f, new Vector3(85f, 0f, 19f), 6f);
            Transform range = MotelFurniture.Prop(g, k, f, "BoltedDinerRange", new Vector3(102f, 0f, 16f), new Vector3(1f, 1f, 5f));
            k.Box("Body", range, new Vector3(0f, .5f, 0f), new Vector3(1f, 1f, 5f), k.Metal, false);
            for (int i = 0; i <6; i++)
            {
                k.Box("Burner", range, new Vector3(0f, 1.01f, - 2f + i * .8f), new Vector3(.6f, .03f, .6f), k.Dark, false);
            }
            MotelFurniture.Table(g, k, f, new Vector3(95f, 0f, 18f), 3f, 1f);
            MotelHiding.Build(g, k, new Vector3(81f, 0f, 14f), 90f, 112, f.At(new Vector3(81f, 0f, 14f)), HideSpotKind.Cabinet);
            foreach (Vector3 at in new []
            {
                new Vector3(88f, .05f, 3f), new Vector3(88f, .05f, 8f), new Vector3(95f, .05f, 3f), new Vector3(95f, .05f, 9f), new Vector3(88f, .05f, 16f), new Vector3(98f, .05f, 16f)
            })
            {
                MotelAmenities.Loot(loot, "motel_diner", at, CarryClass.Huge);
            }
            Maintenance(root, k, f, loot);
        }
        private static void Maintenance(Transform parent, MotelKit k, MotelStructure f, List<LootPointSpec> loot)
        {
            Transform g = MotelKit.Group("Maintenance", parent);
            MotelFurniture.Table(g, k, f, new Vector3(82f, 0f, 39f), 8f, .9f);
            MotelFurniture.Shelf(g, k, f, new Vector3(87f, 0f, 37f), 1.3f);
            Transform tools = MotelKit.Group("PegBoard", g);
            tools.position = new Vector3(82f, 1.8f, 39.65f);
            k.Box("Board", tools, Vector3.zero, new Vector3(7f, 1.2f, .06f), k.Wood, false);
            for (int i = 0; i <12; i++)
            {
                k.Box("FixedToolBracket", tools, new Vector3(- 3f + i * .5f, .2f - i % 3 * .2f, - .06f), new Vector3(.06f, .4f, .06f), k.Metal, false);
            }
            MotelHiding.Build(g, k, new Vector3(76.5f, 0f, 37f), 90f, 120, f.At(new Vector3(76.5f, 0f, 37f)), HideSpotKind.Locker);
            MotelHiding.Build(g, k, new Vector3(87.5f, 0f, 30.5f), 270f, 121, f.At(new Vector3(87.5f, 0f, 30.5f)), HideSpotKind.Cabinet);
            MotelAmenities.Loot(loot, "motel_maintenance", new Vector3(80f, .05f, 33f), CarryClass.Huge);
            MotelAmenities.Loot(loot, "motel_maintenance", new Vector3(84f, .05f, 36f));
            MotelAmenities.Loot(loot, "motel_maintenance", new Vector3(85f, .05f, 32.5f));
        }
    }
}
