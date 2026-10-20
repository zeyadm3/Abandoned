using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The house's fittings and furniture, built from boxes: beds, dressers, the sofa, the dining set, the kitchen
    /// run, bathroom fixtures, packing boxes. Each piece is one group with a single solid box fitted to it (as the
    /// solid-objects pass makes every reachable thing solid), its own parts walk-through. Nothing here looks like a
    /// piece of loot: anything that would (a lamp, a TV, the fridge) is real salvage placed by the gameplay pass.
    /// Positions are the piece's floor centre; yaw turns its front (local +Z) toward the room.
    /// </summary>
    public static class TutorialHouseFurniture
    {
        public static Transform Piece(Transform parent, string name, Vector3 floor, float yaw)
        {
            Transform t = Group(name, parent);
            t.SetPositionAndRotation(floor, Quaternion.Euler(0f, yaw, 0f));
            return t;
        }

        /// <summary>One solid box fitted to the piece (local size, from the floor up).</summary>
        public static void Solid(Transform piece, Vector3 size, SurfaceMaterial surface = SurfaceMaterial.Wood, float y0 = 0f)
        {
            var box = piece.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, y0 + size.y * .5f, 0f);
            box.size = size;
            piece.gameObject.AddComponent<SurfaceTag>().EditorSet(surface);
        }

        private static GameObject Part(Transform piece, string name, Vector3 centre, Vector3 size, Material m) => Box(name, piece, centre, size, m, false);

        public static Transform Bed(Transform parent, string name, Vector3 floor, float yaw, float width, float length, Material bedding, TutorialHouseArt art)
        {
            Transform bed = Piece(parent, name, floor, yaw);
            float hz = length * .5f;
            Part(bed, "Frame", new Vector3(0f, .2f, 0f), new Vector3(width, .22f, length), art.OakDark);
            Part(bed, "Mattress", new Vector3(0f, .42f, .03f), new Vector3(width - .06f, .22f, length - .1f), art.Mattress);
            Part(bed, "Duvet", new Vector3(0f, .55f, .25f), new Vector3(width - .02f, .07f, length - .65f), bedding).transform.localRotation = Quaternion.Euler(1.5f, 0f, 0f);
            Part(bed, "DuvetFold", new Vector3(0f, .6f, -hz + .62f), new Vector3(width - .02f, .08f, .22f), bedding);
            int pillows = width > 1.2f ? 2 : 1;
            for (int i = 0; i < pillows; i++)
                Part(bed, "Pillow", new Vector3(pillows == 1 ? 0f : (i - .5f) * width * .48f, .58f, -hz + .3f), new Vector3(width / pillows - .12f, .1f, .38f), art.Pillow);
            Part(bed, "Headboard", new Vector3(0f, .55f, -hz - .03f), new Vector3(width + .06f, 1.1f, .06f), art.OakDark);
            Part(bed, "Footboard", new Vector3(0f, .32f, hz + .03f), new Vector3(width + .06f, .45f, .05f), art.OakDark);
            Solid(bed, new Vector3(width + .06f, .62f, length + .1f), SurfaceMaterial.Fabric);
            return bed;
        }

        public static Transform Nightstand(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform n = Piece(parent, name, floor, yaw);
            Part(n, "Carcass", new Vector3(0f, .27f, 0f), new Vector3(.46f, .54f, .4f), art.Oak);
            Part(n, "Drawer", new Vector3(0f, .4f, .205f), new Vector3(.4f, .14f, .02f), art.OakDark);
            Part(n, "Knob", new Vector3(0f, .4f, .225f), new Vector3(.04f, .04f, .03f), art.Brass);
            Solid(n, new Vector3(.46f, .55f, .4f));
            return n;
        }

        public static Transform Dresser(Transform parent, string name, Vector3 floor, float yaw, float width, TutorialHouseArt art)
        {
            Transform d = Piece(parent, name, floor, yaw);
            Part(d, "Carcass", new Vector3(0f, .42f, 0f), new Vector3(width, .84f, .5f), art.Oak);
            for (int row = 0; row < 3; row++)
            {
                Part(d, "Drawer", new Vector3(0f, .17f + row * .25f, .255f), new Vector3(width - .08f, .21f, .02f), art.OakDark);
                foreach (float x in new[] { -width * .28f, width * .28f })
                    Part(d, "Pull", new Vector3(x, .17f + row * .25f, .272f), new Vector3(.1f, .025f, .025f), art.Brass);
            }
            Solid(d, new Vector3(width, .85f, .5f));
            return d;
        }

        public static Transform Desk(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform d = Piece(parent, name, floor, yaw);
            Part(d, "Top", new Vector3(0f, .74f, 0f), new Vector3(1.1f, .04f, .55f), art.Oak);
            Part(d, "Pedestal", new Vector3(.38f, .36f, 0f), new Vector3(.34f, .72f, .52f), art.Oak);
            Part(d, "Leg", new Vector3(-.52f, .36f, 0f), new Vector3(.04f, .72f, .5f), art.Oak);
            Transform chair = Group("DeskChair", d);
            chair.localPosition = new Vector3(-.1f, 0f, .55f);
            chair.localRotation = Quaternion.Euler(0f, 160f, 0f);
            Part(chair, "Seat", new Vector3(0f, .45f, 0f), new Vector3(.42f, .05f, .42f), art.OakDark);
            Part(chair, "Back", new Vector3(0f, .72f, -.19f), new Vector3(.42f, .5f, .04f), art.OakDark);
            foreach (float x in new[] { -.18f, .18f })
            foreach (float z in new[] { -.18f, .18f })
                Part(chair, "Leg", new Vector3(x, .22f, z), new Vector3(.035f, .45f, .035f), art.OakDark);
            var seat = chair.gameObject.AddComponent<BoxCollider>();
            seat.center = new Vector3(0f, .25f, 0f);
            seat.size = new Vector3(.44f, .5f, .44f);
            Solid(d, new Vector3(1.1f, .76f, .55f));
            return d;
        }

        public static Transform Sofa(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform s = Piece(parent, name, floor, yaw);
            Part(s, "Base", new Vector3(0f, .22f, 0f), new Vector3(2.1f, .3f, .9f), art.SofaFabric);
            for (int i = 0; i < 3; i++)
                Part(s, "SeatCushion", new Vector3(-.68f + i * .68f, .44f, .06f), new Vector3(.66f, .14f, .72f), art.SofaFabric);
            Part(s, "Back", new Vector3(0f, .62f, -.36f), new Vector3(2.1f, .55f, .2f), art.SofaFabric);
            foreach (float x in new[] { -1f, 1f })
                Part(s, "Arm", new Vector3(x * .98f, .45f, 0f), new Vector3(.16f, .4f, .9f), art.SofaFabric);
            Part(s, "Throw", new Vector3(.75f, .64f, -.28f), new Vector3(.5f, .5f, .03f), art.Curtain).transform.localRotation = Quaternion.Euler(-12f, 0f, 0f);
            Solid(s, new Vector3(2.1f, .85f, .9f), SurfaceMaterial.Fabric);
            return s;
        }

        public static Transform Armchair(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform a = Piece(parent, name, floor, yaw);
            Part(a, "Base", new Vector3(0f, .22f, 0f), new Vector3(.82f, .3f, .82f), art.SofaFabric);
            Part(a, "Cushion", new Vector3(0f, .43f, .05f), new Vector3(.56f, .12f, .64f), art.SofaFabric);
            Part(a, "Back", new Vector3(0f, .65f, -.33f), new Vector3(.82f, .6f, .16f), art.SofaFabric);
            foreach (float x in new[] { -1f, 1f })
                Part(a, "Arm", new Vector3(x * .34f, .48f, 0f), new Vector3(.14f, .28f, .82f), art.SofaFabric);
            Solid(a, new Vector3(.82f, .9f, .82f), SurfaceMaterial.Fabric);
            return a;
        }

        public static Transform Table(Transform parent, string name, Vector3 floor, float yaw, Vector3 size, Material top, Material legs)
        {
            Transform t = Piece(parent, name, floor, yaw);
            Part(t, "Top", new Vector3(0f, size.y - .02f, 0f), new Vector3(size.x, .04f, size.z), top);
            foreach (float x in new[] { -1f, 1f })
            foreach (float z in new[] { -1f, 1f })
                Part(t, "Leg", new Vector3(x * (size.x * .5f - .06f), (size.y - .04f) * .5f, z * (size.z * .5f - .06f)), new Vector3(.05f, size.y - .04f, .05f), legs);
            Solid(t, size);
            return t;
        }

        public static void DiningSet(Transform parent, Vector3 floor, TutorialHouseArt art)
        {
            Transform set = Group("DiningSet", parent);
            Table(set, "DiningTable", floor, 0f, new Vector3(.95f, .76f, 1.7f), art.Oak, art.OakDark);
            foreach (float z in new[] { -.45f, .45f })
            foreach (float side in new[] { -1f, 1f })
                Chair(set, floor + new Vector3(side * .72f, 0f, z), side > 0 ? -90f : 90f, art, pushedBack: z > 0 && side > 0);
            Chair(set, floor + new Vector3(0f, 0f, -1.08f), 0f, art, pushedBack: false);
        }

        public static void Chair(Transform parent, Vector3 floor, float yaw, TutorialHouseArt art, bool pushedBack)
        {
            Transform c = Piece(parent, "DiningChair", floor, yaw + (pushedBack ? 25f : 0f));
            if (pushedBack) c.position += c.rotation * Vector3.back * .25f;
            Part(c, "Seat", new Vector3(0f, .45f, 0f), new Vector3(.44f, .05f, .44f), art.Oak);
            Part(c, "Back", new Vector3(0f, .74f, -.2f), new Vector3(.44f, .55f, .04f), art.OakDark);
            foreach (float x in new[] { -.19f, .19f })
            foreach (float z in new[] { -.19f, .19f })
                Part(c, "Leg", new Vector3(x, .22f, z), new Vector3(.04f, .45f, .04f), art.Oak);
            Solid(c, new Vector3(.44f, .5f, .44f));
        }

        /// <summary>Base cabinets with a laminate top, a run of uppers above. <paramref name="gapAt"/> leaves a slot for the loot stove.</summary>
        public static Transform Counter(Transform parent, string name, Vector3 floor, float yaw, float length, TutorialHouseArt art, bool uppers = true, bool sink = false)
        {
            Transform c = Piece(parent, name, floor, yaw);
            Part(c, "Toekick", new Vector3(0f, .05f, .02f), new Vector3(length, .1f, .52f), art.DarkMetal);
            Part(c, "BaseCabinets", new Vector3(0f, .48f, 0f), new Vector3(length, .76f, .58f), art.Cabinet);
            Part(c, "Countertop", new Vector3(0f, .9f, .02f), new Vector3(length + .02f, .04f, .64f), art.Laminate);
            Part(c, "Backsplash", new Vector3(0f, 1.05f, -.3f), new Vector3(length, .26f, .02f), art.Tile);
            for (float x = -length * .5f + .3f; x < length * .5f - .1f; x += .6f)
            {
                Part(c, "DoorLine", new Vector3(x + .3f, .48f, .292f), new Vector3(.01f, .66f, .01f), art.OakDark);
                Part(c, "Pull", new Vector3(x + .2f, .7f, .3f), new Vector3(.03f, .1f, .025f), art.Chrome);
            }
            if (uppers)
            {
                Part(c, "UpperCabinets", new Vector3(0f, 1.85f, -.14f), new Vector3(length, .72f, .34f), art.Cabinet);
                for (float x = -length * .5f + .3f; x < length * .5f - .1f; x += .6f)
                    Part(c, "UpperDoorLine", new Vector3(x + .3f, 1.85f, .035f), new Vector3(.01f, .66f, .01f), art.OakDark);
            }
            if (sink)
            {
                Part(c, "SinkBasin", new Vector3(0f, .88f, .02f), new Vector3(.72f, .06f, .42f), art.Chrome);
                Part(c, "Faucet", new Vector3(0f, 1.0f, -.2f), new Vector3(.04f, .22f, .04f), art.Chrome);
                Part(c, "Spout", new Vector3(0f, 1.1f, -.12f), new Vector3(.04f, .03f, .16f), art.Chrome);
            }
            Solid(c, new Vector3(length, .92f, .62f));
            return c;
        }

        public static Transform Toilet(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform t = Piece(parent, name, floor, yaw);
            Part(t, "Bowl", new Vector3(0f, .2f, .08f), new Vector3(.38f, .4f, .5f), art.Porcelain);
            Part(t, "Seat", new Vector3(0f, .42f, .1f), new Vector3(.38f, .03f, .44f), art.Porcelain);
            Part(t, "Tank", new Vector3(0f, .6f, -.2f), new Vector3(.44f, .4f, .2f), art.Porcelain);
            Solid(t, new Vector3(.44f, .8f, .68f), SurfaceMaterial.Tile);
            return t;
        }

        public static Transform Vanity(Transform parent, string name, Vector3 floor, float yaw, float width, TutorialHouseArt art)
        {
            Transform v = Piece(parent, name, floor, yaw);
            Part(v, "Cabinet", new Vector3(0f, .4f, 0f), new Vector3(width, .8f, .5f), art.Cabinet);
            Part(v, "Top", new Vector3(0f, .83f, .01f), new Vector3(width + .02f, .05f, .54f), art.Laminate);
            Part(v, "Basin", new Vector3(0f, .82f, .03f), new Vector3(.45f, .05f, .34f), art.Porcelain);
            Part(v, "Tap", new Vector3(0f, .95f, -.17f), new Vector3(.04f, .18f, .04f), art.Chrome);
            Part(v, "Mirror", new Vector3(0f, 1.55f, -.25f), new Vector3(width * .9f, .8f, .02f), art.Mirror);
            Part(v, "LightBar", new Vector3(0f, 2.02f, -.22f), new Vector3(width * .7f, .08f, .08f), art.Chrome);
            Solid(v, new Vector3(width + .02f, .86f, .54f), SurfaceMaterial.Tile);
            return v;
        }

        /// <summary>A built-in tub in its alcove (a fixture, not the clawfoot salvage).</summary>
        public static Transform BuiltInTub(Transform parent, string name, Vector3 floor, float yaw, float length, TutorialHouseArt art)
        {
            Transform t = Piece(parent, name, floor, yaw);
            Part(t, "Apron", new Vector3(0f, .27f, .33f), new Vector3(length, .54f, .06f), art.Porcelain);
            Part(t, "Rim", new Vector3(0f, .53f, 0f), new Vector3(length, .04f, .72f), art.Porcelain);
            Part(t, "Basin", new Vector3(0f, .3f, 0f), new Vector3(length - .14f, .42f, .58f), art.Tile);
            Part(t, "Spout", new Vector3(-length * .5f + .1f, .7f, -.3f), new Vector3(.05f, .05f, .14f), art.Chrome);
            Part(t, "CurtainRod", new Vector3(0f, 2f, .34f), new Vector3(length, .025f, .025f), art.Chrome);
            Part(t, "ShowerCurtain", new Vector3(-length * .28f, 1.4f, .34f), new Vector3(length * .42f, 1.2f, .02f), art.Blind);
            Solid(t, new Vector3(length, .56f, .74f), SurfaceMaterial.Tile);
            return t;
        }

        public static Transform ShowerStall(Transform parent, string name, Vector3 floor, float yaw, TutorialHouseArt art)
        {
            Transform s = Piece(parent, name, floor, yaw);
            Part(s, "Pan", new Vector3(0f, .05f, 0f), new Vector3(.9f, .1f, .9f), art.Porcelain);
            Part(s, "GlassFront", new Vector3(0f, 1f, .44f), new Vector3(.9f, 1.9f, .02f), art.Glass);
            Part(s, "Head", new Vector3(0f, 1.95f, -.4f), new Vector3(.12f, .04f, .12f), art.Chrome);
            Solid(s, new Vector3(.9f, 2f, .9f), SurfaceMaterial.Tile);
            return s;
        }

        /// <summary>A stack of packing boxes, half taped: the eviction got as far as the living room.</summary>
        public static Transform Boxes(Transform parent, string name, Vector3 floor, float yaw, int seed, TutorialHouseArt art)
        {
            Transform b = Piece(parent, name, floor, yaw);
            var random = new System.Random(seed);
            float y = 0f;
            int count = 2 + random.Next(2);
            for (int i = 0; i < count; i++)
            {
                float w = .45f + (float)random.NextDouble() * .2f, h = .3f + (float)random.NextDouble() * .15f;
                GameObject box = Part(b, "Carton", new Vector3((float)random.NextDouble() * .06f, y + h * .5f, 0f), new Vector3(w, h, w * .8f), art.Cardboard);
                box.transform.localRotation = Quaternion.Euler(0f, (float)random.NextDouble() * 14f - 7f, 0f);
                Part(b, "Tape", new Vector3(0f, y + h + .002f, 0f), new Vector3(.06f, .004f, w * .8f), art.Tape);
                y += h;
            }
            Solid(b, new Vector3(.6f, y, .55f), SurfaceMaterial.Paper);
            return b;
        }

        public static Transform Shelf(Transform parent, string name, Vector3 floor, float yaw, float width, float height, int shelves, Material material, TutorialHouseArt art)
        {
            Transform s = Piece(parent, name, floor, yaw);
            foreach (float x in new[] { -width * .5f + .02f, width * .5f - .02f })
                Part(s, "Upright", new Vector3(x, height * .5f, 0f), new Vector3(.04f, height, .4f), material);
            for (int i = 0; i < shelves; i++)
                Part(s, "Board", new Vector3(0f, .1f + i * (height - .15f) / Mathf.Max(1, shelves - 1), 0f), new Vector3(width, .03f, .4f), material);
            Solid(s, new Vector3(width, height, .4f), SurfaceMaterial.Metal);
            return s;
        }

        /// <summary>The Christmas tree nobody took down: three stacked cones, baubles, a star, presents never opened.</summary>
        public static Transform ChristmasTree(Transform parent, Vector3 floor, TutorialHouseArt art)
        {
            Transform t = Piece(parent, "ChristmasTree", floor, 0f);
            Mesh cone = TutorialHouseMeshes.Cone();
            Primitive(PrimitiveType.Cylinder, "Stand", t, new Vector3(0f, .1f, 0f), new Vector3(.4f, .1f, .4f), art.OrnamentRed, false);
            Primitive(PrimitiveType.Cylinder, "Trunk", t, new Vector3(0f, .3f, 0f), new Vector3(.08f, .15f, .08f), art.Bark, false);
            float[] y = { .35f, .85f, 1.3f };
            float[] r = { 1.15f, .9f, .6f };
            for (int i = 0; i < 3; i++)
                TutorialHouseMeshes.Shape("Boughs", t, cone, new Vector3(0f, y[i], 0f), new Vector3(r[i], .9f, r[i]), art.TreeGreen, false);
            var random = new System.Random(1225);
            for (int i = 0; i < 22; i++)
            {
                float h = .5f + (float)random.NextDouble() * 1.3f, radius = Mathf.Lerp(.5f, .12f, (h - .45f) / 1.4f);
                float a = (float)random.NextDouble() * Mathf.PI * 2f;
                Primitive(PrimitiveType.Sphere, "Bauble", t, new Vector3(Mathf.Cos(a) * radius, h, Mathf.Sin(a) * radius), Vector3.one * .07f, i % 3 == 0 ? art.OrnamentGold : art.OrnamentRed, false);
            }
            Box("Star", t, new Vector3(0f, 2.25f, 0f), new Vector3(.14f, .14f, .03f), art.OrnamentGold, false).transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            for (int i = 0; i < 4; i++)
                Box("Present", t, new Vector3(Mathf.Cos(i * 1.7f) * .55f, .1f, Mathf.Sin(i * 1.7f) * .55f), new Vector3(.3f, .2f, .25f), i % 2 == 0 ? art.OrnamentRed : art.Curtain, false)
                    .transform.localRotation = Quaternion.Euler(0f, i * 23f, 0f);
            var box = t.gameObject.AddComponent<BoxCollider>();
            box.center = new Vector3(0f, 1.1f, 0f);
            box.size = new Vector3(1.1f, 2.2f, 1.1f);
            t.gameObject.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Wood);
            return t;
        }

        public static void Rug(Transform parent, string name, Vector3 floor, Vector2 size, float yaw, Material material)
        {
            Box(name, parent, floor + Vector3.up * .006f, new Vector3(size.x, .012f, size.y), material, false).transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }
    }
}
