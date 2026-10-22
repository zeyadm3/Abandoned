using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using static Abandoned.EditorTools.ShowcaseSitePlan;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Land past the edge of a map whose ground ends in a hard slab edge (the School's campus, the Tutorial's town):
    /// a terrain that starts level with the map's ground at its edge and rises into wooded hills, under snow on a winter
    /// map. The map's own footprint is cut out of the terrain (terrain holes), so nothing inside the map, basements and
    /// stairs included, is touched. Its trees are the scanned ones (MapTrees.Forest), thicker the further out.
    /// </summary>
    public static class MapRing
    {
        private const string Folder = "Assets/_Project/Art/MapSite";
        private const int Res = 513;
        private const float Margin = 340f, Rise = 70f;

        public static Terrain Build(Scene scene, MapSiteProfile profile, Bounds level, Transform parent)
        {
            int mask = ~LayerMask.GetMask(Core.GameLayers.Player, Core.GameLayers.Loot, Core.GameLayers.Debris, "Ignore Raycast");
            // The extent of everything the map draws (far houses can be meshes without colliders), backdrops left out.
            level = Drawn(scene, level);
            float size = Mathf.Max(level.size.x, level.size.z) + Margin * 2f;
            var origin = new Vector3(level.center.x - size / 2f, 0f, level.center.z - size / 2f);
            float step = size / (Res - 1);

            // The map's footprint: every heightmap point with the map's ground (or anything of it) under it.
            var inside = new bool[Res, Res];
            var groundY = new List<float>();
            float top = level.max.y + 100f;
            for (int iz = 0; iz < Res; iz++)
            for (int ix = 0; ix < Res; ix++)
            {
                var p = new Vector3(origin.x + ix * step, top, origin.z + iz * step);
                // Inside the map's extent and with something under it (its ground, including a backdrop plane there).
                if (Mathf.Abs(p.x - level.center.x) > level.extents.x + 3f || Mathf.Abs(p.z - level.center.z) > level.extents.z + 3f) continue;
                if (!Physics.Raycast(p, Vector3.down, out RaycastHit hit, top - level.min.y + 20f, mask, QueryTriggerInteraction.Ignore) ||
                    hit.collider is TerrainCollider) continue;
                inside[iz, ix] = true;
                groundY.Add(hit.point.y);
            }
            groundY.Sort();
            float ground = groundY.Count > 0 ? groundY[groundY.Count / 10] : 0f; // floors and roads, not the roofs
            float[,] distance = Distance(inside, step);

            float baseY = ground - 30f, height = 160f;
            var heights = new float[Res, Res];
            var random = new System.Random(scene.name.GetHashCode());
            for (int iz = 0; iz < Res; iz++)
            for (int ix = 0; ix < Res; ix++)
            {
                float x = origin.x + ix * step, z = origin.z + iz * step, d = distance[iz, ix];
                float hills = (Fbm(x * .006f + 5f, z * .006f - 3f, 5) - .35f) * 38f + (Fbm(x * .03f, z * .03f, 3) - .5f) * 3f;
                float h = ground - .06f + Smooth(12f, 200f, d) * (Mathf.Max(hills, -4f) + Rise * Smooth(70f, Margin, d));
                heights[iz, ix] = Mathf.Clamp01((h - baseY) / height);
            }

            var data = new TerrainData { heightmapResolution = Res };
            string path = $"{Folder}/{scene.name}_Ring.asset";
            PolishAssets.EnsureFolder(Folder);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(data, path);
            data.size = new Vector3(size, height, size);
            data.SetHeights(0, 0, heights);
            // A hole only where all four corners are inside the map, so the land meets the slab's edge without a gap.
            var holes = new bool[Res - 1, Res - 1];
            for (int iz = 0; iz < Res - 1; iz++)
            for (int ix = 0; ix < Res - 1; ix++)
                holes[iz, ix] = !(inside[iz, ix] && inside[iz + 1, ix] && inside[iz, ix + 1] && inside[iz + 1, ix + 1]);
            data.SetHoles(0, 0, holes);
            Paint(data, profile.Winter, distance, step);
            Plant(data, profile.Winter, distance, origin, size, random);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();

            GameObject go = Terrain.CreateTerrainGameObject(data);
            go.name = "Land";
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(origin.x, baseY, origin.z);
            var terrain = go.GetComponent<Terrain>();
            terrain.materialTemplate = TerrainMaterial();
            terrain.drawInstanced = true;
            terrain.heightmapPixelError = 4f;
            terrain.basemapDistance = 250f;
            terrain.treeDistance = 900f;
            terrain.treeBillboardDistance = 900f;
            terrain.shadowCastingMode = ShadowCastingMode.On;
            go.GetComponent<TerrainCollider>().terrainData = data;
            go.AddComponent<Core.SurfaceTag>().EditorSet(Core.SurfaceMaterial.Dirt);
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.OccluderStatic | StaticEditorFlags.BatchingStatic);
            return terrain;
        }

        private static Bounds Drawn(Scene scene, Bounds level)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
            {
                Bounds b = r.bounds;
                if (!r.enabled || b.size.x > 300f || b.size.z > 300f || r is ParticleSystemRenderer) continue;
                level.Encapsulate(b);
            }
            return level;
        }

        private static Material TerrainMaterial()
        {
            string path = $"{Folder}/MapSite_Terrain.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
            material.EnableKeyword("_NORMALMAP");
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        // Metres from each point to the nearest point of the footprint (0 inside it), by a two-pass chamfer sweep.
        private static float[,] Distance(bool[,] inside, float step)
        {
            const float Far = 1e6f, Diagonal = 1.41421f;
            var d = new float[Res, Res];
            for (int z = 0; z < Res; z++)
            for (int x = 0; x < Res; x++)
                d[z, x] = inside[z, x] ? 0f : Far;
            for (int z = 0; z < Res; z++)
            for (int x = 0; x < Res; x++)
            {
                if (x > 0) d[z, x] = Mathf.Min(d[z, x], d[z, x - 1] + 1f);
                if (z > 0) d[z, x] = Mathf.Min(d[z, x], d[z - 1, x] + 1f);
                if (x > 0 && z > 0) d[z, x] = Mathf.Min(d[z, x], d[z - 1, x - 1] + Diagonal);
                if (x < Res - 1 && z > 0) d[z, x] = Mathf.Min(d[z, x], d[z - 1, x + 1] + Diagonal);
            }
            for (int z = Res - 1; z >= 0; z--)
            for (int x = Res - 1; x >= 0; x--)
            {
                if (x < Res - 1) d[z, x] = Mathf.Min(d[z, x], d[z, x + 1] + 1f);
                if (z < Res - 1) d[z, x] = Mathf.Min(d[z, x], d[z + 1, x] + 1f);
                if (x < Res - 1 && z < Res - 1) d[z, x] = Mathf.Min(d[z, x], d[z + 1, x + 1] + Diagonal);
                if (x > 0 && z < Res - 1) d[z, x] = Mathf.Min(d[z, x], d[z + 1, x - 1] + Diagonal);
            }
            for (int z = 0; z < Res; z++)
            for (int x = 0; x < Res; x++)
                d[z, x] = d[z, x] >= Far ? 9999f : d[z, x] * step;
            return d;
        }

        private static void Paint(TerrainData data, bool winter, float[,] distance, float step)
        {
            // Winter: two snow scans and bare rock where it is steep. Otherwise the TestMap's grass and forest floor.
            (string set, float tile)[] scans = winter
                ? new[] { ("Snow010A", 4f), ("Snow013", 6f), ("Rock035", 5f) }
                : new[] { ("Grass004", 5f), ("Ground037", 4f), ("Rock035", 5f) };
            var layers = new TerrainLayer[scans.Length];
            for (int i = 0; i < scans.Length; i++)
            {
                (string set, float tile) = scans[i];
                string layerPath = $"{Folder}/Layer_{set}.terrainlayer";
                var layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
                if (layer == null)
                {
                    layer = new TerrainLayer();
                    AssetDatabase.CreateAsset(layer, layerPath);
                }
                string scan = $"Assets/_Project/Art/Scanned/{set}/{set}_2K-JPG_";
                layer.diffuseTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(scan + "Color.jpg");
                layer.normalMapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(scan + "NormalGL.jpg");
                layer.tileSize = new Vector2(tile, tile);
                layer.tileOffset = new Vector2(i * 1.7f, i * .9f);
                layer.smoothness = winter && i < 2 ? .35f : .1f;
                EditorUtility.SetDirty(layer);
                layers[i] = layer;
            }
            data.terrainLayers = layers;
            int res = data.alphamapResolution;
            var maps = new float[res, res, layers.Length];
            for (int z = 0; z < res; z++)
            for (int x = 0; x < res; x++)
            {
                float steep = data.GetSteepness((x + .5f) / res, (z + .5f) / res);
                float rock = Smooth(26f, 40f, steep);
                float mix = Fbm(x * .05f, z * .05f, 3);
                maps[z, x, 0] = (1f - rock) * Mathf.Lerp(1f, .3f, mix);
                maps[z, x, 1] = (1f - rock) * Mathf.Lerp(0f, .7f, mix);
                maps[z, x, 2] = rock;
            }
            data.SetAlphamaps(0, 0, maps);
        }

        private static void Plant(TerrainData data, bool winter, float[,] distance, Vector3 origin, float size, System.Random random)
        {
            (GameObject prefab, float height)[] kinds = MapTrees.Forest(winter);
            if (kinds.Length == 0) return;
            var prototypes = new TreePrototype[kinds.Length];
            for (int i = 0; i < kinds.Length; i++) prototypes[i] = new TreePrototype { prefab = kinds[i].prefab };
            data.treePrototypes = prototypes;
            var instances = new List<TreeInstance>();
            const float cell = 6f;
            float step = size / (Res - 1);
            for (float z = 2f; z < size - 2f; z += cell)
            for (float x = 2f; x < size - 2f; x += cell)
            {
                float px = x + (float)random.NextDouble() * cell * .9f, pz = z + (float)random.NextDouble() * cell * .9f;
                int ix = Mathf.Clamp(Mathf.RoundToInt(px / step), 0, Res - 1), iz = Mathf.Clamp(Mathf.RoundToInt(pz / step), 0, Res - 1);
                float d = distance[iz, ix];
                // Open near the map, woods beyond; clearings where the noise says so.
                float forest = Smooth(14f, 60f, d) * Smooth(.36f, .55f, Fbm((origin.x + px) * .012f, (origin.z + pz) * .012f, 4));
                if (random.NextDouble() > forest * .9f) continue;
                int k = random.Next(kinds.Length);
                // The tall firs mostly; saplings and snags thinly.
                if (kinds[k].height < 8f && random.NextDouble() < .55) k = random.Next(Mathf.Min(3, kinds.Length));
                float s = Mathf.Lerp(.85f, 1.25f, (float)random.NextDouble());
                instances.Add(new TreeInstance
                {
                    prototypeIndex = k, position = new Vector3(px / size, 0f, pz / size), widthScale = s, heightScale = s,
                    rotation = (float)random.NextDouble() * Mathf.PI * 2f, color = Color.white, lightmapColor = Color.white,
                });
            }
            data.SetTreeInstances(instances.ToArray(), true);
        }
    }
}
