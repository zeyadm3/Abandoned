using UnityEngine;
using B = Abandoned.EditorTools.StationProps;

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
    }
}
