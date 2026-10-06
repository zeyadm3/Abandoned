using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Generates a pre-fractured floor tile: a grid of box chunks with jittered cut lines, so no
    /// external fracture tool is needed yet. Chunks tile the slab exactly; they get rigidbodies only
    /// when a collapse spawns them.
    /// </summary>
    public static class FracturedTileGenerator
    {
        public const string PrefabPath = "Assets/_Project/Prefabs/Structure/Fractured_Tile_4x4.prefab";

        // Must match the tiles it replaces; shared with the builder so they can't drift apart.
        private static readonly Vector3 TileSize = new(TestBuildingBuilder.Tile, TestBuildingBuilder.SlabThickness, TestBuildingBuilder.Tile);
        private const int Cells = 5;
        private const float Jitter = 0.35f;
        private const int Seed = 7;

        [MenuItem("Tools/Abandoned/Generate Fractured Tile")]
        public static GameObject Generate() => Generate(TileSize, Cells, Cells, Jitter, Seed, PrefabPath);

        public static GameObject Generate(Vector3 size, int cellsX, int cellsZ, float jitter, int seed, string path)
        {
            var random = new System.Random(seed);
            float[] xs = Cuts(size.x, cellsX, jitter, random);
            float[] zs = Cuts(size.z, cellsZ, jitter, random);

            var root = new GameObject("Fractured_Tile");
            Material material = GreyboxFactory.GetMaterial("Greybox_TileA", new Color(0.55f, 0.55f, 0.55f));
            for (int ix = 0; ix < cellsX; ix++)
            for (int iz = 0; iz < cellsZ; iz++)
            {
                float x0 = xs[ix], x1 = xs[ix + 1], z0 = zs[iz], z1 = zs[iz + 1];
                Vector3 centre = new((x0 + x1) / 2f, 0f, (z0 + z1) / 2f);
                GreyboxFactory.Box($"Chunk_{ix}_{iz}", root.transform, centre, new Vector3(x1 - x0, size.y, z1 - z0), material);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        /// <summary>Cut positions from -size/2 to size/2, each inner cut nudged by up to jitter × cell size.</summary>
        private static float[] Cuts(float size, int cells, float jitter, System.Random random)
        {
            var cuts = new float[cells + 1];
            float cell = size / cells;
            for (int i = 0; i <= cells; i++)
            {
                float offset = i == 0 || i == cells ? 0f : ((float)random.NextDouble() * 2f - 1f) * jitter * cell;
                cuts[i] = -size / 2f + i * cell + offset;
            }
            return cuts;
        }
    }
}
