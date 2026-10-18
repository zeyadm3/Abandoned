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

        public readonly Material Wall, Floor, Steel, Rust, Lining, Bed, Lava, Safety, Lamp, Concrete, Asphalt, Dirt;

        public IronFactoryArt()
        {
            Wall = GetMaterial("IronFactory_Wall", new Color(.20f, .15f, .13f));
            Floor = GetMaterial("IronFactory_Floor", new Color(.21f, .21f, .20f));
            Steel = GetMaterial("IronFactory_Steel", new Color(.17f, .18f, .19f));
            Rust = GetMaterial("IronFactory_Rust", new Color(.33f, .17f, .09f));
            Lining = GetMaterial("IronFactory_Lining", new Color(.30f, .20f, .16f));
            Bed = GetMaterial("IronFactory_Bed", new Color(.06f, .04f, .03f));
            Safety = GetMaterial("IronFactory_Safety", new Color(.78f, .58f, .10f));
            Concrete = GetMaterial("Location_Concrete", new Color(.34f, .35f, .34f));
            Asphalt = GetMaterial("Location_Asphalt", new Color(.10f, .11f, .12f));
            Dirt = GetMaterial("Location_Dirt", new Color(.20f, .22f, .18f));

            Lamp = GetMaterial("IronFactory_Lamp", new Color(1f, .90f, .70f));
            Lamp.SetColor("_EmissionColor", new Color(1.7f, 1.35f, .8f));
            Lamp.EnableKeyword("_EMISSION");

            Lava = LavaMaterial();
            EnsureLavaConfig();
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
