using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.SkyscraperLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Builds the luxury Art-Deco Restroom & Powder Room suite on Floor 2 (y = 44m):
    /// - Checkered marble floor tiles and dividing privacy partitions.
    /// - Double Carrara marble vanity counter with brass gooseneck faucets and undermount basins.
    /// - Full-width illuminated back-lit vanity mirror and paper towel dispenser.
    /// - Private mahogany toilet cubicles with brass hardware, porcelain fixtures, and occupied latch.
    /// - Atmospheric environmental storytelling: dripping faucet, overturned waste bin, and shattered mirror.
    /// </summary>
    public static class SkyscraperRestrooms
    {
        public static void Build(Transform root, SkyscraperArt art)
        {
            Transform suite = Group("PowderRoomRestrooms", root);
            float y = L.RestaurantY;
            suite.position = new Vector3(-13.5f, y, 6.0f);

            // 1. Enclosure Partition Walls (x: -16 to -11, z: 3.5 to 8.5)
            Box("DividerWallEast", suite, new Vector3(2.5f, 2.5f, 0f), new Vector3(0.25f, 5.0f, 5.0f), art.WallPlaster);
            Box("DividerWallSouth", suite, new Vector3(0f, 2.5f, -2.5f), new Vector3(5.0f, 5.0f, 0.25f), art.WallPlaster);

            // Restrooms Entrance Portal (at East wall, z = -1.2f)
            Transform door = Group("RestroomEntryDoorway", suite);
            door.position = new Vector3(2.5f, 0f, -1.2f);
            Box("DoorJamb", door, new Vector3(0f, 1.4f, 0f), new Vector3(0.35f, 2.8f, 1.4f), art.Mahogany);
            Box("BrassSignPlaque", door, new Vector3(0.2f, 1.8f, 0.9f), new Vector3(0.04f, 0.25f, 0.5f), art.Brass, false);

            // 2. Checkered Marble Tile Floor
            Transform floor = Group("CheckeredFloor", suite);
            for (float fx = -1.8f; fx <= 1.8f; fx += 1.2f)
            for (float fz = -1.8f; fz <= 1.8f; fz += 1.2f)
            {
                Material tileMat = ((int)(fx + fz + 10f) % 2 == 0) ? art.Marble : art.GraniteDark;
                Box($"Tile_{fx:0.0}_{fz:0.0}", floor, new Vector3(fx, 0.01f, fz), new Vector3(1.18f, 0.02f, 1.18f), tileMat, false);
            }

            // 3. Double Vanity Station (South Wall)
            Transform vanity = Group("DoubleVanityCounter", suite);
            vanity.position = new Vector3(0f, 0f, -2.1f);
            Box("MarbleSlab", vanity, new Vector3(0f, 0.85f, 0f), new Vector3(3.4f, 0.1f, 0.75f), art.Marble);
            Box("VanityApron", vanity, new Vector3(0f, 0.55f, 0f), new Vector3(3.3f, 0.5f, 0.7f), art.Mahogany);

            // Dual Porcelain Undermount Basins & Brass Gooseneck Faucets
            foreach (float vx in new[] { -0.85f, 0.85f })
            {
                Transform basin = Group($"Sink_{vx:0.0}", vanity);
                basin.localPosition = new Vector3(vx, 0f, 0f);
                Box("BasinVoid", basin, new Vector3(0f, 0.82f, 0f), new Vector3(0.55f, 0.04f, 0.42f), art.WallPlaster, false);
                Primitive(PrimitiveType.Cylinder, "FaucetStem", basin, new Vector3(0f, 1.0f, -0.25f), new Vector3(0.04f, 0.16f, 0.04f), art.Brass, false);
                Box("FaucetSpout", basin, new Vector3(0f, 1.15f, -0.15f), new Vector3(0.04f, 0.04f, 0.2f), art.Brass, false);
            }

            // Full-Width Back-Lit Mirror with Antique Silvering
            Transform mirror = Group("BacklitVanityMirror", vanity);
            mirror.localPosition = new Vector3(0f, 2.0f, -0.32f);
            Box("MirrorGlass", mirror, Vector3.zero, new Vector3(3.2f, 1.6f, 0.04f), art.Glass, false);
            Box("BacklightGlow", mirror, new Vector3(0f, 0f, -0.01f), new Vector3(3.26f, 1.66f, 0.02f), art.LampEmissive, false);

            // Accessories: Soap Dispenser & Paper Towel Dispenser
            Box("SoapDispenser", vanity, new Vector3(0f, 1.1f, -0.28f), new Vector3(0.12f, 0.2f, 0.1f), art.Brass, false);
            Box("PaperTowelDispenser", suite, new Vector3(2.35f, 1.4f, -1.8f), new Vector3(0.15f, 0.6f, 0.4f), art.Steel, false);

            // 4. Private Toilet Cubicles (North Wall)
            Transform cubicles = Group("ToiletCubicles", suite);
            cubicles.position = new Vector3(0f, 0f, 1.5f);

            // Stall partition walls
            Box("PartitionW", cubicles, new Vector3(-2.2f, 1.25f, 0f), new Vector3(0.06f, 2.5f, 1.8f), art.Mahogany);
            Box("PartitionMid", cubicles, new Vector3(0f, 1.25f, 0f), new Vector3(0.06f, 2.5f, 1.8f), art.Mahogany);
            Box("PartitionE", cubicles, new Vector3(2.2f, 1.25f, 0f), new Vector3(0.06f, 2.5f, 1.8f), art.Mahogany);

            // Stall 1 (Left): Door open 30 degrees
            Transform stall1 = Group("Stall_Left", cubicles);
            stall1.position = new Vector3(-1.1f, 0f, 0f);
            BuildToiletFixture(stall1, art);
            GameObject door1 = Box("StallDoor1", stall1, new Vector3(-0.5f, 1.1f, -0.85f), new Vector3(1.0f, 2.0f, 0.04f), art.Mahogany);
            door1.transform.localRotation = Quaternion.Euler(0f, 30f, 0f);

            // Stall 2 (Right): Door closed with red occupied indicator
            Transform stall2 = Group("Stall_Right", cubicles);
            stall2.position = new Vector3(1.1f, 0f, 0f);
            BuildToiletFixture(stall2, art);
            Box("StallDoor2", stall2, new Vector3(0f, 1.1f, -0.88f), new Vector3(1.05f, 2.0f, 0.04f), art.Mahogany);
            Box("OccupiedLatch", stall2, new Vector3(0.42f, 1.05f, -0.92f), new Vector3(0.06f, 0.04f, 0.02f), art.RedEmergencyEmissive, false);

            // 5. Environmental Details: Overturned Waste Basket
            Transform bin = Group("OverturnedWasteBin", suite);
            bin.position = new Vector3(-1.5f, 0f, -1.2f);
            Primitive(PrimitiveType.Cylinder, "BinBody", bin, new Vector3(0f, 0.15f, 0f), new Vector3(0.35f, 0.35f, 0.35f), art.Brass)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 75f);
            Box("SpilledTowels", bin, new Vector3(0.3f, 0.03f, 0f), new Vector3(0.4f, 0.04f, 0.3f), art.WallPlaster, false);
        }

        private static void BuildToiletFixture(Transform parent, SkyscraperArt art)
        {
            Transform toilet = Group("PorcelainToilet", parent);
            toilet.localPosition = new Vector3(0f, 0f, 0.5f);
            Box("Pedestal", toilet, new Vector3(0f, 0.25f, 0f), new Vector3(0.42f, 0.45f, 0.55f), art.WallPlaster);
            Box("Seat", toilet, new Vector3(0f, 0.48f, 0f), new Vector3(0.44f, 0.05f, 0.52f), art.WallPlaster);
            Box("FlushPipe", toilet, new Vector3(0f, 0.85f, 0.25f), new Vector3(0.06f, 0.7f, 0.06f), art.Steel, false);
            Box("FlushHandle", toilet, new Vector3(0.12f, 1.1f, 0.25f), new Vector3(0.12f, 0.04f, 0.04f), art.Brass, false);
        }
    }
}
