using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.GreyboxFactory;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap yard's other buildings (ShowcaseSitePlan has the plot): the roofless shell of the old boiler house with
    /// its chimney, stacked shipping containers in the gravel lot, a steel water tower, and an open lean-to over the lot's
    /// stores. Built from boxes in the scanned materials; they are scenery with solid walls, never structural sections.
    /// </summary>
    public static class ShowcaseYard
    {
        public static void Build(Transform root)
        {
            Transform yard = Group("Yard", root);
            BoilerHouse(Group("BoilerHouse", yard));
            Chimney(Group("Chimney", yard));
            Containers(Group("Containers", yard));
            WaterTower(Group("WaterTower", yard));
            LeanTo(Group("LeanTo", yard));
            MarkStatic(yard);
        }

        private static Material Brick => ShowcaseMaterials.Rescan(ShowcaseMaterials.Surface("TestMap_Ruin_Brick", new Color(.45f, .27f, .2f)), "factory_brick", 1.5f);
        private static Material Rust => ShowcaseMaterials.Surface("TestMap_Rust_Oxide_Tank", new Color(.36f, .2f, .12f));
        private static Material Cladding => ShowcaseMaterials.Surface("TestMap_Corrugated_Steel_Cladding", new Color(.42f, .43f, .43f));

        // Brick walls one metre at a time, so the top can be broken: each strip's height drops where the wall has
        // fallen. Openings are strips left low (doorways at 0, windows with a sill and a head).
        private static void BoilerHouse(Transform house)
        {
            Rect r = BoilerPlot;
            const float height = 5.6f, thick = .38f;
            Material floor = ShowcaseMaterials.Rescan(ShowcaseMaterials.Surface("TestMap_Boiler_Floor_Concrete", new Color(.4f, .4f, .38f)), "concrete_moss", 3f);
            Box("Floor", house, new Vector3(r.center.x, -.08f, r.center.y), new Vector3(r.width - .1f, .2f, r.height - .1f), floor);
            var random = new System.Random(31);
            void Wall(Vector2 a, Vector2 b, string name, System.Func<float, (float sill, float head)> opening)
            {
                Vector2 dir = (b - a).normalized;
                float length = Vector2.Distance(a, b);
                int strips = Mathf.CeilToInt(length);
                float step = length / strips;
                Transform wall = Group(name, house);
                for (int i = 0; i < strips; i++)
                {
                    float s = (i + .5f) * step;
                    Vector2 c = a + dir * s;
                    // The roofline has gone in two places; elsewhere the top is ragged by a course or three.
                    float fallen = Fbm(c.x * .21f + 3f, c.y * .21f, 2);
                    float top = height - (float)random.NextDouble() * .5f - Smooth(.55f, .7f, fallen) * 3.4f;
                    (float sill, float head) = opening(s);
                    Vector3 size = Mathf.Abs(dir.x) > .5f ? new Vector3(step + .001f, 0f, thick) : new Vector3(thick, 0f, step + .001f);
                    void Piece(float y0, float y1)
                    {
                        if (y1 - y0 < .05f) return;
                        size.y = y1 - y0;
                        Box("Brick", wall, new Vector3(c.x, (y0 + y1) / 2f, c.y), size, Brick);
                    }
                    if (sill <= 0f && head <= 0f) Piece(0f, top);
                    else
                    {
                        Piece(0f, sill);
                        if (head < top) Piece(head, top);
                    }
                }
            }
            // Windows every three metres on the long walls; a wide doorway facing the warehouse and one to the south.
            (float, float) Windows(float s) => Mathf.Repeat(s, 3f) is > .9f and < 2.1f && s > 1.5f && s < r.width - 1.5f ? (1.1f, 3.2f) : (0f, 0f);
            Wall(new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin), "Wall_S", s => s is > 7f and < 10f ? (0f, 3.4f) : Windows(s));
            Wall(new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMax), "Wall_N", Windows);
            Wall(new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), "Wall_W", s => Mathf.Repeat(s, 4f) is > 1.5f and < 2.5f ? (1.2f, 3f) : (0f, 0f));
            Wall(new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax), "Wall_E", s => s is > 5f and < 9f ? (0f, 3.6f) : (0f, 0f));

            // Two roof girders left spanning the shell, one fallen in across the boiler.
            Material steel = ShowcaseMaterials.Steel;
            Box("Girder_1", house, new Vector3(r.center.x, 5.1f, r.yMin + 4f), new Vector3(r.width, .36f, .2f), steel);
            Box("Girder_2", house, new Vector3(r.center.x, 5.1f, r.yMax - 4f), new Vector3(r.width, .36f, .2f), steel);
            GameObject fallen = Box("Girder_Fallen", house, new Vector3(r.center.x + 2f, 2.1f, r.center.y), new Vector3(r.width * .8f, .36f, .2f), steel);
            fallen.transform.rotation = Quaternion.Euler(4f, 18f, -22f);

            // The boiler: a riveted drum on brick saddles, its firebox and a flue rising to the chimney's side.
            var boiler = new Vector3(r.center.x - 1.5f, 1.5f, r.center.y + 2.5f);
            GameObject drum = Primitive(PrimitiveType.Cylinder, "Boiler", house, boiler, new Vector3(2.4f, 3f, 2.4f), Rust);
            drum.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            for (int i = -1; i <= 1; i += 2)
                Box("Saddle", house, new Vector3(boiler.x + i * 1.8f, .3f, boiler.z), new Vector3(.5f, .6f, 2.2f), Brick);
            Box("Firebox", house, new Vector3(boiler.x + 3.6f, 1.1f, boiler.z), new Vector3(1.4f, 2.2f, 2f), Rust);
            GameObject flue = Primitive(PrimitiveType.Cylinder, "Flue", house, new Vector3(boiler.x - 2.6f, 3.6f, boiler.z), new Vector3(.6f, 2.4f, .6f), Rust);
            flue.transform.rotation = Quaternion.Euler(0f, 0f, 12f);

            // Rubble where the roof came down: tumbled brick and the scanned rocks, grassed over at the edges.
            Transform rubble = Group("Rubble", house);
            for (int i = 0; i < 26; i++)
            {
                float x = Mathf.Lerp(r.xMin + 1f, r.xMax - 1f, (float)random.NextDouble());
                float z = Mathf.Lerp(r.yMin + 1f, r.yMax - 1f, (float)random.NextDouble());
                if (Vector2.Distance(new Vector2(x, z), new Vector2(boiler.x, boiler.z)) < 3.5f) continue;
                float s = Mathf.Lerp(.25f, .7f, (float)random.NextDouble());
                GameObject chunk = Box("Bricks", rubble, new Vector3(x, s * .3f, z), new Vector3(s * 1.6f, s * .6f, s), Brick, false);
                chunk.transform.rotation = Quaternion.Euler((float)random.NextDouble() * 30f, (float)random.NextDouble() * 360f, (float)random.NextDouble() * 25f);
            }
            for (int i = 0; i < 5; i++)
                ShowcaseNature.Part("rock_moss_set_01", $"rock_moss_set_01_rock0{i + 1}", rubble,
                    new Vector3(r.xMin + 3f + i * 3.1f, Ground, r.yMin + 2f + (i % 2) * 9f), i * 71f, .3f);
            // Nature has the run of it: young firs inside the walls.
            ShowcaseNature.Tree("fir_sapling_medium", "fir_sapling_medium_a", house, new Vector3(r.xMin + 3f, Ground, r.yMax - 3f), 40f, 1.1f);
            ShowcaseNature.Tree("fir_sapling_medium", "fir_sapling_medium_c", house, new Vector3(r.xMax - 4f, Ground, r.yMin + 3.5f), 120f, .8f);
        }

        // A square brick stack, stepping in as it rises, a concrete band at the top.
        private static void Chimney(Transform chimney)
        {
            const int segments = 8;
            const float height = 26f;
            for (int i = 0; i < segments; i++)
            {
                float w = Mathf.Lerp(2.7f, 1.9f, i / (segments - 1f));
                float h = height / segments;
                Box($"Stack_{i}", chimney, ShowcaseSitePlan.Chimney + new Vector3(0f, h * (i + .5f), 0f), new Vector3(w, h + .002f, w), Brick);
            }
            Box("Band", chimney, ShowcaseSitePlan.Chimney + new Vector3(0f, height + .2f, 0f), new Vector3(2.15f, .4f, 2.15f), ShowcaseMaterials.Sill);
            Box("Plinth", chimney, ShowcaseSitePlan.Chimney + new Vector3(0f, .6f, 0f), new Vector3(3.3f, 1.2f, 3.3f), ShowcaseMaterials.CastConcrete);
        }

        // Shipping containers, 6.06 x 2.44 x 2.59 m: a block of them stacked two high with lanes between, and strays.
        private static void Containers(Transform containers)
        {
            (Vector3 at, float yaw, int finish)[] list =
            {
                (new(33.5f, 0f, -34f), 0f, 0), (new(33.5f, 2.59f, -34f), 0f, 2), (new(36f, 0f, -34f), 0f, 1), (new(33.5f, 0f, -27.8f), 0f, 3),
                (new(36f, 0f, -27.8f), 0f, 0), (new(36f, 2.59f, -27.8f), 0f, 3), (new(38.5f, 0f, -34f), 0f, 2),
                (new(46f, 0f, -24f), 90f, 1), (new(46f, 0f, -21.4f), 90f, 2), (new(46f, 2.59f, -22.7f), 87f, 0), (new(52.2f, 0f, -24f), 90f, 3),
                (new(56.5f, 0f, -10f), 0f, 0), (new(56.5f, 0f, -3.8f), 0f, 1), (new(56.5f, 2.59f, -6.9f), 0f, 2), (new(56.5f, 0f, 2.4f), 0f, 3),
                (new(43f, 0f, 8f), 24f, 1), (new(40.5f, 0f, -42f), 90f, 0),
            };
            // The scans are freshly painted; tinted down, they look like boxes that have stood out for years.
            Material[] finishes = { ContainerFinish("container_side", "Green", new Color(.62f, .68f, .62f)),
                ContainerFinish("box_profile_metal_sheet", "Red", new Color(.78f, .7f, .66f)),
                ContainerFinish("corrugated_iron_02", "Grey", new Color(.9f, .9f, .88f)),
                ContainerFinish("factory_wall", "Teal", new Color(.55f, .62f, .64f)) };
            for (int i = 0; i < list.Length; i++)
            {
                (Vector3 at, float yaw, int finish) = list[i];
                var holder = Group($"Container_{i}", containers);
                holder.SetPositionAndRotation(at + Vector3.up * Ground, Quaternion.Euler(0f, yaw, 0f));
                Box("Body", holder, new Vector3(0f, 1.295f, 0f), new Vector3(2.44f, 2.59f, 6.06f), finishes[finish]);
                // Door end: four locking bars and the corner castings.
                for (int b = 0; b < 4; b++)
                    Box("LockBar", holder, new Vector3(-.9f + b * .6f, 1.295f, 3.045f), new Vector3(.04f, 2.3f, .04f), ShowcaseMaterials.Steel, false);
                foreach (float x in new[] { -1.15f, 1.15f })
                foreach (float y in new[] { .08f, 2.51f })
                foreach (float z in new[] { -2.96f, 2.96f })
                    Box("Casting", holder, new Vector3(x, y, z), new Vector3(.18f, .16f, .18f), ShowcaseMaterials.Steel, false);
            }
        }

        private static Material ContainerFinish(string scan, string colour, Color tint)
        {
            string folder = ShowcaseTerrain.GroundFolder + "/" + scan;
            Material material = ShowcaseMaterials.Plain($"TestMap_Container_{colour}", Color.white, .4f);
            ShowcaseNature.Setup(material, ShowcaseNature.Texture(folder + "_diff", true, false, false),
                ShowcaseNature.Texture(folder + "_nor_gl", false, true, false), ShowcaseNature.Texture(folder + "_mask", false, false, false), false);
            // A cube face is one UV square: about three repeats of the corrugation along a container's side.
            material.SetTextureScale("_BaseMap", new Vector2(3f, 1.2f));
            material.SetColor("_BaseColor", tint);
            return material;
        }

        private static void WaterTower(Transform tower)
        {
            Vector3 c = ShowcaseSitePlan.WaterTower;
            const float legsTop = 14f, spread = 2.9f, foot = 3.4f;
            Material steel = ShowcaseMaterials.Steel;
            var tops = new Vector3[4];
            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 1.41421f;
                Vector3 bottom = c + dir * foot, top = c + dir * spread + Vector3.up * legsTop;
                tops[i] = top;
                Strut(tower, "Leg", bottom, top, .32f, steel);
                Box("Footing", tower, bottom + Vector3.up * .2f, new Vector3(.9f, .4f, .9f), ShowcaseMaterials.CastConcrete);
            }
            // Cross bracing on every face, at two heights.
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                for (int level = 0; level < 2; level++)
                {
                    float t0 = .1f + level * .45f, t1 = t0 + .4f;
                    Vector3 Leg(int k, float t) => Vector3.Lerp(c + (tops[k] - c - Vector3.up * legsTop) * (foot / spread), tops[k], t);
                    Strut(tower, "Brace", Leg(i, t0), Leg(j, t1), .07f, steel);
                    Strut(tower, "Brace", Leg(j, t0), Leg(i, t1), .07f, steel);
                    Strut(tower, "Tie", Leg(i, t1), Leg(j, t1), .09f, steel);
                }
            }
            Primitive(PrimitiveType.Cylinder, "Tank", tower, c + Vector3.up * (legsTop + 2.9f), new Vector3(6.6f, 2.9f, 6.6f), Rust);
            Primitive(PrimitiveType.Cylinder, "Walkway", tower, c + Vector3.up * (legsTop + .05f), new Vector3(8f, .05f, 8f), steel);
            Primitive(PrimitiveType.Cylinder, "Roof", tower, c + Vector3.up * (legsTop + 6f), new Vector3(6.9f, .2f, 6.9f), Rust);
            Primitive(PrimitiveType.Cylinder, "Cap", tower, c + Vector3.up * (legsTop + 6.5f), new Vector3(3.2f, .35f, 3.2f), Rust);
            Primitive(PrimitiveType.Cylinder, "Finial", tower, c + Vector3.up * (legsTop + 7.1f), new Vector3(.5f, .3f, .5f), Rust);
            // Railing round the walkway, and a ladder up the south leg.
            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2f / 24f;
                Box("Stanchion", tower, c + new Vector3(Mathf.Cos(a) * 3.9f, legsTop + .55f, Mathf.Sin(a) * 3.9f), new Vector3(.05f, 1f, .05f), steel, false);
            }
            Primitive(PrimitiveType.Cylinder, "HandRail", tower, c + Vector3.up * (legsTop + 1.05f), new Vector3(7.85f, .02f, 7.85f), steel, false);
            Vector3 ladder = c + new Vector3(0f, 0f, -spread - .6f);
            for (int s = -1; s <= 1; s += 2)
                Box("LadderRail", tower, ladder + new Vector3(s * .22f, legsTop / 2f, 0f), new Vector3(.05f, legsTop, .05f), steel, false);
            for (float y = .3f; y < legsTop; y += .3f)
                Box("Rung", tower, ladder + new Vector3(0f, y, 0f), new Vector3(.44f, .03f, .03f), steel, false);
        }

        private static void Strut(Transform parent, string name, Vector3 a, Vector3 b, float thick, Material material)
        {
            GameObject strut = Box(name, parent, (a + b) / 2f, new Vector3(thick, thick, Vector3.Distance(a, b)), material);
            strut.transform.rotation = Quaternion.LookRotation(b - a);
        }

        // An open lean-to on steel posts against the south fence, its corrugated roof sloping back.
        private static void LeanTo(Transform shed)
        {
            const float x0 = 44f, x1 = 58f, z0 = -50f, z1 = -44f;
            for (float x = x0; x <= x1 + .01f; x += 3.5f)
            {
                Box("Post_Front", shed, new Vector3(x, 1.7f, z1), new Vector3(.15f, 3.4f, .15f), ShowcaseMaterials.Steel);
                Box("Post_Back", shed, new Vector3(x, 1.4f, z0), new Vector3(.15f, 2.8f, .15f), ShowcaseMaterials.Steel);
            }
            GameObject roof = Box("Roof", shed, new Vector3((x0 + x1) / 2f, 3.15f, (z0 + z1) / 2f), new Vector3(x1 - x0 + .8f, .06f, z1 - z0 + .8f), Cladding);
            roof.transform.rotation = Quaternion.Euler(-Mathf.Atan2(.6f, z1 - z0) * Mathf.Rad2Deg, 0f, 0f);
            roof.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
            Box("Beam_Front", shed, new Vector3((x0 + x1) / 2f, 3.4f, z1), new Vector3(x1 - x0 + .2f, .2f, .12f), ShowcaseMaterials.Steel);
            Box("Beam_Back", shed, new Vector3((x0 + x1) / 2f, 2.8f, z0), new Vector3(x1 - x0 + .2f, .2f, .12f), ShowcaseMaterials.Steel);
        }
    }
}
