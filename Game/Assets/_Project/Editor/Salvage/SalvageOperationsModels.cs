using UnityEngine;
using static Abandoned.EditorTools.SalvageModelFactory;

namespace Abandoned.EditorTools
{
    /// <summary>Salvage staging equipment and demolition dressing, in metres.</summary>
    public static class SalvageOperationsModels
    {
        public static bool Build(string id, Transform root, int lod)
        {
            switch (id)
            {
                case "shipping_pallet": Pallet(root, lod); return true;
                case "industrial_dumpster": Dumpster(root, lod); return true;
                case "pallet_jack": PalletJack(root, lod); return true;
                case "cargo_crate": CargoCrate(root, lod); return true;
                case "plastic_sorting_crate": SortingCrate(root, lod); return true;
                case "work_floodlight": Floodlight(root, lod); return true;
                case "generator_hub": GeneratorHub(root, lod); return true;
                case "demolition_fence": Fence(root, lod, false); return true;
                case "orange_netting": Fence(root, lod, true); return true;
                case "dust_sheet": DustSheet(root, lod); return true;
                case "exposed_partition": Partition(root, lod); return true;
                case "cable_bundle": Cables(root, lod); return true;
                case "salvage_tag": Tag(root, lod); return true;
                case "tire_track": TireTrack(root, lod); return true;
                case "broken_glass": Glass(root, lod); return true;
                default: return false;
            }
        }

        static int Sides(int lod) => lod == 0 ? 12 : lod == 1 ? 8 : 6;

        static void Pallet(Transform p, int lod)
        {
            for (int i = 0; i < 5; i++)
                Box(p, "Pallet deck board", new Vector3(-.52f + i * .26f, .127f, 0), new Vector3(.20f, .038f, 1.0f), "Wood");
            for (int x = -1; x <= 1; x++)
            {
                Box(p, "Bottom skid board", new Vector3(x * .50f, .015f, 0), new Vector3(.14f, .03f, 1.0f), "Wood");
                for (int z = -1; z <= 1; z++)
                    Box(p, "Pallet spacer block", new Vector3(x * .50f, .069f, z * .40f), new Vector3(.14f, .078f, .15f), "Wood");
            }
            if (lod == 0)
                for (int x = -1; x <= 1; x += 2)
                    for (int z = -1; z <= 1; z += 2)
                        Cylinder(p, "Deck nail head", new Vector3(x * .52f, .148f, z * .40f), new Vector3(.012f, .004f, .012f), "Metal", null, 6);
        }

        static void Dumpster(Transform p, int lod)
        {
            Box(p, "Dumpster base", new Vector3(0, .17f, 0), new Vector3(2.38f, .10f, 1.23f), "Rust");
            Box(p, "Front bin panel", new Vector3(0, .84f, .64f), new Vector3(2.56f, 1.35f, .07f), "Paint", new Vector3(-4, 0, 0));
            Box(p, "Back bin panel", new Vector3(0, .84f, -.64f), new Vector3(2.56f, 1.35f, .07f), "Paint", new Vector3(4, 0, 0));
            for (int side = -1; side <= 1; side += 2)
            {
                Box(p, "End bin panel", new Vector3(side * 1.26f, .84f, 0), new Vector3(.07f, 1.35f, 1.38f), "Paint", new Vector3(0, 0, side * -4));
                Box(p, "Dumpster runner", new Vector3(side * .86f, .06f, 0), new Vector3(.16f, .12f, 1.22f), "Metal");
                Box(p, "Top lip", new Vector3(side * 1.3f, 1.54f, 0), new Vector3(.11f, .09f, 1.44f), "Metal");
                Box(p, "Split lid", new Vector3(side * .64f, 1.58f + (side > 0 ? .22f : 0), -.05f), new Vector3(1.23f, .07f, 1.46f), "Plastic", new Vector3(side > 0 ? -22 : 0, 0, 0));
            }
            if (lod == 2) return;
            Box(p, "Front lifting sleeve", new Vector3(0, .99f, .755f), new Vector3(1.06f, .24f, .15f), "Metal");
            Box(p, "Classification sign", new Vector3(0, .55f, .704f), new Vector3(.61f, .32f, .007f), "Paper");
            for (int i = 0; i < 5; i++)
                Box(p, "Pressed steel rib", new Vector3(-1.03f + i * .515f, .87f, .704f), new Vector3(.037f, 1.18f, .025f), "Paint", new Vector3(-4, 0, 0));
            if (lod == 0)
                for (int side = -1; side <= 1; side += 2)
                    Bar(p, "Lid grip", new Vector3(side * .64f - .15f, 1.67f, .55f), new Vector3(side * .64f + .15f, 1.67f, .55f), .028f, "Metal", Sides(lod));
        }

        static void PalletJack(Transform p, int lod)
        {
            int s = Sides(lod);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(p, "Fork tine", new Vector3(side * .23f, .11f, .27f), new Vector3(.16f, .065f, 1.10f), "Safety");
                Cylinder(p, "Fork load roller", new Vector3(side * .23f, .055f, .71f), new Vector3(.11f, .12f, .11f), "Rubber", new Vector3(0, 0, 90), s);
            }
            Box(p, "Fork bridge", new Vector3(0, .18f, -.27f), new Vector3(.62f, .20f, .21f), "Safety");
            Cylinder(p, "Pump housing", new Vector3(0, .24f, -.38f), new Vector3(.18f, .32f, .18f), "Metal", null, s);
            Cylinder(p, "Steering wheel", new Vector3(0, .075f, -.40f), new Vector3(.15f, .15f, .15f), "Rubber", new Vector3(0, 0, 90), s);
            Bar(p, "Tiller stem", new Vector3(0, .34f, -.39f), new Vector3(0, 1.03f, -.70f), .045f, "Metal", s);
            Bar(p, "Loop left", new Vector3(0, 1.03f, -.70f), new Vector3(-.19f, 1.19f, -.77f), .032f, "Metal", s);
            Bar(p, "Loop right", new Vector3(0, 1.03f, -.70f), new Vector3(.19f, 1.19f, -.77f), .032f, "Metal", s);
            Bar(p, "Steering grip", new Vector3(-.19f, 1.19f, -.77f), new Vector3(.19f, 1.19f, -.77f), .038f, "Rubber", s);
            if (lod == 0)
            {
                Bar(p, "Release lever", new Vector3(.06f, 1.08f, -.72f), new Vector3(.10f, 1.16f, -.76f), .016f, "Metal", s);
                Box(p, "Capacity label", new Vector3(.12f, .29f, -.264f), new Vector3(.18f, .035f, .005f), "Paper");
            }
        }

        static void CargoCrate(Transform p, int lod)
        {
            Box(p, "Cargo box", new Vector3(0, .59f, 0), new Vector3(1.08f, .95f, .83f), "Wood");
            for (int x = -1; x <= 1; x += 2)
            {
                Box(p, "Extraction skid", new Vector3(x * .39f, .055f, 0), new Vector3(.17f, .11f, .89f), "Wood");
                for (int z = -1; z <= 1; z += 2)
                    Box(p, "Corner reinforcement", new Vector3(x * .51f, .59f, z * .418f), new Vector3(.087f, 1.01f, .037f), "Wood");
            }
            Box(p, "Crate lid", new Vector3(0, 1.084f, 0), new Vector3(1.13f, .047f, .88f), "Wood");
            for (int z = -1; z <= 1; z += 2)
                Box(p, "End horizontal brace", new Vector3(0, .58f, z * .441f), new Vector3(1.06f, .11f, .03f), "Wood");
            if (lod == 2) return;
            Box(p, "Cargo classification label", new Vector3(.12f, .78f, .463f), new Vector3(.36f, .22f, .008f), "Paper");
            Box(p, "Lid metal band", new Vector3(-.24f, 1.112f, 0), new Vector3(.031f, .006f, .88f), "Metal");
            Box(p, "Front metal band", new Vector3(-.24f, .59f, .459f), new Vector3(.031f, 1.01f, .008f), "Metal");
            if (lod == 0)
                for (int i = 0; i < 4; i++)
                    Box(p, "Individual board seam", new Vector3(-.39f + i * .25f, .59f, .417f), new Vector3(.008f, .92f, .005f), "Rust");
        }

        static void SortingCrate(Transform p, int lod)
        {
            Box(p, "Crate floor", new Vector3(0, .02f, 0), new Vector3(.58f, .04f, .39f), "Plastic");
            for (int side = -1; side <= 1; side += 2)
            {
                Box(p, "Long crate wall", new Vector3(0, .16f, side * .192f), new Vector3(.59f, .28f, .025f), "Plastic");
                Box(p, "Short crate wall", new Vector3(side * .284f, .16f, 0), new Vector3(.025f, .28f, .36f), "Plastic");
                Box(p, "Long stacking rim", new Vector3(0, .305f, side * .198f), new Vector3(.61f, .03f, .042f), "Plastic");
                Box(p, "Short stacking rim", new Vector3(side * .289f, .305f, 0), new Vector3(.042f, .03f, .40f), "Plastic");
            }
            if (lod == 2) return;
            Box(p, "Classification card", new Vector3(-.06f, .17f, .211f), new Vector3(.21f, .09f, .006f), "Paper");
            for (int side = -1; side <= 1; side += 2)
                Box(p, "Carry grip inset", new Vector3(side * .30f, .245f, 0), new Vector3(.008f, .027f, .15f), "Rubber");
            if (lod == 0)
                for (int i = 0; i < 6; i++)
                    for (int side = -1; side <= 1; side += 2)
                        Box(p, "Moulded side rib", new Vector3(-.245f + i * .098f, .14f, side * .211f), new Vector3(.013f, .23f, .012f), "Plastic");
        }

        static void Floodlight(Transform p, int lod)
        {
            int s = Sides(lod);
            Bar(p, "Telescopic mast", new Vector3(0, .13f, 0), new Vector3(0, 2.10f, 0), .041f, "Metal", s);
            for (int i = 0; i < 3; i++)
            {
                float a = i * Mathf.PI * 2 / 3;
                Bar(p, "Tripod leg", new Vector3(0, .55f, 0), new Vector3(Mathf.Cos(a) * .49f, .027f, Mathf.Sin(a) * .49f), .035f, "Safety", s);
            }
            Bar(p, "Twin head crossbar", new Vector3(-.42f, 2.08f, 0), new Vector3(.42f, 2.08f, 0), .033f, "Metal", s);
            for (int side = -1; side <= 1; side += 2)
            {
                Box(p, "Floodlight housing", new Vector3(side * .30f, 2.22f, 0), new Vector3(.32f, .24f, .10f), "Safety", new Vector3(-10, 0, 0));
                Box(p, "Lamp glass", new Vector3(side * .30f, 2.222f, .06f), new Vector3(.26f, .18f, .008f), "Glass", new Vector3(-10, 0, 0));
                if (lod < 2) Box(p, "LED emitter", new Vector3(side * .30f, 2.22f, .067f), new Vector3(.17f, .10f, .003f), "Paint", new Vector3(-10, 0, 0));
            }
            if (lod == 0)
            {
                Cylinder(p, "Mast adjustment knob", new Vector3(.036f, 1.24f, 0), new Vector3(.06f, .045f, .06f), "Plastic", new Vector3(0, 0, 90), s);
                Bar(p, "Hanging light cable", new Vector3(.02f, 2.11f, -.04f), new Vector3(.06f, .10f, -.06f), .008f, "Cable", s);
                Box(p, "Foot protection cap", new Vector3(.49f, .027f, 0), new Vector3(.08f, .05f, .06f), "Rubber");
            }
        }

        static void GeneratorHub(Transform p, int lod)
        {
            Box(p, "Crew generator enclosure", new Vector3(0, .53f, 0), new Vector3(1.78f, .92f, .82f), "Paint");
            Box(p, "Heavy lifting base", new Vector3(0, .045f, 0), new Vector3(1.85f, .09f, .88f), "Metal");
            Box(p, "Weather hood", new Vector3(0, 1.02f, 0), new Vector3(1.88f, .10f, .91f), "Metal");
            Box(p, "Distribution panel", new Vector3(.39f, .65f, .44f), new Vector3(.68f, .61f, .09f), "Plastic");
            Bar(p, "Exhaust outlet", new Vector3(-.55f, 1.01f, -.12f), new Vector3(-.55f, 1.20f, -.12f), .075f, "Rust", Sides(lod));
            if (lod == 2) return;
            Box(p, "Control screen", new Vector3(.39f, .83f, .493f), new Vector3(.25f, .105f, .012f), "Glass");
            for (int i = 0; i < 4; i++)
                Cylinder(p, "Site power socket", new Vector3(.17f + i * .145f, .60f, .499f), new Vector3(.075f, .020f, .075f), "Rubber", new Vector3(90, 0, 0), Sides(lod));
            for (int i = 0; i < (lod == 0 ? 9 : 4); i++)
                Box(p, "Vent louvre", new Vector3(-.48f, .30f + i * (lod == 0 ? .053f : .12f), .422f), new Vector3(.56f, .027f, .024f), "Rubber");
            Box(p, "Crew operating label", new Vector3(-.48f, .88f, .434f), new Vector3(.39f, .09f, .006f), "Paper");
            if (lod == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float x = .24f + i * .14f;
                    Bar(p, "Outgoing site lead", new Vector3(x, .59f, .514f), new Vector3(x, .09f, .65f), .016f, "Cable", 8);
                    Bar(p, "Floor lead", new Vector3(x, .09f, .65f), new Vector3(x - .17f, .018f, 1.10f), .016f, "Cable", 8);
                }
                Cylinder(p, "Emergency stop", new Vector3(.63f, .83f, .5f), new Vector3(.04f, .022f, .04f), "Safety", new Vector3(90, 0, 0), 8);
            }
        }

        static void Fence(Transform p, int lod, bool net)
        {
            float w = net ? 2.40f : 2.20f;
            float h = net ? 1.25f : 1.80f;
            string finish = net ? "Net" : "Metal";
            for (int side = -1; side <= 1; side += 2)
            {
                Bar(p, "Barrier upright", new Vector3(side * w * .5f, .02f, 0), new Vector3(side * w * .5f, h, 0), .037f, net ? "Safety" : "Metal", Sides(lod));
                Box(p, "Barrier stability foot", new Vector3(side * w * .5f, .037f, 0), new Vector3(.16f, .074f, .53f), "Concrete");
            }
            for (int edge = 0; edge < 2; edge++)
                Bar(p, "Perimeter rail", new Vector3(-w * .5f, .16f + edge * (h - .16f), 0), new Vector3(w * .5f, .16f + edge * (h - .16f), 0), .028f, finish, Sides(lod));
            if (lod == 2)
            {
                Bar(p, "Open panel diagonal", new Vector3(-w * .5f, .16f, 0), new Vector3(w * .5f, h, 0), .009f, finish, 6);
                return;
            }
            float step = lod == 0 ? (net ? .13f : .16f) : .32f;
            float diameter = net ? .012f : .004f;
            // Clipped diamond wires preserve clear views through the barrier.
            for (int slope = -1; slope <= 1; slope += 2)
                for (float offset = -w * .5f - h; offset <= w * .5f + h; offset += step)
                {
                    float minY = .17f;
                    float maxY = h - .025f;
                    float leftY = (-w * .5f - offset) / slope;
                    float rightY = (w * .5f - offset) / slope;
                    minY = Mathf.Max(minY, Mathf.Min(leftY, rightY));
                    maxY = Mathf.Min(maxY, Mathf.Max(leftY, rightY));
                    if (maxY - minY < .025f) continue;
                    Bar(p, "Diamond safety mesh", new Vector3(offset + slope * minY, minY, .002f), new Vector3(offset + slope * maxY, maxY, .002f), diameter, finish, 6);
                }
            if (!net) Box(p, "Demolition warning board", new Vector3(.52f, 1.28f, .015f), new Vector3(.43f, .29f, .011f), "Safety");
        }

        static void DustSheet(Transform p, int lod)
        {
            int columns = lod == 2 ? 3 : lod == 1 ? 5 : 8;
            float width = 2.4f / columns;
            for (int i = 0; i < columns; i++)
            {
                float x = -1.2f + width * (i + .5f);
                float drop = .12f * Mathf.Sin((i + 1) * 1.6f);
                Box(p, "Faceted hanging dust cloth", new Vector3(x, 1.0f + drop * .5f, (i % 2 == 0 ? .02f : -.04f)), new Vector3(width * 1.02f, 1.94f + drop, .012f), "Fabric", new Vector3((i % 2 == 0 ? 4 : -4), 0, (i % 3 - 1) * 2));
                Box(p, "Dust sheet floor pooling", new Vector3(x, .016f, .15f), new Vector3(width * 1.06f, .018f, .36f), "Fabric", new Vector3(0, (i % 3 - 1) * 10, 0));
            }
            Bar(p, "Temporary suspension cord", new Vector3(-1.23f, 2.15f, -.04f), new Vector3(1.23f, 2.05f, -.04f), .011f, "Cable", 6);
            if (lod == 0)
                for (int i = 0; i < 5; i++)
                    Box(p, "Cloth tie", new Vector3(-1.08f + i * .54f, 2.05f - i * .015f, -.04f), new Vector3(.025f, .065f, .017f), "Plastic");
        }

        static void Partition(Transform p, int lod)
        {
            for (int i = 0; i < 5; i++)
                Box(p, "Exposed wall stud", new Vector3(-1.15f + i * .575f, 1.15f, 0), new Vector3(.042f, 2.30f, .08f), "Metal");
            Box(p, "Bottom framing track", new Vector3(0, .025f, 0), new Vector3(2.39f, .05f, .09f), "Metal");
            Box(p, "Top framing track", new Vector3(0, 2.325f, 0), new Vector3(2.39f, .05f, .09f), "Metal");
            Box(p, "Remaining drywall panel", new Vector3(-.88f, 1.15f, .056f), new Vector3(.54f, 2.30f, .027f), "Concrete");
            Box(p, "Broken upper panel", new Vector3(-.17f, 2.07f, .056f), new Vector3(.87f, .44f, .027f), "Concrete", new Vector3(0, 0, -6));
            Box(p, "Broken lower panel", new Vector3(.41f, .22f, .056f), new Vector3(1.37f, .44f, .027f), "Concrete", new Vector3(0, 0, 3));
            if (lod == 2) return;
            Bar(p, "Exposed conduit", new Vector3(.69f, .10f, -.01f), new Vector3(.69f, 1.81f, -.01f), .023f, "Copper", Sides(lod));
            Bar(p, "Conduit turn", new Vector3(.69f, 1.81f, -.01f), new Vector3(.31f, 1.81f, -.01f), .023f, "Copper", Sides(lod));
            Box(p, "Exposed junction box", new Vector3(.31f, 1.81f, .025f), new Vector3(.14f, .14f, .045f), "Metal");
            if (lod == 0)
            {
                Bar(p, "Dangling wire", new Vector3(.31f, 1.75f, .06f), new Vector3(.36f, 1.05f, .095f), .008f, "Cable", 6);
                Box(p, "Broken drywall fragment", new Vector3(.12f, .035f, .21f), new Vector3(.48f, .04f, .23f), "Concrete", new Vector3(0, 19, 0));
            }
        }

        static void Cables(Transform p, int lod)
        {
            int count = lod == 2 ? 2 : lod == 1 ? 4 : 7;
            for (int i = 0; i < count; i++)
            {
                float x = (i - count * .5f) * .014f;
                Bar(p, "Cable bend one", new Vector3(x - .05f, .014f, -.68f), new Vector3(x, .018f, -.10f), .012f, "Cable", 6);
                Bar(p, "Cable bend two", new Vector3(x, .018f, -.10f), new Vector3(x + .16f, .015f, .41f), .012f, "Cable", 6);
                Bar(p, "Cable bend three", new Vector3(x + .16f, .015f, .41f), new Vector3(x + .10f, .018f, .77f), .012f, "Cable", 6);
            }
            if (lod < 2)
            {
                Box(p, "Zip tie", new Vector3(0, .026f, -.18f), new Vector3(.13f, .007f, .015f), "Plastic");
                Box(p, "Cord safety ramp", new Vector3(.05f, .035f, .07f), new Vector3(.27f, .05f, .31f), "Safety");
            }
            if (lod == 0)
                Box(p, "Cord plug", new Vector3(.09f, .021f, .82f), new Vector3(.035f, .035f, .07f), "Rubber");
        }

        static void Tag(Transform p, int lod)
        {
            Box(p, "Salvage door tag panel", new Vector3(0, .205f, 0), new Vector3(.59f, .41f, .008f), "Paper");
            Box(p, "Spray painted stripe", new Vector3(0, .075f, .005f), new Vector3(.52f, .045f, .002f), "Safety");
            if (lod == 0)
            {
                Box(p, "Left fixing tape", new Vector3(-.23f, .38f, .005f), new Vector3(.07f, .063f, .003f), "Wrap", new Vector3(0, 0, -13));
                Box(p, "Right fixing tape", new Vector3(.23f, .38f, .005f), new Vector3(.07f, .063f, .003f), "Wrap", new Vector3(0, 0, 9));
            }
        }

        static void TireTrack(Transform p, int lod)
        {
            // Thin graphic geometry is visual dressing only; builder supplies no collider.
            int count = lod == 2 ? 8 : lod == 1 ? 14 : 22;
            for (int i = 0; i < count; i++)
                for (int side = -1; side <= 1; side += 2)
                    Box(p, "Dust tire tread imprint", new Vector3(side * .055f, .0015f, -1f + i * (2f / count)), new Vector3(.085f, .002f, .035f), "Rubber", new Vector3(0, side * 28, 0));
        }

        static void Glass(Transform p, int lod)
        {
            int count = lod == 2 ? 4 : lod == 1 ? 7 : 12;
            for (int i = 0; i < count; i++)
            {
                float a = i * 2.39996f;
                float distance = .11f + (i % 5) * .064f;
                Box(p, "Smashed display glass shard", new Vector3(Mathf.Cos(a) * distance, .004f + (i % 3) * .002f, Mathf.Sin(a) * distance), new Vector3(.10f + (i % 3) * .04f, .004f, .035f + (i % 4) * .02f), "Glass", new Vector3(0, i * 31, 0));
            }
        }
    }
}
