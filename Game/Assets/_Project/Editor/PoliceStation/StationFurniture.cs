using UnityEngine;
using B = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The furniture both stations share, as original low-poly models built from boxes and cylinders in each piece's own
    /// frame (+z is its front). Each method places one piece through <see cref="StationProps.Place"/>, so every piece is
    /// solid and kept off doorways and flights, and returns null when it was left out.
    /// </summary>
    public static class StationFurniture
    {
        public static Transform Desk(StationProps p, string room, Vector3 at, float yaw, bool computer = true, float width = 1.5f)
        {
            Transform t = p.Place(room, "Desk", at, yaw, new Vector3(width, .78f, .8f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Top", new Vector3(0f, .755f, 0f), new Vector3(width, .04f, .8f), a.Wood);
            B.B(t, "Pedestal", new Vector3(width * .5f - .25f, .37f, 0f), new Vector3(.44f, .74f, .74f), a.DarkSteel);
            B.B(t, "Leg", new Vector3(-width * .5f + .04f, .37f, 0f), new Vector3(.05f, .74f, .7f), a.DarkSteel);
            B.B(t, "Modesty", new Vector3(-.15f, .45f, -.36f), new Vector3(width - .6f, .5f, .02f), a.DarkSteel);
            for (int i = 0; i < 3; i++)
                B.B(t, "Drawer", new Vector3(width * .5f - .25f, .6f - i * .22f, .375f), new Vector3(.38f, .18f, .01f), a.Steel);
            if (computer)
            {
                B.B(t, "Monitor", new Vector3(-.1f, 1.02f, -.15f), new Vector3(.42f, .34f, .32f), a.White);
                B.B(t, "Screen", new Vector3(-.1f, 1.03f, .015f), new Vector3(.34f, .26f, .01f), a.Rubber);
                B.B(t, "Keyboard", new Vector3(-.1f, .785f, .18f), new Vector3(.44f, .025f, .15f), a.White);
            }
            B.B(t, "Papers", new Vector3(width * .25f, .78f, .05f), new Vector3(.3f, .02f, .22f), a.Paper);
            return t;
        }

        public static Transform Chair(StationProps p, string room, Vector3 at, float yaw, bool office = true)
        {
            Transform t = p.Place(room, office ? "OfficeChair" : "Chair", at, yaw, new Vector3(.55f, .95f, .55f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Seat", new Vector3(0f, .46f, 0f), new Vector3(.48f, .07f, .46f), a.Fabric);
            B.B(t, "Back", new Vector3(0f, .72f, -.22f), new Vector3(.46f, .5f, .05f), a.Fabric);
            if (office)
            {
                B.C(t, "Column", new Vector3(0f, .23f, 0f), new Vector3(.06f, .2f, .06f), a.Chrome);
                B.B(t, "BaseA", new Vector3(0f, .04f, 0f), new Vector3(.56f, .04f, .06f), a.Rubber);
                B.B(t, "BaseB", new Vector3(0f, .04f, 0f), new Vector3(.06f, .04f, .56f), a.Rubber);
            }
            else
                foreach (Vector2 l in new[] { new Vector2(-.2f, -.2f), new Vector2(.2f, -.2f), new Vector2(-.2f, .2f), new Vector2(.2f, .2f) })
                    B.B(t, "Leg", new Vector3(l.x, .21f, l.y), new Vector3(.03f, .42f, .03f), a.Steel);
            return t;
        }

        public static Transform FilingCabinet(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "FilingCabinet", at, yaw, new Vector3(.48f, 1.33f, .64f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .665f, 0f), new Vector3(.48f, 1.33f, .64f), a.DarkSteel);
            for (int i = 0; i < 4; i++)
            {
                B.B(t, "Drawer", new Vector3(0f, .2f + i * .31f, .325f), new Vector3(.42f, .27f, .01f), a.Steel);
                B.B(t, "Pull", new Vector3(0f, .27f + i * .31f, .335f), new Vector3(.12f, .025f, .02f), a.Chrome);
            }
            return t;
        }

        /// <summary>Steel shelving with archive boxes, 2 m tall.</summary>
        public static Transform Shelving(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f, int seed = 0, Material stock = null)
        {
            Transform t = p.Place(room, "Shelving", at, yaw, new Vector3(width, 2.1f, .5f));
            if (t == null) return null;
            StationArt a = p.Art;
            foreach (float x in new[] { -width * .5f + .03f, width * .5f - .03f })
            foreach (float z in new[] { -.22f, .22f })
                B.B(t, "Upright", new Vector3(x, 1.05f, z), new Vector3(.04f, 2.1f, .04f), a.Safety);
            for (int s = 0; s < 5; s++)
            {
                float y = .08f + s * .48f;
                B.B(t, "Shelf", new Vector3(0f, y, 0f), new Vector3(width - .04f, .03f, .46f), a.Steel);
                if (s == 4) continue;
                for (float x = -width * .5f + .25f; x < width * .5f - .2f; x += .42f)
                {
                    int roll = Mathf.Abs((int)(x * 97f) + s * 31 + seed * 17) % 7;
                    if (roll == 0) continue;
                    float h = roll % 2 == 0 ? .28f : .34f;
                    B.B(t, "Box", new Vector3(x, y + .015f + h * .5f, .02f), new Vector3(.36f, h, .4f), stock != null ? stock : roll < 4 ? a.Board : a.Paper);
                }
            }
            return t;
        }

        public static Transform Lockers(StationProps p, string room, Vector3 at, float yaw, int count)
        {
            float w = count * .4f;
            Transform t = p.Place(room, "LockerBank", at, yaw, new Vector3(w, 1.9f, .52f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .95f, 0f), new Vector3(w, 1.9f, .5f), a.DarkSteel);
            for (int i = 0; i < count; i++)
            {
                float x = -w * .5f + .2f + i * .4f;
                B.B(t, "Door", new Vector3(x, 1f, .255f), new Vector3(.36f, 1.7f, .01f), a.AccentDark);
                B.B(t, "Vent", new Vector3(x, 1.65f, .262f), new Vector3(.24f, .12f, .005f), a.Rubber);
                B.B(t, "Handle", new Vector3(x + .12f, 1.05f, .265f), new Vector3(.03f, .12f, .02f), a.Chrome);
            }
            B.B(t, "Bench", new Vector3(0f, .22f, .55f), new Vector3(w, .05f, .3f), a.Wood);
            return t;
        }

        public static Transform Bench(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f)
        {
            Transform t = p.Place(room, "Bench", at, yaw, new Vector3(width, .46f, .4f));
            if (t == null) return null;
            B.B(t, "Seat", new Vector3(0f, .44f, 0f), new Vector3(width, .05f, .38f), p.Art.Wood);
            foreach (float x in new[] { -width * .5f + .15f, width * .5f - .15f })
                B.B(t, "Leg", new Vector3(x, .21f, 0f), new Vector3(.05f, .42f, .32f), p.Art.DarkSteel);
            return t;
        }

        public static Transform Table(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f, float depth = .9f, Material top = null)
        {
            Transform t = p.Place(room, "Table", at, yaw, new Vector3(width, .76f, depth));
            if (t == null) return null;
            B.B(t, "Top", new Vector3(0f, .74f, 0f), new Vector3(width, .04f, depth), top != null ? top : p.Art.Wood);
            foreach (float x in new[] { -width * .5f + .06f, width * .5f - .06f })
            foreach (float z in new[] { -depth * .5f + .06f, depth * .5f - .06f })
                B.B(t, "Leg", new Vector3(x, .36f, z), new Vector3(.05f, .72f, .05f), p.Art.DarkSteel);
            return t;
        }

        public static Transform Bunk(StationProps p, string room, Vector3 at, float yaw, bool upper = true, Material blanket = null)
        {
            Transform t = p.Place(room, upper ? "BunkBed" : "Bed", at, yaw, new Vector3(.95f, upper ? 1.75f : .6f, 2.05f));
            if (t == null) return null;
            StationArt a = p.Art;
            foreach (float y in upper ? new[] { .4f, 1.4f } : new[] { .4f })
            {
                B.B(t, "Frame", new Vector3(0f, y, 0f), new Vector3(.95f, .06f, 2.05f), a.DarkSteel);
                B.B(t, "Mattress", new Vector3(0f, y + .1f, 0f), new Vector3(.88f, .14f, 1.95f), a.Mattress);
                B.B(t, "Blanket", new Vector3(0f, y + .18f, .3f), new Vector3(.9f, .04f, 1.2f), blanket != null ? blanket : a.Fabric);
                B.B(t, "Pillow", new Vector3(0f, y + .21f, -.78f), new Vector3(.6f, .1f, .3f), a.White);
            }
            float top = upper ? 1.75f : .6f;
            foreach (float x in new[] { -.45f, .45f })
            foreach (float z in new[] { -1f, 1f })
                B.B(t, "Post", new Vector3(x, top * .5f, z), new Vector3(.05f, top, .05f), a.DarkSteel);
            return t;
        }

        public static Transform Toilet(StationProps p, string room, Vector3 at, float yaw, Material m = null)
        {
            Transform t = p.Place(room, "Toilet", at, yaw, new Vector3(.45f, .8f, .7f));
            if (t == null) return null;
            Material mat = m != null ? m : p.Art.Porcelain;
            B.B(t, "Bowl", new Vector3(0f, .22f, .08f), new Vector3(.38f, .42f, .5f), mat);
            B.B(t, "Seat", new Vector3(0f, .44f, .1f), new Vector3(.4f, .03f, .46f), mat);
            B.B(t, "Cistern", new Vector3(0f, .6f, -.27f), new Vector3(.42f, .38f, .16f), mat);
            return t;
        }

        public static Transform Sink(StationProps p, string room, Vector3 at, float yaw, Material m = null)
        {
            Transform t = p.Place(room, "Sink", at, yaw, new Vector3(.55f, .9f, .45f));
            if (t == null) return null;
            Material mat = m != null ? m : p.Art.Porcelain;
            B.B(t, "Basin", new Vector3(0f, .8f, .02f), new Vector3(.55f, .16f, .42f), mat);
            B.B(t, "Pedestal", new Vector3(0f, .36f, -.05f), new Vector3(.16f, .72f, .16f), mat);
            B.B(t, "Tap", new Vector3(0f, .93f, -.14f), new Vector3(.04f, .12f, .04f), p.Art.Chrome);
            return t;
        }

        public static Transform Counter(StationProps p, string room, Vector3 at, float yaw, float length, float height = 1.05f, Material front = null)
        {
            Transform t = p.Place(room, "Counter", at, yaw, new Vector3(length, height, .7f));
            if (t == null) return null;
            B.B(t, "Body", new Vector3(0f, height * .5f - .02f, -.02f), new Vector3(length, height - .04f, .64f), front != null ? front : p.Art.Panel);
            B.B(t, "Top", new Vector3(0f, height - .02f, 0f), new Vector3(length + .04f, .04f, .7f), p.Art.Terrazzo);
            B.B(t, "Kick", new Vector3(0f, .05f, .3f), new Vector3(length, .1f, .02f), p.Art.Rubber);
            return t;
        }

        public static Transform Sofa(StationProps p, string room, Vector3 at, float yaw, float width = 2f, Material cover = null)
        {
            Transform t = p.Place(room, "Sofa", at, yaw, new Vector3(width, .85f, .9f));
            if (t == null) return null;
            Material m = cover != null ? cover : p.Art.Fabric;
            B.B(t, "Base", new Vector3(0f, .22f, .05f), new Vector3(width, .3f, .8f), m);
            B.B(t, "Cushions", new Vector3(0f, .42f, .08f), new Vector3(width - .3f, .12f, .7f), m);
            B.B(t, "Back", new Vector3(0f, .6f, -.34f), new Vector3(width, .55f, .2f), m);
            foreach (float x in new[] { -width * .5f + .1f, width * .5f - .1f })
                B.B(t, "Arm", new Vector3(x, .45f, .05f), new Vector3(.2f, .35f, .8f), m);
            return t;
        }

        public static Transform Television(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "TVStand", at, yaw, new Vector3(1.2f, 1.25f, .45f));
            if (t == null) return null;
            B.B(t, "Stand", new Vector3(0f, .25f, 0f), new Vector3(1.2f, .5f, .45f), p.Art.DarkWood);
            B.B(t, "Set", new Vector3(0f, .82f, -.02f), new Vector3(.8f, .6f, .4f), p.Art.Rubber);
            B.B(t, "Screen", new Vector3(0f, .84f, .185f), new Vector3(.62f, .46f, .01f), p.Art.DarkSteel);
            return t;
        }

        public static Transform Fridge(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Fridge", at, yaw, new Vector3(.75f, 1.8f, .72f));
            if (t == null) return null;
            B.B(t, "Body", new Vector3(0f, .9f, 0f), new Vector3(.75f, 1.8f, .72f), p.Art.White);
            B.B(t, "Split", new Vector3(0f, 1.25f, .365f), new Vector3(.73f, .02f, .01f), p.Art.Steel);
            B.B(t, "Handle", new Vector3(.3f, 1f, .38f), new Vector3(.03f, .4f, .03f), p.Art.Chrome);
            return t;
        }

        public static Transform Stove(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Range", at, yaw, new Vector3(.9f, .92f, .68f));
            if (t == null) return null;
            B.B(t, "Body", new Vector3(0f, .45f, 0f), new Vector3(.9f, .9f, .68f), p.Art.Steel);
            B.B(t, "Oven", new Vector3(0f, .4f, .345f), new Vector3(.7f, .45f, .01f), p.Art.Rubber);
            foreach (float x in new[] { -.22f, .22f })
            foreach (float z in new[] { -.15f, .15f })
                B.C(t, "Burner", new Vector3(x, .91f, z), new Vector3(.2f, .01f, .2f), p.Art.Rubber);
            return t;
        }

        public static Transform Crate(StationProps p, string room, Vector3 at, float yaw, Vector3 size)
        {
            Transform t = p.Place(room, "Crate", at, yaw, size);
            if (t == null) return null;
            B.B(t, "Box", new Vector3(0f, size.y * .5f, 0f), size, p.Art.Board);
            B.B(t, "Band", new Vector3(0f, size.y * .5f, 0f), size + new Vector3(.01f, -size.y * .7f, .01f), p.Art.DarkWood);
            return t;
        }

        public static Transform Drum(StationProps p, string room, Vector3 at, Material m = null)
        {
            Transform t = p.Place(room, "Drum", at, 0f, new Vector3(.6f, .9f, .6f));
            if (t == null) return null;
            B.C(t, "Body", new Vector3(0f, .45f, 0f), new Vector3(.58f, .45f, .58f), m != null ? m : p.Art.AccentDark);
            B.C(t, "Rim", new Vector3(0f, .88f, 0f), new Vector3(.6f, .02f, .6f), p.Art.Steel);
            return t;
        }

        public static Transform WaterCooler(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "WaterCooler", at, yaw, new Vector3(.36f, 1.35f, .36f));
            if (t == null) return null;
            B.B(t, "Body", new Vector3(0f, .5f, 0f), new Vector3(.34f, 1f, .34f), p.Art.White);
            B.C(t, "Bottle", new Vector3(0f, 1.18f, 0f), new Vector3(.26f, .17f, .26f), p.Art.Glass);
            return t;
        }

        public static Transform Vending(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "VendingMachine", at, yaw, new Vector3(.95f, 1.85f, .8f));
            if (t == null) return null;
            B.B(t, "Body", new Vector3(0f, .925f, 0f), new Vector3(.95f, 1.85f, .8f), p.Art.AccentDark);
            B.B(t, "Window", new Vector3(-.12f, 1.1f, .405f), new Vector3(.6f, 1.2f, .01f), p.Art.Glass);
            B.B(t, "Panel", new Vector3(.34f, 1.1f, .405f), new Vector3(.18f, .9f, .01f), p.Art.Rubber);
            return t;
        }

        public static Transform Treadmill(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Treadmill", at, yaw, new Vector3(.8f, 1.35f, 1.8f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Deck", new Vector3(0f, .12f, 0f), new Vector3(.8f, .24f, 1.8f), a.Rubber);
            B.B(t, "Belt", new Vector3(0f, .245f, .05f), new Vector3(.55f, .01f, 1.5f), a.DarkSteel);
            foreach (float x in new[] { -.36f, .36f }) B.B(t, "Upright", new Vector3(x, .75f, -.75f), new Vector3(.05f, 1.2f, .05f), a.Steel);
            B.B(t, "Console", new Vector3(0f, 1.3f, -.78f), new Vector3(.7f, .12f, .25f), a.Rubber);
            return t;
        }

        /// <summary>A flat bench with a barbell on its rack (the bar across +-0.95 m).</summary>
        public static Transform WeightBench(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "WeightBench", at, yaw, new Vector3(1.2f, 1.6f, 1.9f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Pad", new Vector3(0f, .45f, .2f), new Vector3(.3f, .08f, 1.2f), a.Leather);
            B.B(t, "Frame", new Vector3(0f, .2f, .2f), new Vector3(.08f, .4f, 1.1f), a.DarkSteel);
            foreach (float s in new[] { -.5f, .5f }) B.B(t, "Upright", new Vector3(s, .75f, -.5f), new Vector3(.06f, 1.5f, .06f), a.DarkSteel);
            B.B(t, "Bar", new Vector3(0f, 1.2f, -.5f), new Vector3(1.9f, .03f, .03f), a.Chrome);
            foreach (float s in new[] { -.75f, .75f })
                B.C(t, "Plate", new Vector3(s, 1.2f, -.5f), new Vector3(.45f, .03f, .45f), a.Rubber).transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            return t;
        }

        /// <summary>A heavy bag on a chain from the ceiling <paramref name="drop"/> metres above the floor.</summary>
        public static Transform PunchingBag(StationProps p, string room, Vector3 at, float drop = 3.75f)
        {
            Transform t = p.Hang(room, "PunchingBag", at, 0f);
            B.C(t, "Bag", new Vector3(0f, 1.45f, 0f), new Vector3(.4f, .55f, .4f), p.Art.Leather);
            float chain = Mathf.Max(.2f, drop - 2.05f);
            B.B(t, "Chain", new Vector3(0f, 2f + chain * .5f, 0f), new Vector3(.03f, chain, .03f), p.Art.Steel);
            return t;
        }

        /// <summary>A board on the wall (whiteboard, corkboard, map): no collider, nothing to walk into.</summary>
        public static Transform WallBoard(StationProps p, string room, Vector3 at, float yaw, float width, float height, Material face)
        {
            Transform t = p.Hang(room, "WallBoard", at, yaw);
            B.B(t, "Frame", new Vector3(0f, 0f, .015f), new Vector3(width + .06f, height + .06f, .03f), p.Art.DarkSteel);
            B.B(t, "Face", new Vector3(0f, 0f, .032f), new Vector3(width, height, .005f), face);
            return t;
        }

        public static Transform WallClock(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Hang(room, "Clock", at, yaw);
            GameObject face = B.C(t, "Face", new Vector3(0f, 0f, .03f), new Vector3(.32f, .015f, .32f), p.Art.White);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            B.B(t, "Hand", new Vector3(.04f, .03f, .05f), new Vector3(.1f, .012f, .005f), p.Art.Rubber);
            return t;
        }

        /// <summary>A green emergency-exit sign over a doorway (no words: an arrow and a running figure's block).</summary>
        public static Transform ExitSign(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Hang(room, "ExitSign", at, yaw);
            B.B(t, "Box", Vector3.zero, new Vector3(.42f, .2f, .07f), p.Art.White);
            B.B(t, "Glow", new Vector3(0f, 0f, .036f), new Vector3(.36f, .14f, .005f), p.Art.ExitGreen);
            return t;
        }

        public static Transform Plant(StationProps p, string room, Vector3 at)
        {
            Transform t = p.Place(room, "DeadPlant", at, 0f, new Vector3(.45f, 1.1f, .45f));
            if (t == null) return null;
            B.C(t, "Pot", new Vector3(0f, .2f, 0f), new Vector3(.42f, .2f, .42f), p.Art.DarkSteel);
            B.B(t, "Stems", new Vector3(0f, .7f, 0f), new Vector3(.3f, .8f, .3f), p.Art.Leather);
            return t;
        }
    }
}
