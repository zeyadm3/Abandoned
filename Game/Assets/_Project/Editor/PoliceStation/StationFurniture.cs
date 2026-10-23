using UnityEngine;
using B = Abandoned.EditorTools.StationProps;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The furniture both stations share, original models built in code in each piece's own frame (+z is its front), with
    /// photo-scanned pieces from the project's Poly Haven set on and in them where they fit (a desk lamp, binders, waiting
    /// chairs, a sofa). Each method places one piece through <see cref="StationProps.Place"/>, so every piece is solid and
    /// kept off doorways and flights, and returns null when it was left out. Office pieces here; small fittings in
    /// StationFurniture.General.cs, beds, bathrooms, kitchens and gym kit in StationFurniture.Domestic.cs, the extra dressing
    /// in StationFurniture.Extra.cs and the parts they are made of in StationFurniture.Parts.cs.
    /// </summary>
    public static partial class StationFurniture
    {
        public static Transform Desk(StationProps p, string room, Vector3 at, float yaw, bool computer = true, float width = 1.5f)
        {
            Transform t = p.Place(room, "Desk", at, yaw, new Vector3(width, .78f, .8f));
            if (t == null) return null;
            StationArt a = p.Art;
            int seed = Seed(at);
            B.B(t, "Top", new Vector3(0f, .755f, 0f), new Vector3(width, .035f, .8f), a.Wood);
            B.B(t, "Edge", new Vector3(0f, .755f, .401f), new Vector3(width, .038f, .006f), a.DarkWood);
            B.B(t, "Blotter", new Vector3(-width * .1f, .775f, .12f), new Vector3(.6f, .004f, .42f), a.Leather);
            float px = width * .5f - .24f;
            B.B(t, "Pedestal", new Vector3(px, .37f, -.01f), new Vector3(.44f, .72f, .74f), a.DarkSteel);
            DrawerFront(t, a, new Vector3(px, .62f, .365f), .4f, .16f, true);
            DrawerFront(t, a, new Vector3(px, .44f, .365f), .4f, .16f);
            DrawerFront(t, a, new Vector3(px, .2f, .365f), .4f, .28f);
            B.B(t, "Leg", new Vector3(-width * .5f + .03f, .37f, 0f), new Vector3(.04f, .74f, .72f), a.DarkSteel);
            B.B(t, "Foot", new Vector3(-width * .5f + .03f, .015f, 0f), new Vector3(.06f, .03f, .76f), a.Rubber);
            B.B(t, "Modesty", new Vector3(-.2f, .46f, -.37f), new Vector3(width - .5f, .52f, .02f), a.DarkSteel);
            B.B(t, "CentreDrawer", new Vector3(-.15f, .7f, .37f), new Vector3(.6f, .06f, .015f), a.Steel);
            if (computer)
            {
                bool crt = seed % 3 != 0;
                if (crt)
                {
                    B.B(t, "MonitorCase", new Vector3(-.15f, .99f, -.12f), new Vector3(.42f, .38f, .4f), a.White);
                    B.B(t, "MonitorBack", new Vector3(-.15f, .97f, -.38f), new Vector3(.32f, .28f, .14f), a.White);
                    B.B(t, "Bezel", new Vector3(-.15f, 1f, .083f), new Vector3(.38f, .32f, .01f), a.Porcelain);
                    B.B(t, "Screen", new Vector3(-.15f, 1.005f, .09f), new Vector3(.31f, .24f, .005f), a.Rubber);
                    B.B(t, "Stand", new Vector3(-.15f, .79f, -.1f), new Vector3(.24f, .03f, .24f), a.White);
                }
                else
                {
                    B.B(t, "Panel", new Vector3(-.15f, 1.05f, -.2f), new Vector3(.52f, .33f, .03f), a.Rubber);
                    B.B(t, "Neck", new Vector3(-.15f, .86f, -.23f), new Vector3(.05f, .16f, .04f), a.DarkSteel);
                    B.B(t, "Foot", new Vector3(-.15f, .78f, -.2f), new Vector3(.22f, .015f, .16f), a.DarkSteel);
                }
                B.B(t, "Keyboard", new Vector3(-.15f, .785f, .2f), new Vector3(.44f, .02f, .15f), a.White);
                for (int r = 0; r < 4; r++)
                    B.B(t, "Keys", new Vector3(-.15f, .797f, .15f + r * .032f), new Vector3(.4f, .006f, .024f), a.Porcelain);
                B.B(t, "Mouse", new Vector3(.17f, .79f, .2f), new Vector3(.06f, .03f, .1f), a.White);
            }
            if (seed % 2 == 0)
            {
                B.B(t, "PhoneBase", new Vector3(width * .38f, .8f, -.15f), new Vector3(.2f, .06f, .22f), a.Rubber);
                B.B(t, "Handset", new Vector3(width * .38f, .84f, -.15f), new Vector3(.06f, .04f, .22f), a.Rubber);
            }
            ClutterOn(t, .775f, width, .8f, seed, a);
            if (seed % 3 == 0) Prop(t, "desk_lamp_arm_01", new Vector3(-width * .42f, .772f, -.25f), 30f, new Vector3(.45f, .6f, .45f));
            else if (seed % 3 == 1) Prop(t, "stationery_supplies", new Vector3(width * .3f, .772f, .2f), -10f, new Vector3(.35f, .2f, .3f));
            if (seed % 4 == 2) Prop(t, "binder_notebook", new Vector3(width * .42f, .772f, -.28f), 80f, new Vector3(.32f, .3f, .1f));
            return t;
        }

        /// <summary>An office swivel chair (or, <paramref name="office"/> false, a waiting-room chair: a scan when there is one).</summary>
        public static Transform Chair(StationProps p, string room, Vector3 at, float yaw, bool office = true)
        {
            int seed = Seed(at);
            // Nobody pushed the chairs in: each is a little off square.
            float jitter = (seed % 7 - 3) * 6f;
            Transform t = p.Place(room, office ? "OfficeChair" : "Chair", at, yaw + jitter, new Vector3(.58f, .98f, .58f));
            if (t == null) return null;
            StationArt a = p.Art;
            if (!office)
            {
                string id = seed % 2 == 0 ? "GreenChair_01" : "SchoolChair_01";
                if (StationScans.In(t, id, Vector3.zero, 180f, new Vector3(.58f, .98f, .58f)) != null) return t;
                Cushion(t, a, "Seat", new Vector3(0f, .46f, .02f), new Vector3(.44f, .06f, .42f), a.Fabric);
                Cushion(t, a, "Back", new Vector3(0f, .74f, -.2f), new Vector3(.42f, .3f, .05f), a.Fabric, -8f);
                foreach (Vector2 l in new[] { new Vector2(-.19f, -.19f), new Vector2(.19f, -.19f), new Vector2(-.19f, .19f), new Vector2(.19f, .19f) })
                    B.B(t, "Leg", new Vector3(l.x, .215f, l.y), new Vector3(.025f, .43f, .025f), a.Chrome);
                foreach (float x in new[] { -.19f, .19f }) B.B(t, "BackPost", new Vector3(x, .72f, -.22f), new Vector3(.025f, .55f, .025f), a.Chrome);
                return t;
            }
            StarBase(t, a, .44f, seed % 72);
            B.B(t, "SeatPan", new Vector3(0f, .42f, 0f), new Vector3(.4f, .03f, .38f), a.Rubber);
            Cushion(t, a, "Seat", new Vector3(0f, .47f, .02f), new Vector3(.5f, .08f, .48f), a.Fabric);
            B.B(t, "Spine", new Vector3(0f, .55f, -.25f), new Vector3(.06f, .28f, .04f), a.Rubber);
            Cushion(t, a, "Back", new Vector3(0f, .8f, -.25f), new Vector3(.46f, .44f, .08f), a.Fabric, -6f);
            foreach (float x in new[] { -.26f, .26f })
            {
                B.B(t, "ArmPost", new Vector3(x, .56f, 0f), new Vector3(.03f, .18f, .03f), a.Rubber);
                B.B(t, "ArmPad", new Vector3(x, .655f, .02f), new Vector3(.06f, .03f, .26f), a.Rubber);
            }
            B.B(t, "Lever", new Vector3(.2f, .38f, .1f), new Vector3(.12f, .015f, .015f), a.Rubber);
            return t;
        }

        public static Transform FilingCabinet(StationProps p, string room, Vector3 at, float yaw)
        {
            Transform t = p.Place(room, "FilingCabinet", at, yaw, new Vector3(.48f, 1.33f, .64f));
            if (t == null) return null;
            StationArt a = p.Art;
            int seed = Seed(at), open = seed % 5 == 1 ? seed % 4 : -1;
            B.B(t, "Body", new Vector3(0f, .665f, -.01f), new Vector3(.48f, 1.33f, .62f), a.DarkSteel);
            B.B(t, "TopLip", new Vector3(0f, 1.335f, 0f), new Vector3(.49f, .012f, .645f), a.DarkSteel);
            B.B(t, "Plinth", new Vector3(0f, .03f, .3f), new Vector3(.46f, .06f, .02f), a.Rubber);
            B.C(t, "Lock", new Vector3(.17f, 1.28f, .325f), new Vector3(.03f, .008f, .03f), a.Chrome).transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            for (int i = 0; i < 4; i++)
            {
                float y = .2f + i * .31f;
                if (i == open)
                {
                    // One drawer left out, its files leaning in it.
                    B.B(t, "DrawerBox", new Vector3(0f, y, .5f), new Vector3(.4f, .26f, .4f), a.Steel);
                    for (int f = 0; f < 6; f++)
                        B.B(t, "File", new Vector3(0f, y + .1f, .36f + f * .055f), new Vector3(.36f, .2f, .012f), f % 2 == 0 ? a.Paper : a.Board).transform.localRotation = Quaternion.Euler(f * 3f - 8f, 0f, 0f);
                    DrawerFront(t, a, new Vector3(0f, y, .705f), .42f, .27f);
                    continue;
                }
                DrawerFront(t, a, new Vector3(0f, y, .305f), .42f, .27f);
            }
            return t;
        }

        /// <summary>Steel shelving with archive boxes, 2 m tall: braced, bolted, the boxes lidded and labelled.</summary>
        public static Transform Shelving(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f, int seed = 0, Material stock = null)
        {
            Transform t = p.Place(room, "Shelving", at, yaw, new Vector3(width, 2.1f, .5f));
            if (t == null) return null;
            StationArt a = p.Art;
            foreach (float x in new[] { -width * .5f + .03f, width * .5f - .03f })
            {
                foreach (float z in new[] { -.22f, .22f })
                {
                    B.B(t, "Upright", new Vector3(x, 1.05f, z), new Vector3(.035f, 2.1f, .035f), a.Safety);
                    B.B(t, "FootPlate", new Vector3(x, .005f, z), new Vector3(.07f, .01f, .07f), a.DarkSteel);
                }
                for (int k = 0; k < 4; k++)
                    StationMeshes.Strut(t, "SideBrace", new Vector3(x, .1f + k * .5f, -.2f), new Vector3(x, .6f + k * .5f, .2f), .015f, a.Safety);
            }
            StationMeshes.Strut(t, "BackBrace", new Vector3(-width * .5f + .05f, .1f, -.235f), new Vector3(width * .5f - .05f, 2f, -.235f), .018f, a.DarkSteel);
            StationMeshes.Strut(t, "BackBrace", new Vector3(width * .5f - .05f, .1f, -.235f), new Vector3(-width * .5f + .05f, 2f, -.235f), .018f, a.DarkSteel);
            for (int s = 0; s < 5; s++)
            {
                float y = .08f + s * .48f;
                B.B(t, "Shelf", new Vector3(0f, y, 0f), new Vector3(width - .04f, .025f, .46f), a.Steel);
                B.B(t, "ShelfLip", new Vector3(0f, y - .02f, .225f), new Vector3(width - .04f, .04f, .01f), a.Steel);
                if (s == 4) continue;
                for (float x = -width * .5f + .25f; x < width * .5f - .2f; x += .42f)
                {
                    int roll = Mathf.Abs((int)(x * 97f) + s * 31 + seed * 17) % 9;
                    if (roll == 0) continue;
                    float h = roll % 2 == 0 ? .28f : .34f, pull = roll == 5 ? .1f : 0f;
                    Material box = stock != null ? stock : roll < 5 ? a.Board : a.Paper;
                    Vector3 c = new(x + (roll % 3 - 1) * .02f, y + .013f + h * .5f, .02f + pull);
                    GameObject g = B.B(t, "Box", c, new Vector3(.36f, h, .4f), box);
                    g.transform.localRotation = Quaternion.Euler(0f, (roll % 5 - 2) * 2.5f, 0f);
                    B.B(t, "Lid", c + new Vector3(0f, h * .5f - .02f, 0f), new Vector3(.37f, .04f, .41f), box);
                    B.B(t, "Hand", c + new Vector3(0f, h * .2f, .201f), new Vector3(.08f, .03f, .004f), a.Rubber);
                    B.B(t, "Tag", c + new Vector3(0f, -h * .15f, .201f), new Vector3(.14f, .08f, .003f), a.White);
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
            int seed = Seed(at);
            B.B(t, "Body", new Vector3(0f, .95f, 0f), new Vector3(w, 1.9f, .5f), a.DarkSteel);
            B.B(t, "Base", new Vector3(0f, .05f, .24f), new Vector3(w, .1f, .03f), a.Rubber);
            GameObject slope = B.B(t, "SlopeTop", new Vector3(0f, 1.97f, .04f), new Vector3(w, .02f, .5f), a.DarkSteel);
            slope.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            for (int i = 0; i < count; i++)
            {
                float x = -w * .5f + .2f + i * .4f;
                bool ajar = (seed + i) % 6 == 2;
                if (ajar)
                {
                    B.B(t, "Inside", new Vector3(x, 1f, .245f), new Vector3(.34f, 1.68f, .005f), a.Rubber);
                    B.B(t, "Shelf", new Vector3(x, 1.55f, .1f), new Vector3(.34f, .02f, .3f), a.Steel);
                    Transform hinge = new GameObject("Door").transform;
                    hinge.SetParent(t, false);
                    hinge.localPosition = new Vector3(x - .18f, 0f, .255f);
                    hinge.localRotation = Quaternion.Euler(0f, -60f - (seed % 3) * 15f, 0f);
                    B.B(hinge, "Leaf", new Vector3(.18f, 1f, 0f), new Vector3(.36f, 1.7f, .015f), a.AccentDark);
                    continue;
                }
                B.B(t, "Door", new Vector3(x, 1f, .255f), new Vector3(.36f, 1.7f, .01f), a.AccentDark);
                for (int v = 0; v < 4; v++) B.B(t, "Vent", new Vector3(x, 1.58f + v * .035f, .262f), new Vector3(.22f, .012f, .005f), a.Rubber);
                for (int v = 0; v < 4; v++) B.B(t, "Vent", new Vector3(x, .22f + v * .035f, .262f), new Vector3(.22f, .012f, .005f), a.Rubber);
                B.B(t, "Handle", new Vector3(x + .12f, 1.05f, .266f), new Vector3(.03f, .12f, .02f), a.Chrome);
                B.B(t, "NumberPlate", new Vector3(x, 1.4f, .262f), new Vector3(.07f, .04f, .004f), a.Brass);
                if ((seed + i) % 4 == 1) B.B(t, "Padlock", new Vector3(x + .12f, .97f, .28f), new Vector3(.04f, .05f, .02f), a.Brass);
            }
            B.B(t, "Bench", new Vector3(0f, .43f, .62f), new Vector3(w, .045f, .3f), a.Wood);
            foreach (float x in new[] { -w * .5f + .1f, w * .5f - .1f }) B.B(t, "BenchLeg", new Vector3(x, .21f, .62f), new Vector3(.04f, .42f, .26f), a.DarkSteel);
            return t;
        }

        public static Transform Bench(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f)
        {
            Transform t = p.Place(room, "Bench", at, yaw, new Vector3(width, .46f, .4f));
            if (t == null) return null;
            for (int i = 0; i < 3; i++) B.B(t, "Slat", new Vector3(0f, .44f, -.13f + i * .13f), new Vector3(width, .04f, .11f), p.Art.Wood);
            foreach (float x in new[] { -width * .5f + .15f, width * .5f - .15f })
            {
                B.B(t, "Leg", new Vector3(x, .21f, 0f), new Vector3(.05f, .42f, .32f), p.Art.DarkSteel);
                B.B(t, "Foot", new Vector3(x, .015f, 0f), new Vector3(.08f, .03f, .36f), p.Art.Rubber);
            }
            B.B(t, "Stretcher", new Vector3(0f, .12f, 0f), new Vector3(width - .3f, .04f, .04f), p.Art.DarkSteel);
            return t;
        }

        public static Transform Table(StationProps p, string room, Vector3 at, float yaw, float width = 1.8f, float depth = .9f, Material top = null)
        {
            Transform t = p.Place(room, "Table", at, yaw, new Vector3(width, .76f, depth));
            if (t == null) return null;
            Material surface = top != null ? top : p.Art.Wood;
            B.B(t, "Top", new Vector3(0f, .74f, 0f), new Vector3(width, .035f, depth), surface);
            B.B(t, "EdgeBand", new Vector3(0f, .72f, 0f), new Vector3(width + .006f, .02f, depth + .006f), p.Art.DarkWood);
            foreach (float s in new[] { -1f, 1f })
            {
                B.B(t, "Apron", new Vector3(0f, .66f, s * (depth * .5f - .05f)), new Vector3(width - .16f, .09f, .02f), p.Art.DarkSteel);
                B.B(t, "Apron", new Vector3(s * (width * .5f - .05f), .66f, 0f), new Vector3(.02f, .09f, depth - .16f), p.Art.DarkSteel);
            }
            foreach (float x in new[] { -width * .5f + .06f, width * .5f - .06f })
            foreach (float z in new[] { -depth * .5f + .06f, depth * .5f - .06f })
            {
                B.B(t, "Leg", new Vector3(x, .36f, z), new Vector3(.045f, .72f, .045f), p.Art.DarkSteel);
                B.B(t, "Glide", new Vector3(x, .01f, z), new Vector3(.055f, .02f, .055f), p.Art.Rubber);
            }
            ClutterOn(t, .76f, width, depth, Mathf.RoundToInt(at.x * 11f + at.z * 5f + at.y), p.Art);
            return t;
        }

        public static Transform Counter(StationProps p, string room, Vector3 at, float yaw, float length, float height = 1.05f, Material front = null)
        {
            Transform t = p.Place(room, "Counter", at, yaw, new Vector3(length, height, .7f));
            if (t == null) return null;
            Material face = front != null ? front : p.Art.Panel;
            B.B(t, "Body", new Vector3(0f, height * .5f - .02f, -.02f), new Vector3(length, height - .04f, .64f), face);
            B.B(t, "Top", new Vector3(0f, height - .02f, 0f), new Vector3(length + .04f, .04f, .72f), p.Art.Terrazzo);
            B.B(t, "TopEdge", new Vector3(0f, height - .045f, .36f), new Vector3(length + .04f, .02f, .01f), p.Art.DarkSteel);
            B.B(t, "Kick", new Vector3(0f, .05f, .3f), new Vector3(length, .1f, .02f), p.Art.Rubber);
            for (float x = -length * .5f + .6f; x < length * .5f - .3f; x += 1.2f)
                B.B(t, "PanelLine", new Vector3(x, height * .5f, .305f), new Vector3(.015f, height - .3f, .006f), p.Art.DarkWood);
            B.B(t, "Rail", new Vector3(0f, height * .78f, .31f), new Vector3(length - .1f, .025f, .012f), p.Art.Chrome);
            return t;
        }
    }
}
