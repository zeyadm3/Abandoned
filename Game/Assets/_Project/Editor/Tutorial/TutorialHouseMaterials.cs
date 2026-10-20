using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>Original Larch Court PBR maps, shared by the authored models and metre-mapped architecture.</summary>
    public static class TutorialHouseMaterials
    {
        public const string Folder = "Assets/_Project/Art/Custom/Environment/LarchCourt";
        private static readonly HashSet<string> Prepared = new();

        // The legacy environment importer forces every image to 256px sRGB, including normal/mask data.
        // Route this kit through its own import policy on EVERY import, not just once in the scene builder.
        public static void ConfigureImporter(TextureImporter importer, string path)
        {
            bool normal = path.EndsWith("_Normal.png"), colour = path.EndsWith("_BaseColor.png");
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = colour;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 8;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            var desktop = importer.GetPlatformTextureSettings("Standalone");
            desktop.overridden = true;
            desktop.maxTextureSize = 1024;
            desktop.format = normal ? TextureImporterFormat.BC5 : TextureImporterFormat.BC7;
            desktop.compressionQuality = 100;
            importer.SetPlatformTextureSettings(desktop);
        }

        public static void Apply(TutorialHouseArt a)
        {
            Prepared.Clear();
            Bind(a.Oak, "FloorWood", 1.6f, 2.4f, a);
            foreach (var m in new[] { a.OakDark, a.Cabinet, a.FenceWood }) Bind(m, "Wood", 1, 1, a);
            foreach (var m in new[] { a.SofaFabric, a.Bedding, a.Mattress, a.Pillow, a.Curtain, a.Blind }) Bind(m, "Fabric", .6f, .6f, a);
            Bind(a.BeddingB, "Floral", .8f, .8f, a);
            Bind(a.Carpet, "Carpet", 1, 1, a);
            Bind(a.Rug, "Rug", 1.6f, 1.6f, a);
            Bind(a.Tile, "Tile", 1.2f, 1.2f, a);
            Bind(a.Linoleum, "Lino", 1.2f, 1.2f, a);
            Bind(a.Laminate, "Laminate", .7f, .7f, a);
            Bind(a.Ceiling, "Plaster", 1, 1, a);
            Bind(a.Plaster, "Plaster", 1.5f, 1.5f, a);
            Bind(a.BasementWall, "Block", 1.6f, 1.6f, a);
            Bind(a.Siding, "Siding", 1.6f, 1.6f, a);
            Bind(a.Shingles, "Shingles", 2.4f, 2.4f, a);
            Bind(a.Cardboard, "Cardboard", .6f, .6f, a);
            Bind(a.Bark, "Bark", .6f, 1.2f, a);
            foreach (var m in new[] { a.Trim, a.Paint, a.FrontDoor }) Bind(m, "Paint", 1, 1, a);
            Metal(a.Chrome, .92f, .72f);
            Metal(a.Brass, .78f, .52f);
            Metal(a.DarkMetal, .65f, .30f);
            // Glass needs to admit the winter view; the school's heavily frosted map reads as rippled water here.
            a.Glass.SetFloat("_Surface", 1);
            a.Glass.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            a.Glass.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            a.Glass.SetFloat("_ZWrite", 0);
            a.Glass.SetFloat("_Cull", 0);
            a.Glass.SetColor("_BaseColor", new Color(.70f, .81f, .86f, .18f));
            a.Glass.SetFloat("_Smoothness", .82f);
            a.Glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            a.Glass.SetOverrideTag("RenderType", "Transparent");
            a.Glass.renderQueue = 3000;
            GeneratedMaterialRepair.Apply(a.Glass);
        }

        private static void Bind(Material material, string key, float x, float y, TutorialHouseArt art)
        {
            material.SetTexture("_BaseMap", Map(key, "BaseColor"));
            material.SetTexture("_BumpMap", Map(key, "Normal"));
            material.SetTexture("_MetallicGlossMap", Map(key, "Mask"));
            material.SetTexture("_OcclusionMap", Map(key, "Mask"));
            material.SetFloat("_BumpScale", .65f);
            material.SetFloat("_Smoothness", 1);
            material.SetFloat("_SmoothnessTextureChannel", 0);
            material.SetFloat("_OcclusionStrength", .5f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_METALLICSPECGLOSSMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            material.SetTextureScale("_BaseMap", Vector2.one);
            art.Tiling[material] = new Vector2(x, y);
            GeneratedMaterialRepair.Apply(material);
        }

        private static Texture2D Map(string key, string kind)
        {
            string path = $"{Folder}/Textures/{key}_{kind}.png";
            if (Prepared.Add(path))
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException($"Missing house map: {path}. Run Tools/Blender/tutorial/build.py.");
                bool normal = kind == "Normal", colour = kind == "BaseColor";
                if (importer.textureType != (normal ? TextureImporterType.NormalMap : TextureImporterType.Default)
                    || importer.sRGBTexture != colour || importer.anisoLevel != 8 || importer.maxTextureSize != 1024)
                {
                    ConfigureImporter(importer, path);
                    importer.SaveAndReimport();
                }
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void Metal(Material material, float metallic, float smoothness)
        {
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
        }
    }
}
