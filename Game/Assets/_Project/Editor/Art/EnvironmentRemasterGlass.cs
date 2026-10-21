using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Abandoned.EditorTools
{
    /// <summary>Architectural glazing: clean centres, accumulated perimeter dirt and fine wipe/scratch roughness.</summary>
    public static class EnvironmentRemasterGlass
    {
        private const string Folder = "Assets/_Project/Art/Remaster/Environment";
        private const int Size = 512;
        private static readonly HashSet<Material> processed = new();

        public static int ApplyToScene(Scene scene)
        {
            EnsureTextures();
            var found = new HashSet<Material>();
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is not MeshRenderer || renderer.GetComponentInParent<Abandoned.Structure.LightFixture>() != null) continue;
                Vector3 size = renderer.bounds.size;
                if (Mathf.Max(size.x * size.y, size.z * size.y) < .3f) continue;
                foreach (Material material in renderer.sharedMaterials)
                    if (Candidate(material)) found.Add(material);
            }
            foreach (Material material in found)
            {
                if (!processed.Add(material)) continue;
                bool frosted = material.name.ToLowerInvariant().Contains("frost");
                material.shader = Shader.Find("Universal Render Pipeline/Lit");
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 1f);
                material.SetFloat("_SrcBlend", (float)BlendMode.One);
                material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_SrcBlendAlpha", (float)BlendMode.One);
                material.SetFloat("_DstBlendAlpha", (float)BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0f);
                material.SetFloat("_AlphaClip", 0f);
                material.SetFloat("_BlendModePreserveSpecular", 1f);
                material.SetFloat("_Cull", (float)CullMode.Off);
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Smoothness", frosted ? .46f : .94f);
                material.SetFloat("_BumpScale", frosted ? .11f : .035f);
                material.SetFloat("_WorkflowMode", 1f);
                material.SetFloat("_SpecularHighlights", 1f);
                material.SetFloat("_EnvironmentReflections", 1f);
                material.SetColor("_BaseColor", frosted ? new Color(.76f, .8f, .82f, .58f) : new Color(.75f, .82f, .84f, .3f));
                material.SetTexture("_BaseMap", Texture(frosted ? "GlazingFrosted" : "GlazingDirt"));
                material.SetTextureScale("_BaseMap", Vector2.one);
                material.SetTextureOffset("_BaseMap", Vector2.zero);
                material.SetTexture("_MetallicGlossMap", Texture("GlazingSmoothness"));
                material.SetTexture("_BumpMap", Texture("GlazingNormal"));
                material.SetOverrideTag("RenderType", "Transparent");
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.EnableKeyword("_NORMALMAP");
                material.EnableKeyword("_METALLICSPECGLOSSMAP");
                material.DisableKeyword("_ALPHAMODULATE_ON");
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_SPECULAR_SETUP");
                material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
                material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
                material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
                material.SetFloat("_SmoothnessTextureChannel", 0f);
                material.DisableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", Color.black);
                material.renderQueue = (int)RenderQueue.Transparent;
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);
            }
            return found.Count;
        }

        public static void ResetSession() => processed.Clear();

        public static bool Candidate(Material material)
        {
            if (material == null || material.shader == null) return false;
            string path = AssetDatabase.GetAssetPath(material);
            if (!path.EndsWith(".mat") || path.Contains("/Characters/") || path.Contains("/Cosmetics/")) return false;
            string name = material.name.ToLowerInvariant();
            if (!(name.Contains("glass") || name.Contains("glazing"))) return false;
            return !name.Contains("loot") && !name.Contains("lamp") && !name.Contains("flood") && !name.Contains("sodium")
                && !name.Contains("truck") && !name.Contains("vehicle") && !name.Contains("smoked") && !name.Contains("mirror");
        }

        private static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>($"{Folder}/{name}.png");

        private static float Dirt(float u, float v)
        {
            float edge = Mathf.Min(u, 1f - u, v, 1f - v);
            float perimeter = 1f - Mathf.SmoothStep(.012f, .14f, edge);
            float runs = Mathf.Pow(Mathf.PerlinNoise(u * 43f + 7f, v * 1.8f + 1f), 4f);
            float wipe = Mathf.PerlinNoise(u * 7f + v * 2f, v * 8f + 9f);
            float lower = 1f - Mathf.SmoothStep(.01f, .25f, v);
            return Mathf.Clamp01(perimeter * (.38f + runs * .6f) + lower * .12f + Mathf.Pow(wipe, 5f) * .15f);
        }

        private static void EnsureTextures()
        {
            PolishAssets.EnsureFolder(Folder);
            if (Texture("GlazingDirt") != null && Texture("GlazingFrosted") != null && Texture("GlazingSmoothness") != null && Texture("GlazingNormal") != null) return;
            var colour = new Color[Size * Size];
            var frosted = new Color[colour.Length];
            var smooth = new Color[colour.Length];
            var normal = new Color[colour.Length];
            for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float u = (x + .5f) / Size, v = (y + .5f) / Size;
                float dirt = Dirt(u, v);
                float micro = Mathf.PerlinNoise(u * 127f + 11f, v * 67f + 31f);
                float scratch = Mathf.Pow(Mathf.PerlinNoise(u * 191f + 5f, v * 3f + 4f), 14f);
                float alpha = .13f + dirt * .84f;
                colour[y * Size + x] = new Color(1f - dirt * .36f, 1f - dirt * .32f, 1f - dirt * .28f, alpha);
                frosted[y * Size + x] = new Color(colour[y * Size + x].r, colour[y * Size + x].g, colour[y * Size + x].b, .8f + dirt * .2f);
                smooth[y * Size + x] = new Color(0f, 0f, 0f, Mathf.Clamp01(.94f - dirt * .6f - scratch * .1f));
                float nx = (micro - .5f) * .045f, ny = (scratch - .1f) * .025f;
                normal[y * Size + x] = new Color(.5f + nx, .5f + ny, 1f, 1f);
            }
            Save("GlazingDirt", colour, true, false);
            Save("GlazingFrosted", frosted, true, false);
            Save("GlazingSmoothness", smooth, false, false);
            Save("GlazingNormal", normal, false, true);
        }

        private static void Save(string name, Color[] pixels, bool srgb, bool normal)
        {
            string path = $"{Folder}/{name}.png";
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false, !srgb);
            texture.SetPixels(pixels);
            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.anisoLevel = 8;
            importer.maxTextureSize = Size;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }
}
