using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Original metre-scale clearance hardware, with bottom pivots.</summary>
    public static class SalvageHardwareModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "hydraulic_jack": Jack(root, lod); return true;
                case "portable_generator": Generator(root, lod); return true;
                case "angle_grinder": Grinder(root, lod); return true;
                case "tool_chest": ToolChest(root, lod); return true;
                case "scaffolding_pipes": Scaffold(root, lod); return true;
                case "safety_harness": Harness(root, lod); return true;
                default: return false;
            }
        }

        static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        static void Jack(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Trolley chassis", new Vector3(0, .12f, 0), new Vector3(.31f, .09f, .68f), "Paint");
            Box(p, "Left rail", new Vector3(-.13f, .18f, -.02f), new Vector3(.045f, .13f, .58f), "Metal");
            Box(p, "Right rail", new Vector3(.13f, .18f, -.02f), new Vector3(.045f, .13f, .58f), "Metal");
            Bar(p, "Pivoting lift arm", new Vector3(0, .18f, -.19f), new Vector3(0, .37f, .24f), .085f, "Paint", s);
            Cylinder(p, "Lifting saddle", new Vector3(0, .40f, .24f), new Vector3(.17f, .045f, .17f), "Metal", null, s);
            Bar(p, "Handle shaft", new Vector3(0, .2f, -.31f), new Vector3(0, .72f, -.56f), .025f, "Metal", s);
            Bar(p, "Rubber handle grip", new Vector3(0, .64f, -.52f), new Vector3(0, .77f, -.59f), .036f, "Rubber", s);
            for (int i = -1; i <= 1; i += 2)
            {
                Cylinder(p, "Front roller", new Vector3(i * .18f, .08f, .22f), new Vector3(.16f, .055f, .16f), "Metal", new Vector3(0, 0, 90), s);
                Cylinder(p, "Rear caster", new Vector3(i * .145f, .055f, -.24f), new Vector3(.11f, .048f, .11f), "Rubber", new Vector3(0, 0, 90), s);
            }
            if (lod == 2) return;
            Bar(p, "Hydraulic ram", new Vector3(0, .20f, -.20f), new Vector3(0, .30f, .11f), .046f, "Metal", s);
            Cylinder(p, "Arm pivot", new Vector3(0, .19f, -.18f), new Vector3(.065f, .32f, .065f), "Metal", new Vector3(0, 0, 90), s);
            if (lod == 0)
            {
                Box(p, "Warning label", new Vector3(-.132f, .20f, -.035f), new Vector3(.003f, .055f, .16f), "Safety");
                Cylinder(p, "Release screw", new Vector3(.05f, .24f, -.22f), new Vector3(.025f, .025f, .025f), "Copper", null, s);
            }
        }

        static void Generator(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Engine crankcase", new Vector3(-.12f, .26f, 0), new Vector3(.27f, .26f, .31f), "Metal");
            Cylinder(p, "Alternator drum", new Vector3(.16f, .27f, 0), new Vector3(.28f, .28f, .28f), "Metal", new Vector3(0, 0, 90), s);
            Box(p, "Fuel tank", new Vector3(0, .48f, -.02f), new Vector3(.54f, .15f, .35f), "Paint");
            Box(p, "Outlet panel", new Vector3(0, .31f, .22f), new Vector3(.45f, .22f, .025f), "Plastic");
            for (int z = -1; z <= 1; z += 2)
            {
                Bar(p, "Bottom frame", new Vector3(-.34f, .045f, z * .24f), new Vector3(.34f, .045f, z * .24f), .038f, "Metal", s);
                Bar(p, "Top carrying bar", new Vector3(-.34f, .60f, z * .24f), new Vector3(.34f, .60f, z * .24f), .038f, "Metal", s);
                for (int x = -1; x <= 1; x += 2)
                    Bar(p, "Protective cage upright", new Vector3(x * .34f, .045f, z * .24f), new Vector3(x * .34f, .60f, z * .24f), .038f, "Metal", s);
            }
            for (int x = -1; x <= 1; x += 2)
                Box(p, "Anti vibration feet", new Vector3(x * .28f, .018f, 0), new Vector3(.10f, .036f, .48f), "Rubber");
            if (lod == 2) return;
            Cylinder(p, "Fuel filler cap", new Vector3(.12f, .575f, -.02f), new Vector3(.075f, .025f, .075f), "Plastic", null, s);
            Cylinder(p, "Pull starter cover", new Vector3(-.13f, .29f, -.175f), new Vector3(.21f, .032f, .21f), "Plastic", new Vector3(90, 0, 0), s);
            Box(p, "Exhaust muffler", new Vector3(-.16f, .36f, -.23f), new Vector3(.15f, .11f, .06f), "Rust");
            for (int i = 0; i < 2; i++)
                Box(p, "Socket", new Vector3(-.10f + i * .14f, .32f, .24f), new Vector3(.058f, .067f, .012f), "Rubber");
            if (lod == 0)
            {
                for (int i = 0; i < 5; i++)
                    Box(p, "Cooling rib", new Vector3(-.15f, .25f + i * .023f, -.07f), new Vector3(.24f, .008f, .30f), "Metal");
                Box(p, "Load indicator", new Vector3(.16f, .32f, .24f), new Vector3(.067f, .046f, .013f), "Glass");
                Box(p, "Electricity warning", new Vector3(.1f, .482f, .16f), new Vector3(.14f, .08f, .005f), "Safety");
            }
        }

        static void Grinder(Transform p, int lod)
        {
            int s = Sides(lod);
            Cylinder(p, "Motor grip body", new Vector3(0, .12f, -.035f), new Vector3(.095f, .22f, .095f), "Paint", new Vector3(90, 0, 0), s);
            Cylinder(p, "Gear head", new Vector3(0, .12f, .105f), new Vector3(.083f, .075f, .083f), "Metal", null, s);
            Cylinder(p, "Cutting disc", new Vector3(0, .052f, .125f), new Vector3(.125f, .006f, .125f), "Rubber", null, s);
            Bar(p, "Auxiliary handle", new Vector3(-.028f, .12f, .095f), new Vector3(-.145f, .12f, .095f), .032f, "Rubber", s);
            Box(p, "Guard spine", new Vector3(0, .078f, .076f), new Vector3(.135f, .024f, .047f), "Metal");
            if (lod == 2) return;
            Cylinder(p, "Disc retaining nut", new Vector3(0, .065f, .125f), new Vector3(.026f, .018f, .026f), "Metal", null, s);
            Box(p, "Slide switch", new Vector3(0, .17f, -.04f), new Vector3(.025f, .01f, .045f), "Plastic");
            Bar(p, "Cable strain relief", new Vector3(0, .12f, -.155f), new Vector3(0, .09f, -.20f), .016f, "Rubber", s);
            if (lod == 0)
            {
                for (int i = 0; i < 4; i++)
                    Box(p, "Body vents", new Vector3(.047f, .12f, -.08f + i * .022f), new Vector3(.003f, .035f, .009f), "Rubber");
                Box(p, "Rating sticker", new Vector3(-.047f, .12f, -.025f), new Vector3(.003f, .038f, .065f), "Paper");
            }
        }

        static void ToolChest(Transform p, int lod)
        {
            int s = Sides(lod);
            Box(p, "Rolling cabinet body", new Vector3(0, .58f, 0), new Vector3(.91f, .83f, .49f), "Paint");
            Box(p, "Rubber worktop", new Vector3(0, 1.01f, 0), new Vector3(.95f, .04f, .53f), "Rubber");
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                    Cylinder(p, "Caster wheel", new Vector3(x * .36f, .075f, z * .17f), new Vector3(.15f, .045f, .15f), "Rubber", new Vector3(0, 0, 90), s);
            Bar(p, "Push handle", new Vector3(.52f, .82f, -.18f), new Vector3(.52f, .82f, .18f), .035f, "Metal", s);
            if (lod == 2) return;
            int count = lod == 0 ? 6 : 4;
            float h = .73f / count;
            for (int i = 0; i < count; i++)
            {
                float y = .24f + i * h;
                Box(p, "Drawer fascia", new Vector3(0, y, .253f), new Vector3(.85f, h - .014f, .018f), "Paint");
                Box(p, "Drawer pull", new Vector3(0, y + h * .23f, .274f), new Vector3(.72f, .018f, .021f), "Metal");
            }
            if (lod == 0)
            {
                Cylinder(p, "Drawer lock", new Vector3(.37f, .948f, .269f), new Vector3(.022f, .008f, .022f), "Metal", new Vector3(90, 0, 0), s);
                Box(p, "Salvage inventory label", new Vector3(-.31f, .47f, .269f), new Vector3(.13f, .065f, .003f), "Paper");
            }
        }

        static void Scaffold(Transform p, int lod)
        {
            int s = Sides(lod);
            int count = lod == 2 ? 5 : 9;
            for (int i = 0; i < count; i++)
            {
                float x = (i % 3 - 1) * .085f;
                float y = .03f + (i / 3) * .063f;
                float length = i % 3 == 0 ? 2.35f : 2.60f;
                Bar(p, "Stacked scaffold tube", new Vector3(x, y, -length * .5f), new Vector3(x, y, length * .5f), .049f, i % 4 == 0 ? "Rust" : "Metal", s);
            }
            for (int z = -1; z <= 1; z += 2)
            {
                Box(p, "Shipping timber", new Vector3(0, .025f, z * .83f), new Vector3(.43f, .05f, .10f), "Wood");
                Box(p, "Retaining strap top", new Vector3(0, .215f, z * .83f), new Vector3(.33f, .01f, .035f), "Plastic");
                for (int x = -1; x <= 1; x += 2)
                    Box(p, "Retaining strap side", new Vector3(x * .16f, .12f, z * .83f), new Vector3(.01f, .19f, .035f), "Plastic");
            }
            if (lod == 0)
                for (int i = 0; i < 3; i++)
                    Box(p, "Clamp block", new Vector3(.11f - i * .08f, .18f, .55f), new Vector3(.08f, .065f, .12f), "Metal", new Vector3(0, i * 13, 0));
        }

        static void Harness(Transform p, int lod)
        {
            var straps = Group("Safety harness straps", p);
            Box(straps, "Left shoulder webbing", new Vector3(-.13f, .59f, 0), new Vector3(.055f, .50f, .014f), "Safety", new Vector3(0, 0, -14));
            Box(straps, "Right shoulder webbing", new Vector3(.13f, .59f, 0), new Vector3(.055f, .50f, .014f), "Safety", new Vector3(0, 0, 14));
            Box(straps, "Chest strap", new Vector3(0, .64f, .005f), new Vector3(.27f, .04f, .017f), "Fabric");
            Box(straps, "Waist belt", new Vector3(0, .33f, 0), new Vector3(.39f, .065f, .022f), "Safety");
            for (int side = -1; side <= 1; side += 2)
            {
                Box(straps, "Thigh strap outside", new Vector3(side * .19f, .14f, 0), new Vector3(.05f, .26f, .017f), "Safety", new Vector3(0, 0, side * 12));
                Box(straps, "Thigh strap inside", new Vector3(side * .075f, .14f, 0), new Vector3(.05f, .26f, .017f), "Safety", new Vector3(0, 0, side * -8));
                Box(straps, "Thigh loop base", new Vector3(side * .13f, .025f, 0), new Vector3(.16f, .05f, .019f), "Safety");
            }
            if (lod == 2) return;
            Box(p, "Waist clasp", new Vector3(0, .33f, .021f), new Vector3(.076f, .075f, .025f), "Metal");
            Box(p, "Chest buckle", new Vector3(.025f, .64f, .022f), new Vector3(.055f, .055f, .023f), "Metal");
            Bar(p, "D ring left", new Vector3(-.035f, .77f, -.015f), new Vector3(-.035f, .85f, -.015f), .012f, "Metal", Sides(lod));
            Bar(p, "D ring right", new Vector3(.035f, .77f, -.015f), new Vector3(.035f, .85f, -.015f), .012f, "Metal", Sides(lod));
            Bar(p, "D ring top", new Vector3(-.035f, .85f, -.015f), new Vector3(.035f, .85f, -.015f), .012f, "Metal", Sides(lod));
            if (lod == 0)
                Box(p, "Inspection tag", new Vector3(-.19f, .48f, .02f), new Vector3(.043f, .095f, .004f), "Paper", new Vector3(0, 0, -14));
        }
    }
}
