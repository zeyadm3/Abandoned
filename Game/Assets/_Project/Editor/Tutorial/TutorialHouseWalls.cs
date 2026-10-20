using System.Collections.Generic;
using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using L = Abandoned.EditorTools.TutorialHouseLayout;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Every wall of the house, straight from the plan: exterior walls in two skins (vinyl siding outside over a
    /// block foundation, plaster inside), interior partitions, cased door openings and double-hung windows with
    /// frosted glass and half-drawn blinds. Glass is solid: nobody leaves through a window in this house.
    /// </summary>
    public static class TutorialHouseWalls
    {
        public readonly struct Hole
        {
            public readonly float From, To, Bottom, Top;
            public readonly bool Window;
            public Hole(float from, float to, float bottom, float top, bool window) { From = from; To = to; Bottom = bottom; Top = top; Window = window; }
        }

        public static Hole Door(float from, float to) => new(from, to, 0f, L.DoorHeight, false);
        public static Hole Win(float from, float to, float bottom = 0.9f, float top = 2.15f) => new(from, to, bottom, top, true);

        /// <summary>One outside wall of the plan: its line, its span, which way is out (+1 / -1) and its openings.</summary>
        public readonly struct OuterWall
        {
            public readonly string Name;
            public readonly bool Along;
            public readonly float At, From, To, Outward;
            public readonly Hole[] Holes;
            public OuterWall(string name, bool along, float at, float from, float to, float outward, params Hole[] holes)
            { Name = name; Along = along; At = at; From = from; To = to; Outward = outward; Holes = holes; }
        }

        /// <summary>
        /// The house's outside walls, windows and door openings: the same list for the training house and for every
        /// copy of it along the street (TutorialStreetHouseKit), so the two never drift apart.
        /// </summary>
        public static OuterWall[] OuterWalls()
        {
            const float wing = L.WingEast, south = L.BandSouth, north = L.BandNorth, east = L.HouseEast;
            float wingNorth = L.WestWing.yMax;
            return new[]
            {
                // ---- West wing ----
                new OuterWall("WestWing_West", false, 0f, 0f, wingNorth, -1f, Win(1.6f, 3.4f), Win(8.4f, 9.4f, 1.5f, 2.3f), Win(13.4f, 15.4f)),
                new OuterWall("WestWing_North", true, wingNorth, 0f, wing, 1f, Win(2f, 3.8f)),
                new OuterWall("WestWing_South", true, 0f, 0f, wing, -1f, Win(2.2f, 4.2f)),
                new OuterWall("WestWing_EastNorth", false, wing, north, wingNorth, 1f),
                new OuterWall("WestWing_EastPatio", false, wing, 0f, south, 1f, Win(4.5f, 5.8f, 1.3f, 2.3f)),
                // ---- The open band ----
                new OuterWall("Band_North", true, north, wing, east, 1f, Win(9.6f, 11f, 1.15f), Win(15.9f, 17.6f), Win(21.8f, 24.2f)),
                new OuterWall("Band_East", false, east, south, north, 1f,
                    Door(L.FrontDoorZ - L.FrontDoorWidth * .5f, L.FrontDoorZ + L.FrontDoorWidth * .5f), Win(7.1f, 8.5f)),
                new OuterWall("Band_SouthKitchen", true, south, wing, L.Kitchen.xMax, -1f, Door(L.SliderX0, L.SliderX1)),
                // ---- The suite ----
                new OuterWall("Suite_West", false, L.Suite.xMin, 0f, south, -1f, Win(1.8f, 2.8f, 1.5f, 2.3f)),
                new OuterWall("Suite_South", true, 0f, L.Suite.xMin, east, -1f, Win(15f, 16.2f, 1.5f, 2.3f), Win(21f, 23.2f)),
                new OuterWall("Suite_East", false, east, 0f, south, 1f, Win(2f, 4f)),
            };
        }

        public static void Build(Transform root, TutorialHouseArt art)
        {
            Transform walls = Group("Walls", root);
            Transform exterior = Group("Exterior", walls), interior = Group("Interior", walls), openings = Group("Openings", walls);
            const float wing = L.WingEast, south = L.BandSouth, north = L.BandNorth, east = L.HouseEast;
            float hallWest = L.Hallway.xMin, hallNorth = L.Hallway.yMax;
            Exterior(exterior, openings, art, full: true);

            // ---- Partitions (every doorway is a cased opening 1.8 m wide: nothing hinged stands out into a room) ----
            Inner(interior, openings, art, "BedroomB_HallBath", along: true, at: south, from: 0f, to: hallWest);
            Inner(interior, openings, art, "Hall_SouthEnd", along: true, at: south, from: hallWest, to: wing, Door(L.BedroomDoorX0, L.BedroomDoorX1));
            Inner(interior, openings, art, "Hall_BathSide", along: false, at: hallWest, from: south, to: hallNorth, Door(L.BathDoorZ0, L.BathDoorZ1));
            Inner(interior, openings, art, "HallBath_BedroomA", along: true, at: hallNorth, from: 0f, to: hallWest);
            Inner(interior, openings, art, "Hall_NorthEnd", along: true, at: hallNorth, from: hallWest, to: wing, Door(L.BedroomDoorX0, L.BedroomDoorX1));
            Inner(interior, openings, art, "Hall_Kitchen", along: false, at: wing, from: south, to: hallNorth, Door(L.HallwayOpeningZ0, L.HallwayOpeningZ1));
            Inner(interior, openings, art, "BedroomA_Kitchen", along: false, at: wing, from: hallNorth, to: north);
            Inner(interior, openings, art, "Band_Suite", along: true, at: south, from: L.Suite.xMin, to: east,
                Door(L.LaundryDoorX0, L.LaundryDoorX1), Door(L.MasterDoorX0, L.MasterDoorX1));
            Inner(interior, openings, art, "Laundry_Ensuite", along: true, at: L.Laundry.yMin, from: L.Suite.xMin, to: L.Master.xMin);
            Inner(interior, openings, art, "Ensuite_Master", along: false, at: L.Master.xMin, from: 0f, to: L.Laundry.yMin, Door(L.EnsuiteDoorZ0, L.EnsuiteDoorZ1));
            Inner(interior, openings, art, "Laundry_Master", along: false, at: L.Master.xMin, from: L.Laundry.yMin, to: south);
            // A short stub between dining and living: the open room still reads as two.
            Inner(interior, openings, art, "Dining_LivingStub", along: false, at: L.Living.xMin, from: north - 1.1f, to: north);

            // The front door's two leaves stand swung right back flat against the wall either side (no collider: a
            // door left open is never something to catch on).
            float half = L.FrontDoorWidth * .5f;
            FlatLeaf(openings, art, "FrontDoorLeaf_North", east, L.FrontDoorZ + half, 1f, half, wreath: true);
            FlatLeaf(openings, art, "FrontDoorLeaf_South", east, L.FrontDoorZ - half, -1f, half, wreath: false);
        }

        /// <summary>
        /// Every outside wall with its windows. <paramref name="full"/> is the house as you see it up close; without
        /// it (a street copy seen from down the road) only the siding, foundation, fascia and glass are built: no
        /// plaster skin, lap lines, casings or blinds.
        /// </summary>
        public static void Exterior(Transform exterior, Transform openings, TutorialHouseArt art, bool full)
        {
            OuterWall[] walls = OuterWalls();
            foreach (OuterWall wall in walls) Outer(exterior, openings, art, wall, full);
            CornerBoards(Group("Corners", exterior), walls, L.Ceiling, art);
        }

        /// <summary>
        /// One outside wall: siding outside over a block foundation, plaster inside, one centreline; the siding
        /// runs from 0.3 m to <paramref name="top"/> + 0.3 m (the eave). Also the street copies' garages.
        /// </summary>
        public static void Outer(Transform parent, Transform openings, TutorialHouseArt art, OuterWall w, bool full, float top = L.Ceiling)
        {
            Transform group = Group(w.Name, parent);
            const float skin = L.ExteriorWall * .5f;
            bool along = w.Along;
            float at = w.At, from = w.From, to = w.To, outward = w.Outward;
            Hole[] holes = w.Holes;
            if (full) Run(group, "Plaster", along, at - outward * skin * .5f, from, to, L.Floor, top + .05f, skin, art.Plaster, holes);
            Run(group, "Siding", along, at + outward * skin * .5f, from, to, .3f, top + .3f, skin, art.Siding, holes);
            Run(group, "Foundation", along, at + outward * skin * .55f, from, to, L.Grade - .05f, .3f, skin * 1.1f, art.BasementWall, FoundationHoles(holes));
            // Lap lines on the siding: thin horizontal shadows every 20 cm, broken by the openings.
            if (full)
                for (float y = .5f; y < top + .25f; y += .2f)
                    Run(group, "Lap", along, at + outward * (skin + .006f), from, to, y, y + .018f, .012f, art.SidingDark, holes, collider: false);
            Run(group, "Fascia", along, at + outward * (skin + .02f), from - .2f, to + .2f, top + .3f, top + .5f, .05f, art.Trim, null, collider: false);
            foreach (Hole h in holes) Opening(openings, art, w.Name, along, at, outward, h, full);
        }

        /// <summary>
        /// A trim board up every outside corner (and the foundation under it): two siding skins that each stop at the
        /// wall line leave a square notch there. Inside corners need none (the skins overlap). Found from the walls
        /// themselves: an east-west and a north-south wall sharing an end, each facing away from the other's run.
        /// </summary>
        public static void CornerBoards(Transform parent, IList<OuterWall> walls, float top, TutorialHouseArt art)
        {
            const float skin = L.ExteriorWall * .5f;
            foreach (OuterWall a in walls)
            {
                if (!a.Along) continue;
                foreach (OuterWall b in walls)
                {
                    if (b.Along) continue;
                    float ox, oz;
                    if (Mathf.Approximately(b.At, a.From)) ox = -1f;
                    else if (Mathf.Approximately(b.At, a.To)) ox = 1f;
                    else continue;
                    if (Mathf.Approximately(a.At, b.From)) oz = -1f;
                    else if (Mathf.Approximately(a.At, b.To)) oz = 1f;
                    else continue;
                    if (!Mathf.Approximately(b.Outward, ox) || !Mathf.Approximately(a.Outward, oz)) continue;
                    var at = new Vector3(b.At + ox * skin * .5f, 0f, a.At + oz * skin * .5f);
                    Box("CornerBoard", parent, at + Vector3.up * (.3f + top * .5f), new Vector3(skin + .05f, top, skin + .05f), art.Trim, false);
                    float footing = .3f - (L.Grade - .05f);
                    Box("CornerFooting", parent, at + Vector3.up * ((L.Grade - .05f + .3f) * .5f), new Vector3(skin * 1.15f, footing, skin * 1.15f), art.BasementWall, false);
                }
            }
        }

        private static void Inner(Transform parent, Transform openings, TutorialHouseArt art, string name, bool along, float at, float from, float to, params Hole[] holes)
        {
            Transform group = Group(name, parent);
            Run(group, "Partition", along, at, from, to, L.Floor, L.Ceiling + .05f, L.InteriorWall, art.Plaster, holes);
            foreach (Hole h in holes) Casing(openings, art, name, along, at, L.InteriorWall, h);
            // Baseboards both sides.
            foreach (float side in new[] { -1f, 1f })
                Run(group, "Baseboard", along, at + side * (L.InteriorWall * .5f + .008f), from, to, L.Floor, .1f, .016f, art.Trim, holes, collider: false);
        }

        private static Hole[] FoundationHoles(Hole[] holes)
        {
            var doors = new List<Hole>();
            foreach (Hole h in holes) if (!h.Window) doors.Add(new Hole(h.From, h.To, L.Grade - .1f, .35f, false));
            return doors.ToArray();
        }

        /// <summary>One straight skin with holes cut out; <paramref name="at"/> is its centreline across the run.</summary>
        public static void Run(Transform parent, string name, bool along, float at, float from, float to, float y0, float y1, float thickness,
            Material material, Hole[] holes, bool collider = true)
        {
            var cuts = new List<Hole>();
            if (holes != null) foreach (Hole h in holes) if (h.To > from && h.From < to && h.Bottom < y1 && h.Top > y0) cuts.Add(h);
            cuts.Sort((a, b) => a.From.CompareTo(b.From));
            float cursor = from;
            int n = 0;
            foreach (Hole h in cuts)
            {
                if (h.From > cursor) Piece(parent, $"{name}_{n++}", along, at, cursor, h.From, y0, y1, thickness, material, collider);
                if (h.Bottom > y0) Piece(parent, $"{name}_{n++}", along, at, h.From, h.To, y0, Mathf.Min(h.Bottom, y1), thickness, material, collider);
                if (h.Top < y1) Piece(parent, $"{name}_{n++}", along, at, h.From, h.To, Mathf.Max(h.Top, y0), y1, thickness, material, collider);
                cursor = Mathf.Max(cursor, h.To);
            }
            if (cursor < to) Piece(parent, $"{name}_{n}", along, at, cursor, to, y0, y1, thickness, material, collider);
        }

        private static void Piece(Transform parent, string name, bool along, float at, float s0, float s1, float y0, float y1, float thickness, Material material, bool collider)
        {
            if (s1 - s0 < .005f || y1 - y0 < .005f) return;
            float mid = (s0 + s1) * .5f, len = s1 - s0, h = y1 - y0;
            Vector3 centre = along ? new Vector3(mid, (y0 + y1) * .5f, at) : new Vector3(at, (y0 + y1) * .5f, mid);
            Vector3 size = along ? new Vector3(len, h, thickness) : new Vector3(thickness, h, len);
            Box(name, parent, centre, size, material, collider);
        }

        private static Vector3 Point(bool along, float at, float s, float y, float across) =>
            along ? new Vector3(s, y, at + across) : new Vector3(at + across, y, s);

        private static Vector3 Size(bool along, float len, float h, float depth) =>
            along ? new Vector3(len, h, depth) : new Vector3(depth, h, len);

        // Exterior opening: casing both faces, and for a window the sash, frosted glass (solid), sill and blinds.
        // Not full: the glass alone.
        private static void Opening(Transform parent, TutorialHouseArt art, string wall, bool along, float at, float outward, Hole h, bool full)
        {
            if (full) Casing(parent, art, wall, along, at, L.ExteriorWall, h);
            if (!h.Window) return;
            Transform win = Group($"{wall}_Window_{h.From:0.0}", parent);
            float mid = (h.From + h.To) * .5f, len = h.To - h.From, height = h.Top - h.Bottom;
            GameObject glass = Box("FrostedGlass", win, Point(along, at, mid, h.Bottom + height * .5f, 0f), Size(along, len, height, .03f), art.Glass);
            glass.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Glass);
            if (!full) return;
            Box("MeetingRail", win, Point(along, at, mid, h.Bottom + height * .5f, outward * .03f), Size(along, len, .06f, .05f), art.Trim, false);
            Box("SashMullion", win, Point(along, at, mid, h.Bottom + height * .5f, outward * .03f), Size(along, .05f, height, .05f), art.Trim, false);
            Box("SnowOnSill", win, Point(along, at, mid, h.Bottom + .03f, outward * (L.ExteriorWall * .5f + .07f)), Size(along, len, .05f, .12f), art.SnowDrift, false);
            Box("InteriorSill", win, Point(along, at, mid, h.Bottom - .02f, -outward * (L.ExteriorWall * .5f + .04f)), Size(along, len + .1f, .04f, .1f), art.Trim, false);
            // Blinds half down, a few slats bent: closed up for the winter and left.
            float blindTop = h.Top - .04f, blindBottom = h.Bottom + height * .45f;
            Box("BlindHeadrail", win, Point(along, at, mid, blindTop, -outward * (L.ExteriorWall * .5f + .05f)), Size(along, len, .04f, .05f), art.Blind, false);
            int slat = 0;
            for (float y = blindTop - .05f; y > blindBottom; y -= .06f, slat++)
            {
                GameObject s = Box("BlindSlat", win, Point(along, at, mid, y, -outward * (L.ExteriorWall * .5f + .05f)), Size(along, len - .02f, .008f, .045f), art.Blind, false);
                if (slat % 7 == 3) s.transform.localRotation = Quaternion.Euler(along ? 18f : 0f, 0f, along ? 0f : 18f);
            }
        }

        private static void Casing(Transform parent, TutorialHouseArt art, string wall, bool along, float at, float thickness, Hole h)
        {
            Transform casing = Group($"{wall}_Casing_{h.From:0.0}", parent);
            float mid = (h.From + h.To) * .5f, len = h.To - h.From;
            foreach (float side in new[] { -1f, 1f })
            {
                float face = side * (thickness * .5f + .012f);
                Box("Head", casing, Point(along, at, mid, h.Top + .045f, face), Size(along, len + .18f, .09f, .025f), art.Trim, false);
                Box("JambL", casing, Point(along, at, h.From - .045f, (h.Bottom + h.Top) * .5f, face), Size(along, .09f, h.Top - h.Bottom, .025f), art.Trim, false);
                Box("JambR", casing, Point(along, at, h.To + .045f, (h.Bottom + h.Top) * .5f, face), Size(along, .09f, h.Top - h.Bottom, .025f), art.Trim, false);
            }
            if (!h.Window) Box("Threshold", casing, Point(along, at, mid, h.Bottom + .006f, 0f), Size(along, len, .012f, thickness), art.OakDark, false);
        }

        // A front-door leaf swung back flat against the inside of the east wall beside its jamb, running away from the
        // opening (direction = +1 north, -1 south). Visual only.
        private static void FlatLeaf(Transform parent, TutorialHouseArt art, string name, float wallX, float jambZ, float direction, float width, bool wreath)
        {
            Transform leaf = Group(name, parent);
            float x = wallX - L.ExteriorWall * .5f - .04f;
            leaf.position = new Vector3(x, 0f, jambZ + direction * (width * .5f + .03f));
            Box("Slab", leaf, Vector3.up * (L.DoorHeight * .5f), new Vector3(.045f, L.DoorHeight - .02f, width), art.SidingDark, false);
            Primitive(PrimitiveType.Sphere, "Knob", leaf, new Vector3(-.04f, 1.05f, direction * (width * .5f - .08f)), Vector3.one * .06f, art.Brass, false);
            // The wreath is still on the front door, a December the family never finished.
            if (wreath) Box("Wreath", leaf, new Vector3(-.05f, 1.65f, 0f), new Vector3(.05f, .42f, .42f), art.TreeGreen, false);
        }
    }
}
