using System.Collections.Generic;
using Abandoned.Interaction;
using Unity.AI.Navigation;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The cellhouse's fitted rooms, each with a story you can read without words. The dining hall: bolted tables in
    /// rows, a serving line, the frozen clock, tear-gas canisters in the ceiling; the middle kept clear because that's
    /// the haul route from Broadway to the basement stair and the yard. The kitchen: ranges, prep tables, a walk-in
    /// store, pans still hanging. The library: shelves in ranks and a scavenger's camp (mattress, cans, a scorched
    /// floor). Admin: desks, filing cabinets, the control room's gun cage and key board. The entrance hall: the guard
    /// desk and the visiting booths. Every room places its loot points where an item could actually rest.
    /// </summary>
    public static class IslandInteriors
    {
        public static void Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot, List<Rect> keepClear)
        {
            Transform g = Group("Interiors", root);
            Dining(Group("DiningHall", g), art, loot, keepClear);
            Kitchen(Group("Kitchen", g), art, loot);
            Library(Group("Library", g), art, loot);
            Admin(Group("Admin", g), art, loot);
            Hall(Group("EntranceHall", g), art, loot);
            IslandInteriorDetails.Build(g, art);
            MarkStatic(g);
        }

        private const float Y = L.PlateauY;

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float z, CarryClass min, CarryClass max, float y = Y + .05f, bool jackpot = false) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, jackpot));

        // A long steel table with its stools welded on: one collider for the lot.
        private static void Table(Transform g, string name, Vector3 at, float length, IslandPrisonArt art, float tilt = 0f)
        {
            Transform t = Group(name, g);
            t.SetPositionAndRotation(at, Quaternion.Euler(0f, 0f, tilt));
            Box("Top", t, new Vector3(0f, .76f, 0f), new Vector3(.8f, .05f, length), art.Steel, false);
            Box("Frame", t, new Vector3(0f, .4f, 0f), new Vector3(.12f, .7f, length - .4f), art.Steel, false);
            for (float z = -length * .5f + .4f; z < length * .5f - .3f; z += .75f)
            foreach (float side in new[] { -.6f, .6f })
            {
                Box("Stool", t, new Vector3(side, .46f, z), new Vector3(.3f, .04f, .3f), art.Steel, false);
                Box("StoolArm", t, new Vector3(side * .6f, .3f, z), new Vector3(Mathf.Abs(side) * .8f, .04f, .04f), art.Steel, false);
            }
            var c = t.gameObject.AddComponent<BoxCollider>();
            c.center = new Vector3(0f, .4f, 0f);
            c.size = new Vector3(1.6f, .8f, length);
            var mod = t.gameObject.AddComponent<NavMeshModifier>();
            mod.overrideArea = true;
            mod.area = 1;
        }

        private static void Dining(Transform g, IslandPrisonArt art, List<LootPointSpec> loot, List<Rect> keepClear)
        {
            Rect r = Rect.MinMaxRect(L.Cellhouse.xMin + .5f, L.NorthBand.yMin + .2f, L.KitchenWest, L.Cellhouse.yMax - .5f);
            // The haul lane: Broadway's mouth to the stairwell and the yard door, and on to the kitchen door.
            keepClear.Add(Rect.MinMaxRect(L.Cellhouse.xMin, L.NorthBand.yMin, L.Cellhouse.xMax, L.NorthBand.yMin + 3.2f));
            keepClear.Add(Rect.MinMaxRect(48.6f, L.NorthBand.yMin, 59.4f, r.yMax));
            keepClear.Add(Rect.MinMaxRect(30f, 83.6f, 40.5f, 88.4f));
            int n = 0;
            foreach (float x in new[] { 41.5f, 45f, 62.5f })
            foreach (float z in new[] { 85.2f, 89.4f })
            {
                if (x < 42f && z < 88f) continue;  // the stairwell's railing
                Table(g, $"Table_{n++}", new Vector3(x, Y, z), 3f, art);
                if (K.Hash(n, 3) < .55f) Point(loot, $"Dining_Table_{n}", IslandLootItems.Contraband, x, z, CarryClass.Pocket, CarryClass.OneHand, Y + .82f);
            }
            // Two thrown over in the riot that closed the place, either side of the lane.
            Table(g, "Table_Overturned_A", new Vector3(47f, Y + .8f, 88.5f), 3f, art, 90f);
            Table(g, "Table_Overturned_B", new Vector3(60.5f, Y + .8f, 86f), 3f, art, -90f);
            // Serving line against the kitchen wall, its sneeze-guard long gone.
            Box("ServingCounter", g, new Vector3(64.9f, Y + .45f, 88.5f), new Vector3(1f, .9f, 5f), art.Steel);
            Box("ServingRail", g, new Vector3(64.3f, Y + .8f, 88.5f), new Vector3(.06f, .06f, 5f), art.Steel, false);
            for (int i = 0; i < 4; i++) Box("Tray", g, new Vector3(64.9f, Y + .92f, 86.7f + i * 1.2f), new Vector3(.45f, .03f, .35f), art.Steel, false);
            Point(loot, "Dining_Counter", IslandLootItems.Contraband, 63.8f, 87.5f, CarryClass.Pocket, CarryClass.OneHand, Y + .05f);
            // The clock that stopped (a landmark; no numbers on it) and the gas canisters in the ceiling.
            Transform clock = Group("Clock", g);
            clock.position = new Vector3(48f, Y + 3.6f, r.yMax - .06f);
            Primitive(PrimitiveType.Cylinder, "Face", clock, Vector3.zero, new Vector3(1.1f, .03f, 1.1f), art.Porcelain, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Primitive(PrimitiveType.Cylinder, "Rim", clock, Vector3.forward * .02f, new Vector3(1.2f, .02f, 1.2f), art.Steel, false).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            K.RotBox("Hour", clock, new Vector3(.1f, .08f, -.05f), new Vector3(.04f, .3f, .02f), Quaternion.Euler(0f, 0f, -40f), art.Bars, false);
            K.RotBox("Minute", clock, new Vector3(-.15f, -.1f, -.05f), new Vector3(.03f, .44f, .02f), Quaternion.Euler(0f, 0f, 125f), art.Bars, false);
            for (float x = 36f; x < 64f; x += 7f)
                Primitive(PrimitiveType.Cylinder, "GasCanister", g, new Vector3(x, L.NorthBandCeiling - .2f, 86f), new Vector3(.18f, .2f, .18f), art.Steel, false);
            // The stairwell's guard rail.
            K.Railing(g, "StairRail_S", new Vector3(30.6f, Y, 84f), new Vector3(38f, Y, 84f), 1.05f, art.Railing);
            K.Railing(g, "StairRail_N", new Vector3(30.6f, Y, 88f), new Vector3(38f, Y, 88f), 1.05f, art.Railing);
            K.Railing(g, "StairRail_E1", new Vector3(38f, Y, 84f), new Vector3(38f, Y, 84.6f), 1.05f, art.Railing);
            K.Railing(g, "StairRail_E2", new Vector3(38f, Y, 87.4f), new Vector3(38f, Y, 88f), 1.05f, art.Railing);
            // Spilled trays and food cans under the tables.
            for (int i = 0; i < 14; i++)
            {
                float hx = K.Hash(i, 61), hz = K.Hash(61, i);
                float x = Mathf.Lerp(39f, 65f, hx), z = Mathf.Lerp(83.5f, 91f, hz);
                if (x > 48.6f && x < 59.4f) continue;
                K.RotBox("Litter", g, new Vector3(x, Y + .015f, z), new Vector3(.35f, .02f, .25f), Quaternion.Euler(0f, hx * 360f, 0f), i % 3 == 0 ? art.Steel : art.Paper, false);
            }
        }

        private static void Kitchen(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float x0 = L.KitchenWest + .2f, x1 = L.Cellhouse.xMax - .5f, z0 = L.NorthBand.yMin + .2f, z1 = L.Cellhouse.yMax - .5f;
            // Ranges along the north wall under a hood, prep tables down the middle, a walk-in store in the corner.
            for (float x = x0 + 1.2f; x < x1 - 4f; x += 1.8f)
            {
                // The service door (north wall, x 72) stays clear.
                if (Mathf.Abs(x - L.ServiceDoorX) < 2.2f) continue;
                Box("Range", g, new Vector3(x, Y + .45f, z1 - .55f), new Vector3(1.6f, .9f, 1f), art.Steel);
                Box("RangeTop", g, new Vector3(x, Y + .91f, z1 - .55f), new Vector3(1.6f, .03f, 1f), art.Bars, false);
            }
            Box("Hood", g, new Vector3((x0 + x1 - 4f) * .5f, Y + 2.6f, z1 - .6f), new Vector3(x1 - 4f - x0, .5f, 1.3f), art.Steel, false);
            Box("Flue", g, new Vector3((x0 + x1 - 4f) * .5f, Y + 3.8f, z1 - .5f), new Vector3(.6f, 2f, .6f), art.Steel, false);
            // Prep tables west of the service door's line, so both doors and the hatch keep a way through.
            foreach (float z in new[] { 85.4f, 88.4f })
            {
                IslandInteriorDetails.PrepTable(g, new Vector3(69f, Y, z), art);
                for (int i = 0; i < 4; i++) Box("Pan", g, new Vector3(67.7f + i * .85f, Y + 1.9f, z), new Vector3(.32f, .04f, .32f), art.Bars, false);
                Box("PanRail", g, new Vector3(69f, Y + 2f, z), new Vector3(3.4f, .04f, .04f), art.Steel, false);
            }
            Point(loot, "Kitchen_Prep_A", IslandLootItems.Contraband, 68.2f, 85.4f, CarryClass.Pocket, CarryClass.OneHand, Y + .95f);
            Point(loot, "Kitchen_Prep_B", IslandLootItems.Warden, 69.8f, 88.4f, CarryClass.OneHand, CarryClass.TwoHand, Y + .95f);
            // The walk-in store: a steel room in the north-east corner, door open, shelves of tins.
            float wx0 = x1 - 3.6f, wz0 = z0 + .1f;
            K.WallX(g, "WalkIn_S", wx0, x1, wz0 + 3.4f, Y, 2.8f, .1f, art.Steel, IslandKit.Opening.Door(wx0 + 1f, 1.3f, 2.2f));
            K.WallZ(g, "WalkIn_W", wz0, wz0 + 3.4f, wx0, Y, 2.8f, .1f, art.Steel);
            K.Span("WalkIn_Roof", g, new Vector3(wx0, Y + 2.8f, wz0), new Vector3(x1, Y + 2.9f, wz0 + 3.45f), art.Steel, false);
            for (int i = 0; i < 3; i++)
                Box("WalkIn_Shelf", g, new Vector3(x1 - .3f, Y + .5f + i * .7f, wz0 + 1.7f), new Vector3(.5f, .04f, 3f), art.Steel, false);
            Point(loot, "Kitchen_WalkIn", "workshop", x1 - 1.6f, wz0 + 1.7f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Kitchen_Floor", IslandLootItems.Contraband, 74f, 89.4f, CarryClass.Pocket, CarryClass.TwoHand);
        }

        private static void Library(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Rect r = L.Library;
            float x0 = r.xMin + .2f, x1 = r.xMax - .5f, z0 = r.yMin + .5f, z1 = r.yMax - .2f;
            // Wall shelving and two free-standing ranks; most books are on the floor.
            // Clear of the east door (z 46) and the aisle door (x 72).
            Shelf(g, "Shelf_E", new Vector3(x1 - .25f, Y, 50.2f), new Vector3(.45f, 2.4f, 3f), art);
            Shelf(g, "Shelf_N", new Vector3(75.3f, Y, z1 - .25f), new Vector3(3.6f, 2.4f, .45f), art);
            Shelf(g, "Rank_A", new Vector3(73.5f, Y, 48.5f), new Vector3(3.6f, 2f, .5f), art);
            Shelf(g, "Rank_B", new Vector3(73.5f, Y, 45.5f), new Vector3(3.6f, 2f, .5f), art);
            for (int i = 0; i < 18; i++)
            {
                float hx = K.Hash(i, 71), hz = K.Hash(71, i);
                K.RotBox("FallenBook", g, new Vector3(Mathf.Lerp(x0 + .5f, x1 - 1f, hx), Y + .025f, Mathf.Lerp(z0 + .5f, z1 - .8f, hz)),
                    new Vector3(.2f, .05f, .28f), Quaternion.Euler(0f, hx * 360f, 0f), art.Books, false);
            }
            IslandInteriorDetails.Desk(g, "Desk", new Vector3(71f, Y, 42.5f), art);
            Point(loot, "Library_Desk", IslandLootItems.Warden, 71f, 42.5f, CarryClass.Pocket, CarryClass.OneHand, Y + .8f);
            Point(loot, "Library_Ranks", "records", 75.5f, 47f, CarryClass.Pocket, CarryClass.TwoHand);
            // A scavenger slept here after the closure: mattress dragged in, a cold fire in a bucket, cans, a tarp.
            Box("CampMattress", g, new Vector3(76f, Y + .08f, 42f), new Vector3(1f, .16f, 1.9f), art.Mattress, false);
            Primitive(PrimitiveType.Cylinder, "FireBucket", g, new Vector3(74.2f, Y + .2f, 41.4f), new Vector3(.45f, .2f, .45f), art.Rust, false);
            K.Patch("Scorch", g, new Vector3(74.2f, Y + .03f, 41.4f), new Vector2(1.8f, 1.6f), 20f, art.Scorch);
            for (int i = 0; i < 6; i++)
                Primitive(PrimitiveType.Cylinder, "Can", g, new Vector3(73.4f + i * .22f, Y + .06f, 42.4f + (i % 2) * .3f), new Vector3(.08f, .06f, .08f), art.Steel, false);
            K.RotBox("Tarp", g, new Vector3(76.4f, Y + 1.2f, 43.6f), new Vector3(2.2f, .02f, 2f), Quaternion.Euler(0f, 0f, 62f), art.Canvas, false);
            Point(loot, "Library_Camp", IslandLootItems.Maritime, 75.6f, 43.9f, CarryClass.Pocket, CarryClass.OneHand);
        }

        private static void Shelf(Transform g, string name, Vector3 baseCentre, Vector3 size, IslandPrisonArt art)
        {
            IslandLibraryShelves.Build(g, name, baseCentre, size, art);
        }

        private static void Admin(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            Rect r = L.Admin;
            float x0 = r.xMin + .5f, x1 = r.xMax - .2f, z0 = r.yMin + .5f, z1 = r.yMax - .2f;
            // Two desks, a bank of filing cabinets along the west wall, one cabinet pulled over and spilled.
            IslandInteriorDetails.Desk(g, "Desk_A", new Vector3(33f, Y, 43.5f), art);
            IslandInteriorDetails.Desk(g, "Desk_B", new Vector3(37.6f, Y, 47.5f), art);
            for (int i = 0; i < 5; i++)
                Box("Filing", g, new Vector3(x0 + .3f, Y + .7f, z0 + .5f + i * .55f), new Vector3(.6f, 1.4f, .5f), art.Steel);
            K.RotBox("FilingDown", g, new Vector3(32f, Y + .25f, 46.2f), new Vector3(1.4f, .5f, .6f), Quaternion.Euler(0f, 15f, 0f), art.Steel);
            for (int i = 0; i < 10; i++)
                K.RotBox("Paper", g, new Vector3(31.5f + K.Hash(i, 81) * 5f, Y + .01f, 42f + K.Hash(81, i) * 6f), new Vector3(.21f, .005f, .3f),
                    Quaternion.Euler(0f, i * 37f, 0f), art.Paper, false);
            Point(loot, "Admin_DeskA", "records", 33f, 43.5f, CarryClass.Pocket, CarryClass.TwoHand, Y + .8f);
            Point(loot, "Admin_DeskB", IslandLootItems.Armoury, 37.6f, 47.5f, CarryClass.Pocket, CarryClass.OneHand, Y + .8f);
            // The control room in the north-west corner (clear of the aisle door at x 36): a gun cage with an
            // empty rack, the key board on its wall and the switch panel that once opened whole tiers.
            float cx1 = 34.4f, cz0 = 48.6f;
            IslandOfficeDetails.Cage(g, x0, cx1, cz0, z1, art);
            Box("GunRack", g, new Vector3(x0 + .2f, Y + 1f, 50.3f), new Vector3(.3f, 1.6f, 2.4f), art.DarkWood);
            Box("KeyBoard", g, new Vector3(cx1 - .06f, Y + 1.6f, 50.2f), new Vector3(.04f, .8f, 1.2f), art.DarkWood, false);
            Box("SwitchPanel", g, new Vector3(32.4f, Y + 1.3f, z1 - .1f), new Vector3(1.6f, 1f, .15f), art.Steel, false);
            Point(loot, "Admin_Cage", IslandLootItems.Armoury, 32.6f, 50.2f, CarryClass.Pocket, CarryClass.Heavy);
            _ = x1;
        }

        private static void Hall(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            // The guard desk inside the main door, off Broadway's line (the haul route runs straight past it).
            Box("GuardDesk", g, new Vector3(49.8f, Y + .55f, 42.6f), new Vector3(.8f, 1.1f, 2.4f), art.DarkWood);
            Box("GuardDeskTop", g, new Vector3(49.8f, Y + 1.12f, 42.6f), new Vector3(1f, .04f, 2.6f), art.DarkWood, false);
            Point(loot, "Hall_Desk", IslandLootItems.Armoury, 49.8f, 42.6f, CarryClass.Pocket, CarryClass.OneHand, Y + 1.16f);
            // Visiting booths along the south wall either side of the door: glass, a phone, a stool each side.
            foreach (float x0 in new[] { 41f, 61f })
            {
                for (int i = 0; i < 5; i++)
                {
                    float x = x0 + .3f + i * 1.25f;
                    Box("BoothDivider", g, new Vector3(x, Y + .9f, 41.2f), new Vector3(.06f, 1.8f, 1.4f), art.Steel);
                    Box("BoothGlass", g, new Vector3(x + .62f, Y + 1.3f, 41.2f), new Vector3(1.15f, .9f, .02f), art.Glass);
                    Box("BoothShelf", g, new Vector3(x + .62f, Y + .8f, 41.2f), new Vector3(1.15f, .05f, 1.2f), art.Steel);
                    Box("Phone", g, new Vector3(x + .3f, Y + 1.2f, 41.8f), new Vector3(.08f, .2f, .06f), art.Bars, false);
                }
            }
            Point(loot, "Hall_Booth_W", IslandLootItems.Contraband, 43f, 41.9f, CarryClass.Pocket, CarryClass.OneHand, Y + .85f);
            Point(loot, "Hall_Floor", IslandLootItems.Armoury, 44f, 47f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Hall_Floor_B", IslandLootItems.Contraband, 64f, 47f, CarryClass.Pocket, CarryClass.TwoHand);
            // Broadway itself: puddles under the empty lantern bays, moss creeping from them, a dropped mop bucket.
            for (int i = 0; i < 8; i++)
            {
                float z = 55f + i * 3.2f, h = K.Hash(i, 91);
                if (h < .35f) continue;
                K.Patch("Moss", g, new Vector3(51.4f + h * 6f, Y + .03f, z + .3f), new Vector2(2.4f + h * 2f, 1.8f + h * 1.4f), h * 90f, art.Moss);
                K.Patch("Puddle", g, new Vector3(51f + h * 6f, Y + .05f, z), new Vector2(1.4f + h * 2f, 1f + h * 1.6f), h * 140f, art.Water);
            }
            Primitive(PrimitiveType.Cylinder, "MopBucket", g, new Vector3(58.5f, Y + .12f, 61f), new Vector3(.45f, .25f, .45f), art.Warning, false);
            Point(loot, "Broadway_A", IslandLootItems.Contraband, 50.5f, 66f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Broadway_B", IslandLootItems.Armoury, 57.5f, 74f, CarryClass.OneHand, CarryClass.Heavy);
            Point(loot, "DAisle_A", IslandLootItems.Contraband, 36f, 60f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "DAisle_B", IslandLootItems.Armoury, 36.5f, 74f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "AAisle_A", IslandLootItems.Contraband, 72f, 58f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "AAisle_B", IslandLootItems.Warden, 72.5f, 70f, CarryClass.OneHand, CarryClass.TwoHand);
        }
    }
}
