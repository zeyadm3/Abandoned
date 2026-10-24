using Abandoned.Core;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The downtown's street grid: eight avenues along x (Broadway, at the tower's front, leaves the plaza's own street
    /// alone) and eight cross streets stopping at them, so no two road surfaces overlap; a kerbed pavement filling every
    /// block to the road's edge; centre dashes, edge lines and zebras on every arm of every junction, merged into one
    /// saved mesh. The tower's own block keeps its saved pavements; only thin strips fill it out to the kerb line.
    /// </summary>
    public static class SkyscraperDowntownStreets
    {
        public const float Half = 7f;

        /// <summary>Centre line of avenue <paramref name="n"/> (-3..4; 0 is Broadway).</summary>
        public static float AvenueZ(int n) => n * 100f - 28f;

        /// <summary>Centre line of cross street <paramref name="m"/> (-3..4).</summary>
        public static float StreetX(int m) => m * 100f - 50f;

        public static void Build(Transform city, SkyscraperArt a, SkyscraperDowntownMaterials p, StationRingStock stock)
        {
            Transform g = Group("StreetGrid", city);
            Rect r = SkyscraperDowntown.Ground;
            Box("DowntownGround", g, new Vector3(0, -.43f, 22), new Vector3(r.width, .5f, r.height), a.Asphalt);
            for (int n = -3; n <= 4; n++)
            {
                float z = AvenueZ(n);
                if (n == 0)
                {
                    Road(g, "BroadwayWest", Rect.MinMaxRect(r.xMin, z - Half, -24f, z + Half), a);
                    Road(g, "BroadwayEast", Rect.MinMaxRect(24f, z - Half, r.xMax, z + Half), a);
                }
                else Road(g, "Avenue", Rect.MinMaxRect(r.xMin, z - Half, r.xMax, z + Half), a);
            }
            for (int m = -3; m <= 4; m++)
            {
                float x = StreetX(m), from = r.yMin;
                for (int n = -3; n <= 4; n++)
                {
                    Road(g, "CrossStreet", Rect.MinMaxRect(x - Half, from, x + Half, AvenueZ(n) - Half), a);
                    from = AvenueZ(n) + Half;
                }
                Road(g, "CrossStreet", Rect.MinMaxRect(x - Half, from, x + Half, r.yMax), a);
            }
            for (int z = -3; z <= 3; z++)
            for (int x = -3; x <= 3; x++)
            {
                if (x == 0 && z == 0) continue;
                Pavement(g, "BlockPavement", new Vector3(x * 100f, -.075f, z * 100f + 22f), new Vector3(86f, .15f, 86f), p);
            }
            // The tower's block as saved (its plaza is the tower's), filled out only at the edges to the kerb line.
            Pavement(g, "HomeBlockNorth", new Vector3(0, -.075f, 43), new Vector3(82, .15f, 42), p);
            foreach (float side in new[] { -1f, 1f })
            {
                Pavement(g, "HomeBlockSide", new Vector3(side * 33.5f, -.075f, 0), new Vector3(15, .15f, 44), p);
                Pavement(g, "HomeBlockEdge", new Vector3(side * 42f, -.075f, 22f), new Vector3(2f, .15f, 86f), p);
            }
            Pavement(g, "HomeBlockEdgeNorth", new Vector3(0, -.075f, 64.5f), new Vector3(82f, .15f, 1f), p);
            int marks = stock.Add("DowntownMarkings", r.width, r.height, t => Markings(t, a));
            Transform painted = stock.Place(city, marks, Vector3.zero, 0f);
            painted.name = "StreetMarkings";
            // Paint on the road hides nothing: the builder's occlusion pass leaves floors out by name.
            foreach (Renderer r in painted.GetComponentsInChildren<Renderer>(true)) r.gameObject.name = "StreetMarkingsFloor";
        }

        private static void Road(Transform g, string name, Rect r, SkyscraperArt a)
        {
            if (r.width < .05f || r.height < .05f) return;
            GameObject road = Box(name, g, new Vector3(r.center.x, -.12f, r.center.y), new Vector3(r.width, .12f, r.height), a.Asphalt);
            road.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Asphalt);
        }

        private static void Pavement(Transform g, string name, Vector3 at, Vector3 size, SkyscraperDowntownMaterials p) =>
            Box(name, g, at, size, p.Sidewalk).AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Concrete);

        // Everything painted or set into the streets, in world coordinates (the design is placed at the origin).
        private static void Markings(Transform t, SkyscraperArt a)
        {
            const float paint = SkyscraperDowntown.StreetTop + .006f;
            Rect r = SkyscraperDowntown.Ground;
            for (int n = -3; n <= 4; n++)
            {
                float z = AvenueZ(n);
                for (float x = r.xMin + 6f; x < r.xMax - 4f; x += 8f)
                {
                    if (Near(x, true, 10f) || n == 0 && Mathf.Abs(x) < 28f) continue;
                    Box("CentreDash", t, new Vector3(x, paint, z), new Vector3(4f, .01f, .13f), a.RoadYellow, false);
                }
                for (int m = -3; m <= 3; m++)
                {
                    float x0 = StreetX(m) + Half + 1.5f, x1 = StreetX(m + 1) - Half - 1.5f;
                    if (n == 0 && x0 < 24f && x1 > -24f) continue;
                    foreach (float s in new[] { -1f, 1f })
                        Box("EdgeLine", t, new Vector3((x0 + x1) * .5f, paint, z + s * (Half - .5f)), new Vector3(x1 - x0, .01f, .12f), a.RoadWhite, false);
                }
            }
            for (int m = -3; m <= 4; m++)
            {
                float x = StreetX(m);
                for (float z = r.yMin + 6f; z < r.yMax - 4f; z += 8f)
                    if (!Near(z, false, 10f)) Box("CentreDash", t, new Vector3(x, paint, z), new Vector3(.13f, .01f, 4f), a.RoadYellow, false);
            }
            // Zebras on all four arms of every junction (none across the plaza's own street).
            for (int m = -3; m <= 4; m++)
            for (int n = -3; n <= 4; n++)
            {
                float x = StreetX(m), z = AvenueZ(n);
                foreach (float s in new[] { -1f, 1f })
                {
                    float ax = x + s * (Half + 2.4f);
                    if (!(n == 0 && Mathf.Abs(ax) < 26f))
                        for (float k = -Half + .9f; k < Half - .5f; k += 1.1f)
                            Box("Zebra", t, new Vector3(ax, paint + .001f, z + k), new Vector3(3f, .01f, .55f), a.RoadWhite, false);
                    float az = z + s * (Half + 2.4f);
                    for (float k = -Half + .9f; k < Half - .5f; k += 1.1f)
                        Box("Zebra", t, new Vector3(x + k, paint + .001f, az), new Vector3(.55f, .01f, 3f), a.RoadWhite, false);
                }
            }
            // A granite kerb round every block, a hair inside the pavement's edge so no faces share a plane.
            for (int bz = -3; bz <= 3; bz++)
            for (int bx = -3; bx <= 3; bx++)
            {
                if (bx == 0 && bz == 0) continue;
                float cx = bx * 100f, cz = bz * 100f + 22f;
                foreach (float s in new[] { -1f, 1f })
                {
                    Box("Kerb", t, new Vector3(cx, .006f, cz + s * 42.83f), new Vector3(86f - .08f, .024f, .3f), a.GraniteDark, false);
                    Box("Kerb", t, new Vector3(cx + s * 42.83f, .006f, cz), new Vector3(.3f, .024f, 86f - .7f), a.GraniteDark, false);
                }
            }
        }

        // Within pad metres of a cross street (alongX) or an avenue (along z).
        private static bool Near(float u, bool alongX, float pad)
        {
            for (int i = -3; i <= 4; i++)
                if (Mathf.Abs(u - (alongX ? StreetX(i) : AvenueZ(i))) < pad) return true;
            return false;
        }
    }
}
