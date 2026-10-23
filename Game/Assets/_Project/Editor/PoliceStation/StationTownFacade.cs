using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;
using M = Abandoned.EditorTools.StationMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The pieces the neighbourhood's buildings are dressed with, drawn on a <b>face</b>: a frame on one side of a building
    /// whose +z points out from the wall, x runs along it and y is height above the ground (<see cref="Face"/>). Windows
    /// with frames, sills, lintels and the streak of grime under them; doors with stoops; cornices and parapets; fire
    /// escapes; storefronts with awnings and sign bands; gutters. Visual only: the building's mass carries the collider.
    /// </summary>
    public static class StationTownFacade
    {
        public enum Side { Front, Back, Left, Right }

        /// <summary>A face of a building <paramref name="width"/> by <paramref name="depth"/> whose front is at z 0, back at -depth.</summary>
        public static Transform Face(Transform building, Side side, float width, float depth)
        {
            Transform f = Group("Face_" + side, building);
            switch (side)
            {
                case Side.Front: f.localPosition = Vector3.zero; f.localRotation = Quaternion.identity; break;
                case Side.Back: f.localPosition = new Vector3(0f, 0f, -depth); f.localRotation = Quaternion.Euler(0f, 180f, 0f); break;
                case Side.Left: f.localPosition = new Vector3(-width * .5f, 0f, -depth * .5f); f.localRotation = Quaternion.Euler(0f, -90f, 0f); break;
                default: f.localPosition = new Vector3(width * .5f, 0f, -depth * .5f); f.localRotation = Quaternion.Euler(0f, 90f, 0f); break;
            }
            return f;
        }

        /// <summary>The look of one window.</summary>
        public enum Pane { Glass, Dark, Boarded, Blind, Broken }

        public static Pane PaneFor(int seed) => (Mathf.Abs(seed * 7919) % 20) switch
        {
            < 9 => Pane.Glass, < 12 => Pane.Blind, < 15 => Pane.Dark, < 18 => Pane.Boarded, _ => Pane.Broken,
        };

        /// <summary>A window with its bottom at <paramref name="sill"/>, centred at <paramref name="x"/>.</summary>
        public static void Window(Transform face, StationArt a, StationTownArt t, float x, float sill, float w, float h, Pane pane, Material frame,
            Material head, bool mullion = true, bool streak = true)
        {
            Transform win = Group("Window", face);
            win.localPosition = new Vector3(x, sill, 0f);
            Box("Frame", win, new Vector3(0f, h * .5f, .03f), new Vector3(w + .14f, h + .14f, .06f), frame, false);
            Material glass = pane switch
            {
                Pane.Dark or Pane.Broken => t.Void,
                Pane.Boarded => a.Board,
                _ => t.Window,
            };
            Box("Pane", win, new Vector3(0f, h * .5f, .065f), new Vector3(w, h, .02f), glass, false);
            if (pane == Pane.Blind)
                Box("Blind", win, new Vector3(0f, h * .78f, .076f), new Vector3(w - .04f, h * .42f, .004f), t.Frame, false);
            if (pane == Pane.Boarded)
                for (int i = 0; i < 2; i++)
                {
                    GameObject plank = Box("Plank", win, new Vector3(0f, h * (.3f + i * .4f), .09f), new Vector3(w + .1f, .14f, .02f), a.DarkWood, false);
                    plank.transform.localRotation = Quaternion.Euler(0f, 0f, i == 0 ? 8f : -6f);
                }
            if (pane == Pane.Broken)
                foreach (Vector3 shard in new[] { new Vector3(-w * .3f, h * .8f, 0f), new Vector3(w * .28f, h * .15f, 0f) })
                {
                    GameObject g = Box("Shard", win, new Vector3(shard.x, shard.y, .078f), new Vector3(w * .35f, h * .25f, .005f), t.Window, false);
                    g.transform.localRotation = Quaternion.Euler(0f, 0f, 25f);
                }
            if (mullion && pane != Pane.Boarded)
            {
                Box("Mullion", win, new Vector3(0f, h * .5f, .08f), new Vector3(.05f, h, .02f), frame, false);
                Box("Transom", win, new Vector3(0f, h * .62f, .08f), new Vector3(w, .05f, .02f), frame, false);
            }
            Box("Sill", win, new Vector3(0f, -.05f, .1f), new Vector3(w + .3f, .09f, .2f), a.Trim, false);
            Box("Lintel", win, new Vector3(0f, h + .12f, .05f), new Vector3(w + .32f, .2f, .1f), head, false);
            if (streak) Box("Streak", win, new Vector3(0f, -.7f, .012f), new Vector3(w * .55f, 1.2f, .004f), a.Grime, false);
        }

        /// <summary>A grid of windows: <paramref name="columns"/> across <paramref name="width"/>, one row per sill height.</summary>
        public static void WindowGrid(Transform face, StationArt a, StationTownArt t, float width, float[] sills, int columns, float w, float h,
            Material frame, Material head, int seed, float skipX = float.NaN, float skipHalf = 0f)
        {
            float step = width / columns;
            for (int row = 0; row < sills.Length; row++)
            for (int c = 0; c < columns; c++)
            {
                float x = -width * .5f + step * (c + .5f);
                if (!float.IsNaN(skipX) && Mathf.Abs(x - skipX) < skipHalf && row == 0) continue;
                int roll = seed * 31 + row * 7 + c * 13;
                Window(face, a, t, x, sills[row], w, h, PaneFor(roll), frame, head);
                if (Mathf.Abs(roll) % 9 == 4 && row > 0) WindowAc(face, a, x, sills[row]);
            }
        }

        /// <summary>A window air conditioner hanging off the sill.</summary>
        public static void WindowAc(Transform face, StationArt a, float x, float sill)
        {
            Transform ac = Group("WindowAc", face);
            ac.localPosition = new Vector3(x, sill + .02f, .25f);
            Box("Case", ac, new Vector3(0f, .2f, 0f), new Vector3(.62f, .4f, .5f), a.White, false);
            Box("Grille", ac, new Vector3(0f, .2f, .252f), new Vector3(.5f, .3f, .005f), a.DarkSteel, false);
            Box("Bracket", ac, new Vector3(0f, -.08f, -.1f), new Vector3(.5f, .04f, .3f), a.DarkSteel, false);
            Box("Drip", ac, new Vector3(.15f, -.8f, -.23f), new Vector3(.2f, 1.4f, .004f), a.Grime, false);
        }

        /// <summary>A door with its frame, a fanlight over it and steps up to it if <paramref name="rise"/> &gt; 0.</summary>
        public static void Door(Transform face, StationArt a, StationTownArt t, float x, float w, float h, Material leaf, float rise = 0f, bool open = false)
        {
            Transform d = Group("Door", face);
            d.localPosition = new Vector3(x, rise, 0f);
            Box("Surround", d, new Vector3(0f, (h + .5f) * .5f, .04f), new Vector3(w + .3f, h + .5f, .08f), a.Trim, false);
            Box("Leaf", d, new Vector3(0f, h * .5f, .07f), new Vector3(w, h, .03f), open ? t.Void : leaf, false);
            Box("Fanlight", d, new Vector3(0f, h + .22f, .075f), new Vector3(w, .3f, .02f), t.Window, false);
            if (!open)
            {
                Box("Knob", d, new Vector3(w * .38f, h * .47f, .1f), new Vector3(.05f, .05f, .04f), a.Brass, false);
                Box("Panel", d, new Vector3(0f, h * .72f, .088f), new Vector3(w * .7f, h * .3f, .01f), leaf, false);
                Box("Panel", d, new Vector3(0f, h * .28f, .088f), new Vector3(w * .7f, h * .34f, .01f), leaf, false);
            }
            else
            {
                Transform hinge = Group("Swing", d);
                hinge.localPosition = new Vector3(-w * .5f, 0f, .07f);
                hinge.localRotation = Quaternion.Euler(0f, -70f, 0f);
                Box("Leaf", hinge, new Vector3(w * .5f, h * .5f, 0f), new Vector3(w, h, .04f), leaf, false);
            }
            if (rise <= .05f) return;
            int steps = Mathf.Max(1, Mathf.RoundToInt(rise / .18f));
            for (int i = 0; i < steps; i++)
            {
                float y = rise * (i + 1) / steps;
                GameObject step = Box("Step", face, new Vector3(x, y * .5f, .3f * (steps - i) + .1f), new Vector3(w + .8f, y, .3f), a.Trim);
                step.name = "Step";
            }
            Box("Landing", face, new Vector3(x, rise * .5f, .25f), new Vector3(w + .8f, rise, .5f), a.Trim);
            foreach (float s in new[] { -1f, 1f })
            {
                Vector3 top = new(x + s * (w * .5f + .35f), rise + .9f, .3f), bottom = new(x + s * (w * .5f + .35f), .9f, .3f * steps + .4f);
                M.Strut(face, "Railing", top, bottom, .04f, t.Lattice);
                Box("Newel", face, new Vector3(bottom.x, .45f, bottom.z), new Vector3(.05f, .9f, .05f), t.Lattice, false);
            }
        }

        /// <summary>A stepped cornice along a face at <paramref name="y"/>, with dentils under its crown.</summary>
        public static void Cornice(Transform face, Material m, Material dentil, float width, float y, bool dentils = true)
        {
            Box("CorniceBand", face, new Vector3(0f, y, .06f), new Vector3(width + .1f, .3f, .12f), m, false);
            Box("CorniceCrown", face, new Vector3(0f, y + .28f, .2f), new Vector3(width + .4f, .18f, .4f), m, false);
            Box("CorniceSoffit", face, new Vector3(0f, y + .17f, .14f), new Vector3(width + .2f, .06f, .28f), m, false);
            if (!dentils) return;
            for (float x = -width * .5f + .15f; x < width * .5f - .1f; x += .32f)
                Box("Dentil", face, new Vector3(x, y + .08f, .16f), new Vector3(.12f, .12f, .1f), dentil, false);
        }

        /// <summary>A flat roof: the membrane, a parapet round it with coping, and a scupper and downpipe at one corner.</summary>
        public static void FlatRoof(Transform b, StationArt a, StationTownArt t, Material wall, float width, float depth, float height, float parapet = .9f)
        {
            Box("Membrane", b, new Vector3(0f, height + .01f, -depth * .5f), new Vector3(width - .1f, .02f, depth - .1f), t.TarRoof, false);
            const float thick = .25f;
            float y = height + parapet * .5f;
            Box("ParapetF", b, new Vector3(0f, y, -thick * .5f), new Vector3(width, parapet, thick), wall);
            Box("ParapetB", b, new Vector3(0f, y, -depth + thick * .5f), new Vector3(width, parapet, thick), wall);
            Box("ParapetL", b, new Vector3(-width * .5f + thick * .5f, y, -depth * .5f), new Vector3(thick, parapet, depth - thick * 2f), wall);
            Box("ParapetR", b, new Vector3(width * .5f - thick * .5f, y, -depth * .5f), new Vector3(thick, parapet, depth - thick * 2f), wall);
            float top = height + parapet + .05f;
            Box("CopingF", b, new Vector3(0f, top, -thick * .5f), new Vector3(width + .1f, .1f, thick + .1f), a.Trim, false);
            Box("CopingB", b, new Vector3(0f, top, -depth + thick * .5f), new Vector3(width + .1f, .1f, thick + .1f), a.Trim, false);
            Box("CopingL", b, new Vector3(-width * .5f + thick * .5f, top, -depth * .5f), new Vector3(thick + .1f, .1f, depth), a.Trim, false);
            Box("CopingR", b, new Vector3(width * .5f - thick * .5f, top, -depth * .5f), new Vector3(thick + .1f, .1f, depth), a.Trim, false);
            Box("Downpipe", b, new Vector3(width * .5f - .25f, height * .5f, -depth - .08f), new Vector3(.1f, height, .1f), t.Gutter, false);
            Box("Scupper", b, new Vector3(width * .5f - .25f, height - .2f, -depth - .12f), new Vector3(.3f, .3f, .2f), t.Gutter, false);
        }

        /// <summary>A fire escape on a face: a grated landing at each upper storey, railings, and stairs between them.</summary>
        public static void FireEscape(Transform face, StationTownArt t, float x, float width, float[] levels)
        {
            Transform fe = Group("FireEscape", face);
            fe.localPosition = new Vector3(x, 0f, 0f);
            for (int i = 0; i < levels.Length; i++)
            {
                float y = levels[i];
                Box("Landing", fe, new Vector3(0f, y, .62f), new Vector3(width, .05f, 1.1f), t.Lattice, false);
                Box("TopRail", fe, new Vector3(0f, y + 1f, 1.15f), new Vector3(width, .04f, .04f), t.Lattice, false);
                Box("MidRail", fe, new Vector3(0f, y + .5f, 1.15f), new Vector3(width, .03f, .03f), t.Lattice, false);
                foreach (float s in new[] { -1f, 1f })
                {
                    Box("EndRail", fe, new Vector3(s * width * .5f, y + 1f, .62f), new Vector3(.04f, .04f, 1.1f), t.Lattice, false);
                    Box("Post", fe, new Vector3(s * width * .5f, y + .5f, 1.15f), new Vector3(.04f, 1f, .04f), t.Lattice, false);
                    Box("Bracket", fe, new Vector3(s * (width * .5f - .2f), y - .35f, .4f), new Vector3(.05f, .05f, .9f), t.Lattice, false)
                        .transform.localRotation = Quaternion.Euler(-38f, 0f, 0f);
                }
                for (float bx = -width * .5f + .15f; bx < width * .5f; bx += .3f)
                    Box("Baluster", fe, new Vector3(bx, y + .5f, 1.15f), new Vector3(.015f, 1f, .015f), t.Lattice, false);
                if (i == 0)
                {
                    // The drop ladder, still hooked up.
                    foreach (float s in new[] { -.22f, .22f }) Box("DropRail", fe, new Vector3(width * .5f - .5f + s, y - 1.2f, 1.05f), new Vector3(.04f, 2.4f, .04f), t.Lattice, false);
                    for (int r = 0; r < 7; r++) Box("DropRung", fe, new Vector3(width * .5f - .5f, y - 2.3f + r * .34f, 1.05f), new Vector3(.44f, .03f, .03f), t.Lattice, false);
                    continue;
                }
                // Stairs up from the landing below, along the face.
                float below = levels[i - 1], dir = i % 2 == 0 ? 1f : -1f;
                Vector3 foot = new(-dir * width * .35f, below + .05f, .85f), head = new(dir * width * .3f, y, .85f);
                foreach (float dz in new[] { -.3f, .3f }) M.Strut(fe, "Stringer", foot + Vector3.forward * dz, head + Vector3.forward * dz, .05f, t.Lattice, .14f);
                int treads = Mathf.RoundToInt((y - below) / .2f);
                for (int k = 1; k < treads; k++)
                    Box("Tread", fe, Vector3.Lerp(foot, head, k / (float)treads), new Vector3(.22f, .03f, .62f), t.Lattice, false);
            }
        }

        /// <summary>A shopfront across <paramref name="width"/>: stall riser, glazing with mullions, a recessed door,
        /// posters, a sign band with a maker's mark, an awning, and maybe its roller shutter part-way down.</summary>
        public static void Storefront(Transform face, StationArt a, StationTownArt t, float width, float height, int seed, bool awning = true)
        {
            Transform s = Group("Storefront", face);
            float glassTop = height - .9f, doorX = (seed % 3 - 1) * width * .3f;
            Material frame = t.Paint(seed + 1);
            Box("Fascia", s, new Vector3(0f, height - .45f, .07f), new Vector3(width, .9f, .14f), frame, false);
            Material sign = t.Sign(seed);
            Box("SignBoard", s, new Vector3(0f, height - .45f, .15f), new Vector3(width * .7f, .6f, .03f), sign, false);
            // A maker's mark on the sign: a disc and two bars (no lettering).
            GameObject disc = Primitive(PrimitiveType.Cylinder, "Mark", s, new Vector3(-width * .25f, height - .45f, .17f), new Vector3(.42f, .01f, .42f), t.SignLight, false);
            disc.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            Box("Bar", s, new Vector3(width * .05f, height - .35f, .17f), new Vector3(width * .38f, .1f, .01f), t.SignLight, false);
            Box("Bar", s, new Vector3(width * .02f, height - .55f, .17f), new Vector3(width * .3f, .06f, .01f), t.SignLight, false);
            Box("Riser", s, new Vector3(0f, .25f, .06f), new Vector3(width, .5f, .12f), a.Trim, false);
            Box("Glazing", s, new Vector3(0f, (.5f + glassTop) * .5f, .05f), new Vector3(width - .2f, glassTop - .5f, .02f), t.Window, false);
            for (float x = -width * .5f + .1f; x <= width * .5f; x += 1.6f)
                Box("Mullion", s, new Vector3(x, (.5f + glassTop) * .5f, .07f), new Vector3(.08f, glassTop - .5f, .04f), frame, false);
            Box("Head", s, new Vector3(0f, glassTop, .07f), new Vector3(width, .1f, .06f), frame, false);
            Box("DoorRecess", s, new Vector3(doorX, 1.2f, .075f), new Vector3(1.2f, 2.4f, .02f), t.Void, false);
            Box("DoorFrame", s, new Vector3(doorX, 2.42f, .085f), new Vector3(1.3f, .08f, .04f), frame, false);
            Box("DoorPush", s, new Vector3(doorX + .3f, 1.1f, .095f), new Vector3(.04f, .5f, .03f), a.Chrome, false);
            for (int i = 0; i < 3; i++)
            {
                int roll = seed * 17 + i * 5;
                if (roll % 4 == 0) continue;
                float px = -width * .4f + i * width * .3f;
                if (Mathf.Abs(px - doorX) < 1f) continue;
                GameObject poster = Box("Poster", s, new Vector3(px, 1.5f + (roll % 3) * .2f, .064f), new Vector3(.5f, .7f, .003f), roll % 2 == 0 ? a.Paper : t.Sign(roll), false);
                poster.transform.localRotation = Quaternion.Euler(0f, 0f, (roll % 5 - 2) * 2f);
            }
            // The awning: a sloped canvas with its valance, on two arms (none under a strip mall's canopy).
            Material canvas = t.Awning(seed + 2);
            if (awning) Awning(s, t, canvas, width, glassTop, seed);
            if (seed % 3 == 2)
            {
                // The roller shutter, pulled most of the way down and left.
                float down = glassTop * (.4f + (seed % 5) * .1f);
                Box("ShutterBox", s, new Vector3(0f, glassTop + .05f, .2f), new Vector3(width - .1f, .3f, .3f), a.DarkSteel, false);
                Box("Shutter", s, new Vector3(0f, glassTop - down * .5f, .11f), new Vector3(width - .2f, down, .02f), a.Steel, false);
                for (float y = glassTop - down + .1f; y < glassTop; y += .12f)
                    Box("Slat", s, new Vector3(0f, y, .123f), new Vector3(width - .22f, .02f, .006f), a.DarkSteel, false);
            }
        }

        private static void Awning(Transform s, StationTownArt t, Material canvas, float width, float glassTop, int seed)
        {
            Transform aw = Group("Awning", s);
            aw.localPosition = new Vector3(0f, glassTop + .2f, .1f);
            aw.localRotation = Quaternion.Euler(22f, 0f, 0f);
            Box("Canvas", aw, new Vector3(0f, 0f, .7f), new Vector3(width - .3f, .03f, 1.4f), canvas, false);
            if (seed % 2 == 0)
                for (float x = -width * .5f + .45f; x < width * .5f - .3f; x += .6f)
                    Box("Stripe", aw, new Vector3(x, .016f, .7f), new Vector3(.3f, .005f, 1.4f), t.Awnings[4], false);
            Box("Valance", s, new Vector3(0f, glassTop + .2f - .52f - .12f, 1.4f), new Vector3(width - .3f, .25f, .02f), canvas, false);
            foreach (float sx in new[] { -1f, 1f })
                M.Strut(s, "AwningArm", new Vector3(sx * (width * .5f - .25f), glassTop - .7f, .05f), new Vector3(sx * (width * .5f - .25f), glassTop - .34f, 1.36f), .03f, t.Lattice);
        }

        /// <summary>A gutter run along the eaves and a downpipe at each end of a face.</summary>
        public static void Gutters(Transform face, StationTownArt t, float width, float eaves)
        {
            Box("Gutter", face, new Vector3(0f, eaves - .05f, .12f), new Vector3(width + .2f, .12f, .14f), t.Gutter, false);
            foreach (float s in new[] { -1f, 1f })
                Box("Downpipe", face, new Vector3(s * (width * .5f - .15f), eaves * .5f, .1f), new Vector3(.09f, eaves, .09f), t.Gutter, false);
        }

        /// <summary>A wall lamp in its cage over a door (unlit: the power is off).</summary>
        public static void WallLamp(Transform face, StationArt a, float x, float y)
        {
            Transform l = Group("WallLamp", face);
            l.localPosition = new Vector3(x, y, 0f);
            Box("Plate", l, new Vector3(0f, 0f, .02f), new Vector3(.14f, .22f, .04f), a.DarkSteel, false);
            Box("Arm", l, new Vector3(0f, .06f, .14f), new Vector3(.03f, .03f, .24f), a.DarkSteel, false);
            Box("Shade", l, new Vector3(0f, .02f, .27f), new Vector3(.26f, .08f, .22f), a.DarkSteel, false);
            Box("Bulb", l, new Vector3(0f, -.03f, .27f), new Vector3(.12f, .06f, .1f), a.LampOff, false);
        }
    }
}
