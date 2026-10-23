using UnityEngine;
using B = Abandoned.EditorTools.StationProps;
using M = Abandoned.EditorTools.StationMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>The station furniture for living in: beds, bathrooms, kitchens, the day room and the gym.</summary>
    public static partial class StationFurniture
    {
        public static Transform Bunk(StationProps p, string room, Vector3 at, float yaw, bool upper = true, Material blanket = null)
        {
            Transform t = p.Place(room, upper ? "BunkBed" : "Bed", at, yaw, new Vector3(.95f, upper ? 1.75f : .6f, 2.05f));
            if (t == null) return null;
            StationArt a = p.Art;
            int seed = Seed(at);
            Material cover = blanket != null ? blanket : a.Fabric;
            foreach (float y in upper ? new[] { .38f, 1.38f } : new[] { .38f })
            {
                foreach (float s in new[] { -1f, 1f })
                {
                    B.B(t, "SideRail", new Vector3(s * .45f, y, 0f), new Vector3(.04f, .1f, 2.02f), a.DarkSteel);
                    B.B(t, "EndRail", new Vector3(0f, y, s * .99f), new Vector3(.9f, .1f, .04f), a.DarkSteel);
                }
                for (int k = 0; k < 4; k++) B.B(t, "Slat", new Vector3(0f, y + .02f, -.75f + k * .5f), new Vector3(.88f, .02f, .06f), a.DarkSteel);
                Cushion(t, a, "Mattress", new Vector3(0f, y + .12f, 0f), new Vector3(.86f, .16f, 1.94f), a.Mattress);
                bool made = (seed + Mathf.RoundToInt(y)) % 3 != 0;
                GameObject cloth = Cushion(t, a, "Blanket", new Vector3(made ? 0f : .1f, y + .22f, made ? .3f : .45f), new Vector3(.9f, .06f, made ? 1.2f : .9f), cover, made ? 0f : 4f);
                if (!made) cloth.transform.localRotation = Quaternion.Euler(4f, 12f, 0f);
                Cushion(t, a, "Pillow", new Vector3(0f, y + .25f, -.75f), new Vector3(.58f, .1f, .32f), a.White, -6f);
            }
            float top = upper ? 1.75f : .62f;
            foreach (float x in new[] { -.45f, .45f })
            foreach (float z in new[] { -1f, 1f })
                B.B(t, "Post", new Vector3(x, top * .5f, z), new Vector3(.045f, top, .045f), a.DarkSteel);
            if (!upper) return t;
            // The ladder at the foot, the guard rail along the top bunk.
            foreach (float x in new[] { .2f, .4f }) B.B(t, "LadderRail", new Vector3(x, .9f, 1.02f), new Vector3(.025f, 1.5f, .025f), a.DarkSteel);
            for (int r = 0; r < 4; r++) B.B(t, "LadderRung", new Vector3(.3f, .55f + r * .3f, 1.02f), new Vector3(.2f, .025f, .025f), a.DarkSteel);
            B.B(t, "Guard", new Vector3(.45f, 1.62f, -.2f), new Vector3(.03f, .03f, 1.4f), a.DarkSteel);
            return t;
        }

        public static Transform Toilet(StationProps p, string room, Vector3 at, float yaw, Material m = null)
        {
            Transform t = p.Place(room, "Toilet", at, yaw, new Vector3(.45f, .8f, .7f));
            if (t == null) return null;
            Material mat = m != null ? m : p.Art.Porcelain;
            M.Draw(t, "Bowl", M.Lathe(p.Art, "ToiletBowl", new[]
            {
                new Vector2(0f, 0f), new Vector2(.12f, 0f), new Vector2(.12f, 0f), new Vector2(.13f, .18f), new Vector2(.19f, .34f), new Vector2(.2f, .4f),
                new Vector2(.2f, .4f), new Vector2(.15f, .41f), new Vector2(.15f, .41f), new Vector2(.08f, .3f), new Vector2(0f, .27f),
            }, 20), new Vector3(0f, 0f, .1f), Vector3.zero, new Vector3(1f, 1f, 1.25f), mat);
            B.B(t, "Seat", new Vector3(0f, .42f, .12f), new Vector3(.38f, .025f, .44f), p.Art.White);
            GameObject lid = B.B(t, "Lid", new Vector3(0f, .6f, -.13f), new Vector3(.38f, .38f, .025f), p.Art.White);
            lid.transform.localRotation = Quaternion.Euler(-10f, 0f, 0f);
            B.B(t, "Cistern", new Vector3(0f, .62f, -.27f), new Vector3(.42f, .36f, .16f), mat);
            B.B(t, "CisternLid", new Vector3(0f, .81f, -.27f), new Vector3(.44f, .03f, .18f), mat);
            B.B(t, "Flush", new Vector3(-.15f, .74f, -.18f), new Vector3(.06f, .02f, .04f), p.Art.Chrome);
            B.B(t, "Stain", new Vector3(0f, .005f, .1f), new Vector3(.5f, .003f, .6f), p.Art.Grime);
            return t;
        }

        public static Transform Sink(StationProps p, string room, Vector3 at, float yaw, Material m = null)
        {
            Transform t = p.Place(room, "Sink", at, yaw, new Vector3(.55f, .9f, .45f));
            if (t == null) return null;
            Material mat = m != null ? m : p.Art.Porcelain;
            M.Draw(t, "Basin", M.Lathe(p.Art, "SinkBasin", new[]
            {
                new Vector2(.1f, 0f), new Vector2(.24f, .1f), new Vector2(.26f, .16f), new Vector2(.26f, .16f), new Vector2(.22f, .16f), new Vector2(.22f, .16f),
                new Vector2(.1f, .06f), new Vector2(0f, .05f),
            }, 24), new Vector3(0f, .74f, .03f), Vector3.zero, new Vector3(1f, 1f, .8f), mat);
            M.Draw(t, "Pedestal", M.Lathe(p.Art, "SinkPedestal", new[]
            {
                new Vector2(0f, 0f), new Vector2(.11f, 0f), new Vector2(.11f, 0f), new Vector2(.07f, .35f), new Vector2(.09f, .74f), new Vector2(0f, .74f),
            }, 16), new Vector3(0f, 0f, -.04f), Vector3.zero, Vector3.one, mat);
            B.B(t, "Tap", new Vector3(0f, .95f, -.14f), new Vector3(.03f, .1f, .03f), p.Art.Chrome);
            B.B(t, "Spout", new Vector3(0f, .99f, -.09f), new Vector3(.025f, .025f, .1f), p.Art.Chrome);
            foreach (float x in new[] { -.08f, .08f }) B.B(t, "Handle", new Vector3(x, .93f, -.14f), new Vector3(.05f, .02f, .02f), p.Art.Chrome);
            B.B(t, "Mirror", new Vector3(0f, 1.45f, -.2f), new Vector3(.45f, .6f, .01f), p.Art.Chrome);
            B.B(t, "MirrorFrame", new Vector3(0f, 1.45f, -.21f), new Vector3(.49f, .64f, .01f), p.Art.DarkSteel);
            return t;
        }

        /// <summary>A sofa: the scanned one when it fits, else a code-built one with loose seat cushions.</summary>
        public static Transform Sofa(StationProps p, string room, Vector3 at, float yaw, float width = 2f, Material cover = null)
        {
            Transform t = p.Place(room, "Sofa", at, yaw, new Vector3(width, .85f, .9f));
            if (t == null) return null;
            if (cover == null && StationScans.In(t, "Sofa_01", Vector3.zero, 0f, new Vector3(width, .9f, .95f)) != null) return t;
            StationArt a = p.Art;
            Material m = cover != null ? cover : a.Fabric;
            B.B(t, "Frame", new Vector3(0f, .16f, .02f), new Vector3(width, .22f, .82f), m);
            int seats = Mathf.Max(2, Mathf.RoundToInt((width - .4f) / .6f));
            float cw = (width - .4f) / seats;
            for (int i = 0; i < seats; i++)
            {
                float x = -width * .5f + .2f + cw * (i + .5f);
                Cushion(t, a, "SeatCushion", new Vector3(x, .34f, .1f), new Vector3(cw - .02f, .14f, .66f), m);
                Cushion(t, a, "BackCushion", new Vector3(x, .6f, -.28f), new Vector3(cw - .02f, .42f, .2f), m, -10f);
            }
            B.B(t, "Back", new Vector3(0f, .5f, -.38f), new Vector3(width, .66f, .1f), m);
            foreach (float x in new[] { -width * .5f + .1f, width * .5f - .1f })
                Cushion(t, a, "Arm", new Vector3(x, .42f, .02f), new Vector3(.2f, .34f, .82f), m);
            foreach (float x in new[] { -width * .5f + .12f, width * .5f - .12f })
            foreach (float z in new[] { -.33f, .37f })
                B.B(t, "Foot", new Vector3(x, .025f, z), new Vector3(.05f, .05f, .05f), a.DarkWood);
            return t;
        }

        public static Transform Television(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "TVStand", at, yaw, new Vector3(1.2f, 1.25f, .45f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Stand", new Vector3(0f, .25f, 0f), new Vector3(1.2f, .5f, .45f), a.DarkWood);
            foreach (float x in new[] { -.3f, .3f }) B.B(t, "Door", new Vector3(x, .25f, .228f), new Vector3(.56f, .44f, .01f), a.Wood);
            B.B(t, "Set", new Vector3(0f, .83f, -.04f), new Vector3(.78f, .62f, .38f), a.Rubber);
            B.B(t, "Tube", new Vector3(0f, .82f, -.28f), new Vector3(.5f, .44f, .2f), a.Rubber);
            B.B(t, "Bezel", new Vector3(0f, .84f, .152f), new Vector3(.74f, .56f, .01f), a.DarkSteel);
            B.B(t, "Screen", new Vector3(-.06f, .85f, .158f), new Vector3(.56f, .44f, .01f), StationVehicleArt.For(a).Glass);
            for (int i = 0; i < 3; i++) B.B(t, "Knob", new Vector3(.3f, .98f - i * .1f, .16f), new Vector3(.04f, .04f, .02f), a.Chrome);
            M.Strut(t, "Antenna", new Vector3(0f, 1.14f, -.1f), new Vector3(-.28f, 1.42f, -.15f), .01f, a.Chrome);
            M.Strut(t, "Antenna", new Vector3(0f, 1.14f, -.1f), new Vector3(.25f, 1.45f, -.15f), .01f, a.Chrome);
            B.B(t, "VCR", new Vector3(0f, .53f, .02f), new Vector3(.45f, .08f, .32f), a.Rubber);
            return t;
        }

        public static Transform Fridge(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Fridge", at, yaw, new Vector3(.75f, 1.8f, .72f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .9f, -.02f), new Vector3(.75f, 1.8f, .68f), a.White);
            B.B(t, "FreezerDoor", new Vector3(0f, 1.52f, .335f), new Vector3(.73f, .54f, .05f), a.White);
            B.B(t, "Door", new Vector3(0f, .66f, .335f), new Vector3(.73f, 1.16f, .05f), a.White);
            B.B(t, "Gasket", new Vector3(0f, 1.24f, .33f), new Vector3(.73f, .02f, .04f), a.Rubber);
            foreach (float y in new[] { 1.3f, .85f }) B.B(t, "Handle", new Vector3(.3f, y + (y > 1f ? .12f : 0f), .38f), new Vector3(.03f, .35f, .04f), a.Chrome);
            B.B(t, "Grille", new Vector3(0f, .05f, .32f), new Vector3(.66f, .08f, .01f), a.DarkSteel);
            int seed = Seed(at);
            for (int i = 0; i < 4; i++)
            {
                int roll = seed + i * 7;
                if (roll % 3 == 0) continue;
                B.B(t, "Note", new Vector3(-.25f + (roll % 4) * .12f, 1f + (roll % 5) * .12f, .362f), new Vector3(.1f, .13f, .003f), roll % 2 == 0 ? a.Paper : a.Safety);
            }
            return t;
        }

        public static Transform Stove(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Range", at, yaw, new Vector3(.9f, .92f, .68f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .45f, 0f), new Vector3(.9f, .9f, .68f), a.Steel);
            B.B(t, "OvenDoor", new Vector3(0f, .42f, .345f), new Vector3(.76f, .52f, .02f), a.DarkSteel);
            B.B(t, "OvenWindow", new Vector3(0f, .45f, .357f), new Vector3(.5f, .26f, .005f), StationVehicleArt.For(a).Glass);
            B.B(t, "OvenHandle", new Vector3(0f, .72f, .38f), new Vector3(.6f, .03f, .03f), a.Chrome);
            B.B(t, "Backguard", new Vector3(0f, 1.02f, -.32f), new Vector3(.9f, .2f, .04f), a.Steel);
            for (int i = 0; i < 4; i++)
                B.C(t, "Knob", new Vector3(-.3f + i * .2f, .82f, .35f), new Vector3(.05f, .015f, .05f), a.Rubber).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            foreach (float x in new[] { -.22f, .22f })
            foreach (float z in new[] { -.15f, .15f })
            {
                B.C(t, "Burner", new Vector3(x, .905f, z), new Vector3(.2f, .006f, .2f), a.Rubber);
                B.B(t, "GrateA", new Vector3(x, .915f, z), new Vector3(.24f, .012f, .02f), a.DarkSteel);
                B.B(t, "GrateB", new Vector3(x, .915f, z), new Vector3(.02f, .012f, .24f), a.DarkSteel);
            }
            M.Draw(t, "Pot", M.Lathe(a, "StockPot", new[]
            {
                new Vector2(0f, 0f), new Vector2(.13f, 0f), new Vector2(.13f, 0f), new Vector2(.13f, .2f), new Vector2(.13f, .2f), new Vector2(.12f, .2f),
                new Vector2(.12f, .2f), new Vector2(0f, .02f),
            }, 18), new Vector3(-.22f, .92f, .15f), Vector3.zero, Vector3.one, a.Steel);
            return t;
        }

        public static Transform Treadmill(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "Treadmill", at, yaw, new Vector3(.8f, 1.35f, 1.8f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Deck", new Vector3(0f, .12f, 0f), new Vector3(.8f, .2f, 1.8f), a.Rubber);
            B.B(t, "Belt", new Vector3(0f, .225f, .05f), new Vector3(.55f, .01f, 1.5f), a.DarkSteel);
            foreach (float x in new[] { -.36f, .36f })
            {
                B.B(t, "SideRail", new Vector3(x, .24f, .05f), new Vector3(.09f, .03f, 1.5f), a.Steel);
                M.Strut(t, "Upright", new Vector3(x, .2f, -.8f), new Vector3(x, 1.2f, -.7f), .05f, a.Steel);
                B.B(t, "Handlebar", new Vector3(x, 1.05f, -.5f), new Vector3(.04f, .04f, .45f), a.Rubber);
            }
            B.B(t, "Console", new Vector3(0f, 1.28f, -.72f), new Vector3(.72f, .14f, .28f), a.Rubber).transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            B.B(t, "Display", new Vector3(0f, 1.32f, -.64f), new Vector3(.4f, .08f, .005f), StationVehicleArt.For(a).Glass).transform.localRotation = Quaternion.Euler(-25f, 0f, 0f);
            B.B(t, "Motor", new Vector3(0f, .2f, -.8f), new Vector3(.74f, .28f, .3f), a.DarkSteel);
            return t;
        }

        /// <summary>A flat bench with a barbell on its rack (the bar across +-0.95 m).</summary>
        public static Transform WeightBench(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "WeightBench", at, yaw, new Vector3(1.2f, 1.6f, 1.9f));
            if (t == null) return null;
            StationArt a = p.Art;
            Cushion(t, a, "Pad", new Vector3(0f, .46f, .2f), new Vector3(.3f, .08f, 1.2f), a.Leather);
            B.B(t, "Frame", new Vector3(0f, .22f, .2f), new Vector3(.08f, .08f, 1.1f), a.DarkSteel);
            foreach (float z in new[] { -.35f, .75f }) B.B(t, "Foot", new Vector3(0f, .03f, z), new Vector3(.5f, .06f, .08f), a.DarkSteel);
            B.B(t, "Leg", new Vector3(0f, .21f, .75f), new Vector3(.06f, .4f, .06f), a.DarkSteel);
            foreach (float s in new[] { -.5f, .5f })
            {
                B.B(t, "Upright", new Vector3(s, .75f, -.5f), new Vector3(.06f, 1.5f, .06f), a.DarkSteel);
                B.B(t, "Hook", new Vector3(s, 1.18f, -.46f), new Vector3(.06f, .04f, .08f), a.DarkSteel);
            }
            B.B(t, "Bar", new Vector3(0f, 1.22f, -.46f), new Vector3(1.9f, .03f, .03f), a.Chrome);
            Mesh plate = M.Lathe(a, "WeightPlate", new[]
            {
                new Vector2(.03f, -.03f), new Vector2(.22f, -.03f), new Vector2(.22f, -.03f), new Vector2(.225f, 0f), new Vector2(.22f, .03f),
                new Vector2(.22f, .03f), new Vector2(.03f, .03f),
            }, 24);
            foreach (float s in new[] { -.75f, -.68f, .68f, .75f })
                M.Draw(t, "Plate", plate, new Vector3(s, 1.22f, -.46f), new Vector3(0f, 0f, 90f), Vector3.one, a.Rubber);
            foreach (float x in new[] { .3f, .45f })
                M.Draw(t, "LoosePlate", plate, new Vector3(x, .03f, .9f), Vector3.zero, Vector3.one * .8f, a.Rubber);
            return t;
        }

        /// <summary>A heavy bag on a chain from the ceiling <paramref name="drop"/> metres above the floor.</summary>
        public static Transform PunchingBag(StationProps p, string room, Vector3 at, float drop = 3.75f)
        {
            Transform t = p.Hang(room, "PunchingBag", at, 0f);
            M.Draw(t, "Bag", M.Lathe(p.Art, "HeavyBag", new[]
            {
                new Vector2(0f, 0f), new Vector2(.15f, .02f), new Vector2(.19f, .1f), new Vector2(.2f, .3f), new Vector2(.2f, .9f), new Vector2(.19f, 1.02f),
                new Vector2(.12f, 1.08f), new Vector2(0f, 1.1f),
            }, 20), new Vector3(0f, .9f, 0f), Vector3.zero, Vector3.one, p.Art.Leather);
            B.B(t, "Strap", new Vector3(0f, 1.6f, 0f), new Vector3(.41f, .06f, .41f), p.Art.Rubber);
            float chain = Mathf.Max(.2f, drop - 2.05f);
            for (int i = 0; i < 3; i++)
                M.Strut(t, "Sling", new Vector3(Mathf.Cos(i * 2.1f) * .15f, 2f, Mathf.Sin(i * 2.1f) * .15f), new Vector3(0f, 2.25f, 0f), .015f, p.Art.Steel);
            B.B(t, "Chain", new Vector3(0f, 2.25f + chain * .5f, 0f), new Vector3(.03f, chain, .03f), p.Art.Steel);
            return t;
        }
    }
}
