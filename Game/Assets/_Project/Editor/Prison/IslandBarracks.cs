using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Extraction;
using Abandoned.Interaction;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The quay: the guards' barracks (three storeys against the cliff: armoury, mess and locker room on the ground,
    /// a dormitory above, officers' rooms at the top; timber upper floors that can fall) and the guard tower at the
    /// quay's west end (a ladder up to a cabin, pocket loot only: hands must be empty to climb). Loot here is close to
    /// the boat, so a crew always has something within reach of the way out.
    /// </summary>
    public static class IslandBarracks
    {
        private static Rect B => L.Barracks;
        private const float Wall = .35f;

        public static List<IslandCellhouse.Tile> Build(Transform root, IslandPrisonArt art, List<LootPointSpec> loot, List<IslandFlights.Built> flights)
        {
            var floors = new List<IslandCellhouse.Tile>();
            Transform g = Group("Barracks", root);
            Shell(Group("Shell", g), art);
            Floors(Group("Floors", root.Find("Levels") ?? Group("Levels", root)), art, floors);
            Stairs(Group("BarracksStairs", root), art, flights);
            Ground(Group("GroundFloor", g), Group("GuardLockers", root), art, loot);
            Upper(Group("Upstairs", g), art, loot);
            MarkStatic(g);
            Tower(Group("GuardTower", root), art, loot, floors);
            return floors;
        }

        // Tile (x, z) of the upper floors: 7 x 3 tiles of 4 m from the barracks' south-west corner.
        private static Vector3 TileCentre(int x, int z, float y) => new(B.xMin + (x + .5f) * L.Tile, y, B.yMin + (z + .5f) * L.Tile);
        private static bool StairHole(int f, int x, int z) => z == 2 && (f == 1 ? (x == 4 || x == 5) : x == 5);

        private static void Shell(Transform g, IslandPrisonArt art)
        {
            float h = L.Storey * L.BarracksStoreys;
            var front = new List<O> { O.Door(116f, 2.4f, 2.6f), O.Door(126f, 2.4f, 2.6f) };
            var back = new List<O>();
            for (float x = B.xMin + 2f; x < B.xMax - 1f; x += 4f)
            for (int f = 0; f < L.BarracksStoreys; f++)
            {
                if (Mathf.Abs(x - 116f) < 2.5f || Mathf.Abs(x - 126f) < 2.5f) continue;
                front.Add(O.Window(x, 1.4f, f * L.Storey + 1.1f, f * L.Storey + 2.6f));
                if (f > 0) back.Add(O.Window(x, 1.4f, f * L.Storey + 1.4f, f * L.Storey + 2.6f));
            }
            K.WallX(g, "Front", B.xMin, B.xMax, B.yMin + Wall * .5f, L.QuayY, h, Wall, art.Facade, front.ToArray());
            K.WallX(g, "Back", B.xMin, B.xMax, B.yMax - Wall * .5f, L.QuayY, h, Wall, art.Facade, back.ToArray());
            var side = new List<O>();
            for (int f = 1; f < L.BarracksStoreys; f++) side.Add(O.Window(B.center.y, 1.4f, f * L.Storey + 1.1f, f * L.Storey + 2.6f));
            K.WallZ(g, "West", B.yMin, B.yMax, B.xMin + Wall * .5f, L.QuayY, h, Wall, art.Facade, side.ToArray());
            K.WallZ(g, "East", B.yMin, B.yMax, B.xMax - Wall * .5f, L.QuayY, h, Wall, art.Facade, side.ToArray());
            K.Span("Roof", g, new Vector3(B.xMin - .3f, h, B.yMin - .3f), new Vector3(B.xMax + .3f, h + .35f, B.yMax + .3f), art.FacadeDark);
            K.Span("Parapet_S", g, new Vector3(B.xMin - .3f, h + .35f, B.yMin - .3f), new Vector3(B.xMax + .3f, h + 1f, B.yMin), art.FacadeDark, false);
            // Every window barred like the cellhouse's (the guards' quarters were inside the wire too): glass mostly gone.
            Transform bars = Group("WindowBars", g);
            foreach (O o in front)
            {
                if (o.Sill < .5f) continue;
                Vector3 c = new(o.At, (o.Sill + o.Top) * .5f, B.yMin + Wall * .5f);
                GameObject solid = K.WorldBox("BarsSolid", bars, c, new Vector3(o.Width, o.Top - o.Sill, .06f), art.Bars);
                Object.DestroyImmediate(solid.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(solid.GetComponent<MeshFilter>());
                for (float s = -o.Width * .5f + .15f; s < o.Width * .5f; s += .2f)
                    K.WorldBox("Bar", bars, c + Vector3.right * s, new Vector3(.03f, o.Top - o.Sill, .03f), art.Bars, false);
            }
            // Ground-floor partitions: armoury | mess | locker room, doors on the south side of each.
            K.WallZ(g, "Part_Armoury", B.yMin + Wall, B.yMax - Wall, 112f, 0f, L.Storey - L.Slab, .2f, art.Plaster, O.Door(6f, 1.4f, 2.4f));
            K.WallZ(g, "Part_Lockers", B.yMin + Wall, B.yMax - Wall, 120f, 0f, L.Storey - L.Slab, .2f, art.Plaster, O.Door(6f, 1.6f, 2.4f));
            // Top floor: the officers' rooms either side of a partition.
            K.WallZ(g, "Part_Officers", B.yMin + Wall, 10.8f, 116f, 2f * L.Storey, L.Storey - L.Slab, .2f, art.Plaster, O.Door(6f, 1.2f, 2.3f));
            // The porch over the main door, and the steps.
            K.Span("Porch", g, new Vector3(114f, 2.9f, B.yMin - 1.6f), new Vector3(118f, 3.1f, B.yMin), art.FacadeDark, false);
        }

        private static void Floors(Transform g, IslandPrisonArt art, List<IslandCellhouse.Tile> floors)
        {
            for (int f = 1; f < L.BarracksStoreys; f++)
            for (int z = 0; z < 3; z++)
            for (int x = 0; x < 7; x++)
            {
                if (StairHole(f, x, z)) continue;
                Vector3 c = TileCentre(x, z, f * L.Storey);
                Transform tile = Group($"BarracksFloor_{f}_{x}_{z}", g);
                tile.position = c;
                var col = tile.gameObject.AddComponent<BoxCollider>();
                col.center = Vector3.down * (L.Slab * .5f);
                col.size = new Vector3(L.Tile, L.Slab, L.Tile);
                Transform visual = Group("Visual", tile);
                K.WorldBox("Boards", visual, c + Vector3.down * (L.Slab * .5f), new Vector3(L.Tile, L.Slab, L.Tile), art.Board, false);
                K.WorldBox("Ceiling", visual, c + Vector3.down * (L.Slab + .01f), new Vector3(L.Tile - .02f, .02f, L.Tile - .02f), art.Ceiling, false);
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
                floors.Add(new IslandCellhouse.Tile { Transform = tile, Collapsible = true, Cell = new Vector2Int(x, z) });
            }
        }

        private static void Stairs(Transform g, IslandPrisonArt art, List<IslandFlights.Built> flights)
        {
            flights.Add(IslandFlights.Flight(g, new L.Flight("BarracksStair_1", new Vector3(122.4f, 0f, 13.8f), Vector3.right, 5.6f, 4f, 1.6f, false), art.Board, art.DarkWood));
            flights.Add(IslandFlights.Flight(g, new L.Flight("BarracksStair_2", new Vector3(129.6f, 4f, 12f), Vector3.left, 5.6f, 4f, 1.6f, false), art.Board, art.DarkWood));
            // Guard rails round the stairwell on both upper floors.
            // Floor 1: south edge (120-128) and west edge (120).
            K.Railing(g, "WellRail_S1", new Vector3(120f, 4f, 11f), new Vector3(128f, 4f, 11f), 1f, art.DarkWood);
            K.Railing(g, "WellRail_W1", new Vector3(120f, 4f, 11f), new Vector3(120f, 4f, 14.6f), 1f, art.DarkWood);
            // Floor 2: hole is at x=5 (124-128). South edge (124-128), east edge (128), and west edge north of the stair (124, 12.8-14.6).
            K.Railing(g, "WellRail_S2", new Vector3(124f, 8f, 11f), new Vector3(128f, 8f, 11f), 1f, art.DarkWood);
            K.Railing(g, "WellRail_E2", new Vector3(128f, 8f, 11f), new Vector3(128f, 8f, 14.6f), 1f, art.DarkWood);
            K.Railing(g, "WellRail_W2", new Vector3(124f, 8f, 12.8f), new Vector3(124f, 8f, 14.6f), 1f, art.DarkWood);
        }

        private static void Point(List<LootPointSpec> loot, string name, string tag, float x, float y, float z, CarryClass min, CarryClass max, bool jackpot = false) =>
            loot.Add(new LootPointSpec(name, tag, new Vector3(x, y, z), min, max, jackpot));

        // Lockers go in their own (non-static) group: their doors move.
        private static void Ground(Transform g, Transform lockers, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            // Armoury: empty gun racks along the walls, an ammunition crate, the issue counter.
            for (int i = 0; i < 3; i++)
                Box("GunRack", g, new Vector3(B.xMin + .6f, 1f, 5f + i * 3f), new Vector3(.4f, 1.8f, 2.4f), art.DarkWood);
            Box("IssueCounter", g, new Vector3(110f, .55f, 9f), new Vector3(.8f, 1.1f, 4f), art.DarkWood);
            Box("AmmoCrate", g, new Vector3(107.5f, .3f, 13.4f), new Vector3(1.1f, .6f, .7f), art.Board);
            Point(loot, "Armoury_Counter", IslandLootItems.Armoury, 110f, 1.15f, 9.8f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Armoury_Floor", IslandLootItems.Armoury, 107.5f, .05f, 7f, CarryClass.OneHand, CarryClass.Heavy, true);
            // Mess: two tables and benches, a coffee urn, a notice board stripped bare.
            foreach (float x in new[] { 114.5f, 117.5f })
            {
                Box("MessTable", g, new Vector3(x, .38f, 10f), new Vector3(1f, .76f, 4.2f), art.DarkWood);
                Box("Bench_W", g, new Vector3(x - .8f, .23f, 10f), new Vector3(.35f, .46f, 4f), art.DarkWood);
                Box("Bench_E", g, new Vector3(x + .8f, .23f, 10f), new Vector3(.35f, .46f, 4f), art.DarkWood);
            }
            Primitive(PrimitiveType.Cylinder, "Urn", g, new Vector3(119.2f, 1f, 14f), new Vector3(.4f, .25f, .4f), art.Steel, false);
            Box("UrnStand", g, new Vector3(119.2f, .4f, 14f), new Vector3(.9f, .8f, .6f), art.DarkWood);
            Point(loot, "Mess_Table", IslandLootItems.Contraband, 114.5f, .82f, 9f, CarryClass.Pocket, CarryClass.OneHand);
            Point(loot, "Mess_Floor", IslandLootItems.Maritime, 116f, .05f, 5f, CarryClass.Pocket, CarryClass.TwoHand);
            // Locker room: the guards' lockers (hiding places) along the east wall and the partition, a bench between.
            Box("LockerBench", g, new Vector3(126f, .23f, 7.5f), new Vector3(3f, .46f, .4f), art.DarkWood);
            Point(loot, "Lockers_Bench", IslandLootItems.Armoury, 126f, .5f, 7.5f, CarryClass.Pocket, CarryClass.OneHand);
            int n = 0;
            for (float z = 4.2f; z < 10.4f; z += .78f)
            {
                Locker(lockers, n++, new Vector3(B.xMax - Wall, 0f, z), Vector3.left, art);
                if (z < 9.5f) Locker(lockers, n++, new Vector3(120.1f, 0f, z + 1f), Vector3.right, art);
            }
        }

        // A guard's steel locker, the mall's kind: 0.75 wide, 1.95 tall, slats at eye height.
        private static void Locker(Transform g, int i, Vector3 back, Vector3 outward, IslandPrisonArt art)
        {
            if (IslandHideSpots.Full) return;
            Transform root = Group($"Locker_{i:00}", g);
            root.SetPositionAndRotation(back, Quaternion.LookRotation(outward));
            const float w = .74f, h = 1.95f, d = .55f;
            Box("Back", root, new Vector3(0f, h * .5f, .015f), new Vector3(w, h, .03f), art.Steel, false);
            Box("Side_L", root, new Vector3(-w * .5f, h * .5f, d * .5f), new Vector3(.03f, h, d), art.Steel, false);
            Box("Side_R", root, new Vector3(w * .5f, h * .5f, d * .5f), new Vector3(.03f, h, d), art.Steel, false);
            Box("Top", root, new Vector3(0f, h - .015f, d * .5f), new Vector3(w, .03f, d), art.Steel, false);
            Transform door = Group("Door", root);
            door.localPosition = new Vector3(-w * .5f, 0f, d);
            Box("Lower", door, new Vector3(w * .5f, .7f, 0f), new Vector3(w - .02f, 1.4f, .02f), art.Steel, false);
            Box("Upper", door, new Vector3(w * .5f, 1.88f, 0f), new Vector3(w - .02f, .14f, .02f), art.Steel, false);
            for (int s = 0; s < 5; s++)
                Box("Slat", door, new Vector3(w * .5f, 1.44f + s * .075f, 0f), new Vector3(w - .1f, .018f, .03f), art.Steel, false)
                    .transform.localRotation = Quaternion.Euler(35f, 0f, 0f);
            Box("Handle", door, new Vector3(w - .08f, 1.05f, .02f), new Vector3(.03f, .14f, .03f), art.Bars, false);
            IslandHideSpots.Wire(root, door, new Vector3(w, h, d), new Vector3(0f, .02f, d * .5f), HideSpotKind.Locker);
        }

        private static void Upper(Transform g, IslandPrisonArt art, List<LootPointSpec> loot)
        {
            float y1 = L.Storey, y2 = 2f * L.Storey;
            // Dormitory: rows of iron double bunks, a footlocker at each.
            int n = 0;
            for (float x = B.xMin + 1.5f; x < 118f; x += 2.6f)
            foreach (float z in new[] { 4.6f, 9.2f })
            {
                Box("BunkFrame", g, new Vector3(x, y1 + .9f, z), new Vector3(.95f, 1.8f, 2f), art.Rust);
                Box("Mattress_L", g, new Vector3(x, y1 + .48f, z), new Vector3(.85f, .1f, 1.9f), art.Mattress, false);
                if (K.Hash(n, 5) > .4f) Box("Mattress_U", g, new Vector3(x, y1 + 1.5f, z), new Vector3(.85f, .1f, 1.9f), art.Mattress, false);
                Box("Footlocker", g, new Vector3(x, y1 + .22f, z + 1.3f), new Vector3(.8f, .44f, .45f), art.DarkWood);
                if (K.Hash(n++, 6) < .3f) Point(loot, "Dorm_Footlocker_" + n, IslandLootItems.Armoury, x, y1 + .5f, z + 1.3f, CarryClass.Pocket, CarryClass.OneHand);
            }
            Point(loot, "Dorm_Floor_A", IslandLootItems.Maritime, 108f, y1 + .05f, 7f, CarryClass.Pocket, CarryClass.TwoHand);
            Point(loot, "Dorm_Floor_B", IslandLootItems.Contraband, 124f, y1 + .05f, 6f, CarryClass.Pocket, CarryClass.TwoHand);
            // Officers: a desk and a bed each side of the partition; a wardrobe fallen across one doorway's side.
            foreach (float x in new[] { 109f, 122f })
            {
                Box("OfficerDesk", g, new Vector3(x, y2 + .38f, 5f), new Vector3(1.5f, .76f, .75f), art.DarkWood);
                Box("OfficerBed", g, new Vector3(x + 3f, y2 + .25f, 8.5f), new Vector3(1f, .5f, 2.1f), art.DarkWood);
                Box("Wardrobe", g, new Vector3(x - 3f, y2 + 1f, 13.8f), new Vector3(1.2f, 2f, .6f), art.DarkWood);
                Point(loot, "Officer_Desk_" + x, IslandLootItems.Warden, x, y2 + .8f, 5f, CarryClass.Pocket, CarryClass.OneHand);
                Point(loot, "Officer_Floor_" + x, IslandLootItems.Maritime, x + 1.5f, y2 + .05f, 11f, CarryClass.OneHand, CarryClass.TwoHand);
            }
        }

        // The guard tower: four legs, a ladder on the south face, a cabin (one walkway section that can fall) with a
        // fixed searchlight, a parapet with glass long gone.
        private static void Tower(Transform g, IslandPrisonArt art, List<LootPointSpec> loot, List<IslandCellhouse.Tile> floors)
        {
            Rect t = L.GuardTower;
            float y = L.GuardCabinY;
            foreach (float x in new[] { t.xMin + .15f, t.xMax - .15f })
            foreach (float z in new[] { t.yMin + .15f, t.yMax - .15f })
                Box("Leg", g, new Vector3(x, y * .5f, z), new Vector3(.3f, y, .3f), art.Steel);
            for (float yy = 2.5f; yy < y; yy += 3f)
            {
                K.Span("Brace_S", g, new Vector3(t.xMin, yy, t.yMin + .1f), new Vector3(t.xMax, yy + .12f, t.yMin + .2f), art.Steel, false);
                K.Span("Brace_N", g, new Vector3(t.xMin, yy, t.yMax - .2f), new Vector3(t.xMax, yy + .12f, t.yMax - .1f), art.Steel, false);
            }
            Transform cabin = Group("GuardCabin", g);
            cabin.position = new Vector3(t.center.x, y, t.center.y);
            var col = cabin.gameObject.AddComponent<BoxCollider>();
            col.center = Vector3.down * .15f;
            col.size = new Vector3(t.width, .3f, t.height);
            Transform visual = Group("Visual", cabin);
            K.WorldBox("Floor", visual, cabin.position + Vector3.down * .15f, new Vector3(t.width, .3f, t.height), art.Board, false);
            // Parapet walls with a gap over the ladder; posts carrying the roof.
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(t.xMin, y, t.yMax - .1f), new Vector3(t.xMax, y + 1.1f, t.yMax)),
                (new Vector3(t.xMin, y, t.yMin), new Vector3(t.xMin + .1f, y + 1.1f, t.yMax)), (new Vector3(t.xMax - .1f, y, t.yMin), new Vector3(t.xMax, y + 1.1f, t.yMax)),
                (new Vector3(t.xMin, y, t.yMin), new Vector3(t.center.x - .6f, y + 1.1f, t.yMin + .1f)), (new Vector3(t.center.x + .6f, y, t.yMin), new Vector3(t.xMax, y + 1.1f, t.yMin + .1f)) })
            {
                GameObject wall = K.Span("Parapet", visual, a, b, art.FacadeDark, false);
                Transform solid = Group("ParapetSolid", cabin);
                solid.position = (a + b) * .5f;
                solid.gameObject.AddComponent<BoxCollider>().size = b - a;
                _ = wall;
            }
            foreach (float x in new[] { t.xMin + .1f, t.xMax - .1f })
            foreach (float z in new[] { t.yMin + .1f, t.yMax - .1f })
                K.Span("Post", visual, new Vector3(x - .06f, y + 1.1f, z - .06f), new Vector3(x + .06f, y + 2.5f, z + .06f), art.Steel, false);
            K.Span("Roof", visual, new Vector3(t.xMin - .3f, y + 2.5f, t.yMin - .3f), new Vector3(t.xMax + .3f, y + 2.7f, t.yMax + .3f), art.Rust, false);
            // The searchlight is a fitting bolted to the parapet (there is no searchlight loot, so nothing looks like it).
            Box("SearchlightMount", visual, new Vector3(t.xMax - .5f, y + 1.3f, t.yMax - .5f), new Vector3(.2f, .4f, .2f), art.Steel, false);
            Primitive(PrimitiveType.Cylinder, "Searchlight", visual, new Vector3(t.xMax - .5f, y + 1.65f, t.yMax - .7f), new Vector3(.55f, .3f, .55f), art.Steel, false)
                .transform.rotation = Quaternion.Euler(70f, 30f, 0f);
            cabin.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            floors.Add(new IslandCellhouse.Tile { Transform = cabin, Collapsible = true, Cell = new Vector2Int(-1, -1) });
            // Pocket loot only: you climb down with empty hands.
            Point(loot, "Tower_Cabin", IslandLootItems.Armoury, t.center.x - .8f, y + .05f, t.center.y + .8f, CarryClass.Pocket, CarryClass.Pocket);
            Point(loot, "Tower_Cabin_B", IslandLootItems.Maritime, t.center.x + .8f, y + .05f, t.center.y + .4f, CarryClass.Pocket, CarryClass.Pocket);

            // The ladder: its foot on the quay at the cabin's south edge, facing out.
            Transform ladder = Group("TowerLadder", g);
            ladder.SetPositionAndRotation(new Vector3(t.center.x, 0f, t.yMin), Quaternion.LookRotation(Vector3.back));
            float stile = y + 1.1f;
            foreach (float x in new[] { -.25f, .25f })
                Box(x < 0 ? "Stile_L" : "Stile_R", ladder, new Vector3(x, stile * .5f, .06f), new Vector3(.06f, stile, .06f), art.Rust, false);
            for (float yy = .3f; yy < y; yy += .3f)
                Box("Rung", ladder, new Vector3(0f, yy, .06f), new Vector3(.5f, .04f, .04f), art.Steel, false);
            ladder.gameObject.AddComponent<Ladder>().EditorSetup(y);
        }

    }
}
