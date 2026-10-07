using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// Materials and textures for the lighting pass (M7.2), created once under Art/Lighting and
    /// updated in place on every rebuild (stable GUIDs, so scenes don't churn).
    /// </summary>
    public static class LightingAssets
    {
        public const string Folder = "Assets/_Project/Art/Lighting";
        private const string LitShader = "Universal Render Pipeline/Lit";
        private const string ParticleLitShader = "Universal Render Pipeline/Particles/Simple Lit";
        private const string ParticleUnlitShader = "Universal Render Pipeline/Particles/Unlit";

        /// <summary>A ceiling panel: white plastic whose glow LightFixture drives (HDR emission for bloom).</summary>
        public static Material FixturePanel()
        {
            Material m = Load("Fixture_Panel", LitShader);
            m.SetColor("_BaseColor", new Color(0.85f, 0.86f, 0.88f));
            m.SetColor("_EmissionColor", new Color(2.2f, 2.1f, 1.9f));
            m.EnableKeyword("_EMISSION");
            m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            return Save(m);
        }

        /// <summary>Dust motes: soft lit dots, so they show only where light (or a flashlight) falls.</summary>
        public static Material Dust()
        {
            Material m = Load("Dust", ParticleLitShader);
            m.SetTexture("_BaseMap", SoftDot());
            m.SetColor("_BaseColor", new Color(1f, 0.97f, 0.9f, 0.55f));
            Transparent(m, BlendMode.SrcAlpha, BlendMode.OneMinusSrcAlpha, additive: false);
            return Save(m);
        }

        /// <summary>A light shaft: additive, unlit, faded by vertex alpha, seen from both sides.</summary>
        public static Material Shaft()
        {
            Material m = Load("LightShaft", ParticleUnlitShader);
            m.SetTexture("_BaseMap", Texture2D.whiteTexture);
            m.SetColor("_BaseColor", new Color(1f, 0.95f, 0.82f, 1f));
            Transparent(m, BlendMode.SrcAlpha, BlendMode.One, additive: true);
            m.SetFloat("_Cull", (float)CullMode.Off);
            return Save(m);
        }

        private static Material Load(string name, string shaderName)
        {
            EnsureFolder();
            string path = $"{Folder}/{name}.mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find(shaderName));
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        private static Material Save(Material m)
        {
            EditorUtility.SetDirty(m);
            return m;
        }

        // Shader GUIs set these when a surface type is picked by hand; materials made from code must do it themselves.
        private static void Transparent(Material m, BlendMode src, BlendMode dst, bool additive)
        {
            m.SetFloat("_Surface", 1f);
            m.SetFloat("_Blend", additive ? 2f : 0f);
            m.SetFloat("_SrcBlend", (float)src);
            m.SetFloat("_DstBlend", (float)dst);
            m.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
            m.SetFloat("_DstBlendAlpha", (float)dst);
            m.SetFloat("_ZWrite", 0f);
            m.SetOverrideTag("RenderType", "Transparent");
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)RenderQueue.Transparent;
        }

        /// <summary>A 32x32 white dot with a soft edge (alpha), saved as a PNG.</summary>
        private static Texture2D SoftDot()
        {
            string path = $"{Folder}/SoftDot.png";
            var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            const int n = 32;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false);
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(n / 2f, n / 2f)) / (n / 2f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(1f - d * d)));
            }
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void EnsureFolder()
        {
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/_Project/Art", "Lighting");
        }
    }
}
