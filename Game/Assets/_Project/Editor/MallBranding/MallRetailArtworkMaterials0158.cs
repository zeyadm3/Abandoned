using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abandoned.EditorTools
{
    /// <summary>Printed artwork responds to the mall's lights rather than glowing in dark stores.</summary>
    internal static class MallRetailArtworkMaterials0158
    {
        internal const string Folder = "Assets/_Project/Art/Generated/MallBranding0158";
        private static readonly Dictionary<string, Material> Finishes = new();
        private static readonly string[] Brands = { "ApexCircuit", "NordicHabitat", "TitanTools", "VogueThread", "MeridianMall" };

        internal static void Prepare()
        {
            Directory.CreateDirectory(Folder + "/Materials");
            Directory.CreateDirectory(Folder + "/Meshes");
            AssetDatabase.Refresh();
            Finishes.Clear();
            foreach (string brand in Brands)
            foreach (string kind in new[] { "Logo", "Sale" })
            {
                string id = brand + "_" + kind;
                string path = Folder + "/Images/" + id + ".png";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidOperationException("Generated retail artwork is missing: " + path);
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Trilinear;
                importer.anisoLevel = 4;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SaveAndReimport();
                Material material = Finish(id, Color.white, kind == "Logo" ? .24f : .10f);
                material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
                EditorUtility.SetDirty(material);
            }
            Finish("PaperReverse", new Color(.70f, .67f, .57f), .05f);
            Finish("AgedTape", new Color(.61f, .56f, .39f), .17f);
            Finish("DisplayRail", new Color(.16f, .18f, .18f), .32f, .55f);
            Finish("MountingCord", new Color(.25f, .26f, .23f), .08f);
        }

        internal static Material Get(string id) => Finishes[id];

        private static Material Finish(string id, Color color, float smoothness, float metallic = 0)
        {
            string path = Folder + "/Materials/" + id + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = id };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = Shader.Find("Universal Render Pipeline/Lit");
            material.enableInstancing = true;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Surface", 0);
            material.SetFloat("_Cull", (float)CullMode.Back);
            material.SetFloat("_ZWrite", 1);
            material.SetFloat("_SrcBlend", (float)BlendMode.One);
            material.SetFloat("_DstBlend", (float)BlendMode.Zero);
            material.SetColor("_EmissionColor", Color.black);
            material.DisableKeyword("_EMISSION");
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.SetOverrideTag("RenderType", "Opaque");
            material.renderQueue = (int)RenderQueue.Geometry;
            Finishes[id] = material;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
