using System.Collections.Generic;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Under the dining band, 4 m down the stair in the dining hall's corner: the laundry (drum washers, a mangle,
    /// folding tables, canvas carts big enough to climb into), the showers (tiled, a pipe rack of heads, a drain
    /// channel) and the boiler room (a dead boiler, hot-water tanks, a fitter's bench). Wet, low and dark: the floor
    /// above can fall in. The laundry carts are hiding places.
    /// </summary>
    public static class IslandBasement
    {
        private const float Y = L.BasementY;

        public static IslandFlights.Built Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Transform g = Group("BasementRooms", root);
            IslandFlights.Built stair = IslandFlights.Flight(Group("BasementStair", root), L.BasementStair, art.Plate, art.Railing, true, true);
            Laundry(Group("Laundry", g), art, loot);
            Showers(Group("Showers", g), art, loot);
            Boiler(Group("Boiler", g), art, loot);
            IslandBasementDetails.Build(Group("IndustrialFinish", g), art);
            MarkStatic(g);
            Carts(Group("LaundryCarts", root), art);
            return stair;
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float z, CarryClass min, CarryClass max, float y = Y + .05f) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, false));

        private static void Laundry(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float z1 = L.Cellhouse.yMax - .5f;
            // The banks sit on isolated plinths; a fifth washer has been pulled out for a repair that never finished.
            for (int i = 0; i < 4; i++)
            {
                float x = 40.5f + i * 2.4f;
                Box("WasherPlinth", g, new Vector3(x, Y + .08f, z1 - .8f), new Vector3(2.06f, .16f, 1.54f), art.FacadeDark);
                Box("WasherBody", g, new Vector3(x, Y + .8f, z1 - .8f), new Vector3(1.9f, 1.6f, 1.4f), art.Steel);
                GameObject drum = Primitive(PrimitiveType.Cylinder, "Drum", g, new Vector3(x, Y + .82f, z1 - 1.52f), new Vector3(1.1f, .04f, 1.1f), art.Bars, false);
                drum.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                IslandBasementDetails.Washer(g, art, x, z1 - 1.57f, i);
            }
            K.RotBox("WasherDown", g, new Vector3(48.7f, Y + .98f, 88.7f), new Vector3(1.9f, 1.4f, 1.6f), Quaternion.Euler(0f, 20f, 90f), art.Steel);
            // The mangle: a long roller press against the partition.
            Box("MangleFrame", g, new Vector3(48.6f, Y + .5f, 82.5f), new Vector3(1f, 1f, 3f), art.Steel);
            GameObject roller = Primitive(PrimitiveType.Cylinder, "MangleRoll", g, new Vector3(48.6f, Y + 1.15f, 82.5f), new Vector3(.5f, 1.5f, .5f), art.Bars, false);
            roller.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            // Folding tables with stacked grey linen.
            foreach (float x in new[] { 41f, 44.5f })
            {
                Box("FoldTable", g, new Vector3(x, Y + .88f, 82.2f), new Vector3(2.4f, .08f, 1f), art.Board);
                foreach (float side in new[] { -1f, 1f })
                foreach (float end in new[] { -1f, 1f })
                    Box("TableLeg", g, new Vector3(x + side * 1.02f, Y + .42f, 82.2f + end * .35f), new Vector3(.08f, .84f, .08f), art.Steel);
                Box("LowerShelf", g, new Vector3(x, Y + .18f, 82.2f), new Vector3(2.2f, .05f, .76f), art.Steel, false);
                for (int i = 0; i < 3; i++) Box("Linen", g, new Vector3(x - .7f + i * .7f, Y + .98f + i * .02f, 82.2f), new Vector3(.5f, .12f + i * .04f, .7f), art.Blanket, false);
            }
            Point(loot, "Laundry_Table", IslandLootItems.Contraband, 44.5f, 82.2f, CarryClass.Pocket, CarryClass.OneHand, Y + 1.1f);
            Point(loot, "Laundry_Floor", IslandLootItems.Contraband, 45f, 86f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Laundry_Corner", IslandLootItems.Armoury, 39.5f, 81.4f, CarryClass.Pocket, CarryClass.TwoHand);
        }

        private static void Showers(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x0 = L.BasementPartitions[0] + .15f, x1 = L.BasementPartitions[1] - .15f, z0 = L.NorthBand.yMin + .2f, z1 = L.Cellhouse.yMax - .5f;
            // Tile skin on every wall, a pipe rack of heads down the middle, benches, a drain channel.
            K.Span("Tile_N", g, new Vector3(x0, Y, z1 - .02f), new Vector3(x1, Y + 2.4f, z1), art.Tile, false);
            // The tile skins must repeat the masonry openings: a continuous thin wall hid both usable doorways.
            K.WallZ(g, "Tile_W", z0, z1, x0 + .01f, Y, 2.4f, .02f, art.Tile, K.Opening.Door(L.BasementDoorZ, 2.4f, 2.6f));
            K.WallZ(g, "Tile_E", z0, z1, x1 - .01f, Y, 2.4f, .02f, art.Tile, K.Opening.Door(L.BasementDoorZ, 2.4f, 2.6f));
            K.Span("Tile_S", g, new Vector3(x0, Y, z0), new Vector3(x1, Y + 2.4f, z0 + .02f), art.Tile, false);
            K.Span("TileFloor", g, new Vector3(x0, Y, z0), new Vector3(x1, Y + .04f, z1), art.Tile);
            K.Span("Drain", g, new Vector3(x0 + 1f, Y + .045f, 86.85f), new Vector3(x1 - 1f, Y + .06f, 87.12f), art.Bars, false);
            K.Span("PipeRack", g, new Vector3(x0 + 2f, Y + 2.6f, 88.4f), new Vector3(x1 - 2f, Y + 2.68f, 88.5f), art.Rust, false);
            for (float x = x0 + 2.5f; x < x1 - 2f; x += 1.4f)
            {
                Box("Drop", g, new Vector3(x, Y + 2.4f, 88.45f), new Vector3(.04f, .4f, .04f), art.Rust, false);
                Box("Head", g, new Vector3(x, Y + 2.2f, 88.3f), new Vector3(.16f, .045f, .2f), art.Steel, false);
            }
            Box("Bench_S", g, new Vector3((x0 + x1) * .5f, Y + .46f, 81.6f), new Vector3(x1 - x0 - 3f, .08f, .42f), art.Board);
            for (float x = x0 + 2f; x < x1 - 1f; x += 2.6f)
                Box("BenchLeg", g, new Vector3(x, Y + .23f, 81.6f), new Vector3(.1f, .46f, .36f), art.Steel);
            Point(loot, "Showers_Bench", IslandLootItems.Contraband, 57f, 81.6f, CarryClass.Pocket, CarryClass.OneHand, Y + .55f);
            Point(loot, "Showers_Floor", IslandLootItems.Maritime, 60f, 89.6f, CarryClass.Pocket, CarryClass.TwoHand);
        }

        private static void Boiler(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x0 = L.BasementPartitions[1] + .15f, x1 = L.Cellhouse.xMax - .5f, z1 = L.Cellhouse.yMax - .5f;
            // The boiler: a riveted drum on a brick plinth, its flue up through the slab.
            Box("Plinth", g, new Vector3(73.5f, Y + .3f, 88.2f), new Vector3(4.2f, .6f, 2.8f), art.Brick);
            GameObject drum = Primitive(PrimitiveType.Cylinder, "BoilerDrum", g, new Vector3(73.5f, Y + 1.7f, 88.2f), new Vector3(2.2f, 1.9f, 2.2f), art.Steel);
            drum.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Box("Firebox", g, new Vector3(71.4f, Y + .9f, 88.2f), new Vector3(.4f, .9f, 1.2f), art.Bars, false);
            Primitive(PrimitiveType.Cylinder, "Flue", g, new Vector3(75f, Y + 3.2f, 88.6f), new Vector3(.5f, .9f, .5f), art.Rust, false);
            foreach (float x in new[] { 68.5f, 70f })
                Primitive(PrimitiveType.Cylinder, "Tank", g, new Vector3(x, Y + 1.2f, 90.4f), new Vector3(1.2f, 1.2f, 1.2f), art.Rust);
            // A real open-framed bench leaves its lower shelf and foot space visible.
            Box("Bench", g, new Vector3(x1 - .5f, Y + .9f, 82.8f), new Vector3(.9f, .1f, 2.6f), art.Board);
            foreach (float z in new[] { 81.7f, 83.9f })
            foreach (float x in new[] { x1 - .82f, x1 - .18f })
                Box("BenchLeg", g, new Vector3(x, Y + .45f, z), new Vector3(.08f, .9f, .08f), art.Steel);
            Box("LowerShelf", g, new Vector3(x1 - .5f, Y + .2f, 82.8f), new Vector3(.8f, .05f, 2.5f), art.Steel, false);
            Box("Vice", g, new Vector3(x1 - .5f, Y + 1f, 82f), new Vector3(.2f, .2f, .3f), art.Steel, false);
            for (int i = 0; i < 3; i++)
                Box("Shelf", g, new Vector3(x0 + .3f, Y + .5f + i * .7f, 82.5f), new Vector3(.5f, .04f, 3.4f), art.Steel, false);
            Point(loot, "Boiler_Bench", "workshop", x1 - .5f, 83.6f, CarryClass.Pocket, CarryClass.TwoHand, Y + .95f);
            Point(loot, "Boiler_Shelves", "workshop", x0 + .9f, 82.5f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Boiler_Floor", IslandLootItems.Armoury, 70.5f, 84f, CarryClass.OneHand, CarryClass.Heavy);
        }

        // Canvas laundry carts on steel frames (1.25 x 1.8 x 1), big enough to climb into; the front flap is the door.
        private static void Carts(Transform g, IslandPrisonArt art)
        {
            var spots = new[]
            {
                (new Vector3(31.6f, Y, 82f), Vector3.right), (new Vector3(37.5f, Y, 90.6f), Vector3.back),
                (new Vector3(52f, Y, 90.8f), Vector3.back), (new Vector3(36f, Y, 81.2f), Vector3.forward),
                (new Vector3(46.5f, Y, 84.6f), Vector3.left), (new Vector3(65f, Y, 82.4f), Vector3.left),
                (new Vector3(77f, Y, 86f), Vector3.left), (new Vector3(54f, Y, 82.2f), Vector3.forward),
            };
            int n = 0;
            foreach ((Vector3 back, Vector3 outward) in spots)
            {
                if (IslandHideSpots.Full) break;
                Transform root = Group($"Cart_{n++}", g);
                root.SetPositionAndRotation(back, Quaternion.LookRotation(outward));
                const float w = 1.25f, h = 1.8f, d = 1f;
                Box("Frame_B", root, new Vector3(0f, h * .5f, .02f), new Vector3(w, h - .2f, .04f), art.Canvas, false);
                Box("Side_L", root, new Vector3(-w * .5f, h * .5f, d * .5f), new Vector3(.04f, h - .2f, d), art.Canvas, false);
                Box("Side_R", root, new Vector3(w * .5f, h * .5f, d * .5f), new Vector3(.04f, h - .2f, d), art.Canvas, false);
                // Four narrow rails leave the cart open above the hidden player; the old single box was a lid.
                foreach (float x in new[] { -w * .5f, w * .5f })
                    Box("RimSide", root, new Vector3(x, h - .08f, d * .5f), new Vector3(.045f, .05f, d + .04f), art.Steel, false);
                foreach (float z in new[] { 0f, d })
                    Box("RimEnd", root, new Vector3(0f, h - .08f, z), new Vector3(w, .05f, .045f), art.Steel, false);
                Box("Base", root, new Vector3(0f, .15f, d * .5f), new Vector3(w, .06f, d), art.Steel, false);
                foreach (float x in new[] { -w * .45f, w * .45f })
                foreach (float z in new[] { .1f, d - .1f })
                    Primitive(PrimitiveType.Cylinder, "Wheel", root, new Vector3(x, .07f, z), new Vector3(.12f, .02f, .12f), art.Bars, false)
                        .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                for (int i = 0; i < 3; i++)
                    Box("Linen", root, new Vector3(-.3f + i * .3f, h * .45f, d * .4f), new Vector3(.4f, .2f, .5f), art.Blanket, false);
                // The flap: hinged on its left edge at the front, like a locker door.
                Transform flap = Group("Flap", root);
                flap.localPosition = new Vector3(-w * .5f, 0f, d);
                Box("Canvas", flap, new Vector3(w * .5f, h * .5f + .05f, 0f), new Vector3(w, h - .3f, .03f), art.Canvas, false);
                Box("Strap", flap, new Vector3(w - .1f, 1.1f, .02f), new Vector3(.06f, .3f, .02f), art.DarkWood, false);
                IslandHideSpots.Wire(root, flap, new Vector3(w, h, d), new Vector3(0f, .02f, d * .45f), HideSpotKind.Cart);
            }
        }
    }
}
