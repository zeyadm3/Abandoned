using UnityEngine;
using B = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Dressing that keeps rooms from repeating: office machines, coat racks, bins, bookcases, workbenches, radiators,
    /// extinguishers, case boards, chairs knocked over, the clutter on a desk or a table (seeded, so every desk differs),
    /// litter on the floor, and the warnings under a failing floor (a sagging ceiling tile, dangling cable, plaster dust).
    /// Floor litter and ceiling damage are hung (no collider, under 0.3 m or overhead): nothing to trip a route.
    /// </summary>
    public static partial class StationFurniture
    {
        public static Transform Copier(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Copier", at, yaw, new Vector3(1f, 1.15f, .7f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .55f, 0f), new Vector3(1f, 1.1f, .7f), a.White);
            B.B(t, "Glass", new Vector3(0f, 1.105f, 0f), new Vector3(.8f, .01f, .5f), a.Rubber);
            B.B(t, "Tray", new Vector3(.62f, .8f, 0f), new Vector3(.3f, .04f, .45f), a.White);
            for (int i = 0; i < 3; i++) B.B(t, "Drawer", new Vector3(0f, .15f + i * .2f, .355f), new Vector3(.9f, .16f, .01f), a.Steel);
            B.B(t, "Panel", new Vector3(-.3f, 1.12f, .25f), new Vector3(.3f, .03f, .15f), a.DarkSteel);
            return t;
        }

        public static Transform CoatRack(StationProps p, string room, Vector3 at, Material coat = null)
        {
            Transform t = p.Place(room, "CoatRack", at, 0f, new Vector3(.5f, 1.85f, .5f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.C(t, "Pole", new Vector3(0f, .92f, 0f), new Vector3(.05f, .92f, .05f), a.DarkWood);
            B.C(t, "Foot", new Vector3(0f, .03f, 0f), new Vector3(.45f, .03f, .45f), a.DarkWood);
            if (coat != null) B.B(t, "Coat", new Vector3(.12f, 1.35f, 0f), new Vector3(.12f, .8f, .45f), coat);
            B.B(t, "Hat", new Vector3(-.1f, 1.82f, 0f), new Vector3(.25f, .1f, .28f), a.Rubber);
            return t;
        }

        public static Transform TrashCan(StationProps p, string room, Vector3 at, bool tipped = false)
        {
            Transform t = p.Place(room, "TrashCan", at, 0f, new Vector3(.42f, .5f, .42f));
            if (t == null) return null;
            GameObject can = B.C(t, "Can", new Vector3(0f, .25f, 0f), new Vector3(.4f, .25f, .4f), p.Art.DarkSteel);
            if (tipped)
            {
                can.transform.localPosition = new Vector3(0f, .2f, 0f);
                can.transform.localRotation = Quaternion.Euler(90f, 30f, 0f);
                B.B(t, "Spill", new Vector3(.2f, .02f, .35f), new Vector3(.5f, .03f, .4f), p.Art.Paper);
            }
            return t;
        }

        /// <summary>A wooden bookcase, its books in mixed colours and heights (seeded).</summary>
        public static Transform Bookcase(StationProps p, string room, Vector3 at, float yaw, float width = 1.2f, int seed = 0)
        {
            Transform t = p.Place(room, "Bookcase", at, yaw, new Vector3(width, 2f, .38f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Back", new Vector3(0f, 1f, -.17f), new Vector3(width, 2f, .03f), a.DarkWood);
            foreach (float x in new[] { -width * .5f + .02f, width * .5f - .02f }) B.B(t, "Side", new Vector3(x, 1f, 0f), new Vector3(.03f, 2f, .36f), a.DarkWood);
            Material[] spines = { a.Accent, a.Red, a.Leather, a.Paper, a.AccentDark, a.Board };
            for (int s = 0; s < 5; s++)
            {
                float y = .05f + s * .45f;
                B.B(t, "Shelf", new Vector3(0f, y, 0f), new Vector3(width - .06f, .03f, .34f), a.DarkWood);
                if (s == 4) continue;
                float x = -width * .5f + .08f;
                int n = 0;
                while (x < width * .5f - .12f)
                {
                    int r = Mathf.Abs(seed * 31 + s * 17 + n * 7) % 11;
                    float w = .03f + r * .006f, h = .22f + (r % 4) * .04f;
                    if (r == 3) { x += .14f; n++; continue; }   // a gap where a book was taken
                    B.B(t, "Book", new Vector3(x + w * .5f, y + .015f + h * .5f, .02f), new Vector3(w, h, .24f), spines[r % spines.Length]);
                    x += w + .004f;
                    n++;
                }
            }
            return t;
        }

        /// <summary>A workbench with a pegboard of tools behind it and a vice.</summary>
        public static Transform Workbench(StationProps p, string room, Vector3 at, float yaw, float width = 2.2f)
        {
            Transform t = p.Place(room, "Workbench", at, yaw, new Vector3(width, .95f, .8f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Top", new Vector3(0f, .9f, 0f), new Vector3(width, .06f, .8f), a.Board);
            B.B(t, "Shelf", new Vector3(0f, .25f, 0f), new Vector3(width - .1f, .04f, .7f), a.Board);
            foreach (float x in new[] { -width * .5f + .06f, width * .5f - .06f })
            foreach (float z in new[] { -.34f, .34f })
                B.B(t, "Leg", new Vector3(x, .45f, z), new Vector3(.06f, .9f, .06f), a.DarkSteel);
            B.B(t, "Pegboard", new Vector3(0f, 1.55f, -.38f), new Vector3(width, 1f, .02f), a.Board);
            for (int i = 0; i < 6; i++)
                B.B(t, "Tool", new Vector3(-width * .4f + i * width * .16f, 1.5f + (i % 2) * .2f, -.36f), new Vector3(.05f, .3f - (i % 3) * .06f, .03f), i % 2 == 0 ? a.Red : a.Steel);
            B.B(t, "Vice", new Vector3(width * .5f - .2f, 1f, .3f), new Vector3(.2f, .14f, .16f), a.DarkSteel);
            return t;
        }

        /// <summary>A cast-iron radiator under a window: low, solid.</summary>
        public static Transform Radiator(StationProps p, string room, Vector3 at, float yaw, float width = 1.2f)
        {
            Transform t = p.Place(room, "Radiator", at, yaw, new Vector3(width, .7f, .22f));
            if (t == null) return null;
            for (float x = -width * .5f + .05f; x < width * .5f; x += .1f)
                B.B(t, "Fin", new Vector3(x, .4f, 0f), new Vector3(.06f, .6f, .2f), p.Art.Steel);
            B.B(t, "Pipe", new Vector3(0f, .12f, 0f), new Vector3(width, .05f, .05f), p.Art.DarkSteel);
            return t;
        }

        public static Transform Extinguisher(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Hang(room, "Extinguisher", at, yaw);
            B.B(t, "Bracket", new Vector3(0f, 0f, .02f), new Vector3(.12f, .2f, .04f), p.Art.DarkSteel);
            B.C(t, "Bottle", new Vector3(0f, 0f, .12f), new Vector3(.16f, .28f, .16f), p.Art.Red);
            B.B(t, "Head", new Vector3(0f, .31f, .12f), new Vector3(.06f, .08f, .1f), p.Art.DarkSteel);
            return t;
        }

        /// <summary>A detective's case board: photos pinned up and red string between them.</summary>
        public static Transform CaseBoard(StationProps p, string room, Vector3 at, float yaw, int seed = 0)
        {
            Transform t = WallBoard(p, room, at, yaw, 2.4f, 1.3f, p.Art.Board);
            for (int i = 0; i < 7; i++)
            {
                int r = Mathf.Abs(seed * 13 + i * 29) % 100;
                float x = -1f + (r % 10) * .2f, y = -.45f + (r / 10) * .1f;
                B.B(t, "Photo", new Vector3(x, y, .04f), new Vector3(.18f, .13f, .005f), i % 3 == 0 ? p.Art.Paper : p.Art.White);
                if (i == 0) continue;
                GameObject thread = B.B(t, "String", new Vector3(x * .5f, y * .5f, .045f), new Vector3(Mathf.Abs(x) + .2f, .008f, .003f), p.Art.Red);
                thread.transform.localRotation = Quaternion.Euler(0f, 0f, (r % 50) - 25f);
            }
            return t;
        }

        /// <summary>A chair knocked over onto its back.</summary>
        public static Transform FallenChair(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "FallenChair", at, yaw, new Vector3(.55f, .55f, 1f));
            if (t == null) return null;
            Transform chair = Group(t, "Chair");
            chair.localPosition = new Vector3(0f, .25f, -.2f);
            chair.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            B.B(chair, "Seat", new Vector3(0f, .46f, 0f), new Vector3(.48f, .07f, .46f), p.Art.Fabric);
            B.B(chair, "Back", new Vector3(0f, .72f, -.22f), new Vector3(.46f, .5f, .05f), p.Art.Fabric);
            foreach (float x in new[] { -.2f, .2f })
            foreach (float z in new[] { -.2f, .2f })
                B.B(chair, "Leg", new Vector3(x, .21f, z), new Vector3(.03f, .42f, .03f), p.Art.Steel);
            return t;
        }

        /// <summary>Papers, files and a box on the floor: walk-through litter, seeded.</summary>
        public static Transform FloorLitter(StationProps p, string room, Vector3 at, int seed)
        {
            Transform t = p.Hang(room, "Litter", at, Mathf.Abs(seed * 47) % 360);
            StationArt a = p.Art;
            for (int i = 0; i < 6; i++)
            {
                int r = Mathf.Abs(seed * 19 + i * 37) % 100;
                GameObject sheet = B.B(t, "Paper", new Vector3((r % 10) * .12f - .55f, .004f + i * .002f, (r / 10) * .1f - .45f), new Vector3(.21f, .003f, .3f), a.Paper);
                sheet.transform.localRotation = Quaternion.Euler(0f, r * 3.6f, 0f);
            }
            if (seed % 3 == 0) B.B(t, "File", new Vector3(.3f, .02f, .2f), new Vector3(.24f, .04f, .32f), a.Board);
            if (seed % 4 == 1) B.B(t, "Box", new Vector3(-.4f, .14f, .1f), new Vector3(.4f, .28f, .3f), a.Board);
            return t;
        }

        /// <summary>
        /// The warning under a floor that will go: a ceiling tile hanging by one corner, a dangling cable and plaster dust
        /// on the floor below. <paramref name="at"/> is on the floor; the ceiling is <paramref name="ceiling"/> above it.
        /// </summary>
        public static Transform CeilingDamage(StationProps p, string room, Vector3 at, float ceiling, int seed)
        {
            Transform t = p.Hang(room, "CeilingDamage", at, Mathf.Abs(seed * 61) % 360);
            StationArt a = p.Art;
            GameObject tile = B.B(t, "SaggingTile", new Vector3(0f, ceiling - .35f, 0f), new Vector3(.6f, .02f, .6f), a.Ceiling);
            tile.transform.localRotation = Quaternion.Euler(50f + seed % 20, 0f, 0f);
            B.B(t, "Cable", new Vector3(.4f, ceiling - .5f, .1f), new Vector3(.015f, 1f, .015f), a.Rubber);
            B.B(t, "Dust", new Vector3(0f, .006f, .1f), new Vector3(1.2f, .01f, .9f), a.Plaster);
            B.B(t, "Chunk", new Vector3(.25f, .04f, -.1f), new Vector3(.18f, .08f, .14f), a.Concrete);
            B.B(t, "Chunk", new Vector3(-.3f, .03f, .25f), new Vector3(.12f, .06f, .1f), a.Concrete);
            return t;
        }

        // Small things on a desk or table top, different for every seed.
        private static void ClutterOn(Transform t, float top, float width, float depth, int seed, StationArt a)
        {
            int r = Mathf.Abs(seed * 7919) % 97;
            if (r % 2 == 0) B.C(t, "Mug", new Vector3(width * .3f, top + .05f, depth * .15f), new Vector3(.08f, .05f, .08f), r % 4 == 0 ? a.White : a.Red);
            if (r % 3 == 0)
            {
                B.B(t, "Files", new Vector3(-width * .3f, top + .04f, depth * .1f), new Vector3(.3f, .08f, .24f), a.Board);
                B.B(t, "Files", new Vector3(-width * .3f + .02f, top + .1f, depth * .1f), new Vector3(.28f, .04f, .23f), a.Paper);
            }
            if (r % 5 == 1)
            {
                B.B(t, "LampBase", new Vector3(width * .38f, top + .02f, -depth * .3f), new Vector3(.14f, .03f, .14f), a.DarkSteel);
                B.B(t, "LampArm", new Vector3(width * .38f, top + .2f, -depth * .25f), new Vector3(.02f, .35f, .02f), a.DarkSteel);
                B.B(t, "LampShade", new Vector3(width * .38f, top + .38f, -depth * .15f), new Vector3(.14f, .08f, .16f), a.AccentDark);
            }
            if (r % 7 == 3) B.B(t, "Phone", new Vector3(width * .1f, top + .04f, -depth * .3f), new Vector3(.2f, .07f, .18f), a.Rubber);
            if (r % 4 == 1) B.B(t, "Printer", new Vector3(-width * .25f, top + .1f, -depth * .2f), new Vector3(.42f, .2f, .35f), a.White);
            if (r % 6 == 2) B.B(t, "Ashtray", new Vector3(width * .05f, top + .015f, depth * .25f), new Vector3(.12f, .03f, .12f), a.Glass);
        }

        private static Transform Group(Transform parent, string name) => GreyboxFactory.Group(name, parent);
    }
}
