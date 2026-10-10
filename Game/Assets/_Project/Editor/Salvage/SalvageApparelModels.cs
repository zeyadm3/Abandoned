using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original apparel salvage and interrupted retail fixtures.</summary>
    public static class SalvageApparelModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "textile_bundle": Textile(root, lod); return true;
                case "tag_detacher": Detacher(root, lod); return true;
                case "floor_safe": Safe(root, lod); return true;
                case "laundry_bin": Laundry(root, lod); return true;
                case "mannequin": Mannequin(root, lod); return true;
                case "garment_rack": Rack(root, lod); return true;
                case "cashwrap_counter": CashWrap(root, lod); return true;
                default: return false;
            }
        }

        static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        static void Textile(Transform p, int lod)
        {
            int layers = lod == 2 ? 2 : lod == 1 ? 4 : 6;
            float h = .36f / layers;
            for (int i = 0; i < layers; i++)
                Box(p, "Folded fabric layer", new Vector3((i % 2) * .02f, h * (i + .5f), 0), new Vector3(.70f - i * .012f, h * .96f, .51f), "Fabric", new Vector3(0, (i % 3 - 1) * 2, 0));
            for (int x = -1; x <= 1; x += 2)
            {
                Box(p, "Strap over bundle", new Vector3(x * .21f, .365f, 0), new Vector3(.035f, .012f, .52f), "Plastic");
                Box(p, "Front strap", new Vector3(x * .21f, .18f, .263f), new Vector3(.035f, .36f, .012f), "Plastic");
                Box(p, "Rear strap", new Vector3(x * .21f, .18f, -.263f), new Vector3(.035f, .36f, .012f), "Plastic");
            }
            if (lod == 2) return;
            Box(p, "Loose folded flap", new Vector3(.22f, .326f, .20f), new Vector3(.24f, .022f, .24f), "Fabric", new Vector3(-9, -7, 0));
            Box(p, "Textile grade card", new Vector3(-.10f, .25f, .271f), new Vector3(.19f, .12f, .004f), "Paper");
            if (lod == 0)
                for (int i = 0; i < 5; i++)
                    Box(p, "Creased folded edge", new Vector3(-.01f, .055f + i * .057f, .259f), new Vector3(.62f, .012f, .015f), "Fabric");
        }

        static void Detacher(Transform p, int lod)
        {
            int s = Sides(lod);
            Cylinder(p, "Magnetic detacher base", new Vector3(0, .024f, 0), new Vector3(.12f, .048f, .12f), "Metal", null, s);
            Cylinder(p, "Magnet turret", new Vector3(0, .064f, 0), new Vector3(.075f, .041f, .075f), "Metal", null, s);
            Cylinder(p, "Release well", new Vector3(0, .086f, 0), new Vector3(.04f, .006f, .04f), "Rubber", null, s);
            Box(p, "Counter fixing flange", new Vector3(0, .006f, 0), new Vector3(.16f, .012f, .09f), "Metal");
            if (lod == 2) return;
            Box(p, "Locking cover hinge", new Vector3(.05f, .065f, 0), new Vector3(.03f, .012f, .075f), "Plastic");
            Bar(p, "Tether segment", new Vector3(-.065f, .012f, 0), new Vector3(-.12f, .018f, -.03f), .004f, "Cable", s);
            Bar(p, "Tether loop", new Vector3(-.12f, .018f, -.03f), new Vector3(-.09f, .018f, -.09f), .004f, "Cable", s);
            if (lod == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                    Cylinder(p, "Fixing screw", new Vector3(side * .066f, .014f, 0), new Vector3(.007f, .004f, .007f), "Metal", null, s);
                Box(p, "Detached security tag", new Vector3(.10f, .012f, -.02f), new Vector3(.034f, .023f, .061f), "Plastic", new Vector3(0, -20, 0));
            }
        }

        static void Safe(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Heavy steel safe", new Vector3(0, .36f, 0), new Vector3(.53f, .72f, .49f), "Metal");
            Box(p, "Inset door", new Vector3(0, .37f, .253f), new Vector3(.445f, .61f, .022f), "Paint");
            Cylinder(p, "Combination dial", new Vector3(-.11f, .46f, .277f), new Vector3(.078f, .026f, .078f), "Metal", new Vector3(90, 0, 0), s);
            Bar(p, "Handle shaft", new Vector3(.085f, .37f, .277f), new Vector3(.085f, .37f, .31f), .035f, "Metal", s);
            Bar(p, "Handle spoke", new Vector3(.035f, .37f, .31f), new Vector3(.135f, .37f, .31f), .021f, "Metal", s);
            if (lod == 2) return;
            for (int i = 0; i < 2; i++)
                Cylinder(p, "Door hinge", new Vector3(.231f, .19f + i * .37f, .255f), new Vector3(.045f, .09f, .045f), "Metal", null, s);
            Box(p, "Asset recovery sticker", new Vector3(-.035f, .61f, .27f), new Vector3(.19f, .074f, .003f), "Paper");
            if (lod == 0)
            {
                Box(p, "Floor mounting foot", new Vector3(0, .028f, -.255f), new Vector3(.60f, .055f, .06f), "Metal");
                Cylinder(p, "Key override", new Vector3(.092f, .53f, .27f), new Vector3(.024f, .006f, .024f), "Metal", new Vector3(90, 0, 0), s);
                for (int side = -1; side <= 1; side += 2)
                    Cylinder(p, "Anchoring bolt", new Vector3(side * .265f, .063f, -.255f), new Vector3(.023f, .014f, .023f), "Metal", null, s);
            }
        }

        static void Laundry(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Laundry cart bottom", new Vector3(0, .17f, 0), new Vector3(.78f, .08f, .58f), "Plastic");
            Box(p, "Front bin wall", new Vector3(0, .55f, .277f), new Vector3(.78f, .72f, .026f), "Plastic");
            Box(p, "Back bin wall", new Vector3(0, .55f, -.277f), new Vector3(.78f, .72f, .026f), "Plastic");
            Box(p, "Left bin wall", new Vector3(-.377f, .55f, 0), new Vector3(.026f, .72f, .53f), "Plastic");
            Box(p, "Right bin wall", new Vector3(.377f, .55f, 0), new Vector3(.026f, .72f, .53f), "Plastic");
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Cylinder(p, "Cart caster", new Vector3(x * .29f, .065f, z * .20f), new Vector3(.13f, .04f, .13f), "Rubber", new Vector3(0, 0, 90), s);
            if (lod == 2) return;
            for (int z = -1; z <= 1; z += 2)
                Box(p, "Reinforced rim", new Vector3(0, .923f, z * .283f), new Vector3(.81f, .034f, .045f), "Metal");
            for (int x = -1; x <= 1; x += 2)
            {
                Box(p, "Reinforced side rim", new Vector3(x * .383f, .923f, 0), new Vector3(.045f, .034f, .57f), "Metal");
                Bar(p, "Push grip", new Vector3(x * .415f, .84f, -.10f), new Vector3(x * .415f, .84f, .10f), .026f, "Metal", s);
            }
            Box(p, "Laundry folded load", new Vector3(.03f, .54f, 0), new Vector3(.65f, .54f, .46f), "Fabric", new Vector3(0, 4, 0));
            if (lod == 0)
            {
                Box(p, "Inventory card pocket", new Vector3(0, .71f, .297f), new Vector3(.19f, .13f, .009f), "Paper");
                for (int i = 0; i < 4; i++)
                    Box(p, "Moulded reinforcing rib", new Vector3(-.29f + i * .19f, .55f, .296f), new Vector3(.013f, .60f, .012f), "Plastic");
            }
        }

        static void Mannequin(Transform p, int lod)
        {
            int s = Sides(lod);
            Cylinder(p, "Display base", new Vector3(0, .018f, 0), new Vector3(.43f, .036f, .43f), "Metal", null, s);
            Bar(p, "Support rod", new Vector3(.10f, .035f, -.04f), new Vector3(.10f, .49f, -.04f), .023f, "Metal", s);
            Sphere(p, "Pelvis", new Vector3(0, .96f, 0), new Vector3(.27f, .26f, .18f), "Plastic", null, s);
            Sphere(p, "Torso", new Vector3(0, 1.28f, 0), new Vector3(.35f, .44f, .21f), "Plastic", null, s);
            Cylinder(p, "Neck", new Vector3(0, 1.53f, 0), new Vector3(.095f, .10f, .095f), "Plastic", null, s);
            Sphere(p, "Featureless head", new Vector3(0, 1.71f, 0), new Vector3(.18f, .25f, .20f), "Plastic", null, s);
            for (int side = -1; side <= 1; side += 2)
            {
                Bar(p, "Upper leg", new Vector3(side * .075f, .95f, 0), new Vector3(side * .09f, .53f, .025f), .11f, "Plastic", s);
                Bar(p, "Lower leg", new Vector3(side * .09f, .53f, .025f), new Vector3(side * .11f, .12f, 0), .07f, "Plastic", s);
                Box(p, "Foot", new Vector3(side * .11f, .085f, .045f), new Vector3(.084f, .07f, .22f), "Plastic");
                Bar(p, "Upper arm", new Vector3(side * .19f, 1.43f, 0), new Vector3(side * .24f, 1.16f, .015f), .075f, "Plastic", s);
                Bar(p, "Forearm", new Vector3(side * .24f, 1.16f, .015f), new Vector3(side * .27f, .91f, .05f), .05f, "Plastic", s);
                Sphere(p, "Hand", new Vector3(side * .27f, .86f, .05f), new Vector3(.054f, .11f, .045f), "Plastic", null, s);
            }
            if (lod == 0)
            {
                for (int side = -1; side <= 1; side += 2)
                    Cylinder(p, "Shoulder attachment seam", new Vector3(side * .188f, 1.43f, 0), new Vector3(.088f, .012f, .088f), "Metal", new Vector3(0, 0, 90), s);
                Box(p, "Recovery tag", new Vector3(0, 1.26f, .112f), new Vector3(.10f, .09f, .002f), "Paper");
            }
        }

        static void Rack(Transform p, int lod)
        {
            int s = Sides(lod);
            for (int side = -1; side <= 1; side += 2)
            {
                Bar(p, "Rack upright", new Vector3(side * .62f, .13f, 0), new Vector3(side * .62f, 1.63f, 0), .035f, "Metal", s);
                Bar(p, "Rack foot", new Vector3(side * .62f, .13f, -.31f), new Vector3(side * .62f, .13f, .31f), .045f, "Metal", s);
                for (int z = -1; z <= 1; z += 2)
                    Cylinder(p, "Rack caster", new Vector3(side * .62f, .047f, z * .27f), new Vector3(.094f, .032f, .094f), "Rubber", new Vector3(0, 0, 90), s);
            }
            Bar(p, "Clothes rail", new Vector3(-.66f, 1.63f, 0), new Vector3(.66f, 1.63f, 0), .032f, "Metal", s);
            Bar(p, "Base cross brace", new Vector3(-.62f, .13f, 0), new Vector3(.62f, .13f, 0), .025f, "Metal", s);
            if (lod == 2) return;
            int count = lod == 0 ? 4 : 2;
            for (int i = 0; i < count; i++)
            {
                float x = -.28f + i * .12f;
                Bar(p, "Hanger hook", new Vector3(x, 1.61f, 0), new Vector3(x, 1.53f, .012f), .008f, "Metal", s);
                Bar(p, "Hanger left shoulder", new Vector3(x, 1.53f, .012f), new Vector3(x - .18f, 1.42f, .012f), .009f, "Plastic", s);
                Bar(p, "Hanger right shoulder", new Vector3(x, 1.53f, .012f), new Vector3(x + .18f, 1.42f, .012f), .009f, "Plastic", s);
                Bar(p, "Hanger bottom", new Vector3(x - .18f, 1.42f, .012f), new Vector3(x + .18f, 1.42f, .012f), .009f, "Plastic", s);
            }
        }

        static void CashWrap(Transform p, int lod)
        {
            Box(p, "Retail wrap front", new Vector3(0, .53f, .36f), new Vector3(1.72f, 1.06f, .075f), "Wood");
            Box(p, "Left end panel", new Vector3(-.81f, .53f, 0), new Vector3(.10f, 1.06f, .71f), "Wood");
            Box(p, "Right end panel", new Vector3(.81f, .53f, 0), new Vector3(.10f, 1.06f, .71f), "Wood");
            Box(p, "Sales counter top", new Vector3(0, 1.08f, 0), new Vector3(1.80f, .055f, .82f), "Plastic");
            Box(p, "Return wing", new Vector3(.58f, .87f, -.55f), new Vector3(.62f, .045f, .39f), "Wood");
            Box(p, "Return wing end", new Vector3(.81f, .43f, -.54f), new Vector3(.10f, .86f, .39f), "Wood");
            Box(p, "Cash drawer", new Vector3(-.39f, .85f, -.015f), new Vector3(.65f, .13f, .53f), "Metal");
            if (lod == 2) return;
            Box(p, "Wrapping storage shelf", new Vector3(.15f, .36f, .02f), new Vector3(1.25f, .035f, .55f), "Wood");
            Box(p, "Drawer face", new Vector3(-.39f, .85f, -.30f), new Vector3(.61f, .115f, .025f), "Metal");
            Box(p, "Drawer pull", new Vector3(-.39f, .85f, -.32f), new Vector3(.19f, .018f, .022f), "Plastic");
            if (lod == 0)
            {
                Box(p, "Loose receipt pad", new Vector3(-.30f, 1.114f, -.03f), new Vector3(.19f, .009f, .28f), "Paper", new Vector3(0, -17, 0));
                Box(p, "Broken till cable socket", new Vector3(.32f, 1.114f, -.05f), new Vector3(.07f, .014f, .09f), "Rubber");
                Box(p, "Removal patch", new Vector3(0, .7f, .402f), new Vector3(.37f, .23f, .007f), "Paint");
            }
        }
    }
}
