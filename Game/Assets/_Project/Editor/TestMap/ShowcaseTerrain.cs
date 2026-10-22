using Abandoned.Core;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The TestMap's land (ShowcaseSitePlan says where everything is): one Unity terrain, 640 m square. The yard is flat;
    /// past the fence the ground rises into wooded hills, highest on the ridge to the north, and the gravel road is cut
    /// into it with a ditch each side. Eight scanned ground layers are painted by rule (asphalt and gravel in the yard,
    /// forest floor under the trees, grass in the clearings, bare dirt where it is steep), the scanned trees are scattered
    /// where the forest is, and grass, ferns and weeds where they would grow. Past the terrain, a ring of far hills
    /// closes the horizon. Seeded: every rebuild is the same.
    /// </summary>
    public static class ShowcaseTerrain
    {
        private const string Folder = "Assets/_Project/Art/TestMap/Terrain";
        /// <summary>The ground and cladding scans fetch_nature.py packs (colour, normal, mask per scan).</summary>
        public const string GroundFolder = "Assets/_Project/Art/TestMap/Ground";
        private const int HeightRes = 1025, SplatRes = 1024, DetailRes = 1024;

        private enum Layer { Grass, Meadow, ForestFloor, Dirt, Gravel, YardGravel, Asphalt, Litter }

        private static readonly (string scan, float tile)[] Layers =
        {
            ("forrest_ground_01", 5f), ("grass_ground", 7f), ("forest_leaves_02", 4f), ("forest_ground_04", 4.5f),
            ("rocky_trail", 3.5f), ("gravel_ground_01", 4f), ("asphalt_02", 6f), ("brown_mud_leaves_01", 4f),
        };

        private static Terrain built;

        /// <summary>The ground's height at world (x, z) once the terrain is built (the road and its ditches included).</summary>
        public static float HeightAt(float x, float z) =>
            built != null ? built.SampleHeight(new Vector3(x, 0f, z)) + built.transform.position.y : NaturalHeight(x, z);

        public static Terrain Build(Transform parent)
        {
            PolishAssets.EnsureFolder(Folder);
            // The asset first: alphamaps, trees and grass set on a TerrainData that isn't an asset yet are lost when it becomes one.
            AssetDatabase.DeleteAsset($"{Folder}/TestMapTerrain.asset");
            var data = new TerrainData { heightmapResolution = HeightRes };
            AssetDatabase.CreateAsset(data, $"{Folder}/TestMapTerrain.asset");
            data.size = new Vector3(Land.width, TerrainHeight, Land.height);
            data.SetHeights(0, 0, Heights());
            data.terrainLayers = MakeLayers();
            data.alphamapResolution = SplatRes;
            data.SetAlphamaps(0, 0, Splat(data));
            Trees(data);
            Details(data);
            data.wavingGrassAmount = .35f;
            data.wavingGrassSpeed = .4f;
            data.wavingGrassStrength = .45f;
            data.wavingGrassTint = new Color(.82f, .84f, .7f);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            GameObject go = UnityEngine.Terrain.CreateTerrainGameObject(data);
            go.name = "Terrain";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(Land.xMin, TerrainBase, Land.yMin);
            var terrain = go.GetComponent<Terrain>();
            terrain.materialTemplate = TerrainMaterial();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 3f;
            terrain.basemapDistance = 220f;
            terrain.treeDistance = 650f;
            terrain.treeBillboardDistance = 650f; // our trees carry their own impostor LOD
            terrain.treeLODBiasMultiplier = 1f;
            terrain.detailObjectDistance = 110f;
            terrain.detailObjectDensity = 1f;
            terrain.shadowCastingMode = ShadowCastingMode.On;
            terrain.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            terrain.bakeLightProbesForTrees = true;
            go.GetComponent<TerrainCollider>().terrainData = data;
            // Footsteps outside are dirt and gravel (SurfaceTests probes the yard west of the warehouse for Dirt).
            go.AddComponent<SurfaceTag>().EditorSet(SurfaceMaterial.Dirt);
            built = terrain;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI | StaticEditorFlags.OccluderStatic |
                                                       StaticEditorFlags.BatchingStatic);
            Horizon(parent);
            return terrain;
        }

        /// <summary>Terrain height at world (x, z), in world metres (before the road is cut in).</summary>
        public static float NaturalHeight(float x, float z)
        {
            float rise = Smooth(4f, 60f, OutsideYard(x, z));
            float hills = (Fbm(x * .0055f + 3.1f, z * .0055f + 7.7f, 5) - .42f) * 30f;
            float detail = (Fbm(x * .04f + 40f, z * .04f - 12f, 3) - .5f) * 2.6f;
            float ridge = 40f * Smooth(55f, 260f, z) * (.7f + .6f * Fbm(x * .01f, 5f, 3));
            float east = 22f * Smooth(90f, 280f, x), west = 15f * Smooth(110f, 300f, -x);
            float south = -7f * Smooth(110f, 320f, -z);
            return Ground + rise * (hills + detail + ridge + east + west + south);
        }

        private static float[,] Heights()
        {
            var heights = new float[HeightRes, HeightRes];
            float step = Land.width / (HeightRes - 1);
            // The road's own height: the land along it, smoothed so it climbs gently, level with the yard at the gate.
            float length = RoadLength;
            int samples = Mathf.CeilToInt(length / 2f) + 1;
            var profile = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                Vector2 at = RoadAt(i * 2f);
                profile[i] = NaturalHeight(at.x, at.y);
            }
            var road = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float sum = 0f;
                int n = 0;
                for (int j = Mathf.Max(0, i - 12); j <= Mathf.Min(samples - 1, i + 12); j++, n++) sum += profile[j];
                road[i] = Mathf.Lerp(Ground, sum / n, Smooth(6f, 30f, i * 2f));
            }
            for (int iz = 0; iz < HeightRes; iz++)
            for (int ix = 0; ix < HeightRes; ix++)
            {
                float x = Land.xMin + ix * step, z = Land.yMin + iz * step;
                float h = NaturalHeight(x, z);
                if (x < 30f && z < -40f)
                {
                    (float d, float along) = ToRoad(x, z);
                    if (d < 14f)
                    {
                        float bed = road[Mathf.Clamp(Mathf.RoundToInt(along / 2f), 0, samples - 1)];
                        h = Mathf.Lerp(h, bed, 1f - Smooth(RoadHalfWidth + 1.2f, 13f, d));
                        // A shallow ditch each side, filled in near the yard where the road meets the forecourt.
                        float ditch = Mathf.Exp(-Mathf.Pow((d - (RoadHalfWidth + 2.2f)) / .9f, 2f)) * .45f;
                        h -= ditch * Smooth(8f, 24f, along);
                    }
                }
                heights[iz, ix] = Mathf.Clamp01((h - TerrainBase) / TerrainHeight);
            }
            return heights;
        }

        /// <summary>How much forest stands at (x, z), 0..1: clearings where the noise is low, none near the fence or road.</summary>
        public static float Forest(float x, float z)
        {
            float f = Smooth(.38f, .56f, Fbm(x * .012f + 11f, z * .012f - 5f, 4));
            f = Mathf.Max(f, Smooth(90f, 170f, z) * .85f);
            f *= Smooth(9f, 26f, OutsideYard(x, z));
            if (z < -36f && x < 40f) f *= Smooth(7f, 15f, ToRoad(x, z).distance);
            return f;
        }

        private static float[,,] Splat(TerrainData data)
        {
            int n = Layers.Length;
            var maps = new float[SplatRes, SplatRes, n];
            float step = Land.width / SplatRes;
            var w = new float[n];
            for (int iz = 0; iz < SplatRes; iz++)
            for (int ix = 0; ix < SplatRes; ix++)
            {
                float x = Land.xMin + (ix + .5f) * step, z = Land.yMin + (iz + .5f) * step;
                System.Array.Clear(w, 0, n);
                float patch = Fbm(x * .09f + 7f, z * .09f + 3f, 3), patch2 = Fbm(x * .03f - 21f, z * .03f + 9f, 3);
                if (InYard(x, z, 1f)) YardGround(x, z, patch, patch2, w);
                else
                {
                    float forest = Forest(x, z);
                    w[(int)Layer.Grass] = (1f - forest) * Mathf.Lerp(1f, .25f, patch2);
                    w[(int)Layer.Meadow] = (1f - forest) * Mathf.Lerp(.15f, .9f, patch2);
                    w[(int)Layer.ForestFloor] = forest * Mathf.Lerp(1f, .35f, patch);
                    w[(int)Layer.Litter] = forest * Mathf.Lerp(.2f, .8f, patch);
                    float steep = data.GetSteepness((ix + .5f) / SplatRes, (iz + .5f) / SplatRes);
                    float bare = Smooth(24f, 36f, steep);
                    for (int k = 0; k < n; k++) w[k] *= 1f - bare;
                    w[(int)Layer.Dirt] += bare * .7f;
                    w[(int)Layer.Gravel] += bare * .3f;
                    if (z < -40f && x < 30f)
                    {
                        float d = ToRoad(x, z).distance;
                        float bed = 1f - Smooth(RoadHalfWidth - .8f, RoadHalfWidth + .4f, d + (patch - .5f) * 1.2f);
                        float shoulder = (1f - Smooth(RoadHalfWidth + .5f, RoadHalfWidth + 3.2f, d + (patch - .5f) * 2f)) * (1f - bed);
                        for (int k = 0; k < n; k++) w[k] *= 1f - Mathf.Max(bed, shoulder);
                        w[(int)Layer.Gravel] += bed;
                        w[(int)Layer.Dirt] += shoulder * .7f;
                        w[(int)Layer.Litter] += shoulder * .3f;
                    }
                }
                float sum = 0f;
                for (int k = 0; k < n; k++) sum += w[k];
                for (int k = 0; k < n; k++) maps[iz, ix, k] = sum > 0f ? w[k] / sum : k == 0 ? 1f : 0f;
            }
            return maps;
        }

        // Inside the fence: a cracked asphalt forecourt, a gravel lot, beaten dirt by the old boiler house, rough grass.
        private static void YardGround(float x, float z, float patch, float patch2, float[] w)
        {
            float edge = (patch - .5f) * 2.4f;
            float asphalt = Inside(Forecourt, x, z, edge);
            float gravel = Inside(GravelLot, x, z, edge) * (1f - asphalt);
            float dirt = Inside(Rect.MinMaxRect(BoilerPlot.xMin - 6f, BoilerPlot.yMin - 6f, BoilerPlot.xMax + 3f, BoilerPlot.yMax + 9f), x, z, edge * 1.5f);
            dirt = Mathf.Max(dirt, Inside(Rect.MinMaxRect(-4f, -4f, 24f, 37f), x, z, edge)); // round the buildings: trodden
            dirt *= 1f - Mathf.Max(asphalt, gravel);
            float rest = Mathf.Max(0f, 1f - asphalt - gravel - dirt);
            // Weeds come up through the asphalt's cracks; the gravel lot is half grown over at its edges.
            w[(int)Layer.Asphalt] = asphalt * Mathf.Lerp(1f, .55f, Smooth(.55f, .75f, patch));
            w[(int)Layer.YardGravel] = asphalt * Mathf.Lerp(0f, .45f, Smooth(.55f, .75f, patch)) + gravel * .45f;
            w[(int)Layer.Gravel] = gravel * Mathf.Lerp(.75f, .25f, patch2);
            w[(int)Layer.Dirt] = dirt * Mathf.Lerp(.9f, .4f, patch) + gravel * .1f;
            w[(int)Layer.Litter] = dirt * Mathf.Lerp(.1f, .6f, patch);
            w[(int)Layer.Grass] = rest * Mathf.Lerp(.9f, .3f, patch2);
            w[(int)Layer.Meadow] = rest * Mathf.Lerp(.2f, .9f, patch2);
        }

        private static float Inside(Rect r, float x, float z, float jitter)
        {
            float d = Mathf.Max(r.xMin - x, x - r.xMax, r.yMin - z, z - r.yMax) + jitter;
            return 1f - Smooth(-.6f, .6f, d);
        }

        private static TerrainLayer[] MakeLayers()
        {
            var layers = new TerrainLayer[Layers.Length];
            for (int i = 0; i < Layers.Length; i++)
            {
                (string scan, float tile) = Layers[i];
                string path = $"{Folder}/Layer_{scan}.terrainlayer";
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
                if (layer == null)
                {
                    layer = new TerrainLayer();
                    AssetDatabase.CreateAsset(layer, path);
                }
                string ground = $"{GroundFolder}/{scan}";
                layer.diffuseTexture = ShowcaseNature.Texture(ground + "_diff", srgb: true, normal: false, cutout: false);
                layer.normalMapTexture = ShowcaseNature.Texture(ground + "_nor_gl", srgb: false, normal: true, cutout: false);
                layer.maskMapTexture = ShowcaseNature.Texture(ground + "_mask", srgb: false, normal: false, cutout: false);
                layer.normalScale = 1f;
                layer.tileSize = new Vector2(tile, tile);
                layer.tileOffset = new Vector2(i * 1.37f, i * .71f); // the layers' repeats don't line up
                layer.smoothness = 1f;
                layer.metallic = 0f;
                // Mask: R metal, G occlusion, B (height) unused, A smoothness; ground is never glossy.
                layer.maskMapRemapMin = Vector4.zero;
                layer.maskMapRemapMax = new Vector4(1f, 1f, 1f, .55f);
                EditorUtility.SetDirty(layer);
                layers[i] = layer;
            }
            return layers;
        }

        private static Material TerrainMaterial()
        {
            string path = $"{Folder}/TestMap_Terrain.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_MASKMAP");
            material.SetFloat("_EnableInstancedPerPixelNormal", 1f);
            material.EnableKeyword("_TERRAIN_INSTANCED_PERPIXEL_NORMAL");
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void Trees(TerrainData data)
        {
            var prototypes = new List<TreePrototype>();
            var kinds = new List<(int index, float weightEdge, float weightDeep, Vector2 scale)>();
            void Add(GameObject prefab, float edge, float deep, Vector2 scale)
            {
                if (prefab == null) return;
                kinds.Add((prototypes.Count, edge, deep, scale));
                prototypes.Add(new TreePrototype { prefab = prefab, bendFactor = 0f });
            }
            var lod = new Vector3(.9f, .3f, .004f);
            foreach (string v in new[] { "fir_tree_01_a", "fir_tree_01_b", "fir_tree_01_c" })
                Add(ShowcaseNature.TreePrefab("fir_tree_01", v, .3f, lod), 1f, 3f, new Vector2(.85f, 1.2f));
            foreach (string v in new[] { "fir_sapling_medium_a", "fir_sapling_medium_b", "fir_sapling_medium_c" })
                Add(ShowcaseNature.TreePrefab("fir_sapling_medium", v, 0f, new Vector3(.6f, .2f, .004f)), 1.4f, .5f, new Vector2(.8f, 1.3f));
            foreach (string v in ShowcaseNature.Parts("pine_sapling_small"))
                Add(ShowcaseNature.TreePrefab("pine_sapling_small", v, 0f, new Vector3(.5f, .15f, .004f)), 1f, .25f, new Vector2(.8f, 1.4f));
            foreach (string v in ShowcaseNature.Parts("tree_small_02"))
                Add(ShowcaseNature.TreePrefab("tree_small_02", v, .15f, new Vector3(.6f, .2f, .004f)), 1.2f, .3f, new Vector2(1.1f, 1.8f));
            foreach (string v in ShowcaseNature.Parts("dead_tree_trunk_02"))
                Add(ShowcaseNature.TreePrefab("dead_tree_trunk_02", v, .3f, new Vector3(.6f, .2f, .004f)), .15f, .2f, new Vector2(.9f, 1.2f));
            data.treePrototypes = prototypes.ToArray();
            if (kinds.Count == 0) return;

            var random = new System.Random(4127);
            var instances = new List<TreeInstance>();
            const float cell = 5.2f;
            for (float z = Land.yMin + 3f; z < Land.yMax - 3f; z += cell)
            for (float x = Land.xMin + 3f; x < Land.xMax - 3f; x += cell)
            {
                float px = x + (float)random.NextDouble() * cell * .9f, pz = z + (float)random.NextDouble() * cell * .9f;
                float forest = Forest(px, pz);
                if (random.NextDouble() > forest * .9f) continue;
                // At the forest's edge more young firs and small trees; deep in it, the tall firs.
                float edge = 1f - Smooth(.45f, .9f, forest);
                Place(instances, kinds, random, px, pz, edge, data);
            }
            // A few in the yard: firs and small trees in the rough grass, where the site has been left alone.
            foreach (Vector2 p in new Vector2[]
                     {
                         new(-36f, 44f), new(-31f, 47f), new(-24f, 42f), new(-12f, 46f), new(34f, 45f), new(41f, 48f), new(57f, 44f),
                         new(-37f, 28f), new(-36f, 20f), new(-38f, -20f), new(-35f, -34f), new(-37f, -47f), new(59f, -50f), new(58f, 26f),
                     })
                Place(instances, kinds, random, p.x, p.y, .7f, data);
            data.SetTreeInstances(instances.ToArray(), true);
            Debug.Log($"[Showcase] Terrain: {instances.Count} trees from {prototypes.Count} kinds.");
        }

        private static void Place(List<TreeInstance> instances, List<(int index, float edge, float deep, Vector2 scale)> kinds,
            System.Random random, float x, float z, float edge, TerrainData data)
        {
            float total = 0f;
            foreach (var k in kinds) total += Mathf.Lerp(k.deep, k.edge, edge);
            float pick = (float)random.NextDouble() * total;
            var kind = kinds[0];
            foreach (var k in kinds)
            {
                pick -= Mathf.Lerp(k.deep, k.edge, edge);
                if (pick > 0f) continue;
                kind = k;
                break;
            }
            float s = Mathf.Lerp(kind.scale.x, kind.scale.y, (float)random.NextDouble());
            float shade = Mathf.Lerp(.8f, 1f, (float)random.NextDouble());
            instances.Add(new TreeInstance
            {
                prototypeIndex = kind.index,
                position = new Vector3((x - Land.xMin) / Land.width, 0f, (z - Land.yMin) / Land.height),
                widthScale = s,
                heightScale = s * Mathf.Lerp(.92f, 1.08f, (float)random.NextDouble()),
                rotation = (float)random.NextDouble() * Mathf.PI * 2f,
                color = new Color(shade, shade, shade, 1f),
                lightmapColor = Color.white,
            });
        }

        private static void Details(TerrainData data)
        {
            // Counts per cell, not 0-255 coverage (a new TerrainData's default), so the numbers below are plants.
            data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
            data.SetDetailResolution(DetailRes, 32);
            var prototypes = new List<DetailPrototype>();
            Texture2D card = ShowcaseNature.Texture($"{ShowcaseNature.Prefabs}/grass_card", srgb: true, normal: false, cutout: true);
            if (card != null)
                prototypes.Add(new DetailPrototype
                {
                    prototypeTexture = card, renderMode = DetailRenderMode.Grass, usePrototypeMesh = false,
                    minWidth = .9f, maxWidth = 1.6f, minHeight = .3f, maxHeight = .8f, noiseSpread = .12f,
                    healthyColor = new Color(.78f, .95f, .66f), dryColor = new Color(1f, .88f, .62f),
                });
            int cardIndex = card != null ? 0 : -1;
            int Mesh(string id, string part, float min, float max)
            {
                GameObject prefab = ShowcaseNature.PartPrefab(id, part);
                if (prefab == null) return -1;
                prototypes.Add(new DetailPrototype
                {
                    prototype = prefab, usePrototypeMesh = true, renderMode = DetailRenderMode.VertexLit, useInstancing = true,
                    minWidth = min, maxWidth = max, minHeight = min, maxHeight = max, noiseSpread = .5f,
                    healthyColor = Color.white, dryColor = new Color(.9f, .86f, .75f),
                });
                return prototypes.Count - 1;
            }
            int tuft = Mesh("grass_medium_01", "grass_medium_01_tall_a_LOD0", 2.2f, 3.4f);
            int clump = Mesh("grass_medium_02", "grass_medium_02_e", 1.4f, 2.2f);
            int fern = Mesh("fern_02", "fern_02_b", .9f, 1.5f);
            int fern2 = Mesh("fern_02", "fern_02_c", .9f, 1.4f);
            int nettle = Mesh("nettle_plant", ShowcaseNature.Parts("nettle_plant").Length > 0 ? ShowcaseNature.Parts("nettle_plant")[0] : null, 1f, 1.6f);
            int weed = Mesh("weed_plant_02", ShowcaseNature.Parts("weed_plant_02").Length > 0 ? ShowcaseNature.Parts("weed_plant_02")[0] : null, 1f, 1.8f);
            int shrub = Mesh("shrub_02", "shrub_02_a_LOD0", .7f, 1.2f);
            data.detailPrototypes = prototypes.ToArray();

            var layers = new int[prototypes.Count][,];
            for (int i = 0; i < layers.Length; i++) layers[i] = new int[DetailRes, DetailRes];
            var random = new System.Random(77);
            float step = Land.width / DetailRes;
            float[,,] splat = data.GetAlphamaps(0, 0, SplatRes, SplatRes);
            for (int iz = 0; iz < DetailRes; iz++)
            for (int ix = 0; ix < DetailRes; ix++)
            {
                float x = Land.xMin + (ix + .5f) * step, z = Land.yMin + (iz + .5f) * step;
                if (Cleared(x, z, 1f) || InYard(x, z) && Inside(Forecourt, x, z, 0f) > .5f && Fbm(x * .3f, z * .3f, 2) < .62f) continue;
                if (z < -40f && x < 30f && ToRoad(x, z).distance < RoadHalfWidth + .6f) continue;
                float grass = splat[iz, ix, (int)Layer.Grass] + splat[iz, ix, (int)Layer.Meadow];
                float forest = splat[iz, ix, (int)Layer.ForestFloor] + splat[iz, ix, (int)Layer.Litter];
                float gravel = splat[iz, ix, (int)Layer.YardGravel] + splat[iz, ix, (int)Layer.Asphalt] * .3f;
                double r = random.NextDouble();
                // Long grass in drifts, cropped short between them; thinner on gravel and under the trees.
                float drift = Fbm(x * .05f + 13f, z * .05f - 2f, 3);
                if (cardIndex >= 0) layers[cardIndex][iz, ix] = Mathf.RoundToInt(grass * Mathf.Lerp(.5f, 6.5f, Smooth(.3f, .7f, drift)) + gravel * 1.5f + forest * .6f);
                if (tuft >= 0 && r < grass * .1f) layers[tuft][iz, ix] = 1;
                if (clump >= 0 && r > .9f && r < .9f + grass * .06f + gravel * .04f) layers[clump][iz, ix] = 1;
                if (fern >= 0 && r < forest * .05f) layers[r < forest * .025f ? fern : fern2][iz, ix] = 1;
                if (nettle >= 0 && r > .97f && r < .97f + gravel * .03f + (InYard(x, z) ? grass * .015f : 0f)) layers[nettle][iz, ix] = 1;
                if (weed >= 0 && r > .5f && r < .5f + gravel * .025f) layers[weed][iz, ix] = 1;
                if (shrub >= 0 && r > .8f && r < .8f + forest * .012f + grass * .004f) layers[shrub][iz, ix] = 1;
            }
            for (int i = 0; i < layers.Length; i++) data.SetDetailLayer(0, 0, i, layers[i]);
        }

        /// <summary>A ring of far hills past the terrain, so the horizon is land and not the sky's empty lower half. Fog
        /// turns it blue-grey; it only has to read as a wooded skyline.</summary>
        private static void Horizon(Transform parent)
        {
            const int segments = 160, rings = 7;
            float inner = Land.width * .5f - 10f;
            var centre = new Vector2(Land.center.x, Land.center.y);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (int r = 0; r < rings; r++)
            {
                float radius = Mathf.Lerp(inner, 2600f, Mathf.Pow(r / (rings - 1f), 1.6f));
                for (int s = 0; s <= segments; s++)
                {
                    float a = s * Mathf.PI * 2f / segments;
                    var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                    Vector2 p = centre + dir * radius;
                    // The first ring sits under the terrain's edge; behind it the hills rise to a broken skyline.
                    float sky = Fbm(dir.x * 2.2f + 5f, dir.y * 2.2f + 5f, 4);
                    float h = r == 0 ? TerrainBase + 4f : Mathf.Lerp(20f, 50f + sky * 240f, Mathf.Pow(r / (rings - 1f), .6f));
                    if (r == rings - 1) h = 30f + sky * 80f;
                    vertices.Add(new Vector3(p.x, h, p.y));
                }
            }
            for (int r = 0; r < rings - 1; r++)
            for (int s = 0; s < segments; s++)
            {
                int a = r * (segments + 1) + s, b = a + segments + 1;
                triangles.AddRange(new[] { a, a + 1, b, a + 1, b + 1, b }); // facing up
            }
            var mesh = new Mesh { name = "TestMap_Horizon" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.DeleteAsset($"{Folder}/TestMap_Horizon.asset");
            AssetDatabase.CreateAsset(mesh, $"{Folder}/TestMap_Horizon.asset");
            var go = new GameObject("Horizon");
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = go.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = ShowcaseMaterials.Plain("TestMap_Distant_Forest", new Color(.07f, .09f, .07f), .05f);
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }
}
