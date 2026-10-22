using System.IO;
using Abandoned.Core;
using UnityEditor;
using UnityEngine;

namespace Abandoned.EditorTools
{
    /// <summary>
    /// PBR material palette and asset factory for the Skyscraper Restaurant.
    /// References CC0 scanned surfaces (ambientCG) for marble, parquet, stainless steel, and concrete.
    /// </summary>
    public sealed class SkyscraperArt
    {
        public const string Folder = "Assets/_Project/Art/Skyscraper";
        public const string MaterialsFolder = Folder + "/Materials";

        public readonly Material Marble;
        public readonly Material Parquet;
        public readonly Material Mahogany;
        public readonly Material WallPlaster;
        public readonly Material Concrete;
        public readonly Material Steel;
        public readonly Material DiamondPlate;
        public readonly Material KitchenTile;
        public readonly Material ColdStoreWall;
        public readonly Material Asphalt;
        public readonly Material CeilingTile;
        public readonly Material Glass;
        public readonly Material Brass;
        public readonly Material Leather;
        public readonly Material LampEmissive;
        public readonly Material RedEmergencyEmissive;

        public SkyscraperArt()
        {
            EnsureFolders();

            Marble = CreateMaterial("Marble_Carrara", new Color(0.85f, 0.85f, 0.85f), "Marble012", 0.65f, 0.05f);
            Parquet = CreateMaterial("Parquet_Luxury", new Color(0.45f, 0.32f, 0.22f), "WoodFloor051", 0.45f, 0.0f);
            Mahogany = CreateMaterial("Mahogany_Rich", new Color(0.28f, 0.12f, 0.08f), "WoodFloor051", 0.55f, 0.0f);
            WallPlaster = CreateMaterial("Wall_Plaster", new Color(0.72f, 0.70f, 0.66f), "Plaster001", 0.15f, 0.0f);
            Concrete = CreateMaterial("Concrete_Core", new Color(0.55f, 0.54f, 0.52f), "Concrete031", 0.18f, 0.0f);
            Steel = CreateMaterial("Steel_Industrial", new Color(0.48f, 0.50f, 0.52f), "Metal049A", 0.55f, 0.85f);
            DiamondPlate = CreateMaterial("Steel_DiamondPlate", new Color(0.40f, 0.42f, 0.44f), "DiamondPlate008A", 0.40f, 0.80f);
            KitchenTile = CreateMaterial("Kitchen_Tiles", new Color(0.80f, 0.82f, 0.80f), "Tiles107", 0.50f, 0.0f);
            ColdStoreWall = CreateMaterial("ColdStore_Panels", new Color(0.68f, 0.74f, 0.78f), "PaintedMetal012", 0.35f, 0.30f);
            Asphalt = CreateMaterial("Street_Asphalt", new Color(0.24f, 0.24f, 0.25f), "Asphalt031", 0.12f, 0.0f);
            CeilingTile = CreateMaterial("Ceiling_Office", new Color(0.65f, 0.65f, 0.64f), "OfficeCeiling001", 0.10f, 0.0f);
            Leather = CreateMaterial("Leather_Lounge", new Color(0.28f, 0.18f, 0.14f), "Leather037", 0.30f, 0.0f);
            Brass = CreateMaterial("Brass_Trim", new Color(0.82f, 0.68f, 0.32f), null, 0.75f, 0.90f);

            // Architectural Curtain Glass (tinted dark sky reflection)
            Glass = CreateGlassMaterial("Curtain_Glass", new Color(0.12f, 0.20f, 0.24f, 0.85f), 0.92f);

            // Fixture Diffusers (emissive)
            LampEmissive = CreateEmissiveMaterial("Lamp_WarmGlow", new Color(1.0f, 0.92f, 0.78f), new Color(2.0f, 1.8f, 1.4f));
            RedEmergencyEmissive = CreateEmissiveMaterial("Lamp_EmergencyRed", new Color(0.9f, 0.15f, 0.1f), new Color(2.5f, 0.2f, 0.1f));
        }

        public static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Art/Skyscraper"))
                AssetDatabase.CreateFolder("Assets/_Project/Art", "Skyscraper");
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Art/Skyscraper", "Materials");
            if (!AssetDatabase.IsValidFolder(Folder + "/Loot"))
                AssetDatabase.CreateFolder(Folder, "Loot");
            if (!AssetDatabase.IsValidFolder(Folder + "/Meshes"))
                AssetDatabase.CreateFolder(Folder, "Meshes");
        }

        private static Material CreateMaterial(string name, Color baseColor, string scanId, float smoothness, float metallic)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Abandoned/Surface") ?? Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", baseColor);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", metallic);

            if (!string.IsNullOrEmpty(scanId))
            {
                string scanDir = $"Assets/_Project/Art/Scanned/{scanId}";
                if (Directory.Exists(scanDir))
                {
                    string[] colors = Directory.GetFiles(scanDir, "*_Color.jpg");
                    if (colors.Length > 0)
                        mat.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(colors[0]));

                    string[] normals = Directory.GetFiles(scanDir, "*_NormalGL.jpg");
                    if (normals.Length > 0)
                    {
                        mat.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(normals[0]));
                        mat.EnableKeyword("_NORMALMAP");
                    }
                }
            }

            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateGlassMaterial(string name, Color tint, float smoothness)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", tint);
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0.1f);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static Material CreateEmissiveMaterial(string name, Color albedo, Color emission)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null) return mat;

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", albedo);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
