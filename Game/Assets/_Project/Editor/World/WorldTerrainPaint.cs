using System.Collections.Generic;
using Abandoned.World;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Paints, plants and grasses one world terrain tile with the TestMap's own scanned ground layers, trees and grass
    /// (read from its saved terrain, never changed): forest floor under the trees, grass and meadow in the clearings,
    /// bare dirt and stone where it is steep, gravel and dirt along the road shoulders, nothing on the road or sites.
    /// </summary>
    public static class WorldTerrainPaint
    {
        private const string TestMapTerrain = "Assets/_Project/Art/TestMap/Terrain/";
        private const int SplatRes = 256, DetailRes = 256;
        private enum Layer { Grass, Meadow, ForestFloor, Dirt, Stone, Litter }
        private static readonly string[] LayerScans = { "forrest_ground_01", "grass_ground", "forest_leaves_02", "forest_ground_04", "rocky_trail", "brown_mud_leaves_01" };

        public static void Paint(TerrainData data, int tx, int tz, IReadOnlyList<WorldPoi> places)
        {
            var layers = new TerrainLayer[LayerScans.Length];
            for (int i = 0; i < layers.Length; i++) layers[i] = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"{TestMapTerrain}Layer_{LayerScans[i]}.terrainlayer");
            data.terrainLayers = layers;
            data.alphamapResolution = SplatRes;
            data.SetAlphamaps(0, 0, Splat(data, tx, tz, places));
            Trees(data, tx, tz, places);
            Details(data, tx, tz, places);
        }

        private static Vector2 Origin(int tx, int tz) => new(-WorldPlan.Half + tx * WorldPlan.TileSize, -WorldPlan.Half + tz * WorldPlan.TileSize);

        private static float RoadAt(float x, float z)
        {
            int ix = Mathf.Clamp(Mathf.RoundToInt((x + WorldPlan.Half) / WorldTerrain.Cell), 0, WorldTerrain.Samples - 1);
            int iz = Mathf.Clamp(Mathf.RoundToInt((z + WorldPlan.Half) / WorldTerrain.Cell), 0, WorldTerrain.Samples - 1);
            return WorldTerrain.Road[iz, ix];
        }

        // Distance outside the nearest place's ground (0 inside).
        private static float FromSites(float x, float z, IReadOnlyList<WorldPoi> places)
        {
            float best = float.MaxValue;
            foreach (WorldPoi p in places)
            {
                Rect r = WorldPlan.GroundRect(p);
                float dx = Mathf.Max(r.xMin - x, 0f, x - r.xMax), dz = Mathf.Max(r.yMin - z, 0f, z - r.yMax);
                best = Mathf.Min(best, Mathf.Sqrt(dx * dx + dz * dz));
            }
            return best;
        }

        private static float[,,] Splat(TerrainData data, int tx, int tz, IReadOnlyList<WorldPoi> places)
        {
            int n = LayerScans.Length;
            var maps = new float[SplatRes, SplatRes, n];
            float step = WorldPlan.TileSize / SplatRes;
            Vector2 o = Origin(tx, tz);
            var w = new float[n];
            for (int iz = 0; iz < SplatRes; iz++)
            for (int ix = 0; ix < SplatRes; ix++)
            {
                float x = o.x + (ix + .5f) * step, z = o.y + (iz + .5f) * step;
                System.Array.Clear(w, 0, n);
                float patch = Fbm(x * .09f + 7f, z * .09f + 3f, 3), patch2 = Fbm(x * .03f - 21f, z * .03f + 9f, 3);
                float forest = WorldPlan.Forest(x, z);
                w[(int)Layer.Grass] = (1f - forest) * Mathf.Lerp(1f, .25f, patch2);
                w[(int)Layer.Meadow] = (1f - forest) * Mathf.Lerp(.15f, .9f, patch2);
                w[(int)Layer.ForestFloor] = forest * Mathf.Lerp(1f, .35f, patch);
                w[(int)Layer.Litter] = forest * Mathf.Lerp(.2f, .8f, patch);
                float steep = data.GetSteepness((ix + .5f) / SplatRes, (iz + .5f) / SplatRes);
                float bare = Smooth(22f, 34f, steep);
                for (int k = 0; k < n; k++) w[k] *= 1f - bare;
                w[(int)Layer.Dirt] += bare * .6f;
                w[(int)Layer.Stone] += bare * .4f;
                // Road shoulders: worn gravel and dirt either side of the asphalt (the road itself is a mesh).
                float road = RoadAt(x, z) + (patch - .5f) * .25f;
                float shoulder = Mathf.Clamp01(road * 1.3f);
                for (int k = 0; k < n; k++) w[k] *= 1f - shoulder;
                w[(int)Layer.Stone] += shoulder * .6f;
                w[(int)Layer.Dirt] += shoulder * .4f;
                // Round the sites: trodden dirt where crews have parked for years.
                float site = 1f - Smooth(0f, 25f, FromSites(x, z, places) + (patch - .5f) * 8f);
                for (int k = 0; k < n; k++) w[k] *= 1f - site * .7f;
                w[(int)Layer.Dirt] += site * .5f;
                w[(int)Layer.Stone] += site * .2f;
                float sum = 0f;
                for (int k = 0; k < n; k++) sum += w[k];
                for (int k = 0; k < n; k++) maps[iz, ix, k] = sum > 0f ? w[k] / sum : k == 0 ? 1f : 0f;
            }
            return maps;
        }

        private static void Trees(TerrainData data, int tx, int tz, IReadOnlyList<WorldPoi> places)
        {
            var source = AssetDatabase.LoadAssetAtPath<TerrainData>(TestMapTerrain + "TestMapTerrain.asset");
            if (source == null || source.treePrototypes.Length == 0) return;
            data.treePrototypes = source.treePrototypes;
            int kinds = source.treePrototypes.Length;
            var random = new System.Random(9173 + tx * 31 + tz * 977);
            var instances = new List<TreeInstance>();
            const float cell = 9f;
            Vector2 o = Origin(tx, tz);
            for (float lz = 2f; lz < WorldPlan.TileSize - 2f; lz += cell)
            for (float lx = 2f; lx < WorldPlan.TileSize - 2f; lx += cell)
            {
                float px = lx + (float)random.NextDouble() * cell * .9f, pz = lz + (float)random.NextDouble() * cell * .9f;
                float x = o.x + px, z = o.y + pz;
                float forest = WorldPlan.Forest(x, z) * Smooth(10f, 40f, FromSites(x, z, places));
                if (RoadAt(x, z) > 0f || random.NextDouble() > forest * .85f) continue;
                if (data.GetSteepness(px / WorldPlan.TileSize, pz / WorldPlan.TileSize) > 38f) continue;
                // Mostly the tall firs; saplings and small trees at the forest's edge; the odd dead trunk.
                int kind = forest < .6f && random.NextDouble() < .5 ? 3 + random.Next(Mathf.Max(1, kinds - 3)) : random.Next(Mathf.Min(3, kinds));
                kind = Mathf.Clamp(kind, 0, kinds - 1);
                float s = Mathf.Lerp(.85f, 1.25f, (float)random.NextDouble());
                float shade = Mathf.Lerp(.8f, 1f, (float)random.NextDouble());
                instances.Add(new TreeInstance
                {
                    prototypeIndex = kind,
                    position = new Vector3(px / WorldPlan.TileSize, 0f, pz / WorldPlan.TileSize),
                    widthScale = s,
                    heightScale = s * Mathf.Lerp(.92f, 1.08f, (float)random.NextDouble()),
                    rotation = (float)random.NextDouble() * Mathf.PI * 2f,
                    color = new Color(shade, shade, shade, 1f),
                    lightmapColor = Color.white,
                });
            }
            data.SetTreeInstances(instances.ToArray(), true);
        }

        // Grass cards in the clearings (counts per cell), thinning under the trees; none on roads or sites.
        private static void Details(TerrainData data, int tx, int tz, IReadOnlyList<WorldPoi> places)
        {
            var source = AssetDatabase.LoadAssetAtPath<TerrainData>(TestMapTerrain + "TestMapTerrain.asset");
            if (source == null || source.detailPrototypes.Length == 0) return;
            data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
            data.SetDetailResolution(DetailRes, 32);
            // The grass card and the two grass meshes only: the ferns and weeds belong to the yard's close-up dressing.
            var prototypes = new List<DetailPrototype>();
            for (int i = 0; i < source.detailPrototypes.Length && prototypes.Count < 3; i++) prototypes.Add(source.detailPrototypes[i]);
            data.detailPrototypes = prototypes.ToArray();
            var layers = new int[prototypes.Count][,];
            for (int i = 0; i < layers.Length; i++) layers[i] = new int[DetailRes, DetailRes];
            var random = new System.Random(311 + tx * 17 + tz * 101);
            float step = WorldPlan.TileSize / DetailRes;
            Vector2 o = Origin(tx, tz);
            for (int iz = 0; iz < DetailRes; iz++)
            for (int ix = 0; ix < DetailRes; ix++)
            {
                float x = o.x + (ix + .5f) * step, z = o.y + (iz + .5f) * step;
                if (RoadAt(x, z) > .05f || FromSites(x, z, places) < 4f) continue;
                float open = 1f - WorldPlan.Forest(x, z) * .8f;
                float drift = Fbm(x * .05f + 13f, z * .05f - 2f, 3);
                layers[0][iz, ix] = Mathf.RoundToInt(open * Mathf.Lerp(4f, 22f, Smooth(.3f, .7f, drift)));
                double r = random.NextDouble();
                if (layers.Length > 1 && r < open * .25f) layers[1][iz, ix] = 1 + random.Next(2);
                if (layers.Length > 2 && r > .85f && r < .85f + open * .1f) layers[2][iz, ix] = 1;
            }
            for (int i = 0; i < layers.Length; i++) data.SetDetailLayer(0, 0, i, layers[i]);
        }
    }
}
