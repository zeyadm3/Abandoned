using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.Structure;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using K = Abandoned.EditorTools.IslandKit;
using O = Abandoned.EditorTools.IslandKit.Opening;
using L = Abandoned.EditorTools.IslandPrisonLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The cellhouse: a 48 x 52 m concrete hall, 13 m to the eaves, with tall barred two-light windows, one wide main
    /// door on Broadway's axis and three side doors, a flat roof with a glazed monitor down Broadway (half its panes
    /// gone: the leaks, the moss and the moonlight are under it), steel trusses, and the inside walls: the dining
    /// band across the north end, the admin office and the library either side of the entrance hall, and the basement
    /// under the dining band. The ground floor is one structural section per 4 m tile; only the tiles over the basement
    /// can fall (into the laundry and showers below).
    /// </summary>
    public static class IslandCellhouse
    {
        public sealed class Tile
        {
            public Transform Transform;
            public bool Collapsible;
            public Vector2Int Cell;
        }

        public static List<Tile> Build(Transform root, IslandPrisonArt art)
        {
            List<Tile> tiles = Floor(Group("GroundFloor", root.Find("Levels") ?? Group("Levels", root)), art);
            Transform shell = Group("Cellhouse", root);
            Exterior(Group("Walls", shell), art);
            Interior(Group("Inside", shell), art);
            Basement(Group("Basement", shell), art);
            MarkStatic(shell);
            Transform roof = Group("CellhouseRoof", root);
            Roof(roof, art);
            MarkStatic(roof);
            return tiles;
        }

        private static List<Tile> Floor(Transform g, IslandPrisonArt art)
        {
            var tiles = new List<Tile>();
            for (int z = 0; z < L.CellhouseTilesZ; z++)
            for (int x = 0; x < L.CellhouseTilesX; x++)
            {
                var cell = new Vector2Int(x, z);
                if (L.IsBasementStairHole(cell)) continue;
                Vector3 c = L.CellhouseTileCentre(cell, L.PlateauY);
                Transform tile = Group($"Tile_{x}_{z}", g);
                tile.position = c;
                var col = tile.gameObject.AddComponent<BoxCollider>();
                col.center = Vector3.down * (L.Slab * .5f);
                col.size = new Vector3(L.Tile, L.Slab, L.Tile);
                bool south = c.z < L.SouthBand.yMax, north = L.OverBasement(cell), kitchen = north && c.x > L.KitchenWest;
                Material m = kitchen ? art.Tile : south || north ? art.Terrazzo : art.Floor;
                Transform visual = Group("Visual", tile);
                K.WorldBox("Slab", visual, c + Vector3.down * (L.Slab * .5f), new Vector3(L.Tile, L.Slab, L.Tile), m, false);
                // Over the basement the slab's underside is the laundry's ceiling: give it its own face.
                if (north) K.WorldBox("Soffit", visual, c + Vector3.down * (L.Slab + .01f), new Vector3(L.Tile - .02f, .02f, L.Tile - .02f), art.Ceiling, false);
                tile.gameObject.AddComponent<SurfaceTag>().EditorSet(kitchen ? SurfaceMaterial.Tile : south || north ? SurfaceMaterial.Terrazzo : SurfaceMaterial.Concrete);
                tiles.Add(new Tile { Transform = tile, Collapsible = north, Cell = cell });
            }
            return tiles;
        }

        public static void AddSections(StructureSceneSetup setup, IEnumerable<Tile> tiles)
        {
            foreach (Tile t in tiles)
            {
                setup.AddSection(t.Transform, SectionType.Floor, t.Collapsible, t.Collapsible ? .8f : 1f);
                if (t.Collapsible) SectionNavCarver.EditorAdd(t.Transform.gameObject, new Vector3(L.Tile, L.Slab, L.Tile));
                // Broadway and the main door's approach stay clear of rubble: the haul route out.
                else if (t.Cell.x is 5 or 6) t.Transform.gameObject.AddComponent<SectionKeepClear>();
            }
        }

        // Windows every 4 m: a low light (13.2-15.4) and a tall one above (17-22.6), barred, half the glass gone.
        private static O[] Windows(float from, float to, float skipA = float.NaN, float skipB = float.NaN, float lowFrom = float.MaxValue)
        {
            var list = new List<O>();
            for (float at = from + 2f; at < to - 1f; at += 4f)
            {
                if (!float.IsNaN(skipA) && Mathf.Abs(at - skipA) < 3.2f) continue;
                if (!float.IsNaN(skipB) && Mathf.Abs(at - skipB) < 2.6f) continue;
                list.Add(O.Window(at, 1.6f, at >= lowFrom ? 8.5f : 1.2f, at >= lowFrom ? 11.3f : 10.6f));
            }
            return list.ToArray();
        }

        private static void Exterior(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            float h = L.CellhouseEaves - L.PlateauY, t = L.CellhouseWall, y = L.PlateauY;
            var south = new List<O>(Windows(c.xMin, c.xMax, L.MainDoorX)) { O.Door(L.MainDoorX, L.MainDoorWidth, L.MainDoorHeight) };
            K.WallX(g, "South", c.xMin, c.xMax, c.yMin + t * .5f, y, h, t, art.Facade, south.ToArray());
            var north = new List<O>(Windows(c.xMin, c.xMax, float.NaN, L.ServiceDoorX)) { O.Door(L.ServiceDoorX, L.SideDoorWidth, L.SideDoorHeight) };
            // The north wall goes down to the basement floor: it's the basement's outer wall too.
            K.WallX(g, "North", c.xMin, c.xMax, c.yMax - t * .5f, y, h, t, art.Facade, north.ToArray());
            // West and east: over D- and A-block's two tiers of cells only the top lights are glazed.
            var west = new List<O>(Windows(c.yMin, c.yMax, float.NaN, L.YardDoorZ, L.StackSouth)) { O.Door(L.YardDoorZ, L.SideDoorWidth, L.SideDoorHeight) };
            K.WallZ(g, "West", c.yMin, c.yMax, c.xMin + t * .5f, y, h, t, art.Facade, west.ToArray());
            var east = new List<O>(Windows(c.yMin, c.yMax, float.NaN, L.EastDoorZ, L.StackSouth)) { O.Door(L.EastDoorZ, L.SideDoorWidth, L.SideDoorHeight) };
            K.WallZ(g, "East", c.yMin, c.yMax, c.xMax - t * .5f, y, h, t, art.Facade, east.ToArray());
            WindowFill(Group("Windows", g), art);

            // Plinth, string course and cornice: the building reads as poured concrete with some ambition, long ago.
            foreach ((Vector3 a, Vector3 b) in new[] { (new Vector3(c.xMin - .15f, 0f, c.yMin - .15f), new Vector3(c.xMax + .15f, 0f, c.yMin + .1f)),
                (new Vector3(c.xMin - .15f, 0f, c.yMax - .1f), new Vector3(c.xMax + .15f, 0f, c.yMax + .15f)),
                (new Vector3(c.xMin - .15f, 0f, c.yMin), new Vector3(c.xMin + .1f, 0f, c.yMax)),
                (new Vector3(c.xMax - .1f, 0f, c.yMin), new Vector3(c.xMax + .15f, 0f, c.yMax)) })
            {
                K.Span("Plinth", g, a + Vector3.up * y, b + Vector3.up * (y + .9f), art.FacadeDark, false);
                K.Span("Course", g, a + Vector3.up * (y + 4.6f), b + Vector3.up * (y + 4.85f), art.FacadeDark, false);
                K.Span("Cornice", g, a - new Vector3(.2f, 0f, .2f) + Vector3.up * (L.CellhouseEaves - .1f), b + new Vector3(.2f, 0f, .2f) + Vector3.up * (L.CellhouseEaves + .5f), art.FacadeDark, false);
            }
            // The main entrance: a heavy concrete surround, a canopy and a sally-port grille rolled up into its box.
            float mx = L.MainDoorX, mw = L.MainDoorWidth;
            K.Span("Surround_L", g, new Vector3(mx - mw * .5f - .7f, y, c.yMin - .5f), new Vector3(mx - mw * .5f, y + L.MainDoorHeight + .6f, c.yMin), art.FacadeDark);
            K.Span("Surround_R", g, new Vector3(mx + mw * .5f, y, c.yMin - .5f), new Vector3(mx + mw * .5f + .7f, y + L.MainDoorHeight + .6f, c.yMin), art.FacadeDark);
            K.Span("Canopy", g, new Vector3(mx - mw * .5f - 1.2f, y + L.MainDoorHeight + .6f, c.yMin - 2.6f), new Vector3(mx + mw * .5f + 1.2f, y + L.MainDoorHeight + .9f, c.yMin), art.FacadeDark, false);
            K.Span("GrilleBox", g, new Vector3(mx - mw * .5f, y + L.MainDoorHeight, c.yMin + t), new Vector3(mx + mw * .5f, y + L.MainDoorHeight + .5f, c.yMin + t + .4f), art.Steel, false);
            K.Span("Step", g, new Vector3(mx - mw * .5f - 1f, y, c.yMin - 2.4f), new Vector3(mx + mw * .5f + 1f, y + .08f, c.yMin), art.FacadeDark, false);
        }

        // Every window opening: bars that stop anyone climbing out (one collider each) and glass, much of it broken.
        private static void WindowFill(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            int n = 0;
            void Fill(Vector3 centre, bool alongX, float width, float y0, float y1, bool transom)
            {
                n++;
                if (transom)
                {
                    float w = L.CellhouseWall;
                    K.WorldBox("Transom", g, centre + Vector3.up * ((y0 + y1) * .5f), alongX ? new Vector3(width, y1 - y0, w) : new Vector3(w, y1 - y0, width), art.Facade);
                    return;
                }
                Vector3 across = alongX ? Vector3.right : Vector3.forward;
                Vector3 size = alongX ? new Vector3(width, y1 - y0, .08f) : new Vector3(.08f, y1 - y0, width);
                Vector3 mid = centre + Vector3.up * ((y0 + y1) * .5f);
                GameObject bars = K.WorldBox("BarsSolid", g, mid, size, art.Bars);
                Object.DestroyImmediate(bars.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(bars.GetComponent<MeshFilter>());
                for (float s = -width * .5f + .15f; s < width * .5f; s += .2f)
                    K.WorldBox("Bar", g, mid + across * s, alongX ? new Vector3(.03f, y1 - y0, .03f) : new Vector3(.03f, y1 - y0, .03f), art.Bars, false);
                foreach (float yy in new[] { y0 + .02f, y1 - .02f, (y0 + y1) * .5f })
                    K.WorldBox("Rail", g, new Vector3(mid.x, yy, mid.z), alongX ? new Vector3(width, .05f, .05f) : new Vector3(.05f, .05f, width), art.Bars, false);
                // Glass: a quarter of the windows whole, half cracked to a few panes, a quarter empty.
                float h = K.Hash((int)(centre.x * 3f), (int)(centre.z * 3f + y0));
                if (h < .25f) return;
                int panes = h > .75f ? 6 : 3;
                for (int i = 0; i < panes; i++)
                {
                    float py = y0 + (y1 - y0) * (i + .5f) / panes;
                    if (h < .75f && K.Hash(i, n) < .45f) continue;
                    Vector3 pc = new Vector3(mid.x, py, mid.z) + (alongX ? Vector3.forward : Vector3.right) * .12f;
                    K.WorldBox("Pane", g, pc, alongX ? new Vector3(width - .05f, (y1 - y0) / panes - .05f, .01f) : new Vector3(.01f, (y1 - y0) / panes - .05f, width - .05f), art.Glass, false);
                }
            }
            float t = L.CellhouseWall;
            foreach (O o in Windows(c.xMin, c.xMax, L.MainDoorX)) Pair(o, (at, y0, y1, tr) => Fill(new Vector3(at, L.PlateauY, c.yMin + t * .5f), true, o.Width, y0, y1, tr));
            foreach (O o in Windows(c.xMin, c.xMax, float.NaN, L.ServiceDoorX)) Pair(o, (at, y0, y1, tr) => Fill(new Vector3(at, L.PlateauY, c.yMax - t * .5f), true, o.Width, y0, y1, tr));
            foreach (O o in Windows(c.yMin, c.yMax, float.NaN, L.YardDoorZ, L.StackSouth)) Pair(o, (at, y0, y1, tr) => Fill(new Vector3(c.xMin + t * .5f, L.PlateauY, at), false, o.Width, y0, y1, tr));
            foreach (O o in Windows(c.yMin, c.yMax, float.NaN, L.EastDoorZ, L.StackSouth)) Pair(o, (at, y0, y1, tr) => Fill(new Vector3(c.xMax - t * .5f, L.PlateauY, at), false, o.Width, y0, y1, tr));
            MarkStatic(g);
        }

        // A window opening is one tall gap in the wall; it becomes two lights with a solid concrete transom between
        // them (y 3.4-5 over the floor), so nothing at gallery height opens to the outside.
        private static void Pair(O o, System.Action<float, float, float, bool> fill)
        {
            if (o.Sill > 5f) { fill(o.At, o.Sill, o.Top, false); return; }
            fill(o.At, o.Sill, 3.4f, false);
            fill(o.At, 3.4f, 5f, true);
            fill(o.At, 5f, o.Top, false);
        }

        private static void Interior(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            float y = L.PlateauY, top = L.CellhouseEaves - L.PlateauY;
            float nb = L.NorthBand.yMin;
            // The dining band's wall: full height, from the basement floor (it stands on the basement's south face).
            // Broadway opens straight into the dining hall; D-aisle and A-aisle each have a door.
            K.WallX(g, "DiningWall", c.xMin + .5f, c.xMax - .5f, nb, y, top, .35f, art.Plaster,
                O.Door(54f, 10.8f, 4f), O.Door(36f, 2.6f, 2.9f), O.Door(72f, 2.6f, 2.9f));
            // Dining hall / kitchen, on the basement partition below: a serving hatch and a door.
            K.WallZ(g, "KitchenWall", nb + .2f, c.yMax - .5f, L.KitchenWest, y, L.NorthBandCeiling - y, .25f, art.Tile,
                O.Door(83f, 2.4f, 2.6f), O.Window(88.5f, 3f, 1f, 2.1f));
            // The band's ceiling: a lid at 5 m, the roof void above (the north wraps of the galleries look down on it).
            K.Span("DiningCeiling", g, new Vector3(c.xMin + .5f, L.NorthBandCeiling, nb + .2f), new Vector3(c.xMax - .5f, L.NorthBandCeiling + .3f, c.yMax - .5f), art.Ceiling);
            // Admin and library: two 4 m rooms in the south band, ceilinged, each with a door to its aisle and the hall.
            // Hall doors sit south of the stair towers' feet (z 44), so a flight's rail never blocks them.
            Room(g, "Admin", L.Admin, art, O.Door(42.3f, 1.4f, 2.4f), O.Door(36f, 1.6f, 2.4f), true);
            Room(g, "Library", L.Library, art, O.Door(42.3f, 1.4f, 2.4f), O.Door(72f, 1.6f, 2.4f), false);
        }

        private static void Room(Transform g, string name, Rect r, IslandPrisonArt art, O hallDoor, O aisleDoor, bool hallOnEast)
        {
            float y = L.PlateauY, h = L.RoomCeiling - y;
            Transform room = Group(name, g);
            float hallX = hallOnEast ? r.xMax : r.xMin;
            K.WallZ(room, name + "_HallWall", r.yMin + .5f, r.yMax, hallX, y, h, .22f, art.Plaster, hallDoor);
            float x0 = hallOnEast ? r.xMin + .5f : r.xMin, x1 = hallOnEast ? r.xMax : r.xMax - .5f;
            K.WallX(room, name + "_AisleWall", x0, x1, r.yMax, y, h, .22f, art.Plaster, aisleDoor);
            K.Span(name + "_Ceiling", room, new Vector3(x0, L.RoomCeiling, r.yMin + .5f), new Vector3(x1, L.RoomCeiling + .25f, r.yMax + .11f), art.Ceiling);
        }

        private static void Basement(Transform g, IslandPrisonArt art)
        {
            Rect b = Rect.MinMaxRect(L.Cellhouse.xMin + .5f, L.NorthBand.yMin + .2f, L.Cellhouse.xMax - .5f, L.Cellhouse.yMax - .5f);
            float y = L.BasementY, h = L.Storey - L.Slab;
            // The rock round it, faced in painted block.
            K.Span("Face_W", g, new Vector3(b.xMin - .2f, y, b.yMin), new Vector3(b.xMin, y + h, b.yMax), art.Block);
            K.Span("Face_E", g, new Vector3(b.xMax, y, b.yMin), new Vector3(b.xMax + .2f, y + h, b.yMax), art.Block);
            K.Span("Face_N", g, new Vector3(b.xMin, y, b.yMax), new Vector3(b.xMax, y + h, b.yMax + .2f), art.Block);
            // The south face: the rock under the cell blocks, faced in block like the rest.
            K.Span("Face_S", g, new Vector3(b.xMin, y, b.yMin - .4f), new Vector3(b.xMax, y + h, b.yMin), art.Block);
            foreach (float x in L.BasementPartitions)
                K.WallZ(g, "Partition_" + x, b.yMin, b.yMax, x, y, h, .25f, art.Block, O.Door(L.BasementDoorZ, 2.4f, 2.6f));
            // Wet floor: the north band's drains backed up long ago.
            for (int i = 0; i < 6; i++)
            {
                float hx = K.Hash(i, 51), hz = K.Hash(51, i);
                K.Patch("Puddle", g, new Vector3(Mathf.Lerp(b.xMin + 3f, b.xMax - 3f, hx), y + .03f, Mathf.Lerp(b.yMin + 1.5f, b.yMax - 1.5f, hz)),
                    new Vector2(1.6f + hx * 2f, 1.2f + hz * 2f), hx * 180f, art.Water);
            }
            // Concrete beams under the dining floor (they carry nothing in the game; the tiles are the structure).
            for (float x = L.Cellhouse.xMin + 8f; x < L.Cellhouse.xMax - 1f; x += 8f)
            {
                // A downstand across the shaft hits the player's head on the last few treads. Trim it to the
                // actual slab edges, rather than hanging a beam across the deliberately open stair tiles.
                float z = b.yMin;
                foreach (Vector2Int hole in L.BasementStairHoles)
                {
                    float hx = L.Cellhouse.xMin + hole.x * L.Tile;
                    if (x + .2f <= hx || x - .2f >= hx + L.Tile) continue;
                    float hz = L.Cellhouse.yMin + hole.y * L.Tile;
                    Beam(z, Mathf.Max(z, hz - .15f));
                    z = Mathf.Max(z, hz + L.Tile + .15f);
                }
                Beam(z, b.yMax);

                void Beam(float from, float to)
                {
                    if (to - from < .05f) return;
                    K.Span("Beam", g, new Vector3(x - .2f, L.PlateauY - L.Slab - .45f, from),
                        new Vector3(x + .2f, L.PlateauY - L.Slab - .01f, to), art.FacadeDark, false);
                }
            }
        }

        private static void Roof(Transform g, IslandPrisonArt art)
        {
            Rect c = L.Cellhouse;
            float e = L.CellhouseEaves;
            // Broadway's monitor: the strip over it is open, with a raised glazed lantern; everything else is slab.
            float mx0 = L.Stacks[0].East, mx1 = L.Stacks[1].West, mz0 = L.StackSouth - 2f, mz1 = L.StackNorth + 2f;
            K.Span("Roof_W", g, new Vector3(c.xMin, e, c.yMin), new Vector3(mx0, e + .4f, c.yMax), art.FacadeDark);
            K.Span("Roof_E", g, new Vector3(mx1, e, c.yMin), new Vector3(c.xMax, e + .4f, c.yMax), art.FacadeDark);
            K.Span("Roof_S", g, new Vector3(mx0, e, c.yMin), new Vector3(mx1, e + .4f, mz0), art.FacadeDark);
            K.Span("Roof_N", g, new Vector3(mx0, e, mz1), new Vector3(mx1, e + .4f, c.yMax), art.FacadeDark);
            const float lantern = 1.8f;
            K.Span("Lantern_W", g, new Vector3(mx0, e + .4f, mz0), new Vector3(mx0 + .25f, e + lantern, mz1), art.Steel, false);
            K.Span("Lantern_E", g, new Vector3(mx1 - .25f, e + .4f, mz0), new Vector3(mx1, e + lantern, mz1), art.Steel, false);
            K.Span("Lantern_S", g, new Vector3(mx0, e + .4f, mz0), new Vector3(mx1, e + lantern, mz0 + .25f), art.Steel, false);
            K.Span("Lantern_N", g, new Vector3(mx0, e + .4f, mz1 - .25f), new Vector3(mx1, e + lantern, mz1), art.Steel, false);
            // The glazing: steel glazing bars every 1.2 m; panes in about half the bays (the rest let the rain in).
            int bay = 0;
            for (float z = mz0; z < mz1 - .1f; z += 1.2f, bay++)
            {
                K.Span("GlazingBar", g, new Vector3(mx0, e + lantern, z), new Vector3(mx1, e + lantern + .08f, z + .06f), art.Steel, false);
                foreach (bool west in new[] { true, false })
                {
                    if (K.Hash(bay, west ? 1 : 2) < .5f) continue;
                    float x0 = west ? mx0 : (mx0 + mx1) * .5f, x1 = west ? (mx0 + mx1) * .5f : mx1;
                    K.Span("Pane", g, new Vector3(x0 + .04f, e + lantern + .02f, z + .06f), new Vector3(x1 - .04f, e + lantern + .04f, z + 1.2f), art.Glass, false);
                }
            }
            K.Span("Ridge", g, new Vector3((mx0 + mx1) * .5f - .1f, e + lantern, mz0), new Vector3((mx0 + mx1) * .5f + .1f, e + lantern + .2f, mz1), art.Steel, false);
            // Trusses across the hall every 6 m, above the stacks' tops (24 m) and the stair landings.
            for (float z = c.yMin + 3f; z < c.yMax; z += 6f)
            {
                K.Span("Truss_Bottom", g, new Vector3(c.xMin + .5f, e - .9f, z - .1f), new Vector3(c.xMax - .5f, e - .7f, z + .1f), art.Steel, false);
                for (float x = c.xMin + 2f; x < c.xMax - 1f; x += 4f)
                {
                    GameObject web = K.WorldBox("Truss_Web", g, new Vector3(x, e - .4f, z), new Vector3(.08f, .9f, .08f), art.Steel, false);
                    web.transform.rotation = Quaternion.Euler(0f, 0f, ((int)x / 4) % 2 == 0 ? 35f : -35f);
                }
            }
            // The open monitor admits the real sun. Tall crossed transparent rectangles looked like untextured
            // walls from the gallery and stacked up over the full hall; use actual light and local dust instead.
        }
    }
}
