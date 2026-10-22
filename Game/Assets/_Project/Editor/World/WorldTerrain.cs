using System.Collections.Generic;
using Abandoned.Core;
using Abandoned.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The open world's land: one height grid for all six kilometres (two-metre samples), split into 6 x 6 Unity
    /// terrains that join up. Wooded hills (<see cref="WorldPlan.Natural"/>), each place's site levelled to its own
    /// ground with a hole where its scene's ground goes, and every road's bed graded smooth through the hills.
    /// Painted, planted and grassed by <see cref="WorldTerrainPaint"/>. Seeded: every rebuild is the same.
    /// </summary>
    public static class WorldTerrain
    {
        public const string Folder = "Assets/_Project/Art/Generated/World/Terrain";
        private const int TileRes = 513;
        public const float Cell = WorldPlan.TileSize / (TileRes - 1);
        public static readonly int Samples = WorldPlan.Tiles * (TileRes - 1) + 1;
        private const float SiteBlend = 180f, RoadReach = 18f;

        /// <summary>Heights (world metres) on the grid, [z, x]; filled by <see cref="Shape"/>.</summary>
        public static float[,] Heights { get; private set; }
        /// <summary>0..1 how much of each grid sample is road bed (for paint and planting).</summary>
        public static float[,] Road { get; private set; }

        public static float X(int ix) => -WorldPlan.Half + ix * Cell;
        public static float Z(int iz) => -WorldPlan.Half + iz * Cell;

        /// <summary>World height at (x, z) from the grid (bilinear).</summary>
        public static float HeightAt(float x, float z)
        {
            float fx = Mathf.Clamp((x + WorldPlan.Half) / Cell, 0f, Samples - 1.001f), fz = Mathf.Clamp((z + WorldPlan.Half) / Cell, 0f, Samples - 1.001f);
            int ix = (int)fx, iz = (int)fz;
            float tx = fx - ix, tz = fz - iz;
            return Mathf.Lerp(Mathf.Lerp(Heights[iz, ix], Heights[iz, ix + 1], tx), Mathf.Lerp(Heights[iz + 1, ix], Heights[iz + 1, ix + 1], tx), tz);
        }

        /// <summary>Shapes the land; returns each road's centre line with its bed height (x, bed, z).</summary>
        public static List<List<Vector3>> Shape(IReadOnlyList<WorldPoi> places, List<Vector2[]> roadPlans)
        {
            int n = Samples;
            var h = new float[n, n];
            for (int iz = 0; iz < n; iz++)
            for (int ix = 0; ix < n; ix++) h[iz, ix] = WorldPlan.Natural(X(ix), Z(iz));

            // Each site level with its scene's ground, blending back into the hills.
            foreach (WorldPoi p in places)
            {
                Rect r = WorldPlan.GroundRect(p);
                float y = WorldPlan.GroundY(p) - .05f;
                Stamp(r, SiteBlend, (ix, iz, d) => h[iz, ix] = Mathf.Lerp(h[iz, ix], y, 1f - ShowcaseSitePlan.Smooth(0f, SiteBlend, d)));
            }
            Heights = h;

            // Road beds: the land along each road, smoothed so it climbs gently, level with the sites at its ends.
            var roads = new List<List<Vector3>>();
            var weight = new float[n, n];
            var bed = new float[n, n];
            foreach (Vector2[] plan in roadPlans)
            {
                List<Vector2> line = WorldPlan.Smooth(plan, Cell);
                var raw = new float[line.Count];
                for (int i = 0; i < line.Count; i++) raw[i] = HeightAt(line[i].x, line[i].y);
                var road = new List<Vector3>(line.Count);
                const int window = 30;
                for (int i = 0; i < line.Count; i++)
                {
                    float sum = 0f;
                    int count = 0;
                    for (int j = Mathf.Max(0, i - window); j <= Mathf.Min(line.Count - 1, i + window); j++, count++) sum += raw[j];
                    float y = sum / count;
                    // The first and last stretches lie on the site ground exactly.
                    float ends = Mathf.Min(i, line.Count - 1 - i) * Cell;
                    y = Mathf.Lerp(raw[i], y, ShowcaseSitePlan.Smooth(40f, 110f, ends));
                    road.Add(new Vector3(line[i].x, y, line[i].y));
                }
                foreach (Vector3 p in road)
                {
                    int cx = Mathf.RoundToInt((p.x + WorldPlan.Half) / Cell), cz = Mathf.RoundToInt((p.z + WorldPlan.Half) / Cell);
                    int reach = Mathf.CeilToInt(RoadReach / Cell);
                    for (int iz = Mathf.Max(0, cz - reach); iz <= Mathf.Min(n - 1, cz + reach); iz++)
                    for (int ix = Mathf.Max(0, cx - reach); ix <= Mathf.Min(n - 1, cx + reach); ix++)
                    {
                        float d = Vector2.Distance(new Vector2(X(ix), Z(iz)), new Vector2(p.x, p.z));
                        float w = 1f - ShowcaseSitePlan.Smooth(WorldPlan.RoadHalfWidth + 1.5f, RoadReach, d);
                        if (w <= weight[iz, ix]) continue;
                        weight[iz, ix] = w;
                        bed[iz, ix] = p.y - .08f;
                    }
                }
                roads.Add(road);
            }
            var roadPaint = new float[n, n];
            for (int iz = 0; iz < n; iz++)
            for (int ix = 0; ix < n; ix++)
            {
                if (weight[iz, ix] <= 0f) continue;
                h[iz, ix] = Mathf.Lerp(h[iz, ix], bed[iz, ix], weight[iz, ix]);
                roadPaint[iz, ix] = ShowcaseSitePlan.Smooth(.55f, .95f, weight[iz, ix]);
            }
            for (int iz = 0; iz < n; iz++)
            for (int ix = 0; ix < n; ix++) h[iz, ix] = Mathf.Clamp(h[iz, ix], WorldPlan.Floor + .5f, WorldPlan.Floor + WorldPlan.Height - .5f);
            Road = roadPaint;
            return roads;
        }

        // Calls back for every grid sample within reach of a rectangle, with its distance outside it (0 inside).
        private static void Stamp(Rect r, float reach, System.Action<int, int, float> each)
        {
            int x0 = Mathf.Max(0, Mathf.FloorToInt((r.xMin - reach + WorldPlan.Half) / Cell)), x1 = Mathf.Min(Samples - 1, Mathf.CeilToInt((r.xMax + reach + WorldPlan.Half) / Cell));
            int z0 = Mathf.Max(0, Mathf.FloorToInt((r.yMin - reach + WorldPlan.Half) / Cell)), z1 = Mathf.Min(Samples - 1, Mathf.CeilToInt((r.yMax + reach + WorldPlan.Half) / Cell));
            for (int iz = z0; iz <= z1; iz++)
            for (int ix = x0; ix <= x1; ix++)
            {
                float dx = Mathf.Max(r.xMin - X(ix), 0f, X(ix) - r.xMax), dz = Mathf.Max(r.yMin - Z(iz), 0f, Z(iz) - r.yMax);
                float d = Mathf.Sqrt(dx * dx + dz * dz);
                if (d <= reach) each(ix, iz, d);
            }
        }

        /// <summary>Makes the terrain tiles from the shaped grid, under <paramref name="parent"/>.</summary>
        public static Terrain[] Tiles(Transform parent, IReadOnlyList<WorldPoi> places)
        {
            PolishAssets.EnsureFolder(Folder);
            var terrains = new Terrain[WorldPlan.Tiles * WorldPlan.Tiles];
            Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/TestMap/Terrain/TestMap_Terrain.mat");
            for (int tz = 0; tz < WorldPlan.Tiles; tz++)
            for (int tx = 0; tx < WorldPlan.Tiles; tx++)
            {
                string path = $"{Folder}/World_{tx}_{tz}.asset";
                // The asset first: alphamaps, trees and grass set on a TerrainData that isn't an asset yet are lost.
                AssetDatabase.DeleteAsset(path);
                var data = new TerrainData { heightmapResolution = TileRes };
                AssetDatabase.CreateAsset(data, path);
                data.size = new Vector3(WorldPlan.TileSize, WorldPlan.Height, WorldPlan.TileSize);
                var tile = new float[TileRes, TileRes];
                for (int iz = 0; iz < TileRes; iz++)
                for (int ix = 0; ix < TileRes; ix++)
                    tile[iz, ix] = (Heights[tz * (TileRes - 1) + iz, tx * (TileRes - 1) + ix] - WorldPlan.Floor) / WorldPlan.Height;
                data.SetHeights(0, 0, tile);
                data.SetHoles(0, 0, Holes(tx, tz, places));
                WorldTerrainPaint.Paint(data, tx, tz, places);
                EditorUtility.SetDirty(data);

                GameObject go = Terrain.CreateTerrainGameObject(data);
                go.name = $"Terrain_{tx}_{tz}";
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(-WorldPlan.Half + tx * WorldPlan.TileSize, WorldPlan.Floor, -WorldPlan.Half + tz * WorldPlan.TileSize);
                var terrain = go.GetComponent<Terrain>();
                terrain.materialTemplate = material;
                terrain.drawInstanced = true;
                terrain.heightmapPixelError = 4f;
                terrain.basemapDistance = 250f;
                terrain.treeDistance = 700f;
                terrain.treeBillboardDistance = 700f; // the scanned trees carry their own impostor LOD
                terrain.detailObjectDistance = 100f;
                terrain.shadowCastingMode = ShadowCastingMode.On;
                terrain.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
                terrain.allowAutoConnect = true;
                terrain.groupingID = 7;
                go.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
                terrains[tz * WorldPlan.Tiles + tx] = terrain;
            }
            AssetDatabase.SaveAssets();
            return terrains;
        }

        // Holes over each built place's ground, so its scene's own ground and basements show through (true = solid).
        private static bool[,] Holes(int tx, int tz, IReadOnlyList<WorldPoi> places)
        {
            int res = TileRes - 1;
            var solid = new bool[res, res];
            for (int iz = 0; iz < res; iz++)
            for (int ix = 0; ix < res; ix++)
            {
                float x = X(tx * res + ix) + Cell * .5f, z = Z(tz * res + iz) + Cell * .5f;
                bool hole = false;
                foreach (WorldPoi p in places)
                {
                    Rect r = WorldPlan.GroundRect(p);
                    if (x > r.xMin + 1f && x < r.xMax - 1f && z > r.yMin + 1f && z < r.yMax - 1f) { hole = true; break; }
                }
                solid[iz, ix] = !hole;
            }
            return solid;
        }
    }
}
