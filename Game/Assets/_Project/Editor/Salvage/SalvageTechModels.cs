using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original clearance-stock silhouettes, authored in metres with a floor-level pivot.</summary>
    internal static class SalvageTechModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "pc_tower": Tower(root, lod); return true;
                case "motherboard": Board(root, lod, true); return true;
                case "crt_monitor": Crt(root, lod); return true;
                case "led_monitor": Led(root, lod); return true;
                case "circuit_board": Board(root, lod, false); return true;
                case "server_rack": Rack(root, lod); return true;
                case "copper_spool": Spool(root, lod); return true;
                case "industrial_battery": Battery(root, lod); return true;
                default: return false;
            }
        }

        private static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        private static void Tower(Transform p, int lod)
        {
            Box(p, "steel chassis", new Vector3(0, .255f, 0), new Vector3(.224f, .43f, .43f), "Metal");
            Box(p, "front plastic fascia", new Vector3(0, .255f, -.221f), new Vector3(.24f, .44f, .018f), "Plastic");
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "rubber foot", new Vector3(x * .087f, .02f, z * .16f), new Vector3(.038f, .04f, .05f), "Rubber");
            Box(p, "empty optical drive bay", new Vector3(0, .399f, -.234f), new Vector3(.175f, .035f, .008f), "Rubber");
            Box(p, "drive tray lip", new Vector3(0, .364f, -.235f), new Vector3(.175f, .018f, .008f), "Metal");
            Box(p, "removed side access window", new Vector3(.114f, .25f, .04f), new Vector3(.008f, .285f, .285f), "Rubber");
            if (lod == 2) return;
            Cylinder(p, "power button", new Vector3(.067f, .315f, -.236f), new Vector3(.015f, .006f, .015f), "Metal", new Vector3(90, 0, 0), Sides(lod));
            for (int i = 0; i < (lod == 0 ? 9 : 4); i++)
                Box(p, "front ventilation slit", new Vector3(0, .105f + i * .014f, -.233f), new Vector3(.168f, .003f, .006f), "Rubber");
            Box(p, "salvage sticker", new Vector3(-.055f, .278f, -.233f), new Vector3(.045f, .027f, .003f), "Paper", new Vector3(0, 0, -8));
            Box(p, "rear power supply", new Vector3(0, .135f, .218f), new Vector3(.14f, .105f, .006f), "Plastic");
            for (int i = 0; i < 3; i++)
                Box(p, "rear expansion blank", new Vector3(.01f, .24f + i * .033f, .219f), new Vector3(.153f, .017f, .007f), "Metal");
            if (lod != 0) return;
            for (int i = 0; i < 2; i++)
                Box(p, "USB socket", new Vector3(-.025f + i * .039f, .315f, -.238f), new Vector3(.024f, .01f, .007f), "Rubber");
            Box(p, "side service circuit", new Vector3(.12f, .27f, .04f), new Vector3(.006f, .22f, .22f), "Circuit");
            Box(p, "side CPU heatsink", new Vector3(.125f, .28f, .055f), new Vector3(.015f, .058f, .065f), "Metal");
            for (int i = 0; i < 5; i++)
                Box(p, "side heatsink fin", new Vector3(.137f, .254f + i * .012f, .055f), new Vector3(.014f, .003f, .065f), "Metal");
            Bar(p, "severed mains cord", new Vector3(-.06f, .17f, .216f), new Vector3(-.09f, .03f, .222f), .008f, "Cable");
        }

        private static void Board(Transform p, int lod, bool motherboard)
        {
            float w = motherboard ? .305f : .18f;
            float d = motherboard ? .244f : .12f;
            Box(p, "fibreglass PCB", new Vector3(0, .003f, 0), new Vector3(w, .006f, d), "Circuit");
            Box(p, "processor socket", new Vector3(-w * .16f, .012f, -d * .1f), new Vector3(w * .24f, .018f, d * .3f), "Plastic");
            Box(p, "silicon package", new Vector3(-w * .16f, .022f, -d * .1f), new Vector3(w * .18f, .003f, d * .22f), "Metal");
            int slots = motherboard ? 4 : 2;
            if (lod < 2)
                for (int i = 0; i < slots; i++)
                    Box(p, "memory edge connector", new Vector3(w * .19f + i * .013f, .012f, -.018f), new Vector3(.007f, .016f, d * .7f), "Plastic");
            Box(p, "I/O shield block", new Vector3(-w * .435f, .014f, .01f), new Vector3(w * .13f, .022f, d * .55f), "Metal");
            if (lod == 2) return;
            for (int i = 0; i < (motherboard ? 3 : 1); i++)
                Box(p, "PCI socket", new Vector3(-.02f, .011f, d * .2f + i * .021f), new Vector3(w * .48f, .012f, .009f), "Plastic");
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? -1 : 1) * w * .45f;
                float z = (i < 2 ? -1 : 1) * d * .43f;
                Cylinder(p, "mounting eyelet", new Vector3(x, .007f, z), new Vector3(.009f, .004f, .009f), "Copper", null, Sides(lod));
            }
            if (lod != 0) return;
            for (int i = 0; i < 5; i++)
            {
                Cylinder(p, "capacitor", new Vector3(-w * .29f, .014f, -d * .37f + i * d * .1f), new Vector3(.011f, .016f, .011f), "Metal", null, 8);
                Box(p, "SMD chip", new Vector3(-w * .02f + i * .017f, .008f, -d * .38f), new Vector3(.009f, .004f, .007f), "Plastic");
                Box(p, "copper trace", new Vector3(-w * .07f, .0065f, -d * .34f + i * .006f), new Vector3(w * .34f, .001f, .0015f), "Copper");
            }
            Box(p, "QA barcode sticker", new Vector3(w * .29f, .007f, d * .34f), new Vector3(.037f, .001f, .022f), "Paper");
        }

        private static void Crt(Transform p, int lod)
        {
            Box(p, "monitor pedestal", new Vector3(0, .022f, .045f), new Vector3(.28f, .044f, .28f), "Plastic");
            Box(p, "neck", new Vector3(0, .066f, .02f), new Vector3(.16f, .044f, .16f), "Plastic");
            Box(p, "deep CRT enclosure", new Vector3(0, .244f, .025f), new Vector3(.40f, .312f, .36f), "Plastic");
            Box(p, "front bevel rim", new Vector3(0, .244f, -.193f), new Vector3(.43f, .31f, .06f), "Paint");
            Box(p, "recessed black screen", new Vector3(0, .258f, -.225f), new Vector3(.353f, .24f, .004f), "Glass");
            if (lod == 2) return;
            Box(p, "lower bezel", new Vector3(0, .105f, -.229f), new Vector3(.43f, .037f, .006f), "Paint");
            for (int i = 0; i < (lod == 0 ? 5 : 2); i++)
                Box(p, "side cooling slit", new Vector3(.201f, .285f + i * .015f, .05f), new Vector3(.002f, .005f, .22f), "Rubber");
            Cylinder(p, "power button", new Vector3(.172f, .105f, -.235f), new Vector3(.013f, .006f, .013f), "Metal", new Vector3(90, 0, 0), Sides(lod));
            if (lod != 0) return;
            for (int i = 0; i < 3; i++)
                Cylinder(p, "adjustment key", new Vector3(.08f + i * .022f, .105f, -.233f), new Vector3(.008f, .004f, .008f), "Plastic", new Vector3(90, 0, 0), 6);
            Box(p, "rear input recess", new Vector3(0, .13f, .207f), new Vector3(.13f, .045f, .007f), "Rubber");
            Bar(p, "cut VGA lead", new Vector3(.015f, .13f, .21f), new Vector3(.11f, .07f, .195f), .009f, "Cable");
        }

        private static void Led(Transform p, int lod)
        {
            Box(p, "weighted stand foot", new Vector3(0, .015f, .02f), new Vector3(.24f, .03f, .20f), "Metal");
            Box(p, "stand stem", new Vector3(0, .105f, .043f), new Vector3(.048f, .16f, .035f), "Metal", new Vector3(-10, 0, 0));
            Box(p, "LCD enclosure", new Vector3(0, .275f, 0), new Vector3(.54f, .31f, .032f), "Plastic");
            Box(p, "dead LCD panel", new Vector3(0, .283f, -.018f), new Vector3(.512f, .276f, .003f), "Glass");
            if (lod == 2) return;
            Box(p, "lower brand bezel", new Vector3(0, .126f, -.019f), new Vector3(.54f, .019f, .006f), "Metal");
            Box(p, "rear VESA housing", new Vector3(0, .247f, .026f), new Vector3(.13f, .11f, .025f), "Plastic");
            if (lod != 0) return;
            for (int i = 0; i < 4; i++)
                Cylinder(p, "VESA screw", new Vector3(i % 2 == 0 ? -.047f : .047f, i < 2 ? .21f : .284f, .04f), new Vector3(.005f, .005f, .005f), "Metal", new Vector3(90, 0, 0), 6);
            Box(p, "connection socket", new Vector3(.075f, .192f, .020f), new Vector3(.04f, .012f, .008f), "Rubber");
            Box(p, "classification label", new Vector3(.16f, .125f, -.023f), new Vector3(.045f, .013f, .002f), "Paper");
        }

        private static void Rack(Transform p, int lod)
        {
            Box(p, "rack plinth", new Vector3(0, .052f, 0), new Vector3(.65f, .104f, .8f), "Metal");
            Box(p, "rack roof", new Vector3(0, 1.965f, 0), new Vector3(.65f, .07f, .8f), "Metal");
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "upright", new Vector3(x * .301f, 1.02f, z * .365f), new Vector3(.04f, 1.83f, .05f), "Metal");
            Box(p, "rear mesh backing", new Vector3(0, 1.01f, .394f), new Vector3(.565f, 1.80f, .012f), "Net");
            int servers = lod == 2 ? 3 : 6;
            for (int i = 0; i < servers; i++)
            {
                float y = .28f + i * (lod == 2 ? .51f : .25f);
                Box(p, "rackmount server", new Vector3(0, y, .015f), new Vector3(.54f, .13f, .65f), "Metal");
                Box(p, "server front", new Vector3(0, y, -.317f), new Vector3(.535f, .116f, .018f), "Plastic");
                if (lod == 2) continue;
                for (int x = -1; x <= 1; x += 2)
                    Bar(p, "rack extraction handle", new Vector3(x * .234f, y - .031f, -.345f), new Vector3(x * .234f, y + .031f, -.345f), .012f, "Metal", Sides(lod));
                if (lod != 0) continue;
                for (int k = 0; k < 4; k++)
                    Box(p, "drive caddy", new Vector3(-.157f + k * .095f, y, -.329f), new Vector3(.077f, .049f, .006f), "Metal");
                Box(p, "asset tag", new Vector3(.155f, y + .037f, -.329f), new Vector3(.052f, .016f, .002f), "Paper");
            }
            if (lod == 2) return;
            Bar(p, "hanging rear cable", new Vector3(.25f, 1.8f, .375f), new Vector3(.22f, .15f, .375f), .016f, "Cable");
            Box(p, "empty recovery bay", new Vector3(0, 1.78f, -.29f), new Vector3(.52f, .14f, .008f), "Rubber");
            if (lod == 0)
                for (int i = 0; i < 16; i++)
                    Box(p, "rail mounting hole", new Vector3(-.298f, .2f + i * .105f, -.393f), new Vector3(.013f, .018f, .003f), "Rubber");
        }

        private static void Spool(Transform p, int lod)
        {
            Cylinder(p, "bottom timber flange", new Vector3(0, .018f, 0), new Vector3(.36f, .036f, .36f), "Wood", null, Sides(lod));
            Cylinder(p, "copper winding", new Vector3(0, .16f, 0), new Vector3(.31f, .248f, .31f), "Copper", null, Sides(lod));
            Cylinder(p, "top timber flange", new Vector3(0, .302f, 0), new Vector3(.36f, .036f, .36f), "Wood", null, Sides(lod));
            Cylinder(p, "central spindle bore", new Vector3(0, .322f, 0), new Vector3(.066f, .003f, .066f), "Rubber", null, Sides(lod));
            if (lod == 2) return;
            for (int i = 0; i < (lod == 0 ? 9 : 3); i++)
                Cylinder(p, "wire winding ridge", new Vector3(0, .06f + i * (lod == 0 ? .024f : .08f), 0), new Vector3(.318f, .009f, .318f), "Copper", null, Sides(lod));
            Bar(p, "loose copper tail", new Vector3(.145f, .13f, .045f), new Vector3(.168f, .07f, .095f), .008f, "Copper");
            Box(p, "salvage lot label", new Vector3(-.09f, .322f, .045f), new Vector3(.05f, .002f, .037f), "Paper");
        }

        private static void Battery(Transform p, int lod)
        {
            Box(p, "battery case", new Vector3(0, .215f, 0), new Vector3(.42f, .43f, .27f), "Plastic");
            Box(p, "reinforced lid", new Vector3(0, .442f, 0), new Vector3(.42f, .035f, .27f), "Metal");
            Cylinder(p, "positive terminal", new Vector3(-.14f, .478f, -.065f), new Vector3(.028f, .036f, .028f), "Copper", null, Sides(lod));
            Cylinder(p, "negative terminal", new Vector3(.14f, .478f, -.065f), new Vector3(.028f, .036f, .028f), "Metal", null, Sides(lod));
            Bar(p, "folded handle", new Vector3(-.095f, .505f, .04f), new Vector3(.095f, .505f, .04f), .03f, "Rubber", Sides(lod));
            if (lod == 2) return;
            for (int x = -1; x <= 1; x += 2)
                Bar(p, "handle hinge", new Vector3(x * .095f, .458f, .04f), new Vector3(x * .095f, .505f, .04f), .014f, "Metal", Sides(lod));
            Box(p, "hazard plate", new Vector3(0, .295f, -.138f), new Vector3(.19f, .115f, .004f), "Safety");
            Box(p, "recovery inventory sticker", new Vector3(.12f, .155f, -.138f), new Vector3(.07f, .045f, .003f), "Paper");
            if (lod != 0) return;
            for (int i = 0; i < 5; i++)
                Box(p, "moulded cooling rib", new Vector3(-.167f + i * .083f, .13f, -.138f), new Vector3(.014f, .21f, .007f), "Plastic");
            for (int i = 0; i < 4; i++)
                Cylinder(p, "cell cap", new Vector3(-.12f + i * .08f, .465f, .087f), new Vector3(.032f, .01f, .032f), "Plastic", null, 8);
        }
    }
}
