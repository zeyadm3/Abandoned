using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// One downtown high-rise: a stone podium with shopfronts, awnings, lit sign bands and revolving doors on all four
    /// sides; a curtain-wall shaft and its set-back upper tier, each with mullion fins and spandrel bands in its own saved
    /// mesh; and one of four crowns (a stepped glass crown with a spire, an art-deco lantern, a blade with a mast and a
    /// helipad, or stone fins over water tanks and a roof garden). Beacons on the tall ones, a window-cleaning cradle on
    /// some. One solid footprint keeps everyone out of the shell.
    /// </summary>
    public static class SkyscraperDowntownBuildings
    {
        public static void Tower(Transform city, SkyscraperArt a, SkyscraperDowntownMaterials p, Vector3 at, float height, int seed)
        {
            Transform g = Group($"CityTower_{seed}_{height:0}m", city);
            g.position = at;
            int style = seed % 4;
            float width = 32 + seed % 5 * 3, depth = 34 + seed % 4 * 4;
            Podium(g, a, p, style, seed);
            float setback = style == 1 ? height * .72f : height - 16;
            Material trim = style == 1 ? a.Brass : a.Steel;
            Body(g, p.Facades[style], width, depth, 10, setback - 10, seed, 0);
            Fins(g, trim, width, depth, 10, setback - 10, seed, 0);
            Box("SetbackTerrace", g, new Vector3(0, setback, 0), new Vector3(width, .22f, depth), a.GraniteDark, false);
            Box("TerraceParapet", g, new Vector3(0, setback + .55f, 0), new Vector3(width + .2f, .9f, depth + .2f), a.GraniteDark, false);
            Body(g, p.Facades[style], width * .78f, depth * .78f, setback, height - setback, seed, 1);
            Fins(g, trim, width * .78f, depth * .78f, setback, height - setback, seed, 1);
            var solid = g.gameObject.AddComponent<BoxCollider>();
            solid.center = new Vector3(0, height * .5f, 0);
            solid.size = new Vector3(width, height, depth);
            Crown(g, a, p, style, width * .78f, depth * .78f, height, seed);
            foreach (float side in new[] { -1f, 1f })
                Box("CornerFin", g, new Vector3(side * (width * .5f + .1f), setback * .5f, 0), new Vector3(.35f, setback, .45f), trim, false);
            if (height > 300f)
                foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    Box("CornerBeacon", g, new Vector3(sx * width * .37f, height + .4f, sz * depth * .37f), new Vector3(.3f, .3f, .3f), a.RedEmergencyEmissive, false);
            if (seed % 5 == 0) Cradle(g, a, width * .78f, depth * .78f, height, setback, seed);
        }

        // The podium: stone mass, shopfronts and sign bands on all four faces, awnings, a revolving door, planters.
        private static void Podium(Transform g, SkyscraperArt a, SkyscraperDowntownMaterials p, int style, int seed)
        {
            Box("CityBlockPodium", g, new Vector3(0, 5, 0), new Vector3(64, 10, 62), style % 2 == 0 ? p.Limestone : a.GraniteDark);
            for (int face = 0; face < 4; face++)
            {
                bool alongX = face < 2;
                float s = face % 2 == 0 ? -1f : 1f, half = alongX ? 31f : 32f, span = alongX ? 58f : 56f;
                Vector3 Out(float u, float y, float o) => alongX ? new Vector3(u, y, s * (half + o)) : new Vector3(s * (half + o), y, u);
                Vector3 Size(float along, float up, float thick) => alongX ? new Vector3(along, up, thick) : new Vector3(thick, up, along);
                Box("Shopfront", g, Out(0, 2.8f, .05f), Size(span, 4.8f, .08f), p.Shop, false);
                Box("SignBand", g, Out(0, 5.9f, .12f), Size(span, .9f, .1f), (seed + face) % 3 == 0 ? p.WindowLight : a.GraniteDark, false);
                for (int k = -3; k <= 3; k++)
                    Box("ShopPier", g, Out(k * 9, 3, .2f), Size(.5f, 6, .4f), p.Limestone, false);
                Box("PodiumCornice", g, Out(0, 9.5f, .3f), Size(alongX ? 65f : 61.4f, .8f, .7f), a.GraniteDark, false); // the side runs stop inside the front ones
                Box("PodiumCopingLine", g, Out(0, 7.4f, .12f), Size(span + 2f, .18f, .2f), a.Brass, false);
                for (int k = -2; k <= 2; k++)
                {
                    if (k == 0) continue;
                    Material awning = (seed + k + face) % 3 == 0 ? a.CarPaintBurgundy : (seed + k) % 3 == 1 ? a.GraniteDark : a.CarPaintBlack;
                    GameObject canvas = Box("Awning", g, Out(k * 13.5f, 4.6f, .9f), Size(7f, .1f, 1.8f), awning, false);
                    canvas.transform.localRotation = Quaternion.Euler(alongX ? s * 12f : 0f, 0f, alongX ? 0f : -s * 12f);
                }
                if (alongX)
                {
                    Box("EntranceCanopy", g, Out(0, 4.5f, 2f), Size(10, .25f, 4), a.Steel, false);
                    Primitive(PrimitiveType.Cylinder, "RevolvingDoor", g, Out(0, 1.2f, .9f), new Vector3(2.6f, 1.2f, 2.6f), a.Glass, false);
                    Box("DoorHood", g, Out(0, 2.5f, .9f), new Vector3(2.8f, .15f, 2.8f), a.Brass, false);
                    foreach (float u in new[] { -8f, 8f })
                    {
                        Box("Planter", g, Out(u, .45f, 2.2f), new Vector3(2.2f, .9f, 1.2f), a.GraniteDark);
                        Box("PlanterSoil", g, Out(u, .91f, 2.2f), new Vector3(2f, .03f, 1f), a.Mahogany, false);
                    }
                }
            }
        }

        private static void Crown(Transform g, SkyscraperArt a, SkyscraperDowntownMaterials p, int style, float w, float d, float h, int seed)
        {
            Box("TowerRoof", g, new Vector3(0, h + .15f, 0), new Vector3(w * .99f, .3f, d * .99f), a.GraniteDark, false);
            switch (style)
            {
                case 0: // stepped glass crown and spire
                    Box("CrownStep1", g, new Vector3(0, h + 4f, 0), new Vector3(w * .7f, 8f, d * .7f), a.Glass, false);
                    Box("CrownStep2", g, new Vector3(0, h + 11f, 0), new Vector3(w * .45f, 6f, d * .45f), a.Glass, false);
                    Box("CrownBand", g, new Vector3(0, h + 8.2f, 0), new Vector3(w * .71f, .4f, d * .71f), p.WindowLight, false);
                    Primitive(PrimitiveType.Cylinder, "Spire", g, new Vector3(0, h + 26f, 0), new Vector3(.9f, 12f, .9f), a.Steel, false);
                    Box("SpireBeacon", g, new Vector3(0, h + 38.3f, 0), new Vector3(.4f, .5f, .4f), a.RedEmergencyEmissive, false);
                    break;
                case 1: // art-deco lantern
                    for (int i = 0; i < 3; i++)
                        Box($"DecoTier{i}", g, new Vector3(0, h + 2.5f + i * 5f, 0), new Vector3(w * (.8f - i * .2f), 5f, d * (.8f - i * .2f)), p.Limestone, false);
                    Box("Lantern", g, new Vector3(0, h + 18f, 0), new Vector3(w * .16f, 6f, d * .16f), p.WindowLight, false);
                    for (int i = -2; i <= 2; i++)
                        Box("DecoRib", g, new Vector3(i * w * .14f, h + 7.5f, d * .4f + .1f), new Vector3(.4f, 15f, .3f), a.Brass, false);
                    Primitive(PrimitiveType.Cylinder, "DecoNeedle", g, new Vector3(0, h + 27f, 0), new Vector3(.5f, 6f, .5f), a.Brass, false);
                    break;
                case 2: // blade, mast and helipad
                    Box("CrownBlade", g, new Vector3(0, h + 4, -d * .3f), new Vector3(w * .5f, 8, 1), a.Steel, false);
                    Box("Antenna", g, new Vector3(0, h + 14, -d * .3f), new Vector3(.25f, 20, .25f), a.Steel, false);
                    Primitive(PrimitiveType.Cylinder, "Helipad", g, new Vector3(0, h + .55f, d * .15f), new Vector3(w * .5f, .1f, w * .5f), a.Concrete, false);
                    Box("HelipadH_L", g, new Vector3(-1.4f, h + .66f, d * .15f), new Vector3(.6f, .02f, 4.2f), a.RoadYellow, false);
                    Box("HelipadH_R", g, new Vector3(1.4f, h + .66f, d * .15f), new Vector3(.6f, .02f, 4.2f), a.RoadYellow, false);
                    Box("HelipadH_Bar", g, new Vector3(0, h + .67f, d * .15f), new Vector3(2.8f, .02f, .6f), a.RoadYellow, false);
                    break;
                default: // stone fins over water tanks and a roof garden
                    foreach (float x in new[] { -w * .25f, w * .25f })
                        Box("CrownFin", g, new Vector3(x, h + 3, 0), new Vector3(.5f, 6, d * .8f), p.Limestone, false);
                    foreach (float x in new[] { -w * .12f, w * .12f })
                    {
                        Primitive(PrimitiveType.Cylinder, "WaterTank", g, new Vector3(x, h + 4.5f, -d * .2f), new Vector3(4f, 2.2f, 4f), a.Mahogany, false);
                        Box("TankStand", g, new Vector3(x, h + 1.2f, -d * .2f), new Vector3(3.2f, 2.2f, 3.2f), a.Steel, false);
                    }
                    Box("RoofGarden", g, new Vector3(0, h + .45f, d * .22f), new Vector3(w * .4f, .3f, d * .3f), a.Mahogany, false);
                    break;
            }
            foreach (float x in new[] { -6f, 6f })
            {
                Box("RoofPlant", g, new Vector3(x, h + 1.1f, 3), new Vector3(4, 2, 6), a.Steel, false);
                Primitive(PrimitiveType.Cylinder, "CoolingFan", g, new Vector3(x, h + 2.2f, 3), new Vector3(2, .1f, 2), a.Rubber, false);
            }
            Box("AviationBeacon", g, new Vector3(0, h + 3, -d * .45f), new Vector3(.25f, .3f, .25f), a.RedEmergencyEmissive, false);
        }

        // A window-cleaning cradle left hanging half-way down the south face, its davit on the roof.
        private static void Cradle(Transform g, SkyscraperArt a, float w, float d, float h, float setback, int seed)
        {
            float y = setback + (h - setback) * (.3f + seed % 3 * .15f), z = -d * .5f - .9f, x = (seed % 2 == 0 ? -1f : 1f) * w * .2f;
            Box("CradleDeck", g, new Vector3(x, y, z), new Vector3(5f, .15f, .9f), a.Steel, false);
            Box("CradleRail", g, new Vector3(x, y + .9f, z - .42f), new Vector3(5f, .06f, .06f), a.CarPaintYellow, false);
            foreach (float s in new[] { -2.4f, 2.4f })
            {
                Box("CradleCable", g, new Vector3(x + s, (y + h) * .5f + .5f, z + .3f), new Vector3(.03f, h - y, .03f), a.Steel, false);
                Box("CradleEnd", g, new Vector3(x + s, y + .5f, z), new Vector3(.08f, 1f, .9f), a.CarPaintYellow, false);
            }
            Box("Davit", g, new Vector3(x, h + 1.4f, -d * .5f + .5f), new Vector3(6f, .35f, 2.4f), a.CarPaintYellow, false);
        }

        // The curtain wall: four faces textured with the style's window atlas, saved per tower and tier.
        private static void Body(Transform parent, Material material, float width, float depth, float y, float height, int seed, int tier)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var indices = new List<int>();
            void Face(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float span)
            {
                int n = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                float offset = seed % 8;
                uv.AddRange(new[] { new Vector2(offset, 0), new Vector2(offset + span / 12, 0), new Vector2(offset + span / 12, height / 32), new Vector2(offset, height / 32) });
                indices.AddRange(new[] { n, n + 2, n + 1, n, n + 3, n + 2 });
            }
            float x = width * .5f, z = depth * .5f, t = y + height;
            Face(new Vector3(-x, y, -z), new Vector3(x, y, -z), new Vector3(x, t, -z), new Vector3(-x, t, -z), width);
            Face(new Vector3(x, y, z), new Vector3(-x, y, z), new Vector3(-x, t, z), new Vector3(x, t, z), width);
            Face(new Vector3(x, y, -z), new Vector3(x, y, z), new Vector3(x, t, z), new Vector3(x, t, -z), depth);
            Face(new Vector3(-x, y, z), new Vector3(-x, y, -z), new Vector3(-x, t, -z), new Vector3(-x, t, z), depth);
            Keep(parent, "TowerFacade", $"CityTower_{seed}_{tier}", vertices, uv, indices, material);
        }

        // Mullion fins every few metres up each face and a spandrel band every four floors, in one saved mesh.
        private static void Fins(Transform parent, Material material, float width, float depth, float y, float height, int seed, int tier)
        {
            var vertices = new List<Vector3>();
            var uv = new List<Vector2>();
            var indices = new List<int>();
            void Prism(Vector3 min, Vector3 max)
            {
                Vector3 c = (min + max) * .5f, e = (max - min) * .5f;
                foreach (Vector3 n in new[] { Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back })
                {
                    Vector3 u = Mathf.Abs(n.y) > .5f ? Vector3.right : Vector3.up, v = Vector3.Cross(n, u);
                    Vector3 o = c + Vector3.Scale(n, e), du = Vector3.Scale(u, e), dv = Vector3.Scale(v, e);
                    int i = vertices.Count;
                    vertices.AddRange(new[] { o - du - dv, o + du - dv, o + du + dv, o - du + dv });
                    uv.AddRange(new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up });
                    indices.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
                }
            }
            float hx = width * .5f, hz = depth * .5f, top = y + height;
            float pitch = 3f + seed % 3;
            for (float u = -hx + pitch; u < hx - .5f; u += pitch)
                foreach (float s in new[] { -1f, 1f })
                    Prism(new Vector3(u - .12f, y, s > 0 ? hz - .02f : -hz - .3f), new Vector3(u + .12f, top, s > 0 ? hz + .3f : -hz + .02f));
            for (float u = -hz + pitch; u < hz - .5f; u += pitch)
                foreach (float s in new[] { -1f, 1f })
                    Prism(new Vector3(s > 0 ? hx - .02f : -hx - .3f, y, u - .12f), new Vector3(s > 0 ? hx + .3f : -hx + .02f, top, u + .12f));
            for (float b = y + 16f; b < top - 2f; b += 16f)
            {
                Prism(new Vector3(-hx - .34f, b - .5f, -hz - .34f), new Vector3(hx + .34f, b + .5f, -hz + .03f));
                Prism(new Vector3(-hx - .34f, b - .5f, hz - .03f), new Vector3(hx + .34f, b + .5f, hz + .34f));
                Prism(new Vector3(-hx - .34f, b - .5f, -hz + .03f), new Vector3(-hx + .03f, b + .5f, hz - .03f));
                Prism(new Vector3(hx - .03f, b - .5f, -hz + .03f), new Vector3(hx + .34f, b + .5f, hz - .03f));
            }
            Keep(parent, "TowerFins", $"CityTowerFins_{seed}_{tier}", vertices, uv, indices, material);
        }

        private static void Keep(Transform parent, string name, string asset, List<Vector3> vertices, List<Vector2> uv, List<int> indices, Material material)
        {
            var mesh = new Mesh { name = asset, indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            string path = SkyscraperDowntownMaterials.Folder + "/" + mesh.name + ".asset";
            Mesh saved = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (saved == null)
            {
                saved = mesh;
                AssetDatabase.CreateAsset(saved, path);
            }
            else
            {
                EditorUtility.CopySerialized(mesh, saved);
                Object.DestroyImmediate(mesh);
            }
            Transform g = Group(name, parent);
            g.gameObject.AddComponent<MeshFilter>().sharedMesh = saved;
            g.gameObject.AddComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
