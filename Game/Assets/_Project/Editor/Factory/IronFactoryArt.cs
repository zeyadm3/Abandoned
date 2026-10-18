using System.IO;
using Abandoned.Structure;
using UnityEditor;
using UnityEngine;
using static Abandoned.EditorTools.GreyboxFactory;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// The Iron Factory's materials: soot-dark brick and steel, oily concrete, refractory lining and the molten
    /// iron itself (an original code-generated crust texture, no art pack). Also makes the LavaConfig asset
    /// so its tunables show up in the Inspector.
    /// </summary>
    public sealed class IronFactoryArt
    {
        private const string ArtFolder = "Assets/_Project/Art/Factory";
        private const string CrustPath = ArtFolder + "/LavaCrust.png";
        private const string LavaConfigPath = "Assets/_Project/Data/Structure/Resources/LavaConfig.asset";

        public readonly Material Wall, RoomWall, Floor, Steel, Rust, Lining, Bed, Lava, Safety, Lamp, Concrete, Asphalt, Dirt;
        /// <summary>L1: see-through catwalk decks and treads, their rusted version, and the mesh guard panels.</summary>
        public readonly Material Grating, RustGrating, Guard;
        /// <summary>L1 decals: oil, the soot veil on the walls, and the mall's scorch, crack and damp moss.</summary>
        public readonly Material Oil, SootVeil, Scorch, Crack, Moss;
        /// <summary>Metres per texture repeat (u, v) for each tiled material: the world-UV pass reads it.</summary>
        public readonly System.Collections.Generic.Dictionary<Material, Vector2> Tiling = new();

        public IronFactoryArt()
        {
            // L1: real surfaces. The factory's own maps are made in code; the rest come from the mall's PBR kit,
            // copied into factory materials so nothing the mall, school or prison uses is touched.
            IronFactoryTextures.EnsureAll();
            Wall = Tile(IronFactoryMaterialKit.Textured("IronFactory_Wall", "SootBrick", Color.white), 4f);
            RoomWall = Tile(IronFactoryMaterialKit.FromMall("IronFactory_RoomWall", "Block", new Color(.62f, .6f, .55f)), 2.5f);
            Floor = Tile(IronFactoryMaterialKit.FromMall("IronFactory_Floor", "Concrete", new Color(.58f, .56f, .52f)), 4f);
            Steel = Tile(IronFactoryMaterialKit.Textured("IronFactory_Steel", "RivetedPlate", Color.white), 2f);
            Rust = Tile(IronFactoryMaterialKit.FromMall("IronFactory_Rust", "Rust", Color.white), 2f);
            Lining = Tile(IronFactoryMaterialKit.Textured("IronFactory_Lining", "FireBrick", new Color(.85f, .72f, .65f)), 2f);
            Bed = GetMaterial("IronFactory_Bed", new Color(.06f, .04f, .03f));
            Safety = Tile(IronFactoryMaterialKit.Textured("IronFactory_Safety", "WornPaint", Color.white), 1f);
            Concrete = Tile(IronFactoryMaterialKit.FromMall("IronFactory_Concrete", "Concrete", Color.white), 4f);
            Asphalt = Tile(IronFactoryMaterialKit.FromMall("IronFactory_Asphalt", "Asphalt", Color.white), 6f);
            Dirt = Tile(IronFactoryMaterialKit.Textured("IronFactory_Cinder", "Cinder", Color.white), 4f);
            Grating = Tile(IronFactoryMaterialKit.Textured("IronFactory_Grating", "Grating", Color.white, IronFactoryMaterialKit.Surface.CutoutTwoSided), .5f);
            RustGrating = Tile(IronFactoryMaterialKit.Textured("IronFactory_RustGrating", "Grating", new Color(.9f, .55f, .38f), IronFactoryMaterialKit.Surface.CutoutTwoSided), .5f);
            Guard = Tile(IronFactoryMaterialKit.Textured("IronFactory_Guard", "MeshGuard", Color.white, IronFactoryMaterialKit.Surface.CutoutTwoSided), .5f);
            Oil = IronFactoryMaterialKit.Textured("IronFactory_Oil", "OilStain", Color.white, IronFactoryMaterialKit.Surface.Cutout);
            SootVeil = IronFactoryMaterialKit.Textured("IronFactory_SootVeil", "SootVeil", Color.white, IronFactoryMaterialKit.Surface.Transparent);
            Scorch = MeridianArt.Material("Scorch");
            Crack = MeridianArt.Material("Crack");
            Moss = MeridianArt.Material("Moss");

            Lamp = GetMaterial("IronFactory_Lamp", new Color(1f, .90f, .70f));
            Lamp.SetColor("_EmissionColor", new Color(1.7f, 1.35f, .8f));
            Lamp.EnableKeyword("_EMISSION");

            Lava = LavaMaterial();
            EnsureLavaConfig();
        }

        private Material Tile(Material m, float metres)
        {
            Tiling[m] = new Vector2(metres, metres);
            return m;
        }

        private static Material LavaMaterial()
        {
            Texture2D crust = CrustTexture();
            Material m = GetMaterial("IronFactory_Lava", new Color(1f, .55f, .2f));
            m.SetTexture("_BaseMap", crust);
            m.SetTexture("_EmissionMap", crust);
            // HDR emission: the bright cracks bloom, the dark crust stays dark (the map multiplies this).
            m.SetColor("_EmissionColor", new Color(5.5f, 1.9f, .35f));
            m.EnableKeyword("_EMISSION");
            m.SetFloat("_Smoothness", .35f);
            // Base and emission share the base map's tiling: three crust tiles across, nine along the channel.
            m.mainTextureScale = new Vector2(3f, 9f);
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(m);
            return m;
        }

        /// <summary>Molten cracks under a cooling crust, tileable so the drift never shows a seam.</summary>
        private static Texture2D CrustTexture()
        {
            if (!File.Exists(CrustPath))
            {
                Directory.CreateDirectory(ArtFolder);
                const int size = 256;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                var hot = new Color(1f, .62f, .16f);
                var crust = new Color(.07f, .03f, .02f);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = (float)x / size, v = (float)y / size;
                    float n = Tileable(u, v, 6f, 11f) * .65f + Tileable(u, v, 14f, 37f) * .35f;
                    // Thin bright seams where the noise crosses the middle; plates of crust elsewhere.
                    float seam = 1f - Mathf.Clamp01(Mathf.Abs(n - .5f) * 9f);
                    float glow = Mathf.Max(seam, Mathf.SmoothStep(.18f, .05f, n) * .8f);
                    tex.SetPixel(x, y, Color.Lerp(crust, hot, glow));
                }
                tex.Apply();
                File.WriteAllBytes(CrustPath, tex.EncodeToPNG());
                Object.DestroyImmediate(tex);
                AssetDatabase.ImportAsset(CrustPath);
                if (AssetImporter.GetAtPath(CrustPath) is TextureImporter importer)
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    importer.sRGBTexture = true;
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(CrustPath);
        }

        // Perlin noise blended across its four wrapped copies, so it repeats seamlessly on a 0-1 square.
        private static float Tileable(float u, float v, float scale, float seed)
        {
            float N(float a, float b) => Mathf.PerlinNoise(a * scale + seed, b * scale + seed * .7f);
            return N(u, v) * (1 - u) * (1 - v) + N(u - 1, v) * u * (1 - v)
                 + N(u, v - 1) * (1 - u) * v + N(u - 1, v - 1) * u * v;
        }

        private static void EnsureLavaConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<LavaConfig>(LavaConfigPath) != null) return;
            Directory.CreateDirectory(Path.GetDirectoryName(LavaConfigPath));
            AssetDatabase.Refresh();
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LavaConfig>(), LavaConfigPath);
        }
    }
}
