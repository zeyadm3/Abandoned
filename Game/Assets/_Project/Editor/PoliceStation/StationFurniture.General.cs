using UnityEngine;
using B = Abandoned.EditorTools.StationProps;
using M = Abandoned.EditorTools.StationMeshes;

namespace Abandoned.EditorTools
{
    /// <summary>The small fittings every room uses: crates, drums, a water cooler, a vending machine, boards and clocks
    /// on the walls, exit signs and the dead office plant.</summary>
    public static partial class StationFurniture
    {
        /// <summary>A slatted timber crate: battens round planks with gaps, its lid nailed down.</summary>
        public static Transform Crate(StationProps p, string room, Vector3 at, float yaw, Vector3 size)
        {
            Transform t = p.Place(room, "Crate", at, yaw, size);
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Core", new Vector3(0f, size.y * .5f, 0f), size - new Vector3(.04f, .02f, .04f), a.Rubber);
            int planks = Mathf.Max(2, Mathf.RoundToInt(size.y / .14f));
            float ph = size.y / planks;
            for (int i = 0; i < planks; i++)
            {
                float y = ph * (i + .5f);
                B.B(t, "PlankF", new Vector3(0f, y, size.z * .5f - .01f), new Vector3(size.x, ph - .02f, .02f), a.Board);
                B.B(t, "PlankB", new Vector3(0f, y, -size.z * .5f + .01f), new Vector3(size.x, ph - .02f, .02f), a.Board);
                B.B(t, "PlankL", new Vector3(-size.x * .5f + .01f, y, 0f), new Vector3(.02f, ph - .02f, size.z - .04f), a.Board);
                B.B(t, "PlankR", new Vector3(size.x * .5f - .01f, y, 0f), new Vector3(.02f, ph - .02f, size.z - .04f), a.Board);
            }
            B.B(t, "Lid", new Vector3(0f, size.y - .01f, 0f), new Vector3(size.x, .02f, size.z), a.Board);
            foreach (float x in new[] { -size.x * .5f + .04f, size.x * .5f - .04f })
            foreach (float z in new[] { -size.z * .5f + .025f, size.z * .5f - .025f })
                B.B(t, "Batten", new Vector3(x, size.y * .5f, z), new Vector3(.07f, size.y, .045f), a.DarkWood);
            B.B(t, "Stencil", new Vector3(0f, size.y * .55f, size.z * .5f + .002f), new Vector3(size.x * .45f, size.y * .25f, .002f), a.Grime);
            return t;
        }

        /// <summary>A steel drum (the scanned barrel when there is one), ribbed, its bung on top.</summary>
        public static Transform Drum(StationProps p, string room, Vector3 at, Material m = null)
        {
            Transform t = p.Place(room, "Drum", at, 0f, new Vector3(.6f, .9f, .6f));
            if (t == null) return null;
            if (m == null && StationScans.In(t, Seed(at) % 2 == 0 ? "Barrel_01" : "barrel_03", Vector3.zero, Seed(at) % 90, new Vector3(.62f, .92f, .62f)) != null) return t;
            M.Draw(t, "Body", M.Lathe(p.Art, "Drum", new[]
            {
                new Vector2(0f, .01f), new Vector2(.28f, .01f), new Vector2(.28f, .01f), new Vector2(.29f, .03f), new Vector2(.28f, .05f), new Vector2(.28f, .29f),
                new Vector2(.295f, .3f), new Vector2(.28f, .31f), new Vector2(.28f, .59f), new Vector2(.295f, .6f), new Vector2(.28f, .61f), new Vector2(.28f, .85f),
                new Vector2(.29f, .87f), new Vector2(.28f, .89f), new Vector2(.28f, .89f), new Vector2(0f, .88f),
            }, 20), Vector3.zero, Vector3.zero, Vector3.one, m != null ? m : p.Art.AccentDark);
            B.C(t, "Bung", new Vector3(.15f, .895f, .05f), new Vector3(.06f, .01f, .06f), p.Art.Steel);
            return t;
        }

        public static Transform WaterCooler(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "WaterCooler", at, yaw, new Vector3(.36f, 1.35f, .36f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .5f, 0f), new Vector3(.34f, 1f, .34f), a.White);
            B.B(t, "Recess", new Vector3(0f, .82f, .17f), new Vector3(.2f, .2f, .01f), a.DarkSteel);
            foreach (float x in new[] { -.05f, .05f }) B.B(t, "Tap", new Vector3(x, .88f, .18f), new Vector3(.03f, .04f, .04f), x < 0f ? a.Red : a.AccentDark);
            B.B(t, "DripTray", new Vector3(0f, .73f, .17f), new Vector3(.18f, .02f, .06f), a.DarkSteel);
            M.Draw(t, "Bottle", M.Lathe(a, "CoolerBottle", new[]
            {
                new Vector2(.03f, 0f), new Vector2(.05f, .02f), new Vector2(.13f, .08f), new Vector2(.13f, .3f), new Vector2(.12f, .34f), new Vector2(0f, .36f),
            }, 16), new Vector3(0f, 1f, 0f), Vector3.zero, Vector3.one, a.Glass);
            return t;
        }

        public static Transform Vending(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "VendingMachine", at, yaw, new Vector3(.95f, 1.85f, .8f));
            if (t == null) return null;
            StationArt a = p.Art;
            B.B(t, "Body", new Vector3(0f, .925f, -.02f), new Vector3(.95f, 1.85f, .76f), a.AccentDark);
            B.B(t, "Cabinet", new Vector3(-.12f, 1.1f, .3f), new Vector3(.62f, 1.25f, .08f), a.Rubber);
            for (int row = 0; row < 5; row++)
            for (int c = 0; c < 4; c++)
            {
                if ((row * 5 + c * 3) % 7 == 0) continue;   // emptied
                Material can = (row + c) % 3 == 0 ? a.Red : (row + c) % 3 == 1 ? a.Safety : a.White;
                B.B(t, "Can", new Vector3(-.35f + c * .15f, .62f + row * .23f, .32f), new Vector3(.08f, .13f, .08f), can);
            }
            for (int row = 0; row < 5; row++) B.B(t, "Spiral", new Vector3(-.12f, .56f + row * .23f, .33f), new Vector3(.58f, .01f, .04f), a.Chrome);
            B.B(t, "Window", new Vector3(-.12f, 1.1f, .38f), new Vector3(.6f, 1.2f, .01f), a.Glass);
            B.B(t, "Panel", new Vector3(.34f, 1.1f, .37f), new Vector3(.2f, .9f, .02f), a.Rubber);
            for (int i = 0; i < 9; i++) B.B(t, "Button", new Vector3(.3f + (i % 3) * .04f, 1.3f - (i / 3) * .05f, .385f), new Vector3(.025f, .025f, .01f), a.Porcelain);
            B.B(t, "CoinSlot", new Vector3(.34f, 1.05f, .385f), new Vector3(.05f, .08f, .01f), a.Chrome);
            B.B(t, "Flap", new Vector3(-.12f, .3f, .37f), new Vector3(.55f, .16f, .02f), a.Rubber);
            return t;
        }

        /// <summary>A board on the wall (whiteboard, corkboard, map): no collider, nothing to walk into.</summary>
        public static Transform WallBoard(StationProps p, string room, Vector3 at, float yaw, float width, float height, Material face)
        {
            Transform t = p.Hang(room, "WallBoard", at, yaw);
            B.B(t, "Frame", new Vector3(0f, 0f, .015f), new Vector3(width + .06f, height + .06f, .03f), p.Art.DarkSteel);
            B.B(t, "Face", new Vector3(0f, 0f, .032f), new Vector3(width, height, .005f), face);
            B.B(t, "Ledge", new Vector3(0f, -height * .5f - .04f, .05f), new Vector3(width * .8f, .025f, .06f), p.Art.DarkSteel);
            int seed = Seed(at);
            for (int i = 0; i < 4; i++)
            {
                int roll = seed * 13 + i * 7;
                if (roll % 3 == 0) continue;
                GameObject note = B.B(t, "Note", new Vector3(-width * .35f + i * width * .23f, height * ((roll % 5) * .12f - .2f), .036f),
                    new Vector3(.18f, .22f, .002f), roll % 2 == 0 ? p.Art.Paper : p.Art.White);
                note.transform.localRotation = Quaternion.Euler(0f, 0f, roll % 9 - 4);
            }
            return t;
        }

        public static Transform WallClock(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Hang(room, "Clock", at, yaw);
            GameObject rim = B.C(t, "Rim", new Vector3(0f, 0f, .025f), new Vector3(.36f, .025f, .36f), p.Art.DarkSteel);
            rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            GameObject face = B.C(t, "Face", new Vector3(0f, 0f, .04f), new Vector3(.32f, .012f, .32f), p.Art.White);
            face.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 12; i++)
            {
                float ang = i * 30f * Mathf.Deg2Rad;
                B.B(t, "Tick", new Vector3(Mathf.Sin(ang) * .13f, Mathf.Cos(ang) * .13f, .053f), new Vector3(.01f, .01f, .003f), p.Art.Rubber);
            }
            // Stopped at twenty to four.
            B.B(t, "HourHand", new Vector3(.03f, -.02f, .056f), new Vector3(.015f, .08f, .003f), p.Art.Rubber).transform.localRotation = Quaternion.Euler(0f, 0f, -115f);
            B.B(t, "MinuteHand", new Vector3(-.04f, .02f, .058f), new Vector3(.01f, .12f, .003f), p.Art.Rubber).transform.localRotation = Quaternion.Euler(0f, 0f, 60f);
            return t;
        }

        /// <summary>A green emergency-exit sign over a doorway (no words: an arrow and a running figure's block).</summary>
        public static Transform ExitSign(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Hang(room, "ExitSign", at, yaw);
            B.B(t, "Box", Vector3.zero, new Vector3(.42f, .2f, .07f), p.Art.White);
            B.B(t, "Glow", new Vector3(0f, 0f, .036f), new Vector3(.36f, .14f, .005f), p.Art.ExitGreen);
            B.B(t, "Figure", new Vector3(-.08f, 0f, .04f), new Vector3(.06f, .1f, .003f), p.Art.White);
            B.B(t, "Arrow", new Vector3(.07f, 0f, .04f), new Vector3 (.12f, .03f, .003f), p.Art.White);
            B.B(t, "Bracket", new Vector3(0f, .13f, -.02f), new Vector3(.06f, .06f, .04f), p.Art.DarkSteel);
            return t;
        }

        /// <summary>A dead office plant: the pot, dry soil and what is left of it (dry branches from the scans).</summary>
        public static Transform Plant(StationProps p, string room, Vector3 at)
        {
            Transform t = p.Place(room, "DeadPlant", at, 0f, new Vector3(.45f, 1.1f, .45f));
            if (t == null) return null;
            M.Draw(t, "Pot", M.Lathe(p.Art, "PlantPot", new[]
            {
                new Vector2(0f, 0f), new Vector2(.15f, 0f), new Vector2(.15f, 0f), new Vector2(.2f, .38f), new Vector2(.22f, .4f), new Vector2(.22f, .42f),
                new Vector2(.2f, .42f), new Vector2(.2f, .42f), new Vector2(0f, .4f),
            }, 18), Vector3.zero, Vector3.zero, Vector3.one, p.Art.DarkSteel);
            B.C(t, "Soil", new Vector3(0f, .405f, 0f), new Vector3(.38f, .005f, .38f), p.Art.Dirt);
            if (StationScans.In(t, "dry_branches_medium_01", new Vector3(0f, .4f, 0f), Seed(at) % 180, new Vector3(.45f, .7f, .45f)) == null)
                B.B(t, "Stems", new Vector3(0f, .7f, 0f), new Vector3(.3f, .6f, .3f), p.Art.Leather);
            B.B(t, "Leaf", new Vector3(.25f, .005f, .1f), new Vector3(.08f, .004f, .05f), p.Art.Leather).transform.localRotation = Quaternion.Euler(0f, 30f, 0f);
            return t;
        }
    }
}
