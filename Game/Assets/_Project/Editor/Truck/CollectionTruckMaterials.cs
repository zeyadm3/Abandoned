using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Independent vehicle finishes: restrained paint, glass and metal with usable PBR response.</summary>
    public static class CollectionTruckMaterials
    {
        private static readonly Dictionary<string, Material> materials = new();
        private const string Folder = CollectionTruckGeometry.Folder + "/Materials";

        public static void Prepare()
        {
            PolishAssets.EnsureFolder(Folder);
            materials.Clear();
            Finish("Paint", new Color(.86f, .87f, .85f), .05f, .48f, .009f);
            Finish("Steel", new Color(.53f, .56f, .57f), .82f, .38f, .012f);
            Finish("Dark", new Color(.075f, .085f, .09f), .06f, .23f, .008f);
            Finish("Chrome", new Color(.69f, .71f, .73f), .94f, .72f, .002f);
            Finish("Tire", new Color(.035f, .038f, .042f), 0f, .16f, .011f);
            Finish("Seat", new Color(.15f, .17f, .19f), 0f, .12f, .022f);
            Finish("Deck", new Color(.30f, .23f, .16f), 0f, .17f, .025f);
            Finish("CargoLining", new Color(.71f, .72f, .68f), 0f, .25f, .007f);
            Lens("Amber", new Color(.76f, .27f, .035f), .06f);
            Lens("Red", new Color(.48f, .025f, .02f), .025f);
            Lens("Lamp", new Color(.68f, .72f, .71f), .22f);
            Material glass = NewMaterial("Glass");
            glass.SetColor("_BaseColor", new Color(.19f, .28f, .32f, .32f));
            glass.SetFloat("_Metallic", .06f); glass.SetFloat("_Smoothness", .92f);
            glass.SetFloat("_Surface", 1f); glass.SetFloat("_ZWrite", 0f);
            glass.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            glass.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            glass.SetFloat("_Cull", (float)CullMode.Off);
            glass.SetOverrideTag("RenderType", "Transparent");
            glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            glass.SetShaderPassEnabled("ShadowCaster", false);
            glass.renderQueue = (int)RenderQueue.Transparent;
            Store("Glass", glass);
        }

        public static Material Get(string name) => materials[name];

        private static Material NewMaterial(string name)
        {
            string path = $"{Folder}/Truck_{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            var clean = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            if (material == null)
            {
                material = clean; material.name = "Truck_" + name;
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                // Remove inherited salvage maps/keywords that previously overrode vehicle roughness and tint.
                EditorUtility.CopySerialized(clean, material);
                Object.DestroyImmediate(clean);
                material.name = "Truck_" + name;
            }
            material.enableInstancing = true;
            return material;
        }

        private static void Store(string name, Material material)
        {
            EditorUtility.SetDirty(material);
            materials[name] = material;
        }

        private static void Lens(string name, Color color, float emission)
        {
            Material material = NewMaterial(name);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", .75f);
            material.SetColor("_EmissionColor", color * emission);
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            Store(name, material);
        }

        private static void Finish(string name, Color baseColor, float metal, float smooth, float variation)
        {
            const int size = 512;
            var colors = new Color32[size * size];
            var packed = new Color32[size * size];
            var normals = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int i = x + y * size;
                float grain = Mathf.PerlinNoise(x * .31f + 13f, y * .27f + 17f) - .5f;
                float broad = Mathf.PerlinNoise(x * .014f + 19f, y * .014f + 11f) - .5f;
                float wear = grain * variation + broad * variation * .3f;
                if (name == "Deck") wear += Mathf.Sin(y * .037f + Mathf.PerlinNoise(x * .021f, y * .005f) * 2f) * .018f;
                colors[i] = new Color(Mathf.Clamp01(baseColor.r + wear),
                    Mathf.Clamp01(baseColor.g + wear), Mathf.Clamp01(baseColor.b + wear));
                packed[i] = new Color(metal, 0f, 0f, Mathf.Clamp01(smooth + grain * .025f));
                normals[i] = new Color32((byte)(128 + grain * 3f), (byte)(128 + grain * 3f), 255, 255);
            }
            Material material = NewMaterial(name);
            material.SetColor("_BaseColor", Color.white);
            material.SetTexture("_BaseMap", Map(name + "_BaseColor", colors, true, false));
            material.SetTexture("_MetallicGlossMap", Map(name + "_MetallicSmoothness", packed, false, false));
            material.SetTexture("_BumpMap", Map(name + "_Normal", normals, false, true));
            material.SetFloat("_Smoothness", 1f); material.SetFloat("_BumpScale", .12f);
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_NORMALMAP");
            Store(name, material);
        }

        private static Texture2D Map(string name, Color32[] pixels, bool srgb, bool normal)
        {
            string path = Folder + "/Redo_" + name + ".png";
            // These generated maps are deterministic and shared by every scene truck. Rewriting an already
            // imported PNG once per scene can race Unity's Windows texture importer, so reuse the configured
            // asset unless it is genuinely absent.
            Texture2D existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (existing != null) return existing;
            var texture = new Texture2D(512, 512, TextureFormat.RGBA32, false);
            texture.SetPixels32(pixels); texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb; importer.wrapMode = TextureWrapMode.Repeat;
            importer.mipmapEnabled = true; importer.maxTextureSize = 512; importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
