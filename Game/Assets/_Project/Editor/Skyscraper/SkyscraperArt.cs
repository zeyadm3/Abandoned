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
        public readonly Material LiftSteel;
        public readonly Material LobbyWalnut;
        public readonly Material Parquet;
        public readonly Material Mahogany;
        public readonly Material WallPlaster;
        public readonly Material Concrete;
        public readonly Material Steel;
        public readonly Material DiamondPlate;
        public readonly Material KitchenTile;
        public readonly Material ColdStoreWall;
        public readonly Material Asphalt;
        public readonly Material PlazaPaver;
        public readonly Material GraniteDark;
        public readonly Material RoadYellow;
        public readonly Material RoadWhite;
        public readonly Material CarPaintBlack;
        public readonly Material CarPaintYellow;
        public readonly Material CarPaintBurgundy;
        public readonly Material Rubber;
        public readonly Material Rust;
        public readonly Material WaterMurky;
        public readonly Material CeilingTile;
        public readonly Material Glass;
        public readonly Material TowerCladding;
        public readonly Material Brass;
        public readonly Material Leather;
        public readonly Material Linen;
        public readonly Material Porcelain;
        public readonly Material Velvet;
        public readonly Material LampEmissive;
        public readonly Material RedEmergencyEmissive;

        public SkyscraperArt()
        {
            EnsureFolders();

            Marble = CreateMaterial("Marble_Carrara", new Color(0.85f, 0.85f, 0.85f), "Marble012", 0.65f, 0.05f);
            LiftSteel = CreateMaterial("Lift_BrushedChampagne", new Color(.64f,.59f,.48f), null, .65f, .78f);
            LobbyWalnut = CreateMaterial("Lobby_Walnut", new Color(.42f,.31f,.23f), "WoodFloor051", .45f, 0);
            Parquet = CreateMaterial("Parquet_Luxury", new Color(0.45f, 0.32f, 0.22f), "WoodFloor051", 0.45f, 0.0f);
            Mahogany = CreateMaterial("Mahogany_Rich", new Color(0.28f, 0.12f, 0.08f), "WoodFloor051", 0.55f, 0.0f);
            WallPlaster = CreateMaterial("Wall_Plaster", new Color(0.72f, 0.70f, 0.66f), "Plaster001", 0.15f, 0.0f);
            Concrete = CreateMaterial("Concrete_Core", new Color(0.55f, 0.54f, 0.52f), "Concrete031", 0.18f, 0.0f);
            Steel = CreateMaterial("Steel_Industrial", new Color(0.48f, 0.50f, 0.52f), "Metal049A", 0.55f, 0.85f);
            DiamondPlate = CreateMaterial("Steel_DiamondPlate", new Color(0.40f, 0.42f, 0.44f), "DiamondPlate008A", 0.40f, 0.80f);
            KitchenTile = CreateMaterial("Kitchen_Tiles", new Color(0.80f, 0.82f, 0.80f), "Tiles107", 0.50f, 0.0f);
            ColdStoreWall = CreateMaterial("ColdStore_Panels", new Color(0.68f, 0.74f, 0.78f), "PaintedMetal012", 0.35f, 0.30f);
            Asphalt = CreateMaterial("Street_Asphalt", new Color(0.24f, 0.24f, 0.25f), "Asphalt031", 0.12f, 0.0f);
            PlazaPaver = CreateMaterial("Plaza_Paving", new Color(0.68f, 0.66f, 0.62f), "PavingStones126A", 0.25f, 0.0f);
            GraniteDark = CreateMaterial("Granite_DarkColonnade", new Color(0.18f, 0.18f, 0.20f), "Concrete034", 0.60f, 0.05f);
            RoadYellow = CreateMaterial("Road_Marking_Yellow", new Color(0.92f, 0.75f, 0.12f), null, 0.25f, 0.0f);
            RoadWhite = CreateMaterial("Road_Marking_White", new Color(0.88f, 0.88f, 0.86f), null, 0.25f, 0.0f);
            CarPaintBlack = CreateMaterial("Car_Paint_Black", new Color(0.08f, 0.08f, 0.09f), null, 0.88f, 0.20f);
            CarPaintYellow = CreateMaterial("Car_Paint_Yellow", new Color(0.85f, 0.65f, 0.08f), null, 0.85f, 0.10f);
            CarPaintBurgundy = CreateMaterial("Car_Paint_Burgundy", new Color(0.35f, 0.08f, 0.12f), null, 0.85f, 0.20f);
            Rubber = CreateMaterial("Tire_Rubber", new Color(0.12f, 0.12f, 0.12f), "Rubber004", 0.10f, 0.0f);
            Rust = CreateMaterial("Rusted_Metal", new Color(0.48f, 0.28f, 0.18f), "Metal016", 0.15f, 0.25f);
            WaterMurky = CreateGlassMaterial("Water_Murky", new Color(0.15f, 0.22f, 0.20f, 0.85f), 0.95f);
            CeilingTile = CreateMaterial("Ceiling_Office", new Color(0.65f, 0.65f, 0.64f), "OfficeCeiling001", 0.10f, 0.0f);
            Leather = CreateMaterial("Leather_Lounge", new Color(0.28f, 0.18f, 0.14f), "Leather037", 0.30f, 0.0f);
            Brass = CreateMaterial("Brass_Trim", new Color(0.82f, 0.68f, 0.32f), null, 0.75f, 0.90f);
            Linen = CreateMaterial("Apex_IvoryLinen", new Color(.91f,.86f,.73f), "Fabric030", .18f, 0);
            Porcelain = CreateMaterial("Apex_BoneChina", new Color(.97f,.94f,.87f), null, .82f, 0);
            Velvet = CreateMaterial("Apex_EmeraldUpholstery", new Color(.055f,.19f,.15f), "Leather037", .32f, 0);

            TowerCladding=CreateMaterial("Tower_Cladding",new Color(.18f,.24f,.29f),null,.65f,.25f);

            // A small sky-coloured reflection floor keeps sealed office glazing legible at night.
            TowerCladding.EnableKeyword("_EMISSION");
            TowerCladding.SetColor("_EmissionColor",new Color(.015f,.03f,.05f));
            EditorUtility.SetDirty(TowerCladding);

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
            bool create = mat == null;
            if (create) mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            mat.SetColor("_BaseColor", new Color(tint.r, tint.g, tint.b, .16f));
            mat.SetFloat("_Smoothness", smoothness);
            mat.SetFloat("_Metallic", 0f);
            mat.SetFloat("_Surface",1f);
            mat.SetFloat("_Blend",0f);
            mat.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite",0f);
            mat.SetFloat("_Cull",0f);
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.SetOverrideTag("RenderType","Transparent");
            mat.renderQueue=3000;
            if (create) AssetDatabase.CreateAsset(mat,path);
            EditorUtility.SetDirty(mat);
            return mat;
        }

        private static Material CreateEmissiveMaterial(string name, Color albedo, Color emission)
        {
            string path = $"{MaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emission);
                GeneratedMaterialRepair.Apply(mat);
                EditorUtility.SetDirty(mat);
                return mat;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            mat = new Material(shader) { name = name };
            mat.SetColor("_BaseColor", albedo);
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", emission);
            GeneratedMaterialRepair.Apply(mat);
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }
    }
}
