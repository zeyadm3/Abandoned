using System.IO;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Original tiling colour, tangent-normal and metallic/AO/smoothness maps for close character views.</summary>
    public static class CharacterSurfaceTextures
    {
        public enum Pattern { Fabric, Canvas, Rubber, Leather, Metal, Tape, Skin, Glass, Paint, Screen, Wood, Ash, Bone, Rust }
        public const string Folder = "Assets/_Project/Art/Characters/Textures";
        private const int Size = 512;

        public static void Ensure(Pattern pattern)
        {
            PolishAssets.EnsureFolder(Folder);
            string stem = Folder + "/" + pattern + "_v1";
            if (File.Exists(stem + "_BaseColor.png") && File.Exists(stem + "_Normal.png") && File.Exists(stem + "_Mask.png")) return;
            var colours = new Color32[Size * Size];
            var masks = new Color32[colours.Length];
            var height = new float[colours.Length];
            int seed = 104729 + (int)pattern * 9151;
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float u = x / (float)Size, v = y / (float)Size;
                float broad = Wave(u, v, 3f, seed), fine = Hash(x, y, seed);
                float mid = Wave(u, v, 17f, seed + 7);
                float shade = .84f + broad * .12f + (fine - .5f) * .025f;
                float bump = broad * .025f, metallic = 0f, smooth = .2f, ao = .98f;
                float weave = Mathf.Sin(u * Mathf.PI * 192f) * Mathf.Sin(v * Mathf.PI * 192f);
                switch (pattern)
                {
                    case Pattern.Fabric:
                    case Pattern.Canvas:
                        float threads = pattern == Pattern.Canvas ? .11f : .06f;
                        bump += weave * threads + mid * .02f;
                        shade += weave * .035f - Mathf.Max(0f, mid - .55f) * .08f;
                        smooth = .1f + fine * .06f;
                        break;
                    case Pattern.Rubber:
                        bump += fine * .035f + mid * .025f;
                        shade *= .91f + mid * .06f;
                        smooth = .14f + fine * .13f;
                        break;
                    case Pattern.Leather:
                        float grain = Mathf.Pow(Mathf.Abs(Mathf.Sin((u + mid * .012f) * Mathf.PI * 90f)), 10f);
                        bump += fine * .04f - grain * .055f;
                        shade -= grain * .07f;
                        smooth = .22f + broad * .2f;
                        break;
                    case Pattern.Metal:
                        float scratches = Mathf.Pow(Mathf.Abs(Mathf.Sin((v + Mathf.Sin(u * Mathf.PI * 8f) * .003f) * Mathf.PI * 204f)), 28f);
                        bump += mid * .02f - scratches * .07f;
                        shade += scratches * .12f;
                        metallic = .86f;
                        smooth = .42f + broad * .2f - scratches * .2f;
                        break;
                    case Pattern.Tape:
                        bump += weave * .025f;
                        shade += weave * .025f;
                        metallic = .25f;
                        smooth = .48f + broad * .16f;
                        break;
                    case Pattern.Skin:
                        float pores = fine > .94f ? (fine - .94f) * 5f : 0f;
                        bump += mid * .015f - pores * .035f;
                        shade -= pores * .035f;
                        smooth = .2f + mid * .12f;
                        break;
                    case Pattern.Glass:
                    case Pattern.Screen:
                        bump = 0f;
                        shade = .92f + broad * .07f;
                        smooth = pattern == Pattern.Glass ? .9f : .77f;
                        metallic = .1f;
                        break;
                    case Pattern.Paint:
                        float chips = Mathf.Max(0f, mid * fine - .53f) * 3.3f;
                        shade -= chips * .24f;
                        bump += fine * .025f - chips * .07f;
                        metallic = chips * .6f;
                        smooth = .28f + broad * .12f - chips * .15f;
                        break;
                    case Pattern.Wood:
                        float rings = Mathf.Sin((u * 40f + Mathf.Sin(v * Mathf.PI * 2f) * .7f + mid * .2f) * Mathf.PI * 2f);
                        bump += rings * .035f;
                        shade += rings * .08f;
                        smooth = .2f;
                        break;
                    case Pattern.Ash:
                        float folds = Mathf.Pow(Mathf.Abs(Mathf.Sin((u + mid * .035f) * Mathf.PI * 16f)), 8f);
                        bump += mid * .06f - folds * .05f;
                        shade -= folds * .11f;
                        smooth = .13f + broad * .2f;
                        break;
                    case Pattern.Bone:
                        float cracks = Mathf.Pow(Mathf.Abs(Mathf.Sin((u * 9f + Mathf.Sin(v * Mathf.PI * 6f) * .2f) * Mathf.PI * 2f)), 36f);
                        bump += fine * .02f - cracks * .065f;
                        shade -= cracks * .22f;
                        smooth = .26f + broad * .12f;
                        break;
                    case Pattern.Rust:
                        bump += mid * .11f + fine * .06f;
                        shade -= mid * .13f;
                        metallic = Mathf.Clamp01(.5f - mid * .4f);
                        smooth = .07f + broad * .09f;
                        ao = .88f + fine * .1f;
                        break;
                }
                int at = y * Size + x;
                height[at] = bump;
                float warm = pattern is Pattern.Bone or Pattern.Wood or Pattern.Rust ? .025f : 0f;
                colours[at] = new Color(shade + warm, shade, shade - warm, 1f);
                masks[at] = new Color(Mathf.Clamp01(metallic), ao, 1f, Mathf.Clamp01(smooth));
            }
            var normals = new Color32[colours.Length];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float dx = height[y * Size + (x + 1) % Size] - height[y * Size + (x + Size - 1) % Size];
                float dy = height[(y + 1) % Size * Size + x] - height[(y + Size - 1) % Size * Size + x];
                Vector3 normal = new Vector3(-dx * 2f, -dy * 2f, 1f).normalized;
                normals[y * Size + x] = new Color(normal.x * .5f + .5f, normal.y * .5f + .5f, normal.z * .5f + .5f, 1f);
            }
            Save(stem + "_BaseColor.png", colours, normal: false, colour: true);
            Save(stem + "_Normal.png", normals, normal: true, colour: false);
            Save(stem + "_Mask.png", masks, normal: false, colour: false);
        }

        public static Texture2D Load(Pattern pattern, string channel) =>
            AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{pattern}_v1_{channel}.png");

        private static void Save(string path, Color32[] pixels, bool normal, bool colour)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = colour;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 4;
            importer.maxTextureSize = Size;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }

        private static float Wave(float u, float v, float frequency, int seed) => .5f +
            Mathf.Sin(u * Mathf.PI * 2f * frequency + Mathf.Sin(v * Mathf.PI * 4f) * .7f + seed % 23) * .24f +
            Mathf.Cos(v * Mathf.PI * 2f * (frequency + 2f) + Mathf.Sin(u * Mathf.PI * 6f) * .6f + seed % 17) * .18f;

        private static float Hash(int x, int y, int seed)
        {
            unchecked
            {
                uint h = (uint)(x * 73856093 ^ y * 19349663 ^ seed);
                h ^= h >> 13; h *= 1274126177u; h ^= h >> 16;
                return (h & 65535u) / 65535f;
            }
        }
    }
}
