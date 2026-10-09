using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Separate HQ materials keep the warehouse pass from recolouring mall fixtures or air.</summary>
    public static class HqSalvageAtmosphereAssets
    {
        public const string Folder = "Assets/_Project/Art/Generated/HqSalvageAtmosphere";

        public static Material Solid(string name, Color color, float metallic = 0f, float glow = 0f)
        {
            Material material = Load(name, "Universal Render Pipeline/Lit");
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", .18f);
            material.SetColor("_EmissionColor", color * glow);
            if (glow > 0f) material.EnableKeyword("_EMISSION");
            else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material Dust()
        {
            Material material = Load("ShopDust", "Universal Render Pipeline/Particles/Simple Lit");
            // Reuse the existing soft mask read-only; its mall material and density remain separate.
            Texture2D dot = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Generated/MeridianAtmosphere/DustDot.png")
                ?? AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Project/Art/Lighting/SoftDot.png");
            material.SetTexture("_BaseMap", dot != null ? dot : SoftDot());
            material.SetColor("_BaseColor", new Color(.92f, .88f, .75f, .36f));
            Transparent(material, false);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material Shaft(bool daylight = false)
        {
            Material material = Load(daylight ? "ClerestoryLightShaft" : "ShopLightShaft", "Universal Render Pipeline/Particles/Unlit");
            material.SetTexture("_BaseMap", SoftDot());
            material.SetColor("_BaseColor", daylight ? new Color(.65f, .78f, .87f, .23f) : new Color(.92f, .86f, .69f, .30f));
            Transparent(material, true);
            material.SetFloat("_Cull", (float)CullMode.Off);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material Load(string name, string shader)
        {
            PolishAssets.EnsureFolder(Folder);
            string path = $"{Folder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;
            material = new Material(Shader.Find(shader)) { name = name };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Texture2D SoftDot()
        {
            string path = Folder + "/ShopSoftDot.asset";
            Texture2D dot = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (dot != null) return dot;
            const int size = 32;
            dot = new Texture2D(size, size, TextureFormat.RGBA32, true)
                { name = "ShopSoftDot", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float radius = Vector2.Distance(new Vector2(x + .5f, y + .5f), Vector2.one * size * .5f) / (size * .5f);
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Pow(Mathf.Clamp01(1f - radius * radius), 2f));
            }
            dot.SetPixels(pixels);
            dot.Apply(true, false);
            AssetDatabase.CreateAsset(dot, path);
            return dot;
        }

        private static void Transparent(Material material, bool additive)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", additive ? 2f : 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
