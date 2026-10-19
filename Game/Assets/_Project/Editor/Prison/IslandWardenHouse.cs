using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The warden's house on the north tip: a two-storey brick villa that burned after the closure. The west half of
    /// its roof is gone (charred joists against the sky), the upper floor is burned timber that gives way easily, and
    /// the best things on the island are still inside: the parlour's upright piano (a three-person carry out of a
    /// wide front door), the clock in the hall, the portrait and the tea service upstairs.
    /// </summary>
    public static class IslandWardenHouse
    {
        private static Rect H => L.WardenHouse;
        private const float Wall = .35f, Y = L.PlateauY;

        public static List<IslandCellhouse.Tile> Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot, List<IslandFlights.Built> flights)
        {
            Transform g = Group("WardenHouse", root);
            Shell(Group("Shell", g), art);
            var floors = UpperFloor(Group("WardenFloor", root.Find("Levels") ?? Group("Levels", root)), art);
            flights.Add(IslandFlights.Flight(Group("WardenStair", root),
                new L.Flight("WardenStair", new Vector3(34.4f, Y, 110.5f), Vector3.right, 7.6f, L.Storey, 1.4f, false), art.DarkWood, art.DarkWood, false, true));
            Rooms(Group("Rooms", g), art, loot);
            Garden(Group("Garden", g), art);
            MarkStatic(g);
            return floors;
        }

        // Tiles over the ground floor: 5 x 3, the two over the stair left open.
        private static bool Hole(int x, int z) => z == 2 && (x == 1 || x == 2);

        private static void Shell(Transform g, IslandPrisonArt art)
        {
            float h = 2f * L.Storey + .6f;
            var front = new List<O> { O.Door(40f, 2.6f, 2.8f) };
            foreach (float x in new[] { 32.5f, 36f, 44f, 47.5f }) front.Add(O.Window(x, 1.3f, 1f, 2.7f));
            foreach (float x in new[] { 32.5f, 36f, 44f, 47.5f }) front.Add(O.Window(x, 1.3f, L.Storey + 1f, L.Storey + 2.7f));
            K.WallX(g, "Front", H.xMin, H.xMax, H.yMin + Wall * .5f, Y, h, Wall, art.BurntBrick, front.ToArray());
            var back = new List<O>();
            foreach (float x in new[] { 33f, 46f }) { back.Add(O.Window(x, 1.3f, 1f, 2.7f)); back.Add(O.Window(x, 1.3f, L.Storey + 1f, L.Storey + 2.7f)); }
            K.WallX(g, "Back", H.xMin, H.xMax, H.yMax - Wall * .5f, Y, h, Wall, art.BurntBrick, back.ToArray());
            var sides = new[] { O.Window(103f, 1.3f, 1f, 2.7f), O.Window(107f, 1.3f, L.Storey + 1f, L.Storey + 2.7f) };
            K.WallZ(g, "West", H.yMin, H.yMax, H.xMin + Wall * .5f, Y, h, Wall, art.BurntBrick, sides);
            K.WallZ(g, "East", H.yMin, H.yMax, H.xMax - Wall * .5f, Y, h, Wall, art.Brick, sides);
            // Ground-floor partitions: parlour | hall | study, open at the north end where the stair runs.
            K.WallZ(g, "Part_Parlour", H.yMin + Wall, 108.6f, 38f, Y, L.Storey - L.Slab, .2f, art.Plaster, O.Door(104f, 2.4f, 2.6f));
            K.WallZ(g, "Part_Study", H.yMin + Wall, 108.6f, 44f, Y, L.Storey - L.Slab, .2f, art.Plaster, O.Door(104f, 1.6f, 2.4f));
            K.WallZ(g, "Part_Upstairs", H.yMin + Wall, 108.6f, 40f, Y + L.Storey, L.Storey - L.Slab, .2f, art.Plaster, O.Door(106f, 1.6f, 2.3f));
            // The roof survives over the east half only. The west half: charred joists against the sky.
            float top = Y + h;
            K.Span("Roof_E", g, new Vector3(40f, top, H.yMin - .4f), new Vector3(H.xMax + .4f, top + .3f, H.yMax + .4f), art.FacadeDark);
            for (float x = H.xMin + .8f; x < 40f; x += 1.4f)
            {
                if (K.Hash((int)(x * 3), 5) < .35f) continue;
                K.RotBox("Joist", g, new Vector3(x, top - .1f, H.center.y), new Vector3(.18f, .24f, H.height + .3f),
                    Quaternion.Euler(K.Hash((int)x, 9) * 8f - 4f, 0f, 0f), art.DarkWood, false);
            }
            K.Span("Chimney", g, new Vector3(H.xMax - 2.2f, Y, H.yMax - 1.4f), new Vector3(H.xMax - .4f, top + 2.2f, H.yMax - .4f), art.Brick);
            // The porch: steps and columns, the front door long gone.
            K.Span("PorchStep", g, new Vector3(37f, Y, H.yMin - 1.8f), new Vector3(43f, Y + .16f, H.yMin), art.Facade, false);
            K.Span("PorchRoof", g, new Vector3(37f, Y + 3.2f, H.yMin - 1.8f), new Vector3(43f, Y + 3.4f, H.yMin), art.FacadeDark, false);
            foreach (float x in new[] { 37.3f, 42.7f })
                Box("PorchColumn", g, new Vector3(x, Y + 1.6f, H.yMin - 1.5f), new Vector3(.25f, 3.2f, .25f), art.Facade);
            // Soot on the floor of the gutted west rooms.
            K.Patch("FloorScorch", g, new Vector3(34f, Y + .03f, 105f), new Vector2(6f, 5f), 15f, art.Scorch);
        }

        private static List<IslandCellhouse.Tile> UpperFloor(Transform g, IslandPrisonArt art)
        {
            var tiles = new List<IslandCellhouse.Tile>();
            for (int z = 0; z < 3; z++)
            for (int x = 0; x < 5; x++)
            {
                if (Hole(x, z)) continue;
                Vector3 c = new(H.xMin + (x + .5f) * L.Tile, Y + L.Storey, H.yMin + (z + .5f) * L.Tile);
                Transform tile = Group($"WardenFloor_{x}_{z}", g);
                tile.position = c;
                var col = tile.gameObject.AddComponent<BoxCollider>();
                col.center = Vector3.down * (L.Slab * .5f);
                col.size = new Vector3(L.Tile, L.Slab, L.Tile);
                Transform visual = Group("Visual", tile);
                // West tiles are burned through in places: dark boards.
                K.WorldBox("Boards", visual, c + Vector3.down * (L.Slab * .5f), new Vector3(L.Tile, L.Slab, L.Tile), x < 2 ? art.DarkWood : art.Board, false);
                K.WorldBox("Ceiling", visual, c + Vector3.down * (L.Slab + .01f), new Vector3(L.Tile - .02f, .02f, L.Tile - .02f), art.Ceiling, false);
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                tiles.Add(new IslandCellhouse.Tile { Transform = tile, Collapsible = true, Cell = new Vector2Int(x, z) });
            }
            // Rail round the stairwell's open south side.
            K.Railing(g, "WellRail", new Vector3(34f, Y + L.Storey, 108f), new Vector3(42f, Y + L.Storey, 108f), 1f, art.DarkWood);
            K.Railing(g, "WellRail_W", new Vector3(34f, Y + L.Storey, 108f), new Vector3(34f, Y + L.Storey, 111.6f), 1f, art.DarkWood);
            return tiles;
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float y, float z, CarryClass min, CarryClass max, bool jackpot = false) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, jackpot));

        private static void Rooms(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float y2 = Y + L.Storey;
            // Parlour (west, a 2.4 m door so three can carry the piano out): the piano's place against the west wall, a burnt sofa, a fireplace; the jackpot.
            Point(loot, "Warden_Parlour_Piano", IslandLootItems.Warden, 32.5f, Y + .05f, 104.5f, CarryClass.TwoHand, CarryClass.Huge, true);
            Box("Sofa", g, new Vector3(34.5f, Y + .4f, 101.5f), new Vector3(2.2f, .8f, .9f), art.Mattress);
            Box("Hearth", g, new Vector3(H.xMin + .5f, Y + .7f, 107.6f), new Vector3(.6f, 1.4f, 1.8f), art.Brick);
            Point(loot, "Warden_Parlour_Floor", "gallery", 35.6f, Y + .05f, 106.6f, CarryClass.Pocket, CarryClass.TwoHand);
            // Hall: the grandfather clock's place, a coat stand, umbrellas.
            Point(loot, "Warden_Hall_Clock", "gallery", 42.8f, Y + .05f, 101.2f, CarryClass.TwoHand, CarryClass.Heavy);
            Box("CoatStand", g, new Vector3(38.6f, Y + .9f, 101f), new Vector3(.12f, 1.8f, .12f), art.DarkWood);
            // Study (east): desk, a chart table, bookcases, a glass case smashed open.
            Box("Desk", g, new Vector3(47f, Y + .38f, 103f), new Vector3(1.8f, .76f, .9f), art.DarkWood);
            Box("ChartTable", g, new Vector3(46.5f, Y + .45f, 107f), new Vector3(1.4f, .9f, 1f), art.DarkWood);
            Box("Bookcase", g, new Vector3(H.xMax - .55f, Y + 1.1f, 105f), new Vector3(.4f, 2.2f, 3f), art.DarkWood);
            Box("Case", g, new Vector3(45f, Y + .5f, 101f), new Vector3(1f, 1f, .5f), art.DarkWood);
            Point(loot, "Warden_Study_Desk", IslandLootItems.Warden, 47f, Y + .8f, 103f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Warden_Study_Chart", IslandLootItems.Maritime, 46.5f, Y + .95f, 107f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Warden_Study_Floor", IslandLootItems.Maritime, 48f, Y + .05f, 109.5f, CarryClass.OneHand, CarryClass.Heavy);
            // Upstairs: the bedroom (west, burned), the dressing room and the sitting room (east).
            Box("Bedframe", g, new Vector3(33f, y2 + .3f, 103.5f), new Vector3(1.6f, .6f, 2.1f), art.DarkWood);
            Box("Dresser", g, new Vector3(37.4f, y2 + .5f, 101f), new Vector3(1.2f, 1f, .5f), art.DarkWood);
            Point(loot, "Warden_Bedroom", IslandLootItems.Warden, 35f, y2 + .05f, 106f, CarryClass.Pocket, CarryClass.TwoHand);
            Box("SideTable", g, new Vector3(46f, y2 + .35f, 102.5f), new Vector3(.8f, .7f, .8f), art.DarkWood);
            Box("Armchair", g, new Vector3(48f, y2 + .45f, 105f), new Vector3(.9f, .9f, .9f), art.Mattress);
            Point(loot, "Warden_Sitting_Table", IslandLootItems.Warden, 46f, y2 + .75f, 102.5f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Warden_Sitting_Floor", IslandLootItems.Warden, 43.5f, y2 + .05f, 109.5f, CarryClass.OneHand, CarryClass.TwoHand);
            // Ash and fallen plaster on the floors.
            for (int i = 0; i < 12; i++)
            {
                float hx = K.Hash(i, 121), hz = K.Hash(121, i);
                K.RotBox("Debris", g, new Vector3(Mathf.Lerp(31f, 49f, hx), (i % 2 == 0 ? Y : y2) + .03f, Mathf.Lerp(101f, 107.5f, hz)),
                    new Vector3(.5f, .06f, .4f), Quaternion.Euler(0f, hx * 360f, 0f), i % 3 == 0 ? art.Scorch : art.Plaster, false);
            }
        }

        // A walled front garden gone to scrub, a gravel path to the cellhouse, the fence to the lighthouse.
        private static void Garden(Transform g, IslandPrisonArt art)
        {
            K.Span("Path", g, new Vector3(38.5f, Y, L.Cellhouse.yMax), new Vector3(41.5f, Y + .02f, H.yMin - 1.8f), art.Gravel, false);
            K.Span("GardenWall_W", g, new Vector3(28f, Y, H.yMin - 6f), new Vector3(28.3f, Y + .9f, H.yMax), art.Brick);
            K.Span("GardenWall_S1", g, new Vector3(28f, Y, H.yMin - 6f), new Vector3(37.5f, Y + .9f, H.yMin - 5.7f), art.Brick);
            K.Span("GardenWall_S2", g, new Vector3(42.5f, Y, H.yMin - 6f), new Vector3(52f, Y + .9f, H.yMin - 5.7f), art.Brick);
            for (int i = 0; i < 9; i++)
            {
                float hx = K.Hash(i, 131);
                PolishAssets.Shape("Scrub", g, new Vector3(Mathf.Lerp(29f, 51f, hx), Y + .3f, H.yMin - 1.5f - K.Hash(131, i) * 4f),
                    new Vector3(1.2f + hx, .6f + hx * .4f, 1f + hx), art.Grass, true);
            }
        }
    }
}
